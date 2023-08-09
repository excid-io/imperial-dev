using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace iam.Models.W3CVC
{
    public class Credential
    {
        [Key]
        public int ID { get; set; }
        public long Exp { get; set; }
        public long Iat { get; set; }
        public string Payload { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsRevoked { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RevocationIndex { get; set; }
    }
}
