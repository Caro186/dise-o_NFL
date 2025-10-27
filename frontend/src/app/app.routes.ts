import { Routes } from '@angular/router';
import { LoginComponent } from '../loginwidgets/login/login';
import { Register } from '../loginwidgets/register/register';
import { Sidenav } from '../mainpage/sidenav/sidenav';
import { Teams } from '../mainpage/teams/teams';
import { Userform } from '../mainpage/userform/userform';
import { authGuard } from '../guards/auth.guard';
import { CrearLigaComponent } from '../mainpage/liga/liga';
import { JoinLeagueComponent } from '../mainpage/join-league/join-league';

/**
 * Configuración de rutas de la aplicación
 */
export const routes: Routes = [
  // Rutas públicas (sin autenticación)
  { 
    path: '', 
    component: LoginComponent
  },
  { 
    path: 'register', 
    component: Register 
  },

  // Rutas principales de la aplicación (con sidenav)
  {
    path: 'mainpage',
    component: Sidenav,
    //canActivate: [authGuard], // Descomenta cuando esté listo
    children: [
      {
        path: '',
        redirectTo: 'teams',
        pathMatch: 'full'
      },
      {
        path: 'teams',
        component: Teams
      },
      {
        path: 'teams/create',
        component: Userform
      },
      {
        path: 'liga',
        component: CrearLigaComponent
      },
      {
        path: 'ligas',
        component: JoinLeagueComponent // Lista de ligas disponibles (si lo implementas)
      },
      {
        path: 'ligas/:id/join',
        component: JoinLeagueComponent // Unirse a una liga específica
      }
    ]
  },

  // Ruta alternativa para crear equipo (fuera del sidenav si es necesario)
  { 
    path: 'form', 
    component: Userform,
    //canActivate: [authGuard] // Descomenta cuando esté listo
  },

  // Ruta alternativa para crear liga (fuera del sidenav si es necesario)
  {
    path: 'crear-liga',
    component: CrearLigaComponent,
    //canActivate: [authGuard] // Descomenta cuando esté listo
  },

  // Redirección de rutas no encontradas
  {
    path: '**',
    redirectTo: ''
  }
];