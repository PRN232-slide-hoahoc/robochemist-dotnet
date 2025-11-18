using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// OrderDetail repository implementation
/// </summary>
public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    private readonly AppDbContext _appContext;

    public OrderDetailRepository(DbContext context) : base(context)
    {
        _appContext = (AppDbContext)context;
    }

    public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(Guid orderId)
    {
        return await _appContext.OrderDetails
            .Where(od => od.OrderId == orderId)
            .Include(od => od.Template)
            .ToListAsync();
    }
}

