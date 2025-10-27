import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

export interface Usuario {
  id: number;
  email: string;
  nombreCompleto: string;
  fechaRegistro: string;
  estado: 'ok' | 'bloqueada';
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginResponse {
  status: string;
  token: string;
  expiracionToken: string;
  usuario: Usuario;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnDestroy {
  private apiUrl = 'http://localhost:5000/api/auth';
  private SESSION_DURATION = 12 * 60 * 60 * 1000; // 12 horas en ms

  private currentUserSubject: BehaviorSubject<Usuario | null>;
  public currentUser: Observable<Usuario | null>;

  private sessionCheckInterval: any;

  constructor(private http: HttpClient) {
    const storedUser = this.getStoredUser();
    this.currentUserSubject = new BehaviorSubject<Usuario | null>(storedUser);
    this.currentUser = this.currentUserSubject.asObservable();

    this.startSessionCheck();
  }

  /** Login del usuario */
  login(loginDto: LoginDto): Observable<LoginResponse> {
    console.log('🚀 [AUTH SERVICE] Enviando petición de login:', {
      email: loginDto.email,
      password: '***' + loginDto.password.slice(-2), // Solo últimos 2 chars por seguridad
      url: `${this.apiUrl}/login`
    });

    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, loginDto).pipe(
      tap(response => {
        console.log('✅ [AUTH SERVICE] Respuesta COMPLETA recibida:', response);
        console.log('✅ [AUTH SERVICE] Tipo de respuesta:', typeof response);
        console.log('✅ [AUTH SERVICE] Claves de la respuesta:', Object.keys(response || {}));
        
        // Verificar si la respuesta tiene la estructura esperada
        if (!response) {
          console.error('❌ [AUTH SERVICE] Respuesta es null o undefined');
          throw new Error('Respuesta vacía del servidor');
        }

        console.log('✅ [AUTH SERVICE] Respuesta desglosada:', {
          status: response.status,
          token: response.token ? 'token recibido (primeros 10 chars): ' + response.token.substring(0, 10) + '...' : 'No token',
          expiracionToken: response.expiracionToken,
          Usuario: response.usuario,
          EstadoCuenta: response.usuario?.estado || 'NO ENCONTRADO'
        });

        // Solo guardar si existe Usuario y la cuenta está activa
        if (!response.usuario) {
          console.error('❌ [AUTH SERVICE] No se encontró objeto Usuario en la respuesta');
          throw new Error('Respuesta inválida del servidor - falta Usuario');
        }

        if (response.status === 'ok') {
          console.log('✅ [AUTH SERVICE] Cuenta activa - Guardando en localStorage');
          localStorage.setItem('token', response.token);
          localStorage.setItem('lastActivity', Date.now().toString());
          localStorage.setItem('usuario', JSON.stringify(response.usuario));
          this.currentUserSubject.next(response.usuario);
          console.log('✅ [AUTH SERVICE] Sesión guardada correctamente');
        } else {
          console.warn('⚠️ [AUTH SERVICE] Cuenta NO activa - Estado:', response.usuario.estado);
        }
      }),
      catchError((error) => {
        console.error('❌ [AUTH SERVICE] Error capturado en catchError:', {
          status: error.status,
          statusText: error.statusText,
          message: error.message,
          error: error.error,
          fullError: error
        });
        return this.handleError(error);
      })
    );
  }

  /** Registro de un nuevo usuario */
  register(registroDto: { nombreCompleto: string, email: string, password: string }): Observable<any> {
    console.log('🚀 [AUTH SERVICE] Enviando petición de registro:', {
      nombreCompleto: registroDto.nombreCompleto,
      email: registroDto.email,
      url: `${this.apiUrl}/register`
    });
    return this.http.post(`${this.apiUrl}/register`, registroDto).pipe(
      tap(response => console.log('✅ [AUTH SERVICE] Registro exitoso:', response)),
      catchError(error => {
        console.error('❌ [AUTH SERVICE] Error en registro:', error);
        return throwError(() => error);
      })
    );
  }

  /** Logout */
  logout(): void {
    console.log('🚪 [AUTH SERVICE] Cerrando sesión');
    this.clearSession();
    this.currentUserSubject.next(null);
  }

  /** Obtener usuario actual */
  getCurrentUser(): Usuario | null {
    return this.currentUserSubject.value;
  }

  /** Getter simplificado */
  get currentUserValue(): Usuario | null {
    return this.currentUserSubject.value;
  }

  /** Verificar si hay usuario logueado */
  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  /** Obtener ID del usuario logueado */
  getUsuarioId(): number | null {
    const usuario = this.getCurrentUser();
    return usuario ? usuario.id : null;
  }

  /** Obtener usuario guardado en localStorage */
  private getStoredUser(): Usuario | null {
    const userJson = localStorage.getItem('usuario');
    const lastActivity = localStorage.getItem('lastActivity');
    
    console.log('🔍 [AUTH SERVICE] Verificando usuario almacenado:', {
      tieneUsuario: !!userJson,
      tieneLastActivity: !!lastActivity
    });

    if (!userJson || !lastActivity) return null;

    const age = Date.now() - parseInt(lastActivity);
    console.log('🕐 [AUTH SERVICE] Edad de sesión:', {
      edadMs: age,
      edadMinutos: Math.floor(age / 60000),
      duracionMaxMs: this.SESSION_DURATION,
      expirada: age > this.SESSION_DURATION
    });

    if (age > this.SESSION_DURATION) {
      console.warn('⚠️ [AUTH SERVICE] Sesión expirada - Limpiando');
      this.clearSession();
      return null;
    }

    try {
      const usuario = JSON.parse(userJson);
      console.log('✅ [AUTH SERVICE] Usuario restaurado desde localStorage:', usuario);
      return usuario;
    } catch (e) {
      console.error('❌ [AUTH SERVICE] Error al parsear usuario:', e);
      this.clearSession();
      return null;
    }
  }

  /** Limpiar sesión */
  private clearSession(): void {
    console.log('🧹 [AUTH SERVICE] Limpiando sesión de localStorage');
    localStorage.removeItem('usuario');
    localStorage.removeItem('token');
    localStorage.removeItem('lastActivity');
  }

  /** Actualizar timestamp de actividad */
  registerActivity(): void {
    if (this.isLoggedIn()) {
      localStorage.setItem('lastActivity', Date.now().toString());
      console.log('⏱️ [AUTH SERVICE] Actividad registrada');
    }
  }

  /** Verificar expiración de sesión cada minuto */
  private startSessionCheck(): void {
    this.sessionCheckInterval = setInterval(() => {
      const lastActivity = localStorage.getItem('lastActivity');
      if (!lastActivity) return;
      if (Date.now() - parseInt(lastActivity) > this.SESSION_DURATION) {
        console.log('⏰ [AUTH SERVICE] Sesión expirada por inactividad');
        this.logout();
      }
    }, 60000);
  }

  ngOnDestroy(): void {
    clearInterval(this.sessionCheckInterval);
  }

  /** Manejo de errores HTTP */
  private handleError(error: HttpErrorResponse) {
    console.error('🔴 [AUTH SERVICE - HANDLE ERROR] Procesando error:', {
      status: error.status,
      statusText: error.statusText,
      errorMessage: error.message,
      errorObject: error.error,
      url: error.url
    });
    
    let errorMessage = '';

    if (error.status === 0) {
      errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
      console.error('🔴 [AUTH SERVICE] Error de conexión - Backend no disponible');
    } else if (error.status === 400) {
      errorMessage = 'Credenciales inválidas';
      console.error('🔴 [AUTH SERVICE] Error 400 - Datos inválidos');
    } else if (error.status === 401) {
      errorMessage = 'Email o contraseña incorrectos';
      console.error('🔴 [AUTH SERVICE] Error 401 - No autorizado');
    } else if (error.status === 423) {
      errorMessage = 'bloqueada'; // Palabra clave para detectar en el componente
      console.error('🔴 [AUTH SERVICE] Error 423 - Cuenta bloqueada');
    } else if (error.error?.message) {
      errorMessage = error.error.message;
      console.error('🔴 [AUTH SERVICE] Error del servidor:', error.error.message);
    } else {
      errorMessage = 'Error al conectar con el servidor';
      console.error('🔴 [AUTH SERVICE] Error desconocido:', error);
    }

    console.error('🔴 [AUTH SERVICE] Mensaje de error final:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}