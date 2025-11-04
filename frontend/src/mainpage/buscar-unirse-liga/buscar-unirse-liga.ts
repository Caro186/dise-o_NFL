import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LigaService, LigaResponse } from '../../services/liga.service';
import { Authservice } from '../../services/authservice';

@Component({
  selector: 'app-buscar-unirse-liga',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './buscar-unirse-liga.html',
  styleUrls: ['./buscar-unirse-liga.css']
})
export class BuscarUnirseLiga implements OnInit {
  buscarForm: FormGroup;
  unirseForm: FormGroup;
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  currentUser: any = null;
  
  ligasEncontradas: LigaResponse[] = [];
  ligaSeleccionada: LigaResponse | null = null;
  mostrarFormularioUnirse: boolean = false;

  constructor(
    private fb: FormBuilder,
    private ligaService: LigaService,
    private authService: Authservice,
    private router: Router
  ) {
    this.buscarForm = this.fb.group({
      nombreBusqueda: ['', [Validators.required, Validators.minLength(1)]]
    });

    this.unirseForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(12), this.passwordValidator]],
      alias: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(50)]],
      nombreEquipo: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    if (!this.currentUser) {
      this.router.navigate(['/']);
    }
    this.buscarLigas();
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

  buscarLigas(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.ligaService.obtenerTodasLasLigas().subscribe({
      next: (ligas) => {
        const nombreBusqueda = this.buscarForm.get('nombreBusqueda')?.value?.toLowerCase() || '';
        
        if (nombreBusqueda) {
          this.ligasEncontradas = ligas.filter(liga => 
            liga.nombreLiga.toLowerCase().includes(nombreBusqueda) &&
            liga.cuposDisponibles > 0 &&
            liga.estado !== 'Finalizada'
          );
        } else {
          this.ligasEncontradas = ligas.filter(liga =>
            liga.cuposDisponibles > 0 &&
            liga.estado !== 'Finalizada'
          );
        }

        this.isLoading = false;

        if (this.ligasEncontradas.length === 0) {
          this.errorMessage = nombreBusqueda 
            ? 'No se encontraron ligas con ese nombre que tengan cupos disponibles'
            : 'No hay ligas disponibles en este momento';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Error al buscar ligas';
        console.error('Error:', error);
      }
    });
  }

  seleccionarLiga(liga: LigaResponse): void {
    this.ligaSeleccionada = liga;
    this.mostrarFormularioUnirse = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.unirseForm.reset();
  }

  cancelarUnirse(): void {
    this.mostrarFormularioUnirse = false;
    this.ligaSeleccionada = null;
    this.unirseForm.reset();
  }

  onUnirse(): void {
    if (this.unirseForm.invalid || !this.ligaSeleccionada) {
      this.unirseForm.markAllAsTouched();
      this.errorMessage = 'Por favor completa todos los campos correctamente';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const unirseData = {
      idLiga: this.ligaSeleccionada.idLiga,
      idUsuario: this.currentUser.id,
      password: this.unirseForm.get('password')?.value,
      alias: this.unirseForm.get('alias')?.value.trim(),
      nombreEquipo: this.unirseForm.get('nombreEquipo')?.value.trim()
    };

    this.ligaService.unirseALiga(unirseData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = `¡Te has unido exitosamente a la liga "${this.ligaSeleccionada?.nombreLiga}"!`;
        
        setTimeout(() => {
          this.router.navigate(['/mainpage/liga', this.ligaSeleccionada?.idLiga]);
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Error al unirse a liga:', error);
        
        if (error.error && error.error.mensaje) {
          this.errorMessage = error.error.mensaje;
        } else if (error.status === 403) {
          this.errorMessage = 'Contraseña incorrecta';
        } else if (error.status === 400) {
          this.errorMessage = error.error?.mensaje || 'No puedes unirte a esta liga';
        } else if (error.status === 0) {
          this.errorMessage = 'No se puede conectar con el servidor';
        } else {
          this.errorMessage = 'Error al unirse a la liga';
        }
      }
    });
  }
}