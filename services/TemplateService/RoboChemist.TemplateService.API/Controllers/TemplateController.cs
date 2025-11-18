using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Retrieves a paginated list of ACTIVE templates (for public users)
    /// </summary>
    /// <param name="paginationParams">Pagination and filtering parameters</param>
    /// <returns>Paginated list of active templates only</returns>
    /// <response code="200">Returns the paginated list of active templates</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// This endpoint returns ONLY ACTIVE templates (IsActive = true).
    /// Used for public-facing template browsing and selection.
    /// For Staff/Admin management that needs to see ALL templates (including inactive), use GET /api/v1/templates/staff endpoint.
    /// </remarks>
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
                $"Retrieved {pagedTemplates.Items.Count()} active templates from page {pagedTemplates.PageNumber}"
            );
            
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<Model.Models.Template>>.ErrorResult("Lỗi hệ thống"));
        }
    }

    /// <summary>
    /// Retrieves a paginated list of ALL templates including INACTIVE ones (for Staff/Admin management)
    /// </summary>
    /// <param name="paginationParams">Pagination and filtering parameters</param>
    /// <returns>Paginated list of all templates (active and inactive)</returns>
    /// <response code="200">Returns the paginated list of all templates</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// This endpoint returns ALL templates regardless of IsActive status.
    /// Used ONLY for Staff/Admin management interface to view, edit, and manage all templates.
    /// Regular users should use GET /api/v1/templates which filters to active templates only.
    /// </remarks>
    [HttpGet("staff")]
    [Authorize(Roles ="Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Model.Models.Template>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<Model.Models.Template>>>> GetAllTemplatesForStaff([FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var pagedTemplates = await _templateService.GetPagedTemplatesForStaffAsync(paginationParams);
            
            var response = ApiResponse<PagedResult<Model.Models.Template>>.SuccessResult(
                pagedTemplates,
                $"Retrieved {pagedTemplates.Items.Count()} templates (including inactive) from page {pagedTemplates.PageNumber}"
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
    /// Generate a presigned URL for template preview
    /// </summary>
    /// <param name="id">The unique identifier of the template</param>
    /// <param name="expirationMinutes">URL expiration time in minutes (default: 60)</param>
    /// <returns>Presigned URL for direct file access</returns>
    /// <response code="200">Returns the presigned URL</response>
    /// <response code="404">Template not found</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// This endpoint generates a temporary public URL (valid for 60 minutes by default)
    /// that can be used with Microsoft Office Online Viewer for PowerPoint preview.
    /// The URL provides direct access to the file without authentication.
    /// </remarks>
    [HttpGet("{id}/presigned-url")]
    [Authorize(Roles ="User, Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<string>>> GetPresignedUrl(Guid id, [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var presignedUrl = await _templateService.GeneratePresignedUrlAsync(id, expirationMinutes);
            
            if (string.IsNullOrEmpty(presignedUrl))
                return NotFound(ApiResponse<string>.ErrorResult($"Template with ID {id} not found"));

            var response = ApiResponse<string>.SuccessResult(
                presignedUrl,
                "Presigned URL generated successfully"
            );
            
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<string>.ErrorResult($"Template with ID {id} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for template {TemplateId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResult("Lỗi hệ thống"));
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

            _logger.LogInformation("Upload attempt for template: {FileName}", request.File.FileName);

            using var stream = request.File.OpenReadStream();
            var result = await _templateService.UploadTemplateAsync(stream, request.File.FileName, request);

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

    /// <summary>
    /// Update an existing template
    /// </summary>
    /// <param name="id">The unique identifier of the template to update</param>
    /// <param name="request">The update request containing new template metadata</param>
    /// <returns>The updated template</returns>
    /// <response code="200">Template successfully updated</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Template not found</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<Model.Models.Template>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<Model.Models.Template>>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<Model.Models.Template>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
            }

            var updatedTemplate = await _templateService.UpdateTemplateAsync(id, request);

            _logger.LogInformation("Template updated successfully: {TemplateId}", id);

            var response = ApiResponse<Model.Models.Template>.SuccessResult(
                updatedTemplate,
                "Template updated successfully"
            );

            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<Model.Models.Template>.ErrorResult($"Template with ID {id} not found"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating template {TemplateId}", id);
            return BadRequest(ApiResponse<Model.Models.Template>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, ApiResponse<Model.Models.Template>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
        }
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    /// <param name="id">The unique identifier of the template to delete</param>
    /// <returns>Success status</returns>
    /// <response code="200">Template successfully deleted</response>
    /// <response code="404">Template not found</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    /// <response code="500">Internal server error occurred</response>
    /// <remarks>
    /// This performs a soft delete - sets IsActive to false.
    /// The template and its file are not physically deleted from storage.
    /// Only Staff and Admin can delete templates.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff, Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(Guid id)
    {
        try
        {
            var result = await _templateService.DeleteTemplateAsync(id);

            if (!result)
                return NotFound(ApiResponse<bool>.ErrorResult($"Template with ID {id} not found"));

            _logger.LogInformation("Template deleted successfully: {TemplateId}", id);

            return Ok(ApiResponse<bool>.SuccessResult(true, "Template deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Lỗi hệ thống"));
        }
    }

    #endregion
}

