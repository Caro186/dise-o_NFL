import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

/**
 * Interfaz para crear un equipo
 */
export interface EquipoCreateDto {
  nombre: string;
  usuarioId: number;
  ligaId?: number;
  alias?: string;
}

/**
 * Interfaz para la respuesta de equipo
 */
export interface EquipoResponseDto {
  id: number;
  nombre: string;
  alias?: string;
  imagenUrl: string | null;
  fechaCreacion: Date;
  usuarioId: number;
  nombreUsuario?: string;
  nombrePropietario?: string; // Alias de nombreUsuario para compatibilidad
  estado: string;
  ligaId?: number | null;
  nombreLiga?: string;
  liga?: string; // Alias de nombreLiga para compatibilidad
  esComisionado: boolean;
  fechaIncorporacion: Date;
}

/**
 * Interfaz para respuestas de error del servidor
 */
export interface ErrorResponse {
  mensaje: string;
  errores?: string[];
}

/**
 * Servicio para gestionar equipos de fantasy
 */
@Injectable({
  providedIn: 'root'
})
export class EquipoService {
  private baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  /**
   * Crea un nuevo equipo
   */
  crearEquipo(equipoDto: EquipoCreateDto): Observable<EquipoResponseDto> {
    console.log('🚀 [EQUIPO SERVICE] Creando equipo:', equipoDto);
    return this.http.post<EquipoResponseDto>(`${this.baseUrl}/Equipo`, equipoDto)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipo creado:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene un equipo por su ID
   */
  obtenerEquipo(id: number): Observable<EquipoResponseDto> {
    return this.http.get<EquipoResponseDto>(`${this.baseUrl}/Equipo/${id}`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipo obtenido:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene todos los equipos de un usuario
   */
  obtenerEquiposPorUsuario(usuarioId: number): Observable<EquipoResponseDto[]> {
    console.log('🚀 [EQUIPO SERVICE] Obteniendo equipos del usuario:', usuarioId);
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo/usuario/${usuarioId}`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipos obtenidos:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene los equipos disponibles de un usuario (sin liga asignada)
   */
  obtenerEquiposDisponibles(usuarioId: number): Observable<EquipoResponseDto[]> {
    console.log('🚀 [EQUIPO SERVICE] Obteniendo equipos disponibles del usuario:', usuarioId);
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo/usuario/${usuarioId}/disponibles`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipos disponibles:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene todos los equipos de una liga
   */
  obtenerEquiposPorLiga(ligaId: number): Observable<EquipoResponseDto[]> {
    console.log('🚀 [EQUIPO SERVICE] Obteniendo equipos de la liga:', ligaId);
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo/liga/${ligaId}`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipos de liga obtenidos:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene todos los equipos
   */
  obtenerTodosLosEquipos(): Observable<EquipoResponseDto[]> {
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Todos los equipos obtenidos:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Sube la imagen de un equipo
   */
  subirImagen(equipoId: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);

    console.log('🚀 [EQUIPO SERVICE] Subiendo imagen para equipo:', equipoId);
    return this.http.post(`${this.baseUrl}/Equipo/${equipoId}/imagen`, formData)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Imagen subida:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Elimina un equipo
   */
  eliminarEquipo(id: number): Observable<any> {
    console.log('🚀 [EQUIPO SERVICE] Eliminando equipo:', id);
    return this.http.delete(`${this.baseUrl}/Equipo/${id}`)
      .pipe(
        tap(response => console.log('✅ [EQUIPO SERVICE] Equipo eliminado:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Maneja los errores HTTP
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Error desconocido';
    
    console.error('🔴 [EQUIPO SERVICE] Error HTTP:', {
      status: error.status,
      statusText: error.statusText,
      message: error.message,
      error: error.error
    });
    
    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      if (error.status === 0) {
        errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
      } else if (error.status === 400 && error.error?.mensaje) {
        errorMessage = error.error.mensaje;
      } else if (error.status === 404) {
        errorMessage = error.error?.mensaje || 'Recurso no encontrado';
      } else if (error.error?.mensaje) {
        errorMessage = error.error.mensaje;
      } else {
        errorMessage = `Código de error: ${error.status}\nMensaje: ${error.message}`;
      }
    }
    
    console.error('🔴 [EQUIPO SERVICE] Mensaje de error:', errorMessage);
    return throwError(() => error);
  }
}