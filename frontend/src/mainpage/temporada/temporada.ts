import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TemporadaService, TemporadaResponseDto, SemanaDto, TemporadaDto } from '../../services/temporada.service';

interface Temporada {
  nombre: string;
  cantidadSemanas: number;
  fechaInicio: Date;
  fechaCierre: Date;
  actual: boolean;
}

interface Semana {
  inicio: Date | null;
  fin: Date | null;
}

@Component({
  selector: 'app-temporada',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './temporada.html',
  styleUrls: ['./temporada.css']
})
export class TemporadaComponent implements OnInit {
  temporada: Temporada = {
    nombre: "",
    cantidadSemanas: 1,
    fechaInicio: new Date(),
    fechaCierre: new Date(),
    actual: false
  };

  semanas: Semana[] = [];
  temporadasCreadas: TemporadaResponseDto[] = [];

  constructor(private temporadaService: TemporadaService) { }

  ngOnInit(): void {
    this.cargarTemporadas();
  }

  cargarTemporadas(): void {
    this.temporadaService.obtenerTemporadas().subscribe({
      next: (resp) => {
        this.temporadasCreadas = resp;
      },
      error: (err) => {
        console.error('Error al cargar temporadas:', err);
      }
    });
  }

  actualizarSemanas(): void {
    this.semanas = [];
    for (let i = 0; i < this.temporada.cantidadSemanas; i++) {
      this.semanas.push({ inicio: null, fin: null });
    }
  }

  crearTemporada(): void {
    const nombre = this.temporada.nombre;
    const fechaInicioTemporada = new Date(this.temporada.fechaInicio);
    const fechaCierreTemporada = new Date(this.temporada.fechaCierre);

    // 1️⃣ Validaciones básicas de temporada
if (nombre.length < 1 || nombre.length > 100) {
  alert('El nombre de la temporada debe tener entre 1 y 100 caracteres.');
  return;
}


    if (fechaInicioTemporada > fechaCierreTemporada) {
      alert('La fecha de inicio de la temporada debe ser anterior a la fecha de fin.');
      return;
    }

    if (this.temporadasCreadas.some(t => t.nombre === nombre)) {
      alert('Ya existe una temporada con ese nombre.');
      return;
    }

    const traslapeTemporadas = this.temporadasCreadas.some(t => {
      const inicio = new Date(t.fechaInicio);
      const fin = new Date(t.fechaCierre);
      return fechaInicioTemporada <= fin && fechaCierreTemporada >= inicio;
    });
    if (traslapeTemporadas) {
      alert('Las fechas de la temporada se traslapan con otra temporada existente.');
      return;
    }

    // 3️⃣ y 4️⃣ Validaciones de semanas
    for (let i = 0; i < this.semanas.length; i++) {
      const s = this.semanas[i];
      if (!s.inicio || !s.fin) {
        alert(`Semana ${i + 1} debe tener fechas de inicio y fin.`);
        return;
      }

      const inicioSemana = new Date(s.inicio);
      const finSemana = new Date(s.fin);

      if (inicioSemana > finSemana) {
        alert(`Semana ${i + 1}: la fecha de inicio debe ser anterior a la fecha de fin.`);
        return;
      }

      if (inicioSemana < fechaInicioTemporada || finSemana > fechaCierreTemporada) {
        alert(`Semana ${i + 1}: las fechas deben estar dentro del rango de la temporada.`);
        return;
      }
    }

    // 5️⃣ Validar que semanas no se traslapen entre sí
    for (let i = 0; i < this.semanas.length; i++) {
      for (let j = i + 1; j < this.semanas.length; j++) {
        const aInicio = new Date(this.semanas[i].inicio!);
        const aFin = new Date(this.semanas[i].fin!);
        const bInicio = new Date(this.semanas[j].inicio!);
        const bFin = new Date(this.semanas[j].fin!);

        if (aInicio <= bFin && aFin >= bInicio) {
          alert(`Las semanas ${i + 1} y ${j + 1} se traslapan.`);
          return;
        }
      }
    }

    // ✅ Si pasa todas las validaciones, crear DTO y enviar al servicio
    const semanasDto = this.semanas.map(s => ({
      fechaInicio: new Date(s.inicio!).toISOString(),
      fechaFin: new Date(s.fin!).toISOString()
    }));

    const temporadaDto: TemporadaDto = {
      nombre,
      fechaInicio: fechaInicioTemporada.toISOString(),
      fechaCierre: fechaCierreTemporada.toISOString(),
      actual: this.temporada.actual,
      semanas: semanasDto
    };

    this.temporadaService.crearTemporada(temporadaDto).subscribe({
      next: (resp) => {
        alert(`Temporada ${resp.nombre} creada correctamente.`);
        // Reinicia formulario
        this.temporada = { nombre: "", cantidadSemanas: 1, fechaInicio: new Date(), fechaCierre: new Date(), actual: false };
        this.semanas = [];
        this.cargarTemporadas(); // recargar temporadas
      },
      error: (err) => {
        console.error('Error al crear temporada:', err);
        alert('Ocurrió un error al crear la temporada.');
      }
    });
  }
}