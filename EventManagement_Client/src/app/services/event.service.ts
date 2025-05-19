import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class EventService {
  private baseUrl= 'https://localhost:7091/api';

  constructor(private http: HttpClient, private authService: AuthService) { }

  public eventUrl="/events";

  public GetAllEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
}
