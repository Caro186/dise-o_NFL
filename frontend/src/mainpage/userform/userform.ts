import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-userform',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './userform.html',
  styleUrls: ['./userform.css']
})
export class Userform {
  imagenPreview: string | null = null;
  selectedFile: File | null = null;
  nombreEquipo: string = '';

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

    // Validar dimensiones de la imagen (entre 300x300 y 1024x1024)
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>): void => {
      if (e.target?.result) {
        const img = new Image();
        img.onload = () => {
          const width = img.width;
          const height = img.height;

          // Verificar que las dimensiones estén entre 300x300 y 1024x1024
          if (width < 300 || height < 300) {
            alert('La imagen debe tener dimensiones mínimas de 300x300 píxeles.');
            input.value = '';
            this.imagenPreview = null;
            this.selectedFile = null;
            return;
          }

          if (width > 1024 || height > 1024) {
            alert('La imagen debe tener dimensiones máximas de 1024x1024 píxeles.');
            input.value = '';
            this.imagenPreview = null;
            this.selectedFile = null;
            return;
          }

          // Verificar que sea cuadrada
          if (width !== height) {
            alert('La imagen debe ser cuadrada (ancho = alto).');
            input.value = '';
            this.imagenPreview = null;
            this.selectedFile = null;
            return;
          }

          // Si pasa todas las validaciones, guardar
          this.selectedFile = file;
          this.imagenPreview = e.target!.result as string;
        };
        
        img.onerror = () => {
          alert('Error al cargar la imagen.');
          input.value = '';
        };

        img.src = e.target.result as string;
      }
    };
    reader.readAsDataURL(file);
  }

  onSubmit(): void {
    // Validar nombre del equipo
    const nombre = this.nombreEquipo.trim();
    
    if (!nombre) {
      alert('Por favor ingresa el nombre del equipo.');
      return;
    }

    if (nombre.length < 1 || nombre.length > 100) {
      alert('El nombre del equipo debe tener entre 1 y 100 caracteres.');
      return;
    }

    if (!this.selectedFile) {
      alert('Por favor selecciona una imagen para el equipo.');
      return;
    }

    console.log('Nombre del equipo:', nombre);
    console.log('Archivo seleccionado:', this.selectedFile);
    console.log('Preview:', this.imagenPreview);
    
    alert('Guardando equipo...');
    
  }
}