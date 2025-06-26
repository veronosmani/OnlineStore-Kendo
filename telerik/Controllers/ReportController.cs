using Microsoft.AspNetCore.Mvc;
using System.Linq;
using telerik.Data;
using telerik.Models;

namespace telerik.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ProductSalesSummary()
        {
            try
            {
                // Fetch all products from DB
                var products = _context.Products.ToList();

                // Group sales by ProductId and sum quantities sold
                var sales = _context.Sales
                    .GroupBy(s => s.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        UnitsSold = g.Sum(x => x.Quantity)
                    })
                    .ToList();

                // Project final result with sales and product info
                var data = products.Select(product =>
                {
                    var sale = sales.FirstOrDefault(s => s.ProductId == product.ProductID);
                    int unitsSold = sale?.UnitsSold ?? 0;

                    return new ProductSalesViewModel
                    {
                        ProductName = product.ProductName,
                        UnitsInStock = product.UnitsInStock,
                        UnitsSold = unitsSold,
                        StockLeft = product.UnitsInStock - unitsSold,
                        Revenue = unitsSold * product.UnitPrice
                    };
                }).ToList();

                // Calculate total revenue
                ViewBag.TotalRevenue = data.Sum(d => d.Revenue);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                ViewBag.TotalRevenue = 0;
                return View(new List<ProductSalesViewModel>());
            }
        }



    }
}
