import { PackageDto } from "./package";


export class PackageWithProjectDto {
    /**
     * The package
     * */
    packageDto: PackageDto;

    /*
     * All project name where is found the package
     */
    projectNames: string[];
}
