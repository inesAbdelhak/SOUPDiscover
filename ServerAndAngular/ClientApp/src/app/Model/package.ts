
export enum PackageType {
  nuget,
  npm,
}

export class PackageDto {
  /**
   * Id of the package in database
   */
  id: number;

  /**
    * The name of the package
    */
  packageId: string;

  /**
    * Version of the package
    */
  version: string;

  /**
    * Nuget package or npm package
    */
  packageType: PackageType;
}
