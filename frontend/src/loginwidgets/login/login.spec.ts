import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { Authservice } from '../../services/authservice';

describe('Login Component', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let mockAuthService: jasmine.SpyObj<Authservice>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('Authservice', ['login', 'isLoggedIn']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [Login, ReactiveFormsModule],
      providers: [
        { provide: Authservice, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debe inicializar el formulario con campos vacíos', () => {
    expect(component.loginForm.get('email')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('debe marcar el email como inválido si está vacío', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('');
    expect(emailControl?.hasError('required')).toBeTruthy();
  });

  it('debe marcar el email como inválido si el formato es incorrecto', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('emailinvalido');
    expect(emailControl?.hasError('email')).toBeTruthy();
  });

  it('debe marcar la contraseña como inválida si tiene menos de 8 caracteres', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('Pass1');
    expect(passwordControl?.hasError('minlength')).toBeTruthy();
  });

  it('debe marcar la contraseña como inválida si no tiene mayúsculas', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('password123');
    expect(passwordControl?.hasError('passwordInvalid')).toBeTruthy();
  });

  it('debe marcar la contraseña como inválida si no tiene minúsculas', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('PASSWORD123');
    expect(passwordControl?.hasError('passwordInvalid')).toBeTruthy();
  });

  it('debe marcar la contraseña como inválida si tiene caracteres especiales', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('Pass@123');
    expect(passwordControl?.hasError('passwordInvalid')).toBeTruthy();
  });

  it('debe marcar el formulario como válido con datos correctos', () => {
    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    expect(component.loginForm.valid).toBeTruthy();
  });

  it('no debe enviar el formulario si es inválido', () => {
    component.loginForm.get('email')?.setValue('');
    component.loginForm.get('password')?.setValue('');
    component.onSubmit();
    expect(mockAuthService.login).not.toHaveBeenCalled();
  });

  it('debe llamar al servicio de login con las credenciales correctas', () => {
    const mockResponse = { status: 'ok', usuario: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User', fechaRegistro: new Date() } };
    mockAuthService.login.and.returnValue(of(mockResponse));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(mockAuthService.login).toHaveBeenCalledWith({
      email: 'test@example.com',
      password: 'Password123'
    });
  });

  it('debe redirigir a mainpage después de login exitoso', () => {
    const mockResponse = { status: 'ok', usuario: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User', fechaRegistro: new Date() } };
    mockAuthService.login.and.returnValue(of(mockResponse));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/mainpage']);
  });

  it('debe mostrar mensaje de error con credenciales incorrectas (401)', () => {
    const mockError = { status: 401, error: { mensaje: 'Credenciales incorrectas' } };
    mockAuthService.login.and.returnValue(throwError(() => mockError));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(component.serverError).toBe('Usuario o contraseña incorrectos');
  });

  it('debe mostrar mensaje de cuenta bloqueada (403)', () => {
    const mockError = { status: 403, error: { mensaje: 'Cuenta bloqueada' } };
    mockAuthService.login.and.returnValue(throwError(() => mockError));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(component.serverError).toBe('Cuenta bloqueada');
  });

  it('debe mostrar mensaje de error de conexión (status 0)', () => {
    const mockError = { status: 0 };
    mockAuthService.login.and.returnValue(throwError(() => mockError));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(component.serverError).toBe('No se puede conectar con el servidor. Verifica que el backend esté corriendo.');
  });

  it('debe limpiar el mensaje de error al enviar el formulario nuevamente', () => {
    component.serverError = 'Error anterior';
    const mockResponse = { status: 'ok', usuario: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User', fechaRegistro: new Date() } };
    mockAuthService.login.and.returnValue(of(mockResponse));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('Password123');
    component.onSubmit();

    expect(component.serverError).toBe('');
  });
});