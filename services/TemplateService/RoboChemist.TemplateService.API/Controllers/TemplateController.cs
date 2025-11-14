using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.Common.Helpers;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.TemplateService.Model.DTOs;
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
    [Authorize(Roles ="User, Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Model.Models.Template>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<Model.Models.Template>>>> GetAllTemplates([FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var pagedTemplates = await _templateService.GetPagedTemplatesAsync(paginationParams);
            
            var response = ApiResponse<PagedResult<Model.Models.Template>>.SuccessResult(
                pagedTemplates,
                $"Retrieved {pagedTemplates.Items.Count()} templates from page {pagedTemplates.PageNumber}"
            );
            
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<Model.Models.Template>>.ErrorResult("Lỗi hệ thống"));
        }
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
    [Authorize(Roles ="User, Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<Model.Models.Template>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Model.Models.Template>>> GetTemplateById(Guid id)
    {
        try
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            
            if (template == null)
                return NotFound(ApiResponse<Model.Models.Template>.ErrorResult($"Template with ID {id} not found"));

            var response = ApiResponse<Model.Models.Template>.SuccessResult(
                template,
                "Template retrieved successfully"
            );
            
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<Model.Models.Template>.ErrorResult("Lỗi hệ thống"));
        }
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
    [Authorize(Roles ="User, Staff, Admin")]
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
            return NotFound(new { message = $"Template with ID {id} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = $"Template file not found in storage for template ID: {id}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading template {TemplateId}", id);
            return StatusCode(500, new { message = "Lỗi hệ thống" });
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
    /// <response code="401">Unauthorized - valid JWT token required</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// Supported file formats: .pptx, .ppt
    /// Maximum file size: 50 MB
    /// Requires authentication - JWT token must be provided
    /// </remarks>
    [HttpPost("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UploadTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UploadTemplateResponse>>> UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UploadTemplateResponse>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
            }

            // Validation ở Controller level
            if (request.File == null || request.File.Length == 0)
                return BadRequest(ApiResponse<UploadTemplateResponse>.ErrorResult("File is required"));

            var allowedExtensions = new[] { ".pptx", ".ppt" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(ApiResponse<UploadTemplateResponse>.ErrorResult("Only .pptx and .ppt files are allowed"));

            const long MaxFileSize = 50 * 1024 * 1024;
            if (request.File.Length > MaxFileSize)
                return BadRequest(ApiResponse<UploadTemplateResponse>.ErrorResult("File size must not exceed 50MB"));

            // Extract userId từ JWT token (giống ExamService)
            var user = HttpContext.User;
            if (user == null || !JwtHelper.TryGetUserId(user, out var userId))
            {
                return Unauthorized(ApiResponse<UploadTemplateResponse>.ErrorResult("Không xác thực được user từ token"));
            }

            _logger.LogInformation("Upload attempt for template: {FileName} by user: {UserId}", request.File.FileName, userId);

            using var stream = request.File.OpenReadStream();
            var result = await _templateService.UploadTemplateAsync(stream, request.File.FileName, request, userId);

            _logger.LogInformation("Template uploaded successfully: {TemplateId}", result.TemplateId);

            var response = ApiResponse<UploadTemplateResponse>.SuccessResult(
                result,
                "Template uploaded successfully"
            );
            
            return CreatedAtAction(
                nameof(GetTemplateById), 
                new { id = result.TemplateId }, 
                response
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading template");
            return StatusCode(500, ApiResponse<UploadTemplateResponse>.ErrorResult("Lỗi hệ thống"));
        }
    }

    #endregion
}
