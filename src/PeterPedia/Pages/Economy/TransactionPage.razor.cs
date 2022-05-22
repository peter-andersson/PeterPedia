using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Economy;

public partial class TransactionPage : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;

    public Category Category { get; set; } = new Category();

    public bool IsTaskRunning { get; set; } = false;

    public List<Category> Categories { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        List<CategoryEF> categories = await DbContext.Categories.Include(c => c.Parent).OrderBy(c => c.Parent != null).ThenBy(c => c.Parent).ToListAsync();

        Categories.Clear();

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
    }

    public void Edit(Category category) => Category = category;

    public async Task SaveAsync()
    {
        IsTaskRunning = true;

        CategoryEF? categoryEF = await DbContext.Categories.Where(c => c.Id == Category.Id).AsTracking().SingleOrDefaultAsync();

        if (categoryEF is null)
        {
            categoryEF = new CategoryEF();
        }
        
        categoryEF.Name = Category.Name;

        if (Category.ParentId > 0)
        {
            CategoryEF? parentEF = await DbContext.Categories.Where(c => c.Id == Category.ParentId).AsTracking().SingleOrDefaultAsync();
            if (parentEF != null)
            {
                categoryEF.Parent = parentEF;
            }
        }        

        if (categoryEF.Id > 0)
        {
            DbContext.Categories.Update(categoryEF);
        }
        else
        {
            DbContext.Categories.Add(categoryEF);
            Categories.Add(Category);            
        }

        await DbContext.SaveChangesAsync();

        Category.Id = categoryEF.Id;
        Category? parent = Categories.Where(c => c.Id == Category.ParentId).FirstOrDefault();
        Category.Parent = parent;
        
        Category = new Category();

        Categories = Categories.OrderBy(c => c.Display).ToList();

        IsTaskRunning = false;
    }

    public async Task DeleteAsync()
    {
        if (Category.Id == 0)
        {
            return;
        }

        IsTaskRunning = true;

        CategoryEF? categoryEF = await DbContext.Categories.Where(l => l.Id == Category.Id).AsTracking().SingleOrDefaultAsync();

        if (categoryEF is null)
        {
            return;
        }

        DbContext.Categories.Remove(categoryEF);
        await DbContext.SaveChangesAsync();

        Categories.Remove(Category);

        Category = new Category();

        IsTaskRunning = false;
    }
}
