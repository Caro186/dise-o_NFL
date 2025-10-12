import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-sidenav',
  imports: [RouterOutlet,
    NgClass,
    RouterLink],
  templateUrl: './sidenav.html',
  styleUrl: './sidenav.css'
})
export class Sidenav {
  constructor(private router: Router) {}
  toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    if (sidebar) {
      sidebar.classList.toggle("expand");
    }
  }
}
