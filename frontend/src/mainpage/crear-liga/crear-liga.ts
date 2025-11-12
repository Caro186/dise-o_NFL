import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LigaService, LigaCreateDto, UnirseLigaDto } from '../../services/liga.service';
import { EquipoFantasyService, EquipoFantasyResponseDto, EquipoFantasyCreateDto } from '../../services/equipo-fantasy.service';
import { TemporadaService, TemporadaResponseDto } from '../../services/temporada.service';
import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-crear-liga',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-liga.html',
  styleUrls: ['./crear-liga.css']
})
export class CrearLiga implements OnInit {
  // Datos de la liga
  nombreLiga: string = '';
  descripcion: string = '';
  password: string = '';
  confirmPassword: string = '';
  cuposTotales: number = 10;
  idTemporada: number = 0;
  permitirDecimales: boolean = true;
  configPlayoffs: string = '4-equipos';
  
  // Opciones para el equipo
  opcionEquipo: 'existente' | 'nuevo' = 'existente';
  equipoSeleccionadoId: number = 0;
  
  // Datos para crear equipo nuevo
  nombreEquipoNuevo: string = '';
  selectedFile: File | null = null;
  imagenPreview: string | null = null;
  
  // Catálogos
  temporadas: TemporadaResponseDto[] = [];
  equiposDisponibles: EquipoFantasyResponseDto[] = [];
  cantidadesEquipos: number[] = [4, 6, 8, 10, 12, 14, 16, 18, 20];
  opcionesPlayoffs = [
    { valor: '4-equipos', texto: '4 equipos (Top 4)' },
    { valor: '6-equipos', texto: '6 equipos (Top 6)' },
    { valor: '8-equipos', texto: '8 equipos (Top 8)' }
  ];
  
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private ligaService: LigaService,
    private equipoFantasyService: EquipoFantasyService,
    private temporadaService: TemporadaService,
    private authService: Authservice,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarTemporadas();
    this.cargarEquiposDisponibles();
  }

  /**
   * Carga las temporadas disponibles
   */
  cargarTemporadas(): void {
    this.temporadaService.obtenerTemporadas().subscribe({
      next: (temporadas) => {
        this.temporadas = temporadas;
        const temporadaActual = temporadas.find(t => t.actual);
        if (temporadaActual) {
          this.idTemporada = temporadaActual.id;
        }
      },
      error: (error) => {
        console.error('Error al cargar temporadas:', error);
        this.errorMessage = 'Error al cargar temporadas disponibles';
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
        // Filtrar solo equipos sin liga
        this.equiposDisponibles = equipos.filter(e => !e.ligaId);
      },
      error: (error) => {
        console.error('Error al cargar equipos:', error);
      }
    });
  }

  /**
   * Maneja la selección de archivo de imagen para equipo nuevo
   */
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      
      // Vista previa
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagenPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  /**
   * Crea la liga
   */
  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    // Validaciones
    if (!this.validarFormulario()) {
      return;
    }

    this.isLoading = true;
    const currentUser = this.authService.currentUserValue!;

    if (this.opcionEquipo === 'nuevo') {
      // Primero crear el equipo nuevo
      this.crearEquipoYLiga(currentUser.id);
    } else {
      // Usar equipo existente
      this.crearLiga(currentUser.id, this.equipoSeleccionadoId);
    }
  }

  /**
   * Crea un equipo nuevo y luego la liga
   */
  private crearEquipoYLiga(usuarioId: number): void {
    const equipoDto: EquipoFantasyCreateDto = {
      nombre: this.nombreEquipoNuevo,
      usuarioId: usuarioId,
      ligaId: undefined // Sin liga por ahora
    };

    this.equipoFantasyService.crear(equipoDto).subscribe({
      next: (equipoCreado) => {
        // Si hay imagen, subirla
        if (this.selectedFile) {
          this.equipoFantasyService.subirImagen(equipoCreado.id, this.selectedFile).subscribe({
            next: () => {
              this.crearLiga(usuarioId, equipoCreado.id);
            },
            error: () => {
              // Continuar aunque falle la imagen
              this.crearLiga(usuarioId, equipoCreado.id);
            }
          });
        } else {
          this.crearLiga(usuarioId, equipoCreado.id);
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
   * Crea la liga con el equipo especificado y lo vincula
   */
  private crearLiga(usuarioId: number, equipoFantasyId: number): void {
    const ligaDto: LigaCreateDto = {
      nombreLiga: this.nombreLiga,
      descripcion: this.descripcion,
      passwordHash: this.password,
      idTemporada: this.idTemporada,
      cuposTotales: this.cuposTotales,
      comisionadoId: usuarioId,
      formatoPosiciones: 'QB,RB,RB,WR,WR,TE,FLEX,K,DEF',
      esquemaPuntos: 'PPR',
      configPlayoffs: this.configPlayoffs,
      permitirDecimales: this.permitirDecimales
    };

    this.ligaService.crear(ligaDto).subscribe({
      next: (ligaCreada) => {
        // Ahora vincular el equipo a la liga usando el endpoint de unirse
        const unirseDto: UnirseLigaDto = {
          ligaId: ligaCreada.idLiga,
          password: this.password,
          usuarioId: usuarioId,
          equipoId: equipoFantasyId,
          alias: this.nombreLiga // o puedes usar otro alias
        };

        this.ligaService.unirseALiga(unirseDto).subscribe({
          next: () => {
            this.isLoading = false;
            this.successMessage = '¡Liga creada y equipo vinculado exitosamente!';
            
            setTimeout(() => {
              this.router.navigate(['/mainpage/ligas']);
            }, 2000);
          },
          error: (error) => {
            console.error('Error al vincular equipo a la liga:', error);
            this.isLoading = false;
            this.errorMessage = 'Liga creada, pero no se pudo vincular el equipo. Intenta unirte manualmente.';
            
            setTimeout(() => {
              this.router.navigate(['/mainpage/ligas']);
            }, 3000);
          }
        });
      },
      error: (error) => {
        console.error('Error al crear liga:', error);
        this.isLoading = false;
        
        if (error.status === 400 && error.error?.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else {
          this.errorMessage = 'Error al crear la liga. Inténtalo de nuevo.';
        }
      }
    });
  }

  /**
   * Valida el formulario
   */
  private validarFormulario(): boolean {
    if (!this.nombreLiga.trim()) {
      this.errorMessage = 'El nombre de la liga es obligatorio';
      return false;
    }

    if (!this.password.trim() || this.password.length < 8) {
      this.errorMessage = 'La contraseña debe tener al menos 8 caracteres';
      return false;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return false;
    }

    if (!this.idTemporada) {
      this.errorMessage = 'Debes seleccionar una temporada';
      return false;
    }

    // Validaciones según opción de equipo
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