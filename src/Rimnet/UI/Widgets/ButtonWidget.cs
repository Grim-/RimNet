using UnityEngine;
using Verse;

namespace RimNet
{
    public class ButtonWidget : UIWidget
    {
        public string Text { get; set; }
        public System.Action OnClick { get; set; }

        public override void Draw(Rect Rect)
        {
            base.Draw(Rect);
            if (Widgets.ButtonText(Rect, Text))
            {
                OnClick?.Invoke();
            }
        }

        public override Vector2 GetPreferredSize()
        {
            return new Vector2(100f, 30f);
        }

        public void SetText(string text)
        {
            Text = text;
        }
    }
}