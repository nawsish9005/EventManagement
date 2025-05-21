import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-assign-role',
  templateUrl: './assign-role.component.html',
  styleUrls: ['./assign-role.component.css']
})
export class AssignRoleComponent implements OnInit{
  assignRoleForm!: FormGroup;
  allRoles: { id: string, name: string }[] = [];
  allAssignedRoles: any[] = [];
  allUsers: string[] = [];
  message = '';
  error = '';
  isSubmitting = false;

  constructor(private fb: FormBuilder, private eventService: EventService) {}

  ngOnInit(): void {
    this.assignRoleForm = this.fb.group({
      userName: ['', Validators.required],
      role: ['', Validators.required]
    });

    this.getRoles();
    this.getAssignedRoles();
    this.getUsers();
  }

  getRoles() {
    this.eventService.getAllRoles().subscribe(response => {
      this.allRoles = response.map((r: any) => {
        // Normalize in case of nested structure
        return {
          id: r?.id?.id || r.id,
          name: r?.name?.name || r.name
        };
      });
    });
  }

  getAssignedRoles() {
    this.eventService.getAllAssignedRoles().subscribe(data => {
      this.allAssignedRoles = data;
    });
  }

  getUsers() {
    this.eventService.getAllUsers().subscribe(data => {
      this.allUsers = data.map((u: any) => u.userName);
    });
  }

  assignRole() {
    if (this.assignRoleForm.invalid) {
      this.assignRoleForm.markAllAsTouched();
      return;
    }
  
    this.isSubmitting = true;
    this.message = '';
    this.error = '';
  
    const payload = {
      userName: this.assignRoleForm.value.userName,
      role: this.assignRoleForm.value.role
    };
  
    this.eventService.assignRole(payload).subscribe({
      next: () => {
        Swal.fire({
          title: 'Success!',
          text: 'Role assigned successfully.',
          icon: 'success',
          confirmButtonText: 'OK'
        });
        this.assignRoleForm.reset();
        this.getAssignedRoles();
      },
      error: (err) => {
        Swal.fire({
          title: 'Error!',
          text: err.error?.message || 'Failed to assign role',
          icon: 'error',
          confirmButtonText: 'OK'
        });
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }

  updateAssignedRoleWithConfirm(userName: string, roles: string[]) {
    console.log('Roles passed:', roles);  // Check what is passed
  
    Swal.fire({
      title: 'Are you sure?',
      text: `Do you want to update roles for user "${userName}"?`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Yes, update!',
      cancelButtonText: 'Cancel',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.eventService.updateAssignedRole({ userName, roles }).subscribe(() => {
          Swal.fire('Updated!', `Roles for "${userName}" have been updated.`, 'success');
          this.getAssignedRoles();
        }, (error) => {
          Swal.fire('Error!', 'Failed to update roles.', 'error');
        });
      }
    });
  }
  deleteAssignedRole(userName: string, role: string) {
    Swal.fire({
      title: 'Are you sure?',
      text: `Do you want to remove the role "${role}" from user "${userName}"?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, remove it!',
      cancelButtonText: 'Cancel',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.eventService.deleteAssignedRole({ userName, role }).subscribe(() => {
          Swal.fire('Removed!', `Role "${role}" has been removed.`, 'success');
          this.getAssignedRoles();
        }, (error) => {
          Swal.fire('Error!', 'Failed to remove role.', 'error');
        });
      }
    });
  }
}
