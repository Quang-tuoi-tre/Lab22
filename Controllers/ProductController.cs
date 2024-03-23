using Lab22.Models;
using Lab22.Respositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab22.Controllers
{
    public class ProductController : Controller
    {

        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        public ProductController(IProductRepository productRepository,
        ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        // Hiển thị danh sách sản phẩm

        public async Task<IActionResult> Index()

        {
            var products = await _productRepository.GetAllAsync();

            return View(products);

        }
        // Hiển thị form thêm sản phẩm mới
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

        IFormFile imageUrl)
        {
            //    ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho

            //if (id != product.Id)
            //    {
            //        return NotFound();
            //    }
            if (ModelState.IsValid)
            {

                //                var existingProduct = await
                //                _productRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync
                //                                                     // Giữ nguyên thông tin hình ảnh nếu không có hình mới được

                if (imageUrl != null)
                {

                    //                {
                    //                    product.ImageUrl = existingProduct.ImageUrl;
                    //                }
                    //                else
                    //                {
                    //                    // Lưu hình ảnh mới
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                //                }
                //                // Cập nhật các thông tin khác của sản phẩm
                //                existingProduct.Name = product.Name;
                //                existingProduct.Price = product.Price;
                //                existingProduct.Description = product.Description;
                //                existingProduct.CategoryId = product.CategoryId;
                //                existingProduct.ImageUrl = product.ImageUrl;
                await _productRepository.UpdateAsync(product);

                //                return RedirectToAction(nameof(Index));


                //}
                //            var categories = await _categoryRepository.GetAllAsync();
                //            ViewBag.Categories = new SelectList(categories, "Id", "Name");
                return RedirectToAction("Index");
            }
            return View(product);

        }
        // Hiển thị form xác nhận xóa sản phẩm

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
    }
}
