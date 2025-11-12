import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { v4 as uuidv4 } from 'uuid'; // npm install uuid

@Component({
  selector: 'app-form-jugador',
  standalone: true,
  templateUrl: './form-jugador.html',
  styleUrls: ['./form-jugador.css'],
  imports: [CommonModule, ReactiveFormsModule]  // 👈 esto es CLAVE
})
export class FormJugadorComponent {
  jugadorForm: FormGroup;
  equiposNFL: string[] = [
    'Kansas City Chiefs', 'Dallas Cowboys', 'San Francisco 49ers',
    'Buffalo Bills', 'Philadelphia Eagles', 'Green Bay Packers'
  ];
  thumbnailUrl: string | null = null;
  mensajeExito: string = '';

  constructor(private fb: FormBuilder) {
    this.jugadorForm = this.fb.group({
      nombre: ['', Validators.required],
      posicion: ['', Validators.required],
      equipo: ['', Validators.required],
      imagen: [null]
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.jugadorForm.patchValue({ imagen: file });

      const reader = new FileReader();
      reader.onload = (e: any) => this.thumbnailUrl = e.target.result;
      reader.readAsDataURL(file);
    }
  }

  crearJugador(): void {
    if (this.jugadorForm.invalid) return;

    const jugador = {
      id: uuidv4(),
      ...this.jugadorForm.value,
      fechaCreacion: new Date().toISOString(),
      thumbnail: this.thumbnailUrl
    };

    console.log('Jugador creado:', jugador);
    this.mensajeExito = `Jugador ${jugador.nombre} creado correctamente.`;

    this.jugadorForm.reset();
    this.thumbnailUrl = null;
  }
}
