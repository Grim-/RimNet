using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class ITab_DroneController : ITab
    {
        private Vector2 scrollPosition;

        // --- Constants for new row-based layout ---
        private const float RowHeight = 60f;
        private const float RowPadding = 8f;
        private const float HeaderHeight = 30f;
        private const float SectionPadding = 15f;
        private const float ScrollBarWidth = 16f;

        public ITab_DroneController()
        {
            this.labelKey = "DroneControllerTab";
            this.size = new Vector2(520f, 450f);
            this.scrollPosition = Vector2.zero;
        }

        public CompDroneController DroneComp => SelThing.TryGetComp<CompDroneController>();

        /// <summary>
        /// Main method to draw the tab's content.
        /// </summary>
        protected override void FillTab()
        {
            ExpanseUI.BeginExpanseStyle();
            Rect mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(4f);
            Rect contentRect = mainRect.ContractedBy(RowPadding);
            var activeDrones = DroneComp.ActiveDrones;
            var storedDrones = DroneComp.StoredDrones;
            float totalContentHeight = 58f;
            if (activeDrones.Any())
            {
                totalContentHeight += HeaderHeight + (activeDrones.Count * (RowHeight + RowPadding));
            }
            if (storedDrones.Any())
            {
                if (totalContentHeight > 0) totalContentHeight += SectionPadding;
                totalContentHeight += HeaderHeight + (storedDrones.Count * (RowHeight + RowPadding));
            }
            Rect viewRect = new Rect(0f, 0f, contentRect.width - ScrollBarWidth, totalContentHeight);
            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
            float currentY = 0f;
            Rect headerRect = new Rect(0, currentY, contentRect.width - ScrollBarWidth, 40f);
            currentY += 48f;

            ExpanseUI.DrawAngularPanel(headerRect, new Color(0.72f, 0.75f, 0.73f, 0.3f), ExpanseUI.Gray.SetAlpha(0.3f));
            float buttonWidth = headerRect.width / 3f;

            Rect deployAllButton = new Rect(headerRect.x, headerRect.y, buttonWidth, headerRect.height);
            if (ExpanseUI.DrawButton(deployAllButton, "DEPLOY ALL", ExpanseUI.Blue))
            {
                DroneComp.DeployDrones();
            }
            Rect returnAllButton = new Rect(headerRect.x + buttonWidth, headerRect.y, buttonWidth, headerRect.height);
            if (ExpanseUI.DrawButton(returnAllButton, "RETURN ALL", ExpanseUI.ExpanseOrange))
            {
                DroneComp.ReturnDrones();
            }
            Rect ejectAllButton = new Rect(headerRect.x + buttonWidth * 2f, headerRect.y, buttonWidth, headerRect.height);
            if (ExpanseUI.DrawButton(ejectAllButton, "EJECT ALL", ExpanseUI.Red))
            {
                DroneComp.EjectDrones();
            }
            if (activeDrones.Any())
            {
                DrawDroneList(activeDrones, "Active Drones", ref currentY, viewRect.width);
            }

            currentY += 8f;

            if (storedDrones.Any())
            {
                if (activeDrones.Any())
                {
                    currentY += SectionPadding;
                }
                DrawDroneList(storedDrones, "Stored Drones", ref currentY, viewRect.width);
            }
            Widgets.EndScrollView();
            ExpanseUI.EndExpanseStyle();
        }

        private void DrawDroneList(List<Drone> drones, string label, ref float currentY, float viewWidth)
        {
            Rect headerRect = new Rect(0f, currentY, viewWidth, HeaderHeight);
            ExpanseUI.DrawHeader(headerRect, label.ToUpper(), ExpanseUI.Blue);
            currentY += HeaderHeight + RowPadding / 2;

            foreach (var drone in drones.ToArray())
            {
                Rect rowRect = new Rect(0f, currentY, viewWidth, RowHeight);
                DrawDroneRow(rowRect, drone);
                currentY += RowHeight + RowPadding;
            }
        }

        private void DrawDroneRow(Rect rowRect, Drone drone)
        {
            Color rowColor = Mouse.IsOver(rowRect) ? ExpanseUI.Gray : ExpanseUI.DarkGray;
            ExpanseUI.DrawAngularPanel(rowRect, rowColor.SetAlpha(0.4f), ExpanseUI.Gray);
            TooltipHandler.TipRegion(rowRect, drone.GetTooltip());

            Rect contentRect = rowRect.ContractedBy(4f);
            Rect leftPart = contentRect.LeftPartPixels(RowHeight - 8f);
            Rect middlePart = new Rect(leftPart.xMax + 5, contentRect.y, contentRect.width - leftPart.width - 130f, contentRect.height);
            Rect rightPart = contentRect.RightPartPixels(170f);

            Rect portraitRect = leftPart.ContractedBy(2f);


            if (drone.IsDeployed && Mouse.IsOver(portraitRect))
            {
                Find.CameraDriver.JumpToCurrentMapLoc(drone.Position);
            }

            ExpanseUI.DrawAngularPanel(portraitRect, Color.black, ExpanseUI.Gray);
            Widgets.DrawTextureFitted(portraitRect.ContractedBy(2f), PortraitsCache.Get(drone, new Vector2(portraitRect.width, portraitRect.height), Rot4.South), 1f);

            float nameHeight = middlePart.height * 0.5f;
            float statusHeight = middlePart.height * 0.5f;

            Rect nameRect = new Rect(middlePart.x, middlePart.y, middlePart.width, nameHeight);
            ExpanseUI.DrawFontLabel(nameRect, drone.LabelCap, GameFont.Small, TextAnchor.MiddleLeft);

            Rect statusRect = new Rect(middlePart.x, middlePart.y + nameHeight, middlePart.width / 2, statusHeight);
            string statusLabel = drone.DroneState.ToString().ToUpper();

            ExpanseUI.BeginExpanseStyle();
            Widgets.Label(statusRect, "STATUS: " + statusLabel);
            ExpanseUI.EndExpanseStyle();

            Rect healthRect = new Rect(statusRect.xMax - 10, middlePart.y + nameHeight + 2, 100, statusHeight - 4);
            float healthPct = drone.health.summaryHealth.SummaryHealthPercent;
            ExpanseUI.DrawProgressBar(healthRect, healthPct, ExpanseUI.GetPercentageColor(healthPct), default(Color), true, TextAnchor.MiddleCenter);

            if (drone.IsDeployed)
            {
                float widthPerButton = rightPart.width / 3;

                Rect locateButtonRect = new Rect(rightPart.x, rightPart.y, widthPerButton, rightPart.height);
                if (ExpanseUI.DrawButton(locateButtonRect, "SELECT", ExpanseUI.Blue))
                {
                    foreach (var item in Find.Selector.SelectedObjects.ToArray())
                    {
                        Find.Selector.Deselect(item);
                    }
                    Find.Selector.Select(drone);
                }

                Rect returnButtonRect = new Rect(locateButtonRect.xMax + 4, rightPart.y, widthPerButton, rightPart.height);
                if (ExpanseUI.DrawButton(returnButtonRect, "RETURN", ExpanseUI.Orange))
                {
                    drone.TryStartReturn();
                }

                Rect toggleStateButton = new Rect(returnButtonRect.xMax + 4, rightPart.y, widthPerButton, rightPart.height);
                if (ExpanseUI.DrawButton(toggleStateButton, drone.IsEnabled ? "DISABLE" : "ENABLE", drone.IsEnabled ? ExpanseUI.Green : ExpanseUI.Orange))
                {
                    if (drone.IsEnabled)
                    {
                        drone.Disable();
                    }
                    else drone.Enable();
                }
            }
            else
            {
                Rect deployButtonRect = new Rect(rightPart.x, rightPart.y, rightPart.width / 2 - 2, rightPart.height);
                if (ExpanseUI.DrawButton(deployButtonRect, "DEPLOY", ExpanseUI.Green))
                {
                    DroneComp.TryDeployDrone(drone);
                }

                Rect ejectButtonRect = new Rect(deployButtonRect.xMax + 4, rightPart.y, rightPart.width / 2 - 2, rightPart.height);
                if (ExpanseUI.DrawButton(ejectButtonRect, "EJECT", ExpanseUI.Red))
                {
                    DroneComp.EjectDrone(drone);
                }
            }
        }
    }
}