import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * DTO para crear o actualizar una semana
 */
export interface SemanaDto {
  fechaInicio: string; // ISO string
  fechaFin: string;
}

/**
 * DTO para crear o actualizar una temporada
 */
export interface TemporadaDto {
  nombre: string;
  fechaInicio: string; // ISO string
  fechaCierre: string;
  actual: boolean;
  semanas?: SemanaDto[];
}

/**
 * DTO de respuesta de temporada
 */
export interface TemporadaResponseDto extends TemporadaDto {
  id: number;
  fechaCreacion: string;
}

/**
 * Interfaz para respuestas de error del servidor
 */
export interface ErrorResponse {
  mensaje: string;
  errores?: string[];
}

/**
 * Servicio para gestionar temporadas y semanas
 */
@Injectable({
  providedIn: 'root'
})
export class TemporadaService {
  private baseUrl = 'http://localhost:5000/api/temporada';

  constructor(private http: HttpClient) { }

  /**
   * Obtiene todas las temporadas
   */
  obtenerTemporadas(): Observable<TemporadaResponseDto[]> {
    return this.http.get<TemporadaResponseDto[]>(this.baseUrl)
      .pipe(catchError(this.handleError));
  }

  /**
   * Obtiene una temporada por ID
   * @param id ID de la temporada
   */
  obtenerTemporada(id: number): Observable<TemporadaResponseDto> {
    return this.http.get<TemporadaResponseDto>(`${this.baseUrl}/${id}`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Crea una nueva temporada
   * @param temporadaDto Datos de la temporada
   */
  crearTemporada(temporadaDto: TemporadaDto): Observable<TemporadaResponseDto> {
    return this.http.post<TemporadaResponseDto>(this.baseUrl, temporadaDto)
      .pipe(catchError(this.handleError));
  }

  /**
   * Actualiza una temporada existente
   * @param id ID de la temporada
   * @param temporadaDto Datos a actualizar
   */
  actualizarTemporada(id: number, temporadaDto: TemporadaDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, temporadaDto)
      .pipe(catchError(this.handleError));
  }

  /**
   * Marca una temporada como actual
   * @param id ID de la temporada
   */
  marcarActual(id: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/actual`, {})
      .pipe(catchError(this.handleError));
  }

  /**
   * Maneja errores HTTP
   * @param error Error HTTP recibido
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Error desconocido';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      if (error.error && error.error.mensaje) {
        errorMessage = error.error.mensaje;
      } else {
        errorMessage = `Código de error: ${error.status}\nMensaje: ${error.message}`;
      }
    }

    console.error('Error en TemporadaService:', errorMessage);
    return throwError(() => error);
  }
}
