using Verse;

namespace RimNet
{
    public class CompProperties_Emitter : CompProperties_Contraption
    {
        public EffecterDef emissionEffecter = null;

        public CompProperties_Emitter()
        {
            compClass = typeof(CompEmitter);
        }
    }


    public class CompEmitter : CompContraption
    {
        private CompProperties_Emitter Props => (CompProperties_Emitter)props;

        public virtual void Emit()
        {
            PlayEmissionEffects();
            DoEmit();
        }

        protected virtual void PlayEmissionEffects()
        {
            if (Props.emissionEffecter != null)
            {
                Props.emissionEffecter.Spawn(this.parent.Position, this.parent.Map, 1f);
            }
        }

        protected virtual void DoEmit()
        {

        }
    }
}