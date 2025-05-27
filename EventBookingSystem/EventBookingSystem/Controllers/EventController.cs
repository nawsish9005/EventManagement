using AutoMapper;
using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace EventBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepo;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public EventController(IEventRepository eventRepo, IMapper mapper, IWebHostEnvironment env)
        {
            _eventRepo = eventRepo;
            _mapper = mapper;
            _env = env;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromForm] EventCreateDto dto)
        {
            var ev = _mapper.Map<Event>(dto);
            ev.AvailableSeats = dto.TotalSeats;

            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
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

            await _eventRepo.AddAsync(ev);
            await _eventRepo.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(ev);
            return CreatedAtAction(nameof(GetEventById), new { id = ev.Id }, response);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<EventResponseDto>> UpdateEvent(int id, [FromForm] EventCreateDto dto)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null)
                return NotFound();

            // Update event properties
            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Time = dto.Time;
            ev.Venue = dto.Venue;
            ev.Organizer = dto.Organizer;
            ev.TicketPrice = dto.TicketPrice;

            // Adjust seats
            if (ev.TotalSeats != dto.TotalSeats)
            {
                int booked = ev.TotalSeats - ev.AvailableSeats;
                ev.TotalSeats = dto.TotalSeats;
                ev.AvailableSeats = Math.Max(0, ev.TotalSeats - booked);
            }

            // Handle image upload if provided
            if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
            {
                // ✅ 1. Validate file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(dto.ImageUrl.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Unsupported image file type. Only JPG, JPEG, PNG, and GIF are allowed.");
                }

                // ✅ 2. Validate file size (limit: 2MB)
                if (dto.ImageUrl.Length > 2 * 1024 * 1024)
                {
                    return BadRequest("Image file too large. Maximum allowed size is 2MB.");
                }

                // ✅ 3. Delete old image if exists
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, ev.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // ✅ 4. Save new image
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

            _eventRepo.Update(ev);
            await _eventRepo.SaveChangesAsync();

            var response = _mapper.Map<EventResponseDto>(ev);
            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEventById(int id)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null)
                return NotFound();

            return _mapper.Map<EventResponseDto>(ev);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAllEvents()
        {
            var events = await _eventRepo.GetAllAsync();
            return Ok(_mapper.Map<List<EventResponseDto>>(events));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetUpcomingEvents()
        {
            var upcoming = await _eventRepo.GetUpcomingEventsAsync();
            return Ok(_mapper.Map<List<EventResponseDto>>(upcoming));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _eventRepo.GetByIdAsync(id);
            if (ev == null) return NotFound();

            // Optionally delete the image
            if (!string.IsNullOrEmpty(ev.ImageUrl))
            {
                var oldImagePath = Path.Combine(_env.WebRootPath, ev.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            _eventRepo.Delete(ev);
            await _eventRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}
