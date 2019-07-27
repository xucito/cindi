import { InputBase } from "./components/form/input/input-base";
import { IntInput } from "./components/form/input/types/int-input";
import { SecretInput } from "./components/form/input/types/secret-input";

export function ConvertStepTemplateToInputs(
  stepTemplate: any,
  step: any = undefined,
  currentUser: any = undefined
): InputBase<any>[] {
  let inputs: InputBase<any>[] = [];
  Object.keys(stepTemplate.inputDefinitions).forEach(element => {
    if (element == "secret") {
      inputs.push(
        new SecretInput({
          id: element,
          type: stepTemplate.inputDefinitions[element].type,
          description: stepTemplate.inputDefinitions[element].description,
          value: step != undefined ? step.inputs[element] : undefined,
          additionalOptions: {
            isUnencryptable: currentUser != undefined && step.createdBy == currentUser.username
          }
        })
      );
    } else {
      inputs.push(
        new IntInput({
          id: element,
          type: stepTemplate.inputDefinitions[element].type,
          description: stepTemplate.inputDefinitions[element].description,
          value: step != undefined ? step.inputs[element] : undefined
        })
      );
    }
  });

  return inputs;
}

export function ConvertStepTemplateToOutputs(
  stepTemplate: any,
  step: any = undefined,
  currentUser: any = undefined
): InputBase<any>[] {
  let outputs: InputBase<any>[] = [];
  Object.keys(stepTemplate.outputDefinitions).forEach(element => {
    if (element == "secret") {
      outputs.push(
        new SecretInput({
          id: element,
          type: stepTemplate.outputDefinitions[element].type,
          description: stepTemplate.outputDefinitions[element].description,
          value: step != undefined ? step.outputs[element] : undefined,
          additionalOptions: {
            isUnencryptable: currentUser != undefined && step.createdBy == currentUser.username
          }
        })
      );
    } else {
      outputs.push(
        new IntInput({
          id: element,
          type: stepTemplate.outputDefinitions[element].type,
          description: stepTemplate.outputDefinitions[element].description,
          value: step != undefined ? step.outputs[element] : undefined
        })
      );
    }
  });

  return outputs;
}

export enum InputDataType {
  Int = "int",
  String = "string",
  Bool = "bool",
  Object = "object",
  ErrorMessage = "errormessage",
  Decimal = "decimal",
  DateTime = "datetime",
  Secret = "secret"
}

export function IsStepComplete(step: any) {
  switch (step.status) {
    case "unassigned":
      return false;
    case "assigned":
      return false;
    case "successful":
      return true;
    case "warning":
      return true;
    case "error":
      return true;
    case "unknown":
      return false;
  }
}
