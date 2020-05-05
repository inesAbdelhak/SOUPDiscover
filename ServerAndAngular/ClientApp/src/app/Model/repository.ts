export enum RepositoryType {
  None,
  Git,
}

export interface RepositoryDto {
  /** The id of the repository in database */
  id?: number;
  /** The type of the repository */
  repositoryType?: RepositoryType;
  /** The name of the repository to display */
  name?: string;
  /** The url to the git repository */
  url?: string;
  /** The name of sh key to clone the repository */
  sshKeyName?: string;
  /** The name of the branch to process */
  branch?: string;
  /** The token of the token, to request GitHUB  */
  tokenName?: string;
}

