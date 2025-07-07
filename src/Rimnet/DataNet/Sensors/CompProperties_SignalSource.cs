using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalSource : CompProperties_SignalNode
    {
        public CompProperties_SignalSource()
        {
            compClass = typeof(Comp_SignalSource);
        }
    }

    public class Comp_SignalSource : Comp_SignalNode
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
        }

        public virtual void TriggerSignal(float value)
        {
            var signal = new Signal
            {
                Value = value,
                LastChangeTick = Find.TickManager.TicksGame,
                SignalSource = this
            };
            SendSignal(signal);
        }


        public virtual bool TryStartJob(Pawn pawn)
        {
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Send Signal (0)",
                action = () =>
                {
                    TriggerSignal(0);
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Send Signal (1)",
                action = () =>
                {
                    TriggerSignal(1);
                }
            };
        }
    }
}
