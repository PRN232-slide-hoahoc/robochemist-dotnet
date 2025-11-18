using RoboChemist.Shared.DTOs.FileDTOs;

namespace RoboChemist.SlidesService.Service.HttpClients
{
    public interface ITemplateServiceClient
    {
        /// <summary>
        /// Download template file as stream from TemplateService
        /// </summary>
        /// <param name="templateId">Template unique identifier</param>
        /// <returns>Tuple containing file stream, content type, and filename</returns>
        Task<(Stream FileStream, string ContentType)> DownloadTemplateAsync(Guid templateId);

        /// <summary>
        /// Upload file to storage service and get ObjectKey
        /// </summary>
        /// <param name="fileStream">File stream to upload</param>
        /// <param name="fileName">Original filename</param>
        /// <returns>FileUploadResponse containing ObjectKey</returns>
        Task<FileUploadResponse> UploadFileAsync(Stream fileStream, string fileName);

        /// <summary>
        /// Download file from storage by ObjectKey
        /// </summary>
        /// <param name="objectKey">Storage object key from FilePath</param>
        /// <returns>Tuple containing file stream and content type</returns>
        Task<(Stream FileStream, string ContentType)> DownloadFileAsync(string objectKey);
    }
}
