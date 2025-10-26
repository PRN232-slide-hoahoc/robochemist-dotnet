namespace RoboChemist.TemplateService.Model.DTOs;

public class UploadTemplateResponse
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

