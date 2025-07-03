using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Models
{
    public partial class Prompt:ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _content;
        [ObservableProperty]
        private string _end;
        public Prompt(string name, string content, string end)
        {
            Name = name;
            Content = content;
            End = end;
        }
    }
}
