import { Routes } from '@angular/router';
import { Login } from '../loginwidgets/login/login';
import { Register } from '../loginwidgets/register/register';
import { Mainpage } from '../mainpage/mainpage.component';
import { Perfil } from '../perfil/perfil';
import { EquipoFantasyList } from '../mainpage/equipos-fantasy-list/equipos-fantasy-list';
import { EquiposFantasyForm } from '../mainpage/equipos-fantasy-form/equipos-fantasy-form';
import { Liga } from '../mainpage/liga/liga';
import { TemporadaComponent } from '../mainpage/temporada/temporada';
import { EquiposNFLListComponent } from '../mainpage/equipos-nfl-list/equipos-nfl-list.component';
import { EquiposNFLFormComponent } from '../mainpage/equipos-nfl-form/equipos-nfl-form.component';

export const routes: Routes = [
    // ===== Rutas públicas (sin autenticación) =====
    { path: '', component: Login },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    
    // ===== Rutas protegidas (requieren autenticación) =====
    // Mainpage es el layout principal con sidenav
    {
        path: 'mainpage',
        component: Mainpage,
        children: [
            // Ruta por defecto cuando entran a /mainpage
            { path: '', redirectTo: 'perfil', pathMatch: 'full' },
            
            // Perfil (página de inicio después del login)
            { path: 'perfil', component: Perfil },
            
            // ===== Equipos Fantasy (todos los usuarios) =====
            { path: 'equipos-fantasy', component: EquipoFantasyList },
            { path: 'equipos-fantasy/crear', component: EquiposFantasyForm },
            
            // ===== Ligas =====
            { path: 'liga', component: Liga },
            
            // ===== Administración (solo admins) =====
            { path: 'admin/equipos-nfl', component: EquiposNFLListComponent },
            { path: 'admin/equipos-nfl/crear', component: EquiposNFLFormComponent },
            { path: 'temporada', component: TemporadaComponent }
        ]
    },
    
    // Redirección por defecto
    { path: '**', redirectTo: 'login' }
];