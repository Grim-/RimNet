using RimWorld;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Module_Building_Turret : NetworkUIModule
    {
        public override int Priority => 10; // Higher priority than component modules

        private Building_Turret turret;
        private float accuracy;
        private float range;
        private string statusText;
        private string buttonText = "Change Targeting";

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing is Building_Turret;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            turret = node.parent as Building_Turret;
            UpdateData();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 16, 16, 16, 16, 16, 20);

            ExpanseUI.DrawHeader(sections[0], "TURRET_SYS");

            // Current target
            string targetText = GetCurrentTargetText();
            Color targetColor = GetTargetColor();
            ExpanseUI.DrawStatusText(sections[1], "TARG: ", targetText, targetColor);

            // Weapon stats
            string rangeText = $"RANG: {range:F0}m";
            ExpanseUI.DrawStatusText(sections[2], "", rangeText);

            string accuracyText = $"ACCU: {accuracy:F0}%";
            ExpanseUI.DrawStatusText(sections[3], "", accuracyText, ExpanseUI.GetPercentageColor(accuracy / 100f));

            // Last target
            string lastTargetText = GetLastTargetText();
            ExpanseUI.DrawStatusText(sections[4], "LAST: ", lastTargetText, ExpanseUI.Gray);

            // System status
            string status = GetSystemStatus();
            ExpanseUI.DrawStatusText(sections[5], "STS: ", status, ExpanseUI.GetStatusColor(status));

            // Targeting button
            if (ExpanseUI.DrawButton(sections[6].InsetBy(2, 2, 2, 2), buttonText))
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForAttackHostile(), delegate (LocalTargetInfo target)
                {
                    turret.OrderAttack(target);
                });
            }

            ExpanseUI.EndExpanseStyle();
        }

        private string GetCurrentTargetText()
        {
            if (!turret.CurrentTarget.IsValid)
                return "NONE";

            if (turret.CurrentTarget.HasThing)
            {
                Thing target = turret.CurrentTarget.Thing;
                if (target is Pawn pawn)
                {
                    return pawn.Name?.ToStringShort ?? pawn.def.defName.ToUpper();
                }
                return target.def.defName.ToUpper();
            }

            return "POSITION";
        }

        private Color GetTargetColor()
        {
            if (!turret.CurrentTarget.IsValid)
                return ExpanseUI.Gray;

            if (turret.ForcedTarget.IsValid)
                return ExpanseUI.Orange;

            return ExpanseUI.Red;
        }

        private string GetLastTargetText()
        {
            if (!turret.LastAttackedTarget.IsValid)
                return "NONE";

            int ticksSince = Find.TickManager.TicksGame - turret.LastAttackTargetTick;
            float secondsSince = ticksSince / 60f;

            if (secondsSince < 60f)
                return $"{secondsSince:F0}s AGO";
            else if (secondsSince < 3600f)
                return $"{secondsSince / 60f:F0}m AGO";
            else
                return ">1h AGO";
        }

        private string GetSystemStatus()
        {
            if (turret.IsBrokenDown())
                return "FAULT";

            //if (turret.IsStunned)
            //    return "STUNNED";

            CompPowerTrader powerComp = turret.GetComp<CompPowerTrader>();
            if (powerComp != null && !powerComp.PowerOn)
                return "OFFLINE";

            CompMannable mannableComp = turret.GetComp<CompMannable>();
            if (mannableComp != null && !mannableComp.MannedNow)
                return "UNMANNED";

            CompCanBeDormant dormantComp = turret.GetComp<CompCanBeDormant>();
            if (dormantComp != null && !dormantComp.Awake)
                return "DORMANT";

            CompInitiatable initComp = turret.GetComp<CompInitiatable>();
            if (initComp != null && !initComp.Initiated)
                return "INACTIVE";

            CompMechPowerCell mechPowerComp = turret.GetComp<CompMechPowerCell>();
            if (mechPowerComp != null && mechPowerComp.depleted)
                return "DEPLETED";

            if (turret.CurrentTarget.IsValid)
                return "ENGAGED";

            return "ACTIVE";
        }

        public override void UpdateData()
        {
            if (turret?.AttackVerb != null)
            {
                range = turret.AttackVerb.verbProps.range;
                accuracy = turret.AttackVerb.verbProps.accuracyTouch * 100f;
            }
            else
            {
                range = 0f;
                accuracy = 0f;
            }

            statusText = GetSystemStatus();

            if (turret.ForcedTarget.IsValid)
                buttonText = "Clear Target";
            else
                buttonText = "Force Target";
        }
    }
    public class Module_CompTurret : NetworkUIModule
    {
        private Building_Turret turret;
        private float accuracy;
        private float range;
        private string statusText;
        private string buttonText = "Change Targeting";

        public override bool CanHandleComponent(ThingWithComps thing)
        {
            return thing is Building_Turret;
        }

        public override void Initialize(Comp_NetworkNode node)
        {
            turret = node.parent as Building_Turret;
            UpdateData();
        }

        public override void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            ExpanseUI.BeginExpanseStyle();
            ExpanseUI.DrawBackground(rect);

            Rect[] sections = GUIExtensions.SplitRectVertical(rect.Inset(2), 18, 16, 16, 16, 16, 16, 20);

            ExpanseUI.DrawHeader(sections[0], "TURRET_SYS");

            // Current target
            string targetText = GetCurrentTargetText();
            Color targetColor = GetTargetColor();
            ExpanseUI.DrawStatusText(sections[1], "TARG: ", targetText, targetColor);

            // Weapon stats
            string rangeText = $"RANG: {range:F0}m";
            ExpanseUI.DrawStatusText(sections[2], "", rangeText);

            string accuracyText = $"ACCU: {accuracy:F0}%";
            ExpanseUI.DrawStatusText(sections[3], "", accuracyText, ExpanseUI.GetPercentageColor(accuracy / 100f));

            // Last target
            string lastTargetText = GetLastTargetText();
            ExpanseUI.DrawStatusText(sections[4], "LAST: ", lastTargetText, ExpanseUI.Gray);

            // System status
            string status = GetSystemStatus();
            ExpanseUI.DrawStatusText(sections[5], "STS: ", status, ExpanseUI.GetStatusColor(status));

            // Targeting button
            if (Widgets.ButtonText(sections[6].InsetBy(2, 2, 2, 2), buttonText))
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForAttackHostile(), delegate (LocalTargetInfo target)
                {
                    turret.OrderAttack(target);
                });
            }

            ExpanseUI.EndExpanseStyle();
        }

        private string GetCurrentTargetText()
        {
            if (!turret.CurrentTarget.IsValid)
                return "NONE";

            if (turret.CurrentTarget.HasThing)
            {
                Thing target = turret.CurrentTarget.Thing;
                if (target is Pawn pawn)
                {
                    return pawn.Name?.ToStringShort ?? pawn.def.defName.ToUpper();
                }
                return target.def.defName.ToUpper();
            }

            return "POSITION";
        }

        private Color GetTargetColor()
        {
            if (!turret.CurrentTarget.IsValid)
                return ExpanseUI.Gray;

            if (turret.ForcedTarget.IsValid)
                return ExpanseUI.Orange; // Forced target

            return ExpanseUI.Red; // Active engagement
        }

        private string GetLastTargetText()
        {
            if (!turret.LastAttackedTarget.IsValid)
                return "NONE";

            int ticksSince = Find.TickManager.TicksGame - turret.LastAttackTargetTick;
            float secondsSince = ticksSince / 60f;

            if (secondsSince < 60f)
                return $"{secondsSince:F0}s AGO";
            else if (secondsSince < 3600f)
                return $"{secondsSince / 60f:F0}m AGO";
            else
                return ">1h AGO";
        }

        private string GetSystemStatus()
        {
            if (turret.IsBrokenDown())
                return "FAULT";

            //if (turret.)
            //    return "STUNNED";

            CompPowerTrader powerComp = turret.GetComp<CompPowerTrader>();
            if (powerComp != null && !powerComp.PowerOn)
                return "OFFLINE";

            CompMannable mannableComp = turret.GetComp<CompMannable>();
            if (mannableComp != null && !mannableComp.MannedNow)
                return "UNMANNED";

            CompCanBeDormant dormantComp = turret.GetComp<CompCanBeDormant>();
            if (dormantComp != null && !dormantComp.Awake)
                return "DORMANT";

            CompInitiatable initComp = turret.GetComp<CompInitiatable>();
            if (initComp != null && !initComp.Initiated)
                return "INACTIVE";

            CompMechPowerCell mechPowerComp = turret.GetComp<CompMechPowerCell>();
            if (mechPowerComp != null && mechPowerComp.depleted)
                return "DEPLETED";

            if (turret.CurrentTarget.IsValid)
                return "ENGAGED";

            return "ACTIVE";
        }

        public override void UpdateData()
        {
            if (turret?.AttackVerb != null)
            {
                range = turret.AttackVerb.verbProps.range;
                accuracy = turret.AttackVerb.verbProps.accuracyTouch * 100f;
            }
            else
            {
                range = 0f;
                accuracy = 0f;
            }

            statusText = GetSystemStatus();

            // Update button text based on current state
            if (turret.ForcedTarget.IsValid)
                buttonText = "Clear Target";
            else
                buttonText = "Force Target";
        }
    }
}