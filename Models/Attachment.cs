using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfChat.Models
{
    public partial class Attachment: ObservableObject
    {
        [ObservableProperty]
        private string _fileName;
        [ObservableProperty]
        private string _filePath;
    }
}
