import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { NoticiaJugadorService, CrearNoticiaJugadorDto, JugadorConNoticiasDto } from '../../services/noticia-jugador.service';
import { JugadorService, JugadorListDto } from '../../services/jugadores.service';

@Component({
  selector: 'app-noticia-jugador',
  standalone: true,
  templateUrl: './noticia-jugador.html',
  styleUrls: ['./noticia-jugador.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class NoticiaJugadorComponent implements OnInit {
  noticiaForm: FormGroup;
  jugadores: JugadorListDto[] = [];
  jugadorSeleccionado: JugadorConNoticiasDto | null = null;
  designacionesDisponibles: { valor: string, descripcion: string }[] = [];

  mensajeExito: string = '';
  mensajeError: string = '';
  cargando: boolean = false;
  mostrarFormulario: boolean = false;

  constructor(
    private fb: FormBuilder,
    private noticiaService: NoticiaJugadorService,
    private jugadoresService: JugadorService
  ) {
    this.noticiaForm = this.fb.group({
      jugadorId: ['', Validators.required],
      texto: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(300)]],
      esLesion: [false],
      resumenLesion: [''],
      designacionLesion: ['']
    });

    // Escuchar cambios en el campo esLesion
    this.noticiaForm.get('esLesion')?.valueChanges.subscribe(esLesion => {
      this.actualizarValidacionesLesion(esLesion);
    });
  }

  ngOnInit(): void {
    this.cargarJugadores();
    this.designacionesDisponibles = this.noticiaService.obtenerDesignacionesDisponibles();
  }

  cargarJugadores(): void {
    this.cargando = true;
    this.jugadoresService.obtenerJugadores().subscribe({
      next: (response) => {
        this.jugadores = response || [];
        this.cargando = false;
      },
      error: (error) => {
        this.mensajeError = 'Error al cargar jugadores: ' + error.message;
        this.cargando = false;
      }
    });
  }

  onJugadorSeleccionado(event: any): void {
    const jugadorId = parseInt(event.target.value);
    if (jugadorId) {
      // Actualizar el campo jugadorId del formulario
      this.noticiaForm.patchValue({ jugadorId: jugadorId });
      this.cargarJugadorConNoticias(jugadorId);
      this.mostrarFormulario = true;
    } else {
      this.jugadorSeleccionado = null;
      this.mostrarFormulario = false;
      this.noticiaForm.patchValue({ jugadorId: '' });
    }
  }

  cargarJugadorConNoticias(jugadorId: number): void {
    this.cargando = true;
    this.noticiaService.obtenerJugadorConNoticias(jugadorId).subscribe({
      next: (jugador) => {
        this.jugadorSeleccionado = jugador;
        this.cargando = false;
      },
      error: (error) => {
        this.mensajeError = 'Error al cargar información del jugador: ' + error.message;
        this.cargando = false;
      }
    });
  }

  actualizarValidacionesLesion(esLesion: boolean): void {
    const resumenControl = this.noticiaForm.get('resumenLesion');
    const designacionControl = this.noticiaForm.get('designacionLesion');

    if (esLesion) {
      resumenControl?.setValidators([Validators.required, Validators.maxLength(30)]);
      designacionControl?.setValidators([Validators.required]);
    } else {
      resumenControl?.clearValidators();
      designacionControl?.clearValidators();
      resumenControl?.setValue('');
      designacionControl?.setValue('');
    }

    resumenControl?.updateValueAndValidity();
    designacionControl?.updateValueAndValidity();
  }

  crearNoticia(): void {
    this.mensajeExito = '';
    this.mensajeError = '';

    if (this.noticiaForm.invalid) {
      this.mensajeError = 'Por favor, complete todos los campos obligatorios correctamente';
      return;
    }

    const dto: CrearNoticiaJugadorDto = {
      jugadorId: parseInt(this.noticiaForm.value.jugadorId),
      texto: this.noticiaForm.value.texto.trim(),
      esLesion: this.noticiaForm.value.esLesion,
      resumenLesion: this.noticiaForm.value.esLesion ? this.noticiaForm.value.resumenLesion.trim() : undefined,
      designacionLesion: this.noticiaForm.value.esLesion ? this.noticiaForm.value.designacionLesion : undefined
    };

    this.cargando = true;

    this.noticiaService.crearNoticia(dto).subscribe({
      next: (response) => {
        this.mensajeExito = response.mensaje;
        this.mensajeError = '';

        // Recargar las noticias del jugador
        if (this.jugadorSeleccionado) {
          this.cargarJugadorConNoticias(this.jugadorSeleccionado.id);
        }

        // Limpiar solo los campos de la noticia, mantener el jugador seleccionado
        this.noticiaForm.patchValue({
          texto: '',
          esLesion: false,
          resumenLesion: '',
          designacionLesion: ''
        });

        this.cargando = false;

        // Ocultar mensaje de éxito después de 5 segundos
        setTimeout(() => {
          this.mensajeExito = '';
        }, 5000);
      },
      error: (error) => {
        this.mensajeError = error.message || 'Error al crear la noticia';
        this.mensajeExito = '';
        this.cargando = false;
      }
    });
  }

  get esFormularioValido(): boolean {
    return this.noticiaForm.valid;
  }

  get caracteresRestantes(): number {
    const texto = this.noticiaForm.get('texto')?.value || '';
    return 300 - texto.length;
  }

  get caracteresResumenRestantes(): number {
    const resumen = this.noticiaForm.get('resumenLesion')?.value || '';
    return 30 - resumen.length;
  }
}
