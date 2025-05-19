import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavbarComponent } from './frontEnd/navbar/navbar.component';
import { SidebarComponent } from './frontEnd/sidebar/sidebar.component';
import { DashboaredComponent } from './frontEnd/dashboared/dashboared.component';
import { EventComponent } from './event/event.component';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { BookingComponent } from './booking/booking.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    SidebarComponent,
    DashboaredComponent,
    EventComponent,
    BookingComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
