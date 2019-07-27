import { TestBed } from '@angular/core/testing';

import { CindiClientService } from './cindi-client.service';

describe('CindiClientService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: CindiClientService = TestBed.get(CindiClientService);
    expect(service).toBeTruthy();
  });
});
