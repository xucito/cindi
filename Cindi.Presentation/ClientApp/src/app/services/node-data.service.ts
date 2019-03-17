import { Component, Inject, Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";

@Injectable()
export class NodeDataService {
  private api = "api/";
  private baseUrl = "http://localhost:5021/"; //'http://10.10.10.24:5021/';//'http://localhost:5021/';//'http://10.10.10.24:5021/';// //'http://10.10.10.24:5021/';// 'http://10.10.10.17:5021/';// 'http://localhost:5021/'; //

  constructor(
    private http: HttpClient /*,
    @Inject('BASE_URL') private baseUrl: string*/
  ) {}

  GetSteps(status: string): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "steps?status=" + status);
  }

  GetStepTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "step-templates");
  }

  GetStepTemplate(name: string, version: string): Observable<any> {
    return this.http.get(
      this.baseUrl + this.api + "step-templates/" + name + "/" + version
    );
  }

  GetSequences(status: string = null): Observable<any> {
    return this.http.get(
      this.baseUrl +
        this.api +
        "sequences" +
        (status != null ? "?status=" + status : "")
    );
  }

  GetSequence(id: string = null) {
    return this.http.get(this.baseUrl + this.api + "sequences/" + id);
  }

  GetSequenceSteps(id: string = null) {
    return this.http.get(
      this.baseUrl + this.api + "sequences/" + id + "/steps"
    );
  }

  GetSequenceTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "sequence-templates");
  }

  PostStep(step: any): Observable<any> {
    return this.http.post(this.baseUrl + this.api + "steps", step);
  }

  GetStats(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "cluster/stats");
  }
}
