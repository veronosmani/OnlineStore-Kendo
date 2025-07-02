namespace telerik.Models.ViewModels
{
    public class SubCategoryGroup
    {
        public string CategoryName { get; set; } // e.g., Phone
        public List<OrderProductViewModel> Products { get; set; } = new();
    }
}
