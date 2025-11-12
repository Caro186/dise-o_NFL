import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { 
  JugadorService, 
  JugadorBatchResultDto,
  JugadorBatchErrorDto,
  JugadorCreatedDto 
} from '../../services/jugadores.service';

/**
 * Componente para carga masiva de jugadores NFL desde archivo JSON
 */
@Component({
  selector: 'app-jugador-batch-upload',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './jugador-batch-upload.html',
  styleUrls: ['./jugador-batch-upload.css']
})
export class JugadorBatchUpload {
  archivoSeleccionado: File | null = null;
  nombreArchivo: string = 'Ningún archivo seleccionado';
  isProcessing: boolean = false;
  resultado: JugadorBatchResultDto | null = null;
  errorMessage: string = '';
  mostrarDetallesErrores: boolean = false;

  constructor(
    private jugadorService: JugadorService,
    private router: Router
  ) {}

  /**
   * Maneja la selección de archivo
   */
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    
    if (file) {
      // Validar que sea JSON
      if (!file.name.toLowerCase().endsWith('.json')) {
        this.errorMessage = 'Por favor, selecciona un archivo JSON válido';
        this.archivoSeleccionado = null;
        this.nombreArchivo = 'Ningún archivo seleccionado';
        return;
      }

      // Validar tamaño (máximo 10 MB)
      const maxSize = 10 * 1024 * 1024; // 10 MB
      if (file.size > maxSize) {
        this.errorMessage = 'El archivo excede el tamaño máximo permitido (10 MB)';
        this.archivoSeleccionado = null;
        this.nombreArchivo = 'Ningún archivo seleccionado';
        return;
      }

      this.archivoSeleccionado = file;
      this.nombreArchivo = file.name;
      this.errorMessage = '';
      this.resultado = null;
    }
  }

  /**
   * Procesa el archivo y envía al backend
   */
  procesarArchivo(): void {
    if (!this.archivoSeleccionado) {
      this.errorMessage = 'Por favor, selecciona un archivo primero';
      return;
    }

    this.isProcessing = true;
    this.errorMessage = '';
    this.resultado = null;

    this.jugadorService.crearJugadoresBatch(this.archivoSeleccionado).subscribe({
      next: (response) => {
        console.log('Respuesta del servidor:', response);
        this.resultado = response;
        this.isProcessing = false;

        // Si fue exitoso, limpiar el archivo seleccionado
        if (response.exito) {
          this.limpiarFormulario();
        }
      },
      error: (error) => {
        console.error('Error al procesar batch:', error);
        this.isProcessing = false;
        
        if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else if (error.error && typeof error.error === 'object') {
          // El servidor devolvió un resultado con errores
          this.resultado = error.error;
        } else {
          this.errorMessage = error.message || 'Error al procesar el archivo. Inténtalo de nuevo.';
        }
      }
    });
  }

  /**
   * Limpia el formulario y resetea el estado
   */
  limpiarFormulario(): void {
    this.archivoSeleccionado = null;
    this.nombreArchivo = 'Ningún archivo seleccionado';
    this.errorMessage = '';
    
    // Resetear el input de archivo
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  /**
   * Alterna la visibilidad de los detalles de errores
   */
  toggleDetallesErrores(): void {
    this.mostrarDetallesErrores = !this.mostrarDetallesErrores;
  }

  /**
   * Descarga un archivo JSON de ejemplo
   */
  descargarEjemplo(): void {
    const ejemplo = {
      jugadores: [
        {
          id: 1,
          nombre: "Patrick Mahomes",
          posicion: "QB",
          equipoNFLId: 1,
          imagenUrl: "https://example.com/mahomes.jpg"
        },
        {
          id: 2,
          nombre: "Travis Kelce",
          posicion: "TE",
          equipoNFLId: 1,
          imagenUrl: "https://example.com/kelce.jpg"
        },
        {
          id: 3,
          nombre: "Christian McCaffrey",
          posicion: "RB",
          equipoNFLId: 1,
          imagenUrl: "https://example.com/mccaffrey.jpg"

        }
      ]
    };

    const blob = new Blob([JSON.stringify(ejemplo, null, 2)], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'ejemplo-jugadores.json';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Navega a la lista de jugadores
   */
  verJugadores(): void {
    this.router.navigate(['/mainpage/jugadores']);
  }
}