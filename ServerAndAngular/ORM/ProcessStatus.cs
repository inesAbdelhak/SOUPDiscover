namespace SoupDiscover.ORM
{
    public enum ProcessStatus
    {
        /// <summary>
        /// No started
        /// </summary>
        Waiting,

        /// <summary>
        /// The last processing return an error
        /// </summary>
        Error,

        /// <summary>
        /// Processing
        /// </summary>
        Running,
    }
}
