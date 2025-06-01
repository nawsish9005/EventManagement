import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-booking',
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit{
  bookingForm!: FormGroup;
  bookings: any[] = [];
  events: any[] = [];
  selectedBookingId: number | null = null;

  constructor(private fb: FormBuilder, private eventService: EventService) {}
  
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
      next: res => this.bookings = res,
      error: err => console.error('Failed to load bookings', err)
    });
  }

  loadEvents(): void {
    this.eventService.getAllEvents().subscribe({
      next: res => this.events = res,
      error: err => console.error('Failed to load events', err)
    });
  }

  submitForm(): void {
    if (this.bookingForm.invalid) return;

    const bookingData = this.bookingForm.value;

    if (this.selectedBookingId) {
      this.eventService.updateBooking(this.selectedBookingId, bookingData).subscribe({
        next: () => {
          this.loadBookings();
          this.resetForm();
        },
        error: err => console.error('Failed to update booking', err)
      });
    } else {
      this.eventService.createBooking(bookingData).subscribe({
        next: () => {
          this.loadBookings();
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
  }

  deleteBooking(id: number): void {
    if (confirm('Are you sure you want to delete this booking?')) {
      this.eventService.deleteBooking(id).subscribe({
        next: () => this.loadBookings(),
        error: err => console.error('Failed to delete booking', err)
      });
    }
  }

  resetForm(): void {
    this.bookingForm.reset();
    this.selectedBookingId = null;
  }
}
