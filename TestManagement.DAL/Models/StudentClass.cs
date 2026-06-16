using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TestManagement.DAL.Models
{
    public class StudentClass
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
        public User? Student { get; set; }
        public Class? Class { get; set; }
    }
}
