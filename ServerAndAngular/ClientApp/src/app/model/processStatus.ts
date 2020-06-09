export enum ProcessStatus {
    /**
     * No started
     */
    Waiting,
    /**
     * The last processing return an error
     * */
    Error,
    /**
     * Processing
     * */
    Running
}
