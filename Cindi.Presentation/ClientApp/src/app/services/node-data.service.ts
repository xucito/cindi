import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class NodeDataService {
  private api = 'api/';
  private baseUrl = 'http://localhost:5021/';//'http://10.10.10.24:5021/';//'http://localhost:5021/';//'http://10.10.10.24:5021/';// //'http://10.10.10.24:5021/';// 'http://10.10.10.17:5021/';// 'http://localhost:5021/'; //

  constructor(private http: HttpClient/*,
    @Inject('BASE_URL') private baseUrl: string*/) {
  }

  GetSteps(status: string): Observable<any> {
    return this.http.get(this.baseUrl + this.api + 'steps/' + status);
  }

  GetStepTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + 'stepTemplates');
  }

  GetStepTemplate( name: string, version: string): Observable<any> {
    return this.http.get(this.baseUrl + this.api + 'stepTemplates/' + name + '/' + version);
  }

  GetSequences(status: string): Observable<any> {
    return this.http.get(this.baseUrl + this.api + 'sequences?status='+ status);
  }

  GetSequence(id: string = null)
  {
    return this.http.get(this.baseUrl + this.api + 'sequences?id='+id);
  }

  GetSequenceTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + 'sequencetemplates');
  }

  PostStep(step: any): Observable<any> {
    return this.http.post(this.baseUrl + this.api + 'steps', step);
  }
}
