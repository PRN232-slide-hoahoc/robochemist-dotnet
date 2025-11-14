using RoboChemist.ExamService.Model.Models;

namespace RoboChemist.ExamService.Service.Interfaces
{
    /// <summary>
    /// Interface for Word document export service
    /// </summary>
    public interface IWordExportService
    {
        /// <summary>
        /// Export exam to Word document (.docx) with questions and answer key
        /// </summary>
        /// <param name="matrixName">Name of the exam matrix</param>
        /// <param name="questions">List of questions with options</param>
        /// <param name="totalQuestions">Total number of questions</param>
        /// <param name="timeLimit">Time limit in minutes (optional)</param>
        /// <returns>Byte array of the generated Word document</returns>
        Task<byte[]> ExportExamToWordAsync(
            string matrixName, 
            List<Question> questions,
            int totalQuestions,
            int? timeLimit = null);
    }
}
