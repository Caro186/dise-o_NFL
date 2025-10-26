import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface Liga {
  id_liga: number;
  nombre_liga: string;
  descripcion: string;
  temporada: string;
  estado: 'activa' | 'inactiva' | 'finalizada';
  cupos_totales: number;
  cupos_ocupados: number;
  fecha_creacion: string;
  fecha_inicio: string;
  fecha_fin: string;
}

export interface JoinLeagueRequest {
  id_liga: number;
  id_usuario: number;
  password: string;
  alias: string;
  nombre_equipo: string;
}

export interface JoinLeagueResponse {
  success: boolean;
  message: string;
  id_participante?: number;
}

@Injectable({
  providedIn: 'root'
})
export class LeagueService {
  private apiUrl = 'http://localhost:3000/api'; // Ajusta según tu configuración

  constructor(private http: HttpClient) {}

  // Obtener información de una liga específica
  getLeagueById(id: number): Observable<Liga> {
    return this.http.get<Liga>(`${this.apiUrl}/ligas/${id}`)
      .pipe(catchError(this.handleError));
  }

  // Buscar ligas por nombre, temporada y estado
  searchLeagues(nombre?: string, temporada?: string, estado?: string): Observable<Liga[]> {
    let params: any = {};
    if (nombre) params.nombre = nombre;
    if (temporada) params.temporada = temporada;
    if (estado) params.estado = estado;

    return this.http.get<Liga[]>(`${this.apiUrl}/ligas/search`, { params })
      .pipe(catchError(this.handleError));
  }

  // Unirse a una liga
  joinLeague(request: JoinLeagueRequest): Observable<JoinLeagueResponse> {
    return this.http.post<JoinLeagueResponse>(`${this.apiUrl}/ligas/join`, request)
      .pipe(catchError(this.handleError));
  }

  // Verificar si el usuario ya está en la liga
  checkUserInLeague(idLiga: number, idUsuario: number): Observable<boolean> {
    return this.http.get<{exists: boolean}>(`${this.apiUrl}/ligas/${idLiga}/users/${idUsuario}`)
      .pipe(
        map(response => response.exists),
        catchError(this.handleError)
      );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Ocurrió un error inesperado';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = error.error?.message || errorMessage;
    }
    
    return throwError(() => new Error(errorMessage));
  }
}