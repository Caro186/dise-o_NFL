import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/authservice';

/**
 * Guard para proteger rutas que requieren autenticación
 * Redirige al login si el usuario no está autenticado
 * @returns true si el usuario está autenticado, false y redirige al login en caso contrario
 */
export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  // Redirigir al login si no está autenticado
  router.navigate(['/']);
  return false;
};