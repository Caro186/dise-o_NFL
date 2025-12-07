import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { Liga } from './liga';
import { LigaService } from '../../services/liga.service';
import { Authservice } from '../../services/authservice';
import { LigaResponseDto } from '../../services/liga.service';

describe('Liga Component', () => {
  let component: Liga;
  let fixture: ComponentFixture<Liga>;
  let mockLigaService: jasmine.SpyObj<LigaService>;
  let mockAuthService: jasmine.SpyObj<Authservice>;

  const mockLigas: LigaResponseDto[] = [
    {
      idLiga: 1,
      nombreLiga: 'Liga Test 1',
      descripcion: 'Descripción test',
      idTemporada: 1,
      estado: 'Pre-Draft',
      cuposTotales: 10,
      cuposOcupados: 5,
      fechaCreacion: new Date(),
      comisionadoId: 1,
      formatoPosiciones: 'QB,RB,WR',
      esquemaPuntos: 'PPR',
      configPlayoffs: '4-equipos',
      permitirDecimales: true
    },
    {
      idLiga: 2,
      nombreLiga: 'Liga Test 2',
      descripcion: 'Descripción test 2',
      idTemporada: 1,
      estado: 'Activa',
      cuposTotales: 12,
      cuposOcupados: 8,
      fechaCreacion: new Date(),
      comisionadoId: 2,
      formatoPosiciones: 'QB,RB,WR',
      esquemaPuntos: 'Standard',
      configPlayoffs: '6-equipos',
      permitirDecimales: false
    }
  ];

  beforeEach(async () => {
    mockLigaService = jasmine.createSpyObj('LigaService', ['obtenerPorUsuario']);
    mockAuthService = jasmine.createSpyObj('Authservice', [], {
      currentUserValue: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' }
    });

    await TestBed.configureTestingModule({
      imports: [Liga, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: LigaService, useValue: mockLigaService },
        { provide: Authservice, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Liga);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debe cargar las ligas del usuario al inicializar', () => {
    mockLigaService.obtenerPorUsuario.and.returnValue(of(mockLigas));

    fixture.detectChanges();

    expect(mockLigaService.obtenerPorUsuario).toHaveBeenCalledWith(1);
    expect(component.ligas.length).toBe(2);
    expect(component.ligas[0].esComisionado).toBe(true);
    expect(component.ligas[1].esComisionado).toBe(false);
    expect(component.isLoading).toBe(false);
  });

  it('debe mostrar error cuando no hay usuario logueado', () => {
    Object.defineProperty(mockAuthService, 'currentUserValue', { value: null, writable: true });

    fixture.detectChanges();

    expect(component.errorMessage).toBe('Debes iniciar sesión para ver tus ligas.');
    expect(component.isLoading).toBe(false);
    expect(mockLigaService.obtenerPorUsuario).not.toHaveBeenCalled();
  });

  it('debe manejar error al cargar ligas', () => {
    mockLigaService.obtenerPorUsuario.and.returnValue(throwError(() => ({ status: 500 })));

    fixture.detectChanges();

    expect(component.errorMessage).toBe('Error al cargar las ligas. Inténtalo de nuevo.');
    expect(component.isLoading).toBe(false);
  });

  it('debe manejar error de conexión al cargar ligas', () => {
    mockLigaService.obtenerPorUsuario.and.returnValue(throwError(() => ({ status: 0 })));

    fixture.detectChanges();

    expect(component.errorMessage).toBe('No se puede conectar con el servidor. Verifica que el backend esté corriendo.');
    expect(component.isLoading).toBe(false);
  });

  it('debe calcular correctamente los cupos disponibles', () => {
    const liga = mockLigas[0];
    const cupos = component.getCuposDisponibles(liga);
    
    expect(cupos).toBe(5); // 10 - 5
  });

  it('debe retornar la clase CSS correcta para el badge según el estado', () => {
    expect(component.getBadgeClass('Pre-Draft')).toBe('bg-info');
    expect(component.getBadgeClass('En Draft')).toBe('bg-warning text-dark');
    expect(component.getBadgeClass('Activa')).toBe('bg-success');
    expect(component.getBadgeClass('Finalizada')).toBe('bg-secondary');
    expect(component.getBadgeClass('EstadoDesconocido')).toBe('bg-secondary');
  });

  it('debe retornar URL de imagen o placeholder', () => {
    const ligaConImagen = { ...mockLigas[0], imagenUrl: 'https://example.com/image.jpg' };
    expect(component.obtenerImagenUrl(ligaConImagen.imagenUrl)).toBe('https://example.com/image.jpg');

    expect(component.obtenerImagenUrl(null)).toBe('https://via.placeholder.com/150?text=Liga');
    expect(component.obtenerImagenUrl(undefined)).toBe('https://via.placeholder.com/150?text=Liga');
  });

  it('debe marcar ligas como comisionado correctamente', () => {
    mockLigaService.obtenerPorUsuario.and.returnValue(of(mockLigas));

    fixture.detectChanges();

    expect(component.ligas[0].esComisionado).toBe(true); // comisionadoId = 1, usuarioId = 1
    expect(component.ligas[1].esComisionado).toBe(false); // comisionadoId = 2, usuarioId = 1
  });
});
