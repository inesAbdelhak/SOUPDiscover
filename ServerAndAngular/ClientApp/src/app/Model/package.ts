
export enum PackageType {
  nuget,
  npm,
}

export class PackageDto {
  /**
   * Id of the package in database
   */
  Id: number;

  /**
    * The name of the package
    */
  PackageId: string;

  /**
    * Version of the package
    */
  Version: string;

  /**
    * Nuget package or npm package
    */
  PackageType: PackageType;
}
