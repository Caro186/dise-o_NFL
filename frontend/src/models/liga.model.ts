export interface CrearLigaDto {
  nombre: string;
  descripcion?: string;
  cantidadEquipos: 4 | 6 | 8 | 10 | 12 | 14 | 16 | 18 | 20;
  contrasena: string;
  nombreEquipoComisionado: string;
  playoffs: 4 | 6; // Semanas 16-17 o 16-18
}
