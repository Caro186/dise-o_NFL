import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Authservice, UsuarioDto } from '../services/authservice';
import { EquipoFantasyService, EquipoFantasyResponseDto } from '../services/equipo-fantasy.service';
import { LigaService, LigaResponseDto } from '../services/liga.service';

@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './perfil.html',
  styleUrls: ['./perfil.css']
})
export class Perfil implements OnInit {
  usuario: UsuarioDto | null = null;
  equiposFantasy: EquipoFantasyResponseDto[] = [];
  ligasComisionadas: LigaResponseDto[] = [];
  isLoadingEquipos: boolean = true;
  isLoadingLigas: boolean = true;
  errorEquipos: string = '';
  errorLigas: string = '';

  constructor(
    public authService: Authservice,  // ✅ Cambiado de private a public
    private equipoFantasyService: EquipoFantasyService,
    private ligaService: LigaService
  ) {}

  ngOnInit(): void {
    this.usuario = this.authService.currentUserValue;
    if (this.usuario) {
      this.cargarDatos();
    }
  }

  /**
   * Carga todos los datos del usuario
   */
  cargarDatos(): void {
    if (!this.usuario) return;

    this.cargarEquiposFantasy();
    this.cargarLigasComisionadas();
  }

  /**
   * Carga los equipos fantasy del usuario
   */
  cargarEquiposFantasy(): void {
    if (!this.usuario) return;

    this.isLoadingEquipos = true;
    this.errorEquipos = '';

    this.equipoFantasyService.obtenerPorUsuario(this.usuario.id).subscribe({
      next: (equipos) => {
        this.equiposFantasy = equipos;
        this.isLoadingEquipos = false;
      },
      error: (error) => {
        console.error('Error al cargar equipos:', error);
        this.errorEquipos = 'Error al cargar los equipos';
        this.isLoadingEquipos = false;
      }
    });
  }

  /**
   * Carga las ligas donde el usuario es comisionado
   */
  cargarLigasComisionadas(): void {
    if (!this.usuario) return;

    this.isLoadingLigas = true;
    this.errorLigas = '';

    this.ligaService.obtenerLigasPorComisionado(this.usuario.id).subscribe({
      next: (ligas) => {
        this.ligasComisionadas = ligas;
        this.isLoadingLigas = false;
        console.log('Ligas comisionadas:', ligas);
      },
      error: (error) => {
        console.error('Error al cargar ligas:', error);
        this.errorLigas = 'Error al cargar las ligas';
        this.isLoadingLigas = false;
      }
    });
  }

  /**
   * Obtiene el badge de color según el estado de la liga
   */
  getBadgeClass(estado: string): string {
    switch (estado) {
      case 'Pre-Draft': return 'bg-info';
      case 'En Draft': return 'bg-warning';
      case 'Activa': return 'bg-success';
      case 'Finalizada': return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  /**
   * Cierra la sesión del usuario
   */
  logout(): void {
    this.authService.logout();
  }
}