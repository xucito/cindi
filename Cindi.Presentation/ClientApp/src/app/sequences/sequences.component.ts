import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { NodeDataService } from '../services/node-data.service';
import { Router, ActivatedRoute } from '@angular/router';
import { AppStateService } from '../services/app-state.service';

@Component({
  selector: 'app-sequences',
  templateUrl: './sequences.component.html',
  styleUrls: ['./sequences.component.css']
})
export class SequencesComponent implements OnInit {

  public sequences: any[];
  private sequences$: Subscription;

  columnsToDisplay = ['id','name', //'version', 
  'createdOn', 'status'];

  run: any;

  constructor(
    private nodeData: NodeDataService,
    private _router: Router,
    private _route: ActivatedRoute,
    private _appState: AppStateService
  )  {
    this.sequences$ = nodeData.GetSequences('started').subscribe(
      (result) => {
        this.sequences = result;
        this.sequences = this.sequences.sort(function(obj1, obj2) {
          // Ascending: first age less than the previous
          return obj1.id - obj2.id;
        });
      }
    );
  }

  selectSequence(selectedRow)
  {
    this._appState.setSelectedSequence(selectedRow);

    this._router.navigate([selectedRow.id], {
      relativeTo: this._route
    });
  }

  ngOnInit() {
  }

}
