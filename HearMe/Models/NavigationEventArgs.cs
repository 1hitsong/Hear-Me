using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearMe.Models
{
    public class NavigationEventArgs : EventArgs
    {
        public sbyte Direction { get; set; }
    }
}
