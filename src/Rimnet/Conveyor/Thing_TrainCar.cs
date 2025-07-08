using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class Thing_TrainCar : ThingWithComps, IThingHolder
    {
        private ThingOwner<Pawn> innerContainer;

        protected float CellPerTick => 0.02f;
        protected TrackNetwork AttachedTrack = null;

        public Thing_TrainCar()
        {
            innerContainer = new ThingOwner<Pawn>(this, false);
        }


        public void AttachToTrack(TrackNetwork newTrack)
        {
            AttachedTrack = newTrack;
            IntVec3 startTrackCell = newTrack.Belts.First().Position;
            this.Position = startTrackCell;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            // Gizmo to load a pawn
            yield return new Command_Action
            {
                defaultLabel = "Board Train",
                defaultDesc = "Select a colonist to board the train car.",
                icon = TexButton.Add,
                action = () =>
                {
                    var floatMenuOptions = new List<FloatMenuOption>();
                    foreach (Pawn pawn in this.Map.mapPawns.FreeColonistsSpawned)
                    {
                        // Action to perform when a pawn is selected
                        void action()
                        {
                            if (pawn.Spawned) pawn.DeSpawn();
                            innerContainer.TryAdd(pawn);
                        }
                        floatMenuOptions.Add(new FloatMenuOption(pawn.LabelShort, action));
                    }
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                }
            };

            if (innerContainer.InnerListForReading.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = "Unload All Pawns",
                    defaultDesc = "All pawns will exit the train car.",
                    icon = TexButton.Delete,
                    action = () =>
                    {
                        innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
                    }
                };
            }
        }

        public override string GetInspectString()
        {
            string original = base.GetInspectString();
            if (innerContainer.InnerListForReading.Any())
            {
                string occupants = "Occupants: " + innerContainer.InnerListForReading.Select(pawn => pawn.LabelShort).ToCommaList();
                return string.IsNullOrEmpty(original) ? occupants : original + "\n" + occupants;
            }
            return original;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
    }
}