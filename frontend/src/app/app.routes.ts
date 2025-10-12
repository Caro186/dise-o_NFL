import { Routes } from '@angular/router';
import { Login } from '../loginwidgets/login/login';
import { Register } from '../loginwidgets/register/register';
import { Sidenav } from '../mainpage/sidenav/sidenav';
export const routes: Routes = [
    { path: '', component: Login },
    {path: 'register' , component: Register },
    {path: 'mainpage' , component: Sidenav }
];
