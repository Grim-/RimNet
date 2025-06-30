using UnityEngine;
using Verse;

namespace RimNet
{
    public class SliderWidget : UIWidget
    {
        public float Min { get; set; }
        public float Max { get; set; } = 100f;
        public string Label { get; set; }
        public System.Action<float> OnValueChanged { get; set; }

        private float valueRef;

        public SliderWidget(ref float value)
        {
            valueRef = value;
        }

        public override void Draw(Rect Rect)
        {
            base.Draw(Rect);
            if (!string.IsNullOrEmpty(Label))
            {
                var labelRect = new Rect(Rect.x, Rect.y - 20f, Rect.width, 20f);
                Widgets.Label(labelRect, $"{Label}: {valueRef:F1}");
            }

            float newValue = Widgets.HorizontalSlider(Rect, valueRef, Min, Max);
            if (Mathf.Abs(newValue - valueRef) > 0.01f)
            {
                valueRef = newValue;
                OnValueChanged?.Invoke(newValue);
            }
        }

        public override Vector2 GetPreferredSize()
        {
            return new Vector2(120f, 20f);
        }

        public void SetRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}