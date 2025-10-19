using RoboChemist.SlidesService.Model.Data;
using RoboChemist.SlidesService.Repository.Interfaces;

namespace RoboChemist.SlidesService.Repository.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGradeRepository Grades { get; private set; }
        public ITopicRepository Topics { get; private set; }
        public ISyllabusRepository Syllabuses { get; private set; }
        public ISliderequestRepository Sliderequests { get; private set; }
        public IGeneratedslideRepository Generatedslides { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Grades = new GradeRepository(_context);
            Topics = new TopicRepository(_context);
            Syllabuses = new SyllabusRepository(_context);
            Sliderequests = new SliderequestRepository(_context);
            Generatedslides = new GeneratedslideRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}