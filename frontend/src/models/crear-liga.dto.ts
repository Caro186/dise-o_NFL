export interface CrearLigaDto {
  NombreLiga: string;
  Descripcion?: string;
  Password: string;
  CantidadEquipos: number;
  TipoPlayoffs: number;
  NombreEquipo: string;
  UsuarioId: number;
}
