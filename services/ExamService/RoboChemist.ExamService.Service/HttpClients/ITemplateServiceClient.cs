using RoboChemist.Shared.DTOs.FileDTOs;

namespace RoboChemist.ExamService.Service.HttpClients
{
    /// <summary>
    /// Client for communicating with Template Service API
    /// </summary>
    public interface ITemplateServiceClient
    {
        /// <summary>
        /// Upload file to storage service and get ObjectKey
        /// </summary>
        /// <param name="fileStream">File stream to upload</param>
        /// <param name="fileName">Original filename</param>
        /// <returns>FileUploadResponse containing ObjectKey and file metadata</returns>
        Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName);
    }
}
