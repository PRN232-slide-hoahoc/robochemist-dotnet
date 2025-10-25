using RoboChemist.ExamService.Repository.Interfaces;
using RoboChemist.ExamService.Service.Interfaces;

namespace RoboChemist.ExamService.Service.Implements
{
    public class MatrixService : IMatrixService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MatrixService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
