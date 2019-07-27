export class InputBase<T> {
	value: T;
	id: string;
	type: number;
	description: string;
	additionalOptions: any;
	constructor(options: {
		id?: string,
		type?: number,
		value?: T,
		description?: string,
		additionalOptions? : any
	  } = {}) {
	  this.value = options.value;
	   this.type = options.type;
	   this.id = options.id;
	   this.description = options.description;
	   this.additionalOptions = options.additionalOptions;
	}
}
