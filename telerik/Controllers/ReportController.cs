using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using telerik.Data;
using telerik.Models;
using telerik.Models.ViewModels;

namespace telerik.Controllers
{
    [Route("Report")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [Route("ProductSalesSummary")]
        public IActionResult ProductSalesSummary()
        {
            // 1. Stock Summary
            var productSalesSummary = _context.Products
                .Select(p => new ProductSalesViewModel
                {
                    ProductName = p.ProductName,
                    UnitsInStock = p.UnitsInStock,
                    UnitsSold = _context.OrderDetails
                        .Where(od => od.ProductId == p.ProductID)
                        .Sum(od => od.Quantity),
                    StockLeft = p.UnitsInStock - _context.OrderDetails
                        .Where(od => od.ProductId == p.ProductID)
                        .Sum(od => od.Quantity),
                    Revenue = _context.OrderDetails
                        .Where(od => od.ProductId == p.ProductID)
                        .Sum(od => od.Quantity * od.UnitPrice)
                })
                .ToList();

            ViewBag.TotalRevenue = productSalesSummary.Sum(x => x.Revenue);

            // 2. Orders with Products + Category Info
            var orderProducts = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category)
                            .ThenInclude(c => c.ParentCategory)
                .SelectMany(o => o.OrderDetails, (o, od) => new OrderProductViewModel
                {
                    CustomerName = o.CustomerName,
                    OrderDate = o.OrderDate,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    TotalPrice = od.Quantity * od.UnitPrice,
                    CategoryName = od.Product.Category.CategoryName,
                    ParentCategoryName = od.Product.Category.ParentCategory != null
                        ? od.Product.Category.ParentCategory.CategoryName
                        : null
                })
                .ToList();

            // 3. Category Tree Grouping
            var categories = _context.Categories
                .Include(c => c.ParentCategory)
                .ToList();

            var grouped = categories
                .Where(c => c.ParentCategoryID == null)
                .Select(parent => new CategoryOrdersViewModel
                {
                    ParentCategoryName = parent.CategoryName,
                    DirectProducts = orderProducts
                        .Where(op => op.CategoryName == parent.CategoryName)
                        .ToList(),
                    SubCategories = categories
                        .Where(child => child.ParentCategoryID == parent.CategoryID)
                        .Select(child => new SubCategoryGroup
                        {
                            CategoryName = child.CategoryName,
                            Products = orderProducts
                                .Where(op => op.CategoryName == child.CategoryName)
                                .ToList()
                        })
                        .ToList()
                })
                .Where(g => g.DirectProducts.Any() || g.SubCategories.Any(sc => sc.Products.Any()))
                .ToList();


            // 4. Flatten for TreeList
            var treeListItems = new List<object>();
            int nodeId = 1;

            foreach (var parent in grouped)
            {
                var parentId = nodeId++;
                treeListItems.Add(new
                {
                    Id = parentId,
                    ParentId = (int?)null,
                    Name = parent.ParentCategoryName,
                    Type = "Parent"
                });

                // Add subcategories
                foreach (var sub in parent.SubCategories)
                {
                    var subId = nodeId++;
                    treeListItems.Add(new
                    {
                        Id = subId,
                        ParentId = parentId,
                        Name = sub.CategoryName,
                        Type = "SubCategory"
                    });

                    foreach (var p in sub.Products)
                    {
                        treeListItems.Add(new
                        {
                            Id = nodeId++,
                            ParentId = subId,
                            Name = p.ProductName,
                            Category = p.CategoryName,
                            Quantity = p.Quantity,
                            OrderDate = p.OrderDate,
                            TotalPrice = p.TotalPrice,
                            Type = "Product"
                        });
                    }
                }

                // Add direct products
                foreach (var p in parent.DirectProducts)
                {
                    treeListItems.Add(new
                    {
                        Id = nodeId++,
                        ParentId = parentId,
                        Name = p.ProductName,
                        Category = p.CategoryName,
                        Quantity = p.Quantity,
                        OrderDate = p.OrderDate,
                        TotalPrice = p.TotalPrice,
                        Type = "Product"
                    });
                }
            }

            ViewBag.TreeListData = treeListItems;

            return View(productSalesSummary);
        }
    }
}
