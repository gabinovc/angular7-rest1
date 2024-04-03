import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {CleanupPanelComponent} from './adjustment-panel.component';

describe('CleanupPanelComponent', () => {
  let component: CleanupPanelComponent;
  let fixture: ComponentFixture<CleanupPanelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CleanupPanelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CleanupPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
