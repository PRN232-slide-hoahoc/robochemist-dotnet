using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Service.Interfaces;

namespace RoboChemist.TemplateService.API.Controllers;

/// <summary>
/// Generic file management API endpoints
/// Upload and download any file type
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Authorize] // Requires authentication
public class FileController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ILogger<FileController> _logger;

    public FileController(IStorageService storageService, ILogger<FileController> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload any file and return ObjectKey
    /// </summary>
    /// <param name="request">Request containing the file to upload</param>
    /// <returns>ObjectKey to save in your database</returns>
    /// <response code="200">File uploaded successfully, returns ObjectKey</response>
    /// <response code="400">Invalid file or validation failed</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// This API only uploads file to storage and returns ObjectKey.
    /// Users should save the ObjectKey to their database.
    /// 
    /// Maximum file size: 100 MB
    /// Files are automatically saved to "files" folder
    /// </remarks>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB
    [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FileUploadResponse>>> UploadFile([FromForm] FileUploadRequest request)
    {
        try
        {
            // Validation
            if (request.File == null || request.File.Length == 0)
                return BadRequest(ApiResponse<FileUploadResponse>.ErrorResult("File is required"));

            // Validate file size (100 MB max)
            const long MaxFileSize = 100 * 1024 * 1024;
            if (request.File.Length > MaxFileSize)
                return BadRequest(ApiResponse<FileUploadResponse>.ErrorResult("File size must not exceed 100MB"));

            // Mặc định lưu vào folder "files"
            const string folder = "files";

            _logger.LogInformation("Upload attempt for file: {FileName} to folder: {Folder}", 
                request.File.FileName, folder);

            // Upload file lên Cloudflare R2
            using var stream = request.File.OpenReadStream();
            var objectKey = await _storageService.UploadFileAsync(stream, request.File.FileName, folder);

            _logger.LogInformation("File uploaded successfully. ObjectKey: {ObjectKey}", objectKey);

            // Trả về response với ObjectKey
            var response = new FileUploadResponse
            {
                ObjectKey = objectKey,
                FileName = request.File.FileName,
                FileSize = request.File.Length,
                ContentType = request.File.ContentType,
                UploadedAt = DateTime.UtcNow,
                Message = "File uploaded successfully. Save this ObjectKey to download later."
            };

            return Ok(ApiResponse<FileUploadResponse>.SuccessResult(response, "File uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, ApiResponse<FileUploadResponse>.ErrorResult($"Lỗi hệ thống: {ex.Message}"));
        }
    }

    /// <summary>
    /// Download file using ObjectKey
    /// </summary>
    /// <param name="objectKey">ObjectKey of the file (e.g., files/filename_20241105.pdf)</param>
    /// <returns>File stream</returns>
    /// <response code="200">Returns the file</response>
    /// <response code="400">Invalid ObjectKey</response>
    /// <response code="404">File not found</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Use the ObjectKey received from Upload API to download file.
    /// 
    /// Example ObjectKeys: 
    /// - "files/document_20241105120000.pdf"
    /// - "files/spreadsheet_20241105120000.xlsx"
    /// </remarks>
    [HttpGet("download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFile([FromQuery] string objectKey)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(objectKey))
                return BadRequest(new { message = "ObjectKey is required" });

            // Validate objectKey format (prevent path traversal)
            if (objectKey.Contains(".."))
                return BadRequest(new { message = "Invalid ObjectKey format" });

            _logger.LogInformation("Download attempt for ObjectKey: {ObjectKey}", objectKey);

            // Download file từ Cloudflare R2
            var (fileStream, contentType, fileName) = await _storageService.DownloadFileAsync(objectKey);

            _logger.LogInformation("File downloaded successfully: {FileName}", fileName);

            // Trả về file stream
            return File(fileStream, contentType, fileName);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning("File not found: {ObjectKey}", objectKey);
            return NotFound(new { message = $"File not found: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file with ObjectKey: {ObjectKey}", objectKey);
            return StatusCode(500, new { message = $"Error downloading file: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete file by ObjectKey (Optional - use only when needed)
    /// </summary>
    /// <param name="objectKey">ObjectKey of the file to delete</param>
    /// <returns>Success status</returns>
    /// <response code="200">File deleted successfully</response>
    /// <response code="400">Invalid ObjectKey</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Be careful when using this API. File will be permanently deleted from storage.
    /// Should verify ownership before deletion.
    /// </remarks>
    [HttpDelete]
    [Authorize(Roles = "Admin,Staff")] // Only Admin/Staff can delete
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFile([FromQuery] string objectKey)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(objectKey))
                return BadRequest(ApiResponse<bool>.ErrorResult("ObjectKey is required"));

            // Validate objectKey format
            if (objectKey.Contains(".."))
                return BadRequest(ApiResponse<bool>.ErrorResult("Invalid ObjectKey format"));

            _logger.LogInformation("Delete attempt for ObjectKey: {ObjectKey}", objectKey);

            // Xóa file từ Cloudflare R2
            await _storageService.DeleteFileAsync(objectKey);

            _logger.LogInformation("File deleted successfully: {ObjectKey}", objectKey);

            return Ok(ApiResponse<bool>.SuccessResult(true, "File deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file with ObjectKey: {ObjectKey}", objectKey);
            return StatusCode(500, ApiResponse<bool>.ErrorResult($"Error deleting file: {ex.Message}"));
        }
    }
}
