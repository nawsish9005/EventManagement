﻿using AutoMapper;
using EventBookingSystem.Data;
using EventBookingSystem.Dto;
using EventBookingSystem.Models;
using EventBookingSystem.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;

namespace EventBookingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IEventRepository _eventRepo;
        private readonly IMapper _mapper;

        public BookingController(
            IBookingRepository bookingRepo,
            IEventRepository eventRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _eventRepo = eventRepo;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] BookingRequestDto dto)
        {
            var ev = await _eventRepo.GetByIdAsync(dto.EventId);
            if (ev == null)
                return NotFound("Event not found");

            if (dto.NumberOfTickets <= 0 || dto.NumberOfTickets > ev.AvailableSeats)
                return BadRequest("Invalid number of tickets or not enough available seats");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var booking = new Booking
            {
                EventId = ev.Id,
                UserId = userId,
                NumberOfTickets = dto.NumberOfTickets,
                BookingDate = DateTime.UtcNow,
                TotalAmount = ev.TicketPrice * dto.NumberOfTickets
            };

            // Update event seats
            ev.AvailableSeats -= dto.NumberOfTickets;

            await _bookingRepo.AddAsync(booking);
            _eventRepo.Update(ev);
            await _bookingRepo.SaveChangesAsync();

            var response = new BookingResponseDto
            {
                Id = booking.Id,
                EventId = ev.Id,
                EventTitle = ev.Title,
                NumberOfTickets = booking.NumberOfTickets,
                TotalAmount = booking.TotalAmount,
                BookingDate = booking.BookingDate
            };

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            var dto = new BookingResponseDto
            {
                Id = booking.Id,
                EventId = booking.EventId,
                EventTitle = booking.Event?.Title,
                NumberOfTickets = booking.NumberOfTickets,
                TotalAmount = booking.TotalAmount,
                BookingDate = booking.BookingDate
            };

            return Ok(dto);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetUserBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingRepo.GetBookingsByUserIdAsync(userId);

            var result = bookings.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                EventId = b.EventId,
                EventTitle = b.Event?.Title,
                NumberOfTickets = b.NumberOfTickets,
                TotalAmount = b.TotalAmount,
                BookingDate = b.BookingDate
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingRequestDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            booking.NumberOfTickets = dto.NumberOfTickets;
            booking.TotalAmount = booking.NumberOfTickets * booking.Event?.TicketPrice ?? 0;
            await _bookingRepo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return NotFound();

            // Get the event associated with this booking
            var ev = await _eventRepo.GetByIdAsync(booking.EventId);
            if (ev != null)
            {
                ev.AvailableSeats += booking.NumberOfTickets; // Restore the seats
                _eventRepo.Update(ev); // Mark event as updated
            }

            _bookingRepo.Delete(booking);
            await _bookingRepo.SaveChangesAsync(); // Save both event and booking changes

            return NoContent();
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetAllBookings()
        {
            var bookings = await _bookingRepo.GetAllWithEventAsync();

            var response = bookings.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                EventId = b.EventId,
                EventTitle = b.Event?.Title ?? "Unknown",
                NumberOfTickets = b.NumberOfTickets,
                TotalAmount = b.TotalAmount,
                BookingDate = b.BookingDate
            });

            return Ok(response);
        }

        [HttpPut("purchase/{id}")]
        public async Task<IActionResult> PurchaseBooking(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return NotFound("Booking not found");

            if (booking.IsPurchased)
                return BadRequest("This booking is already purchased.");

            booking.IsPurchased = true;
            booking.PurchaseDate = DateTime.UtcNow;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            return Ok(new { message = "Booking successfully purchased." });
        }

        [HttpPost("pay")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentRequestDto dto)
        {
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);
            if (booking == null)
                return NotFound("Booking not found");

            if (booking.IsPaid)
                return BadRequest("This booking is already paid.");

            // Simulate payment success (you can integrate real gateway later)
            booking.IsPaid = true;
            booking.PaymentDate = DateTime.UtcNow;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            return Ok(new { message = "Payment successful", paidAmount = booking.TotalAmount });
        }

        [HttpPost("create-payment-intent")]
        public IActionResult CreatePaymentIntent([FromBody] PaymentIntentRequestDto dto)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.Amount * 100), // amount in cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
            };

            var service = new PaymentIntentService();
            var intent = service.Create(options);

            return Ok(new { clientSecret = intent.ClientSecret });
        }



    }
}
