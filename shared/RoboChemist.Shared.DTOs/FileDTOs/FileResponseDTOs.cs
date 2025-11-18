namespace RoboChemist.Shared.DTOs.FileDTOs
{
    public class FileUploadResponse
    {
        public string ObjectKey { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
