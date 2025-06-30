using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Linq;
using System.Threading.Tasks;
using telerik.Data;
using telerik.Models;

namespace telerik.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Categories_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = _context.Categories.Select(c => new
            {
                c.CategoryID,
                c.CategoryName
            });

            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost]
        public async Task<IActionResult> Categories_Create([DataSourceRequest] DataSourceRequest request, Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { category }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public async Task<IActionResult> Categories_Update([DataSourceRequest] DataSourceRequest request, Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { category }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public async Task<IActionResult> Categories_Destroy([DataSourceRequest] DataSourceRequest request, Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return Json(new[] { category }.ToDataSourceResult(request, ModelState));
        }
    }
}
