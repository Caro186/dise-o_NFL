import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject, timer, Subscription } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

/**
 * Interfaz para las credenciales de login
 */
export interface LoginCredentials {
  email: string;
  password: string;
}

/**
 * Interfaz para los datos de registro
 */
export interface RegistroDto {
  email: string;
  password: string;
  nombreCompleto: string;
}

/**
 * Interfaz para la respuesta de login
 */
export interface LoginResponse {
  status: string;
  usuario?: UsuarioDto;
  token?: string;
  tokenExpiracion?: string;
}

/**
 * Interfaz para los datos del usuario
 */
export interface UsuarioDto {
  id: number;
  email: string;
  nombreCompleto: string;
  fechaRegistro: Date;
  rol?: string; // ← AGREGADO: Rol del usuario
}

/**
 * Interfaz para respuestas de error del servidor
 */
export interface ErrorResponse {
  mensaje: string;
  errores?: string[];
}

/**
 * Servicio de autenticación para gestionar login, registro y sesión de usuarios
 */
@Injectable({
  providedIn: 'root'
})
export class Authservice {
  private baseUrl = 'http://localhost:5000/api';
  private currentUserSubject: BehaviorSubject<UsuarioDto | null>;
  public currentUser: Observable<UsuarioDto | null>;
  private inactivityTimer?: Subscription;
  private readonly INACTIVITY_TIMEOUT = 12 * 60 * 60 * 1000; // 12 horas en milisegundos

  /**
   * Constructor del servicio de autenticación
   * @param http Cliente HTTP para realizar peticiones
   * @param router Router para navegación
   */
  constructor(private http: HttpClient, private router: Router) {
    // Recuperar usuario de localStorage si existe
    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<UsuarioDto | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();

    // Iniciar control de inactividad si hay usuario logueado
    if (this.currentUserValue) {
      this.checkSessionValidity();
      this.startInactivityTimer();
    }
  }

  /**
   * Obtiene el valor actual del usuario logueado
   */
  public get currentUserValue(): UsuarioDto | null {
    return this.currentUserSubject.value;
  }

  /**
   * Verifica si el usuario actual es administrador
   */
  public isAdmin(): boolean {
    const user = this.currentUserValue;
    return user?.rol === 'Admin' || user?.rol === 'Administrador';
  }

  /**
   * Verifica si hay un usuario logueado
   * @returns true si hay un usuario logueado, false en caso contrario
   */
  public isLoggedIn(): boolean {
    return this.currentUserValue !== null && this.getToken() !== null;
  }

  /**
   * Obtiene el token JWT del localStorage
   */
  private getToken(): string | null {
    return localStorage.getItem('token');
  }

  /**
   * Verifica si la sesión sigue siendo válida
   */
  private checkSessionValidity(): void {
    const lastActivity = localStorage.getItem('lastActivity');
    const tokenExpiration = localStorage.getItem('tokenExpiration');

    if (!lastActivity || !tokenExpiration) {
      this.logout();
      return;
    }

    const lastActivityDate = new Date(lastActivity);
    const tokenExpirationDate = new Date(tokenExpiration);
    const now = new Date();

    // Verificar si el token expiró
    if (now > tokenExpirationDate) {
      console.log('Token expirado');
      this.logout();
      return;
    }

    // Verificar si pasaron más de 12 horas de inactividad
    const timeSinceLastActivity = now.getTime() - lastActivityDate.getTime();
    if (timeSinceLastActivity > this.INACTIVITY_TIMEOUT) {
      console.log('Sesión expirada por inactividad');
      this.logout();
      return;
    }
  }

  /**
   * Inicia el timer de inactividad que verifica cada minuto
   */
  private startInactivityTimer(): void {
    // Verificar cada minuto si la sesión sigue válida
    this.inactivityTimer = timer(60000, 60000).subscribe(() => {
      this.checkSessionValidity();
    });
  }

  /**
   * Detiene el timer de inactividad
   */
  private stopInactivityTimer(): void {
    if (this.inactivityTimer) {
      this.inactivityTimer.unsubscribe();
    }
  }

  /**
   * Actualiza la última actividad del usuario
   */
  public updateLastActivity(): void {
    if (this.currentUserValue) {
      localStorage.setItem('lastActivity', new Date().toISOString());
    }
  }

  /**
   * Realiza el login de un usuario
   * @param credentials Credenciales de login (email y password)
   * @returns Observable con la respuesta del servidor
   */
  login(credentials: LoginCredentials): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/Auth/login`, credentials)
      .pipe(
        tap(response => {
          // Guardar usuario, token y timestamps en localStorage
          if (response.status === 'ok' && response.usuario && response.token) {
            localStorage.setItem('currentUser', JSON.stringify(response.usuario));
            localStorage.setItem('token', response.token);
            localStorage.setItem('tokenExpiration', response.tokenExpiracion || '');
            localStorage.setItem('lastActivity', new Date().toISOString());
            
            this.currentUserSubject.next(response.usuario);
            
            // Iniciar control de inactividad
            this.startInactivityTimer();
          }
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Registra un nuevo usuario
   * @param registroDto Datos del usuario a registrar
   * @returns Observable con la respuesta del servidor
   */
  register(registroDto: RegistroDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/Auth/register`, registroDto)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Cierra la sesión del usuario actual
   */
  logout(): void {
    // Detener timer de inactividad
    this.stopInactivityTimer();
    
    // Eliminar todo del localStorage
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    localStorage.removeItem('tokenExpiration');
    localStorage.removeItem('lastActivity');
    
    this.currentUserSubject.next(null);
    
    // Redirigir al login
    this.router.navigate(['/']);
  }

  /**
   * Maneja los errores HTTP
   * @param error Error HTTP recibido
   * @returns Observable con el error procesado
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Error desconocido';
    
    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      if (error.error && error.error.mensaje) {
        errorMessage = error.error.mensaje;
      } else {
        errorMessage = `Código de error: ${error.status}\nMensaje: ${error.message}`;
      }
    }
    
    console.error('Error en AuthService:', errorMessage);
    return throwError(() => error);
  }
}