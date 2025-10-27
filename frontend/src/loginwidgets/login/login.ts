import { Component, OnInit, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService, LoginDto, LoginResponse, Usuario } from '../../services/authservice';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  serverError: string = '';
  isLoading: boolean = false;
  cuentaBloqueada: boolean = false;

  private failedAttempts: number = 0;
  private sessionTimeout: any;
  private readonly SESSION_DURATION = 12 * 60 * 60 * 1000; // 12 horas

  constructor(
    private authService: AuthService, 
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    // Inicializar el formulario
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(12),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]{8,12}$/)
      ]]
    });

    // Si ya hay sesión activa
    const token = localStorage.getItem('token');
    const lastActivity = Number(localStorage.getItem('lastActivity') || '0');
    if (token && Date.now() - lastActivity < this.SESSION_DURATION) {
      this.startSessionTimer();
      this.router.navigate(['/perfil']);
    }
  }

  /** Reset temporizador al interactuar */
  @HostListener('document:click')
  @HostListener('document:keydown')
  resetSessionTimer(): void {
    if (localStorage.getItem('token')) {
      localStorage.setItem('lastActivity', Date.now().toString());
      clearTimeout(this.sessionTimeout);
      this.startSessionTimer();
    }
  }

  /** Temporizador de expiración */
  private startSessionTimer(): void {
    this.sessionTimeout = setTimeout(() => {
      localStorage.removeItem('token');
      localStorage.removeItem('lastActivity');
      localStorage.removeItem('usuario');
      alert('Tu sesión ha expirado por inactividad.');
      this.router.navigate(['/login']);
    }, this.SESSION_DURATION);
  }

  /** Envío del formulario */
  onSubmit(): void {
    this.serverError = '';

    // Validar formulario
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    const loginDto: LoginDto = {
      email: this.loginForm.get('email')?.value,
      password: this.loginForm.get('password')?.value

      
    };

    this.authService.login(loginDto).subscribe({
      next: (response: LoginResponse) => {
        this.isLoading = false;

        if (response.status === 'ok') {
          // Login exitoso
          this.failedAttempts = 0;

          // Guardar token y fecha de actividad
          localStorage.setItem('token', response.token);
          localStorage.setItem('lastActivity', Date.now().toString());
          localStorage.setItem('usuario', JSON.stringify(response.usuario));

          this.startSessionTimer();
          this.router.navigate(['/mainpage']);
        } else if (response.usuario.estado === 'bloqueada') {
          this.cuentaBloqueada = true;
          this.serverError = 'Tu cuenta ha sido bloqueada por exceso de intentos fallidos.';
        }
      },
      error: (error) => {
        this.isLoading = false;

        if (error.message.includes('bloqueada')) {
          this.cuentaBloqueada = true;
          this.serverError = error.message;
        } else {
          this.handleFailedAttempt();
        }
      }
    });
  }

  /** Manejo de intentos fallidos */
  private handleFailedAttempt(): void {
    this.failedAttempts++;
    if (this.failedAttempts >= 5) {
      this.cuentaBloqueada = true;
      this.serverError = 'Cuenta bloqueada tras 5 intentos fallidos.';
    } else {
      this.serverError = 'Credenciales inválidas.';
    }
  }

  /** Limpia mensajes al escribir */
  onInputChange(): void {
    this.serverError = '';
    this.cuentaBloqueada = false;
  }

  /** Redirige a registro */
  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}