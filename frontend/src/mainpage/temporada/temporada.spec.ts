import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TemporadaComponent } from './temporada';

describe('TemporadaComponent', () => {
  let component: TemporadaComponent;
  let fixture: ComponentFixture<TemporadaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TemporadaComponent, HttpClientTestingModule]
    })
      .compileComponents();

    fixture = TestBed.createComponent(TemporadaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
