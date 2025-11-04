import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CrearLigaRequest {
  nombreLiga: string;
  descripcion?: string;
  password: string;
  cantidadEquipos: number;
  idComisionado: number;
  nombreEquipoComisionado: string;
  equiposEnPlayoffs: number;
}

export interface LigaResponse {
  idLiga: number;
  nombreLiga: string;
  descripcion?: string;
  idTemporada: number;
  estado: string;
  cuposTotales: number;
  cuposOcupados: number;
  cuposDisponibles: number;
  fechaCreacion: Date;
  fechaInicio?: Date;
  fechaFin?: Date;
  idComisionado: number;
  nombreComisionado: string;
  idEquipoComisionado: number;
  nombreEquipoComisionado: string;
}

export interface UnirseALigaRequest {
  idLiga: number;
  idUsuario: number;
  password: string;
  alias: string;
  nombreEquipo: string;
}

export interface UnirseALigaResponse {
  mensaje: string;
  idLiga: number;
  nombreLiga: string;
  idEquipo: number;
  nombreEquipo: string;
  cuposDisponibles: number;
}

@Injectable({
  providedIn: 'root'
})
export class LigaService {
  private baseUrl = 'http://localhost:5000/api/Liga';

  constructor(private http: HttpClient) {}

  crearLiga(ligaData: CrearLigaRequest): Observable<LigaResponse> {
    return this.http.post<LigaResponse>(this.baseUrl, ligaData);
  }

  obtenerLiga(id: number): Observable<LigaResponse> {
    return this.http.get<LigaResponse>(`${this.baseUrl}/${id}`);
  }

  obtenerTodasLasLigas(): Observable<LigaResponse[]> {
    return this.http.get<LigaResponse[]>(this.baseUrl);
  }

  unirseALiga(data: UnirseALigaRequest): Observable<UnirseALigaResponse> {
    return this.http.post<UnirseALigaResponse>(`${this.baseUrl}/unirse`, data);
  }
}