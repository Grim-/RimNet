using UnityEngine;
using Verse;

namespace RimNet
{
    public static class ExpanseUIAnimated
    {
        public static bool DrawAnimatedButton(Rect rect, string label, string animKey, Color buttonColor = default, bool enabled = true, float cutSize = 3f, GameFont font = GameFont.Tiny, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            bool mouseOver = Mouse.IsOver(rect) && enabled;
            float hoverProgress = ExpanseAnimations.Hover(animKey + "_hover", mouseOver);

            if (buttonColor == default) buttonColor = ExpanseUI.Gray;

            Color currentColor = buttonColor;
            Color fillColor = buttonColor;
            Color borderColor = buttonColor;

            if (!enabled)
            {
                currentColor = currentColor.Mult(0.5f);
                fillColor = fillColor.MultAlpha(0.1f);
                borderColor = borderColor.MultAlpha(0.6f);
            }
            else
            {
                float brightnessBoost = 1f + 0.1f * hoverProgress;
                currentColor = currentColor.Mult(brightnessBoost);
                fillColor = fillColor.MultAlpha(0.2f + 0.2f * hoverProgress);
                borderColor = borderColor.Mult((1f + 0.2f * hoverProgress));
            }

            GUI.color = fillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);

            ExpanseUI.DrawAngularBorder(rect, borderColor, cutSize, 1f + hoverProgress);

            if (hoverProgress > 0f && enabled)
            {
                GUI.color = currentColor.MultAlpha(0.1f * hoverProgress);
                Rect glowRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2);
                GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
            }

            Color textColor = enabled ? ExpanseUI.TextColor : ExpanseUI.TextColor.Mult(0.6f);
            GUI.color = textColor.SetAlpha(0.8f + 0.2f * hoverProgress);
            Text.Font = font;
            Text.Anchor = alignment;

            ExpanseUI.DrawFontLabel(rect, label, GameFont.Small, alignment);
            Text.Anchor = TextAnchor.UpperLeft;

            bool clicked = false;
            if (mouseOver && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                clicked = true;
                Event.current.Use();
            }

