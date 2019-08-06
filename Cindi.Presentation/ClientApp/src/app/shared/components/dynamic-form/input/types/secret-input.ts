import { InputBase } from "../input-base";

export class SecretInput extends InputBase<string> {
  controlType = "textbox";
  isUnencryptable = false;

  constructor(options: any = {}) {
    super(options);
    if (options.additionalOptions.isUnencryptable != undefined) {
      this.isUnencryptable = options.additionalOptions.isUnencryptable;
    }
  }
}
