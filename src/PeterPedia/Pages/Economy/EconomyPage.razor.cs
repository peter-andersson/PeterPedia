using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Economy;

public partial class EconomyPage : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    public TransactionSearch Search { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public List<CategorySummary> CurrentPeriod { get; set; } = new();

    public List<CashFlow> CashFlow { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Search.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        Search.EndDate = Search.StartDate.Value.AddMonths(1).AddDays(-1);

        List<CategoryEF> categories = await DbContext.Categories.Include(c => c.Parent).OrderBy(c => c.Parent != null).ThenBy(c => c.Parent).ToListAsync();

        foreach (CategoryEF categoryEF in categories)
        {
            var category = new Category()
            {
                Id = categoryEF.Id,
                Name = categoryEF.Name,
                IgnoreInOverview = categoryEF.IgnoreInOverView
            };

            if (categoryEF.Parent is not null)
            {
                Category? parent = Categories.Where(c => c.Id == categoryEF.Parent.Id).FirstOrDefault();
                if (parent != null)
                {
                    category.Parent = parent;
                }
            }

            Categories.Add(category);
        }

        Categories = Categories.OrderBy(c => c.Display).ToList();

        await FetchDataAsync();        
    }

    private async Task FetchDataAsync()
    {
        if (Search.StartDate is null || Search.EndDate is null)
        {
            return;
        }

        CurrentPeriod.Clear();

        List<TransactionEF> dbTransactions = await DbContext.Transactions.Include(t => t.Category).Where(t => t.Date >= Search.StartDate.Value && t.Date <= Search.EndDate.Value).ToListAsync();

        var transactions = new List<Transaction>(dbTransactions.Count);
        foreach (TransactionEF transactionEF in dbTransactions)
        {
            var transaction = new Transaction()
            {
                Date = transactionEF.Date,
                Amount = transactionEF.Amount,
                Note1 = transactionEF.Note1,
                Note2 = transactionEF.Note2,
                Id = transactionEF.Id,
            };

            if (transactionEF.Category is not null)
            {
                Category? category = Categories.Where(c => c.Id == transactionEF.Category.Id).SingleOrDefault();

                transaction.Category = category ?? new Category() { Id = 0, Name = "Uncategorized" };
            }
            else
            {
                transaction.Category = new Category() { Id = 0, Name = "Uncategorized" };
            }

            if (transaction.Category.IgnoreInOverview)
            {
                continue;
            }

            transactions.Add(transaction);

            
        }

        foreach (Category category in Categories)
        {
            if (category.IgnoreInOverview)
            {
                continue;
            }

            CategorySummary? summary = null;
            if (category.Parent is null)
            {
                summary = CurrentPeriod.Where(c => c.Name == category.Display).SingleOrDefault();

                if (summary is null)
                {
                    summary = new CategorySummary()
                    {
                        Name = category.Display,
                        TotalAmount = 0,
                    };

                    CurrentPeriod.Add(summary);
                }
            }
            else
            {
                summary = GetParentSummary(category);

                if (summary is not null)
                {
                    var child = new CategorySummary
                    {
                        Name = category.Display,
                        TotalAmount = 0,
                        Parent = summary
                    };
                    summary.Children.Add(child);
                }
            }

            if (summary is not null)
            {
                AddTransactions(category, summary, transactions);
            }            
        }
       
        CurrentPeriod = CurrentPeriod.OrderBy(c => c.Name).ToList();

        foreach(CategorySummary summary in CurrentPeriod)
        {
            OrderChildren(summary);
        }

        await FetchCashFlowAsync();
    }

    private CategorySummary? GetParentSummary(Category category)
    {
        if (category.Parent is null)
        {
            return null;
        }

        Category parent = category.Parent;
        while (true)
        {
            if (parent.Parent is null)
            {
                break;
            }
            else
            {
                parent = parent.Parent;
            }
        }

        CategorySummary? summary = CurrentPeriod.Where(c => c.Name == parent.Display).SingleOrDefault();
        if (summary is null)
        {
            return null;
        }

        if (summary.Name == category.Parent.Display)
        {
            return summary;
        }

        // Find child
        return GetChild(category, summary.Children);
    }

    private CategorySummary? GetChild(Category category, List<CategorySummary> children)
    {
        foreach (var child in children)
        {
            if (child.Name == category.Parent?.Display)
            {
                return child;
            }
        }

        foreach (var child in children)
        {
            CategorySummary? summary = GetChild(category, child.Children);

            if (summary is not null)
            {
                return summary;
            }
        }

        return null;
    }

    private void AddTransactions(Category category, CategorySummary summary, List<Transaction> transactions)
    {
        var i = 0;
        while (i < transactions.Count)
        {
            Transaction transaction = transactions[i];

            if (transaction.Category.Id == category.Id)
            {
                if (summary.Name == transaction.Category.Display)
                {
                    summary.TotalAmount += transaction.Amount;

                    CategorySummary? parent = summary.Parent;
                    while (parent is not null)
                    {
                        parent.TotalAmount += transaction.Amount;

                        parent = parent.Parent;
                    }
                }
                else
                {
                    AddToChild(transaction, summary.Children);
                }
                

                transactions.Remove(transaction);
                continue;
            }

            i += 1;
        }
    }

    private void AddToChild(Transaction transaction, List<CategorySummary> children)
    {
        foreach (var child in children)
        {
            if (child.Name == transaction.Category.Display)
            {
                child.TotalAmount += transaction.Amount;

                CategorySummary? parent = child.Parent;
                while (parent is not null)
                {
                    parent.TotalAmount += transaction.Amount;

                    parent = parent.Parent;
                }

                return;
            }
        }

        foreach (var child in children)
        {
            AddToChild(transaction, child.Children);
        }
    }

    private async Task FetchCashFlowAsync()
    {
        if (Search.StartDate is null)
        {
            return;
        }

        var summary = await DbContext.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date.Year >= Search.StartDate.Value.Year && t.Category != null && !t.Category.IgnoreInOverView)
            .GroupBy(t => t.Date.Month)
            .Select(g => new
            {
                g.Key,
                Amount = g.Sum(t => t.Amount),
            })
            .OrderBy(g => g.Key)
            .ToListAsync();

        CashFlow.Clear();
        foreach (var x in summary)
        {
            var cashFlow = new CashFlow()
            {
                Month = GetMonthName(x.Key),
                Amount = x.Amount
            };

            CashFlow.Add(cashFlow);
        }
    }

    private static string GetMonthName(int month)
    {
        var date = new DateTime(2020, month, 1);

        return date.ToString("MMMM");
    }

    private void OrderChildren(CategorySummary summary)
    {
        if (summary.Children.Count == 0)
        {
            return;
        }

        summary.Children = summary.Children.OrderBy(c => c.Name).ToList();

        foreach (CategorySummary child in summary.Children)
        {
            OrderChildren(child);
        }
    }
}
