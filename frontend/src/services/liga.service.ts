import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * DTO para crear una liga
 */
export interface LigaCreateDto {
  nombreLiga: string;
  descripcion?: string;
  passwordHash: string;
  idTemporada: number;
  cuposTotales: number;
  comisionadoId: number; 
  formatoPosiciones: string;
  esquemaPuntos: string;
  configPlayoffs: string;
  permitirDecimales?: boolean;
  equipoFantasyId?: number; 

}

/**
 * DTO de respuesta de liga
 */
export interface LigaResponseDto {
  idLiga: number;
  imagenUrl?: string;
  nombreLiga: string;
  descripcion?: string;
  idTemporada: number;
  nombreTemporada?: string;
  estado: string;
  cuposTotales: number;
  cuposOcupados: number;
  fechaCreacion: Date;
  fechaInicio?: Date;
  fechaFin?: Date;
  comisionadoId: number;
  nombreComisionado?: string;
  formatoPosiciones: string;
  esquemaPuntos: string;
  configPlayoffs: string;
  permitirDecimales: boolean;
}


/**
 * DTO para unirse a una liga
 */
export interface UnirseLigaDto {
  ligaId: number;
  password: string;
  usuarioId: number;
  equipoId: number;
  alias: string;
}



/**
 * Servicio para gestionar ligas
 */
@Injectable({
  providedIn: 'root'
})
export class LigaService {
  private baseUrl = 'http://localhost:5000/api/Liga';

  constructor(private http: HttpClient) { }

  /**
   * Obtiene todas las ligas
   */
  obtenerTodas(): Observable<LigaResponseDto[]> {
    return this.http.get<LigaResponseDto[]>(this.baseUrl);
  }

  /**
   * Obtiene una liga por ID
   */
  obtenerPorId(id: number): Observable<LigaResponseDto> {
    return this.http.get<LigaResponseDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * Obtiene todas las ligas donde un usuario es comisionado
   */
  obtenerLigasPorComisionado(usuarioId: number): Observable<LigaResponseDto[]> {
    return this.http.get<LigaResponseDto[]>(`${this.baseUrl}/comisionado/${usuarioId}`);
  }

  /**
   * Crea una nueva liga
   */
  crear(liga: LigaCreateDto): Observable<LigaResponseDto> {
    return this.http.post<LigaResponseDto>(this.baseUrl, liga);
  }

  /**
   * Actualiza una liga existente
   */
  actualizar(id: number, liga: Partial<LigaCreateDto>): Observable<LigaResponseDto> {
    return this.http.put<LigaResponseDto>(`${this.baseUrl}/${id}`, liga);
  }

  /**
   * Elimina una liga
   */
  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  /**
   * Sube la imagen de una liga
   */
  subirImagen(id: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);
    return this.http.post(`${this.baseUrl}/${id}/imagen`, formData);
  }


 /**
 * Unirse a una liga existente
 */
unirseALiga(data: UnirseLigaDto): Observable<any> {
  return this.http.post(`${this.baseUrl}/unirse`, data);
}


/**
 * Obtiene todas las ligas donde un usuario participa
 */
obtenerPorUsuario(usuarioId: number): Observable<LigaResponseDto[]> {
  return this.http.get<LigaResponseDto[]>(`${this.baseUrl}/usuario/${usuarioId}`);
}
}