using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// UserTemplate repository implementation
/// </summary>
public class UserTemplateRepository : GenericRepository<UserTemplate>, IUserTemplateRepository
{
    private readonly AppDbContext _appContext;

    public UserTemplateRepository(DbContext context) : base(context)
    {
        _appContext = (AppDbContext)context;
    }

    public async Task<IEnumerable<UserTemplate>> GetUserTemplatesByUserIdAsync(Guid userId)
    {
        return await _appContext.UserTemplates
            .Where(ut => ut.UserId == userId)
            .Include(ut => ut.Template)
            .ToListAsync();
    }

    public async Task<bool> UserHasTemplateAsync(Guid userId, Guid templateId)
    {
        return await _appContext.UserTemplates
            .AnyAsync(ut => ut.UserId == userId && ut.TemplateId == templateId);
    }
}

