using RoboChemist.ExamService.Model.Data;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Questions = new QuestionRepository(_context);
            Matrices = new MatrixRepository(_context);
            MatrixDetails = new MatrixdetailRepository(_context);
            ExamRequests = new ExamrequestRepository(_context);
            GeneratedExams = new GeneratedexamRepository(_context);
            ExamQuestions = new ExamquestionRepository(_context);
            Options = new OptionRepository(_context);
        }

        public IQuestionRepository Questions { get; }
        public IMatrixRepository Matrices { get; }
        public IMatrixdetailRepository MatrixDetails { get; }
        public IExamrequestRepository ExamRequests { get; }
        public IGeneratedexamRepository GeneratedExams { get; }
        public IExamquestionRepository ExamQuestions { get; }
        public IOptionRepository Options { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
