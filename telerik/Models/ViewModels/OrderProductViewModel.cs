public class OrderProductViewModel
{
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string CategoryName { get; set; }      
    public string ParentCategoryName { get; set; }
    public int CategoryID { get; set; }
    public int? ParentCategoryID { get; set; }

}
