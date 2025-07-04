using System;

namespace RimNet
{
    public class NetworkMessageEventArgs : EventArgs
    {
        public Comp_NetworkNode Sender { get; set; }
        public Comp_NetworkNode Receiver { get; set; }
        public NetworkMessage Message { get; set; }
    }

}