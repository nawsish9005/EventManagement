using AutoMapper;
using EventBookingSystem.Data;
using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public EventController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromForm] EventCreateDto dto)
        {
            var ev = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Time = dto.Time,
                Venue = dto.Venue,
                Organizer = dto.Organizer,
                TicketPrice = dto.TicketPrice,
                TotalSeats = dto.TotalSeats,
                AvailableSeats = dto.TotalSeats
            };

            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder); // Make sure the folder exists

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageUrl.CopyToAsync(stream);
                }

                ev.ImageUrl = $"/images/{uniqueFileName}";
            }

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(ev);
            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, response);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<EventResponseDto>> UpdateEvent(int id, [FromForm] EventCreateDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();

            // Update basic fields
            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Time = dto.Time;
            ev.Venue = dto.Venue;
            ev.Organizer = dto.Organizer;
            ev.TicketPrice = dto.TicketPrice;

            // If total seats changed, recalculate available seats
            if (ev.TotalSeats != dto.TotalSeats)
            {
                int bookedSeats = ev.TotalSeats - ev.AvailableSeats;
                ev.TotalSeats = dto.TotalSeats;
                ev.AvailableSeats = Math.Max(0, ev.TotalSeats - bookedSeats);
            }

            // Handle image upload
            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, ev.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageUrl.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageUrl.CopyToAsync(stream);
                }

                ev.ImageUrl = $"/images/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEventById(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var dto = _mapper.Map<EventResponseDto>(ev);
            return dto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEvents()
        {
            var events = await _context.Events.ToListAsync();
            return _mapper.Map<List<EventResponseDto>>(events);
        }
    }
}
