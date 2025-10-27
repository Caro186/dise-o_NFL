import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService, Usuario } from '../../services/authservice';
import { Observable } from 'rxjs';

interface RegistroDto {
  nombreCompleto: string;
  email: string;
  password: string;
}

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
  standalone: true,
  imports: [RouterModule, ReactiveFormsModule, CommonModule]
})
export class Register implements OnInit {
  registerForm!: FormGroup;
  submissionError: string | null = null;

  constructor(
    private fb: FormBuilder, 
    private authService: AuthService,
    private router: Router
  ) {
    // Redirigir si ya está logueado
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/mainpage']);
    }
  }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      nombreCompleto: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(50)]],
      password: ['', [
        Validators.required,
        Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]{8,12}$')
      ]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordsMatchValidator
    });
  }

  passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password !== confirmPassword ? { mismatch: true } : null;
  }

  onSubmit(): void {
    this.submissionError = null;

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const registroDto: RegistroDto = {
      email: this.registerForm.get('email')?.value,
      password: this.registerForm.get('password')?.value,
      nombreCompleto: this.registerForm.get('nombreCompleto')?.value
    };

    this.authService.register(registroDto).subscribe({
      next: (response) => {
        console.log('Registro exitoso:', response);
        alert('Usuario registrado exitosamente. Ahora puedes iniciar sesión.');
        this.registerForm.reset();
        this.router.navigate(['/']);
      },
      error: (error: any) => {
        console.error('Error en registro:', error);
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