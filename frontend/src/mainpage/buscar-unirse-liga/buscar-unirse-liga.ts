import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LigaService, LigaResponseDto, UnirseLigaDto } from '../../services/liga.service';
import { EquipoFantasyService, EquipoFantasyResponseDto, EquipoFantasyCreateDto } from '../../services/equipo-fantasy.service';
import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-buscar-unirse-liga',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './buscar-unirse-liga.html',
  styleUrls: ['./buscar-unirse-liga.css']
})
export class BuscarUnirseLiga implements OnInit {
  ligasEncontradas: LigaResponseDto[] = [];
  equiposDisponibles: EquipoFantasyResponseDto[] = [];
  
  busqueda: string = '';
  password: string = '';
  alias: string = '';
  
  // Opciones para el equipo
  opcionEquipo: 'existente' | 'nuevo' = 'existente';
  equipoSeleccionadoId: number = 0;
  
  // Datos para crear equipo nuevo
  nombreEquipoNuevo: string = '';
  selectedFile: File | null = null;
  imagenPreview: string | null = null;
  
  ligaSeleccionada: LigaResponseDto | null = null;
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  mostrarFormularioUnirse: boolean = false;

  constructor(
    private ligaService: LigaService,
    private authService: Authservice,
    private equipoFantasyService: EquipoFantasyService
  ) {}

  ngOnInit(): void {
    this.cargarTodasLasLigas();
    this.cargarEquiposDisponibles();
  }

  /**
   * Carga todas las ligas disponibles
   */
  cargarTodasLasLigas(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.ligaService.obtenerTodas().subscribe({
      next: (ligas) => {
        if (this.busqueda.trim()) {
          this.ligasEncontradas = ligas.filter(liga =>
            liga.nombreLiga.toLowerCase().includes(this.busqueda.toLowerCase()) &&
            this.getCuposDisponibles(liga) > 0
          );
        } else {
          this.ligasEncontradas = ligas.filter(liga => this.getCuposDisponibles(liga) > 0);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar ligas:', error);
        this.errorMessage = 'Error al cargar las ligas';
        this.isLoading = false;
      }
    });
  }

  /**
   * Carga equipos fantasy del usuario que NO están en ninguna liga
   */
  cargarEquiposDisponibles(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) return;

    this.equipoFantasyService.obtenerPorUsuario(currentUser.id).subscribe({
      next: (equipos) => {
        this.equiposDisponibles = equipos.filter(e => !e.ligaId);
      },
      error: (error) => {
        console.error('Error al cargar equipos:', error);
      }
    });
  }

  /**
   * Busca ligas por nombre
   */
  buscarLigas(): void {
    this.cargarTodasLasLigas();
  }

  /**
   * Calcula cupos disponibles
   */
  getCuposDisponibles(liga: LigaResponseDto): number {
    return liga.cuposTotales - liga.cuposOcupados;
  }

  /**
   * Obtiene clase CSS del badge según estado
   */
  getBadgeClass(estado: string): string {
    switch (estado) {
      case 'Pre-Draft': return 'bg-info';
      case 'En Draft': return 'bg-warning text-dark';
      case 'Activa': return 'bg-success';
      default: return 'bg-secondary';
    }
  }

  /**
   * Selecciona una liga para unirse
   */
  seleccionarLiga(liga: LigaResponseDto): void {
    this.ligaSeleccionada = liga;
    this.mostrarFormularioUnirse = true;
    this.password = '';
    this.alias = '';
    this.opcionEquipo = 'existente';
    this.equipoSeleccionadoId = 0;
    this.nombreEquipoNuevo = '';
    this.selectedFile = null;
    this.imagenPreview = null;
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
   * Cancela la unión a la liga
   */
  cancelarUnion(): void {
    this.mostrarFormularioUnirse = false;
    this.ligaSeleccionada = null;
    this.password = '';
    this.alias = '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
   * Maneja la selección de archivo de imagen
   */
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagenPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  /**
   * Confirma la unión a la liga
   */
  onUnirse(): void {
    if (!this.ligaSeleccionada) return;

    // Validaciones
    if (!this.validarFormulario()) return;

    this.isLoading = true;
    const currentUser = this.authService.currentUserValue!;

    if (this.opcionEquipo === 'nuevo') {
      // Crear equipo nuevo y luego unirse
      this.crearEquipoYUnirse(currentUser.id);
    } else {
      // Usar equipo existente
      this.unirseConEquipo(currentUser.id, this.equipoSeleccionadoId);
    }
  }

  /**
   * Crea un equipo nuevo y luego se une a la liga
   */
  private crearEquipoYUnirse(usuarioId: number): void {
    const equipoDto: EquipoFantasyCreateDto = {
      nombre: this.nombreEquipoNuevo,
      usuarioId: usuarioId,
      ligaId: undefined
    };

    this.equipoFantasyService.crear(equipoDto).subscribe({
      next: (equipoCreado) => {
        if (this.selectedFile) {
          this.equipoFantasyService.subirImagen(equipoCreado.id, this.selectedFile).subscribe({
            next: () => {
              this.unirseConEquipo(usuarioId, equipoCreado.id);
            },
            error: () => {
              this.unirseConEquipo(usuarioId, equipoCreado.id);
            }
          });
        } else {
          this.unirseConEquipo(usuarioId, equipoCreado.id);
        }
      },
      error: (error) => {
        console.error('Error al crear equipo:', error);
        this.isLoading = false;
        this.errorMessage = 'Error al crear el equipo. Inténtalo de nuevo.';
      }
    });
  }

  /**
   * Se une a la liga con el equipo especificado
   */
  private unirseConEquipo(usuarioId: number, equipoFantasyId: number): void {
    const unirseDto: UnirseLigaDto = {
      ligaId: this.ligaSeleccionada!.idLiga,
      password: this.password,
      usuarioId: usuarioId,
      equipoId: equipoFantasyId,
      alias: this.alias
    };

    this.ligaService.unirseALiga(unirseDto).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = '¡Te has unido a la liga exitosamente!';
        
        setTimeout(() => {
          this.mostrarFormularioUnirse = false;
          this.cargarTodasLasLigas();
        }, 2000);
      },
      error: (error) => {
        console.error('Error al unirse a la liga:', error);
        this.isLoading = false;
        
        if (error.status === 400 && error.error?.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else {
          this.errorMessage = 'Error al unirse a la liga. Verifica la contraseña.';
        }
      }
    });
  }

  /**
   * Valida el formulario de unirse
   */
  private validarFormulario(): boolean {
    if (!this.password.trim() || this.password.length < 8) {
      this.errorMessage = 'La contraseña debe tener al menos 8 caracteres';
      return false;
    }

    if (!this.alias.trim()) {
      this.errorMessage = 'El alias es obligatorio';
      return false;
    }

    if (this.alias.length > 50) {
      this.errorMessage = 'El alias no puede exceder 50 caracteres';
      return false;
    }

    if (this.opcionEquipo === 'existente') {
      if (!this.equipoSeleccionadoId || this.equipoSeleccionadoId === 0) {
        this.errorMessage = 'Debes seleccionar un equipo';
        return false;
      }
    } else {
      if (!this.nombreEquipoNuevo.trim()) {
        this.errorMessage = 'El nombre del equipo es obligatorio';
        return false;
      }
    }

    return true;
  }
}