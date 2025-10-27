import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LeagueService, JoinLeagueRequest } from '../../services/liga.service';
import { AuthService } from '../../services/authservice';
import { EquipoService } from '../../services/equipo.service';

interface EquipoDisponible {
  id: number;
  nombre: string;
  imagenUrl?: string;
  fechaCreacion: Date;
}

@Component({
  selector: 'app-join-league',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './join-league.html',
  styleUrls: ['./join-league.css']
})
export class JoinLeagueComponent implements OnInit {
  // Datos de la liga
  idLiga: number = 0;
  ligaNombre: string = '';
  temporada: string = '';
  estado: string = '';
  cuposDisponibles: number = 0;
  cuposTotales: number = 0;

  // Datos del formulario
  password: string = '';
  alias: string = '';
  equipoSeleccionadoId: number | null = null;
  
  // Lista de equipos disponibles
  equiposDisponibles: EquipoDisponible[] = [];

  // Estados
  isLoading: boolean = false;
  isLoadingTeams: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private leagueService: LeagueService,
    private authService: AuthService,
    private equipoService: EquipoService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.idLiga = +params['id'];
      if (this.idLiga) {
        this.loadLeagueData();
        this.loadAvailableTeams();
      }
    });
  }

  /**
   * Carga los datos de la liga
   */
  private loadLeagueData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.leagueService.getLeagueById(this.idLiga).subscribe({
      next: (liga) => {
        this.ligaNombre = liga.nombre_liga;
        this.temporada = liga.temporada;
        this.estado = liga.estado;
        this.cuposTotales = liga.cupos_totales;
        this.cuposDisponibles = liga.cupos_totales - liga.cupos_ocupados;
        this.isLoading = false;

        // Verificar si ya está en la liga
        this.checkIfUserInLeague();
      },
      error: (error) => {
        this.errorMessage = error.message || 'Error al cargar los datos de la liga';
        this.isLoading = false;
      }
    });
  }

  /**
   * Carga los equipos disponibles del usuario (sin liga asignada)
   */
  private loadAvailableTeams(): void {
    const usuarioId = this.authService.getUsuarioId();
    if (!usuarioId) return;

    this.isLoadingTeams = true;
    
    this.leagueService.getAvailableTeams(usuarioId).subscribe({
      next: (equipos) => {
        this.equiposDisponibles = equipos;
        this.isLoadingTeams = false;
        
        console.log('✅ Equipos disponibles cargados:', this.equiposDisponibles);
        
        // Si hay equipos disponibles, seleccionar el primero por defecto
        if (this.equiposDisponibles.length > 0) {
          this.equipoSeleccionadoId = this.equiposDisponibles[0].id;
        }
      },
      error: (error) => {
        console.error('Error al cargar equipos disponibles:', error);
        this.isLoadingTeams = false;
        this.errorMessage = 'No tienes equipos disponibles. Debes crear uno primero.';
      }
    });
  }

  /**
   * Verifica si el usuario ya está en la liga
   */
  private checkIfUserInLeague(): void {
    const usuarioId = this.authService.getUsuarioId();
    if (!usuarioId) return;

    this.equipoService.obtenerEquiposPorUsuario(usuarioId).subscribe({
      next: (equipos) => {
        const equipoEnLiga = equipos.find(e => e.ligaId === this.idLiga);
        if (equipoEnLiga) {
          this.errorMessage = 'Ya eres parte de esta liga';
        }
      },
      error: (error) => {
        console.error('Error al verificar usuario en liga:', error);
      }
    });
  }

  /**
   * Valida si el formulario es válido
   */
  isFormValid(): boolean {
    return this.password.trim() !== '' && this.equipoSeleccionadoId !== null;
  }

  /**
   * Maneja el envío del formulario para unirse a la liga
   */
  onJoin(): void {
    const usuarioId = this.authService.getUsuarioId();
    if (!usuarioId) {
      this.errorMessage = 'Debes iniciar sesión para unirte a la liga.';
      this.router.navigate(['/']);
      return;
    }

    if (!this.isFormValid()) {
      this.errorMessage = 'Por favor completa todos los campos requeridos';
      return;
    }

    if (!this.equipoSeleccionadoId) {
      this.errorMessage = 'Debes seleccionar un equipo';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: JoinLeagueRequest = {
      ligaId: this.idLiga,
      usuarioId: usuarioId,
      password: this.password.trim(),
      equipoId: this.equipoSeleccionadoId,
      alias: this.alias.trim() || undefined
    };

    console.log('🚀 Enviando petición para unirse a liga:', request);

    this.leagueService.joinLeague(request).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        this.successMessage = '¡Te has unido exitosamente a la liga!';
        
        console.log('✅ Unido a liga exitosamente:', response);
        
        setTimeout(() => {
          this.router.navigate(['/leagues', this.idLiga]);
        }, 2000);
      },
      error: (error: any) => {
        this.isLoading = false;
        console.error('❌ Error al unirse a liga:', error);
        
        // Manejar diferentes tipos de errores
        if (error.message) {
          this.errorMessage = error.message;
        } else if (error.error?.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else {
          this.errorMessage = 'Error al intentar unirse a la liga';
        }
      }
    });
  }

  /**
   * Navega a la página de crear equipo
   */
  onCreateNewTeam(): void {
    this.router.navigate(['/mainpage/teams/create']);
  }

  /**
   * Obtiene el nombre del equipo seleccionado
   */
  getEquipoSeleccionadoNombre(): string {
    if (!this.equipoSeleccionadoId) return '';
    const equipo = this.equiposDisponibles.find(e => e.id === this.equipoSeleccionadoId);
    return equipo?.nombre || '';
  }

  /**
   * Cancela y vuelve a la lista de ligas
   */
  onCancel(): void {
    this.router.navigate(['/leagues']);
  }
}