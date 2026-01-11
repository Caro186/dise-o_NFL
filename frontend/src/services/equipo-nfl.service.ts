import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * DTO para crear equipo NFL
 */
export interface EquipoNFLCreateDto {
  nombre: string;
  ciudad: string;
}

/**
 * DTO de respuesta de equipo NFL
 */
export interface EquipoNFLResponseDto {
  id: number;
  nombre: string;
  ciudad: string;
  imagenUrl?: string;
  fechaCreacion: Date;
  estado: string;
}

/**
 * Servicio para gestionar equipos NFL (reales)
 */
@Injectable({
  providedIn: 'root'
})
export class EquipoNFLService {
  private baseUrl = 'http://localhost:5000/api/EquipoNFL';

  constructor(private http: HttpClient) { }

  /**
   * Obtiene todos los equipos NFL
   */
  obtenerTodos(): Observable<EquipoNFLResponseDto[]> {
    return this.http.get<EquipoNFLResponseDto[]>(this.baseUrl);
  }

  /**
   * Obtiene un equipo NFL por ID
   */
  obtenerPorId(id: number): Observable<EquipoNFLResponseDto> {
    return this.http.get<EquipoNFLResponseDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * Crea un nuevo equipo NFL
   */
  crear(equipo: EquipoNFLCreateDto): Observable<EquipoNFLResponseDto> {
    return this.http.post<EquipoNFLResponseDto>(this.baseUrl, equipo);
  }

  /**
   * Sube la imagen de un equipo NFL
   */
  subirImagen(id: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);
    return this.http.post(`${this.baseUrl}/${id}/imagen`, formData);
  }

  /**
   * Elimina un equipo NFL
   */
  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}