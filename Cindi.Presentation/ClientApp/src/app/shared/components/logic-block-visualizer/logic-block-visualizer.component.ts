import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'logic-block-visualizer',
  templateUrl: './logic-block-visualizer.component.html',
  styleUrls: ['./logic-block-visualizer.component.css']
})
export class LogicBlockVisualizerComponent implements OnInit {

  @Input() logicBlock;
  @Input() steps;

  constructor() { }

  ngOnInit() {
  }

}
