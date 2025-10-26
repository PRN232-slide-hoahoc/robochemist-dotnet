using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Interfaces
{
    public interface IPowerPointService
    {
        /// <summary>
        /// Import data into a PowerPoint template and save to output path
        /// </summary>
        /// <param name="data">Data to import</param>
        /// <param name="templatePath">Path to the PowerPoint template</param>
        /// <param name="outputPath">Path to save the output PowerPoint</param>
        void ImportDataToTemplate(ResponseGenerateDataDto data, string templatePath, string outputPath);
    }
}
