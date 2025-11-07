import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Authservice } from '../../services/authservice';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

/**
 * Componente de login para autenticación de usuarios
 */
@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  imports: [ReactiveFormsModule, CommonModule, RouterModule]
})
export class Login {
  loginForm: FormGroup;
  serverError: string = '';

  /**
   * Constructor del componente de login
   * @param fb FormBuilder para crear formularios reactivos
   * @param authService Servicio de autenticación
   * @param router Router para navegación
   */
  constructor(
    private fb: FormBuilder,
    private authService: Authservice,
    private router: Router
  ) {
    // Si ya está logueado, redirigir a mainpage
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/mainpage']);
    }

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(12), this.passwordValidator]]
    });
  }

  /**
   * Validación personalizada de contraseña
   * Debe ser alfanumérica con al menos una mayúscula y una minúscula
   * @param control Control del formulario a validar
   * @returns Objeto con el error o null si es válido
   */
  passwordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value || '';
    const hasUpper = /[A-Z]/.test(value);
    const hasLower = /[a-z]/.test(value);
    const hasAlphaNum = /^[a-zA-Z0-9]+$/.test(value);
    
    if (!hasUpper || !hasLower || !hasAlphaNum) {
      return { passwordInvalid: true };
    }
    return null;
  }

  /**
   * Método para iniciar sesión
   * Valida el formulario y realiza la petición de login al backend
   */
  onSubmit(): void {
    this.serverError = '';
    
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const credentials = {
      email: this.loginForm.get('email')?.value,
      password: this.loginForm.get('password')?.value
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        console.log('Login exitoso:', response);
        
        if (response.status === 'ok') {
          // Redirigir a la página principal
          this.router.navigate(['/mainpage']);
        } else {
          this.serverError = 'Usuario o contraseña incorrectos';
        }
      },
      error: (error) => {
        console.error('Error en login:', error);
        
        // Manejar diferentes tipos de errores
        if (error.status === 401) {
          this.serverError = 'Usuario o contraseña incorrectos';
        } else if (error.status === 403) {
          // Cuenta bloqueada
          this.serverError = error.error?.mensaje || 
            'Tu cuenta ha sido bloqueada por múltiples intentos fallidos. Por favor, contacta al administrador.';
        } else if (error.status === 0) {
          this.serverError = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else if (error.error && error.error.mensaje) {
          this.serverError = error.error.mensaje;
        } else {
          this.serverError = 'Error al conectar con el servidor';
        }
      }
    });
  }
}