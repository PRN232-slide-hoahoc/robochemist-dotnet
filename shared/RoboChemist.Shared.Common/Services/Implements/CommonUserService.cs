using RoboChemist.Shared.Common.Services.Interfaces;

namespace RoboChemist.Shared.Common.Services.Implements
{
    public class CommonUserService : ICommonUserService
    {
        public Guid? GetCurrentUserId()
        {
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }
    }
}
