using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Economy;

public partial class TransactionView : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    [Parameter]
    public List<Category> Categories { get; set; } = null!;

    [Parameter]
    public Transaction Transaction { get; set; } = null!;

    public bool IsTaskRunning { get; set; } = false;

    protected override void OnInitialized()
    {
        if (Transaction.Category is null)
        {
            Transaction.Category = new Category()
            {
                Id = 0,
            };
        }
    }

    private async Task OnCategoryChangedAsync(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var id))
        {
            return;
        }

        TransactionEF? dbTransaction = await DbContext.Transactions.Where(t => t.Id == Transaction.Id).AsTracking().SingleOrDefaultAsync();

        if (dbTransaction is null)
        {
            return;
        }

        if (id > 0)
        {
            CategoryEF? dbCategory = await DbContext.Categories.Where(c => c.Id == id).AsTracking().SingleOrDefaultAsync();

            dbTransaction.Category = dbCategory;
        }
        else
        {
            dbTransaction.Category = null;
        }

        DbContext.Update(dbTransaction);
        await DbContext.SaveChangesAsync();
    }
}
