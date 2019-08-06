import { InputBase } from '../components/dynamic-form/input/input-base';
import { SecretInput } from '../components/dynamic-form/input/types/secret-input';
import { IntInput } from '../components/dynamic-form/input/types/int-input';

export function ConvertTemplateToInputs(
  template: any,
  step: any = undefined,
  currentUser: any = undefined
): InputBase<any>[] {
  let inputs: InputBase<any>[] = [];
  if(template.inputDefinitions == undefined)
  {
    return inputs;
  }
  Object.keys(template.inputDefinitions).forEach(element => {
    if (element == "secret") {
      inputs.push(
        new SecretInput({
          id: element,
          type: template.inputDefinitions[element].type,
          description: template.inputDefinitions[element].description,
          value: step != undefined ? step.inputs != null ? step.inputs[element] : undefined : undefined,
          additionalOptions: {
            isUnencryptable: currentUser != undefined && step.createdBy == currentUser.username
          }
        })
      );
    } else {
      inputs.push(
        new IntInput({
          id: element,
          type: template.inputDefinitions[element].type,
          description: template.inputDefinitions[element].description,
          value: step != undefined ? step.inputs != null ? step.inputs[element] : undefined : undefined
        })
      );
    }
  });

  return inputs;
}

export function ConvertTemplateToOutputs(
  template: any,
  step: any = undefined,
  currentUser: any = undefined
): InputBase<any>[] {
  let outputs: InputBase<any>[] = [];
  if(template.outputDefinitions == undefined)
  {
    return outputs;
  }
  Object.keys(template.outputDefinitions).forEach(element => {
    if (element == "secret") {
      outputs.push(
        new SecretInput({
          id: element,
          type: template.outputDefinitions[element].type,
          description: template.outputDefinitions[element].description,
          value: step != undefined ? step.outputs != null ? step.outputs[element] : undefined : undefined,
          additionalOptions: {
            isUnencryptable: currentUser != undefined && step.createdBy == currentUser.username
          }
        })
      );
    } else {
      outputs.push(
        new IntInput({
          id: element,
          type: template.outputDefinitions[element].type,
          description: template.outputDefinitions[element].description,
          value: step != undefined ? step.outputs != null ? step.outputs[element]: undefined : undefined
        })
      );
    }
  });

  return outputs;
}

