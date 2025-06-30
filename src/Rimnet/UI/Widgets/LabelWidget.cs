using UnityEngine;
using Verse;

namespace RimNet
{
    public class LabelWidget : UIWidget
    {
        public TextAnchor Anchor { get; set; } = TextAnchor.MiddleLeft;

        private string textRef;

        public LabelWidget(string text)
        {
            textRef = text;
        }

        public override void Draw(Rect Rect)
        {
            base.Draw(Rect);
            var oldAnchor = GUI.skin.label.alignment;
            GUI.skin.label.alignment = Anchor;
            Widgets.Label(Rect, textRef);
            GUI.skin.label.alignment = oldAnchor;
        }

        public override Vector2 GetPreferredSize()
        {
            return new Vector2(100f, 20f);
        }
    }
}