using Microsoft.EntityFrameworkCore;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// UserTemplate repository implementation
/// </summary>
public class UserTemplateRepository : GenericRepository<UserTemplate>, IUserTemplateRepository
{
    public UserTemplateRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserTemplate>> GetUserTemplatesByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(ut => ut.UserId == userId)
            .Include(ut => ut.Template)
            .OrderByDescending(ut => ut.AcquiredAt)
            .ToListAsync();
    }

    public async Task<bool> UserHasTemplateAsync(Guid userId, Guid templateId)
    {
        return await _dbSet
            .AnyAsync(ut => ut.UserId == userId && ut.TemplateId == templateId);
    }
}

