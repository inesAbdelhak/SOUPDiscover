import { RepositoryType } from './repositoryType';

export interface RepositoryDto {
  /** The name of the repository to display and the primary key */
  name?: string;
  /** The type of the repository */
  repositoryType?: RepositoryType;
  /** The url to the git repository */
  url?: string;
  /** The name of sh key to clone the repository */
  sshKeyName?: string;
  /** The name of the branch to process */
  branch?: string;
}

