import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgClass, CommonModule } from '@angular/common';
import { Authservice, UsuarioDto } from '../../services/authservice';

/**
 * Componente de navegación lateral principal
 */
@Component({
  selector: 'app-sidenav',
  imports: [
    RouterOutlet,
    RouterLink,
    CommonModule
  ],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.css'
})
export class Sidenav implements OnInit {
  currentUser: UsuarioDto | null = null;

  /**
   * Constructor del componente de navegación
   * @param router Router para navegación
   * @param authService Servicio de autenticación
   */
  constructor(
    private router: Router,
    private authService: Authservice
  ) {}

  /**
   * Inicialización del componente
   * Obtiene el usuario actual
   */
  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  /**
   * Toggle del sidebar (expandir/contraer)
   */
  toggleSidebar(): void {
    const sidebar = document.getElementById("sidebar");
    if (sidebar) {
      sidebar.classList.toggle("expand");
    }
  }

  /**
   * Cierra la sesión del usuario actual
   */
  logout(): void {
    if (confirm('¿Estás seguro que deseas cerrar sesión?')) {
      this.authService.logout();
      this.router.navigate(['/']);
    }
  }

  /**
   * Obtiene las iniciales del nombre del usuario
   * @returns Iniciales del usuario o '?' si no hay usuario
   */
  getUserInitials(): string {
    if (!this.currentUser) {
      return '?';
    }
    const names = this.currentUser.nombreCompleto.split(' ');
    if (names.length >= 2) {
      return names[0][0] + names[1][0];
    }
    return names[0][0];
  }
}