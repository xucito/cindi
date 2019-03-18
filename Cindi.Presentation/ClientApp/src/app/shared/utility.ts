import { InputBase } from "./components/form/input/input-base";
import { IntInput } from "./components/form/input/types/int-input";

export function ConvertStepTemplateToInputs(stepTemplate: any, step: any = undefined): InputBase<any>[]
{
	let inputs: InputBase<any>[] = [];
	Object.keys(stepTemplate.inputDefinitions).forEach(element => {
		if(stepTemplate.inputDefinitions[element].type == 'int')
		{
			inputs.push(new IntInput({
				id: element,
				type: 0,
				description: stepTemplate.inputDefinitions[element].description,
				value: step.inputs[element]
			}))
		}
		else
		{
			inputs.push(new IntInput({
				id: element,
				type: stepTemplate.inputDefinitions[element].type,
				description: stepTemplate.inputDefinitions[element].description,
				value: step.inputs[element]
			}))
		}
	});

	return inputs;
}

export enum InputDataType {
	Int = 'int',
	String = 'string',
	Bool = 'bool',
	Object = 'object',
	ErrorMessage = 'errormessage',
	Decimal = 'decimal',
	DateTime = 'datetime',
	Secret = 'secret'
}