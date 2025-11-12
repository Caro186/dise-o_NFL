import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JugadorService, 
      CrearJugadorDto, 
      JugadorResponseDto, 
      JugadorListDto,
      EquipoNFL 
 } from '../../services/jugadores.service';

@Component({
  selector: 'app-jugadores',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './jugadores.component.html',
  styleUrls: ['./jugadores.component.css']
})
export class JugadoresComponent implements OnInit {
  // Datos del formulario
  jugador: CrearJugadorDto = {
    nombre: '',
    posicion: '',
    equipoNFLId: 0
  };

  // Archivos de imagen
  imagenSeleccionada: File | null = null;
  thumbnailSeleccionado: File | null = null;
  imagenPreview: string | null = null;
  thumbnailPreview: string | null = null;

  // Listas
  jugadoresCreados: JugadorListDto[] = [];
  equiposNFL: EquipoNFL[] = [];
  posicionesDisponibles: string[] = [
    'QB',  // Quarterback
    'RB',  // Running Back
    'WR',  // Wide Receiver
    'TE',  // Tight End
    'K',   // Kicker
    'DEF', // Defense/Special Teams
    'OL',  // Offensive Line
    'DL',  // Defensive Line
    'LB',  // Linebacker
    'DB',  // Defensive Back
    'P'    // Punter
  ];

  // Estados
  cargando: boolean = false;
  subiendoImagen: boolean = false;
  mensajeError: string = '';
  mensajeExito: string = '';
  mostrarFormulario: boolean = true;
  jugadorSeleccionado: JugadorResponseDto | null = null;

  // Filtros
  filtroEquipo: number = 0;
  filtroPosicion: string = '';
  filtroEstado: string = '';

  constructor(private jugadorService: JugadorService) {}

  ngOnInit(): void {
    this.cargarEquiposNFL();
    this.cargarJugadores();
  }

  /**
   * Cargar equipos NFL para el selector
   */
  cargarEquiposNFL(): void {
    this.jugadorService.obtenerEquiposNFL().subscribe({
      next: (equipos) => {
        this.equiposNFL = equipos.filter(e => e.estado === 'Activo');
      },
      error: (error) => {
        this.mostrarError('Error al cargar los equipos NFL: ' + error.message);
      }
    });
  }

