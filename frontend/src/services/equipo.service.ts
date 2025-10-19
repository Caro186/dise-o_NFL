import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * Interfaz para crear un equipo
 */
export interface EquipoCreateDto {
  nombre: string;
  usuarioId: number;
  liga?: string;
}

/**
 * Interfaz para la respuesta de equipo
 */
export interface EquipoResponseDto {
  id: number;
  nombre: string;
  imagenUrl: string | null;
  fechaCreacion: Date;
  usuarioId: number;
  nombrePropietario?: string;
  estado: string;
  liga?: string;
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

  /**
   * Constructor del servicio de equipos
   * @param http Cliente HTTP para realizar peticiones
   */
  constructor(private http: HttpClient) { }

  /**
   * Crea un nuevo equipo
   * @param equipoDto Datos del equipo a crear
   * @returns Observable con el equipo creado
   */
  crearEquipo(equipoDto: EquipoCreateDto): Observable<EquipoResponseDto> {
    return this.http.post<EquipoResponseDto>(`${this.baseUrl}/Equipo`, equipoDto)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene un equipo por su ID
   * @param id ID del equipo
   * @returns Observable con los datos del equipo
   */
  obtenerEquipo(id: number): Observable<EquipoResponseDto> {
    return this.http.get<EquipoResponseDto>(`${this.baseUrl}/Equipo/${id}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene todos los equipos de un usuario
   * @param usuarioId ID del usuario
   * @returns Observable con la lista de equipos
   */
  obtenerEquiposPorUsuario(usuarioId: number): Observable<EquipoResponseDto[]> {
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo/usuario/${usuarioId}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene todos los equipos
   * @returns Observable con la lista de todos los equipos
   */
  obtenerTodosLosEquipos(): Observable<EquipoResponseDto[]> {
    return this.http.get<EquipoResponseDto[]>(`${this.baseUrl}/Equipo`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Sube la imagen de un equipo
   * @param equipoId ID del equipo
   * @param imagen Archivo de imagen
   * @returns Observable con la respuesta del servidor
   */
  subirImagen(equipoId: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);

    return this.http.post(`${this.baseUrl}/Equipo/${equipoId}/imagen`, formData)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Elimina un equipo
   * @param id ID del equipo a eliminar
   * @returns Observable con la respuesta del servidor
   */
  eliminarEquipo(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Equipo/${id}`)
      .pipe(
        catchError(this.handleError)
      );
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
    
    console.error('Error en EquipoService:', errorMessage);
    return throwError(() => error);
  }
}