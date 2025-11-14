using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.FileDTOs;
using System.Text.Json;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// HTTP Client for Template Service API
    /// </summary>
    public class TemplateServiceClient : ITemplateServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TemplateServiceClient> _logger;

        public TemplateServiceClient(
            IHttpClientFactory httpClientFactory, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<TemplateServiceClient> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Add Authorization header to HTTP client from current request context
        /// </summary>
        private void AuthorizeHttpClient(HttpClient httpClient)
        {
            var authToken = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(authToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }
        }

        /// <summary>
        /// Upload file to storage service via Template Service API
        /// </summary>
        public async Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                _logger.LogInformation("[TemplateServiceClient] Uploading file: {FileName}", fileName);

                var httpClient = _httpClientFactory.CreateClient("ApiGateway");
                AuthorizeHttpClient(httpClient);

                // Create multipart/form-data content
                using var content = new MultipartFormDataContent();

                // Copy stream to memory to allow multiple reads
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var streamContent = new StreamContent(memoryStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

                content.Add(streamContent, "File", fileName);

                var url = "/template/v1/files/upload";
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[TemplateServiceClient] Upload failed. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Failed to upload file. Status: {response.StatusCode}, Error: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<FileUploadResponse>>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    _logger.LogError("[TemplateServiceClient] Invalid response from upload service");
                    throw new InvalidOperationException("Invalid response from file upload service");
                }

                _logger.LogInformation("[TemplateServiceClient] File uploaded successfully. ObjectKey: {ObjectKey}", 
                    apiResponse.Data.ObjectKey);

                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[TemplateServiceClient] HTTP error uploading file: {FileName}", fileName);
                throw new HttpRequestException($"Failed to upload file: {fileName}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TemplateServiceClient] Unexpected error uploading file: {FileName}", fileName);
                throw;
            }
        }
    }
}
