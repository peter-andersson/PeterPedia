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

        await FetchDataAsync();        
    }

    private async Task FetchDataAsync()
    {
        if (Search.StartDate is null || Search.EndDate is null)
        {
            return;
        }

        CurrentPeriod.Clear();

        List<TransactionEF> transactions = await DbContext.Transactions.Include(t => t.Category).Where(t => t.Date >= Search.StartDate.Value && t.Date <= Search.EndDate.Value).ToListAsync();

        foreach (TransactionEF transactionEF in transactions)
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

            CategorySummary? summary = CurrentPeriod.Where(c => c.Name == transaction.Category.Display).SingleOrDefault();
            if (summary is null)
            {
                summary = new CategorySummary()
                {
                    Name = transaction.Category.Display,
                    TotalAmount = transaction.Amount,
                };

                CurrentPeriod.Add(summary);
            }
            else
            {
                summary.TotalAmount += transaction.Amount;
            }
        }

        var i = 0;
        while (i < CurrentPeriod.Count)
        {
            CategorySummary summary = CurrentPeriod[i];

            var charPosition = summary.Name.LastIndexOf(':');
            if (charPosition > 0)
            {
                var parentCategory = summary.Name[..charPosition];

                CategorySummary? category = CurrentPeriod.Where(c => c.Name == parentCategory).SingleOrDefault();

                if (category is not null)
                {
                    category.Children.Add(summary);
                    category.TotalAmount += summary.TotalAmount;

                    CurrentPeriod.Remove(summary);

                    continue;
                }
                else
                {
                    category = new CategorySummary()
                    {
                        Name = parentCategory,
                        TotalAmount = summary.TotalAmount,
                    };

                    category.Children.Add(summary);
                    CurrentPeriod.Add(category);

                    CurrentPeriod.Remove(summary);
                }
            }            

            i += 1;
        }

        CurrentPeriod = CurrentPeriod.OrderBy(c => c.Name).ToList();

        foreach(CategorySummary summary in CurrentPeriod)
        {
            OrderChildren(summary);
        }

        await FetchCashFlowAsync();
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
