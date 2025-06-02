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
import { LoginComponent } from './auth/login/login.component';
import { LogoutComponent } from './auth/logout/logout.component';
import { RegisterComponent } from './auth/register/register.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { AssignRoleComponent } from './assign-role/assign-role.component';
import { RoleComponent } from './role/role.component';
import { UpdateProfileComponent } from './update-profile/update-profile.component';
import { AuthInterceptor } from './auth/auth.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    SidebarComponent,
    DashboaredComponent,
    EventComponent,
    BookingComponent,
    LoginComponent,
    LogoutComponent,
    RegisterComponent,
    UnauthorizedComponent,
    AssignRoleComponent,
    RoleComponent,
    UpdateProfileComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
