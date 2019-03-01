import { Injectable } from "@angular/core";
import { NodeDataService } from "./node-data.service";
import { Observable, BehaviorSubject } from "rxjs";
import { map, filter, catchError, mergeMap } from 'rxjs/operators';
import { StepTemplateReference } from "../models/step-template-reference";
@Injectable()
export class AppStateService {
  selectedSequenceTemplate: any;
  selectedSequence: any[];
  allStepTemplates: BehaviorSubject<any[]> = new BehaviorSubject<any[]>([]);
  lastLoadedTemplates: Date;

  constructor(private _nodeData: NodeDataService) {}

  setSelectedSequenceTemplate(sequenceTemplate) {
    this.selectedSequenceTemplate = sequenceTemplate;
  }

  setSelectedSequence(sequence){
    this.selectedSequence = sequence;
  }

  getSelectedSequenceTemplate() {
    return this.selectedSequenceTemplate;
  }

  refreshStepTemplateData(): Observable<any[]> {
    return this._nodeData.GetStepTemplates().pipe(map(result => {
      this.allStepTemplates.next(result);
      return result;
    }));
  }

  getStepTemplateDef(stepTemplate: StepTemplateReference) {
    if (this.allStepTemplates.value.length > 0) {
      var result = this.allStepTemplates.value.filter(
        s => (s.name == stepTemplate.name && s.version == stepTemplate.version) 
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
