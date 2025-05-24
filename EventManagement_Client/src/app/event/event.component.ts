import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css']
})
export class EventComponent implements OnInit{
  events: any[] = [];
  eventForm: FormGroup;
  isEditMode = false;
  selectedEventId: number | null = null;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(private eventService: EventService, private fb: FormBuilder) {
    this.eventForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      eventDate: ['', Validators.required],
      time: ['', Validators.required],
      venue: ['', Validators.required],
      organizer: ['', Validators.required],
      ticketPrice: [0, Validators.required],
      totalSeats: [0, Validators.required],
      imageUrl: [null]
    });
  }
  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents() {
    this.eventService.getAllEvents().subscribe({
      next: data => this.events = data,
      error: err => console.error(err)
    });
  }
  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.eventForm.patchValue({ imageUrl: file });

      const reader = new FileReader();
      reader.onload = () => this.imagePreview = reader.result;
      reader.readAsDataURL(file);
    }
  }

  submitForm() {
    const formData = new FormData();
    Object.keys(this.eventForm.controls).forEach(key => {
      const value = this.eventForm.get(key)?.value;
      formData.append(key, value);
    });

    if (this.isEditMode && this.selectedEventId !== null) {
      this.eventService.updateEvent(this.selectedEventId, formData).subscribe({
        next: () => {
          this.resetForm();
          this.loadEvents();
        },
        error: err => console.error(err)
      });
    } else {
      this.eventService.createEvent(formData).subscribe({
        next: () => {
          this.resetForm();
          this.loadEvents();
        },
        error: err => console.error(err)
      });
    }
  }

  editEvent(event: any) {
    this.isEditMode = true;
    this.selectedEventId = event.id;
    this.imagePreview = event.imageUrl;

    this.eventForm.patchValue({
      title: event.title,
      description: event.description,
      eventDate: event.eventDate,
      time: event.time,
      venue: event.venue,
      organizer: event.organizer,
      ticketPrice: event.ticketPrice,
      totalSeats: event.totalSeats,
      imageUrl: null  // Image upload will be optional for updates
    });
  }

  deleteEvent(id: number) {
    if (confirm('Are you sure to delete this event?')) {
      this.eventService.deleteEvent(id).subscribe(() => {
        this.loadEvents();
      });
    }
  }

  resetForm() {
    this.eventForm.reset();
    this.isEditMode = false;
    this.selectedEventId = null;
    this.imagePreview = null;
  }
}
