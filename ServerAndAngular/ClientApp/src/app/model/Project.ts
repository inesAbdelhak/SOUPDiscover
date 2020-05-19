import { RepositoryDto } from "./repository";
import { PackageDto, PackageType } from "./package";

export class ProjectDto {
  name: string;
  commandLinesBeforeParse: string;
  repositoryId: string;
  packegeTypes: PackageType[];
}

/**
 * Project with found packages
 * */
export class ProjectWithDetailsDto implements ProjectDto {
  packegeTypes: PackageType[];
  name: string;
  commandLinesBeforeParse: string;
  repositoryId: string;
  packages: PackageDto[];
}