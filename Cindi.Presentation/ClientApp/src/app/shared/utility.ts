import { InputBase } from "./components/form/input/input-base";
import { IntInput } from "./components/form/input/types/int-input";

export function ConvertStepTemplateToInputs(stepTemplate: any): InputBase<any>[]
{
	let inputs: InputBase<any>[] = [];
	Object.keys(stepTemplate.inputDefinitions).forEach(element => {
		if(stepTemplate.inputDefinitions[element].type == 'int')
		{
			inputs.push(new IntInput({
				id: element,
				type: 0,
				description: stepTemplate.inputDefinitions[element].description
			}))
		}
		else
		{
			inputs.push(new IntInput({
				id: element,
				type: stepTemplate.inputDefinitions[element].type,
				description: stepTemplate.inputDefinitions[element].description
			}))
		}
	});

	return inputs;
}

export enum InputDataType {
	Int,
	String,
	Bool,
	Object,
	ErrorMessage,
	Decimal,
	DateTime
}