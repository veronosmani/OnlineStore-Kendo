using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using telerik.Data;
using telerik.Models;

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

            var productSalesSummary = _context.Products
                .Select(p => new ProductSalesViewModel
                {
                    ProductName = p.ProductName,
                    UnitsInStock = p.UnitsInStock,
                    UnitsSold = _context.OrderDetails.Where(od => od.ProductId == p.ProductID).Sum(od => od.Quantity),
                    StockLeft = p.UnitsInStock - _context.OrderDetails.Where(od => od.ProductId == p.ProductID).Sum(od => od.Quantity),
                    Revenue = _context.OrderDetails.Where(od => od.ProductId == p.ProductID).Sum(od => od.Quantity * od.UnitPrice)
                })
                .ToList();

            ViewBag.TotalRevenue = productSalesSummary.Sum(x => x.Revenue);

            // Get Orders with Product Details
            var orderProducts = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .SelectMany(o => o.OrderDetails, (o, od) => new OrderProductViewModel
                {
                    CustomerName = o.CustomerName,
                    OrderDate = o.OrderDate,
                    ProductName = od.Product.ProductName,
                    Quantity = od.Quantity,
                    TotalPrice = od.Quantity * od.UnitPrice
                })
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            ViewBag.OrderProductList = orderProducts;

            // Pass ProductSalesSummary as model, orderProducts in ViewBag
            return View(productSalesSummary);
        }
    }
}
