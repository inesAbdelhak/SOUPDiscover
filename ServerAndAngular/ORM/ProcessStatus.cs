namespace SoupDiscover.ORM
{
    public enum ProcessStatus
    {
        /// <summary>
        /// No started or finished
        /// </summary>
        Waiting,

        /// <summary>
        /// The last processing return an error or was stopped before finished
        /// </summary>
        Error,

        /// <summary>
        /// Processing
        /// </summary>
        Running,
    }
}
