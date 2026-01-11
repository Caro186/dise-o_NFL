import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { SidenavComponent } from './sidenav';
import { Authservice } from '../../services/authservice';

describe('SidenavComponent', () => {
  let component: SidenavComponent;
  let fixture: ComponentFixture<SidenavComponent>;
  let mockAuthService: jasmine.SpyObj<Authservice>;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('Authservice', ['isAdmin', 'logout'], {
      currentUserValue: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' }
    });

    await TestBed.configureTestingModule({
      imports: [SidenavComponent, RouterTestingModule],
      providers: [
        { provide: Authservice, useValue: mockAuthService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SidenavComponent);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });
});
