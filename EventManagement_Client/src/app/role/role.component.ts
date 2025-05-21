import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventService } from '../services/event.service';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.css']
})
export class RoleComponent implements OnInit{
  roleForm: FormGroup;
  message: string = '';
  error: string = '';
  editRoleId: string | null = null;

  isLoading = false;
  roles: any[] = [];
  isEditMode: boolean = false;

  constructor(private fb: FormBuilder, private eventService: EventService) {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  get f() {
    return this.roleForm.controls;
  }

  onSubmit() {
    if (this.roleForm.invalid) return;

    this.isLoading = true;
    this.message = '';
    this.error = '';

    const roleData = this.roleForm.value;

    if (this.isEditMode && this.editRoleId) {
      // Update role
      this.eventService.updateRole(this.editRoleId, roleData).subscribe({
        next: (res: any) => {
          this.message = res.message || 'Role updated successfully';
          this.resetForm();
          this.loadRoles();
        },
        error: (err) => {
          this.error = err.error || 'Failed to update role';
        },
        complete: () => this.isLoading = false
      });
    } else {
      // Create role
      this.eventService.createRole(roleData).subscribe({
        next: (res: any) => {
          this.message = res.message || 'Role added successfully';
          this.resetForm();
          this.loadRoles();
        },
        error: (err) => {
          this.error = err.error || 'Failed to add role';
        },
        complete: () => this.isLoading = false
      });
    }
  }

  loadRoles() {
    this.eventService.getAllRoles().subscribe({
      next: (res: any[]) => {
        this.roles = res;
      },
      error: (err) => {
        console.error('Error loading roles', err);
        this.error = 'Failed to load roles';
      }
    });
  }

  editRole(role: any) {
    this.isEditMode = true;
    this.roleForm.patchValue({ name: role.name });
    this.editRoleId = role.id;
  }
  deleteRole(role: any) {
    if (!confirm(`Are you sure you want to delete the role "${role.name}"?`)) return;

    this.eventService.deleteRole(role.id).subscribe({
      next: (res: any) => {
        this.message = res.message || 'Role deleted successfully';
        this.loadRoles();
      },
      error: (err) => {
        this.error = err.error || 'Failed to delete role';
      }
    });
  }

  cancelEdit() {
    this.resetForm();
  }

  private resetForm() {
    this.roleForm.reset();
    this.isEditMode = false;
    this.editRoleId = null;
    this.message = '';
    this.error = '';
  }
} 
