import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { EquipoFantasyList } from './equipos-fantasy-list';
import { EquipoFantasyService } from '../../services/equipo-fantasy.service';
import { Authservice } from '../../services/authservice';

describe('EquipoFantasyList', () => {
  let component: EquipoFantasyList;
  let fixture: ComponentFixture<EquipoFantasyList>;
  let mockEquipoFantasyService: jasmine.SpyObj<EquipoFantasyService>;
  let mockAuthService: jasmine.SpyObj<Authservice>;

  beforeEach(async () => {
    mockEquipoFantasyService = jasmine.createSpyObj('EquipoFantasyService', ['obtenerTodos']);
    mockAuthService = jasmine.createSpyObj('Authservice', [], {
      currentUserValue: { id: 1, email: 'test@example.com', nombreCompleto: 'Test User' }
    });

    await TestBed.configureTestingModule({
      imports: [EquipoFantasyList, HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: EquipoFantasyService, useValue: mockEquipoFantasyService },
        { provide: Authservice, useValue: mockAuthService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EquipoFantasyList);
    component = fixture.componentInstance;
  });

  it('debe crear el componente', () => {
    expect(component).toBeTruthy();
  });
});
