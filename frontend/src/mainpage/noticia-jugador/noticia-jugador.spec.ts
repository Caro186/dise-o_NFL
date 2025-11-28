import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoticiaJugadorComponent } from './noticia-jugador';

describe('NoticiaJugadorComponent', () => {
  let component: NoticiaJugadorComponent;
  let fixture: ComponentFixture<NoticiaJugadorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoticiaJugadorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NoticiaJugadorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});