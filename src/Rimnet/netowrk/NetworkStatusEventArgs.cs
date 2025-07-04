using System;

namespace RimNet
{
    public class NetworkStatusEventArgs : EventArgs
    {
        public Comp_NetworkNode Node { get; set; }
        public bool IsOnline { get; set; }
    }

}