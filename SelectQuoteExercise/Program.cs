using System.Data;

class Program
{
    static void Main()
    {
        DataTable revenueTable = new DataTable();
        revenueTable.Columns.Add("Period", typeof(int));
        revenueTable.Columns.Add("PolicyNumber", typeof(string));
        revenueTable.Columns.Add("Revenue", typeof(decimal));

        revenueTable.Rows.Add(1, "ABC123", 1200m);
        revenueTable.Rows.Add(3, "XYZ456", 2400m);
        revenueTable.Rows.Add(7, "DEF965", 1000m);

        DataTable paymentTable = new DataTable();
        paymentTable.Columns.Add("Period", typeof(int));
        paymentTable.Columns.Add("PolicyNumber", typeof(string));
        paymentTable.Columns.Add("Payment", typeof(decimal));

        paymentTable.Rows.Add(2, "ABC123", 100m);
        paymentTable.Rows.Add(3, "ABC123", 100m);
        paymentTable.Rows.Add(4, "ABC123", 100m);
        paymentTable.Rows.Add(4, "XYZ456", 200m);
        paymentTable.Rows.Add(5, "ABC123", 100m);
        paymentTable.Rows.Add(5, "XYZ456", 200m);
        paymentTable.Rows.Add(6, "ABC123", 100m);
        paymentTable.Rows.Add(6, "XYZ456", 200m);
        paymentTable.Rows.Add(6, "DEF000", 300m);

        DataTable receivableTable = new DataTable();
        receivableTable.Columns.Add("Period", typeof(int));
        receivableTable.Columns.Add("PolicyNumber", typeof(string));
        receivableTable.Columns.Add("Revenue", typeof(decimal));
        receivableTable.Columns.Add("Payment", typeof(decimal));
        receivableTable.Columns.Add("ARBalance", typeof(decimal));

        var combinedRows = revenueTable.AsEnumerable()
            .Select(r => new
            {
                Period = r.Field<int>("Period"),
                PolicyNumber = r.Field<string>("PolicyNumber"),
                Revenue = r.Field<decimal>("Revenue"),
                Payment = 0m,
                IsRevenue = true
            })
            .Concat(paymentTable.AsEnumerable()
            .Select(p => new
            {
                Period = p.Field<int>("Period"),
                PolicyNumber = p.Field<string>("PolicyNumber"),
                Revenue = 0m,
                Payment = p.Field<decimal>("Payment"),
                IsRevenue = false
            }))
            .OrderBy(x => x.PolicyNumber)
            .ThenBy(x => x.Period)
            .ThenByDescending(x => x.IsRevenue);

        Dictionary<string, decimal> arBalances = new Dictionary<string, decimal>();

        foreach (var row in combinedRows)
        {
            arBalances.TryGetValue(row.PolicyNumber, out decimal previousBalance);
            decimal currentBalance = previousBalance + row.Revenue - row.Payment;

            receivableTable.Rows.Add(row.Period, row.PolicyNumber, row.Revenue, row.Payment, currentBalance);

            arBalances[row.PolicyNumber] = currentBalance;
        }

        Console.WriteLine($"{"Period",6} {"Policy",10} {"Revenue",8} {"Payment",8} {"AR Balance",10}");
        foreach (DataRow row in receivableTable.Rows)
        {
            Console.WriteLine($"{row["Period"],6} {row["PolicyNumber"],10} {row["Revenue"],8} {row["Payment"],8} {row["ARBalance"],10}");
        }
    }
}


