using Microsoft.AspNetCore.Http;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.FileDTOs;
using System.Text.Json;

namespace RoboChemist.SlidesService.Service.HttpClients
{
    public class TemplateServiceClient : ITemplateServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TemplateServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

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

        public async Task<(Stream FileStream, string ContentType)> DownloadTemplateAsync(Guid templateId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("TemplateService");
                AuthorizeHttpClient(httpClient);

                var url = $"/api/v1/templates/{templateId}/download";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to download template. Status: {response.StatusCode}");
                }

                // Get content type from response headers
                var contentType = response.Content.Headers.ContentType?.ToString() 
                    ?? "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                // Get stream from response
                var stream = await response.Content.ReadAsStreamAsync();

                return (stream, contentType);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to download template with ID {templateId}", ex);
            }
        }

        public async Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("TemplateService");
                AuthorizeHttpClient(httpClient);

                // Create multipart/form-data content
                using var content = new MultipartFormDataContent();

                // Copy stream to memory to allow multiple reads
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var streamContent = new StreamContent(memoryStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.presentationml.presentation");

                content.Add(streamContent, "File", fileName);

                var url = "/api/v1/files/upload";
                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to upload file. Status: {response.StatusCode}, Error: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<FileUploadResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    throw new InvalidOperationException("Invalid response from file upload service");
                }

                return apiResponse.Data;
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to upload file: {fileName}", ex);
            }
        }

        public async Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string objectKey)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("TemplateService");
                AuthorizeHttpClient(httpClient);

                var url = $"/api/v1/files/download?objectKey={Uri.EscapeDataString(objectKey)}";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to download file. Status: {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.ToString()
                    ?? "application/vnd.openxmlformats-officedocument.presentationml.presentation";

                var stream = await response.Content.ReadAsStreamAsync();

                return (stream, contentType);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Failed to download file with ObjectKey {objectKey}", ex);
            }
        }
    }
}
