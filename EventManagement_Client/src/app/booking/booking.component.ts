import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';
import { ToastrService } from 'ngx-toastr';
import { loadStripe } from '@stripe/stripe-js';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-booking',
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  bookingForm!: FormGroup;
  booking: Booking[] = [];
  events: any[] = [];
  selectedBookingId: number | null = null;
  selectedEvent: any = null;
  stripePromise = loadStripe('your_stripe_publishable_key'); // Replace with your key

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private toastr: ToastrService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadBookings();
    this.loadEvents();
  }

  initForm(): void {
    this.bookingForm = this.fb.group({
      eventId: ['', Validators.required],
      numberOfTickets: ['', [Validators.required, Validators.min(1)]]
    });
  }

  loadBookings(): void {
    this.eventService.getAllBooking().subscribe({
      next: (res) => {
        this.booking = res;
      },
      error: (err) => console.error('Failed to load bookings', err)
    });
  }

  loadEvents(): void {
    this.eventService.getAllEvents().subscribe({
      next: (res) => {
        this.events = res;
      },
      error: (err) => console.error('Failed to load events', err)
    });
  }

  onEventChange(): void {
    const selectedId = this.bookingForm.get('eventId')?.value;
    this.selectedEvent = this.events.find(e => e.id === +selectedId) || null;
  }

  submitForm(): void {
    if (this.bookingForm.invalid) return;

    const bookingData = this.bookingForm.value;

    if (this.selectedBookingId) {
      this.eventService.updateBooking(this.selectedBookingId, bookingData).subscribe({
        next: () => {
          this.toastr.success('Booking updated successfully!');
          this.loadBookings();
          this.loadEvents();
          this.resetForm();
        },
        error: err => console.error('Failed to update booking', err)
      });
    } else {
      this.eventService.createBooking(bookingData).subscribe({
        next: () => {
          this.toastr.success('Booking created successfully!');
          this.loadBookings();
          this.loadEvents();
          this.resetForm();
        },
        error: err => console.error('Failed to create booking', err)
      });
    }
  }

  editBooking(booking: any): void {
    this.selectedBookingId = booking.id;
    this.bookingForm.patchValue({
      eventId: booking.eventId,
      numberOfTickets: booking.numberOfTickets
    });
    this.selectedEvent = this.events.find(e => e.id === booking.eventId) || null;
  }

  deleteBooking(id: number): void {
    if (confirm('Are you sure you want to delete this booking?')) {
      this.eventService.deleteBooking(id).subscribe({
        next: () => {
          this.toastr.success('Booking deleted successfully!');
          this.loadBookings();
          this.loadEvents();
        },
        error: err => console.error('Failed to delete booking', err)
      });
    }
  }

  payNow(bookingId: number): void {
    if (confirm('Proceed to payment?')) {
      this.eventService.payForBooking(bookingId).subscribe({
        next: () => {
          this.toastr.success('Payment successful!');
          this.loadBookings();
        },
        error: err => {
          this.toastr.error('Payment failed!');
          console.error('Payment error', err);
        }
      });
    }
  }

  async pay(booking: Booking): Promise<void> {
    const stripe = await this.stripePromise;
    if (!stripe) {
      this.toastr.error('Stripe failed to load.');
      return;
    }
    try {
      const response = await this.http.post<any>(
        'https://localhost:7091/api/Booking/create-payment-intent',
        { amount: booking.totalAmount }
      ).toPromise();

      const result = await stripe.redirectToCheckout({
        clientReferenceId: booking.id.toString(),
        lineItems: [
          {
            price: 'your_price_id_here', // Replace with your real Price ID
            quantity: booking.numberOfTickets
          }
        ],
        mode: 'payment',
        successUrl: window.location.origin + '/booking-success',
        cancelUrl: window.location.origin + '/booking-cancel',
      });

      if (result.error) {
        console.error(result.error.message);
        this.toastr.error(result.error.message);
      }
    } catch (error) {
      console.error('Stripe checkout error:', error);
      this.toastr.error('Failed to initiate Stripe checkout');
    }
  }

  resetForm(): void {
    this.bookingForm.reset();
    this.selectedBookingId = null;
    this.selectedEvent = null;
  }
}

export interface Booking {
  id: number;
  eventId: number;
  eventTitle: string;
  numberOfTickets: number;
  totalAmount: number;
  bookingDate: Date;
  isPaid: boolean;
}
