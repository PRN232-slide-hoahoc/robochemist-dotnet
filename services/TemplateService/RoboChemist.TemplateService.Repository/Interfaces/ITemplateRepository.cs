using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;

namespace RoboChemist.TemplateService.Repository.Interfaces;

public interface ITemplateRepository : IGenericRepository<Template>
{
    Task<PagedResult<Template>> GetPagedTemplatesAsync(PaginationParams paginationParams);
    Task<PagedResult<Template>> GetPagedTemplatesForStaffAsync(PaginationParams paginationParams);
    Task<IEnumerable<Template>> GetFreeTemplatesAsync();
}