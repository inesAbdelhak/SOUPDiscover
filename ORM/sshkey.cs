using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace testAngulardotnet.ORM
{
    /// <summary>
    /// A ssh key that permit to access to a server
    /// </summary>
    public class Sshkey : EntityObject
    {
        /// <summary>
        /// The name of the ssh key (defined by user)
        /// </summary>
        [Key]
        public string NameID { get; set; }
        
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }
    }
}
