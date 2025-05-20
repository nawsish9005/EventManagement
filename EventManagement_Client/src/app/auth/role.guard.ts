import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const expectedRoles = route.data['roles'] as string[];
  const userRoles = authService.getUserRoles();

  const hasRole = expectedRoles.some(role => userRoles.includes(role));

  if (authService.isAuthenticated() && hasRole) {
    return true;
  } else {
    router.navigate(['/unauthorized']);
    return false;
  }
};
