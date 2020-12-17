import { RepositoryType } from './repositoryType';

export class RepositoryDto {

  public static CreateFromData(data): RepositoryDto {
    let repo = new RepositoryDto();
    repo.name = data.name;
    repo.repositoryType = (<any>RepositoryType)[data.repositoryType];
    repo.url = data.url;
    repo.credentialId = data.credentialId;
    repo.branch = data.branch;
    return repo;
  }

  /**
   * The name of the repository to display and the primary key
   * */
  name?: string;
  /**
   *  The type of the repository
   * */
  repositoryType?: RepositoryType;
  /**
   *  The url to the git repository
   * */
  url?: string;
  /**
   *  The name of sh key to clone the repository
   * */
  credentialId?: string;
  /**
   * The name of the branch to process
   * */
  branch?: string;
}

