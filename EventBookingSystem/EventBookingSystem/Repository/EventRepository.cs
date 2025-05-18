using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repository
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            return await _context.Events
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }
    }
}
