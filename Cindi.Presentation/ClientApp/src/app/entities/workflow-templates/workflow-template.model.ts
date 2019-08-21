export class WorkflowTemplate {
  id: string;
  referenceId: string = '';
  name: string = '';
  version: number = 0;
  description: string = '';
  logicBlocks: any;
  inputDefinitions: any;
  createdBy: string;
  createdOn: Date;
}
