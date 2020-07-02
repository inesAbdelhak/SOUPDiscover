import { RepositoryDto } from './repository';
import { PackageDto, PackageType } from './package';
import { ProcessStatus } from './processStatus';

export class ProjectDto {

  public static CreateFromData(data): ProjectDto {
    if (data == null) {
      return null;
    }
    let project = new ProjectDto();
    project.name = data.name;
    project.commandLinesBeforeParse = data.commandLinesBeforeParse;
    project.repositoryId = data.repositoryId;
    project.nugetServerUrl = data.nugetServerUrl;
    project.lastAnalysisError = data.lastAnalysisError;
    project.processStatus = (<any>ProcessStatus)[data.processStatus];
    if (data.lastAnalysisDate != null) {
      project.lastAnalysisDate = new Date(data.lastAnalysisDate);
    }
    return project;
  }

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

