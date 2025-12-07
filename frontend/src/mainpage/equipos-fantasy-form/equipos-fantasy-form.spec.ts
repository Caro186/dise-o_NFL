import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { EquiposFantasyForm } from './equipos-fantasy-form';
import { EquipoFantasyService } from '../../services/equipo-fantasy.service';
import { Authservice } from '../../services/authservice';

describe('EquiposFantasyForm Component', () => {
  let component: EquiposFantasyForm;
  let fixture: ComponentFixture<EquiposFantasyForm>;
  let mockEquipoFantasyService: jasmine.SpyObj<EquipoFantasyService>;
  let mockAuthService: jasmine.SpyObj<Authservice>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockUsuario = { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' };
  const mockEquipoCreado = {
    id: 1,
    nombre: 'Equipo Test',
    usuarioId: 1,
    fechaCreacion: new Date(),
    estado: 'Activo'
  };

  beforeEach(async () => {
    mockEquipoFantasyService = jasmine.createSpyObj('EquipoFantasyService', ['crear', 'subirImagen']);
    mockAuthService = jasmine.createSpyObj('Authservice', [], {
      currentUserValue: mockUsuario
    });
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [EquiposFantasyForm, HttpClientTestingModule],
      providers: [
        { provide: EquipoFantasyService, useValue: mockEquipoFantasyService },
        { provide: Authservice, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EquiposFantasyForm);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debe validar que el nombre del equipo sea obligatorio', () => {
    component.nombreEquipo = '';

    component.onSubmit();

    expect(component.errorMessage).toBe('Por favor ingresa el nombre del equipo.');
    expect(mockEquipoFantasyService.crear).not.toHaveBeenCalled();
  });

  it('debe crear equipo sin imagen exitosamente', () => {
    component.nombreEquipo = 'Equipo Test';

    mockEquipoFantasyService.crear.and.returnValue(of(mockEquipoCreado));

    component.onSubmit();

    expect(mockEquipoFantasyService.crear).toHaveBeenCalledWith({
      nombre: 'Equipo Test',
      usuarioId: 1,
      ligaId: undefined
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/equipos-fantasy']);
  });

  it('debe crear equipo con imagen exitosamente', () => {
    component.nombreEquipo = 'Equipo Test';
    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    component.selectedFile = file;

    mockEquipoFantasyService.crear.and.returnValue(of(mockEquipoCreado));
    mockEquipoFantasyService.subirImagen.and.returnValue(of({ imagenUrl: 'http://example.com/image.jpg' }));

    component.onSubmit();

    expect(mockEquipoFantasyService.crear).toHaveBeenCalled();
    expect(mockEquipoFantasyService.subirImagen).toHaveBeenCalledWith(1, file);
  });

  it('debe manejar error al crear equipo', () => {
    component.nombreEquipo = 'Equipo Test';

    mockEquipoFantasyService.crear.and.returnValue(throwError(() => ({ status: 400, error: { mensaje: 'Error al crear equipo' } })));

    component.onSubmit();

    expect(component.errorMessage).toBe('Error al crear equipo');
    expect(component.isLoading).toBe(false);
  });

  it('debe manejar error al subir imagen pero continuar', () => {
    component.nombreEquipo = 'Equipo Test';
    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    component.selectedFile = file;

    mockEquipoFantasyService.crear.and.returnValue(of(mockEquipoCreado));
    mockEquipoFantasyService.subirImagen.and.returnValue(throwError(() => ({ status: 500 })));

    component.onSubmit();

    expect(mockEquipoFantasyService.crear).toHaveBeenCalled();
    expect(mockEquipoFantasyService.subirImagen).toHaveBeenCalled();
    // Debe navegar aunque falle la imagen
    expect(mockRouter.navigate).toHaveBeenCalled();
  });

  it('debe validar tipo de archivo de imagen', () => {
    const file = new File([''], 'test.pdf', { type: 'application/pdf' });
    const input = document.createElement('input');
    input.type = 'file';
    const event = { target: input } as any;
    
    spyOn(window, 'alert');

    // Simular selección de archivo
    Object.defineProperty(input, 'files', {
      value: [file],
      writable: false
    });

    component.onFileSelected(event);

    expect(window.alert).toHaveBeenCalledWith('Solo se permiten imágenes JPEG o PNG.');
    expect(component.selectedFile).toBeNull();
  });

  it('debe validar tamaño máximo de archivo', () => {
    const largeFile = new File(['x'.repeat(6 * 1024 * 1024)], 'large.jpg', { type: 'image/jpeg' });
    const input = document.createElement('input');
    input.type = 'file';
    const event = { target: input } as any;
    
    spyOn(window, 'alert');

    Object.defineProperty(input, 'files', {
      value: [largeFile],
      writable: false
    });

    component.onFileSelected(event);

    expect(window.alert).toHaveBeenCalledWith('El tamaño máximo permitido es 5 MB.');
    expect(component.selectedFile).toBeNull();
  });

  it('debe aceptar archivo de imagen válido', (done) => {
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

  it('debe limpiar preview cuando no hay archivo seleccionado', () => {
    const input = document.createElement('input');
    input.type = 'file';
    const event = { target: input } as any;

    Object.defineProperty(input, 'files', {
      value: [],
      writable: false
    });

    component.onFileSelected(event);

    expect(component.imagenPreview).toBeNull();
    expect(component.selectedFile).toBeNull();
  });
});

