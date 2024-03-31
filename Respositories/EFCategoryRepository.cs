using Lab22.DataAccess;
using Lab22.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab22.Respositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync();
            return await _context.Categories.ToListAsync();
        }
        public async Task<Category> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);

            // lấy thông tin kèm theo category
            return await _context.Categories.FindAsync(id);

        }
        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        //public async Task DeleteAsync(int id)
        //{


        //    var category = await _context.Categories.FindAsync(id);
        //    _context.Categories.Remove(category);
        //    await _context.SaveChangesAsync();
        //}
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Thể loại không tồn tại với id: {id}");
            }

            // Xóa tất cả sản phẩm thuộc thể loại
            var products = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
            _context.Products.RemoveRange(products);

            // Xóa thể loại
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
