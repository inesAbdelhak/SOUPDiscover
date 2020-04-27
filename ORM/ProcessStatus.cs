namespace testAngulardotnet.ORM
{
    public enum ProcessStatus
    {
        /// <summary>
        /// No started
        /// </summary>
        Waiting,

        /// <summary>
        /// Added to queue to be processed
        /// </summary>
        Pending,

        /// <summary>
        /// Processing
        /// </summary>
        Running,
    }
}
