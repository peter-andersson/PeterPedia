using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Economy;

public partial class TransactionPage : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    public List<Category> Categories { get; set; } = new();

    public List<Transaction> Transactions { get; set; } = new();

    public TransactionSearch Search { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        List<CategoryEF> categories = await DbContext.Categories.Include(c => c.Parent).OrderBy(c => c.Parent != null).ThenBy(c => c.Parent).ToListAsync();

        foreach (CategoryEF categoryEF in categories)
        {
            var category = new Category()
            {
                Id = categoryEF.Id,
                Name = categoryEF.Name,
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

        await FetchTransactionsAsync();
    }

    public async Task FetchTransactionsAsync()
    {
        Transactions.Clear();

        IQueryable<TransactionEF> query = DbContext.Transactions.Include(t => t.Category);

        if (Search.StartDate is null && Search.EndDate is null)
        {
            query = query.Where(t => t.Category == null);
        }
        else if (Search.StartDate is not null && Search.EndDate is not null)
        {
            query = query.Where(t => t.Date >= Search.StartDate.Value && t.Date <= Search.EndDate.Value);
        }
        else if (Search.StartDate is not null)
        {
            query = query.Where(t => t.Date >= Search.StartDate.Value);
        }
        else if (Search.EndDate is not null)
        {
            query = query.Where(t => t.Date <= Search.EndDate.Value);
        }

        List<TransactionEF> transactions = await query.OrderBy(t => t.Date).ToListAsync();        
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

                transaction.Category = category ?? new Category() { Id = 0 };                    
            }

            Transactions.Add(transaction);
        }
    }
}
