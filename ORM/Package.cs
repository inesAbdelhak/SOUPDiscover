using System.ComponentModel.DataAnnotations.Schema;

namespace testAngulardotnet.ORM
{
    public class Package
    {
        /// <summary>
        /// Id of the package
        /// </summary>
        public string ID { get; set; }

        public PackageType PackageType { get; set; }

        public ProcessStatus ProcessStatus { get; set; }
    }
}
