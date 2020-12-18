export enum CredentialType {
  ssh,
  password,
}

export class CredentialDto {
  /**
   * The primary key of the credential
   */
  name: string;

  /**
   * The credential key (not downloaded, but only uploaded)
   */
  key: string;

  /**
   * The login to use, to clone the repository
   */
  login: string;

  /**
   * The password to use to clone the repository
   */
  password: string;

  credentialType: CredentialType;

  public get credentialTypeName(): string {
    return CredentialType[this.credentialType];
  }

  static CreateEmptyCredential(): CredentialDto {
    var newObject = new CredentialDto();
    newObject.name = '';
    return newObject;
  }

  static CreateFromData(data): CredentialDto {
    var newobject: CredentialDto = new CredentialDto();
    newobject.key = data.key;
    newobject.login = data.login;
    newobject.name = data.name;
    newobject.password = data.password;
    newobject.credentialType = data.credentialType;
    return newobject;
  }
}
