using EventBookingSystem.Data;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(int eventId)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.EventId == eventId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllWithEventAsync()
        {
            return await _context.Bookings
                .Include(b => b.Event)
                .ToListAsync();
        }

    }
}
