using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Window_NetworkControlPanel : Window
    {
        private const float DEFAULT_WINDOW_WIDTH = 380f;
        private const float DEFAULT_WINDOW_HEIGHT = 400f;
        private const int ANIMATION_DURATION_TICKS = 20;
        private const int SIZE_CHECK_INTERVAL_TICKS = 5;
        private const int UPDATE_INTERVAL_TICKS = 30;

        private const float WINDOW_PADDING = 16f;
        private const float MODULE_PADDING = 15f;
        private const float BORDER_SPACE = 10f;
        private const float CONTENT_INSET = 8f;
        private const float MODULE_FRAME_INSET = 4f;
        private const float HEADER_SPACING = 5f;
        private const float SCROLL_BAR_WIDTH = 20f;
        private const float TOTAL_CONTENT_BOTTOM_PADDING = 8f;

        private const float BASE_MODULE_HEIGHT = 150f;
        private const float HEADER_HEIGHT = 35f;
        private const float MIN_WINDOW_HEIGHT = 200f;
        private const float MAX_WINDOW_HEIGHT = 800f;
        private const float MIN_WINDOW_WIDTH = 320f;
        private const float MAX_WINDOW_WIDTH = 600f;
        private const float ADDITIONAL_HEIGHT_BUFFER = 20f;

        private const float MAIN_BORDER_WIDTH = 4f;
        private const float MAIN_BORDER_CORNER_RADIUS = 2f;
        private const float MODULE_BORDER_WIDTH = 3f;
        private const float MODULE_BORDER_CORNER_RADIUS = 1f;
        private const float MODULE_BORDER_INSET = 2f;

        private const float HEADER_SECTION_ONE_HEIGHT = 20f;
        private const float HEADER_SECTION_TWO_HEIGHT = 15f;

        private const float FRAME_BACKGROUND_RED = 0.05f;
        private const float FRAME_BACKGROUND_GREEN = 0.08f;
        private const float FRAME_BACKGROUND_BLUE = 0.12f;
        private const float FRAME_BACKGROUND_ALPHA = 0.9f;
        private const float MODULE_HIGHLIGHT_ALPHA = 0.1f;

        private const float ANIMATION_COMPLETE = 1f;
        private const float ANIMATION_START = 0f;
        private const int ANIMATION_NOT_STARTED = -1;
        private const int GRID_DEFAULT_COLUMNS = 1;

        private Vector2 windowSize = new Vector2(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT);
        private Vector2 targetWindowSize = new Vector2(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT);
        private int windowAnimationStartTick = ANIMATION_NOT_STARTED;
        private int windowAnimationDurationTicks = ANIMATION_DURATION_TICKS;

        public override Vector2 InitialSize => windowSize;

        private Comp_NetworkNode selectedNode;
        private List<NetworkUIModule> currentModules = new List<NetworkUIModule>();
        private Vector2 scrollPosition;
        private int gridMaxColumns = GRID_DEFAULT_COLUMNS;
        private float baseModuleHeight = BASE_MODULE_HEIGHT;
        private float headerHeight = HEADER_HEIGHT;
        private int lastSizeCheckTick = ANIMATION_NOT_STARTED;

        public Window_NetworkControlPanel(Comp_NetworkNode node)
        {
            selectedNode = node;
            draggable = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            drawShadow = false;
            doWindowBackground = false;
            absorbInputAroundWindow = true;
            RefreshModules();
            CalculateWindowSize();
            windowSize = targetWindowSize;
        }

        public override void DoWindowContents(Rect inRect)
        {
            UpdateWindowSizeAnimation();
            CheckForSizeChanges();


            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Mouse1 && Event.current.type == EventType.MouseDown)
            {
                this.Close();
                Event.current.Use();
                return;
            }

            ExpanseUI.DrawBackground(inRect, ExpanseUI.DarkGray);
            ExpanseUI.DrawAngularBorder(inRect, ExpanseUI.Blue, MAIN_BORDER_WIDTH, MAIN_BORDER_CORNER_RADIUS);

            Rect workingRect = inRect.Inset(CONTENT_INSET);

            Rect headerRect = new Rect(workingRect.x, workingRect.y, workingRect.width, headerHeight);
            DrawHeader(headerRect);

            Rect contentRect = new Rect(workingRect.x, workingRect.y + headerHeight + HEADER_SPACING,
                workingRect.width, workingRect.height - headerHeight - HEADER_SPACING);
            DrawModules(contentRect);
        }
        private void CalculateWindowSize()
        {
            float width = DEFAULT_WINDOW_WIDTH;
            float padding = WINDOW_PADDING;
            float modulePadding = MODULE_PADDING;
            float borderSpace = BORDER_SPACE;

            float totalModuleHeight = 0f;
            foreach (var module in currentModules)
            {
                totalModuleHeight += baseModuleHeight + module.ExtraHeight + modulePadding;
            }

            float neededHeight = headerHeight + borderSpace + totalModuleHeight + padding + ADDITIONAL_HEIGHT_BUFFER;

            float minHeight = MIN_WINDOW_HEIGHT;
            float maxHeight = MAX_WINDOW_HEIGHT;
            neededHeight = Mathf.Clamp(neededHeight, minHeight, maxHeight);

            float maxExtraWidth = 0f;
            foreach (var module in currentModules)
            {
                if (module.ExtraWidth > maxExtraWidth)
                    maxExtraWidth = module.ExtraWidth;
            }
            width += maxExtraWidth;
            width = Mathf.Clamp(width, MIN_WINDOW_WIDTH, MAX_WINDOW_WIDTH);

            Vector2 newTargetSize = new Vector2(width, neededHeight);

            if (!Mathf.Approximately(targetWindowSize.x, newTargetSize.x) ||
                !Mathf.Approximately(targetWindowSize.y, newTargetSize.y))
            {
                targetWindowSize = newTargetSize;
                if (windowAnimationStartTick == ANIMATION_NOT_STARTED)
                {
                    windowAnimationStartTick = Find.TickManager.TicksGame;
                }
            }
        }

        private void UpdateWindowSizeAnimation()
        {
            if (windowAnimationStartTick == ANIMATION_NOT_STARTED)
                return;

            int ticksElapsed = Find.TickManager.TicksGame - windowAnimationStartTick;
            float progress = (float)ticksElapsed / windowAnimationDurationTicks;

            if (progress >= ANIMATION_COMPLETE)
            {
                windowSize = targetWindowSize;
                windowAnimationStartTick = ANIMATION_NOT_STARTED;
            }
            else
            {
                float easedProgress = Mathf.SmoothStep(ANIMATION_START, ANIMATION_COMPLETE, progress);
                windowSize = Vector2.Lerp(windowSize, targetWindowSize, easedProgress);
            }
        }

        private void CheckForSizeChanges()
        {
            if (Find.TickManager.TicksGame - lastSizeCheckTick < SIZE_CHECK_INTERVAL_TICKS)
                return;

            lastSizeCheckTick = Find.TickManager.TicksGame;

            bool needsRecalculation = false;
            foreach (var module in currentModules)
            {
                if (module.IsSizeAnimating)
                {
                    needsRecalculation = true;
                    break;
                }
            }

            if (needsRecalculation)
            {
                CalculateWindowSize();
            }
        }

        private void RefreshModules()
        {
            currentModules.Clear();
            currentModules = selectedNode.GetUIModules();
            foreach (var module in currentModules)
            {
                module.Initialize(selectedNode);
            }
        }


        private void DrawHeader(Rect rect)
        {
            ExpanseUI.BeginExpanseStyle();

            Rect[] headerSections = ExpanseUI.SplitRectVertical(rect, HEADER_SECTION_ONE_HEIGHT, HEADER_SECTION_TWO_HEIGHT);

            ExpanseUI.DrawHeader(headerSections[0], "NETWORK_TERMINAL");

            string nodeInfo = $"NODE: {selectedNode.parent.LabelCap.ToUpper()}";
            ExpanseUI.DrawStatusText(headerSections[1], "", nodeInfo, ExpanseUI.TextColor);

            ExpanseUI.EndExpanseStyle();
        }

        private void DrawModules(Rect rect)
        {
            if (currentModules.Count == 0)
            {
                ExpanseUI.BeginExpanseStyle();
                GUI.color = ExpanseUI.Gray;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "NO_MODULES_AVAILABLE");
                Text.Anchor = TextAnchor.UpperLeft;
                ExpanseUI.EndExpanseStyle();
                return;
            }

            float padding = MODULE_PADDING;
            float availableWidth = rect.width - SCROLL_BAR_WIDTH;
            float moduleWidth = (availableWidth - (gridMaxColumns - 1) * padding) / gridMaxColumns;

            Rect viewRect = new Rect(0, 0, rect.width - SCROLL_BAR_WIDTH, GetTotalContentHeight());

            GUI.color = ExpanseUI.DarkGray;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            GUI.color = Color.white;

            float currentY = 0f;

            for (int i = 0; i < currentModules.Count; i++)
            {
                NetworkUIModule module = currentModules[i];
                float moduleHeight = baseModuleHeight + module.ExtraHeight;
                float actualModuleWidth = moduleWidth + module.ExtraWidth;

                Rect moduleRect = new Rect(0, currentY, actualModuleWidth, moduleHeight);

                DrawModuleFrame(moduleRect);
                module.DrawModule(moduleRect.Inset(MODULE_FRAME_INSET), selectedNode);

                currentY += moduleHeight + padding;
            }

            Widgets.EndScrollView();
        }

        private void DrawModuleFrame(Rect rect)
        {
            GUI.color = new Color(FRAME_BACKGROUND_RED, FRAME_BACKGROUND_GREEN, FRAME_BACKGROUND_BLUE, FRAME_BACKGROUND_ALPHA);
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            ExpanseUI.DrawAngularBorder(rect, ExpanseUI.Gray, MODULE_BORDER_WIDTH, MODULE_BORDER_CORNER_RADIUS);

            GUI.color = new Color(ExpanseUI.Blue.r, ExpanseUI.Blue.g, ExpanseUI.Blue.b, MODULE_HIGHLIGHT_ALPHA);
            GUI.DrawTexture(rect.Inset(MODULE_BORDER_INSET), BaseContent.WhiteTex);

            GUI.color = Color.white;
        }

        private float GetTotalContentHeight()
        {
            float totalHeight = 0f;
            float padding = MODULE_PADDING;

            foreach (var module in currentModules)
            {
                totalHeight += baseModuleHeight + module.ExtraHeight + padding;
            }

            return totalHeight + TOTAL_CONTENT_BOTTOM_PADDING;
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (Find.TickManager.TicksGame % UPDATE_INTERVAL_TICKS == 0)
            {
                foreach (var module in currentModules)
                {
                    module.UpdateData();
                }
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);

            foreach (var module in currentModules)
            {
            }
        }
    }
}