using EventBookingSystem.Models;

namespace EventBookingSystem.Repository.IRepository
{
    public interface IBookingRepository: IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(int eventId);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId);
    }
}
