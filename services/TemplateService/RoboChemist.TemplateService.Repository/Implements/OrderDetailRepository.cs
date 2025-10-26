using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// OrderDetail repository implementation
/// </summary>
public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Where(od => od.OrderId == orderId)
            .Include(od => od.Template)
            .ToListAsync();
    }
}

