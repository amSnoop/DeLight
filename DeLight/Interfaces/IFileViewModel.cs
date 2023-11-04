using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Interfaces
{
    //TODO: Decide if this is to be deleted
    public interface IFileViewModel
    {
        public string Header { get; }
        public string HeaderStart { get; }
        public string HeaderEnd { get; }
        public string ReasonString { get; }
        public bool DurationVisibility { get; }
        public bool VolumeVisibility { get; }
        public string FileName { get; }
    }
}
