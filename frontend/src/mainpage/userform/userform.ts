import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EquipoService } from '../../services/equipo.service';
import { Authservice } from '../../services/authservice';

/**
 * Componente para crear/editar equipos de fantasy
 */
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
  errorMessage: string = '';
  isLoading: boolean = false;

  /**
   * Constructor del componente de formulario de equipo
   * @param equipoService Servicio para gestionar equipos
   * @param authService Servicio de autenticación
   * @param router Router para navegación
   */
  constructor(
    private equipoService: EquipoService,
    private authService: Authservice,
    private router: Router
  ) { }

  /**
   * Maneja la selección de archivo de imagen
   * Valida tipo, tamaño y dimensiones de la imagen
   * @param event Evento de selección de archivo
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

    // Validar dimensiones de la imagen
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>): void => {
      if (e.target?.result) {
        const img = new Image();
        img.onload = () => {
          const width = img.width;
          const height = img.height;

          // Verificar dimensiones mínimas
          if (width < 300 || height < 300) {
            alert('La imagen debe tener dimensiones mínimas de 300x300 píxeles.');
            input.value = '';
            this.imagenPreview = null;
            this.selectedFile = null;
            return;
          }

          // Verificar dimensiones máximas
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

  /**
   * Envía el formulario para crear un equipo
   * Crea el equipo primero y luego sube la imagen si existe
   */
  onSubmit(): void {
    this.errorMessage = '';
    
    // Validar nombre del equipo
    const nombre = this.nombreEquipo.trim();
    
    if (!nombre) {
      this.errorMessage = 'Por favor ingresa el nombre del equipo.';
      return;
    }

    if (nombre.length < 1 || nombre.length > 100) {
      this.errorMessage = 'El nombre del equipo debe tener entre 1 y 100 caracteres.';
      return;
    }

    // Verificar que el usuario esté logueado
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'Debes iniciar sesión para crear un equipo.';
      this.router.navigate(['/']);
      return;
    }

    this.isLoading = true;

    // Crear DTO para el equipo
    const equipoDto = {
      nombre: nombre,
      usuarioId: currentUser.id,
      liga: 'NFL'
    };

    console.log('Creando equipo:', equipoDto);

    // Primero crear el equipo
    this.equipoService.crearEquipo(equipoDto).subscribe({
      next: (equipoCreado) => {
        console.log('Equipo creado exitosamente:', equipoCreado);

        // Si hay imagen, subirla
        if (this.selectedFile) {
          this.equipoService.subirImagen(equipoCreado.id, this.selectedFile).subscribe({
            next: (imageResponse) => {
              console.log('Imagen subida exitosamente:', imageResponse);
              this.isLoading = false;
              alert('Equipo creado exitosamente con imagen');
              this.resetForm();
              this.router.navigate(['/mainpage/teams']);
            },
            error: (imageError) => {
              console.error('Error al subir imagen:', imageError);
              this.isLoading = false;
              // El equipo se creó pero la imagen falló
              alert('Equipo creado, pero hubo un error al subir la imagen. Puedes intentar subirla después.');
              this.resetForm();
              this.router.navigate(['/mainpage/teams']);
            }
          });
        } else {
          // No hay imagen, solo navegar
          this.isLoading = false;
          alert('Equipo creado exitosamente');
          this.resetForm();
          this.router.navigate(['/mainpage/teams']);
        }
      },
      error: (error) => {
        console.error('Error al crear equipo:', error);
        this.isLoading = false;
        
        // Manejar errores
        if (error.status === 400 && error.error?.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else if (error.status === 404) {
          this.errorMessage = 'Usuario no encontrado. Por favor, inicia sesión de nuevo.';
        } else if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else {
          this.errorMessage = 'Error al crear el equipo. Inténtalo de nuevo.';
        }
      }
    });
  }

  /**
   * Resetea el formulario a su estado inicial
   */
  private resetForm(): void {
    this.nombreEquipo = '';
    this.selectedFile = null;
    this.imagenPreview = null;
    this.errorMessage = '';
  }
}