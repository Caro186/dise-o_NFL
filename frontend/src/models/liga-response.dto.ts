export interface EquipoEnLigaDto {
  Id: number;
  Nombre: string;
  Alias?: string;
  EsComisionado: boolean;
  FechaIncorporacion: string;
}

export interface LigaResponseDto {
  IdLiga: number;
  NombreLiga: string;
  Descripcion?: string;
  ImagenUrl?: string;
  Temporada: string;
  Estado: string;
  CuposTotales: number;
  CuposOcupados: number;
  CuposDisponibles: number;
  FechaCreacion: string;
  IdCreador: number;
  NombreCreador: string;
  TipoPlayoffs: number;
  PermitirDecimales: boolean;
  EsquemaPosiciones: string;
  EsquemaPuntuacion: string;
  MiEquipo?: EquipoEnLigaDto;
}
