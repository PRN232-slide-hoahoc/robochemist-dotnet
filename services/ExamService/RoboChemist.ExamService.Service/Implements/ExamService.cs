using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;

namespace RoboChemist.ExamService.Service.Implements
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
