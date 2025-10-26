using Microsoft.EntityFrameworkCore.Storage;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// Unit of Work implementation for managing transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Repositories
    private ITemplateRepository? _templates;
    private IOrderRepository? _orders;
    private IOrderDetailRepository? _orderDetails;
    private IUserTemplateRepository? _userTemplates;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Template repository (lazy initialization)
    /// </summary>
    public ITemplateRepository Templates
    {
        get
        {
            _templates ??= new TemplateRepository(_context);
            return _templates;
        }
    }

    /// <summary>
    /// Order repository (lazy initialization)
    /// </summary>
    public IOrderRepository Orders
    {
        get
        {
            _orders ??= new OrderRepository(_context);
            return _orders;
        }
    }

    /// <summary>
    /// OrderDetail repository (lazy initialization)
    /// </summary>
    public IOrderDetailRepository OrderDetails
    {
        get
        {
            _orderDetails ??= new OrderDetailRepository(_context);
            return _orderDetails;
        }
    }

    /// <summary>
    /// UserTemplate repository (lazy initialization)
    /// </summary>
    public IUserTemplateRepository UserTemplates
    {
        get
        {
            _userTemplates ??= new UserTemplateRepository(_context);
            return _userTemplates;
        }
    }

    /// <summary>
    /// Save all changes to database
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Begin transaction
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Commit transaction
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    /// <summary>
    /// Rollback transaction
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

