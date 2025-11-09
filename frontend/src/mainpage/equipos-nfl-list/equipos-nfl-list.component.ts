import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { EquipoNFLService, EquipoNFLResponseDto } from '../../services/equipo-nfl.service';

/**
 * Componente para listar equipos NFL (solo administradores)
 */
@Component({
  selector: 'app-equipos-nfl-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './equipos-nfl-list.component.html',
  styleUrls: ['./equipos-nfl-list.component.css']
})
export class EquiposNFLListComponent implements OnInit {
  equipos: EquipoNFLResponseDto[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(private equipoNFLService: EquipoNFLService) { }

  ngOnInit(): void {
    this.cargarEquipos();
  }

  /**
   * Carga todos los equipos NFL desde el backend
   */
  cargarEquipos(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.equipoNFLService.obtenerTodos().subscribe({
      next: (equipos) => {
        console.log('Equipos NFL cargados:', equipos);
        this.equipos = equipos;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar equipos NFL:', error);
        this.isLoading = false;
        this.errorMessage = 'Error al cargar los equipos NFL. Inténtalo de nuevo.';
      }
    });
  }

  /**
   * Elimina un equipo NFL
   */
  eliminarEquipo(id: number, nombre: string): void {
    if (confirm(`¿Estás seguro de eliminar el equipo "${nombre}"?`)) {
      this.equipoNFLService.eliminar(id).subscribe({
        next: () => {
          alert('Equipo eliminado exitosamente');
          this.cargarEquipos(); // Recargar la lista
        },
        error: (error) => {
          console.error('Error al eliminar equipo:', error);
          alert('Error al eliminar el equipo');
        }
      });
    }
  }
}