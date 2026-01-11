import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * DTO para crear equipo Fantasy
 */
export interface EquipoFantasyCreateDto {
  nombre: string;
  usuarioId: number;
  ligaId?: number;
}

/**
 * DTO de respuesta de equipo Fantasy
 */
export interface EquipoFantasyResponseDto {
  id: number;
  nombre: string;
  usuarioId: number;
  nombrePropietario?: string;
  ligaId?: number;
  nombreLiga?: string;
  imagenUrl?: string;
  fechaCreacion: Date;
  estado: string;
}

/**
 * Servicio para gestionar equipos Fantasy (de usuarios)
 */
@Injectable({
  providedIn: 'root'
})
export class EquipoFantasyService {
  private baseUrl = 'http://localhost:5000/api/EquipoFantasy';

  constructor(private http: HttpClient) { }

  /**
   * Obtiene todos los equipos Fantasy
   */
  obtenerTodos(): Observable<EquipoFantasyResponseDto[]> {
    return this.http.get<EquipoFantasyResponseDto[]>(this.baseUrl);
  }

  /**
   * Obtiene un equipo Fantasy por ID
   */
  obtenerPorId(id: number): Observable<EquipoFantasyResponseDto> {
    return this.http.get<EquipoFantasyResponseDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * Obtiene todos los equipos Fantasy de un usuario
   */
  obtenerPorUsuario(usuarioId: number): Observable<EquipoFantasyResponseDto[]> {
    return this.http.get<EquipoFantasyResponseDto[]>(`${this.baseUrl}/usuario/${usuarioId}`);
  }

  /**
   * Crea un nuevo equipo Fantasy
   */
  crear(equipo: EquipoFantasyCreateDto): Observable<EquipoFantasyResponseDto> {
    return this.http.post<EquipoFantasyResponseDto>(this.baseUrl, equipo);
  }

  /**
   * Sube la imagen de un equipo Fantasy
   */
  subirImagen(id: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);
    return this.http.post(`${this.baseUrl}/${id}/imagen`, formData);
  }

  /**
   * Elimina un equipo Fantasy
   */
  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}