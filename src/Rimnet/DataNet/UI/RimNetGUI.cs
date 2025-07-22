using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace RimNet
{
    [StaticConstructorOnStartup]
    public static class RimNetGUI
    {
        public static readonly Color TextColor = new Color(0.85f, 0.85f, 0.9f);
        public static readonly Color TextHeaderColor = Color.white;
        public static readonly Color TextDisabledColor = new Color(0.6f, 0.6f, 0.6f);
        public static readonly Color AccentColor = new Color(0.25f, 0.6f, 1f);
        public static readonly Color AccentMouseoverColor = new Color(0.4f, 0.75f, 1f);
        public static readonly Color AccentActiveColor = new Color(0.5f, 0.85f, 1f);
        public static readonly Color SignalOn = new Color(0.2f, 1f, 0.3f);
        public static readonly Color SignalOff = new Color(1f, 0.3f, 0.3f);
        public static readonly Color WarningColor = new Color(1f, 0.9f, 0.3f);
        public static readonly Color WindowBackground = new Color(0.05f, 0.06f, 0.08f);
        public static readonly Color PanelColor = new Color(0.1f, 0.12f, 0.15f, 0.8f);
        public static readonly Color BorderColor = new Color(0.2f, 0.22f, 0.25f);
        public static readonly Color BorderActiveColor = AccentColor;

        public static void DrawAngularBorder(Rect rect, Color color, float cutSize = 4f, float width = 1f)
        {
            Vector2 tl = new Vector2(rect.x, rect.y + cutSize);
            Vector2 tr = new Vector2(rect.x + cutSize, rect.y);
            Vector2 br = new Vector2(rect.xMax, rect.yMax - cutSize);
            Vector2 bl = new Vector2(rect.xMax - cutSize, rect.yMax);

            Widgets.DrawLine(tl, tr, color, width);
            Widgets.DrawLine(tr, new Vector2(rect.xMax, rect.y), color, width);
            Widgets.DrawLine(new Vector2(rect.xMax, rect.y), br, color, width);
            Widgets.DrawLine(br, bl, color, width);
            Widgets.DrawLine(bl, new Vector2(rect.x, rect.yMax), color, width);
            Widgets.DrawLine(new Vector2(rect.x, rect.yMax), tl, color, width);
        }

        public static void DrawPanel(Rect rect, Color? backgroundColor = null, Color? borderColor = null)
        {
            Widgets.DrawBoxSolid(rect, backgroundColor ?? PanelColor);
            DrawAngularBorder(rect, borderColor ?? BorderColor);
        }

        public static void DrawHeader(Rect rect, string label, float cutSize = 4f)
        {
            DrawPanel(rect, BorderColor, AccentColor);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = TextHeaderColor;
            Widgets.Label(rect, label);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static bool StyledButton(Rect rect, string label, bool enabled = true)
        {
            bool mouseOver = Mouse.IsOver(rect) && enabled;
            Color panelColor = enabled ? PanelColor : new Color(0.1f, 0.1f, 0.1f);
            Color borderColor = BorderColor;
            Color textColor = enabled ? TextColor : TextDisabledColor;

            if (mouseOver)
            {
                panelColor = new Color(PanelColor.r * 1.5f, PanelColor.g * 1.5f, PanelColor.b * 1.5f);
                borderColor = BorderActiveColor;
                textColor = Color.white;
            }

            DrawPanel(rect, panelColor, borderColor);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = textColor;
            Widgets.Label(rect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            if (!enabled) return false;
            if (mouseOver) TooltipHandler.TipRegion(rect, "Click to interact");
            return Widgets.ButtonInvisible(rect);
        }

        public static void DrawLabeledValue(Rect rect, string label, string value, Color? valueColor = null)
        {
            Rect labelRect = rect.LeftHalf();
            Rect valueRect = rect.RightHalf();

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = TextColor;
            Widgets.Label(labelRect, label);

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = valueColor ?? AccentColor;
            Widgets.Label(valueRect, value);

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        public static float DrawStyledSlider(Rect rect, float val, float min, float max, string label = "", Color? fillColor = null)
        {
            bool mouseOver = Mouse.IsOver(rect);
            if (mouseOver && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
            {
                val = (Event.current.mousePosition.x - rect.x) / rect.width * (max - min) + min;
                val = Mathf.Clamp(val, min, max);
                Event.current.Use();
            }

            DrawPanel(rect, PanelColor, mouseOver ? BorderActiveColor : BorderColor);

            float fillPct = Mathf.Clamp01((val - min) / (max - min));
            Rect fillRect = rect.ContractedBy(2f);
            fillRect.width *= fillPct;
            Widgets.DrawBoxSolid(fillRect, fillColor ?? AccentColor);

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = TextColor;
            Widgets.Label(rect, string.IsNullOrEmpty(label) ? val.ToString("F2") : label);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            return val;
        }

        public static void EnumSelectorButton<T>(Rect rect, string labelPrefix, T currentValue, Action<T> setter) where T : struct, Enum
        {
            string buttonLabel = $"{labelPrefix}{currentValue.ToString().Replace("_", " ")}";
            if (StyledButton(rect, buttonLabel))
            {
                var options = new List<FloatMenuOption>();
                foreach (T value in Enum.GetValues(typeof(T)))
                {
                    var capturedValue = value;
                    options.Add(new FloatMenuOption(
                        capturedValue.ToString().Replace("_", " "),
                        () => setter(capturedValue)
                    ));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        public static void DropdownSelectorButton(Rect rect, string label, Func<List<FloatMenuOption>> itemsGetter)
        {
            string buttonLabel = $"{label}";
            if (StyledButton(rect, buttonLabel))
            {
                List<FloatMenuOption> options = itemsGetter.Invoke();
                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(itemsGetter.Invoke()));
                }    
            }
        }
    }
}