using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public abstract class NetworkUIModule
    {
        protected List<UIWidget> widgets = new List<UIWidget>();
        protected Comp_NetworkNode currentNode;

        public virtual int Priority => 1;
        public virtual float TopMargin => 5f;
        public virtual float SideMargin => 5f;

        private float targetExtraHeight = 0f;
        private float targetExtraWidth = 0f;
        private float currentExtraHeight = 0f;
        private float currentExtraWidth = 0f;
        private int animationStartTick = -1;
        private int animationDurationTicks = 30; 
        private System.Action onSizeChangeComplete;

        public virtual float ExtraHeight => currentExtraHeight;
        public virtual float ExtraWidth => currentExtraWidth;

        public bool IsSizeAnimating => animationStartTick != -1 &&
            (Find.TickManager.TicksGame - animationStartTick) < animationDurationTicks;

        public abstract bool CanHandleComponent(ThingWithComps thing);
        public abstract void Initialize(Comp_NetworkNode node);

        public virtual void UpdateData()
        {
        }

        public virtual void DrawModule(Rect rect, Comp_NetworkNode node)
        {
            currentNode = node;
            UpdateData();
            UpdateSizeAnimation();
            LayoutWidgets(rect);
        }

        protected void RequestSizeChange(float newExtraHeight, float newExtraWidth = 0f, System.Action onComplete = null)
        {
            if (Mathf.Approximately(targetExtraHeight, newExtraHeight) &&
                Mathf.Approximately(targetExtraWidth, newExtraWidth))
                return;

            targetExtraHeight = newExtraHeight;
            targetExtraWidth = newExtraWidth;
            animationStartTick = Find.TickManager.TicksGame;
            onSizeChangeComplete = onComplete;
        }

        protected void SetSizeImmediate(float extraHeight, float extraWidth = 0f)
        {
            targetExtraHeight = extraHeight;
            targetExtraWidth = extraWidth;
            currentExtraHeight = extraHeight;
            currentExtraWidth = extraWidth;
            animationStartTick = -1;
        }

        private void UpdateSizeAnimation()
        {
            if (animationStartTick == -1)
                return;

            int ticksElapsed = Find.TickManager.TicksGame - animationStartTick;
            float progress = (float)ticksElapsed / animationDurationTicks;

            if (progress >= 1f)
            {
                currentExtraHeight = targetExtraHeight;
                currentExtraWidth = targetExtraWidth;
                animationStartTick = -1;
                onSizeChangeComplete?.Invoke();
                onSizeChangeComplete = null;
            }
            else
            {
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                float startHeight = animationStartTick == Find.TickManager.TicksGame ? currentExtraHeight :
                    Mathf.Lerp(targetExtraHeight, currentExtraHeight, 1f - easedProgress);
                float startWidth = animationStartTick == Find.TickManager.TicksGame ? currentExtraWidth :
                    Mathf.Lerp(targetExtraWidth, currentExtraWidth, 1f - easedProgress);

                currentExtraHeight = Mathf.Lerp(startHeight, targetExtraHeight, easedProgress);
                currentExtraWidth = Mathf.Lerp(startWidth, targetExtraWidth, easedProgress);
            }
        }

        protected void LayoutWidgets(Rect rect)
        {
            float padding = 5f;
            int cols = 1;
            float widgetWidth = (rect.width - (cols - 1) * padding - (SideMargin * 2)) / cols;
            float widgetHeight = rect.height - TopMargin;

            for (int i = 0; i < widgets.Count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                float x = rect.x + SideMargin + col * (widgetWidth + padding);
                float y = rect.y + TopMargin + row * (widgetHeight + padding);
                var widgetRect = new Rect(x, y, widgetWidth - SideMargin, widgetHeight - TopMargin);
                widgets[i].Draw(widgetRect);
            }
        }

        protected void SendNetworkMessage(string messageType, string data)
        {
        }
    }
}