import { Component, OnInit, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'logic-block-visualizer',
  templateUrl: './logic-block-visualizer.component.html',
  styleUrls: ['./logic-block-visualizer.component.css']
})
export class LogicBlockVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
    this.selectedStep = undefined;
  }

  @Input() logicBlock;
  @Input() otherSteps;

  selectedStep: any;

  constructor() { }

  ngOnInit() {
  }

}
