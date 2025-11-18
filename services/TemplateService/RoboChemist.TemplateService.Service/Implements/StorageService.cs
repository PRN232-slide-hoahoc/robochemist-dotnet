using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoboChemist.TemplateService.Service.Interfaces;
using System.IO;

namespace RoboChemist.TemplateService.Service.Implements;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<StorageService> logger)
    {
        _s3Client = s3Client;
        
        _bucketName = configuration["CLOUDFLARE_R2_BUCKET_NAME"] 
            ?? throw new ArgumentNullException("CLOUDFLARE_R2_BUCKET_NAME is not configured in .env file");

        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder = "templates")
    {
        try
        {
            _logger.LogInformation("Starting file upload to bucket: {BucketName}", _bucketName);

      
            await using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            // Tạo object key duy nhất
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var objectKey = $"{folder}/{uniqueFileName}";

            _logger.LogInformation("Uploading file with object key: {ObjectKey}", objectKey);

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = memoryStream, // Sử dụng MemoryStream mới
                ContentType = GetContentType(fileName),
                // Thêm CannedACL trở lại để đảm bảo file là private
                CannedACL = S3CannedACL.Private,
                // TẮT CHUNKED ENCODING - QUAN TRỌNG cho Cloudflare R2
                UseChunkEncoding = false
            };

            await _s3Client.PutObjectAsync(putRequest);

            _logger.LogInformation("File uploaded successfully: {ObjectKey}", objectKey);
            
            
            return objectKey;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 Error: {Message}, StatusCode: {StatusCode}", ex.Message, ex.StatusCode);
            throw new Exception($"Cloudflare R2 upload failed: {ex.Message}. " +
                "Please check your R2 credentials and bucket configuration.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            throw new Exception($"File upload failed: {ex.Message}", ex);
        }
    }

    public async Task DeleteFileAsync(string objectKey)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    public async Task<string> GeneratePresignedUrlAsync(string objectKey, int expirationMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Protocol = Protocol.HTTPS
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(string objectKey)
    {
        try
        {
            _logger.LogInformation("Downloading file from bucket: {BucketName}, key: {ObjectKey}", _bucketName, objectKey);

            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey
            };

            var response = await _s3Client.GetObjectAsync(getRequest);
            
            // Extract file name from object key (e.g., "templates/file_20241026.pptx" -> "file_20241026.pptx")
            var fileName = Path.GetFileName(objectKey);
            
            _logger.LogInformation("File downloaded successfully: {ObjectKey}, Size: {Size} bytes", objectKey, response.ContentLength);

            return (response.ResponseStream, response.Headers.ContentType, fileName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found: {ObjectKey}", objectKey);
            throw new FileNotFoundException($"File not found: {objectKey}", objectKey);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 Error while downloading: {Message}", ex.Message);
            throw new Exception($"Failed to download file from R2: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file download");
            throw new Exception($"File download failed: {ex.Message}", ex);
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}

