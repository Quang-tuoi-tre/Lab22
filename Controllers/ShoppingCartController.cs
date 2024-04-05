using Lab22.DataAccess;
using Lab22.Models;
using Lab22.Respositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;



namespace Lab22.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;


        public ShoppingCartController(IProductRepository productRepository,
        ICategoryRepository categoryRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _userManager = userManager;
        }
        List<CartItem>? GetCartItems()
        {
            string jsoncart = HttpContext.Session.GetString("cart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
            }
            return new List<CartItem>();
        }
        void SaveCartSession(List<CartItem> ls)
        {
            string jsoncart = JsonConvert.SerializeObject(ls);
            HttpContext.Session.SetString("cart"

            , jsoncart);

        }
        //        public IActionResult AddToCart(int productId, int quantity)
        //        {
        //            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId

        //            var product = GetProductFromDatabase(productId);
        //            var cartItem = new CartItem
        //            { 
        //                ProductId = productId,
        //Name = product.Name,
        //Price = product.Price,
        //Quantity = quantity
        //};

        //            var cart =

        //            HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
        //            ShoppingCart();
        //            cart.AddItem(cartItem);
        //            HttpContext.Session.SetObjectAsJson("Cart", cart);
        //            return RedirectToAction("Index");
        //        }
        public ActionResult Index()
        {
            var carts = GetCartItems();
            ViewBag.TongTien = carts.Sum(p => p.Price * p.Quantity);
            ViewBag.TongSoLuong = carts.Sum(p => p.Quantity);
            return View(carts);
        }
        //        // Các actions khác...
        //        private Product GetProductFromDatabase(int productId)
        //        {
        //            // Dependency injection for context
        //            using var _context = new CartItem(); // Replace with your context class name

        //            // Retrieve product with eager loading for related properties (optional)
        //            var product = await _context.Products
        //                .Include(p => p.Category) // Include Category if needed
        //                .FirstOrDefaultAsync(p => p.Id == productId);

        //            return product;
        //        }


        public ActionResult AddToCart(int id)
        {
            Product? itemProduct = _context.Products.FirstOrDefault(p => p.Id == id);
            if (itemProduct == null)
                return BadRequest("Sản phẩm không tồn tại");
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id.Equals(id));
            if (findCartItem == null)
            {
                //Th thêm mới vào giỏ hàng
                findCartItem = new CartItem()
                {
                    Id = itemProduct.Id,
                    Name = itemProduct.Name,
                    Image = itemProduct.ImageUrl,
                    Price = itemProduct.Price,
                    Quantity = 1
                };
                carts.Add(findCartItem);
            }
            else
                findCartItem.Quantity++;
            SaveCartSession(carts);
            return RedirectToAction("Index", "Product");
        }
        public ActionResult UpdateCart(int id, int quantity)
        {
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                findCartItem.Quantity = quantity;
                SaveCartSession(carts);
            }
            return RedirectToAction("Index");
        }
        public ActionResult DeleteCart(int id)
        {
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                carts.Remove(findCartItem);
                SaveCartSession(carts);
            }
            return RedirectToAction("Index");
        }

        //[HttpPost]
        //[Authorize]
        public async Task<IActionResult> Order()
        {
            try
            {
                var carts = GetCartItems();
                var user = await _userManager.GetUserAsync(User);
                var order = new Order
                {
                    UserId = user != null ? user.Id : null,
                    OrderDate = DateTime.UtcNow,
                    TotalPrice = carts.Sum(i => i.Price * i.Quantity),
                    OrderDetails = carts.Select(i => new OrderDetail
                    {
                        Order = new Order(),
                        ProductId = i.Id,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                //xóa session giỏ hàng
                HttpContext.Session.Remove("cart");
                return View("Order", order);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
