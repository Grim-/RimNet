using Verse;

namespace RimNet
{
    public class CompProperties_RadialDamageEmitter : CompProperties_RadialEmitter
    {
        public DamageDef damageDef;
        public FloatRange damage = new FloatRange(1, 1);
        public FloatRange armourPen = new FloatRange(1, 1);


        public CompProperties_RadialDamageEmitter()
        {
            compClass = typeof(CompRadialDamageEmitter);
        }
    }

    public class CompRadialDamageEmitter : CompRadialEmitter
    {
        CompProperties_RadialDamageEmitter Props => (CompProperties_RadialDamageEmitter)props;

        protected override void DoEmit()
        {
            base.DoEmit();

            foreach (var item in GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, Props.radius, true))
            {
                if (item is Pawn pawn && !pawn.Dead)
                {
                    pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange, Props.armourPen.RandomInRange));
                }
            }
        }
    }
}