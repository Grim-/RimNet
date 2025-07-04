using UnityEngine;
using Verse;

namespace RimNet
{

    [StaticConstructorOnStartup]
    public static class ExpanseUI
    {
        public static readonly Color Blue = new Color(0.1f, 0.3f, 0.6f, 1f);
        public static readonly Color Orange = new Color(1f, 0.5f, 0.1f, 1f);
        public static readonly Color Green = new Color(0f, 0.839f, 0.039f, 1f);
        public static readonly Color Gray = new Color(0.72f, 0.75f, 0.73f, 1f);
        public static readonly Color DarkGray = new Color(0.1f, 0.1f, 0.15f, 1f);
        public static readonly Color TextColor = new Color(0.8f, 0.9f, 1f, 1f);
        public static readonly Color Red = new Color(0.8f, 0.2f, 0.2f, 1f);


        //old
        public static readonly Color ExpanseBlue = new Color(0.1f, 0.3f, 0.6f, 1f);
        public static readonly Color ExpanseOrange = new Color(1f, 0.5f, 0.1f, 1f);
        public static readonly Color ExpanseGreen = new Color(0, 0.839f, 0.039f, 1f);
        public static readonly Color ExpanseGray = new Color(0.2f, 0.25f, 0.3f, 1f);
        public static readonly Color ExpanseText = new Color(0.8f, 0.9f, 1f, 1f);

        public static readonly Texture2D UINodeTex = ContentFinder<Texture2D>.Get("UI_Para");
        public static readonly Texture2D UIBGIcon = ContentFinder<Texture2D>.Get("UI_Starfall");
        


        //private string GetSystemStatus()
        //{
        //    if (breakdownableComp != null && breakdownableComp.BrokenDown)
        //        return "FAULT";

        //    if (refuelableComp != null && !refuelableComp.HasFuel)
        //        return "NO_FUEL";

        //    if (flickableComp != null && !flickableComp.SwitchIsOn)
        //        return "OFFLINE";

        //    if (autoPoweredComp != null && !autoPoweredComp.WantsToBeOn)
        //        return "AUTO_OFF";

        //    if (toxifier != null && !toxifier.CanPolluteNow)
        //        return "POLLUTION";

        //    if (!powerPlant.PowerOn)
        //        return "INACTIVE";

        //    if (powerPlant.PowerOutput > 0f)
        //        return "GENERATING";

        //    return "STANDBY";
        //}
        public static Color GetStatusColor(string status)
        {
            switch (status.ToUpper())
            {
                case "FAULT":
                case "CRITICAL":
                case "ERROR":
                case "OFFLINE":
                    return Red;
                case "WARNING":
                case "LOW":
                case "DEGRADED":
                    return Orange;
                case "NOMINAL":
                case "ONLINE":
                case "ACTIVE":
                case "READY":
                    return Green;
                default:
                    return TextColor;
            }
        }
        public static Mod_RimNet Mod => LoadedModManager.GetMod<Mod_RimNet>();


        private static GUIStyle Style = null;

        // might need to cache it?
        public static void DrawFontLabel(Rect rect, string text, GameFont gameFont, TextAnchor textAnchor = TextAnchor.UpperLeft)
        {
            if (Style == null)
            {
                Style = new GUIStyle(GUI.skin.label);
            }

            if (Mod != null)
            {
                Mod.LoadFonts();
                Font font = Mod.Fonts[gameFont];
                Style.font = font;
                Style.alignment = textAnchor;
            }
           
            GUI.Label(rect, text, Style);
        }

