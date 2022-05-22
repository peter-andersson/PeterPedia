using System.Globalization;
using HtmlAgilityPack;

namespace PeterPedia.Services;

public class LänsförsäkringarKort
{
    public List<Transaction> Parse(Stream stream)
    {
        var transactions = new List<Transaction>();

        var doc = new HtmlDocument();
        doc.Load(stream);

        HtmlNode? node = doc.DocumentNode.SelectSingleNode("//table[@id='viewVisaForm:cardTransactionData']");

        if (node is not null)
        {
            HtmlNodeCollection? rows = node.SelectNodes("//tbody/tr");

            foreach (HtmlNode row in rows)
            {
                HtmlNodeCollection? columns = row.SelectNodes("td");
                if (columns is not null)
                {
                    var date = columns[0].ChildNodes[0].InnerText;
                    var note1 = columns[1].ChildNodes[0].InnerText;
                    var note2 = columns[2].ChildNodes[0].InnerText;
                    var amount = columns[3].ChildNodes[0].InnerText;

                    Transaction? transaction = CreateTransaction(date, note1, note2, amount);

                    if (transaction is not null)
                    {
                        transactions.Add(transaction);
                    }
                }                    
            }
        }

        return transactions;
    }

    public Transaction? CreateTransaction(string dateString, string note1, string note2, string amount)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(note1))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(note2))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(amount))
        {
            return null;
        }

        // Ignore preliminary transactions
        if (note1.ToLowerInvariant() == "prel.köp")
        {
            return null;
        }

        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return null;
        }

        if (!decimal.TryParse(amount.Replace(",", ".").Replace(" ", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var tmpValue))
        {
            return null;
        }

        var transaction = new Transaction()
        {
            Date = date,
            Note1 = note1,
            Note2 = note2,
            Amount = (double)tmpValue
        };

        if ((note1.ToLowerInvariant() == "kortköp") ||
            (note1.ToLowerInvariant() == "kontantuttag"))
        {
            transaction.Amount *= -1;
        }

        return transaction;
    }
}
