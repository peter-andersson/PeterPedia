using System.Globalization;

namespace PeterPedia.Services;

public class LänsförsäkringarKonto
{
    public List<Transaction> Parse(List<string> lines)
    {
        var transactions = new List<Transaction>();

        foreach (var line in lines)
        {
            Transaction? transaktion = ParseLine(line);

            if (transaktion is not null)
            {
                transactions.Add(transaktion);
            }
        }

        return transactions;
    }

    public static Transaction? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        // Remove header rows
        if (line.StartsWith("\"K") || line.StartsWith("\"9") || line.StartsWith("\"B"))
        {
            return null;
        }

        var data = line.Split(new char[] { ';' });

        if (data.Length < 5)
        {
            return null;
        }

        // Fix format
        var valueString = data[4].Replace("\"", string.Empty).Replace(",", ".").Replace(" ", string.Empty);

        if (!decimal.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out var tmpValue))
        {
            return null;

        }

        var transaction = new Transaction
        {
            Amount = (double)tmpValue,
            Note1 = data[2].Replace("\"", string.Empty),
            Note2 = data[3].Replace("\"", string.Empty),
        };

        if (!DateTime.TryParseExact(data[0].Replace("\"", string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tmpDate))
        {
            return null;
        }

        transaction.Date = tmpDate;
       
        return transaction;
    }
}
