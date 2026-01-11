import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { BuscarUnirseLiga } from './buscar-unirse-liga';
import { LigaService, LigaResponseDto } from '../../services/liga.service';
import { EquipoFantasyService, EquipoFantasyResponseDto } from '../../services/equipo-fantasy.service';
import { Authservice } from '../../services/authservice';

describe('BuscarUnirseLiga Component', () => {
  let component: BuscarUnirseLiga;
  let fixture: ComponentFixture<BuscarUnirseLiga>;
  let mockLigaService: jasmine.SpyObj<LigaService>;
  let mockEquipoFantasyService: jasmine.SpyObj<EquipoFantasyService>;
  let mockAuthService: jasmine.SpyObj<Authservice>;

  const mockUsuario = { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' };
  const mockLigas: LigaResponseDto[] = [
    {
      idLiga: 1,
      nombreLiga: 'Liga Test 1',
      cuposTotales: 10,
      cuposOcupados: 5,
      estado: 'Pre-Draft',
      idTemporada: 1,
      comisionadoId: 1,
      formatoPosiciones: 'QB,RB,WR',
      esquemaPuntos: 'PPR',
      configPlayoffs: '4-equipos',
      permitirDecimales: true,
      fechaCreacion: new Date()
    },
    {
      idLiga: 2,
      nombreLiga: 'Liga Test 2',
      cuposTotales: 12,
      cuposOcupados: 12,
      estado: 'Activa',
      idTemporada: 1,
      comisionadoId: 2,
      formatoPosiciones: 'QB,RB,WR',
      esquemaPuntos: 'Standard',
      configPlayoffs: '6-equipos',
      permitirDecimales: false,
      fechaCreacion: new Date()
    }
  ];
  const mockEquipos: EquipoFantasyResponseDto[] = [
    { id: 1, nombre: 'Equipo 1', usuarioId: 1, ligaId: undefined, fechaCreacion: new Date(), estado: 'Activo' },
    { id: 2, nombre: 'Equipo 2', usuarioId: 1, ligaId: 3, fechaCreacion: new Date(), estado: 'Activo' }
  ];

  beforeEach(async () => {
    mockLigaService = jasmine.createSpyObj('LigaService', ['obtenerTodas', 'unirseALiga']);
    mockEquipoFantasyService = jasmine.createSpyObj('EquipoFantasyService', ['crear', 'subirImagen', 'obtenerPorUsuario']);
    mockAuthService = jasmine.createSpyObj('Authservice', [], {
      currentUserValue: mockUsuario
    });

    await TestBed.configureTestingModule({
      imports: [BuscarUnirseLiga, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: LigaService, useValue: mockLigaService },
        { provide: EquipoFantasyService, useValue: mockEquipoFantasyService },
        { provide: Authservice, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BuscarUnirseLiga);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debe cargar todas las ligas y equipos al inicializar', () => {
    mockLigaService.obtenerTodas.and.returnValue(of(mockLigas));
    mockEquipoFantasyService.obtenerPorUsuario.and.returnValue(of(mockEquipos));

    fixture.detectChanges();

    expect(mockLigaService.obtenerTodas).toHaveBeenCalled();
    expect(mockEquipoFantasyService.obtenerPorUsuario).toHaveBeenCalledWith(1);
    expect(component.ligasEncontradas.length).toBe(1); // Solo la que tiene cupos disponibles
    expect(component.equiposDisponibles.length).toBe(1); // Solo la que no tiene liga
  });

  it('debe filtrar ligas sin cupos disponibles', () => {
    mockLigaService.obtenerTodas.and.returnValue(of(mockLigas));
    mockEquipoFantasyService.obtenerPorUsuario.and.returnValue(of(mockEquipos));

    // Crear nuevo componente para este test
    fixture = TestBed.createComponent(BuscarUnirseLiga);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.ligasEncontradas.length).toBe(1);
    expect(component.ligasEncontradas[0].idLiga).toBe(1); // Solo la que tiene cupos
  });

  it('debe buscar ligas por nombre', () => {
    mockLigaService.obtenerTodas.and.returnValue(of(mockLigas));
    component.busqueda = 'Test 1';

    component.buscarLigas();

    expect(component.ligasEncontradas.length).toBe(1);
    expect(component.ligasEncontradas[0].nombreLiga).toContain('Test 1');
  });

  it('debe calcular correctamente los cupos disponibles', () => {
    const cupos = component.getCuposDisponibles(mockLigas[0]);
    expect(cupos).toBe(5); // 10 - 5
  });

  it('debe retornar la clase CSS correcta para el badge según el estado', () => {
    expect(component.getBadgeClass('Pre-Draft')).toBe('bg-info');
    expect(component.getBadgeClass('En Draft')).toBe('bg-warning text-dark');
    expect(component.getBadgeClass('Activa')).toBe('bg-success');
    expect(component.getBadgeClass('EstadoDesconocido')).toBe('bg-secondary');
  });

  it('debe seleccionar una liga para unirse', () => {
    component.seleccionarLiga(mockLigas[0]);

    expect(component.ligaSeleccionada).toBe(mockLigas[0]);
    expect(component.mostrarFormularioUnirse).toBe(true);
    expect(component.password).toBe('');
    expect(component.alias).toBe('');
  });

  it('debe cancelar la unión a la liga', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.mostrarFormularioUnirse = true;

    component.cancelarUnion();

    expect(component.mostrarFormularioUnirse).toBe(false);
    expect(component.ligaSeleccionada).toBeNull();
    expect(component.password).toBe('');
  });

  it('debe validar que la contraseña tenga al menos 8 caracteres', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Pass1';
    component.alias = 'Mi Alias';

    component.onUnirse();

    expect(component.errorMessage).toBe('La contraseña debe tener al menos 8 caracteres');
    expect(mockLigaService.unirseALiga).not.toHaveBeenCalled();
  });

  it('debe validar que el alias sea obligatorio', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = '';

    component.onUnirse();

    expect(component.errorMessage).toBe('El alias es obligatorio');
    expect(mockLigaService.unirseALiga).not.toHaveBeenCalled();
  });

  it('debe validar que el alias no exceda 50 caracteres', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'a'.repeat(51);

    component.onUnirse();

    expect(component.errorMessage).toBe('El alias no puede exceder 50 caracteres');
    expect(mockLigaService.unirseALiga).not.toHaveBeenCalled();
  });

  it('debe validar que se seleccione un equipo cuando se usa equipo existente', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'Mi Alias';
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 0;

    component.onUnirse();

    expect(component.errorMessage).toBe('Debes seleccionar un equipo');
    expect(mockLigaService.unirseALiga).not.toHaveBeenCalled();
  });

  it('debe validar que se ingrese nombre de equipo cuando se crea uno nuevo', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'Mi Alias';
    component.opcionEquipo = 'nuevo';
    component.nombreEquipoNuevo = '';

    component.onUnirse();

    expect(component.errorMessage).toBe('El nombre del equipo es obligatorio');
    expect(mockEquipoFantasyService.crear).not.toHaveBeenCalled();
  });

  it('debe unirse a liga con equipo existente exitosamente', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'Mi Alias';
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 1;

    mockLigaService.unirseALiga.and.returnValue(of({ mensaje: 'Unido exitosamente' }));

    component.onUnirse();

    expect(mockLigaService.unirseALiga).toHaveBeenCalled();
    expect(component.successMessage).toBe('¡Te has unido a la liga exitosamente!');
  });

  it('debe crear equipo nuevo y luego unirse a liga', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'Mi Alias';
    component.opcionEquipo = 'nuevo';
    component.nombreEquipoNuevo = 'Nuevo Equipo';

    const mockEquipoCreado: EquipoFantasyResponseDto = { id: 3, nombre: 'Nuevo Equipo', usuarioId: 1, fechaCreacion: new Date(), estado: 'Activo' };
    mockEquipoFantasyService.crear.and.returnValue(of(mockEquipoCreado));
    mockLigaService.unirseALiga.and.returnValue(of({ mensaje: 'Unido exitosamente' }));

    component.onUnirse();

    expect(mockEquipoFantasyService.crear).toHaveBeenCalled();
    expect(mockLigaService.unirseALiga).toHaveBeenCalled();
  });

  it('debe manejar error al unirse a liga', () => {
    component.ligaSeleccionada = mockLigas[0];
    component.password = 'Password123';
    component.alias = 'Mi Alias';
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 1;

    mockLigaService.unirseALiga.and.returnValue(throwError(() => ({ status: 400, error: { mensaje: 'Contraseña incorrecta' } })));

    component.onUnirse();

    expect(component.errorMessage).toBe('Contraseña incorrecta');
    expect(component.isLoading).toBe(false);
  });

  it('debe manejar selección de archivo de imagen', (done) => {
    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    Object.defineProperty(file, 'size', { value: 1024 * 1024, writable: false }); // 1MB
    
    const input = document.createElement('input');
    input.type = 'file';
    const event = { target: input } as any;

    Object.defineProperty(input, 'files', {
      value: [file],
      writable: false
    });

    // Mock FileReader
    const mockReader = {
      readAsDataURL: jasmine.createSpy('readAsDataURL'),
      result: 'data:image/jpeg;base64,mockdata',
      onload: null as any
    };
    
    spyOn(window, 'FileReader').and.returnValue(mockReader as any);
    
    // Mock Image
    const mockImage = {
      width: 500,
      height: 500,
      onload: null as any,
      set src(value: string) {
        if (this.onload) {
          setTimeout(() => this.onload(), 0);
        }
      }
    };
    
    spyOn(window, 'Image').and.returnValue(mockImage as any);

    component.onFileSelected(event);

    // Simular FileReader.onload
    setTimeout(() => {
      if (mockReader.onload) {
        mockReader.onload({ target: { result: mockReader.result } } as any);
      }
    }, 10);

    // Esperar a que se procese la imagen
    setTimeout(() => {
      expect(component.selectedFile).toBe(file);
      expect(component.imagenPreview).toBeTruthy();
      done();
    }, 100);
  });
});

