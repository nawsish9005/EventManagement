import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css']
})
export class EventComponent implements OnInit {
  eventForm: FormGroup;
  events: any[] = [];
  selectedPhoto: File | null = null;
  imagePreview: string | ArrayBuffer | null = null;
  isEditMode: boolean = false;
  editEventId: number | null = null;

  baseUrl: string = 'https://localhost:7091'; // For image display

  constructor(private fb: FormBuilder, private eventService: EventService) {
    this.eventForm = this.fb.group({
      title: ['', Validators.required],
      organizer: ['', Validators.required],
      eventDate: ['', Validators.required],
      time: ['', Validators.required],
      venue: ['', Validators.required],
      ticketPrice: ['', Validators.required],
      totalSeats: ['', Validators.required],
      description: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.eventService.getAllEvents().subscribe({
      next: (data) => this.events = data,
      error: (err) => console.error('Error loading events', err)
    });
  }

  onImageSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedPhoto = file;

      const reader = new FileReader();
      reader.onload = () => this.imagePreview = reader.result;
      reader.readAsDataURL(file);
    }
  }

  submitForm(): void {
    if (this.eventForm.invalid) return;

    const formData = new FormData();
    Object.entries(this.eventForm.value).forEach(([key, value]) => {
      formData.append(key, value as string);
    });

    if (this.selectedPhoto) {
      formData.append('imageUrl', this.selectedPhoto); // ✅ Correct field name
    }

    if (this.isEditMode && this.editEventId !== null) {
      this.eventService.updateEvent(this.editEventId, formData).subscribe({
        next: () => {
          this.resetForm();
          this.loadEvents();
        },
        error: (err) => {
          console.error('Error updating event', err);
        }
      });
    } else {
      this.eventService.createEvent(formData).subscribe({
        next: () => {
          this.resetForm();
          this.loadEvents();
        },
        error: (err) => {
          console.error('Error creating event', err);
        }
      });
    }
  }

  editEvent(ev: any): void {
    this.isEditMode = true;
    this.editEventId = ev.id;
    this.selectedPhoto = null;

    const formattedDate = ev.eventDate?.split('T')[0] || '';

    this.eventForm.patchValue({
      title: ev.title,
      organizer: ev.organizer,
      eventDate: formattedDate,
      time: ev.time,
      venue: ev.venue,
      ticketPrice: ev.ticketPrice,
      totalSeats: ev.totalSeats,
      description: ev.description
    });

    this.imagePreview = `${this.baseUrl}${ev.imageUrl}`;
  }

  deleteEvent(id: number): void {
    if (confirm('Are you sure you want to delete this event?')) {
      this.eventService.deleteEvent(id).subscribe({
        next: () => this.loadEvents(),
        error: (err) => console.error('Error deleting event', err)
      });
    }
  }

  resetForm(): void {
    this.eventForm.reset();
    this.isEditMode = false;
    this.editEventId = null;
    this.selectedPhoto = null;
    this.imagePreview = null;
  }
}
