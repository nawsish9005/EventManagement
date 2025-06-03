import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = 'https://localhost:7091/api/account/';
  private readonly tokenKey = 'token';
  
  constructor(private http: HttpClient, private router: Router) { }

  register(model: any): Observable<any> {
    return this.http.post(`${this.baseUrl}register`, model, {
      responseType: 'text'
    });
  }
  
    login(model: any): Observable<any> {
      return this.http.post(`${this.baseUrl}login`, model).pipe(
        tap((response: any) => {
          const token = response?.token || response?.Token;
          if (!token) {
            throw new Error('No token in response');
          }
          this.saveToken(token);
        })
      );
    }

    // Logout
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    console.log('Token removed:', this.getToken());
    this.router.navigate(['/login']);
  }

  // Check if user is authenticated
  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token && this.isTokenValid(token);
  }

  // Save token
  saveToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  // Get token
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

   // Decode token
   private decodeToken(token: string): any | null {
    try {
      return jwtDecode(token);
    } catch (error) {
      console.error('Invalid token', error);
      return null;
    }
  }

  // Validate token
  private isTokenValid(token: string): boolean {
    const decoded = this.decodeToken(token);
    if (!decoded || !decoded.exp) {
      return false;
    }
    return decoded.exp * 1000 > Date.now(); // Check if not expired
  }

  // Get user roles
  getUserRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];
    const decoded: any = this.decodeToken(token);
    const roles = decoded?.role;
    return Array.isArray(roles) ? roles : roles ? [roles] : [];
  }

  // Get username from token
  getUsername(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decoded: any = this.decodeToken(token);
    return decoded?.sub || null; // In your backend, sub = username
  }

  // Get userId from token
  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decoded: any = this.decodeToken(token);
    return decoded?.nameid || null; // In your backend, nameid = userId
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');  // Correct key is 'token'
    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return headers;
  }
}
function jwtDecode(token: string): any {
  throw new Error('Function not implemented.');
}

