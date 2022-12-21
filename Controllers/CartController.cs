using AppDev.Data;
using AppDev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppDev.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        private string? _customerId;

        private string CustomerId
        {
            get
            {
                _customerId ??= userManager.GetUserId(User);
                return _customerId;
            }
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var items = await context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == CustomerId)
                .ToListAsync();
            return View(items);
        }

        // GET: Carts/Create
        public async Task<IActionResult> AddItem(int? bookId, int quantity = 1)
        {
            if (bookId == null || quantity <= 0)
                return BadRequest();

            var book = await context.Books.FindAsync(bookId);

            if (book == null)
                return BadRequest();

            var item = await context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == CustomerId && ci.BookId == book.Id);

            if (item == null)
            {
                item = new()
                {
                    CustomerId = CustomerId,
                    BookId = book.Id,
                    Quantity = quantity,
                };
                context.CartItems.Add(item);
            }
            else
            {
                item.Quantity += quantity;
            }

            await context.SaveChangesAsync();

            return Ok(item);
        }

        public async Task<IActionResult> Checkout()
        {
            var orders = await context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == CustomerId)
                .GroupBy(ci => ci.Book.StoreId)
                .Select(g => new Order(CustomerId, g.Key, g.ToList()))
                .ToListAsync();

            foreach (var order in orders)
            {
                order.Store = await context.Stores
                .AsNoTracking()
                .FirstAsync(s => s.Id == order.StoreId);
            };

            return View(orders);
        }

        [HttpPost, ActionName("Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirm()
        {
            var items = await context.CartItems
                .Include(ci => ci.Book)
                .Where(ci => ci.CustomerId == CustomerId)
                .ToListAsync();

            var orders = items.GroupBy(ci => ci.Book.StoreId, (storeId, items) => new Order(CustomerId, storeId, items));

            context.CartItems.RemoveRange(items);
            context.AddRange(orders);

            await context.SaveChangesAsync();

            return View("CheckoutSuccess");
        }
    }
}
