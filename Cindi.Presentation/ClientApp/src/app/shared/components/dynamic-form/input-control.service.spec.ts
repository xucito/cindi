import { TestBed } from '@angular/core/testing';

import { InputControlService } from './input-control.service';

describe('InputControlService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: InputControlService = TestBed.get(InputControlService);
    expect(service).toBeTruthy();
  });
});
