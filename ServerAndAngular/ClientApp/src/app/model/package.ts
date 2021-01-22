
export enum PackageType {
  nuget,
  npm,
}

export enum LicenseType {
  file,
  url,
  expression,
}

export class PackageDto {

  static CreateFromData(pa): PackageDto {
    var newPackage: PackageDto = new PackageDto();
    newPackage.id = pa.id;
    newPackage.packageId = pa.packageId;
    newPackage.version = pa.version;
    newPackage.license = pa.license;
    newPackage.description = pa.description;
    newPackage.packageType = pa.packageType;
    newPackage.projectUrl = pa.projectUrl;
    newPackage.repositoryUrl = pa.repositoryUrl;
    newPackage.repositoryType = pa.repositoryType;
    newPackage.repositoryCommit = pa.repositoryCommit;
    newPackage.licenseType = pa.licenseType;
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
   * The package license.
   * */
  license: string;

  /*
   * The type of license in field license.
   */
  licenseType: LicenseType;
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

  /*
   * The repository where are source code of the package
   */
  repositoryUrl: string;

  /*
   * The type of repository where are source code.
   * Ex : git
   */
  repositoryType: string;

  /*
   * The commit number of this specific version of package.
   * For git, a sha1.
   */
  repositoryCommit: string;
}
