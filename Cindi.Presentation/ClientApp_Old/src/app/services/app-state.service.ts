import { Injectable } from "@angular/core";
import { NodeDataService } from "./node-data.service";
import { Observable, BehaviorSubject } from "rxjs";
import { map, filter, catchError, mergeMap } from "rxjs/operators";
import { StepTemplateReference } from "../models/step-template-reference";
@Injectable()
export class AppStateService {
  selectedWorkflowTemplate: any;
  selectedWorkflow: any[];
  allStepTemplates: BehaviorSubject<any[]> = new BehaviorSubject<any[]>([]);
  lastLoadedTemplates: Date;
  currentUser: BehaviorSubject<any> = new BehaviorSubject(undefined);

  constructor(private _nodeData: NodeDataService) {
    this.setCurrentUser(JSON.parse(localStorage.getItem("currentUser")));
  }

  setSelectedWorkflowTemplate(workflowTemplate) {
    this.selectedWorkflowTemplate = workflowTemplate;
  }

  setCurrentUser(user) {
    this.currentUser.next(user);
  }

  setSelectedWorkflow(workflow) {
    this.selectedWorkflow = workflow;
  }

  getSelectedWorkflowTemplate() {
    return this.selectedWorkflowTemplate;
  }

  refreshStepTemplateData(): Observable<any[]> {
    return this._nodeData.GetStepTemplates().pipe(
      map(result => {
        this.allStepTemplates.next(result.result);
        return result.result;
      })
    );
  }

  getStepTemplateDef(stepTemplate: StepTemplateReference) {
    if (this.allStepTemplates.value.length > 0) {
      var result = this.allStepTemplates.value.filter(
        s => s.name == stepTemplate.name && s.version == stepTemplate.version
      );

      if (result.length == 1) {
        return result[0];
      }
    }
    return undefined;
  }

  setAllStepTemplates(allTemplates: any[]) {
    this.lastLoadedTemplates = new Date();
    this.allStepTemplates.next(allTemplates);
  }
}
