using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using telerik.Data;
using Microsoft.AspNetCore.Authorization;
using telerik.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace telerik.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Shop(int page = 1, List<int> categoryIds = null)
        {
            int pageSize = 8;

            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => categoryIds.Contains(p.CategoryID));
            }

            var totalProducts = query.Count();
            var pagedProducts = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryList = _context.Categories.ToList();
            ViewBag.SelectedCategories = categoryIds;

            return View(pagedProducts);
        }


        public IActionResult Order()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
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
                CategoryName = p.Category.CategoryName,
                p.ImageUrl
            });

            return Json(result.ToDataSourceResult(request));
        }

        [HttpPost]
        public async Task<JsonResult> Grid_CreateOrUpdate([DataSourceRequest] DataSourceRequest request, Product productVM)
        {
            if (ModelState.IsValid)
            {
                Product product;

                if (productVM.ProductID == 0)
                {
                    product = new Product
                    {
                        ProductName = productVM.ProductName,
                        UnitPrice = productVM.UnitPrice,
                        UnitsInStock = productVM.UnitsInStock,
                        Discontinued = productVM.Discontinued,
                        UnitsOnOrder = productVM.UnitsOnOrder,
                        CategoryID = productVM.CategoryID,
                        ImageUrl = productVM.ImageUrl
                    };

                    _context.Products.Add(product);
                }
                else
                {
                    product = await _context.Products.FindAsync(productVM.ProductID);
                    if (product == null) return Json(new[] { productVM }.ToDataSourceResult(request, ModelState));

                    product.ProductName = productVM.ProductName;
                    product.UnitPrice = productVM.UnitPrice;
                    product.UnitsInStock = productVM.UnitsInStock;
                    product.Discontinued = productVM.Discontinued;
                    product.UnitsOnOrder = productVM.UnitsOnOrder;
                    product.CategoryID = productVM.CategoryID;

                    if (!string.IsNullOrEmpty(productVM.ImageUrl))
                        product.ImageUrl = productVM.ImageUrl;

                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();

                productVM.ProductID = product.ProductID;
                productVM.ImageUrl = product.ImageUrl;
            }

            return Json(new[] { productVM }.ToDataSourceResult(request, ModelState));
        }


        private string SanitizeFileName(string input)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input.Replace(" ", "_").ToLowerInvariant();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile ImageFile)
        {
            if (ImageFile == null || ImageFile.Length == 0)
                return Json(new { success = false, message = "No file uploaded." });

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "product-image");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            var imageUrl = "/product-image/" + fileName;

            return Json(new { success = true, imageUrl });
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

        public JsonResult GetCategories()
        {
            var categories = _context.Categories
                .Select(c => new { c.CategoryID, c.CategoryName })
                .ToList();

            return Json(categories);
        }


        public IActionResult AddProcuduct()
        {

            ViewBag.CategoryList = _context.Categories
               .Select(c => new
               {
                   CategoryID = c.CategoryID,
                   CategoryName = c.CategoryName
               })
               .ToList();

            var model = new Product();
            return View(model);
        }
    }
}
