import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

export interface CrearJugadorDto {
  nombre: string;
  posicion: string;
  equipoNFLId: number;
}

export interface ActualizarJugadorDto {
  nombre?: string;
  posicion?: string;
  equipoNFLId?: number;
  estado?: string;
}

export interface JugadorResponseDto {
  id: number;
  nombre: string;
  posicion: string;
  equipoNFLId: number;
  nombreEquipoNFL: string;
  ciudadEquipoNFL: string;
  imagenUrl?: string;
  thumbnailUrl?: string;
  estado: string;
  fechaCreacion: Date;
  fechaActualizacion?: Date;
}

export interface JugadorListDto {
  id: number;
  nombre: string;
  posicion: string;
  nombreEquipoNFL: string;
  thumbnailUrl?: string;
  estado: string;
}

export interface EquipoNFL {
  id: number;
  nombre: string;
  ciudad: string;
  imagenUrl?: string;
  estado: string;
}



// ============ INTERFACES PARA BATCH ============
export interface JugadorBatchItemDto {
  id: number;
  nombre: string;
  posicion: string;
  equipoNFLId: number;
  imagenUrl?: string;
}

export interface JugadorBatchRequestDto {
  jugadores: JugadorBatchItemDto[];
}

export interface JugadorBatchResultDto {
  exito: boolean;
  mensaje: string;
  totalProcesados: number;
  totalExitosos: number;
  totalErrores: number;
  errores: JugadorBatchErrorDto[];
  jugadoresCreados: JugadorCreatedDto[];
  archivoMovidoA: string;
}

export interface JugadorBatchErrorDto {
  id?: number;
  nombre?: string;
  error: string;
}

export interface JugadorCreatedDto {
  id: number;
  nombre: string;
  posicion: string;
  nombreEquipoNFL: string;
}




@Injectable({
  providedIn: 'root'
})
export class JugadorService {
  private apiUrl = 'http://localhost:5000/api/Jugador';
  private equiposNFLUrl = 'http://localhost:5000/api/EquipoNFL';

  constructor(private http: HttpClient) { }

  /**
   * Obtener todos los jugadores
   */
  obtenerJugadores(): Observable<JugadorListDto[]> {
    return this.http.get<JugadorListDto[]>(this.apiUrl).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Obtener un jugador por ID
   */
  obtenerJugadorPorId(id: number): Observable<JugadorResponseDto> {
    return this.http.get<JugadorResponseDto>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Obtener jugadores por equipo NFL
   */
  obtenerJugadoresPorEquipo(equipoNFLId: number): Observable<JugadorListDto[]> {
    return this.http.get<JugadorListDto[]>(`${this.apiUrl}/Equipo/${equipoNFLId}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Obtener jugadores por posición
   */
  obtenerJugadoresPorPosicion(posicion: string): Observable<JugadorListDto[]> {
    return this.http.get<JugadorListDto[]>(`${this.apiUrl}/Posicion/${posicion}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Crear un nuevo jugador
   */
  crearJugador(jugador: CrearJugadorDto): Observable<JugadorResponseDto> {
    return this.http.post<JugadorResponseDto>(this.apiUrl, jugador).pipe(
      tap(response => console.log('Jugador creado:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Subir imagen principal del jugador
   */
  subirImagen(id: number, imagen: File): Observable<any> {
    const formData = new FormData();
    formData.append('imagen', imagen);
    
    return this.http.post(`${this.apiUrl}/${id}/imagen`, formData).pipe(
      tap(response => console.log('Imagen subida:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Subir thumbnail del jugador
   */
  subirThumbnail(id: number, thumbnail: File): Observable<any> {
    const formData = new FormData();
    formData.append('thumbnail', thumbnail);
    
    return this.http.post(`${this.apiUrl}/${id}/thumbnail`, formData).pipe(
      tap(response => console.log('Thumbnail subido:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Actualizar un jugador existente
   */
  actualizarJugador(id: number, jugador: ActualizarJugadorDto): Observable<JugadorResponseDto> {
    return this.http.put<JugadorResponseDto>(`${this.apiUrl}/${id}`, jugador).pipe(
      tap(response => console.log('Jugador actualizado:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Desactivar un jugador (soft delete)
   */
  desactivarJugador(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(response => console.log('Jugador desactivado:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Eliminar un jugador permanentemente
   */
  eliminarJugadorPermanente(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}/permanente`).pipe(
      tap(response => console.log('Jugador eliminado permanentemente:', response)),
      catchError(this.handleError)
    );
  }

  /**
   * Obtener todos los equipos NFL (para el selector)
   */
  obtenerEquiposNFL(): Observable<EquipoNFL[]> {
    return this.http.get<EquipoNFL[]>(this.equiposNFLUrl).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Validar que la imagen sea cuadrada
   */
  validarImagenCuadrada(file: File): Promise<{ valida: boolean; ancho: number; alto: number }> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const reader = new FileReader();

      reader.onload = (e: any) => {
        img.src = e.target.result;
      };

      img.onload = () => {
        const esValida = img.width === img.height;
        resolve({
          valida: esValida,
          ancho: img.width,
          alto: img.height
        });
      };

      img.onerror = () => {
        reject(new Error('Error al cargar la imagen'));
      };

      reader.readAsDataURL(file);
    });
  }

  /**
   * Manejo de errores
   */
  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Ocurrió un error desconocido';
    
    if (error.error instanceof ErrorEvent) {
      // Error del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del servidor
      if (error.status === 400) {
        errorMessage = error.error.mensaje || 'Solicitud inválida';
      } else if (error.status === 404) {
        errorMessage = error.error.mensaje || 'Recurso no encontrado';
      } else if (error.status === 409) {
        errorMessage = error.error.mensaje || 'El jugador ya existe';
      } else if (error.status === 500) {
        errorMessage = 'Error interno del servidor';
      } else {
        errorMessage = `Error del servidor: ${error.status}`;
      }
    }
    
    console.error('Error completo:', error);
    return throwError(() => new Error(errorMessage));
  }

  // ============ AGREGAR ESTE MÉTODO A LA CLASE JugadorService ============
  /**
   * Crear jugadores en lote desde archivo JSON
   */
  crearJugadoresBatch(archivo: File): Observable<JugadorBatchResultDto> {
    const formData = new FormData();
    formData.append('file', archivo, archivo.name);

    return this.http.post<JugadorBatchResultDto>(`${this.apiUrl}/batch`, formData).pipe(
      tap(response => console.log('Batch procesado:', response)),
      catchError(this.handleError)
    );
  }

  
}