import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';


export interface Equipo {
  id: string;
  nombre: string;
  manager: string;
  estado: string;
  liga: string;
  imagenURL: string;
}

@Component({
  selector: 'app-teams',
  templateUrl: './teams.html',
  styleUrls: ['./teams.css'],
  imports: [CommonModule]
})
export class Teams {
  equipos: Equipo[] = [
    {
      id: "1",
      nombre: "Equipo A",
      manager: "Manager 1",
      estado: "Activo",
      liga: "NFL",
      imagenURL: "https://fabrikbrands.com/wp-content/uploads/NFL-Team-Logos-11-1200x750.png"
    },
    {
      id: "2",
      nombre: "Equipo B",
      manager: "Manager 2",
      estado: "Inactivo",
      liga: "NFL",
      imagenURL: "https://tse4.mm.bing.net/th/id/OIP.c9opj_wj613V0F013nG8owHaEo?cb=12&rs=1&pid=ImgDetMain&o=7&rm=3"
    },
    {
      id: "3",
      nombre: "Equipo C",
      manager: "Manager 3",
      estado: "Activo",
      liga: "NFL",
      imagenURL: "https://tse3.mm.bing.net/th/id/OIP.zklz8AHVfSZIRR-hxf9QaQHaFj?cb=12&rs=1&pid=ImgDetMain&o=7&rm=3"
    }
    // Agrega más equipos estáticos aquí
  ];
}
