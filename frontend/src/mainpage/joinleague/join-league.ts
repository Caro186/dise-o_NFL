import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-join-league',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './join-league.html',
  styleUrls: ['./join-league.css']
})
export class JoinLeague implements OnInit {
  // Información de la liga (viene de la búsqueda)
  ligaId: string = '';
  ligaNombre: string = 'Liga Fantasía 2025';
  temporada: string = '2025';
  estado: string = 'Activa';
  cuposDisponibles: number = 3;
  cuposTotales: number = 10;

  // Datos del formulario
  password: string = '';
  alias: string = '';
  nombreEquipo: string = '';

  // Estado de la UI
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.ligaId = this.route.snapshot.paramMap.get('id') || '';
    this.loadLeagueData();
  }

  loadLeagueData(): void {
    console.log('Cargando datos de la liga:', this.ligaId);
  }

  isFormValid(): boolean {
    return this.password.trim() !== '' && 
           this.alias.trim() !== '' && 
           this.nombreEquipo.trim() !== '';
  }

  onJoin(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.password.trim()) {
      this.errorMessage = 'La contraseña es requerida.';
      return;
    }

    if (!this.alias.trim() || this.alias.length > 50) {
      this.errorMessage = 'El alias debe tener entre 1 y 50 caracteres.';
      return;
    }

    if (!this.nombreEquipo.trim() || this.nombreEquipo.length > 100) {
      this.errorMessage = 'El nombre del equipo debe tener entre 1 y 100 caracteres.';
      return;
    }

    this.isLoading = true;

    setTimeout(() => {
      const success = this.simulateJoinLeague();
      
      if (success) {
        this.successMessage = '¡Te has unido exitosamente a la liga!';
        setTimeout(() => {
          this.router.navigate(['/mainpage/teams']);
        }, 2000);
      }
      
      this.isLoading = false;
    }, 1500);
  }

  simulateJoinLeague(): boolean {
    if (this.password !== '1234') {
      this.errorMessage = 'No se pudo unir a la liga. Verifica los datos e intenta nuevamente.';
      return false;
    }

    if (this.estado !== 'Activa') {
      this.errorMessage = 'La liga no está disponible en este momento.';
      return false;
    }

    if (this.cuposDisponibles <= 0) {
      this.errorMessage = 'La liga no tiene cupos disponibles.';
      return false;
    }

    const existingAliases = ['jugador1', 'el_crack', 'campeón'];
    if (existingAliases.includes(this.alias.toLowerCase())) {
      this.errorMessage = 'El alias ya está en uso. Por favor elige otro.';
      return false;
    }

    const existingTeams = ['Real Madrid Fantasy', 'Los Tigres', 'FC Winners'];
    if (existingTeams.includes(this.nombreEquipo)) {
      this.errorMessage = 'El nombre del equipo ya está en uso. Por favor elige otro.';
      return false;
    }

    return true;
  }

  onCancel(): void {
    this.router.navigate(['/mainpage/teams']);
  }
}