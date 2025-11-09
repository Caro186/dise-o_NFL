import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EquipoNFLService, EquipoNFLCreateDto } from '../../services/equipo-nfl.service';

/**
 * Componente para crear equipos NFL (solo administradores)
 */
@Component({
  selector: 'app-equipos-nfl-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './equipos-nfl-form.component.html',
  styleUrls: ['./equipos-nfl-form.component.css']
})
export class EquiposNFLFormComponent {
  nombre: string = '';
  ciudad: string = '';
  imagenPreview: string | null = null;
  selectedFile: File | null = null;
  errorMessage: string = '';
  isLoading: boolean = false;
  equipoIdCreado: number | null = null;

  constructor(
    private equipoNFLService: EquipoNFLService,
    private router: Router
  ) { }

  /**
   * Maneja la selección de archivo de imagen
   */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      this.imagenPreview = null;
      this.selectedFile = null;
      return;
    }

    const file = input.files[0];

    // Validación de tipo
    const validTypes = ['image/jpeg', 'image/png'];
    if (!validTypes.includes(file.type)) {
      alert('Solo se permiten imágenes JPEG o PNG.');
      input.value = '';
      return;
    }

    // Validación de tamaño (máximo 5 MB)
    if (file.size > 5 * 1024 * 1024) {
      alert('El tamaño máximo permitido es 5 MB.');
      input.value = '';
      return;
    }

    // Guardar el archivo y mostrar preview
    this.selectedFile = file;
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      if (e.target?.result) {
        this.imagenPreview = e.target.result as string;
      }
    };
    reader.readAsDataURL(file);
  }

  /**
   * Envía el formulario para crear un equipo NFL
   */
  onSubmit(): void {
    this.errorMessage = '';

    // Validaciones
    if (!this.nombre.trim()) {
      this.errorMessage = 'El nombre del equipo es obligatorio';
      return;
    }

    if (!this.ciudad.trim()) {
      this.errorMessage = 'La ciudad es obligatoria';
      return;
    }

    this.isLoading = true;

    const equipoDto: EquipoNFLCreateDto = {
      nombre: this.nombre.trim(),
      ciudad: this.ciudad.trim()
    };

    // Crear el equipo
    this.equipoNFLService.crear(equipoDto).subscribe({
      next: (response) => {
        console.log('Equipo NFL creado:', response);
        this.equipoIdCreado = response.id;

        // Si hay imagen, subirla
        if (this.selectedFile) {
          this.subirImagen(response.id);
        } else {
          this.isLoading = false;
          alert('Equipo NFL creado exitosamente');
          this.router.navigate(['/admin/equipos-nfl']);
        }
      },
      error: (error) => {
        console.error('Error al crear equipo NFL:', error);
        this.isLoading = false;
        this.errorMessage = error.error?.mensaje || 'Error al crear el equipo NFL';
      }
    });
  }

  /**
   * Sube la imagen del equipo
   */
  private subirImagen(equipoId: number): void {
    if (!this.selectedFile) return;

    this.equipoNFLService.subirImagen(equipoId, this.selectedFile).subscribe({
      next: () => {
        this.isLoading = false;
        alert('Equipo NFL creado exitosamente con imagen');
        this.router.navigate(['/admin/equipos-nfl']);
      },
      error: (error) => {
        console.error('Error al subir imagen:', error);
        this.isLoading = false;
        alert('Equipo creado, pero hubo un error al subir la imagen');
        this.router.navigate(['/admin/equipos-nfl']);
      }
    });
  }
}