using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileMinds.Shared.Models
{
    public class Todo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Text")]
        public string Text { get; set; }

        [Column("Date")]
        public DateTime Date { get; set; }

        [Column("IsCompleted")]
        public bool IsCompleted { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }
    }
}


