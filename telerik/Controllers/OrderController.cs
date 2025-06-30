using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using telerik.Data;
using telerik.Models;

namespace telerik.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
                return NotFound();

            var model = new OrderViewModel
            {
                ProductId = product.ProductID,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductID == model.ProductId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return View(model);
                }

                int totalSold = _context.Sales
                    .Where(s => s.ProductId == model.ProductId)
                    .Sum(s => (int?)s.Quantity) ?? 0;

                if (totalSold + model.Quantity > product.UnitsInStock)
                {
                    TempData["ErrorMessage"] = "Not enough stock available.";
                    return View(model);
                }

                string userId = _userManager.GetUserId(User);

                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerName = model.FullName,
                    UserId = userId
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice
                };
                _context.OrderDetails.Add(orderDetail);

                var sale = new Sale
                {
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    DateSold = DateTime.Now
                };
                _context.Sales.Add(sale);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order placed successfully!";
                return RedirectToAction("Shop", "Product");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error placing order: " + ex.Message;
                return View(model);
            }
        }
    }
}