  /**
   * Cargar todos los jugadores
   */
  cargarJugadores(): void {
    this.cargando = true;
    this.jugadorService.obtenerJugadores().subscribe({
      next: (jugadores) => {
        this.jugadoresCreados = jugadores;
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al cargar los jugadores: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Manejar selección de imagen
   */
  onImagenSeleccionada(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.validarYPreviewImagen(file, 'imagen');
    }
  }

  /**
   * Manejar selección de thumbnail
   */
  onThumbnailSeleccionado(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.validarYPreviewImagen(file, 'thumbnail');
    }
  }

  /**
   * Validar y generar preview de imagen
   */
  async validarYPreviewImagen(file: File, tipo: 'imagen' | 'thumbnail'): Promise<void> {
    // Validar tipo de archivo
    const tiposPermitidos = ['image/jpeg', 'image/png', 'image/jpg'];
    if (!tiposPermitidos.includes(file.type)) {
      this.mostrarError('Solo se permiten imágenes JPEG o PNG');
      return;
    }

    // Validar tamaño
    const maxSize = tipo === 'imagen' ? 5 * 1024 * 1024 : 2 * 1024 * 1024; // 5MB para imagen, 2MB para thumbnail
    if (file.size > maxSize) {
      const maxSizeMB = tipo === 'imagen' ? 5 : 2;
      this.mostrarError(`El tamaño máximo permitido es ${maxSizeMB} MB`);
      return;
    }

    // Validar que sea cuadrada
    try {
      const validacion = await this.jugadorService.validarImagenCuadrada(file);
      if (!validacion.valida) {
        this.mostrarError(`La imagen debe ser cuadrada. Dimensiones actuales: ${validacion.ancho}x${validacion.alto}px`);
        return;
      }

      // Si es válida, guardar el archivo y generar preview
      if (tipo === 'imagen') {
        this.imagenSeleccionada = file;
        this.generarPreview(file, 'imagen');
      } else {
        this.thumbnailSeleccionado = file;
        this.generarPreview(file, 'thumbnail');
      }

      this.mensajeError = '';
    } catch (error) {
      this.mostrarError('Error al validar la imagen');
    }
  }

  /**
   * Generar preview de imagen
   */
  generarPreview(file: File, tipo: 'imagen' | 'thumbnail'): void {
    const reader = new FileReader();
    reader.onload = (e: any) => {
      if (tipo === 'imagen') {
        this.imagenPreview = e.target.result;
      } else {
        this.thumbnailPreview = e.target.result;
      }
    };
    reader.readAsDataURL(file);
  }

  /**
   * Limpiar imagen seleccionada
   */
  limpiarImagen(tipo: 'imagen' | 'thumbnail'): void {
    if (tipo === 'imagen') {
      this.imagenSeleccionada = null;
      this.imagenPreview = null;
    } else {
      this.thumbnailSeleccionado = null;
      this.thumbnailPreview = null;
    }
  }

  /**
   * Crear un nuevo jugador
   */
  crearJugador(): void {
    this.mensajeError = '';
    this.mensajeExito = '';

    if (!this.validarFormulario()) {
      return;
    }

    this.cargando = true;

    const jugadorDto: CrearJugadorDto = {
      nombre: this.jugador.nombre.trim(),
      posicion: this.jugador.posicion,
      equipoNFLId: this.jugador.equipoNFLId
    };

    this.jugadorService.crearJugador(jugadorDto).subscribe({
      next: (response) => {
        // Si hay imágenes, subirlas
        if (this.imagenSeleccionada || this.thumbnailSeleccionado) {
          this.subirImagenes(response.id);
        } else {
          this.mostrarExito('¡Jugador creado exitosamente!');
          this.limpiarFormulario();
          this.cargarJugadores();
          this.cargando = false;
        }
      },
      error: (error) => {
        this.mostrarError(error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Subir imágenes después de crear el jugador
   */
  subirImagenes(jugadorId: number): void {
    this.subiendoImagen = true;
    let promesas: Promise<any>[] = [];

    if (this.imagenSeleccionada) {
      const promesaImagen = this.jugadorService.subirImagen(jugadorId, this.imagenSeleccionada).toPromise();
      promesas.push(promesaImagen);
    }

    if (this.thumbnailSeleccionado) {
      const promesaThumbnail = this.jugadorService.subirThumbnail(jugadorId, this.thumbnailSeleccionado).toPromise();
      promesas.push(promesaThumbnail);
    }

    Promise.all(promesas)
      .then(() => {
        this.mostrarExito('¡Jugador e imágenes creados exitosamente!');
        this.limpiarFormulario();
        this.cargarJugadores();
      })
      .catch((error) => {
        this.mostrarError('Jugador creado, pero hubo un error al subir las imágenes: ' + error.message);
      })
      .finally(() => {
        this.cargando = false;
        this.subiendoImagen = false;
      });
  }

  /**
   * Validar el formulario
   */
  validarFormulario(): boolean {
    if (!this.jugador.nombre || this.jugador.nombre.trim().length === 0) {
      this.mostrarError('El nombre del jugador es requerido');
      return false;
    }

    if (this.jugador.nombre.trim().length > 100) {
      this.mostrarError('El nombre no puede exceder 100 caracteres');
      return false;
    }

    if (!this.jugador.posicion) {
      this.mostrarError('La posición es requerida');
      return false;
    }

    if (!this.jugador.equipoNFLId || this.jugador.equipoNFLId === 0) {
      this.mostrarError('Debe seleccionar un equipo NFL');
      return false;
    }

    return true;
  }

  /**
   * Limpiar el formulario
   */
  limpiarFormulario(): void {
    this.jugador = {
      nombre: '',
      posicion: '',
      equipoNFLId: 0
    };
    this.limpiarImagen('imagen');
    this.limpiarImagen('thumbnail');
  }

  /**
   * Ver detalles de un jugador
   */
  verDetalles(id: number): void {
    this.cargando = true;
    this.jugadorService.obtenerJugadorPorId(id).subscribe({
      next: (jugador) => {
        this.jugadorSeleccionado = jugador;
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al cargar los detalles: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Cerrar modal de detalles
   */
  cerrarDetalles(): void {
    this.jugadorSeleccionado = null;
  }

  /**
   * Desactivar un jugador
   */
  desactivarJugador(id: number): void {
    if (!confirm('¿Está seguro de que desea desactivar este jugador?')) {
      return;
    }

    this.cargando = true;
    this.jugadorService.desactivarJugador(id).subscribe({
      next: (response) => {
        this.mostrarExito('Jugador desactivado correctamente');
        this.cargarJugadores();
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al desactivar el jugador: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Eliminar permanentemente un jugador
   */
  eliminarJugador(id: number): void {
    if (!confirm('⚠️ ¿Está seguro de que desea eliminar permanentemente este jugador? Esta acción no se puede deshacer.')) {
      return;
    }

    this.cargando = true;
    this.jugadorService.eliminarJugadorPermanente(id).subscribe({
      next: (response) => {
        this.mostrarExito('Jugador eliminado permanentemente');
        this.cargarJugadores();
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al eliminar el jugador: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Aplicar filtros
   */
  aplicarFiltros(): void {
    if (this.filtroEquipo && this.filtroEquipo > 0) {
      this.filtrarPorEquipo();
    } else if (this.filtroPosicion) {
      this.filtrarPorPosicion();
    } else {
      this.cargarJugadores();
    }
  }

  /**
   * Filtrar por equipo
   */
  filtrarPorEquipo(): void {
    this.cargando = true;
    this.jugadorService.obtenerJugadoresPorEquipo(this.filtroEquipo).subscribe({
      next: (jugadores) => {
        this.jugadoresCreados = jugadores;
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al filtrar: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Filtrar por posición
   */
  filtrarPorPosicion(): void {
    this.cargando = true;
    this.jugadorService.obtenerJugadoresPorPosicion(this.filtroPosicion).subscribe({
      next: (jugadores) => {
        this.jugadoresCreados = jugadores;
        this.cargando = false;
      },
      error: (error) => {
        this.mostrarError('Error al filtrar: ' + error.message);
        this.cargando = false;
      }
    });
  }

  /**
   * Limpiar filtros
   */
  limpiarFiltros(): void {
    this.filtroEquipo = 0;
    this.filtroPosicion = '';
    this.filtroEstado = '';
    this.cargarJugadores();
  }

  /**
   * Obtener jugadores filtrados
   */
  get jugadoresFiltrados(): JugadorListDto[] {
    if (!this.filtroEstado) {
      return this.jugadoresCreados;
    }
    return this.jugadoresCreados.filter(j => j.estado === this.filtroEstado);
  }

  /**
   * Alternar visibilidad del formulario
   */
  toggleFormulario(): void {
    this.mostrarFormulario = !this.mostrarFormulario;
    if (this.mostrarFormulario) {
      this.limpiarFormulario();
    }
  }

  /**
   * Mostrar mensaje de error
   */
  private mostrarError(mensaje: string): void {
    this.mensajeError = mensaje;
    this.mensajeExito = '';
    setTimeout(() => {
      this.mensajeError = '';
    }, 5000);
  }

  /**
   * Mostrar mensaje de éxito
   */
  private mostrarExito(mensaje: string): void {
    this.mensajeExito = mensaje;
    this.mensajeError = '';
    setTimeout(() => {
      this.mensajeExito = '';
    }, 5000);
  }

  /**
   * Obtener el nombre del equipo por ID
   */
  obtenerNombreEquipo(id: number): string {
    const equipo = this.equiposNFL.find(e => e.id === id);
    return equipo ? equipo.nombre : 'Desconocido';
  }
}