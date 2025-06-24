using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using telerik.Data;
using telerik.Models;

namespace telerik.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Stock()
        {

            ViewBag.CategoryList = _context.Categories
                .Select(c => new
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName
                })
                .ToList();
            return View();
        }
        public JsonResult Grid_Read([DataSourceRequest] DataSourceRequest request, string searchTerm)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.ProductName.Contains(searchTerm));
            }

            var result = query.Select(p => new
            {
                p.ProductID,
                p.ProductName,
                p.UnitPrice,
                p.UnitsInStock,
                p.Discontinued,
                p.UnitsOnOrder,
                p.CategoryID,
                CategoryName = p.Category.CategoryName
            });

            return Json(result.ToDataSourceResult(request));
        }

        [HttpPost]
        public JsonResult Grid_Create([DataSourceRequest] DataSourceRequest request, Product productVM)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductName = productVM.ProductName,
                    UnitPrice = productVM.UnitPrice,
                    UnitsInStock = productVM.UnitsInStock,
                    Discontinued = productVM.Discontinued,
                    UnitsOnOrder = productVM.UnitsOnOrder,
                    CategoryID = productVM.CategoryID  // ✅ this is essential
                };

                _context.Products.Add(product);
                _context.SaveChanges();

             
            }

            return Json(new[] { productVM }.ToDataSourceResult(request, ModelState));
        }


        [HttpPost]
        public JsonResult Grid_Update([DataSourceRequest] DataSourceRequest request, Product productVM)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductID == productVM.ProductID);
                if (product != null)
                {
                    product.ProductName = productVM.ProductName;
                    product.UnitPrice = productVM.UnitPrice;
                    product.UnitsInStock = productVM.UnitsInStock;
                    product.Discontinued = productVM.Discontinued;
                    product.UnitsOnOrder = productVM.UnitsOnOrder;
                    product.CategoryID = productVM.CategoryID; 

                    _context.Products.Update(product);
                    _context.SaveChanges();

                    // Optional: update returned CategoryName
                    productVM.CategoryName = _context.Categories
                        .FirstOrDefault(c => c.CategoryID == product.CategoryID)?.CategoryName;
                }
            }

            return Json(new[] { productVM }.ToDataSourceResult(request, ModelState));
        }


        [HttpPost]
        public JsonResult Grid_Destroy([DataSourceRequest] DataSourceRequest request, Product productVM)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.Find(productVM.ProductID);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                }
            }

            return Json(new[] { productVM }.ToDataSourceResult(request, ModelState));
        }

        // New action to get categories for dropdown
        public JsonResult GetCategories()
        {
            var categories = _context.Categories
                .Select(c => new { c.CategoryID, c.CategoryName })
                .ToList();

            return Json(categories);
        }
    }
}
