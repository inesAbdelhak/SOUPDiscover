using System.ComponentModel.DataAnnotations;

namespace testAngulardotnet.ORM
{
    public class AuthentificationToken
    {
        [Key]
        public string NameID { get; set; }

        public string Token { get; set; }
    }
}
