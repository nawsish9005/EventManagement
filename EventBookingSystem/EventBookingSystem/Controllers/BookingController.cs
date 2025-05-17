using AutoMapper;
using EventBookingSystem.Data;
using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventBookingSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public BookingController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> BookTickets([FromBody] BookingRequestDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == dto.EventId);
            if (ev == null)
                return NotFound("Event not found.");

            if (dto.NumberOfTickets <= 0)
                return BadRequest("Number of tickets must be greater than 0.");

            if (ev.AvailableSeats < dto.NumberOfTickets)
                return BadRequest("Not enough available seats.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = new Booking
            {
                EventId = ev.Id,
                UserId = userId,
                NumberOfTickets = dto.NumberOfTickets,
                BookingDate = DateTime.UtcNow,
                TotalAmount = ev.TicketPrice * dto.NumberOfTickets
            };

            ev.AvailableSeats -= dto.NumberOfTickets;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Fetch Event for DTO mapping (if not included)
            await _context.Entry(booking).Reference(b => b.Event).LoadAsync();

            var response = _mapper.Map<BookingResponseDto>(booking);
            return Ok(response);
        }

        // GET: api/Booking/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var result = _mapper.Map<List<BookingResponseDto>>(bookings);
            return Ok(result);
        }

    }
}
