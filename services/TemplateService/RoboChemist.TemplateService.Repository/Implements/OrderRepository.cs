using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// Order repository implementation
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Template)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
    {
        return await _dbSet
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Template)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }
}

