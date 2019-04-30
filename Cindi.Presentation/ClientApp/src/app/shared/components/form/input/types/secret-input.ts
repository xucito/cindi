import { InputBase } from "../input-base";

export class SecretInput extends InputBase<string>  {
	controlType = 'textbox';
	isUnencryptable = false;
   // type: number;
  
	constructor(options: any = {}) {
	  super(options);
	  if(options.additionalOptions.isUnencryptable != undefined)
	  {
		this.isUnencryptable = options.additionalOptions.isUnencryptable;
	  }
	  //this.type = options['type'] || '';
	}
  }