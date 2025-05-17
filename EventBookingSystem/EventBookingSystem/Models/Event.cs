namespace EventBookingSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Time { get; set; }
        public string Venue { get; set; }
        public string Organizer { get; set; }
        public string ImageUrl { get; set; }
        public decimal TicketPrice { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
