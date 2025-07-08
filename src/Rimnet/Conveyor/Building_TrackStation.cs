using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class Building_TrackStation : Building_Track
    {
        public override void ProcessItemHandoffs()
        {
            if (heldItems.Any(item => item.Item is Thing_TrainCar))
            {
                return;
            }
            base.ProcessItemHandoffs();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            HeldItem heldTrain = heldItems.FirstOrDefault(hi => hi.Item is Thing_TrainCar);
            if (heldTrain == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Build Train Car",
                    defaultDesc = "Builds a new train car at this station.",
                    icon = TexButton.Add,
                    action = () =>
                    {
                        var newCar = (Thing_TrainCar)ThingMaker.MakeThing(ThingDef.Named("TrainCar"));
                        TryAcceptItem(newCar);
                    }
                };
            }
            else
            {
                // If a train IS here, show a button to launch it.
                yield return new Command_Action
                {
                    defaultLabel = "Launch Train",
                    defaultDesc = "Launches the docked train car down the track.",
                    icon = TexButton.Add,
                    action = () =>
                    {
                        // Manually find the next belt and hand off the train, bypassing our override.
                        var targetBelt = SelectNextBeltForItem();
                        if (targetBelt != null && targetBelt.CanAcceptItem())
                        {
                            heldItems.Remove(heldTrain);
                            innerContainer.Remove(heldTrain.Item);
                            targetBelt.TryAcceptItem(heldTrain.Item);
                        }
                    }
                };
            }
        }
    }
}