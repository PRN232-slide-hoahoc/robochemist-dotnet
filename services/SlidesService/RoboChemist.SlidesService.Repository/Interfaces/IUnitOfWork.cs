namespace RoboChemist.SlidesService.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGradeRepository Grades { get; }

        ITopicRepository Topics { get; }

        ISyllabusRepository Syllabuses { get; }

        ISliderequestRepository Sliderequests { get; }

        IGeneratedslideRepository Generatedslides { get; }
    }
}
