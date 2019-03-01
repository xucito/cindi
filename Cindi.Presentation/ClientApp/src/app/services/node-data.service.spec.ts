import { TestBed, inject } from '@angular/core/testing';

import { NodeDataService } from './node-data.service';

describe('NodeDataService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [NodeDataService]
    });
  });

  it('should be created', inject([NodeDataService], (service: NodeDataService) => {
    expect(service).toBeTruthy();
  }));
});
