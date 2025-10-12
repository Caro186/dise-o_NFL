import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule } from '@angular/router'; // Importa RouterModule
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrl: './register.css',
  imports: [RouterModule,ReactiveFormsModule,CommonModule]
})
export class Register implements OnInit {
  registerForm!: FormGroup;
  submissionError: string | null = null;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void { //metodo oninit, para enviar los datos al form , genera un grupo de validacion
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
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

  // Validador personalizado para confirmar que password y confirmPassword coinciden
  passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      return { mismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    this.submissionError = null;

    if (this.registerForm.invalid) {
      // Marcar todos los campos como tocados para mostrar errores
      this.registerForm.markAllAsTouched();
      return;
    }

    // Aquí iría la lógica para enviar los datos al backend (CAMBIAR)

    // Simulación de respuesta error backend para correo duplicado (como ejemplo):
    const email = this.registerForm.get('email')?.value;
    if (email === 'existing@example.com') { // simular correo duplicado
      this.submissionError = 'The email address is already registered.';
      return;
    }

    // Si no hay errores, proceder con registro (simulación)
    console.log('User registered:', this.registerForm.value);
    alert('Registration successful!');
    this.registerForm.reset();
  }
}
