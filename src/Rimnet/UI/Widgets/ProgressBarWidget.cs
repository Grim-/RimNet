using UnityEngine;
using Verse;

namespace RimNet
{
    public class ProgressBarWidget : UIWidget
    {
        public float MaxValue { get; set; } = 1f;
        public Color FillColor { get; set; } = Color.green;
        public string Label { get; set; }

        private float valueRef;

        public ProgressBarWidget(float value, float maxValue = 1f)
        {
            valueRef = value;
            MaxValue = maxValue;
        }

        public override void Draw(Rect Rect)
        {
            base.Draw(Rect);
            Widgets.FillableBar(Rect, valueRef / MaxValue, BaseContent.WhiteTex, null, false);
        }

        public override Vector2 GetPreferredSize()
        {
            return new Vector2(120f, 20f);
        }

        public void SetMaxValue(float maxValue)
        {
            MaxValue = maxValue;
        }

        public void SetColor(Color color)
        {
            FillColor = color;
        }
    }
}