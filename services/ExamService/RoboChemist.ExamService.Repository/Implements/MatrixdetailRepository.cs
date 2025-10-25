using Microsoft.EntityFrameworkCore;
using RoboChemist.Shared.Common.GenericRepositories;
using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Repository.Interfaces;

namespace RoboChemist.ExamService.Repository.Implements
{
    public class MatrixdetailRepository : GenericRepository<Matrixdetail>, IMatrixdetailRepository
    {
        public MatrixdetailRepository(DbContext context) : base(context)
        {
        }
    }
}
