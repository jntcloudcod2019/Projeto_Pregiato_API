namespace Pregiato.API.Response
{
    public class BillingResponse
    {

        public bool Success { get; set; }
        public string Message { get; set; }
        public BillingData Data { get; set; }
    }

    public class BillingData
    {
        public decimal TotalSales { get; set; }
        public string Currency { get; set; }
        public Period Period { get; set; }
        public decimal TransactionsCount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class Period
    {
        public string  StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
