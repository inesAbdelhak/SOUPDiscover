import { RepositoryDto } from "./repository";
import { PackageDto, PackageType } from "./package";

export class ProjectDto {
  /**
   * The primary  key of the project
   * */
  name: string;

  /**
   * The repository where search files
   * */
  commandLinesBeforeParse: string;

  /**
   * The foreign key to the repository definition
   */
  repositoryId: string;

  /**
   * List of Url nuget server where find metadata of packages
   */
  nugetServerUrl: string;

  /**
   * The last analysis error
   */
  lastAnalysisError?: string;

  /**
   * The status of executing
   */
  processStatus?: ProcessStatus;

  /**
   * Date of the last analysis
   */
  lastAnalysisDate?: Date;
}

export enum ProcessStatus {
  /**
   * No started
   */
  Waiting,

  /**
   * The last processing return an error
   * */
  Error,

  /**
   * Processing
   * */
  Running,
}
