import { RepositoryDto } from "./repository";
import { PackageDto } from "./package";

export class ProjectDto {
  name: string;
  commandLinesBeforeParse: string;
  repositoryId: string;
}

/**
 * Project with found packages
 * */
export class ProjectWithDetailsDto implements ProjectDto {
  name: string;
  commandLinesBeforeParse: string;
  repositoryId: string;
  packages: PackageDto[];
}
