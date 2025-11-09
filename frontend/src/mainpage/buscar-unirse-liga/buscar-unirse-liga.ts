import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LigaService, LigaResponseDto } from '../../services/liga.service';
import { EquipoFantasyService, EquipoFantasyResponseDto } from '../../services/equipo-fantasy.service';
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
  equipoSeleccionadoId: number = 0;
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
    this.cargarEquiposUsuario();
  }

  /**
   * Carga todas las ligas disponibles
   */
  cargarTodasLasLigas(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.ligaService.obtenerTodas().subscribe({
      next: (ligas: LigaResponseDto[]) => {
        if (this.busqueda.trim()) {
          this.ligasEncontradas = ligas.filter((liga: LigaResponseDto) =>
            liga.nombreLiga.toLowerCase().includes(this.busqueda.toLowerCase())
          );
        } else {
          // Mostrar solo ligas con cupos disponibles y en estados permitidos
          this.ligasEncontradas = ligas.filter((liga: LigaResponseDto) =>
            (liga.estado === 'Pre-Draft' || liga.estado === 'Activa') &&
            liga.cuposOcupados < liga.cuposTotales
          );
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error al cargar ligas:', error);
        this.errorMessage = 'Error al cargar las ligas disponibles';
        this.isLoading = false;
      }
    });
  }

  /**
   * Carga los equipos del usuario actual que no están en ninguna liga
   */
  cargarEquiposUsuario(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) return;

    this.equipoFantasyService.obtenerPorUsuario(currentUser.id).subscribe({
      next: (equipos: EquipoFantasyResponseDto[]) => {
        // Solo equipos sin liga
        this.equiposDisponibles = equipos.filter(e => !e.ligaId);
        console.log('Equipos disponibles:', this.equiposDisponibles);
      },
      error: (error: any) => {
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
   * Calcula los cupos disponibles
   */
  getCuposDisponibles(liga: LigaResponseDto): number {
    return liga.cuposTotales - liga.cuposOcupados;
  }

  /**
   * Selecciona una liga para unirse
   */
  seleccionarLiga(liga: LigaResponseDto): void {
    this.ligaSeleccionada = liga;
    this.mostrarFormularioUnirse = true;
    this.password = '';
    this.alias = '';
    this.equipoSeleccionadoId = 0;
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
    this.equipoSeleccionadoId = 0;
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
   * Confirma la unión a la liga
   */
  onUnirse(): void {
    if (!this.ligaSeleccionada) {
      return;
    }

    // Validaciones
    if (!this.password.trim()) {
      this.errorMessage = 'La contraseña es obligatoria';
      return;
    }

    if (this.password.length < 8) {
      this.errorMessage = 'La contraseña debe tener al menos 8 caracteres';
      return;
    }

    if (!this.alias.trim()) {
      this.errorMessage = 'El alias es obligatorio';
      return;
    }

    if (this.alias.length > 50) {
      this.errorMessage = 'El alias no puede exceder 50 caracteres';
      return;
    }

    if (!this.equipoSeleccionadoId || this.equipoSeleccionadoId === 0) {
      this.errorMessage = 'Debes seleccionar un equipo';
      return;
    }

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'Debes iniciar sesión';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const unirseData = {
      ligaId: this.ligaSeleccionada.idLiga,
      password: this.password,
      usuarioId: currentUser.id,
      equipoId: this.equipoSeleccionadoId,
      alias: this.alias.trim()
    };

    console.log('Enviando datos para unirse a liga:', unirseData);

    this.ligaService.unirseALiga(unirseData).subscribe({
      next: (response: any) => {
        console.log('Unido a la liga exitosamente:', response);
        this.successMessage = response.mensaje || 'Te has unido exitosamente a la liga';
        this.isLoading = false;
        
        // Limpiar formulario
        this.password = '';
        this.alias = '';
        this.equipoSeleccionadoId = 0;
        
        // Recargar equipos y ligas
        this.cargarEquiposUsuario();
        this.cargarTodasLasLigas();
        
        // Ocultar formulario después de 2 segundos
        setTimeout(() => {
          this.mostrarFormularioUnirse = false;
          this.ligaSeleccionada = null;
          this.successMessage = '';
        }, 2000);
      },
      error: (error: any) => {
        console.error('Error al unirse a la liga:', error);
        
        // Manejar diferentes tipos de errores
        if (error.status === 400 && error.error?.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else if (error.status === 404) {
          this.errorMessage = 'Liga o equipo no encontrado';
        } else if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else {
          this.errorMessage = 'Error al unirse a la liga. Inténtalo de nuevo.';
        }
        
        this.isLoading = false;
      }
    });
  }
}