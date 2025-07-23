using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_DroneController : ITab
    {
        private Vector2 scrollPosition;

        // Constants for layout
        private const float BoxSize = 80f;
        private const float Padding = 10f;
        private const float GridPadding = 20f;
        private const float LabelHeight = 30f;
        private const int ItemsPerRow = 4;
        private const float ScrollBarWidth = 16f;

        public ITab_DroneController()
        {
            this.labelKey = "DroneControllerTab";
            this.size = new Vector2(500f, 400f);
            this.scrollPosition = Vector2.zero;
        }

        public CompDroneController DroneComp => SelThing.TryGetComp<CompDroneController>();

        protected override void FillTab()
        {
            var activeDrones = DroneComp.activeDrones;
            var storedDrones = DroneComp.StoredDrones;

            float totalContentHeight = 0f;
            int activeRows = activeDrones.Any() ? Mathf.CeilToInt(activeDrones.Count / (float)ItemsPerRow) : 0;
            int storedRows = storedDrones.Any() ? Mathf.CeilToInt(storedDrones.Count / (float)ItemsPerRow) : 0;

            if (activeRows > 0)
            {
                totalContentHeight += LabelHeight + (activeRows * (BoxSize + Padding));
            }
            if (storedRows > 0)
            {
                if (totalContentHeight > 0) totalContentHeight += GridPadding;
                totalContentHeight += LabelHeight + (storedRows * (BoxSize + Padding));
            }
            if (totalContentHeight > 0) totalContentHeight -= Padding;

            // --- ScrollView and Content Drawing ---
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - ScrollBarWidth, totalContentHeight); 

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float currentY = 0f;

            // --- Active Drones Grid ---
            if (activeDrones.Any())
            {
                DrawDroneGrid(activeDrones, activeRows, "Active", ref viewRect, ref currentY);
            }

            // --- Stored Drones Grid ---
            if (storedDrones.Any())
            {
                if (activeDrones.Any())
                {
                    currentY += GridPadding;
                }

                DrawDroneGrid(storedDrones, activeRows, "Stored", ref viewRect, ref currentY);
            }

            Widgets.EndScrollView();
        }

        //private float DrawStoredDrones(List<Drone> storedDrones, Rect viewRect, float currentY)
        //{
        //    // Grid Label
        //    Rect storedLabelRect = new Rect(0f, currentY, viewRect.width, LabelHeight);
        //    Text.Font = GameFont.Medium;
        //    Widgets.Label(storedLabelRect, "Stored Drones");
        //    Text.Font = GameFont.Small;
        //    currentY += LabelHeight;

        //    // Grid Content
        //    for (int i = 0; i < storedDrones.Count; i++)
        //    {
        //        Drone drone = storedDrones[i];
        //        int row = i / ItemsPerRow;
        //        int col = i % ItemsPerRow;
        //        float x = col * (BoxSize + Padding);
        //        float y = currentY + row * (BoxSize + Padding);
        //        Rect boxRect = new Rect(x, y, BoxSize, BoxSize);

        //        DrawDrone(drone, boxRect);
        //    }

        //    return currentY;
        //}

        private void DrawDroneGrid(List<Drone> activeDrones, int activeRows, string gridLabel, ref Rect viewRect, ref float currentY)
        {
            // Grid Label
            Rect activeLabelRect = new Rect(0f, currentY, viewRect.width, LabelHeight);
            Text.Font = GameFont.Medium;
            Widgets.Label(activeLabelRect, gridLabel);
            Text.Font = GameFont.Small;
            currentY += LabelHeight;

            for (int i = 0; i < activeDrones.Count; i++)
            {
                int row = i / ItemsPerRow;
                int col = i % ItemsPerRow;
                float x = col * (BoxSize + Padding);
                float y = currentY + row * (BoxSize + Padding);
                Rect boxRect = new Rect(x, y, BoxSize, BoxSize);
                DrawDrone(activeDrones[i], boxRect);
            }
            currentY += activeRows * (BoxSize + Padding);
        }

        private void DrawDrone(Drone drone, Rect boxRect)
        {
            Widgets.DrawBoxSolidWithOutline(boxRect, Color.grey, GetStatusColor(drone));

            Rect portraitRect = new Rect(boxRect.x + 5f, boxRect.y + 5f, 70f, 50f);
            Widgets.DrawTextureFitted(portraitRect, PortraitsCache.Get(drone, new Vector2(70f, 50f), default), 2f);

            Rect labelRect = new Rect(boxRect.x, boxRect.y + 55f, BoxSize, 20f);
            Widgets.DrawBoxSolidWithOutline(boxRect, Color.clear, Color.white);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, drone.DroneState.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Color GetStatusColor(Drone drone)
        {
            switch (drone.DroneState)
            {
                case DroneState.ACTIVE:
                    return Color.green;
                case DroneState.RETURNING:
                    return Color.red;
                default:
                    return Color.white;
            }
        }
    }
}