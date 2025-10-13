import { Routes } from '@angular/router';
import { Login } from '../loginwidgets/login/login';
import { Register } from '../loginwidgets/register/register';
import { Sidenav } from '../mainpage/sidenav/sidenav';
import { Teams } from '../mainpage/teams/teams';
import { Userform } from '../mainpage/userform/userform';


export const routes: Routes = [
    { path: '', component: Login },
    {path: 'register' , component: Register },
    {
    path: 'mainpage',
    component: Sidenav,
    children: [
      {
        path: 'teams',
        component: Teams
      }
    ]},
  {path: 'form' , component:Userform}
];
