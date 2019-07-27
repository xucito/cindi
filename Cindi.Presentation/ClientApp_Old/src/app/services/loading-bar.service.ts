import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingBarService {
  private loadingQueue: string[] = [];
  IsLoading = new BehaviorSubject<boolean>(false);

  constructor() { }

  public AddLoadingItem(): string
  {
    let id = makeid();
    this.loadingQueue.push(id);
    this.IsLoading.next(true);
    return id;
  }

  public RemoveLoadingItem(id)
  {
    this.loadingQueue = this.loadingQueue.filter(lq => lq != id);
    this.IsLoading.next(this.loadingQueue.length > 0);
  }
}

function makeid() {
  var text = "";
  var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

  for (var i = 0; i < 5; i++)
    text += possible.charAt(Math.floor(Math.random() * possible.length));

  return text;
}
