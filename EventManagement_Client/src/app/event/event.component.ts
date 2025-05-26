import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css']
})
export class EventComponent implements OnInit {
  events: any[] = [];
  eventForm: FormGroup;
  isEditMode = false;
  selectedEventId: number | null = null;
  imagePreview: string | ArrayBuffer | null = null;
  baseImageUrl = 'https://localhost:7091/images/';

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
      next: data => {
        this.events = data.map((ev: { imageUrl: string }) => ({
          ...ev,
          imageUrl: `https://localhost:7091${ev.imageUrl}`
        }));
      },
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
  
      if (key === 'imageUrl') {
        if (value instanceof File) {
          formData.append(key, value);
        }
      } else {
        formData.append(key, value);
      }
    });
  
    if (this.isEditMode && this.selectedEventId !== null) {
      this.eventService.updateEvent(this.selectedEventId!, formData).subscribe({
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

  formatTime(timeString: string): string {
    // If backend sends "2" or "2:0", normalize it to "02:00"
    const timeParts = timeString.split(':');
    const hours = timeParts[0].padStart(2, '0');
    const minutes = (timeParts[1] || '00').padStart(2, '0');
    return `${hours}:${minutes}`;
  }
  
  
  editEvent(event: any) {
    this.isEditMode = true;
    this.selectedEventId = event.id;
  
    this.imagePreview = event.imageUrl;
  
    const date = event.eventDate ? event.eventDate.split('T')[0] : '';
    const time = event.time ? this.formatTime(event.time) : '';
  
    this.eventForm.patchValue({
      title: event.title,
      description: event.description,
      eventDate: date,
      time: time,
      venue: event.venue,
      organizer: event.organizer,
      ticketPrice: event.ticketPrice,
      totalSeats: event.totalSeats,
      imageUrl: null
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
