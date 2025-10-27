using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// Unit of Work implementation for grouping repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ITemplateRepository Templates { get; private set; }
    public IOrderRepository Orders { get; private set; }
    public IOrderDetailRepository OrderDetails { get; private set; }
    public IUserTemplateRepository UserTemplates { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Templates = new TemplateRepository(_context);
        Orders = new OrderRepository(_context);
        OrderDetails = new OrderDetailRepository(_context);
        UserTemplates = new UserTemplateRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

