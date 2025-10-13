import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
  imports: [RouterModule, ReactiveFormsModule, CommonModule]
})
export class Register implements OnInit {
  registerForm!: FormGroup;
  submissionError: string | null = null;

  constructor(private fb: FormBuilder, private authService: Authservice) { }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      nombreCompleto: ['', [Validators.required, Validators.maxLength(50)]],
      alias: ['', [Validators.required, Validators.maxLength(50)]],
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

    const registroDto = {
      email: this.registerForm.get('email')?.value,
      password: this.registerForm.get('password')?.value,
      nombreCompleto: this.registerForm.get('nombreCompleto')?.value,
      alias: this.registerForm.get('alias')?.value
    };

    this.authService.register(registroDto).subscribe({
      next: response => {
        alert('Usuario registrado exitosamente');
        this.registerForm.reset();
      },
      error: err => {
        if (err.status === 400 && err.error?.mensaje) {
          this.submissionError = err.error.mensaje; // Muestra errores del backend, ej. email duplicado
        } else {
          this.submissionError = 'Error en el registro.';
        }
      }
    });
  }
}
