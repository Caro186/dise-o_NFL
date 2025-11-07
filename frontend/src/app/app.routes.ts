import { Routes } from '@angular/router';
import { Login } from '../loginwidgets/login/login';
import { Register } from '../loginwidgets/register/register';
import { Sidenav } from '../mainpage/sidenav/sidenav';
import { Teams } from '../mainpage/teams/teams';
import { Userform } from '../mainpage/userform/userform';
import { TemporadaComponent } from '../mainpage/temporada/temporada';
import { Perfil } from '../perfil/perfil';
import { authGuard } from '../guards/auth.guard';
import { CrearLiga } from '../mainpage/crear-liga/crear-liga';
import { BuscarUnirseLiga } from '../mainpage/buscar-unirse-liga/buscar-unirse-liga';

export const routes: Routes = [
  {
    path: '',
    component: Login
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: 'mainpage',
    component: Sidenav,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'perfil',
        pathMatch: 'full'
      },
      {
        path: 'perfil',
        component: Perfil
      },
      {
        path: 'teams',
        component: Teams
      },
      {
        path: 'form',
        component: Userform
      },
      {
        path: 'temporada',
        component: TemporadaComponent
      },
      {
        path: 'crear-liga',
        component: CrearLiga
      },
      {
        path: 'buscar-liga',
        component: BuscarUnirseLiga
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];