import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Authservice } from '../services/authservice';

/**
 * Guard para proteger rutas que requieren autenticación
 * Verifica que el usuario esté logueado y tenga un token válido
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(Authservice);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Si no está logueado, redirigir al login
  router.navigate(['/'], { queryParams: { returnUrl: state.url } });
  return false;
};