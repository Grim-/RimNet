using Verse;

namespace RimNet
{
    public class TrainTrackNetwork : TrackNetwork
    {
        public override bool CanAddToTrack(Thing thing)
        {
            return thing is Thing_TrainCar;
        }
    }
}