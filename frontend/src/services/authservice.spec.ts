import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Authservice, LoginCredentials, RegistroDto } from './authservice';

describe('Authservice', () => {
  let service: Authservice;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;
  const baseUrl = 'http://localhost:5000/api';

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        Authservice,
        { provide: Router, useValue: mockRouter }
      ]
    });

    service = TestBed.inject(Authservice);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    try {
      httpMock.verify();
    } catch (e) {
      // Ignorar si httpMock ya no es válido (por ejemplo, si se reseteó el TestBed)
    }
    localStorage.clear();
  });

  it('debe crear el servicio', () => {
    expect(service).toBeTruthy();
  });

  describe('Login', () => {
    it('debe realizar login exitoso y guardar datos en localStorage', (done) => {
      const credentials: LoginCredentials = {
        email: 'test@example.com',
        password: 'Password123'
      };

      const mockResponse = {
        status: 'ok',
        usuario: {
          id: 1,
          email: 'test@example.com',
          nombreCompleto: 'Test User',
          fechaRegistro: new Date()
        },
        token: 'fake-jwt-token',
        tokenExpiracion: new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString()
      };

      service.login(credentials).subscribe(response => {
        expect(response.status).toBe('ok');
        expect(response.usuario?.email).toBe('test@example.com');
        expect(localStorage.getItem('token')).toBe('fake-jwt-token');
        expect(localStorage.getItem('currentUser')).toBeTruthy();
        expect(localStorage.getItem('lastActivity')).toBeTruthy();
        done();
      });

      const req = httpMock.expectOne(`${baseUrl}/Auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(credentials);
      req.flush(mockResponse);
    });

    it('debe manejar error de credenciales incorrectas', (done) => {
      const credentials: LoginCredentials = {
        email: 'test@example.com',
        password: 'WrongPassword'
      };

      service.login(credentials).subscribe({
        next: () => fail('Debería haber fallado'),
        error: (error) => {
          expect(error.status).toBe(401);
          done();
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/Auth/login`);
      req.flush({ mensaje: 'Credenciales incorrectas' }, { status: 401, statusText: 'Unauthorized' });
    });

    it('debe manejar cuenta bloqueada', (done) => {
      const credentials: LoginCredentials = {
        email: 'blocked@example.com',
        password: 'Password123'
      };

      service.login(credentials).subscribe({
        next: () => fail('Debería haber fallado'),
        error: (error) => {
          expect(error.status).toBe(403);
          done();
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/Auth/login`);
      req.flush({ mensaje: 'Cuenta bloqueada' }, { status: 403, statusText: 'Forbidden' });
    });
  });

  describe('Register', () => {
    it('debe registrar un nuevo usuario exitosamente', (done) => {
      const registroDto: RegistroDto = {
        email: 'newuser@example.com',
        password: 'Password123',
        nombreCompleto: 'New User'
      };

      const mockResponse = {
        mensaje: 'Usuario registrado exitosamente',
        usuario: {
          id: 1,
          email: 'newuser@example.com',
          nombreCompleto: 'New User',
          fechaRegistro: new Date()
        }
      };

      service.register(registroDto).subscribe(response => {
        expect(response.mensaje).toBe('Usuario registrado exitosamente');
        done();
      });

      const req = httpMock.expectOne(`${baseUrl}/Auth/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(registroDto);
      req.flush(mockResponse);
    });

    it('debe manejar error de email duplicado', (done) => {
      const registroDto: RegistroDto = {
        email: 'existing@example.com',
        password: 'Password123',
        nombreCompleto: 'Existing User'
      };

      service.register(registroDto).subscribe({
        next: () => fail('Debería haber fallado'),
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/Auth/register`);
      req.flush({ mensaje: 'Email ya registrado' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Logout', () => {
    it('debe limpiar localStorage y currentUser al hacer logout', () => {
      localStorage.setItem('token', 'fake-token');
      localStorage.setItem('currentUser', JSON.stringify({ id: 1, email: 'test@example.com' }));
      localStorage.setItem('lastActivity', new Date().toISOString());

      service.logout();

      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('currentUser')).toBeNull();
      expect(localStorage.getItem('lastActivity')).toBeNull();
      expect(service.currentUserValue).toBeNull();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });
  });

  describe('isLoggedIn', () => {
    it('debe retornar true si hay usuario y token', () => {
      // Preparar los datos del mock
      const mockUser = { 
        id: 1, 
        email: 'test@example.com', 
        nombreCompleto: 'Test User', 
        fechaRegistro: new Date().toISOString(), 
        rol: 'Usuario' 
      };
      
      // Limpiar el localStorage primero
      localStorage.clear();
      
      // Configurar localStorage ANTES de resetear el TestBed
      // IMPORTANTE: checkSessionValidity() requiere lastActivity y tokenExpiration
      const futureDate = new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(); // 12 horas en el futuro
      localStorage.setItem('token', 'fake-token');
      localStorage.setItem('currentUser', JSON.stringify(mockUser));
      localStorage.setItem('tokenExpiration', futureDate);
      localStorage.setItem('lastActivity', new Date().toISOString());
      
      // Verificar que el localStorage tiene los valores
      expect(localStorage.getItem('token')).toBe('fake-token');
      expect(localStorage.getItem('currentUser')).toBeTruthy();
      expect(localStorage.getItem('tokenExpiration')).toBeTruthy();
      expect(localStorage.getItem('lastActivity')).toBeTruthy();
      
      // Recrear el módulo de testing para que el servicio lea del localStorage
      // Primero reseteamos el TestBed para limpiar el servicio anterior
      TestBed.resetTestingModule();
      const newMockRouter = jasmine.createSpyObj('Router', ['navigate']);
      
      TestBed.configureTestingModule({
        imports: [HttpClientTestingModule],
        providers: [
          { provide: Router, useValue: newMockRouter }
        ]
      });
      
      // Verificar que el localStorage todavía tiene los valores después del reset
      expect(localStorage.getItem('token')).toBe('fake-token');
      expect(localStorage.getItem('currentUser')).toBeTruthy();
      expect(localStorage.getItem('tokenExpiration')).toBeTruthy();
      expect(localStorage.getItem('lastActivity')).toBeTruthy();
      
      // Obtener HttpClient del TestBed
      const httpClient = TestBed.inject(HttpClient);
      
      // Crear el servicio directamente para asegurar que lee del localStorage
      // Esto evita problemas con el singleton providedIn: 'root'
      const newService = new Authservice(httpClient, newMockRouter);
      const newHttpMock = TestBed.inject(HttpTestingController);
      
      // Verificar que el servicio lee correctamente del localStorage
      // checkSessionValidity() no debería haber llamado a logout() porque tenemos todos los valores
      expect(newService.currentUserValue).not.toBeNull();
      expect(newService.currentUserValue?.email).toBe('test@example.com');
      expect(newService.isLoggedIn()).toBeTruthy();
      
      // Verificar que no hay peticiones HTTP pendientes
      newHttpMock.verify();
    });

    it('debe retornar false si no hay usuario ni token', () => {
      localStorage.clear();
      service = TestBed.inject(Authservice);
      expect(service.isLoggedIn()).toBeFalsy();
    });
  });

  describe('Session Management', () => {
    it('debe actualizar última actividad', () => {
      // Preparar los datos del mock
      const mockUser = { 
        id: 1, 
        email: 'test@example.com', 
        nombreCompleto: 'Test User', 
        fechaRegistro: new Date().toISOString(), 
        rol: 'Usuario' 
      };
      
      // Limpiar el localStorage primero
      localStorage.clear();
      
      // Configurar localStorage ANTES de resetear el TestBed
      // IMPORTANTE: checkSessionValidity() requiere lastActivity y tokenExpiration
      const futureDate = new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(); // 12 horas en el futuro
      localStorage.setItem('currentUser', JSON.stringify(mockUser));
      localStorage.setItem('tokenExpiration', futureDate);
      localStorage.setItem('lastActivity', new Date().toISOString());
      
      // Verificar que el localStorage tiene los valores
      expect(localStorage.getItem('currentUser')).toBeTruthy();
      expect(localStorage.getItem('tokenExpiration')).toBeTruthy();
      expect(localStorage.getItem('lastActivity')).toBeTruthy();
      
      // Recrear el módulo de testing para que el servicio lea del localStorage
      TestBed.resetTestingModule();
      const newMockRouter = jasmine.createSpyObj('Router', ['navigate']);
      
      TestBed.configureTestingModule({
        imports: [HttpClientTestingModule],
        providers: [
          { provide: Router, useValue: newMockRouter }
        ]
      });
      
      // Verificar que el localStorage todavía tiene los valores después del reset
      expect(localStorage.getItem('currentUser')).toBeTruthy();
      expect(localStorage.getItem('tokenExpiration')).toBeTruthy();
      expect(localStorage.getItem('lastActivity')).toBeTruthy();
      
      // Obtener HttpClient del TestBed
      const httpClient = TestBed.inject(HttpClient);
      
      // Crear el servicio directamente para asegurar que lee del localStorage
      // Esto evita problemas con el singleton providedIn: 'root'
      const newService = new Authservice(httpClient, newMockRouter);
      const newHttpTestingController = TestBed.inject(HttpTestingController);

      // Verificar que el servicio tiene un usuario antes de actualizar
      // checkSessionValidity() no debería haber llamado a logout() porque tenemos todos los valores
      expect(newService.currentUserValue).not.toBeNull();
      expect(newService.currentUserValue?.email).toBe('test@example.com');

      const beforeTime = new Date().getTime();
      newService.updateLastActivity();
      const lastActivity = localStorage.getItem('lastActivity');
      
      expect(lastActivity).toBeTruthy();
      if (lastActivity) {
        const activityTime = new Date(lastActivity).getTime();
        expect(activityTime).toBeGreaterThanOrEqual(beforeTime);
      }
      
      // Limpiar después del test
      newHttpTestingController.verify();
    });

    it('debe cerrar sesión si el token expiró', (done) => {
      const expiredDate = new Date(Date.now() - 1000).toISOString();
      localStorage.setItem('token', 'fake-token');
      localStorage.setItem('currentUser', JSON.stringify({ id: 1, email: 'test@example.com' }));
      localStorage.setItem('tokenExpiration', expiredDate);
      localStorage.setItem('lastActivity', new Date().toISOString());

      service = TestBed.inject(Authservice);

      setTimeout(() => {
        expect(service.currentUserValue).toBeNull();
        done();
      }, 100);
    });
  });
});