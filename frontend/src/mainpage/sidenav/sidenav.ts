import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidenav.html',
  styleUrls: ['./sidenav.css']
})
export class SidenavComponent implements OnInit {
  isAdmin: boolean = false;
  usuario: any = null;
  menuCollapsed: boolean = false;

  constructor(
    public authService: Authservice,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.usuario = this.authService.currentUserValue;
    this.isAdmin = this.authService.isAdmin();
  }

  /**
   * Cierra sesión
   */
  logout(): void {
    if (confirm('¿Estás seguro de cerrar sesión?')) {
      this.authService.logout();
    }
  }

  /**
   * Alterna el estado del menú (colapsado/expandido)
   */
  toggleMenu(): void {
    this.menuCollapsed = !this.menuCollapsed;
  }
}