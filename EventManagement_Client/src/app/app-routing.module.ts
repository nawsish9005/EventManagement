import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { EventComponent } from './event/event.component';
import { roleGuard } from './auth/role.guard';
import { DashboaredComponent } from './frontEnd/dashboared/dashboared.component';
import { UpdateProfileComponent } from './update-profile/update-profile.component';
import { RoleComponent } from './role/role.component';
import { AssignRoleComponent } from './assign-role/assign-role.component';
//,canActivate: [roleGuard], data: { roles: ['Admin'] }
const routes: Routes = [
  { path: 'login', title: 'Login', component: LoginComponent },
  { path: 'register', title: 'Register', component: RegisterComponent },
  { path: 'dashboard', title: 'Dashboard', component: DashboaredComponent },
  { path: 'event', title: 'Event', component: EventComponent },
  { path: 'updateProfile', title: 'Update profile', component: UpdateProfileComponent},
  { path: 'role', title: 'Role', component: RoleComponent},
  { path: 'assignRole', title: 'AssignRole', component: AssignRoleComponent},
  { path: 'unauthorized', component: UnauthorizedComponent},
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
