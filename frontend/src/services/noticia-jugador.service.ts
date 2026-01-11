import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

// ============ INTERFACES ============

export interface CrearNoticiaJugadorDto {
  jugadorId: number;
  texto: string;
  esLesion: boolean;
  resumenLesion?: string;
  designacionLesion?: string;
}

export interface NoticiaJugadorResponseDto {
  id: number;
  jugadorId: number;
  nombreJugador: string;
  equipoNFL: string;
  texto: string;
  esLesion: boolean;
  resumenLesion?: string;
  designacionLesion?: string;
  designacionDescripcion?: string;
  autorId: number;
  nombreAutor: string;
  fechaCreacion: Date;
  estado: string;
}

export interface JugadorConNoticiasDto {
  id: number;
  nombre: string;
  posicion: string;
  equipoNFL: string;
  designacionLesion?: string;
  designacionDescripcion?: string;
  imagenUrl?: string;
  noticias: NoticiaJugadorResponseDto[];
}

export interface AuditoriaNoticiaDto {
  noticiaId: number;
  autorId: number;
  nombreAutor: string;
  fechaCreacion: Date;
  accionRealizada: string;
  detallesCambio: string;
}

export interface CrearNoticiaResponse {
  mensaje: string;
  noticia: NoticiaJugadorResponseDto;
  auditoria: AuditoriaNoticiaDto;
}

// ============ SERVICIO ============

@Injectable({
  providedIn: 'root'
})
export class NoticiaJugadorService {
  private baseUrl = 'http://localhost:5000/api/NoticiaJugador';

  constructor(private http: HttpClient) { }

  /**
   * Crear una nueva noticia para un jugador (solo administradores)
   */
  crearNoticia(dto: CrearNoticiaJugadorDto): Observable<CrearNoticiaResponse> {
    return this.http.post<CrearNoticiaResponse>(`${this.baseUrl}`, dto).pipe(
      tap((response) => console.log('Noticia creada:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Obtener todas las noticias de un jugador específico
   */
  obtenerNoticiasJugador(jugadorId: number): Observable<{ noticias: NoticiaJugadorResponseDto[], total: number }> {
    return this.http.get<{ noticias: NoticiaJugadorResponseDto[], total: number }>(`${this.baseUrl}/jugador/${jugadorId}`).pipe(
      tap((response) => console.log('Noticias del jugador:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Obtener un jugador con todas sus noticias
   */
  obtenerJugadorConNoticias(jugadorId: number): Observable<JugadorConNoticiasDto> {
    return this.http.get<JugadorConNoticiasDto>(`${this.baseUrl}/jugador/${jugadorId}/completo`).pipe(
      tap((response) => console.log('Jugador con noticias:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Obtener todas las noticias del sistema
   */
  obtenerTodasNoticias(): Observable<{ noticias: NoticiaJugadorResponseDto[], total: number }> {
    return this.http.get<{ noticias: NoticiaJugadorResponseDto[], total: number }>(`${this.baseUrl}`).pipe(
      tap((response) => console.log('Todas las noticias:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Obtener una noticia específica por su ID
   */
  obtenerNoticiaPorId(id: number): Observable<NoticiaJugadorResponseDto> {
    return this.http.get<NoticiaJugadorResponseDto>(`${this.baseUrl}/${id}`).pipe(
      tap((response) => console.log('Noticia obtenida:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Manejo de errores
   */
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Ha ocurrido un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      if (error.error && error.error.mensaje) {
        errorMessage = error.error.mensaje;
      } else if (error.message) {
        errorMessage = error.message;
      } else {
        errorMessage = `Error ${error.status}: ${error.statusText}`;
      }
    }

    console.error('Error en NoticiaJugadorService:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }

  /**
   * Validar designación de lesión
   */
  esDesignacionValida(designacion: string): boolean {
    const designacionesValidas = ['O', 'D', 'Q', 'P', 'FP', 'IR', 'PUP', 'SUS'];
    return designacionesValidas.includes(designacion);
  }

  /**
   * Obtener lista de designaciones disponibles
   */
  obtenerDesignacionesDisponibles(): { valor: string, descripcion: string }[] {
    return [
      { valor: 'O', descripcion: 'Fuera (Out) - No jugará' },
      { valor: 'D', descripcion: 'Dudoso (Doubtful) - ~25% probabilidad' },
      { valor: 'Q', descripcion: 'Cuestionable (Questionable) - ~50% probabilidad' },
      { valor: 'P', descripcion: 'Probable (Probable) - Muy probable que juegue' },
      { valor: 'FP', descripcion: 'Participación Plena (Full Practice)' },
      { valor: 'IR', descripcion: 'Reserva de Lesionados (Injured Reserve)' },
      { valor: 'PUP', descripcion: 'Incapaz Físicamente (PUP)' },
      { valor: 'SUS', descripcion: 'Suspendido (Suspended)' }
    ];
  }
}
