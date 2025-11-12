import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LigaService, LigaResponseDto } from '../../services/liga.service';
import { Authservice } from '../../services/authservice';

interface LigaConRol extends LigaResponseDto {
  esComisionado: boolean;
}

/**
 * Componente para mostrar las ligas del usuario
 */
@Component({
  selector: 'app-liga',
  imports: [CommonModule, RouterModule],
  templateUrl: './liga.html',
  styleUrl: './liga.css'
})
export class Liga implements OnInit {
  ligas: LigaConRol[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';
  
  constructor(
    private ligaService: LigaService,
    private authService: Authservice
  ) { }

  ngOnInit(): void {
    this.cargarLigas();
  }

  /**
   * Carga todas las ligas del usuario (como comisionado y como participante)
   */
  cargarLigas(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const currentUser = this.authService.currentUserValue;
    
    if (!currentUser) {
      this.errorMessage = 'Debes iniciar sesión para ver tus ligas.';
      this.isLoading = false;
      return;
    }

    // Usar el nuevo endpoint que obtiene todas las ligas del usuario
    this.ligaService.obtenerPorUsuario(currentUser.id).subscribe({
      next: (ligasDelUsuario) => {
        // Mapear las ligas indicando si el usuario es comisionado
        this.ligas = ligasDelUsuario.map(liga => ({
          ...liga,
          esComisionado: liga.comisionadoId === currentUser.id  // ✅ Ahora usa comisionadoId
        }));
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar ligas:', error);
        this.isLoading = false;
        
        if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else {
          this.errorMessage = 'Error al cargar las ligas. Inténtalo de nuevo.';
        }
      }
    });
  }

  /**
   * Obtiene la clase CSS del badge según el estado de la liga
   */
  getBadgeClass(estado: string): string {
    switch (estado) {
      case 'Pre-Draft': return 'bg-info';
      case 'En Draft': return 'bg-warning text-dark';
      case 'Activa': return 'bg-success';
      case 'Finalizada': return 'bg-secondary';
      default: return 'bg-secondary';
    }
  }

  /**
   * Calcula los cupos disponibles
   */
  getCuposDisponibles(liga: LigaResponseDto): number {
    return liga.cuposTotales - liga.cuposOcupados;
  }

  /**
   * Obtiene la URL de la imagen o placeholder
   */
  obtenerImagenUrl(imagenUrl: string | null | undefined): string {
    if (imagenUrl) {
      return imagenUrl;
    }
    return 'https://via.placeholder.com/150?text=Liga';
  }
}