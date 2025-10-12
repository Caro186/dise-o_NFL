import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Authservice } from '../services/authservice';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule} from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  imports: [ReactiveFormsModule, CommonModule]
})
export class Login {
  loginForm: FormGroup;
  serverError: string = '';
  constructor(
    private fb: FormBuilder,
    private authService: Authservice,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(12), this.passwordValidator]]
    });
  }
  // Validación personalizada de contraseña: alfanumérica, al menos una min, una mayúscula
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
  // Método para iniciar sesión
  onSubmit() {
    this.serverError = '';
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    const credentials = this.loginForm.value;
    this.authService.login(credentials).subscribe({
      next: (res) => {
        if(res.status === 'ok') {
          // Redirigir al perfil del jugador
          this.router.navigate(['/perfil']);
        } else {
          // Mensaje genérico por credenciales inválidas
          this.serverError = 'Usuario o contraseña incorrectos';
        }
      },
      error: (err) => {
        this.serverError = 'Error al conectar con el servidor';
      }
    });
  }
}
