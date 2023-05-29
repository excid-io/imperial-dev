using System.ComponentModel.DataAnnotations;

namespace iam.Models.RelBAC
{
    public class Relationship
    {
        [Key]
        public int ID { get; set; }
        public string Relation { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;

        public int AuthorizationID { get; set; }
        public virtual Authorization Authorization { get; set; } = new Authorization();

    }
}
