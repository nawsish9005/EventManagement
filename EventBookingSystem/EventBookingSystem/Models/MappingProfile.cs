using AutoMapper;
using EventBookingSystem.Dto;

namespace EventBookingSystem.Models
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            // Event Mapping
            CreateMap<Event, EventResponseDto>();
            CreateMap<EventCreateDto, Event>()
                .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.TotalSeats));

            // Booking Mapping
            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event.Title));
            CreateMap<BookingRequestDto, Booking>();
        }
    }
}
