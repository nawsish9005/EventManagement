using EventBookingSystem.Data;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            // Retrieve all bookings with related data
            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            // Get current user's ID
            var userId = _userManager.GetUserId(User);

            // Query bookings for this user, including event details
            var bookings = await _context.Bookings
                .Include(b => b.Event)      // Include related Event
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking request)
        {
            // Find the event by ID
            var ev = await _context.Events.FindAsync(request.EventId);
            if (ev == null)
                return NotFound("Event not found.");

            // Ensure enough seats are available
            if (ev.AvailableSeats < request.NumberOfTickets)
                return BadRequest("Not enough available seats.");

            // Calculate total price
            var totalAmount = ev.TicketPrice * request.NumberOfTickets;

            // Get current user's ID (from Identity)
            var userId = _userManager.GetUserId(User);

            // Create the booking entity
            var booking = new Booking
            {
                EventId = ev.Id,
                UserId = userId,
                NumberOfTickets = request.NumberOfTickets,
                TotalAmount = totalAmount,
                BookingDate = DateTime.UtcNow
            };

            // Update available seats
            ev.AvailableSeats -= request.NumberOfTickets;

            // Add the new booking
            _context.Bookings.Add(booking);

            try
            {
                // Save all changes (booking + event update)
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrent updates to seat count
                return Conflict("Booking failed due to a concurrent update. Please try again.");
            }

            return Ok(booking); // Or CreatedAtAction(...) with location header in real use
        }

    }
}
