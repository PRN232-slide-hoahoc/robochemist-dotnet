namespace RoboChemist.ExamService.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IQuestionRepository Questions { get; }
        IMatrixRepository Matrices { get; }
        IMatrixdetailRepository MatrixDetails { get; }
        IExamrequestRepository ExamRequests { get; }
        IGeneratedexamRepository GeneratedExams { get; }
        IExamquestionRepository ExamQuestions { get; }
        IOptionRepository Options { get; } 
    }
}
