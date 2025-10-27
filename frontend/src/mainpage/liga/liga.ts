import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LeagueService, CrearLigaDto } from '../../services/liga.service';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../services/authservice';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-crear-liga',
  templateUrl: './liga.html',
  styleUrls: ['./liga.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class CrearLigaComponent {
  crearLigaForm: FormGroup;
  submitting = false;
  errorMessage = '';
  successMessage = '';

  valoresPermitidosEquipos = [4, 6, 8, 10, 12, 14, 16, 18, 20];
  valoresPermitidosPlayoffs = [4, 6];

  constructor(
    private fb: FormBuilder,
    private leagueService: LeagueService,
    private authService: AuthService,
    private router: Router
  ) {
    this.crearLigaForm = this.fb.group({
      NombreLiga: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
      Descripcion: ['', [Validators.maxLength(500)]],
      Password: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,12}$/)
        ]
      ],
      CantidadEquipos: [4, [Validators.required, Validators.min(4), Validators.max(20)]],
      TipoPlayoffs: [4, Validators.required],
      NombreEquipo: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]]
    });
  }

  get f() {
    return this.crearLigaForm.controls;
  }

  onSubmit() {
    console.log('🚀 [CREAR LIGA] Iniciando envío de formulario');
    this.errorMessage = '';
    this.successMessage = '';

    // Validar formulario
    if (this.crearLigaForm.invalid) {
      console.warn('⚠️ [CREAR LIGA] Formulario inválido');
      this.crearLigaForm.markAllAsTouched();
      return;
    }

    // Obtener usuario actual
    const usuarioId = this.authService.getUsuarioId();
    console.log('👤 [CREAR LIGA] Usuario ID:', usuarioId);
    
    if (!usuarioId) {
      this.errorMessage = 'No se encontró el usuario autenticado.';
      return;
    }

    // IMPORTANTE: Convertir valores a números explícitamente
    const formValues = this.crearLigaForm.value;
    console.log('📝 [CREAR LIGA] Valores del formulario (RAW):', formValues);
    console.log('📝 [CREAR LIGA] Valor de CantidadEquipos:', formValues.CantidadEquipos, '| Tipo:', typeof formValues.CantidadEquipos);
    console.log('📝 [CREAR LIGA] Valor de TipoPlayoffs:', formValues.TipoPlayoffs, '| Tipo:', typeof formValues.TipoPlayoffs);

    // Convertir SIEMPRE a número, incluso si ya parece serlo
    const cantidadEquipos = parseInt(formValues.CantidadEquipos, 10);
    const tipoPlayoffs = parseInt(formValues.TipoPlayoffs, 10);

    console.log('🔄 [CREAR LIGA] Después de parseInt:');
    console.log('   CantidadEquipos:', cantidadEquipos, '| Tipo:', typeof cantidadEquipos);
    console.log('   TipoPlayoffs:', tipoPlayoffs, '| Tipo:', typeof tipoPlayoffs);

    const dto: CrearLigaDto = {
      nombreLiga: formValues.NombreLiga,
      descripcion: formValues.Descripcion || '',
      password: formValues.Password,
      cantidadEquipos: cantidadEquipos,
      tipoPlayoffs: tipoPlayoffs,
      nombreEquipo: formValues.NombreEquipo,
      usuarioId: usuarioId
    };

    console.log('📦 [CREAR LIGA] DTO preparado para enviar:', dto);
    console.log('📦 [CREAR LIGA] Tipo de cantidadEquipos (DTO):', typeof dto.cantidadEquipos);
    console.log('📦 [CREAR LIGA] Tipo de tipoPlayoffs (DTO):', typeof dto.tipoPlayoffs);

    // Validar cantidad de equipos
    console.log('🔍 [CREAR LIGA] Validando cantidad de equipos...');
    console.log('   Valor a validar:', dto.cantidadEquipos, '| Tipo:', typeof dto.cantidadEquipos);
    console.log('   Valores permitidos:', this.valoresPermitidosEquipos);
    console.log('   ¿Está incluido?', this.valoresPermitidosEquipos.includes(dto.cantidadEquipos));

    if (!this.valoresPermitidosEquipos.includes(dto.cantidadEquipos)) {
      this.errorMessage = `La cantidad de equipos debe ser 4, 6, 8, 10, 12, 14, 16, 18 o 20. Recibido: ${dto.cantidadEquipos} (tipo: ${typeof dto.cantidadEquipos})`;
      console.error('❌ [CREAR LIGA] Cantidad de equipos inválida:', dto.cantidadEquipos);
      return;
    }

    // Validar tipo de playoffs
    console.log('🔍 [CREAR LIGA] Validando tipo de playoffs...');
    console.log('   Valor a validar:', dto.tipoPlayoffs, '| Tipo:', typeof dto.tipoPlayoffs);
    console.log('   Valores permitidos:', this.valoresPermitidosPlayoffs);
    console.log('   ¿Está incluido?', this.valoresPermitidosPlayoffs.includes(dto.tipoPlayoffs));

    if (!this.valoresPermitidosPlayoffs.includes(dto.tipoPlayoffs)) {
      this.errorMessage = `El tipo de playoffs debe ser 4 o 6 equipos. Recibido: ${dto.tipoPlayoffs} (tipo: ${typeof dto.tipoPlayoffs})`;
      console.error('❌ [CREAR LIGA] Tipo de playoffs inválido:', dto.tipoPlayoffs);
      return;
    }

    console.log('✅ [CREAR LIGA] Validaciones pasadas, enviando al servidor...');
    this.submitting = true;

    this.leagueService.createLeague(dto).subscribe({
      next: (res: any) => {
        console.log('✅ [CREAR LIGA] Liga creada exitosamente:', res);
        this.successMessage = `Liga "${res.nombreLiga || res.NombreLiga}" creada exitosamente!`;

        // Resetear formulario con valores por defecto
        this.crearLigaForm.reset({
          CantidadEquipos: 4,
          TipoPlayoffs: 4
        });

        this.submitting = false;
        
        // Opcional: Redirigir a la liga creada después de 2 segundos
        setTimeout(() => {
          this.router.navigate(['/mainpage/liga']);
        }, 2000);
      },
      error: (err: HttpErrorResponse) => {
        console.error('❌ [CREAR LIGA] Error al crear liga:', err);
        console.error('❌ [CREAR LIGA] Error completo:', {
          status: err.status,
          statusText: err.statusText,
          error: err.error,
          message: err.message
        });
        
        this.errorMessage = err.error?.mensaje || 
                           err.error?.Mensaje || 
                           err.message || 
                           'Error al crear la liga';
        this.submitting = false;
      }
    });
  }
}