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

  public getAllEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
   public getEventById(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
   public updateEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
   public createEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
   public getUpComingEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
   public deleteEvent(){
    return this.http.get(this.baseUrl + this.eventUrl);
   }
}
