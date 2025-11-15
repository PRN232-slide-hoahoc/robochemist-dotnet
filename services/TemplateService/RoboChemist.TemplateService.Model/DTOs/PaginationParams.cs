using System.ComponentModel.DataAnnotations;

namespace RoboChemist.TemplateService.Model.DTOs;

/// <summary>
/// Pagination parameters for list queries
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Optional search term
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional filter for premium only
    /// </summary>
    public bool? IsPremium { get; set; }

    /// <summary>
    /// Sort field (default: CreatedAt)
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (asc/desc, default: desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