            GUI.color = Color.white;
            return clicked;
        }

        public static void DrawAnimatedPanel(Rect rect, string animKey, Color bgColor, Color borderColor, bool highlighted = false, float cutSize = 3f)
        {
            float highlightProgress = ExpanseAnimations.Hover(animKey + "_highlight", highlighted);
            float scale = ExpanseAnimations.Scale(animKey + "_scale", highlighted);

            Rect scaledRect = rect.ScaledBy(scale);

            ExpanseUI.DrawAngularPanel(scaledRect, bgColor, borderColor, cutSize);

            if (highlightProgress > 0f)
            {
                Color glowColor = ExpanseAnimations.GlowColor(animKey + "_glow", borderColor, highlighted, 0.3f);
                GUI.color = glowColor;
                Rect glowRect = scaledRect.ExpandedBy(3f);
                ExpanseUI.DrawAngularBorder(glowRect, glowColor, cutSize * 1.5f, 3f);
                GUI.color = Color.white;
            }
        }

        public static void DrawPulsingStatusStrip(Rect rect, string animKey, Color baseColor, bool shouldPulse)
        {
            Color statusColor = baseColor;
            if (shouldPulse)
            {
                float pulse = ExpanseAnimations.Pulse(animKey + "_pulse", 3f);
                statusColor = Color.Lerp(baseColor, Color.white, pulse * 0.3f);
            }

            GUI.color = statusColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        public static void DrawFadeInText(Rect rect, string text, string animKey, int index, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            float fadeProgress = ExpanseAnimations.StaggeredFade(animKey, index);
            GUI.color = ExpanseUI.TextColor.SetAlpha(fadeProgress);
            Text.Font = font;
            Text.Anchor = anchor;
            ExpanseUI.DrawFontLabel(rect, text, font, anchor);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        public static bool NeedsRepaint()
        {
            foreach (var key in new[] { "hover", "pulse", "fade", "scale", "scroll" })
            {
                if (ExpanseAnimations.IsAnimating(key))
                    return true;
            }
            return false;
        }
    }

    //[StaticConstructorOnStartup]
    //public class Window_NetworkGridView : Window
    //{
    //    private const float WINDOW_WIDTH = 900f;
    //    private const float WINDOW_HEIGHT = 700f;
    //    private const float WINDOW_MARGIN = 8f;

    //    private const float TOOLBAR_HEIGHT = 40f;
    //    private const float TOOLBAR_SPACING = 4f;
    //    private const float TOOLBAR_BUTTON_WIDTH = 80f;
    //    private const float CATEGORY_BUTTON_WIDTH = 70f;
    //    private const float SEARCH_BOX_WIDTH = 120f;

    //    private const float NODE_BUTTON_WIDTH = 270f;
    //    private const float NODE_BUTTON_HEIGHT = 80f;
    //    private const float GRID_PADDING = 8f;
    //    private const float SCROLLBAR_WIDTH = 16f;

    //    private const float STATUS_STRIP_WIDTH = 4f;
    //    private const float CONTENT_MARGIN = 8f;
    //    private const float CONTENT_VERTICAL_MARGIN = 2f;
    //    private const float TEXT_FIELD_MARGIN = 4f;
    //    private const float ANGULAR_CORNER_SIZE = 2f;
    //    private const float BORDER_THICKNESS = 4f;
    //    private const float BORDER_WIDTH = 2f;
    //    private const int MAX_CATEGORY_LABEL_LENGTH = 8;
    //    private const int MAX_VISIBLE_CATEGORIES = 4;
    //    private const float NAME_RECT_WIDTH_PERCENT = 0.6f;
    //    private const float NAME_RECT_HEIGHT_PERCENT = 0.6f;
    //    private const float TYPE_RECT_HEIGHT_PERCENT = 0.4f;
    //    private const float STATUS_RECT_WIDTH_PERCENT = 0.4f;

    //    public override Vector2 InitialSize => new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

    //    private readonly Comp_NetworkNode NetworkNode;
    //    public RimNet TargetNetwork { get; private set; }

    //    private Vector2 scrollPosition = Vector2.zero;
    //    private HashSet<NetworkFilterType> activeFilters = new HashSet<NetworkFilterType> { NetworkFilterType.All };
    //    private HashSet<DesignationCategoryDef> activeCategoryFilters = new HashSet<DesignationCategoryDef>();
    //    private string searchText = "";
    //    private List<Comp_NetworkNode> filteredNodes = new List<Comp_NetworkNode>();
    //    private List<DesignationCategoryDef> availableCategories = new List<DesignationCategoryDef>();

    //    //private Font Font;

    //    private NetworkFilterType[] logicalFilters = {
    //            NetworkFilterType.All,
    //            NetworkFilterType.Servers,
    //            NetworkFilterType.Clients,
    //            NetworkFilterType.Online,
    //            NetworkFilterType.Offline
    //        };

    //    public Window_NetworkGridView(RimNet targetNetwork, Comp_NetworkNode networkNode)
    //    {
    //        TargetNetwork = targetNetwork;
    //        NetworkNode = networkNode;
    //        draggable = true;
    //        doCloseX = true;
    //        closeOnClickedOutside = false;
    //        preventCameraMotion = false;
    //        absorbInputAroundWindow = true;
    //        drawShadow = false;
    //        doWindowBackground = false;
    //       // Font = LoadedModManager.GetMod<Mod_RimNet>().Font;
    //        RefreshFilteredNodes();
    //        RefreshAvailableCategories();
    //        SubscribeToNetworkEvents();
    //    }

    //    public override void DoWindowContents(Rect inRect)
    //    {
    //        ExpanseUI.DrawBackground(inRect, ExpanseUI.DarkGray);
    //        GUI.DrawTexture(new Rect(inRect.width / 2 - 150, inRect.height / 2 - 150, 300, 300), ExpanseUI.UIBGIcon);
    //        ExpanseUI.DrawAngularBorder(inRect, ExpanseUI.Blue, BORDER_THICKNESS, BORDER_WIDTH);

    //        if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
    //        {
    //            ExpanseUI.BeginExpanseStyle();
    //            GUI.color = ExpanseUI.TextColor;
    //            Text.Anchor = TextAnchor.MiddleCenter;


    //            ExpanseUI.DrawFontLabel(inRect, "NO NETWORK OR NODES FOUND", GameFont.Small);
    //            Text.Anchor = TextAnchor.UpperLeft;
    //            ExpanseUI.EndExpanseStyle();
    //            return;
    //        }

    //        Rect workingRect = inRect.Inset(WINDOW_MARGIN);

    //        Rect toolbarRect = new Rect(workingRect.x, workingRect.y, workingRect.width, TOOLBAR_HEIGHT);
    //        Rect contentRect = new Rect(workingRect.x, workingRect.y + TOOLBAR_HEIGHT + GRID_PADDING,
    //                                  workingRect.width, workingRect.height - TOOLBAR_HEIGHT - GRID_PADDING);

    //        DrawToolbar(toolbarRect);
    //        DrawNodeGrid(contentRect);
    //    }

    //    private void DrawToolbar(Rect toolbarRect)
    //    {
    //        float currentX = toolbarRect.x;

    //        foreach (var filter in logicalFilters)
    //        {
    //            Rect buttonRect = new Rect(currentX, toolbarRect.y, TOOLBAR_BUTTON_WIDTH, toolbarRect.height);
    //            bool isActive = activeFilters.Contains(filter);

    //            Color buttonColor = isActive ? ExpanseUI.Blue : ExpanseUI.Gray;

    //            if (ExpanseUI.DrawButton(buttonRect, filter.ToString().ToUpper(), buttonColor))
    //            {
    //                ToggleLogicalFilter(filter);
    //            }

    //            currentX += TOOLBAR_BUTTON_WIDTH + TOOLBAR_SPACING;
    //        }

    //        foreach (var category in availableCategories.Take(MAX_VISIBLE_CATEGORIES))
    //        {
    //            if (currentX + CATEGORY_BUTTON_WIDTH > toolbarRect.xMax - SEARCH_BOX_WIDTH - TOOLBAR_SPACING)
    //                break;

    //            Rect buttonRect = new Rect(currentX, toolbarRect.y, CATEGORY_BUTTON_WIDTH, toolbarRect.height);
    //            bool isActive = activeCategoryFilters.Contains(category);

    //            Color buttonColor = isActive ? ExpanseUI.Orange : ExpanseUI.Gray;
    //            string labelText = category.ToString().ToUpper();
    //            if (labelText.Length > MAX_CATEGORY_LABEL_LENGTH)
    //                labelText = labelText.Substring(0, MAX_CATEGORY_LABEL_LENGTH);

    //            if (ExpanseUI.DrawButton(buttonRect, labelText, buttonColor))
    //            {
    //                ToggleCategoryFilter(category);
    //            }

    //            currentX += CATEGORY_BUTTON_WIDTH + TOOLBAR_SPACING;
    //        }

    //        Rect searchRect = new Rect(toolbarRect.xMax - SEARCH_BOX_WIDTH, toolbarRect.y, SEARCH_BOX_WIDTH, toolbarRect.height);

    //        string newSearchText = GUI.TextField(searchRect.ContractedBy(TEXT_FIELD_MARGIN), searchText);
    //        if (newSearchText != searchText)
    //        {
    //            searchText = newSearchText;
    //            RefreshFilteredNodes();
    //        }
    //    }


    //    private Rect viewRect;

    //    private void DrawNodeGrid(Rect contentRect)
    //    {
    //        if (filteredNodes.Count == 0)
    //        {
    //            ExpanseUI.BeginExpanseStyle();
    //            GUI.color = ExpanseUI.TextColor;
    //            Text.Anchor = TextAnchor.MiddleCenter;
    //            ExpanseUI.DrawFontLabel(contentRect, "NO NODES MATCH CURRENT FILTERS", GameFont.Medium);
    //            Text.Anchor = TextAnchor.UpperLeft;
    //            ExpanseUI.EndExpanseStyle();
    //            return;
    //        }

    //        int columns = Mathf.FloorToInt((contentRect.width - GRID_PADDING) / (NODE_BUTTON_WIDTH + GRID_PADDING));
    //        if (columns < 1) columns = 1;

    //        int rows = Mathf.CeilToInt((float)filteredNodes.Count / columns);
    //        float totalHeight = rows * (NODE_BUTTON_HEIGHT + GRID_PADDING) - GRID_PADDING;

    //        viewRect = new Rect(0, 0, contentRect.width - SCROLLBAR_WIDTH, Mathf.Max(totalHeight, contentRect.height));
    //        Rect scrollRect = contentRect;




    //        scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, viewRect);

    //        for (int i = 0; i < filteredNodes.Count; i++)
    //        {
    //            int row = i / columns;
    //            int col = i % columns;

    //            float x = col * (NODE_BUTTON_WIDTH + GRID_PADDING);
    //            float y = row * (NODE_BUTTON_HEIGHT + GRID_PADDING);

    //            Rect nodeRect = new Rect(x, y, NODE_BUTTON_WIDTH, NODE_BUTTON_HEIGHT);

    //            if (nodeRect.yMax >= scrollPosition.y && nodeRect.y <= scrollPosition.y + contentRect.height)
    //            {
    //                DrawNodeButton(filteredNodes[i], nodeRect);
    //            }
    //        }

    //        GUI.EndScrollView();
    //    }

    //    private void DrawNodeButton(Comp_NetworkNode node, Rect rect)
    //    {
    //        bool isServer = IsServerNode(node);
    //        bool isConnected = node.IsConnectedToServer(out Comp_NetworkServer _);
    //        string failReason;
    //        bool isOperational = node.CanConnect(out failReason);

    //        Color bgColor = ExpanseUI.DarkGray;
    //        Color borderColor = ExpanseUI.Gray;
    //        Color statusColor = ExpanseUI.Red;

    //        if (isServer)
    //        {
    //            bgColor = new Color(ExpanseUI.Blue.r * 0.3f, ExpanseUI.Blue.g * 0.3f, ExpanseUI.Blue.b * 0.8f, 0.4f);
    //            borderColor = ExpanseUI.Blue;
    //        }

    //        if (isConnected && isOperational)
    //        {
    //            statusColor = ExpanseUI.Green;
    //        }
    //        else if (isConnected && !isOperational)
    //        {
    //            statusColor = ExpanseUI.Orange;
    //        }

    //        ExpanseUI.DrawAngularPanel(rect, bgColor, borderColor, ANGULAR_CORNER_SIZE);

    //        Rect statusStrip = new Rect(rect.x, rect.y, STATUS_STRIP_WIDTH, rect.height);
    //        GUI.color = statusColor;
    //        GUI.DrawTexture(statusStrip, BaseContent.WhiteTex);
    //        GUI.color = Color.white;

    //        Rect contentRect = new Rect(rect.x + CONTENT_MARGIN, rect.y + CONTENT_VERTICAL_MARGIN,
    //                                  rect.width - (CONTENT_MARGIN * 2), rect.height - (CONTENT_VERTICAL_MARGIN * 2));

    //        Rect nameRect = new Rect(contentRect.x, contentRect.y, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * NAME_RECT_HEIGHT_PERCENT);
    //        Rect typeRect = new Rect(contentRect.x, nameRect.yMax, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * TYPE_RECT_HEIGHT_PERCENT);
    //        Rect statusRect = new Rect(nameRect.xMax, contentRect.y, contentRect.width * STATUS_RECT_WIDTH_PERCENT, contentRect.height);

    //        ExpanseUI.BeginExpanseStyle();
    //        Text.Font = GameFont.Small;
    //        GUI.color = ExpanseUI.TextColor;
    //        Text.Anchor = TextAnchor.MiddleLeft;
    //        ExpanseUI.DrawFontLabel(nameRect, node.parent.LabelShort.ToUpper(), GameFont.Medium);

    //        Text.Font = GameFont.Tiny;
    //        GUI.color = isServer ? ExpanseUI.Blue : ExpanseUI.Gray;
    //        string nodeType = isServer ? "SERVER" : "CLIENT";
    //        //Widgets.Label(typeRect, nodeType);

    //        ExpanseUI.DrawFontLabel(typeRect, nodeType, GameFont.Medium);
    //        Text.Anchor = TextAnchor.MiddleRight;
    //        GUI.color = statusColor;
    //        string statusText = isConnected ? "ONLINE" : "OFFLINE";
    //        if (!isOperational && isConnected)
    //        {
    //            statusText = "ERROR";
    //            statusText += $"\nISSUE: {failReason}";
    //        }

    //        ExpanseUI.DrawFontLabel(statusRect, statusText, GameFont.Small);

    //        ExpanseUI.EndExpanseStyle();

    //        if (Widgets.ButtonInvisible(rect))
    //        {
    //            Find.WindowStack.Add(new Window_NetworkControlPanel(node));
    //        }

    //        string tooltipText = $"{nodeType}: {node.parent.LabelCap}\n" +
    //                           $"STATUS: {statusText}\n" +
    //                           $"NETWORK: {node.ConnectedNetwork?.ID ?? "NONE"}";

    //        TooltipHandler.TipRegion(rect, tooltipText);
    //    }

    //    private void ToggleLogicalFilter(NetworkFilterType filter)
    //    {
    //        if (filter == NetworkFilterType.All)
    //        {
    //            activeFilters.Clear();
    //            activeFilters.Add(NetworkFilterType.All);
    //        }
    //        else
    //        {
    //            activeFilters.Remove(NetworkFilterType.All);

    //            if (activeFilters.Contains(filter))
    //            {
    //                activeFilters.Remove(filter);
    //            }
    //            else
    //            {
    //                activeFilters.Add(filter);
    //            }

    //            if (activeFilters.Count == 0)
    //            {
    //                activeFilters.Add(NetworkFilterType.All);
    //            }
    //        }

    //        RefreshFilteredNodes();
    //    }

    //    private void ToggleCategoryFilter(DesignationCategoryDef category)
    //    {
    //        if (activeCategoryFilters.Contains(category))
    //        {
    //            activeCategoryFilters.Remove(category);
    //        }
    //        else
    //        {
    //            activeCategoryFilters.Add(category);
    //        }

    //        RefreshFilteredNodes();
    //    }

    //    private void RefreshAvailableCategories()
    //    {
    //        if (TargetNetwork?.NetworkNodes == null)
    //        {
    //            availableCategories.Clear();
    //            return;
    //        }

    //        availableCategories = TargetNetwork.NetworkNodes
    //            .Select(node => node.parent.def.designationCategory)
    //            .Distinct()
    //            .OrderBy(cat => cat.ToString())
    //            .ToList();
    //    }

    //    private void RefreshFilteredNodes()
    //    {
    //        if (TargetNetwork?.NetworkNodes == null)
    //        {
    //            filteredNodes.Clear();
    //            return;
    //        }

    //        filteredNodes = TargetNetwork.NetworkNodes.Where(node =>
    //        {
    //            if (!string.IsNullOrEmpty(searchText) &&
    //                !node.parent.LabelShort.ToLower().Contains(searchText.ToLower()) &&
    //                !node.NodeLabel.ToLower().Contains(searchText.ToLower()))
    //            {
    //                return false;
    //            }

    //            if (!activeFilters.Contains(NetworkFilterType.All))
    //            {
    //                bool matchesLogicalFilter = false;

    //                if (activeFilters.Contains(NetworkFilterType.Servers) && IsServerNode(node))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Clients) && !IsServerNode(node))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Online) && node.IsConnectedToServer(out _))
    //                    matchesLogicalFilter = true;
    //                if (activeFilters.Contains(NetworkFilterType.Offline) && !node.IsConnectedToServer(out _))
    //                    matchesLogicalFilter = true;

    //                if (!matchesLogicalFilter)
    //                    return false;
    //            }

    //            if (activeCategoryFilters.Count > 0)
    //            {
    //                if (!activeCategoryFilters.Contains(node.parent.def.designationCategory))
    //                    return false;
    //            }

    //            return true;
    //        }).ToList();

    //        filteredNodes = filteredNodes.OrderBy(n => !IsServerNode(n))
    //                                   .ThenBy(n => n.parent.LabelShort)
    //                                   .ToList();
    //    }

    //    private bool IsServerNode(Comp_NetworkNode node)
    //    {
    //        return node.parent.TryGetComp<Comp_NetworkServer>() != null;
    //    }

    //    private void SubscribeToNetworkEvents()
    //    {
    //        if (TargetNetwork != null)
    //        {
    //            TargetNetwork.NetworkMessageSent += OnNetworkEvent;
    //            TargetNetwork.NetworkMessageReceived += OnNetworkEvent;
    //            TargetNetwork.NodeStatusChanged += OnNodeStatusChanged;
    //        }
    //    }

    //    private void UnsubscribeFromNetworkEvents()
    //    {
    //        if (TargetNetwork != null)
    //        {
    //            TargetNetwork.NetworkMessageSent -= OnNetworkEvent;
    //            TargetNetwork.NetworkMessageReceived -= OnNetworkEvent;
    //            TargetNetwork.NodeStatusChanged -= OnNodeStatusChanged;
    //        }
    //    }

    //    private void OnNetworkEvent(object sender, NetworkMessageEventArgs e)
    //    {
    //        RefreshFilteredNodes();
    //    }

    //    private void OnNodeStatusChanged(object sender, NetworkStatusEventArgs e)
    //    {
    //        RefreshFilteredNodes();
    //        RefreshAvailableCategories();
    //    }

    //    public override void PreClose()
    //    {
    //        base.PreClose();
    //        UnsubscribeFromNetworkEvents();
    //    }
    //}
}