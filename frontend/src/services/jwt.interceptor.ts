import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Authservice } from './authservice';

/**
 * Interceptor HTTP para agregar el token JWT a todas las peticiones
 * y actualizar la última actividad del usuario
 */
@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  
  constructor(private authService: Authservice) {}

  /**
   * Intercepta todas las peticiones HTTP salientes
   * @param request Petición HTTP original
   * @param next Handler para continuar con la petición
   * @returns Observable con el evento HTTP
   */
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Obtener token del localStorage
    const token = localStorage.getItem('token');
    const currentUser = this.authService.currentUserValue;

    // Si hay usuario logueado y token, agregar header de autorización
    if (currentUser && token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });

      // Actualizar última actividad en cada petición
      this.authService.updateLastActivity();
    }

    return next.handle(request);
  }
}