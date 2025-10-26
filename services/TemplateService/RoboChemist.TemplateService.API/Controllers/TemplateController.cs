using Microsoft.AspNetCore.Mvc;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Exceptions;
using RoboChemist.TemplateService.Service.Interfaces;

namespace RoboChemist.TemplateService.API.Controllers;

/// <summary>
/// Template management API endpoints
/// </summary>
[ApiController]
[Route("api/v1/templates")]
public class TemplateController : ControllerBase
{
    #region Fields

    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplateController> _logger;

    #endregion

    #region Constructor

    public TemplateController(
        ITemplateService templateService,
        ILogger<TemplateController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    #endregion

    #region Query Endpoints

    /// <summary>
    /// Retrieves a paginated list of templates
    /// </summary>
    /// <param name="paginationParams">Pagination and filtering parameters</param>
    /// <returns>Paginated list of templates</returns>
    /// <response code="200">Returns the paginated list of templates</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Model.Models.Template>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTemplates([FromQuery] PaginationParams paginationParams)
    {
        var pagedTemplates = await _templateService.GetPagedTemplatesAsync(paginationParams);
        
        var response = ApiResponse<PagedResult<Model.Models.Template>>.SuccessResponse(
            pagedTemplates,
            $"Retrieved {pagedTemplates.Items.Count()} templates from page {pagedTemplates.PageNumber}"
        );
        
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a specific template by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the template</param>
    /// <returns>The requested template details</returns>
    /// <response code="200">Returns the template details</response>
    /// <response code="404">Template not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Model.Models.Template>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTemplateById(Guid id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);
        
        if (template == null)
            throw new NotFoundException("Template", id);

        var response = ApiResponse<Model.Models.Template>.SuccessResponse(
            template,
            "Template retrieved successfully"
        );
        
        return Ok(response);
    }

    /// <summary>
    /// Downloads a template file as a stream
    /// </summary>
    /// <param name="id">The unique identifier of the template to download</param>
    /// <returns>The template file as a binary stream</returns>
    /// <response code="200">Returns the template file</response>
    /// <response code="404">Template not found or file not found in storage</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// This endpoint streams the template file directly to the client.
    /// The download count will be automatically incremented upon successful download.
    /// </remarks>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadTemplate(Guid id)
    {
        try
        {
            var (fileStream, contentType, fileName) = await _templateService.DownloadTemplateAsync(id);
            
            _logger.LogInformation("Template {TemplateId} downloaded: {FileName}", id, fileName);

            return File(fileStream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            throw new NotFoundException("Template", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException($"Template file not found in storage for template ID: {id}");
        }
    }

    #endregion

    #region Command Endpoints

    /// <summary>
    /// Uploads a new template file with metadata
    /// </summary>
    /// <param name="request">The upload request containing file and metadata</param>
    /// <returns>The upload result with template details</returns>
    /// <response code="201">Template successfully uploaded and created</response>
    /// <response code="400">Invalid request or file validation failed</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// Supported file formats: .pptx, .ppt
    /// Maximum file size: 50 MB
    /// </remarks>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UploadTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            throw new BadRequestException("File is required");

        var allowedExtensions = new[] { ".pptx", ".ppt" };
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            throw new BadRequestException("Only .pptx and .ppt files are allowed");

        const long MaxFileSize = 50 * 1024 * 1024;
        if (request.File.Length > MaxFileSize)
            throw new BadRequestException("File size must not exceed 50MB");

        using var stream = request.File.OpenReadStream();
        var result = await _templateService.UploadTemplateAsync(stream, request.File.FileName, request);

        _logger.LogInformation("Template uploaded successfully: {TemplateId}", result.TemplateId);

        var response = ApiResponse<UploadTemplateResponse>.SuccessResponse(
            result,
            "Template uploaded successfully",
            StatusCodes.Status201Created
        );
        
        return CreatedAtAction(
            nameof(GetTemplateById), 
            new { id = result.TemplateId }, 
            response
        );
    }

    #endregion
}
