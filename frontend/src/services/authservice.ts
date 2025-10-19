import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

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
}

/**
 * Interfaz para los datos del usuario
 */
export interface UsuarioDto {
  id: number;
  email: string;
  nombreCompleto: string;
  fechaRegistro: Date;
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

  /**
   * Constructor del servicio de autenticación
   * @param http Cliente HTTP para realizar peticiones
   */
  constructor(private http: HttpClient) {
    // Recuperar usuario de localStorage si existe
    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<UsuarioDto | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  /**
   * Obtiene el valor actual del usuario logueado
   */
  public get currentUserValue(): UsuarioDto | null {
    return this.currentUserSubject.value;
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
          // Guardar usuario en localStorage y actualizar subject
          if (response.status === 'ok' && response.usuario) {
            localStorage.setItem('currentUser', JSON.stringify(response.usuario));
            this.currentUserSubject.next(response.usuario);
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
    // Eliminar usuario de localStorage
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  /**
   * Verifica si hay un usuario logueado
   * @returns true si hay un usuario logueado, false en caso contrario
   */
  isLoggedIn(): boolean {
    return this.currentUserValue !== null;
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