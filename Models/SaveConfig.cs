using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Models
{
    public class SaveConfig
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string ModelFolder { get; set; } = "Models";
        public string ModelPath { get; set; }
    }
}
