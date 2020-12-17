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
}
