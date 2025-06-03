namespace EventBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public string UserId { get; set; }
        public int NumberOfTickets { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }

        public bool IsPurchased { get; set; } = false;
        public DateTime? PurchaseDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }
    }
}
