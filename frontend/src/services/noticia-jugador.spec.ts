import { TestBed } from '@angular/core/testing';

import { NoticiaJugador } from './noticia-jugador';

describe('NoticiaJugador', () => {
  let service: NoticiaJugador;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NoticiaJugador);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
