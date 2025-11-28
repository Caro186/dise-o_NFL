import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VerNoticiasComponent } from './ver-noticias';

describe('VerNoticiasComponent', () => {
  let component: VerNoticiasComponent;
  let fixture: ComponentFixture<VerNoticiasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VerNoticiasComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VerNoticiasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
