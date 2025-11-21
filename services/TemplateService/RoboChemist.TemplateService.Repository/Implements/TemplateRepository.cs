using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

public class TemplateRepository : GenericRepository<Template>, ITemplateRepository
{
    private readonly AppDbContext _appContext;

    public TemplateRepository(DbContext context) : base(context)
    {
        _appContext = (AppDbContext)context;
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams)
    {
        var query = _appContext.Templates.Where(t => t.IsActive);

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            query = query.Where(t => 
                t.TemplateName.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        if (paginationParams.IsPremium.HasValue)
        {
            query = query.Where(t => t.IsPremium == paginationParams.IsPremium.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return PagedResult<Template>.Create(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams)
    {
        var query = _appContext.Templates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            query = query.Where(t => 
                t.TemplateName.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        if (paginationParams.IsPremium.HasValue)
        {
            query = query.Where(t => t.IsPremium == paginationParams.IsPremium.Value);
        }

        if (paginationParams.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == paginationParams.IsActive.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return PagedResult<Template>.Create(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<IEnumerable<Template>> GetFreeTemplatesAsync()
    {
        return await _appContext.Templates
            .Where(t => t.IsActive && !t.IsPremium)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}

