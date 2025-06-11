using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASPProjectFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPProjectFinal.Controllers
{
    public class OrdersController : Controller
    {
        private readonly Northwind _context;

        public OrdersController(Northwind context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string searchString)
        {
            var orders = from o in _context.Orders
                         select o;

            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int orderId))
                {
                    orders = orders.Where(o => o.OrderId == orderId || o.CustomerId!.Contains(searchString));
                }
                else
                {
                    orders = orders.Where(o => o.CustomerId!.Contains(searchString));
                }
                orders = orders.Take(100);
            }

            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Order_Details)
                .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShipViaNavigation)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Employees = _context.Employees.ToList();
            ViewBag.Shippers = _context.Shippers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View(new Order { Order_Details = new List<Order_Details>() });
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, string[] productIds, short[] quantities, decimal[] unitPrices, float[] discounts)
        {
            // Log current culture for debugging
            Console.WriteLine($"Current Culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Erreurs de validation : " + string.Join("; ", errors);
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }

            // Validate array inputs
            if (productIds == null || productIds.Length == 0 ||
                productIds.Length != quantities.Length ||
                productIds.Length != unitPrices.Length ||
                productIds.Length != discounts.Length)
            {
                TempData["ErrorMessage"] = "Veuillez ajouter au moins un détail de commande avec des données cohérentes.";
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }

            order.Order_Details = new List<Order_Details>();
            for (int i = 0; i < productIds.Length; i++)
            {
                if (!int.TryParse(productIds[i], out int productId))
                {
                    TempData["ErrorMessage"] = $"ID produit invalide à l'index {i} : {productIds[i]}";
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Employees = _context.Employees.ToList();
                    ViewBag.Shippers = _context.Shippers.ToList();
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = $"Produit avec ID {productId} n'existe pas.";
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Employees = _context.Employees.ToList();
                    ViewBag.Shippers = _context.Shippers.ToList();
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                if (quantities[i] <= 0)
                {
                    TempData["ErrorMessage"] = $"La quantité doit être positive pour le produit {productId}.";
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Employees = _context.Employees.ToList();
                    ViewBag.Shippers = _context.Shippers.ToList();
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                if (unitPrices[i] < 0)
                {
                    TempData["ErrorMessage"] = $"Le prix unitaire doit être non négatif pour le produit {productId}.";
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Employees = _context.Employees.ToList();
                    ViewBag.Shippers = _context.Shippers.ToList();
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                if (discounts[i] < 0 || discounts[i] > 100)
                {
                    TempData["ErrorMessage"] = $"La remise doit être entre 0 et 100 pour le produit {productId}.";
                    ViewBag.Customers = _context.Customers.ToList();
                    ViewBag.Employees = _context.Employees.ToList();
                    ViewBag.Shippers = _context.Shippers.ToList();
                    ViewBag.Products = _context.Products.ToList();
                    return View(order);
                }

                order.Order_Details.Add(new Order_Details
                {
                    ProductId = productId,
                    Quantity = quantities[i],
                    UnitPrice = unitPrices[i],
                    Discount = discounts[i] / 100f
                });
            }

            try
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "La commande a été créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de l'enregistrement : {ex.InnerException?.Message ?? ex.Message}";
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Order_Details)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Employees = _context.Employees.ToList();
            ViewBag.Shippers = _context.Shippers.ToList();
            ViewBag.Products = _context.Products.ToList();
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order, string[] productIds, short[] quantities, decimal[] unitPrices, float[] discounts)
        {
            // Log current culture and form data for debugging
            Console.WriteLine($"Current Culture: {System.Globalization.CultureInfo.CurrentCulture.Name}");
            Console.WriteLine($"Form Data: productIds={string.Join(",", productIds ?? new string[0])}, unitPrices={string.Join(",", unitPrices ?? new decimal[0])}");

            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Erreurs de validation : " + string.Join("; ", errors);
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }

            if (productIds == null || productIds.Length == 0 ||
                productIds.Length != quantities.Length ||
                productIds.Length != unitPrices?.Length ||
                productIds.Length != discounts.Length)
            {
                TempData["ErrorMessage"] = "Veuillez ajouter au moins un détail de commande avec des données cohérentes.";
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }

            try
            {
                var existingOrder = await _context.Orders
                    .Include(o => o.Order_Details)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (existingOrder == null)
                {
                    return NotFound();
                }

                existingOrder.CustomerId = order.CustomerId;
                existingOrder.EmployeeId = order.EmployeeId;
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.RequiredDate = order.RequiredDate;
                existingOrder.ShippedDate = order.ShippedDate;
                existingOrder.ShipVia = order.ShipVia;
                existingOrder.Freight = order.Freight;
                existingOrder.ShipName = order.ShipName;
                existingOrder.ShipAddress = order.ShipAddress;
                existingOrder.ShipCity = order.ShipCity;
                existingOrder.ShipRegion = order.ShipRegion;
                existingOrder.ShipPostalCode = order.ShipPostalCode;
                existingOrder.ShipCountry = order.ShipCountry;

                _context.Order_Details.RemoveRange(existingOrder.Order_Details);
                existingOrder.Order_Details = new List<Order_Details>();

                for (int i = 0; i < productIds.Length; i++)
                {
                    if (!int.TryParse(productIds[i], out int productId))
                    {
                        TempData["ErrorMessage"] = $"ID produit invalide à l'index {i} : {productIds[i]}";
                        ViewBag.Customers = _context.Customers.ToList();
                        ViewBag.Employees = _context.Employees.ToList();
                        ViewBag.Shippers = _context.Shippers.ToList();
                        ViewBag.Products = _context.Products.ToList();
                        return View(order);
                    }

                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                    {
                        TempData["ErrorMessage"] = $"Produit avec ID {productId} n'existe pas.";
                        ViewBag.Customers = _context.Customers.ToList();
                        ViewBag.Employees = _context.Employees.ToList();
                        ViewBag.Shippers = _context.Shippers.ToList();
                        ViewBag.Products = _context.Products.ToList();
                        return View(order);
                    }

                    if (quantities[i] <= 0)
                    {
                        TempData["ErrorMessage"] = $"La quantité doit être positive pour le produit {productId}.";
                        ViewBag.Customers = _context.Customers.ToList();
                        ViewBag.Employees = _context.Employees.ToList();
                        ViewBag.Shippers = _context.Shippers.ToList();
                        ViewBag.Products = _context.Products.ToList();
                        return View(order);
                    }

                    if (unitPrices[i] < 0)
                    {
                        TempData["ErrorMessage"] = $"Le prix unitaire doit être non négatif pour le produit {productId}.";
                        ViewBag.Customers = _context.Customers.ToList();
                        ViewBag.Employees = _context.Employees.ToList();
                        ViewBag.Shippers = _context.Shippers.ToList();
                        ViewBag.Products = _context.Products.ToList();
                        return View(order);
                    }

                    if (discounts[i] < 0 || discounts[i] > 100)
                    {
                        TempData["ErrorMessage"] = $"La remise doit être entre 0 et 100 pour le produit {productId}.";
                        ViewBag.Customers = _context.Customers.ToList();
                        ViewBag.Employees = _context.Employees.ToList();
                        ViewBag.Shippers = _context.Shippers.ToList();
                        ViewBag.Products = _context.Products.ToList();
                        return View(order);
                    }

                    existingOrder.Order_Details.Add(new Order_Details
                    {
                        OrderId = id,
                        ProductId = productId,
                        Quantity = quantities[i],
                        UnitPrice = unitPrices[i],
                        Discount = discounts[i] / 100f
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "La commande a été modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la mise à jour : {ex.InnerException?.Message ?? ex.Message}";
                ViewBag.Customers = _context.Customers.ToList();
                ViewBag.Employees = _context.Employees.ToList();
                ViewBag.Shippers = _context.Shippers.ToList();
                ViewBag.Products = _context.Products.ToList();
                return View(order);
            }
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShipViaNavigation)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                try
                {
                    _context.Order_Details.RemoveRange(order.Order_Details);
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "La commande a été supprimée avec succès.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = $"Erreur lors de la suppression : {ex.InnerException?.Message ?? ex.Message}";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}