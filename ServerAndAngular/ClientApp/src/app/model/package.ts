
export enum PackageType {
  nuget,
  npm,
}

export class PackageDto {

  static CreateFromData(pa): PackageDto {
    var newPackage: PackageDto = new PackageDto();
    newPackage.id = pa.id;
    newPackage.packageId = pa.packageId;
    newPackage.version = pa.version;
    newPackage.licence = pa.licence;
    newPackage.description = pa.description;
    newPackage.packageType = pa.packageType;
    newPackage.projectUrl = pa.projectUrl;
    return newPackage;
  }
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
   * The package licence.
   * */
  licence: string;

  /**
   * Description of the package
   */
  description: string;

  /**
    * Nuget package or npm package
    */
  packageType: PackageType;

  /**
   * The project Url of this package.
   * Can be empty.
   */
  projectUrl: string;

  get isLicenceUrl() {
    return this.licence?.startsWith('http') === true;
  }
}
