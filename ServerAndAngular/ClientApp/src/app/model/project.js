"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ProjectDto = /** @class */ (function () {
    function ProjectDto() {
    }
    return ProjectDto;
}());
exports.ProjectDto = ProjectDto;
var ProcessStatus;
(function (ProcessStatus) {
    /**
     * No started
     */
    ProcessStatus[ProcessStatus["Waiting"] = 0] = "Waiting";
    /**
     * The last processing return an error
     * */
    ProcessStatus[ProcessStatus["Error"] = 1] = "Error";
    /**
     * Processing
     * */
    ProcessStatus[ProcessStatus["Running"] = 2] = "Running";
})(ProcessStatus = exports.ProcessStatus || (exports.ProcessStatus = {}));
//# sourceMappingURL=project.js.map