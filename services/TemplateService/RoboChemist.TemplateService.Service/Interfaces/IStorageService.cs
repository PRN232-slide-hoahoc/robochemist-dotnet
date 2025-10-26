namespace RoboChemist.TemplateService.Service.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Upload file to cloud storage
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="folder">Folder path in storage</param>
    /// <returns>Object key of uploaded file</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder = "templates");
    
    /// <summary>
    /// Delete file from cloud storage
    /// </summary>
    /// <param name="objectKey">Object key to delete</param>
    Task DeleteFileAsync(string objectKey);
    
    /// <summary>
    /// Generate presigned URL for private file access
    /// </summary>
    /// <param name="objectKey">Object key</param>
    /// <param name="expirationMinutes">URL expiration time in minutes</param>
    /// <returns>Presigned URL</returns>
    Task<string> GeneratePresignedUrlAsync(string objectKey, int expirationMinutes = 60);
    
    /// <summary>
    /// Download file as stream from cloud storage
    /// </summary>
    /// <param name="objectKey">Object key to download</param>
    /// <returns>File stream and content type</returns>
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(string objectKey);
}

