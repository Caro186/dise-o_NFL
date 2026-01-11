import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { Authservice } from '../services/authservice';

describe('authGuard', () => {
  let mockAuthService: jasmine.SpyObj<Authservice>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('Authservice', ['isLoggedIn']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: Authservice, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('debe permitir acceso cuando el usuario está logueado', () => {
    mockAuthService.isLoggedIn.and.returnValue(true);
    
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    
    expect(result).toBe(true);
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('debe denegar acceso y redirigir al login cuando el usuario no está logueado', () => {
    mockAuthService.isLoggedIn.and.returnValue(false);
    
    spyOn(window, 'alert');
    
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    
    expect(result).toBe(false);
    expect(window.alert).toHaveBeenCalledWith('Debes iniciar sesión para acceder a esta página.');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('debe verificar el estado de autenticación antes de permitir acceso', () => {
    mockAuthService.isLoggedIn.and.returnValue(true);
    
    TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
    
    expect(mockAuthService.isLoggedIn).toHaveBeenCalled();
  });
});
