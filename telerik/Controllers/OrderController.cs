using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using telerik.Data;
using telerik.Models;

namespace telerik.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
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

        // POST: /Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult Create(OrderViewModel model)
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

                // Calculate total quantity already sold
                int totalSold = _context.Sales
                    .Where(s => s.ProductId == model.ProductId)
                    .Sum(s => (int?)s.Quantity) ?? 0;

                if (totalSold + model.Quantity > product.UnitsInStock)
                {
                    TempData["ErrorMessage"] = "Not enough stock available.";
                    return View(model);
                }

                // Create Order
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerName = model.FullName
                };
                _context.Orders.Add(order);
                _context.SaveChanges();

                // Add order detail
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice
                };
                _context.OrderDetails.Add(orderDetail);

                // Add sale record
                var sale = new Sale
                {
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    DateSold = DateTime.Now
                };
                _context.Sales.Add(sale);

                _context.SaveChanges();

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
