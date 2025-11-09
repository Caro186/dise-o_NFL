import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Authservice } from '../services/authservice';

/**
 * Guard para rutas que solo pueden acceder administradores
 */
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(Authservice);
  const router = inject(Router);

  if (authService.isLoggedIn() && authService.isAdmin()) {
    return true;
  }

  alert('No tienes permisos para acceder a esta página. Solo administradores.');
  router.navigate(['/mainpage/perfil']);
  return false;
};