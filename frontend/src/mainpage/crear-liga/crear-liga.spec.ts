import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CrearLiga } from './crear-liga';
import { LigaService, LigaResponseDto } from '../../services/liga.service';
import { EquipoFantasyService, EquipoFantasyResponseDto } from '../../services/equipo-fantasy.service';
import { TemporadaService, TemporadaResponseDto } from '../../services/temporada.service';
import { Authservice } from '../../services/authservice';

describe('CrearLiga Component', () => {
  let component: CrearLiga;
  let fixture: ComponentFixture<CrearLiga>;
  let mockLigaService: jasmine.SpyObj<LigaService>;
  let mockEquipoFantasyService: jasmine.SpyObj<EquipoFantasyService>;
  let mockTemporadaService: jasmine.SpyObj<TemporadaService>;
  let mockAuthService: jasmine.SpyObj<Authservice>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockUsuario = { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' };
  const mockTemporadas = [
    { id: 1, nombre: '2024', actual: true, fechaInicio: '2024-01-01', fechaCierre: '2024-12-31', fechaCreacion: '2024-01-01' },
    { id: 2, nombre: '2023', actual: false, fechaInicio: '2023-01-01', fechaCierre: '2023-12-31', fechaCreacion: '2023-01-01' }
  ];
  const mockEquipos = [
    { id: 1, nombre: 'Equipo 1', usuarioId: 1, ligaId: undefined, fechaCreacion: new Date(), estado: 'Activo' },
    { id: 2, nombre: 'Equipo 2', usuarioId: 1, ligaId: undefined, fechaCreacion: new Date(), estado: 'Activo' }
  ];
  const mockLigaCreada = {
    idLiga: 1,
    nombreLiga: 'Nueva Liga',
    cuposTotales: 10,
    cuposOcupados: 1,
    estado: 'Pre-Draft',
    idTemporada: 1,
    comisionadoId: 1,
    formatoPosiciones: 'QB,RB,WR',
    esquemaPuntos: 'PPR',
    configPlayoffs: '4-equipos',
    permitirDecimales: true,
    fechaCreacion: new Date()
  };

  beforeEach(async () => {
    mockLigaService = jasmine.createSpyObj('LigaService', ['crear', 'unirseALiga']);
    mockEquipoFantasyService = jasmine.createSpyObj('EquipoFantasyService', ['crear', 'subirImagen', 'obtenerPorUsuario']);
    mockTemporadaService = jasmine.createSpyObj('TemporadaService', ['obtenerTemporadas']);
    mockAuthService = jasmine.createSpyObj('Authservice', [], {
      currentUserValue: mockUsuario
    });
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [CrearLiga, HttpClientTestingModule],
      providers: [
        { provide: LigaService, useValue: mockLigaService },
        { provide: EquipoFantasyService, useValue: mockEquipoFantasyService },
        { provide: TemporadaService, useValue: mockTemporadaService },
        { provide: Authservice, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CrearLiga);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debe cargar temporadas y equipos al inicializar', () => {
    mockTemporadaService.obtenerTemporadas.and.returnValue(of(mockTemporadas));
    mockEquipoFantasyService.obtenerPorUsuario.and.returnValue(of(mockEquipos));

    fixture.detectChanges();

    expect(mockTemporadaService.obtenerTemporadas).toHaveBeenCalled();
    expect(mockEquipoFantasyService.obtenerPorUsuario).toHaveBeenCalledWith(1);
    expect(component.temporadas.length).toBe(2);
    expect(component.equiposDisponibles.length).toBe(2);
    expect(component.idTemporada).toBe(1); // Debe seleccionar la temporada actual
  });

  it('debe validar que el nombre de liga sea obligatorio', () => {
    component.nombreLiga = '';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;

    component.onSubmit();

    expect(component.errorMessage).toBe('El nombre de la liga es obligatorio');
    expect(mockLigaService.crear).not.toHaveBeenCalled();
  });

  it('debe validar que la contraseña tenga al menos 8 caracteres', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Pass1';
    component.confirmPassword = 'Pass1';
    component.idTemporada = 1;

    component.onSubmit();

    expect(component.errorMessage).toBe('La contraseña debe tener al menos 8 caracteres');
    expect(mockLigaService.crear).not.toHaveBeenCalled();
  });

  it('debe validar que las contraseñas coincidan', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password456';
    component.idTemporada = 1;

    component.onSubmit();

    expect(component.errorMessage).toBe('Las contraseñas no coinciden');
    expect(mockLigaService.crear).not.toHaveBeenCalled();
  });

  it('debe validar que se seleccione una temporada', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 0;

    component.onSubmit();

    expect(component.errorMessage).toBe('Debes seleccionar una temporada');
    expect(mockLigaService.crear).not.toHaveBeenCalled();
  });

  it('debe validar que se seleccione un equipo cuando se usa equipo existente', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 0;

    component.onSubmit();

    expect(component.errorMessage).toBe('Debes seleccionar un equipo');
    expect(mockLigaService.crear).not.toHaveBeenCalled();
  });

  it('debe validar que se ingrese nombre de equipo cuando se crea uno nuevo', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.opcionEquipo = 'nuevo';
    component.nombreEquipoNuevo = '';

    component.onSubmit();

    expect(component.errorMessage).toBe('El nombre del equipo es obligatorio');
    expect(mockEquipoFantasyService.crear).not.toHaveBeenCalled();
  });

  it('debe crear liga con equipo existente exitosamente', () => {
    component.nombreLiga = 'Liga Test';
    component.descripcion = 'Descripción';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.cuposTotales = 10;
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 1;

    mockLigaService.crear.and.returnValue(of(mockLigaCreada));
    mockLigaService.unirseALiga.and.returnValue(of({ mensaje: 'Unido exitosamente' }));

    component.onSubmit();

    expect(mockLigaService.crear).toHaveBeenCalled();
    expect(mockLigaService.unirseALiga).toHaveBeenCalled();
    expect(component.successMessage).toBe('¡Liga creada y equipo vinculado exitosamente!');
  });

  it('debe crear equipo nuevo y luego liga cuando se selecciona crear equipo', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.opcionEquipo = 'nuevo';
    component.nombreEquipoNuevo = 'Nuevo Equipo';

    const mockEquipoCreado: EquipoFantasyResponseDto = { id: 3, nombre: 'Nuevo Equipo', usuarioId: 1, fechaCreacion: new Date(), estado: 'Activo' };
    mockEquipoFantasyService.crear.and.returnValue(of(mockEquipoCreado));
    mockLigaService.crear.and.returnValue(of(mockLigaCreada));
    mockLigaService.unirseALiga.and.returnValue(of({ mensaje: 'Unido exitosamente' }));

    component.onSubmit();

    expect(mockEquipoFantasyService.crear).toHaveBeenCalled();
    expect(mockLigaService.crear).toHaveBeenCalled();
    expect(mockLigaService.unirseALiga).toHaveBeenCalled();
  });

  it('debe manejar error al crear liga', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 1;

    mockLigaService.crear.and.returnValue(throwError(() => ({ status: 400, error: { mensaje: 'Error al crear liga' } })));

    component.onSubmit();

    expect(component.errorMessage).toBe('Error al crear liga');
    expect(component.isLoading).toBe(false);
  });

  it('debe manejar error al vincular equipo a liga', () => {
    component.nombreLiga = 'Liga Test';
    component.password = 'Password123';
    component.confirmPassword = 'Password123';
    component.idTemporada = 1;
    component.opcionEquipo = 'existente';
    component.equipoSeleccionadoId = 1;

    mockLigaService.crear.and.returnValue(of(mockLigaCreada));
    mockLigaService.unirseALiga.and.returnValue(throwError(() => ({ status: 400 })));

    component.onSubmit();

    expect(component.errorMessage).toContain('no se pudo vincular el equipo');
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

