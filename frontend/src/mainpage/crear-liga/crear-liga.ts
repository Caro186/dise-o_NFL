import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LigaResponse, LigaService } from '../../services/liga.service';

import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-crear-liga',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './crear-liga.html',
  styleUrls: ['./crear-liga.css']
})
export class CrearLiga implements OnInit {
  crearLigaForm: FormGroup;
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  currentUser: any = null;

  cantidadesEquipos = [4, 6, 8, 10, 12, 14, 16, 18, 20];
  opcionesPlayoffs = [
    { valor: 4, texto: '4 equipos (Semanas 16-17)' },
    { valor: 6, texto: '6 equipos (Semanas 16-17-18)' }
  ];

  constructor(
    private fb: FormBuilder,
    private ligaService: LigaService,
    private authService: Authservice,
    private router: Router
  ) {
    this.crearLigaForm = this.fb.group({
      nombreLiga: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      descripcion: [''],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(12), this.passwordValidator]],
      cantidadEquipos: [10, [Validators.required]],
      nombreEquipoComisionado: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      equiposEnPlayoffs: [4, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    if (!this.currentUser) {
      this.router.navigate(['/']);
    }
  }

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

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (this.crearLigaForm.invalid) {
      this.crearLigaForm.markAllAsTouched();
      this.errorMessage = 'Por favor completa todos los campos correctamente';
      return;
    }

    this.isLoading = true;

    const ligaData = {
      nombreLiga: this.crearLigaForm.get('nombreLiga')?.value,
      descripcion: this.crearLigaForm.get('descripcion')?.value || null,
      password: this.crearLigaForm.get('password')?.value,
      cantidadEquipos: this.crearLigaForm.get('cantidadEquipos')?.value,
      idComisionado: this.currentUser.id,
      nombreEquipoComisionado: this.crearLigaForm.get('nombreEquipoComisionado')?.value,
      equiposEnPlayoffs: this.crearLigaForm.get('equiposEnPlayoffs')?.value
    };

    this.ligaService.crearLiga(ligaData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = `¡Liga "${response.nombreLiga}" creada exitosamente! Cupos disponibles: ${response.cuposDisponibles}`;
        
        setTimeout(() => {
          this.router.navigate(['/mainpage/liga', response.idLiga]);
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Error al crear liga:', error);
        
        if (error.error && error.error.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else if (error.status === 400) {
          this.errorMessage = 'Datos inválidos. Verifica que todos los campos estén correctos.';
        } else if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor. Verifica que el backend esté corriendo.';
        } else {
          this.errorMessage = 'Error al crear la liga. Por favor intenta nuevamente.';
        }
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/mainpage']);
  }
}