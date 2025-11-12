import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JugadoresBatch } from './jugadores-batch';

describe('JugadoresBatch', () => {
  let component: JugadoresBatch;
  let fixture: ComponentFixture<JugadoresBatch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JugadoresBatch]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JugadoresBatch);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
