import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LeagueService, JoinLeagueRequest } from '../../services/liga.service';

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
  nombreEquipo: string = '';

  // Estados
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // ID del usuario actual (deberías obtenerlo de tu servicio de autenticación)
  idUsuario: number = 1; // TODO: Reemplazar con el ID real del usuario logueado

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private leagueService: LeagueService
  ) {}

  ngOnInit(): void {
    // Obtener el ID de la liga desde la ruta
    this.route.params.subscribe(params => {
      this.idLiga = +params['id']; // El '+' convierte el string a número
      if (this.idLiga) {
        this.loadLeagueData();
      }
    });
  }

  loadLeagueData(): void {
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

  checkIfUserInLeague(): void {
    this.leagueService.checkUserInLeague(this.idLiga, this.idUsuario).subscribe({
      next: (exists) => {
        if (exists) {
          this.errorMessage = 'Ya eres parte de esta liga';
        }
      },
      error: (error) => {
        console.error('Error al verificar usuario en liga:', error);
      }
    });
  }

  isFormValid(): boolean {
    return this.password.trim() !== '' && 
           this.alias.trim() !== '' && 
           this.nombreEquipo.trim() !== '';
  }

  onJoin(): void {
    if (!this.isFormValid()) {
      this.errorMessage = 'Por favor completa todos los campos';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: JoinLeagueRequest = {
      id_liga: this.idLiga,
      id_usuario: this.idUsuario,
      password: this.password,
      alias: this.alias.trim(),
      nombre_equipo: this.nombreEquipo.trim()
    };

    this.leagueService.joinLeague(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.successMessage = response.message || '¡Te has unido exitosamente!';
          
          // Redirigir después de 2 segundos
          setTimeout(() => {
            this.router.navigate(['/leagues', this.idLiga]); // Ajusta la ruta según tu app
          }, 2000);
        } else {
          this.errorMessage = response.message || 'No se pudo unir a la liga';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Error al intentar unirse a la liga';
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/leagues']); // Ajusta la ruta según tu app
  }
}