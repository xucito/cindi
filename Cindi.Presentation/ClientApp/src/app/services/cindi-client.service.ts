import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { EnvService } from "./env.service";
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root"
})
export class CindiClientService {
  private api = "/api/";
  private baseUrl; //= environment.apiUrl; //'http://10.10.10.24:5021/';//'http://localhost:5021/';//'http://10.10.10.24:5021/';// //'http://10.10.10.24:5021/';// 'http://10.10.10.17:5021/';// 'http://localhost:5021/'; //

  constructor(
    private http: HttpClient,
    private env: EnvService /*,
  @Inject('BASE_URL') private baseUrl: string*/
  ) {
    this.baseUrl = env.apiUrl;
  }

  GetSteps(status: string = ""): Observable<any> {
    if (status != "")
      return this.http.get(this.baseUrl + this.api + "steps?status=" + status);
    else {
      return this.http.get(this.baseUrl + this.api + "steps?status=" + status);
    }
  }

  GetStep(stepId: string): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "steps/" + stepId);
  }

  GetStepTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "step-templates");
  }

  GetStepTemplate(name: string, version: string): Observable<any> {
    return this.http.get(
      this.baseUrl + this.api + "step-templates/" + name + "/" + version
    );
  }

  GetWorkflows(status: string = null): Observable<any> {
    return this.http.get(
      this.baseUrl +
        this.api +
        "workflows" +
        (status != null ? "?status=" + status : "")
    );
  }

  GetWorkflow(id: string = null) {
    return this.http.get(this.baseUrl + this.api + "workflows/" + id);
  }

  GetWorkflowSteps(id: string = null) {
    return this.http.get(
      this.baseUrl + this.api + "workflows/" + id + "/steps"
    );
  }

  GetWorkflowTemplates(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "workflow-templates");
  }

  PostStep(step: any): Observable<any> {
    return this.http.post(this.baseUrl + this.api + "steps", step);
  }

  GetStats(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "cluster/stats");
  }

  GetSecret(stepId: string, type: string, fieldName: string): Observable<any> {
    return this.http.get(
      this.baseUrl +
        this.api +
        "encryption/steps/" +
        stepId +
        "/" +
        type +
        "/" +
        fieldName
    );
  }

  GetUsers(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "users");
  }

  PostUser(user: any) {
    return this.http.post(this.baseUrl + this.api + "users", user);
  }

  GetGlobalValues(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "global-values");
  }

  PostGlobalValues(gv: any): Observable<any> {
    return this.http.post(this.baseUrl + this.api + "global-values", gv);
  }
}