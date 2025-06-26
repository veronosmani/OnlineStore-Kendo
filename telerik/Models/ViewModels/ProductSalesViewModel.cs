namespace telerik.Models
{
    public class ProductSalesViewModel
    {
        public string ProductName { get; set; }
        public int UnitsInStock { get; set; }
        public int UnitsSold { get; set; }
        public int StockLeft { get; set; }      // ✅ new

        public decimal Revenue { get; set; }
    }
}
