import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EquipoService, EquipoResponseDto } from '../../services/equipo.service';
import { AuthService } from '../../services/authservice';

/**
 * Componente para mostrar y gestionar equipos
 */
@Component({
  selector: 'app-teams',
  templateUrl: './teams.html',
  styleUrls: ['./teams.css'],
  imports: [CommonModule, RouterModule]
})
export class Teams implements OnInit {
  equipos: EquipoResponseDto[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';
  baseUrl: string = 'http://localhost:5000';

  /**
   * Constructor del componente de equipos
   * @param equipoService Servicio para gestionar equipos
   * @param authService Servicio de autenticación
   */
  constructor(
    private equipoService: EquipoService,
    private authService: AuthService
  ) { }

  /**
   * Inicialización del componente
   * Carga los equipos del usuario actual
   */
  ngOnInit(): void {
    this.cargarEquipos();
  }

  /**
   * Carga los equipos del usuario logueado desde el backend
   */
  cargarEquipos(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    
    if (!currentUser) {
      this.errorMessage = 'Debes iniciar sesión para ver tus equipos.';
      this.isLoading = false;
      return;
    }

    // Obtener equipos del usuario actual
    this.equipoService.obtenerEquiposPorUsuario(currentUser.id).subscribe({
      next: (equipos) => {
        console.log('Equipos cargados:', equipos);
        this.equipos = equipos;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar equipos:', error);
        this.isLoading = false;
        
        if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else {
          this.errorMessage = 'Error al cargar los equipos. Inténtalo de nuevo.';
        }
      }
    });
  }

  /**
   * Obtiene la URL completa de la imagen del equipo
   * @param imagenUrl URL relativa de la imagen
   * @returns URL completa o imagen por defecto
   */
  obtenerImagenUrl(imagenUrl: string | null): string {
    if (imagenUrl) {
      return `${this.baseUrl}${imagenUrl}`;
    }
    // Imagen por defecto si no hay imagen
    return 'https://via.placeholder.com/150?text=Sin+Imagen';
  }
}