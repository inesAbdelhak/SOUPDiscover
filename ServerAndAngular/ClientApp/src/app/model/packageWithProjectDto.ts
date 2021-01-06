import { PackageDto } from "./package";
import { PackageConsumerDto } from "./PackageConsumerDto";

export class PackageWithProjectDto {
  /**
   * The package
   * */
  packageDto: PackageDto;

  /*
   * All project name where is found the package
   */
  packageConsumers: PackageConsumerDto[];
}
