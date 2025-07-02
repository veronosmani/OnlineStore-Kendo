using telerik.Models.ViewModels;

public class CategoryOrdersViewModel
{
    public string? ParentCategoryName { get; set; } 

    public List<OrderProductViewModel> DirectProducts { get; set; } = new();

    public List<SubCategoryGroup> SubCategories { get; set; } = new();
}
