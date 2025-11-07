import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Authservice } from '../services/authservice';
import { EquipoService, EquipoResponseDto } from '../services/equipo.service';

/**
 * Componente de perfil de usuario
 * Muestra información del usuario logueado y sus equipos
 */
@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './perfil.html',
  styleUrls: ['./perfil.css']
})
export class Perfil implements OnInit {
  usuario: any = null;
  equipos: EquipoResponseDto[] = [];
  isLoadingEquipos: boolean = true;
  errorMessage: string = '';
  baseUrl: string = 'http://localhost:5000';

  constructor(
    private authService: Authservice,
    private equipoService: EquipoService
  ) {}

  ngOnInit(): void {
    // Obtener usuario actual
    this.usuario = this.authService.currentUserValue;

    // Cargar equipos del usuario
    if (this.usuario) {
      this.cargarEquipos();
    }
  }

  /**
   * Carga los equipos del usuario desde el backend
   */
  cargarEquipos(): void {
    this.isLoadingEquipos = true;
    this.errorMessage = '';

    this.equipoService.obtenerEquiposPorUsuario(this.usuario.id).subscribe({
      next: (equipos) => {
        this.equipos = equipos;
        this.isLoadingEquipos = false;
      },
      error: (error) => {
        console.error('Error al cargar equipos:', error);
        this.isLoadingEquipos = false;
        this.errorMessage = 'Error al cargar los equipos';
      }
    });
  }

  /**
   * Obtiene la URL completa de la imagen del equipo
   */
  obtenerImagenUrl(imagenUrl: string | null): string {
    if (imagenUrl) {
      return `${this.baseUrl}${imagenUrl}`;
    }
    return 'https://via.placeholder.com/150?text=Sin+Imagen';
  }

  /**
   * Cierra la sesión del usuario
   */
  logout(): void {
    this.authService.logout();
  }

  /**
   * Formatea la fecha de registro
   */
  formatearFecha(fecha: Date): string {
    return new Date(fecha).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}