        public static void DrawCutAngularPanel(Rect rect, Color color, float cutSize = 3f)
        {
            Vector2[] points = new Vector2[]
            {
                new Vector2(rect.x + cutSize, rect.y),
                new Vector2(rect.xMax, rect.y),
                new Vector2(rect.xMax, rect.yMax - cutSize),
                new Vector2(rect.xMax - cutSize, rect.yMax),
                new Vector2(rect.x, rect.yMax),
                new Vector2(rect.x, rect.y + cutSize)
            };

            Color originalColor = GUI.color;
            GUI.color = color;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Length];
                Widgets.DrawLine(start, end, color, 1f);
            }

            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
            Color fillColor = color;
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, BaseContent.WhiteTex);

            GUI.color = originalColor;
        }



        public static void DrawAngularPanel(Rect rect, Color fillColor, Color outlineColor, float slantSize = 3f)
        {
            Vector2[] points = new Vector2[]
            {
                new Vector2(rect.x + slantSize, rect.y),
                new Vector2(rect.xMax, rect.y),
                new Vector2(rect.xMax - slantSize, rect.yMax),
                new Vector2(rect.x, rect.yMax)
            };

            Color originalColor = GUI.color;
            //GUI.color = color;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[(i + 1) % points.Length];
                Widgets.DrawLine(start, end, outlineColor, 1f);
            }

            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, BaseContent.WhiteTex);

            GUI.color = originalColor;
        }

        public static void DrawAngularBorder(Rect rect, Color color, float cutSize = 3f, float lineWidth = 1f)
        {
            Color originalColor = GUI.color;
            GUI.color = color;

            Widgets.DrawLine(new Vector2(rect.x + cutSize, rect.y), new Vector2(rect.xMax, rect.y), color, lineWidth);
            Widgets.DrawLine(new Vector2(rect.xMax, rect.y), new Vector2(rect.xMax, rect.yMax - cutSize), color, lineWidth);
            Widgets.DrawLine(new Vector2(rect.xMax - cutSize, rect.yMax), new Vector2(rect.x, rect.yMax), color, lineWidth);
            Widgets.DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.x, rect.y + cutSize), color, lineWidth);

            GUI.color = originalColor;
        }

        public static void DrawProgressBar(Rect rect, float fillPct, Color fillColor, Color bgColor = default, bool showPercentage = true)
        {
            Color originalColor = GUI.color;

            if (bgColor == default) bgColor = DarkGray;

            GUI.color = bgColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, Gray, 2f);

            if (fillPct > 0f)
            {
                Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * fillPct, rect.height - 4);
                GUI.color = fillColor;
                GUI.DrawTexture(fillRect, BaseContent.WhiteTex);

                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.3f);
                Rect glowRect = new Rect(fillRect.x - 1, fillRect.y - 1, fillRect.width + 2, fillRect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            if (showPercentage)
            {
                GUI.color = TextColor;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                string percentText = $"{fillPct * 100f:F0}%";
                ExpanseUI.DrawFontLabel(rect, percentText, GameFont.Small);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            GUI.color = originalColor;
        }

        public static void DrawHeader(Rect rect, string label, Color bgColor = default)
        {
            Color originalColor = GUI.color;

            if (bgColor == default) bgColor = Gray;

            DrawAngularPanel(rect, new Color(0.72f, 0.75f, 0.73f, 0.3f), new Color(bgColor.r, bgColor.g, bgColor.b, 0.3f));

            GUI.color = TextColor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            ExpanseUI.DrawFontLabel(new Rect(rect.x + 4, rect.y, rect.width - 8, rect.height), label, GameFont.Small);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = originalColor;
        }

        public static void DrawStatusText(Rect rect, string label, string value, Color valueColor = default, GameFont font = GameFont.Tiny, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            Color originalColor = GUI.color;

            if (valueColor == default) valueColor = TextColor;

            GUI.color = TextColor;
            Text.Font = font;
            Text.Anchor = anchor;

            Vector2 labelSize = Text.CalcSize(label);
            ExpanseUI.DrawFontLabel(new Rect(rect.x, rect.y, labelSize.x, rect.height), label, GameFont.Small);

            GUI.color = valueColor;

            ExpanseUI.DrawFontLabel(new Rect(rect.x + labelSize.x, rect.y, rect.width - labelSize.x, rect.height), value, GameFont.Small);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Tiny;
            GUI.color = originalColor;
        }

        public static void DrawBackground(Rect rect, Color color = default)
        {
            if (color == default) color = DarkGray;
            Widgets.DrawBoxSolid(rect, color);
        }

 

        public static Color GetPercentageColor(float percentage)
        {
            if (percentage > 0.6f)
                return Green;
            else if (percentage > 0.3f)
                return Orange;
            else
                return Red;
        }

        public static void BeginExpanseStyle()
        {
            Text.Font = GameFont.Tiny;
            GUI.color = TextColor;
        }

        public static void EndExpanseStyle()
        {
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static float DrawInteractiveProgressBar(Rect rect, float currentValue, float targetValue, float minValue, float maxValue, string label = "", string unit = "", bool showValues = true, Color fillColor = default)
        {
            Color originalColor = GUI.color;
            float newTargetValue = targetValue;

            if (fillColor == default) fillColor = GetPercentageColor((currentValue - minValue) / (maxValue - minValue));

            // Background
            GUI.color = DarkGray;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, Gray, 2f);

            // Current value fill
            float currentPct = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            if (currentPct > 0f)
            {
                Rect currentFillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * currentPct, rect.height - 4);
                GUI.color = fillColor;
                GUI.DrawTexture(currentFillRect, BaseContent.WhiteTex);

                // Glow effect
                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.3f);
                Rect glowRect = new Rect(currentFillRect.x - 1, currentFillRect.y - 1, currentFillRect.width + 2, currentFillRect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            // Target value indicator
            float targetPct = Mathf.Clamp01((targetValue - minValue) / (maxValue - minValue));
            float targetX = rect.x + 2 + (rect.width - 4) * targetPct;
            GUI.color = Orange;
            Widgets.DrawLine(new Vector2(targetX, rect.y + 2), new Vector2(targetX, rect.yMax - 2), Orange, 2f);

            // Handle click to set target
            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                float clickX = mousePos.x;
                float clickPct = Mathf.Clamp01((clickX - rect.x - 2) / (rect.width - 4));
                newTargetValue = Mathf.Lerp(minValue, maxValue, clickPct);
                Event.current.Use();
            }

            // Text overlay
            if (showValues || !string.IsNullOrEmpty(label))
            {
                GUI.color = TextColor;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;

                string displayText = "";
                if (!string.IsNullOrEmpty(label))
                {
                    displayText = label + ": ";
                }

                if (showValues)
                {
                    displayText += $"{currentValue:F1}";
                    if (Mathf.Abs(targetValue - currentValue) > 0.1f)
                    {
                        displayText += $" → {targetValue:F1}";
                    }
                    if (!string.IsNullOrEmpty(unit))
                    {
                        displayText += unit;
                    }
                }

                Widgets.Label(rect, displayText);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            // Tooltip
            if (Mouse.IsOver(rect))
            {
                string tooltipText = $"Current: {currentValue:F1}{unit}\nTarget: {targetValue:F1}{unit}\nRange: {minValue:F1} - {maxValue:F1}{unit}\n\nClick to set target";
                TooltipHandler.TipRegion(rect, tooltipText);
            }

            GUI.color = originalColor;
            return newTargetValue;
        }

        public static float DrawProgressBarWithMarkers(Rect rect, float currentValue, float targetValue, float minValue, float maxValue, string label = "", string unit = "", bool showValues = true, Color fillColor = default, int markerCount = 10)
        {
            Color originalColor = GUI.color;
            float newTargetValue = targetValue;

            if (fillColor == default) fillColor = GetPercentageColor((currentValue - minValue) / (maxValue - minValue));

            GUI.color = DarkGray;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, Gray, 2f);

            float currentPct = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            if (currentPct > 0f)
            {
                Rect currentFillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * currentPct, rect.height - 4);
                GUI.color = fillColor;
                GUI.DrawTexture(currentFillRect, BaseContent.WhiteTex);

                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.3f);
                Rect glowRect = new Rect(currentFillRect.x - 1, currentFillRect.y - 1, currentFillRect.width + 2, currentFillRect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            GUI.color = new Color(Gray.r, Gray.g, Gray.b, 0.8f);
            for (int i = 1; i < markerCount; i++)
            {
                float markerX = rect.x + 2 + ((rect.width - 4) * i / markerCount);
                Widgets.DrawLine(new Vector2(markerX, rect.y + 2), new Vector2(markerX, rect.yMax - 2), Gray, 1f);
            }

            float targetPct = Mathf.Clamp01((targetValue - minValue) / (maxValue - minValue));
            float targetX = rect.x + 2 + (rect.width - 4) * targetPct;
            GUI.color = Orange;
            Widgets.DrawLine(new Vector2(targetX, rect.y + 2), new Vector2(targetX, rect.yMax - 2), Orange, 2f);

            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                float clickX = mousePos.x;
                float clickPct = Mathf.Clamp01((clickX - rect.x - 2) / (rect.width - 4));
                newTargetValue = Mathf.Lerp(minValue, maxValue, clickPct);
                Event.current.Use();
            }

            if (showValues || !string.IsNullOrEmpty(label))
            {
                GUI.color = TextColor;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;

                string displayText = "";
                if (!string.IsNullOrEmpty(label))
                {
                    displayText = label + ": ";
                }

                if (showValues)
                {
                    displayText += $"{currentValue:F1}";
                    if (Mathf.Abs(targetValue - currentValue) > 0.1f)
                    {
                        displayText += $" → {targetValue:F1}";
                    }
                    if (!string.IsNullOrEmpty(unit))
                    {
                        displayText += unit;
                    }
                }

                ExpanseUI.DrawFontLabel(rect, displayText, GameFont.Small);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            if (Mouse.IsOver(rect))
            {
                string tooltipText = $"Current: {currentValue:F1}{unit}\nTarget: {targetValue:F1}{unit}\nRange: {minValue:F1} - {maxValue:F1}{unit}\n\nClick to set target";
                TooltipHandler.TipRegion(rect, tooltipText);
            }

            GUI.color = originalColor;
            return newTargetValue;
        }

        public static float DrawSectionedProgressBar(Rect rect, float currentValue, float targetValue, float minValue, float maxValue, string label = "", string unit = "", bool showValues = true, Color fillColor = default, int sectionCount = 10)
        {
            Color originalColor = GUI.color;
            float newTargetValue = targetValue;

            if (fillColor == default) fillColor = GetPercentageColor((currentValue - minValue) / (maxValue - minValue));

            GUI.color = DarkGray;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, Gray, 2f);

            float currentPct = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
            float sectionWidth = (rect.width - 4) / sectionCount;
            int filledSections = Mathf.FloorToInt(currentPct * sectionCount);
            float partialSection = (currentPct * sectionCount) - filledSections;

            for (int i = 0; i < sectionCount; i++)
            {
                float sectionX = rect.x + 2 + (i * sectionWidth);
                Rect sectionRect = new Rect(sectionX + 1, rect.y + 3, sectionWidth - 2, rect.height - 6);

                if (i < filledSections)
                {
                    GUI.color = fillColor;
                    GUI.DrawTexture(sectionRect, BaseContent.WhiteTex);
                }
                else if (i == filledSections && partialSection > 0f)
                {
                    Rect partialRect = new Rect(sectionRect.x, sectionRect.y, sectionRect.width * partialSection, sectionRect.height);
                    GUI.color = fillColor;
                    GUI.DrawTexture(partialRect, BaseContent.WhiteTex);
                }

                GUI.color = Gray;
                DrawAngularBorder(sectionRect, Gray, 1f, 0.5f);
            }

            float targetPct = Mathf.Clamp01((targetValue - minValue) / (maxValue - minValue));
            float targetX = rect.x + 2 + (rect.width - 4) * targetPct;
            GUI.color = Orange;
            Widgets.DrawLine(new Vector2(targetX, rect.y + 2), new Vector2(targetX, rect.yMax - 2), Orange, 2f);

            if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                float clickX = mousePos.x;
                float clickPct = Mathf.Clamp01((clickX - rect.x - 2) / (rect.width - 4));

                int targetSection = Mathf.RoundToInt(clickPct * sectionCount);
                float snappedPct = (float)targetSection / sectionCount;
                newTargetValue = Mathf.Lerp(minValue, maxValue, snappedPct);
                Event.current.Use();
            }

            if (showValues || !string.IsNullOrEmpty(label))
            {
                GUI.color = TextColor;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;

                string displayText = "";
                if (!string.IsNullOrEmpty(label))
                {
                    displayText = label + ": ";
                }

                if (showValues)
                {
                    displayText += $"{currentValue:F1}";
                    if (Mathf.Abs(targetValue - currentValue) > 0.1f)
                    {
                        displayText += $" → {targetValue:F1}";
                    }
                    if (!string.IsNullOrEmpty(unit))
                    {
                        displayText += unit;
                    }
                }

                DrawFontLabel(rect, displayText, GameFont.Small, TextAnchor.MiddleCenter);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            if (Mouse.IsOver(rect))
            {
                string tooltipText = $"Current: {currentValue:F1}{unit}\nTarget: {targetValue:F1}{unit}\nRange: {minValue:F1} - {maxValue:F1}{unit}\n\nClick to set target";
                TooltipHandler.TipRegion(rect, tooltipText);
            }

            GUI.color = originalColor;
            return newTargetValue;
        }
        public static float DrawSlider(Rect rect, float value, float minValue, float maxValue, string label = "", string unit = "", bool showValue = true, Color fillColor = default)
        {
            Color originalColor = GUI.color;
            float newValue = value;

            if (fillColor == default) fillColor = GetPercentageColor((value - minValue) / (maxValue - minValue));

            // Background
            GUI.color = DarkGray;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, Gray, 2f);

            // Value fill
            float valuePct = Mathf.Clamp01((value - minValue) / (maxValue - minValue));
            if (valuePct > 0f)
            {
                Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * valuePct, rect.height - 4);
                GUI.color = fillColor;
                GUI.DrawTexture(fillRect, BaseContent.WhiteTex);
            }

            // Handle interaction
            if (Mouse.IsOver(rect) && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                float clickX = mousePos.x;
                float clickPct = Mathf.Clamp01((clickX - rect.x - 2) / (rect.width - 4));
                newValue = Mathf.Lerp(minValue, maxValue, clickPct);
                Event.current.Use();
            }

            // Text
            if (showValue || !string.IsNullOrEmpty(label))
            {
                GUI.color = TextColor;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;

                string displayText = "";
                if (!string.IsNullOrEmpty(label))
                {
                    displayText = label + ": ";
                }

                if (showValue)
                {
                    displayText += $"{value:F1}{unit}";
                }

                Widgets.Label(rect, displayText);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            GUI.color = originalColor;
            return newValue;
        }

        public static bool DrawButton(Rect rect, string label, Color buttonColor = default, bool enabled = true, float cutSize = 3f, GameFont font = GameFont.Tiny, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            Color originalColor = GUI.color;
            bool clicked = false;

            if (buttonColor == default) buttonColor = Gray;

            bool mouseOver = Mouse.IsOver(rect);

            Color currentColor = buttonColor;
            Color fillColor = buttonColor;
            Color borderColor = buttonColor;

            if (!enabled)
            {
                currentColor = new Color(buttonColor.r * 0.5f, buttonColor.g * 0.5f, buttonColor.b * 0.5f, buttonColor.a);
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.1f);
                borderColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.6f);
            }
            else if (mouseOver)
            {
                currentColor = new Color(buttonColor.r * 1.1f, buttonColor.g * 1.1f, buttonColor.b * 1.1f, buttonColor.a);
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.4f);
                borderColor = new Color(currentColor.r * 1.2f, currentColor.g * 1.2f, currentColor.b * 1.2f, buttonColor.a);
            }
            else
            {
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.2f);
            }

            GUI.color = fillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, borderColor, cutSize, mouseOver ? 2f : 1f);

            if (mouseOver && enabled)
            {
                GUI.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.1f);
                Rect glowRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            Color textColor = enabled ? TextColor : new Color(TextColor.r * 0.6f, TextColor.g * 0.6f, TextColor.b * 0.6f, TextColor.a);
            GUI.color = textColor;
            Text.Font = font;
            Text.Anchor = alignment;

            ExpanseUI.DrawFontLabel(rect, label, GameFont.Small, alignment);
            Text.Anchor = TextAnchor.UpperLeft;

            if (mouseOver && enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                clicked = true;
                Event.current.Use();
            }

            GUI.color = originalColor;
            return clicked;
        }
        public static bool DrawIconButton(Rect rect, Texture2D icon, Color buttonColor = default, bool enabled = true, float cutSize = 3f, string tooltip = "")
        {
            Color originalColor = GUI.color;
            bool clicked = false;

            if (buttonColor == default) buttonColor = Gray;

            bool mouseOver = Mouse.IsOver(rect);

            Color currentColor = buttonColor;
            Color fillColor = buttonColor;
            Color borderColor = buttonColor;

            if (!enabled)
            {
                currentColor = new Color(buttonColor.r * 0.5f, buttonColor.g * 0.5f, buttonColor.b * 0.5f, buttonColor.a);
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.1f);
                borderColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.6f);
            }
            else if (mouseOver)
            {
                currentColor = new Color(buttonColor.r * 1.1f, buttonColor.g * 1.1f, buttonColor.b * 1.1f, buttonColor.a);
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.4f);
                borderColor = new Color(currentColor.r * 1.2f, currentColor.g * 1.2f, currentColor.b * 1.2f, buttonColor.a);
            }
            else
            {
                fillColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.2f);
            }

            GUI.color = fillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            DrawAngularBorder(rect, borderColor, cutSize, mouseOver ? 2f : 1f);

            if (mouseOver && enabled)
            {
                GUI.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.1f);
                Rect glowRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            if (icon != null)
            {
                Color iconColor = enabled ? TextColor : new Color(TextColor.r * 0.6f, TextColor.g * 0.6f, TextColor.b * 0.6f, TextColor.a);
                GUI.color = iconColor;

                float iconSize = Mathf.Min(rect.width, rect.height) * 0.6f;
                Rect iconRect = new Rect(rect.center.x - iconSize / 2, rect.center.y - iconSize / 2, iconSize, iconSize);

                GUI.DrawTexture(iconRect, icon);
            }

            if (mouseOver && enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                clicked = true;
                Event.current.Use();
            }

            if (!string.IsNullOrEmpty(tooltip) && mouseOver)
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            GUI.color = originalColor;
            return clicked;
        }
    }
}