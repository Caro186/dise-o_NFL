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
import { authGuard } from '../guards/auth.guard';
import { adminGuard } from '../guards/admin.guard';

export const routes: Routes = [
    // ===== Rutas públicas =====
    { path: '', component: Login },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    
    // ===== Rutas protegidas =====
    {
        path: 'mainpage',
        component: Mainpage,
        canActivate: [authGuard],
        children: [
            { path: '', redirectTo: 'perfil', pathMatch: 'full' },
            
            // Rutas de usuarios normales
            { path: 'perfil', component: Perfil },
            { path: 'equipos-fantasy', component: EquipoFantasyList },
            { path: 'equipos-fantasy/crear', component: EquiposFantasyForm },
            { path: 'liga', component: Liga },
            
         
            { 
                path: 'admin/equipos-nfl', 
                component: EquiposNFLListComponent,
                canActivate: [adminGuard] 
            },
            { 
                path: 'admin/equipos-nfl/crear', 
                component: EquiposNFLFormComponent,
                canActivate: [adminGuard]
            },
            { 
                path: 'temporada', 
                component: TemporadaComponent,
                canActivate: [adminGuard] 
            }
        ]
    },
    
    { path: '**', redirectTo: 'login' }
];