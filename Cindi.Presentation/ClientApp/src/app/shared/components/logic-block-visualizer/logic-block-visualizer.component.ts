import { Component, OnInit, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'logic-block-visualizer',
  templateUrl: './logic-block-visualizer.component.html',
  styleUrls: ['./logic-block-visualizer.component.css']
})
export class LogicBlockVisualizerComponent implements OnInit, OnChanges {
  ngOnChanges(changes: import("@angular/core").SimpleChanges): void {
  }

  @Input() logicBlock;
  @Input() otherSteps;
  @Input() allPossibleMappings;

  @Input() selectedStep: any;
  @Input() selectedStepTemplate: any;

  constructor() { }

  ngOnInit() {
  }

}
