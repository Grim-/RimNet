using UnityEngine;
using Verse;

namespace RimNet
{
    public class CheckboxWidget : UIWidget
    {
        public string Label { get; set; }
        public System.Action<bool> OnValueChanged { get; set; }

        private bool valueRef;

        public CheckboxWidget(ref bool value)
        {
            valueRef = value;
        }

        public override void Draw(Rect Rect)
        {
            base.Draw(Rect);
            bool newValue = valueRef;
            Widgets.CheckboxLabeled(Rect, Label, ref newValue);

            if (newValue != valueRef)
            {
                valueRef = newValue;
                OnValueChanged?.Invoke(newValue);
            }
        }

        public override Vector2 GetPreferredSize()
        {
            return new Vector2(120f, 24f);
        }
    }
}