import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Authservice } from '../../services/authservice';

/**
 * Componente de registro para crear nuevos usuarios
 */
@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
  imports: [RouterModule, ReactiveFormsModule, CommonModule]
})
export class Register implements OnInit {
  registerForm!: FormGroup;
  submissionError: string | null = null;

  /**
   * Constructor del componente de registro
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
  }

  /**
   * Inicialización del componente
   * Configura el formulario de registro con sus validaciones
   */
  ngOnInit(): void {
    this.registerForm = this.fb.group({
      nombreCompleto: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      password: ['', [
      Validators.required,
      Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[a-zA-Z0-9]{8,12}$')
    ]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordsMatchValidator
    });
  }

  /**
   * Validador personalizado para verificar que las contraseñas coincidan
   * @param group Grupo de controles del formulario
   * @returns Objeto con el error o null si las contraseñas coinciden
   */
  passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password !== confirmPassword ? { mismatch: true } : null;
  }

  /**
   * Método para registrar un nuevo usuario
   * Valida el formulario y realiza la petición de registro al backend
   */
  onSubmit(): void {
    this.submissionError = null;

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const registroDto = {
      email: this.registerForm.get('email')?.value,
      password: this.registerForm.get('password')?.value,
      nombreCompleto: this.registerForm.get('nombreCompleto')?.value
    };

    this.authService.register(registroDto).subscribe({
      next: (response) => {
        console.log('Registro exitoso:', response);
        alert('Usuario registrado exitosamente. Ahora puedes iniciar sesión.');
        
        // Resetear formulario y redirigir al login
        this.registerForm.reset();
        this.router.navigate(['/']);
      },
      error: (error) => {
        console.error('Error en registro:', error);
        
        // Manejar diferentes tipos de errores
        if (error.status === 400 && error.error?.mensaje) {
          this.submissionError = error.error.mensaje;
        } else if (error.status === 0) {
          this.submissionError = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else if (error.error?.errores && error.error.errores.length > 0) {
          this.submissionError = error.error.errores.join(', ');
        } else {
          this.submissionError = 'Error en el registro. Inténtalo de nuevo.';
        }
      }
    });
  }
}