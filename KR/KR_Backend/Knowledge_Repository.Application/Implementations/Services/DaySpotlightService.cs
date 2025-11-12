using Knowledge_Repository.Application.Dtos;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace Knowledge_Repository.Application.Implementations.Services
{
    public class DaySpotlightService : IDaySpotlightService
    {
        private readonly IDaySpotlightRepository _spotlightRepository;

        public DaySpotlightService(IDaySpotlightRepository spotlightRepository)
        {
            _spotlightRepository = spotlightRepository;
        }

        public async Task<DaySpotlightDto?> GetDaySpotlightAsync()
        {
            var spotlight = await _spotlightRepository.GetLatestSpotlightAsync();
            if (spotlight == null)
                return null;

            return new DaySpotlightDto
            {
                ResourceTitle = spotlight.ContentText,
                ResourceLink = spotlight.ResourceLink,
                Tip = "Use tagging consistently to boost searchability.",
                Quote = "Knowledge grows when shared."
            };
        }
    }
}
