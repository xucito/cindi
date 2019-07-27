import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-status-card',
  templateUrl: './status-card.component.html',
  styleUrls: ['./status-card.component.css']
})
export class StatusCardComponent implements OnInit {

  _title: string;
  _icon: string;
  _body: string;

  get Title(){return this._title;}
  
  @Input()
  set Title(text: string)
  {
    this._title = text;
  }

  get Icon(){return this._icon;}
  
  @Input()
  set Icon(text: string)
  {
    this._icon = text;
  }

  get Body(){return this._body;}
  
  @Input()
  set Body(text: string)
  {
    this._body = text;
  }

  constructor() { }

  ngOnInit() {
  }

}
