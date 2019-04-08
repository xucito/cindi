import { TestBed, inject } from '@angular/core/testing';

import { InputControlServiceService } from './input-control-service.service';

describe('InputControlServiceService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [InputControlServiceService]
    });
  });

  it('should be created', inject([InputControlServiceService], (service: InputControlServiceService) => {
    expect(service).toBeTruthy();
  }));
});
