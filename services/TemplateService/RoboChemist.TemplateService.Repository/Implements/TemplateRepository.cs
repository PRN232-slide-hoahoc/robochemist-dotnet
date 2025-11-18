using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.Data;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;

namespace RoboChemist.TemplateService.Repository.Implements;

/// <summary>
/// Template repository implementation with specific methods
/// </summary>
public class TemplateRepository : GenericRepository<Template>, ITemplateRepository
{
    private readonly AppDbContext _appContext;

    public TemplateRepository(DbContext context) : base(context)
    {
        _appContext = (AppDbContext)context;
    }

    public async Task<IEnumerable<Template>> GetActiveTemplatesAsync()
    {
        return await _appContext.Templates
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams)
    {
        // NOTE: This method returns ONLY ACTIVE templates (IsActive = true)
        // For Staff/Admin management that needs ALL templates (including inactive), use GetPagedTemplatesForStaffAsync
        var query = _appContext.Templates.Where(t => t.IsActive);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            query = query.Where(t => 
                t.TemplateName.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        // Apply premium filter
        if (paginationParams.IsPremium.HasValue)
        {
            query = query.Where(t => t.IsPremium == paginationParams.IsPremium.Value);
        }

        // Apply sorting
        query = paginationParams.SortBy.ToLower() switch
        {
            "name" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.TemplateName) 
                : query.OrderByDescending(t => t.TemplateName),
            "price" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.Price) 
                : query.OrderByDescending(t => t.Price),
            "downloadcount" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.DownloadCount) 
                : query.OrderByDescending(t => t.DownloadCount),
            _ => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.CreatedAt) 
                : query.OrderByDescending(t => t.CreatedAt)
        };

        // Get total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return PagedResult<Template>.Create(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams)
    {
        // NOTE: This method returns ALL templates (both active and inactive)
        // Used for Staff/Admin management to view and manage all templates
        var query = _appContext.Templates.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            query = query.Where(t => 
                t.TemplateName.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        // Apply premium filter
        if (paginationParams.IsPremium.HasValue)
        {
            query = query.Where(t => t.IsPremium == paginationParams.IsPremium.Value);
        }

        // Apply sorting
        query = paginationParams.SortBy.ToLower() switch
        {
            "name" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.TemplateName) 
                : query.OrderByDescending(t => t.TemplateName),
            "price" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.Price) 
                : query.OrderByDescending(t => t.Price),
            "downloadcount" => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.DownloadCount) 
                : query.OrderByDescending(t => t.DownloadCount),
            _ => paginationParams.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(t => t.CreatedAt) 
                : query.OrderByDescending(t => t.CreatedAt)
        };

        // Get total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return PagedResult<Template>.Create(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<IEnumerable<Template>> GetPremiumTemplatesAsync()
    {
        return await _appContext.Templates
            .Where(t => t.IsActive && t.IsPremium)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> GetFreeTemplatesAsync()
    {
        return await _appContext.Templates
            .Where(t => t.IsActive && !t.IsPremium)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementDownloadCountAsync(Guid templateId)
    {
        var template = await _appContext.Templates.FindAsync(templateId);
        if (template != null)
        {
            template.DownloadCount++;
            template.UpdatedAt = DateTime.UtcNow;
        }
    }
}

