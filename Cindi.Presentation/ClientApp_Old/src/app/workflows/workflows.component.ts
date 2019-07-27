import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { NodeDataService } from '../services/node-data.service';
import { Router, ActivatedRoute } from '@angular/router';
import { AppStateService } from '../services/app-state.service';

@Component({
  selector: 'app-workflows',
  templateUrl: './workflows.component.html',
  styleUrls: ['./workflows.component.css']
})
export class WorkflowsComponent implements OnInit {

  public workflows: any[];
  private workflows$: Subscription;

  columnsToDisplay = ['id','name', //'version', 
  'createdOn', 'status'];

  run: any;

  constructor(
    private nodeData: NodeDataService,
    private _router: Router,
    private _route: ActivatedRoute,
    private _appState: AppStateService
  )  {
    this.workflows$ = nodeData.GetWorkflows().subscribe(
      (result) => {
        this.workflows = result.result;
        this.workflows = this.workflows.sort(function(obj1, obj2) {
          // Ascending: first age less than the previous
          return obj1.id - obj2.id;
        });
      }
    );
  }

  selectWorkflow(selectedRow)
  {
    this._appState.setSelectedWorkflow(selectedRow);

    this._router.navigate([selectedRow.id], {
      relativeTo: this._route
    });
  }

  ngOnInit() {
  }

}
