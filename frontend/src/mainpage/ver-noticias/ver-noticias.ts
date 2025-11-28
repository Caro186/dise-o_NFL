import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NoticiaJugadorService, NoticiaJugadorResponseDto } from '../../services/noticia-jugador.service';

@Component({
  selector: 'app-ver-noticias',
  standalone: true,
  templateUrl: './ver-noticias.html',
  styleUrls: ['./ver-noticias.css'],
  imports: [CommonModule]
})
export class VerNoticiasComponent implements OnInit {
  noticias: NoticiaJugadorResponseDto[] = [];
  noticiasFiltradas: NoticiaJugadorResponseDto[] = [];
  cargando: boolean = false;
  mensajeError: string = '';

  // Filtros
  filtroTipo: string = 'todas'; // 'todas', 'lesiones', 'generales'
  filtroBusqueda: string = '';

  constructor(private noticiaService: NoticiaJugadorService) { }

  ngOnInit(): void {
    this.cargarNoticias();
  }

  cargarNoticias(): void {
    this.cargando = true;
    this.noticiaService.obtenerTodasNoticias().subscribe({
      next: (response) => {
        this.noticias = response.noticias || [];
        this.aplicarFiltros();
        this.cargando = false;
      },
      error: (error) => {
        this.mensajeError = 'Error al cargar noticias: ' + error.message;
        this.cargando = false;
      }
    });
  }

  aplicarFiltros(): void {
    let resultado = [...this.noticias];

    // Filtrar por tipo
    if (this.filtroTipo === 'lesiones') {
      resultado = resultado.filter(n => n.esLesion);
    } else if (this.filtroTipo === 'generales') {
      resultado = resultado.filter(n => !n.esLesion);
    }

    // Filtrar por búsqueda
    if (this.filtroBusqueda.trim()) {
      const busqueda = this.filtroBusqueda.toLowerCase();
      resultado = resultado.filter(n =>
        n.nombreJugador.toLowerCase().includes(busqueda) ||
        n.equipoNFL.toLowerCase().includes(busqueda) ||
        n.texto.toLowerCase().includes(busqueda)
      );
    }

    this.noticiasFiltradas = resultado;
  }

  onFiltroTipoChange(event: any): void {
    this.filtroTipo = event.target.value;
    this.aplicarFiltros();
  }

  onBusquedaChange(event: any): void {
    this.filtroBusqueda = event.target.value;
    this.aplicarFiltros();
  }

  limpiarFiltros(): void {
    this.filtroTipo = 'todas';
    this.filtroBusqueda = '';
    this.aplicarFiltros();
  }

  get noticiasLesiones(): number {
    return this.noticias.filter(n => n.esLesion).length;
  }

  get noticiasGenerales(): number {
    return this.noticias.filter(n => !n.esLesion).length;
  }
}
