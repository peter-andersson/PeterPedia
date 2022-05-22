using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace PeterPedia.Pages.Economy;

public partial class ImportPage : ComponentBase
{
    [Inject]
    private PeterPediaContext DbContext { get; set; } = null!;
    
    public bool IsTaskRunning { get; set; } = false;

    public string AccountType { get; set; } = string.Empty;

    public List<string> AccountTypes { get; set; } = new();

    private IReadOnlyList<IBrowserFile> _selectedFiles;

    protected override void OnInitialized()
    {
        AccountType = string.Empty;
        AccountTypes.Clear();
        AccountTypes.Add("Länsförsäkringar Konto");
        AccountTypes.Add("Länsförsäkringar Kort");        
    }

    public void LoadFiles(InputFileChangeEventArgs e)
    {
        _selectedFiles = e.GetMultipleFiles();        
    }

    public async Task UploadAsync()
    {
        var maxAllowSize = 1024 * 1024 * 5;

        var transactions = new List<Transaction>();
        foreach (IBrowserFile file in _selectedFiles)
        {
            var memoryStream = new MemoryStream();
            await file.OpenReadStream(maxAllowSize).CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (AccountType == "Länsförsäkringar Konto")
            {                
                var lines = new List<string>();
                using var reader = new StreamReader(memoryStream);
                while (reader.Peek() >= 0)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                    }
                }

                var konto = new LänsförsäkringarKonto();
                transactions.AddRange(konto.Parse(lines));
            }
            else if (AccountType == "Länsförsäkringar Kort")
            {
                var kort = new LänsförsäkringarKort();
                transactions.AddRange(kort.Parse(memoryStream));
            }
        }

        foreach (Transaction transaction in transactions)
        {
            TransactionEF? transactionEF = await DbContext.Transactions.Where(t => t.Note1 == transaction.Note1 && t.Note2 == transaction.Note2 && t.Date == transaction.Date && t.Amount == transaction.Amount).FirstOrDefaultAsync();

            if (transactionEF is null)
            {
                transactionEF = new TransactionEF()
                {
                    Note1 = transaction.Note1,
                    Note2 = transaction.Note2,
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                };

                DbContext.Transactions.Add(transactionEF);
            }
        }

        // TODO: Log existing transactions?

        await DbContext.SaveChangesAsync();
    }
}
