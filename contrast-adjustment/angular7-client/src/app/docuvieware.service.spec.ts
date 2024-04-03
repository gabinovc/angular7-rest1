import {TestBed} from '@angular/core/testing';

import {DocuviewareService} from './docuvieware.service';

describe('DocuviewareService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: DocuviewareService = TestBed.get(DocuviewareService);
    expect(service).toBeTruthy();
  });
});
