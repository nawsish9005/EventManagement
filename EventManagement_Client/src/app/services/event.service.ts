import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private baseUrl= 'https://localhost:7091/api';

  constructor(private http: HttpClient, private authService: AuthService) { }

  public eventUrl="/event";
  
  public getAllEvents(): Observable<any> {
    return this.http.get(this.baseUrl + this.eventUrl);
  }

  public getUpcomingEvents(): Observable<any> {
    return this.http.get(this.baseUrl + this.eventUrl + '/upcoming');
  }

  public getEventById(id: number): Observable<any> {
    return this.http.get(this.baseUrl + this.eventUrl + `/${id}`);
  }

  public createEvent(formData: FormData): Observable<any> {
    return this.http.post(this.baseUrl + this.eventUrl, formData);
  }

  updateEvent(id: number, data: FormData) {
    return this.http.put(`${this.baseUrl}/event/${id}`, data);
  }

  public deleteEvent(id: number): Observable<any> {
    return this.http.delete(this.baseUrl + this.eventUrl + `/${id}`);
  }


  public bookingUrl="/booking";
  
  public getUserBooking(): Observable<any> {
    return this.http.get(this.baseUrl + this.bookingUrl);
  }
  public getBookingById(id: number): Observable<any> {
    return this.http.get(this.baseUrl + this.bookingUrl + `/${id}`);
  }

  public createBooking(formData: FormData): Observable<any> {
    return this.http.post(this.baseUrl + this.bookingUrl, formData);
  }
  
  updateBooking(id: number, data: FormData) {
    return this.http.put(`${this.baseUrl}/event/${id}`, data);
  }

  public deleteBooking(id: number): Observable<any> {
    return this.http.delete(this.baseUrl + this.bookingUrl + `/${id}`);
  }

  getAllBooking(): Observable<any> {
    return this.http.get(this.baseUrl + this.bookingUrl);
  }
  

    private updateProfileUrl = '/account/updateprofile';
    private getProfileUrl = '/account/getprofile';

    public updateProfile(data: any): Observable<any> {
      return this.http.put(`${this.baseUrl}${this.updateProfileUrl}`, data);
    }
    
    public GetProfile(): Observable<any> {
      return this.http.get(`${this.baseUrl}${this.getProfileUrl}`);
    }
    
    public getAllUsers(): Observable<any[]> {
      return this.http.get<any[]>(this.roleBaseUrl + '/get-users');
    }

    private roleBaseUrl = this.baseUrl + '/account';

    // ✅ POST: Create a new role
    createRole(roleData: any): Observable<any> {
      return this.http.post(`${this.roleBaseUrl}/add-role`, roleData);
    }

    updateRole(id: string, newRoleName: any): Observable<any> {
      return this.http.put(`${this.roleBaseUrl}/roleUpdate?id=${id}`, newRoleName);
    }

    public getAllRoles() {
      return this.http.get<string[]>(this.roleBaseUrl + '/get-roles');
    }

    // ✅ DELETE: Remove role
    public deleteRole(id: string) {
      return this.http.delete(this.roleBaseUrl + `/roleDelete?id=${id}`);
    }

    // Assign role to user — unchanged
    assignRole(data: { userName: string; role: string }): Observable<any> {
      return this.http.post(`${this.roleBaseUrl}/assign-role`, data, {
        headers: new HttpHeaders({
          'Content-Type': 'application/json'
        }),
        observe: 'response'  // This will give you full response access
      }).pipe(
        catchError(error => {
          console.error('API Error:', error);
          return throwError(error);
        })
      );
    }

    // ✅ GET: Get all assigned roles for all users
    public getAllAssignedRoles(): Observable<any[]> {
      return this.http.get<any[]>(`${this.roleBaseUrl}/get-all-assign-role`);
    }

    // ✅ GET: Get assigned roles by username
    public getAssignedRoleById(userName: string): Observable<any> {
      return this.http.get(`${this.roleBaseUrl}/get-assign-role-by-id?username=${userName}`);
    }

    // ✅ PUT: Update user's assigned roles
    updateAssignedRole(payload: { userName: string, roles: string[] }): Observable<any> {
      return this.http.put('your-api-url', payload);
    }


    // ✅ DELETE: Remove role from user
    public deleteAssignedRole(data: { userName: string; role: string }): Observable<any> {
      return this.http.request('delete', `${this.roleBaseUrl}/delete-assign-role`, { body: data });
    }

    // Get role by ID — optional
    public getRoleById(id: string) {
      return this.http.get(this.roleBaseUrl + `/getRoleById?id=${id}`);
    }
}
