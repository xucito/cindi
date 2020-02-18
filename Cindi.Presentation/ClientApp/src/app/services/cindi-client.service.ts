import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { EnvService } from "./env.service";
import { Observable } from "rxjs";
import { Router } from "@angular/router";

@Injectable({
  providedIn: "root"
})
export class CindiClientService {
  private api = "/api/";
  private baseUrl; //= environment.apiUrl; //'http://10.10.10.24:5021/';//'http://localhost:5021/';//'http://10.10.10.24:5021/';// //'http://10.10.10.24:5021/';// 'http://10.10.10.17:5021/';// 'http://localhost:5021/'; //

  constructor(
    private http: HttpClient,
    private env: EnvService,
    private router: Router
  ) /*,
  @Inject('BASE_URL') private baseUrl: string*/
  {
    if (env.dynamicRoutingEnabled) {
      console.log("myURL " + location.origin);
      this.baseUrl = location.origin;
    } else {
      this.baseUrl = env.apiUrl;
    }
  }

  GetSteps(
    status: string = "",
    page: number = 0,
    size: number = 0,
    sort?: string
  ): Observable<any> {
    var queryString = "";
    var hasQueries = false;
    if (status != "") {
      queryString += "status=" + status;
      hasQueries = true;
    }

    if (page != 0) {
      if (hasQueries) {
        queryString += "&";
      }
      queryString += "page=" + page;
      hasQueries = true;
    }

    if (size != 0) {
      if (hasQueries) {
        queryString += "&";
      }
      queryString += "size=" + size;
      hasQueries = true;
    }

    if(sort)
    {
      if (hasQueries) {
        queryString += "&";
      }
      queryString += "sort=" + sort;
      hasQueries = true;
    }

    return this.http.get(
      this.baseUrl + this.api + "steps" + (hasQueries ? "?" : "") + queryString
    );
  }

  PutStep(id: string, status: string): Observable<any> {
    return this.http.put(this.baseUrl + this.api + "steps/" + id + "/status", {
      Status: status
    });
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

  GetCurrentUser(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "users/me");
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

  PostWorkflow(workflow: any): Observable<any> {
    return this.http.post(this.baseUrl + this.api + "workflows", workflow);
  }

  PostWorkflowTemplate(workflowTemplate: any): Observable<any> {
    return this.http.post(
      this.baseUrl + this.api + "workflow-templates",
      workflowTemplate
    );
  }

  GetBotKeys(): Observable<any> {
    return this.http.get(this.baseUrl + this.api + "bot-keys?size=1000");
  }

  UpdateBotKey(
    botKeyId: string,
    isDisabled: boolean = undefined,
    name: string = undefined
  ): Observable<any> {
    return this.http.put(this.baseUrl + this.api + "bot-keys/" + botKeyId, {
      botName: name,
      isDisabled: isDisabled
    });
  }

  DeleteBotKey(botKeyId: string): Observable<any> {
    return this.http.delete(this.baseUrl + this.api + "bot-keys/" + botKeyId);
  }

  GetMetrics(
    from: Date,
    to: Date,
    metricName: string,
    aggs: string[],
    interval: string,
    includeSubcategories: boolean = false
  ): Observable<any> {
    return this.http.post(this.baseUrl + this.api + "metrics/request", {
      from: from,
      to: to,
      metricName: metricName,
      aggs: aggs,
      interval: interval,
      includeSubcategories: includeSubcategories
    });
  }
}
