using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace testAngulardotnet.ORM
{
    public class Project
    {
        public int ID { get; set; }

        public Repository Repository { get; set; }

        public ICollection<Package> Packages { get; set; }
    }
}
