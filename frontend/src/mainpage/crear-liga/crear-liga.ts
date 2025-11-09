import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LigaService, LigaCreateDto, LigaResponseDto } from '../../services/liga.service';
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
  nombreLiga: string = '';
  descripcion: string = '';
  password: string = '';
  confirmPassword: string = '';
  cuposTotales: number = 10;
  idTemporada: number = 0;
  nombreEquipoComisionado: string = '';
  permitirDecimales: boolean = true;
  configPlayoffs: string = '4-equipos';
  
  temporadas: TemporadaResponseDto[] = [];
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
    private temporadaService: TemporadaService,
    private authService: Authservice,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarTemporadas();
  }

  /**
   * Carga las temporadas disponibles
   */
  cargarTemporadas(): void {
    this.temporadaService.obtenerTemporadas().subscribe({
      next: (temporadas: TemporadaResponseDto[]) => {
        this.temporadas = temporadas;
        const temporadaActual = temporadas.find((t: TemporadaResponseDto) => t.actual);
        if (temporadaActual) {
          this.idTemporada = temporadaActual.id;
        }
      },
      error: (error: any) => {
        console.error('Error al cargar temporadas:', error);
        this.errorMessage = 'Error al cargar temporadas disponibles';
      }
    });
  }

  /**
   * Crea una nueva liga
   */
  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    // Validaciones
    if (!this.nombreLiga.trim()) {
      this.errorMessage = 'El nombre de la liga es obligatorio';
      return;
    }

    if (!this.password.trim()) {
      this.errorMessage = 'La contraseña es obligatoria';
      return;
    }

    if (this.password.length < 8) {
      this.errorMessage = 'La contraseña debe tener al menos 8 caracteres';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Las contraseñas no coinciden';
      return;
    }

    if (!this.nombreEquipoComisionado.trim()) {
      this.errorMessage = 'El nombre de tu equipo es obligatorio';
      return;
    }

    if (!this.idTemporada) {
      this.errorMessage = 'Debes seleccionar una temporada';
      return;
    }

    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.errorMessage = 'Debes iniciar sesión';
      return;
    }

    this.isLoading = true;

    const ligaData: LigaCreateDto = {
      nombreLiga: this.nombreLiga.trim(),
      descripcion: this.descripcion.trim() || undefined,
      passwordHash: this.password,
      idTemporada: this.idTemporada,
      cuposTotales: this.cuposTotales,
      comisionadoId: currentUser.id,
      formatoPosiciones: JSON.stringify({ QB: 1, RB: 2, WR: 2, TE: 1, FLEX: 1, K: 1, DEF: 1 }),
      esquemaPuntos: JSON.stringify({ passingYd: 0.04, passingTD: 4, rushingYd: 0.1, rushingTD: 6 }),
      configPlayoffs: this.configPlayoffs,
      permitirDecimales: this.permitirDecimales
    };

    this.ligaService.crear(ligaData).subscribe({
      next: (response: LigaResponseDto) => {
        this.successMessage = `Liga "${response.nombreLiga}" creada exitosamente`;
        this.isLoading = false;
        
        setTimeout(() => {
          this.router.navigate(['/mainpage/liga']);
        }, 2000);
      },
      error: (error: any) => {
        console.error('Error al crear liga:', error);
        this.errorMessage = error.error?.mensaje || 'Error al crear la liga';
        this.isLoading = false;
      }
    });
  }

  /**
   * Cancela la creación
   */
  onCancel(): void {
    this.router.navigate(['/mainpage/liga']);
  }
}