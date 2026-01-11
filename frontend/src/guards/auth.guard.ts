import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Authservice } from '../services/authservice';

/**
 * Guard para rutas que requieren autenticación (cualquier usuario logueado)
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(Authservice);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  alert('Debes iniciar sesión para acceder a esta página.');
  router.navigate(['/login']);
  return false;
};