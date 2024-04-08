using Lab22.DataAccess;
using Lab22.Models;
using Lab22.Respositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Lab22.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        public ProductController(IProductRepository productRepository,
        ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }
        // Hiển thị danh sách sản phẩm

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int pageSize = 10;
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery,
            pageNumber, pageSize);
            return View(paginatedProducts);
        }
        // Hiển thị form thêm sản phẩm mới
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add()

        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); // Thay
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }

        // Xử lý thêm sản phẩm mới
        [HttpPost]

        public async Task<IActionResult> Add(Product product, IFormFile

        imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));

            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
        // Viết thêm hàm SaveImage (tham khảo bào 02)
        // Hiển thị thông tin chi tiết sản phẩm

        public async Task<IActionResult> Details(int id)

        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)

            {
                return NotFound();
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)

        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)

            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
        // Xử lý cập nhật sản phẩm
        [HttpPost]

        public async Task<IActionResult> Edit(Product product,

        IFormFile imageUrl, int id)
        {
            ModelState.Remove("ImageUrl");
            if (id != product.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingProduct = await
                _productRepository.GetByIdAsync(id);
                if (imageUrl == null)

                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);

        }
        // Hiển thị form xác nhận xóa sản phẩm
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)

        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)

            {
                return NotFound();
            }
            return View(product);
        }
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));

        }
        private async Task<string> DeleteImage(IFormFile image)
        {
            var fileProvider = new PhysicalFileProvider("wwwroot/images");            // Tạo đường dẫn lưu trữ hình ảnh
            var filePath = Path.Combine("wwwroot/images", image.FileName);

            // Xóa file hình ảnh
            if (fileProvider.GetFileInfo(filePath).Exists)
            {
                //fileProvider.DeleteFile(filePath);
            }

            return image.FileName;
        }
        //[HttpGet]
        //public IActionResult SearchProducts(string querry)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(querry))
                
        //            return BadRequest("Search querry is required.");
        //            var result = _context.Products.Where(p => p.Name.Contains(querry) || (p.Description != null && p.Description.Contains(querry))).ToList();
        //            return View("Index", result);
                
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        //public async Task<IActionResult> PagingNoLibrary(int pageNumber)
        //{
        //    int pageSize = 2;
        //    IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
        //    var pagedProducts = await productsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        //    return View(pagedProducts);
        //}
        public List<string> SearchSuggestions(string query)
        {

            return _context.Products

            .Where(p => p.Name.StartsWith(query))
            .Select(p => p.Name)
            .ToList();
         
        }
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string query, int pageNumber = 1)
        {
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category)
            .Where(p => p.Name.Contains(query));

            var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery, pageNumber, 10);
            return PartialView(paginatedProducts);
        }


    }
}
