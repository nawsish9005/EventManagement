import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-update-profile',
  templateUrl: './update-profile.component.html',
  styleUrls: ['./update-profile.component.css']
})
export class UpdateProfileComponent implements OnInit {
  profileForm: FormGroup;
  profileData: any = null;
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private fb: FormBuilder, 
    private eventService: EventService
  ) {
    this.profileForm = this.fb.group({
      UserName: ['', [Validators.required, Validators.minLength(3)]],
      Email: ['', [Validators.required, Validators.email]],
      PhoneNumber: ['', [Validators.required, Validators.pattern(/^(\+88|0088)?(01[3-9]\d{8})$/)]],
    });
  }

  ngOnInit(): void {
    this.loadProfileData();
  }

  loadProfileData() {
    this.isLoading = true;
    this.eventService.GetProfile().subscribe(
      (response: any) => {
        this.profileData = response;
        // Map the response to match the form controls
        this.profileForm.patchValue({
          UserName: response.userName || response.UserName,
          Email: response.email || response.Email,
          PhoneNumber: response.phoneNumber || response.PhoneNumber
        });
        this.isLoading = false;
      },
      error => {
        console.error('Error loading profile', error);
        this.errorMessage = 'Error loading profile';
        this.isLoading = false;
      }
    );
  }

  onSubmit(): void {
    if (this.profileForm.invalid) {
      return;
    }
    
    this.isLoading = true;
    const updatedProfile = this.profileForm.value;

    this.eventService.updateProfile(updatedProfile).subscribe({
      next: (res) => {
        this.successMessage = 'Profile updated successfully!';
        this.errorMessage = '';
        this.isLoading = false;
        this.loadProfileData(); // Refresh data
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Error updating profile';
        this.successMessage = '';
        this.isLoading = false;
      }
    });  
  }

  get f() {
    return this.profileForm.controls;
  }
}
