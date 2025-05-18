using EventBookingSystem.Models;

namespace EventBookingSystem.Repository.IRepository
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    }
}
