import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {DocuviewareComponent} from './docuvieware.component';

describe('DocuviewareComponent', () => {
  let component: DocuviewareComponent;
  let fixture: ComponentFixture<DocuviewareComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DocuviewareComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DocuviewareComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
