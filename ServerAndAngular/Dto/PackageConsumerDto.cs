namespace SoupDiscover.Dto
{
    public class PackageConsumerDto
    {
        /// <summary>
        /// The name of the project associated
        /// </summary>
        public string projectId { get; set; }

        /// <summary>
        /// The name of the package Consumer (the csproj file)
        /// </summary>
        public string name { get; set; }
    }
}
