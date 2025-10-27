import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap, map } from 'rxjs/operators';

/**
 * Interfaz para crear una liga
 */
export interface CreateLeagueRequest {
  nombreLiga: string;
  descripcion?: string;
  password: string;
  cantidadEquipos: number;
  tipoPlayoffs: number;
  usuarioId: number;
  nombreEquipo: string;
}

/**
 * Alias para mantener compatibilidad
 */
export interface CrearLiga extends CreateLeagueRequest {}

/**
 * Interfaz DTO para crear liga (compatible con backend)
 */
export interface CrearLigaDto {
  nombreLiga: string;
  descripcion?: string;
  password: string;
  cantidadEquipos: number;
  tipoPlayoffs: number;
  usuarioId: number;
  nombreEquipo: string;
}

/**
 * Interfaz para la respuesta de liga
 */
export interface LeagueResponse {
  idLiga: number;
  nombre_liga: string;
  descripcion?: string;
  imagenUrl?: string;
  temporada: string;
  estado: string;
  cupos_totales: number;
  cupos_ocupados: number;
  cupos_disponibles: number;
  fecha_creacion: Date;
  id_creador: number;
  nombre_creador: string;
  tipo_playoffs: number;
  permitir_decimales: boolean;
  esquema_posiciones: string;
  esquema_puntuacion: string;
  mi_equipo?: EquipoEnLiga;
}

/**
 * Interfaz para equipo dentro de una liga
 */
export interface EquipoEnLiga {
  id: number;
  nombre: string;
  alias?: string;
  esComisionado: boolean;
  fechaIncorporacion: Date;
}

/**
 * Interfaz para unirse a una liga
 */
export interface JoinLeagueRequest {
  ligaId: number;
  usuarioId: number;
  equipoId?: number;  // Opcional: si quiere usar un equipo existente
  nombreEquipo?: string;  // Opcional: si quiere crear un nuevo equipo
  alias?: string;  // Opcional: alias del usuario en la liga
  password: string;
}

/**
 * Servicio para gestionar ligas de fantasy
 */
@Injectable({
  providedIn: 'root'
})
export class LeagueService {
  private baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  /**
   * Obtiene todas las ligas disponibles
   */
  getAllLeagues(): Observable<LeagueResponse[]> {
    return this.http.get<LeagueResponse[]>(`${this.baseUrl}/Liga`)
      .pipe(
        tap(ligas => console.log('✅ [LIGA SERVICE] Ligas obtenidas:', ligas)),
        catchError(this.handleError)
      );
  }

  /**
   * Obtiene una liga por su ID
   */
  getLeagueById(id: number): Observable<LeagueResponse> {
    return this.http.get<LeagueResponse>(`${this.baseUrl}/Liga/${id}`)
      .pipe(
        tap(liga => console.log('✅ [LIGA SERVICE] Liga obtenida:', liga)),
        catchError(this.handleError)
      );
  }

  /**
   * Crea una nueva liga
   */
  createLeague(request: CreateLeagueRequest): Observable<LeagueResponse> {
    console.log('🚀 [LIGA SERVICE] Creando liga:', request);
    return this.http.post<LeagueResponse>(`${this.baseUrl}/Liga`, request)
      .pipe(
        tap(response => console.log('✅ [LIGA SERVICE] Liga creada:', response)),
        catchError(this.handleError)
      );
  }

  /**
   * Crea una nueva liga (alias para compatibilidad)
   */
  crearLiga(request: CrearLigaDto): Observable<LeagueResponse> {
    return this.createLeague(request);
  }

  /**
   * Permite a un usuario unirse a una liga existente
   * IMPORTANTE: Retorna directamente LeagueResponse del backend
   */
  joinLeague(request: JoinLeagueRequest): Observable<LeagueResponse> {
    console.log('🚀 [LIGA SERVICE] Uniéndose a liga:', {
      ligaId: request.ligaId,
      usuarioId: request.usuarioId,
      equipoId: request.equipoId,
      nombreEquipo: request.nombreEquipo,
      alias: request.alias
    });

    return this.http.post<LeagueResponse>(`${this.baseUrl}/Liga/unirse`, request)
      .pipe(
        tap(response => console.log('✅ [LIGA SERVICE] Unido a liga exitosamente:', response)),
        catchError((error) => {
          console.error('❌ [LIGA SERVICE] Error al unirse a liga:', error);
          return throwError(() => error);
        })
      );
  }

  /**
   * Verifica si un usuario ya está en una liga
   * Retorna true si el usuario ya está en la liga, false en caso contrario
   */
  checkUserInLeague(ligaId: number, usuarioId: number): Observable<boolean> {
    console.log('🔍 [LIGA SERVICE] Verificando si usuario está en liga:', { ligaId, usuarioId });
    
    return this.http.get<any[]>(`${this.baseUrl}/Equipo/liga/${ligaId}`)
      .pipe(
        map(equipos => {
          const yaEnLiga = equipos.some(e => e.usuarioId === usuarioId);
          console.log('✅ [LIGA SERVICE] Usuario en liga:', yaEnLiga);
          return yaEnLiga;
        }),
        catchError((error) => {
          console.error('❌ [LIGA SERVICE] Error al verificar usuario en liga:', error);
          // Si hay error, asumimos que no está en la liga
          return throwError(() => error);
        })
      );
  }

  /**
   * Obtiene los equipos disponibles de un usuario (equipos sin liga)
   */
  getAvailableTeams(usuarioId: number): Observable<any[]> {
    console.log('🚀 [LIGA SERVICE] Obteniendo equipos disponibles para usuario:', usuarioId);
    
    return this.http.get<any[]>(`${this.baseUrl}/Equipo/usuario/${usuarioId}/disponibles`)
      .pipe(
        tap(equipos => console.log('✅ [LIGA SERVICE] Equipos disponibles:', equipos)),
        catchError(this.handleError)
      );
  }

  /**
   * Maneja los errores HTTP
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Error desconocido';
    
    console.error('🔴 [LIGA SERVICE] Error HTTP:', {
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
      } else if (error.status === 400) {
        errorMessage = error.error?.mensaje || 'Datos inválidos';
      } else if (error.status === 401) {
        errorMessage = error.error?.mensaje || 'Contraseña incorrecta';
      } else if (error.status === 404) {
        errorMessage = error.error?.mensaje || 'Recurso no encontrado';
      } else if (error.status === 409) {
        errorMessage = error.error?.mensaje || 'Conflicto con los datos existentes';
      } else if (error.error?.mensaje) {
        errorMessage = error.error.mensaje;
      } else {
        errorMessage = `Error ${error.status}: ${error.message}`;
      }
    }
    
    console.error('🔴 [LIGA SERVICE] Mensaje de error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}