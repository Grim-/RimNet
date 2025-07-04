using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public enum NetworkFilterType
    {
        All,
        Servers,
        Clients,
        Online,
        Offline,
        Connected,
        Disconnected
    }


    [StaticConstructorOnStartup]
    public class Window_NetworkGridView : Window
    {
        private const float WINDOW_WIDTH = 900f;
        private const float WINDOW_HEIGHT = 700f;
        private const float WINDOW_MARGIN = 8f;

        private const float TOOLBAR_HEIGHT = 40f;
        private const float TOOLBAR_SPACING = 4f;
        private const float TOOLBAR_BUTTON_WIDTH = 80f;
        private const float CATEGORY_BUTTON_WIDTH = 70f;
        private const float SEARCH_BOX_WIDTH = 120f;
        private const float ARROW_BUTTON_WIDTH = 30f;
        private const float SEARCH_TOGGLE_WIDTH = 30f;

        private const float NODE_BUTTON_WIDTH = 270f;
        private const float NODE_BUTTON_HEIGHT = 80f;
        private const float GRID_PADDING = 8f;
        private const float SCROLLBAR_WIDTH = 16f;

        private const float STATUS_STRIP_WIDTH = 4f;
        private const float CONTENT_MARGIN = 8f;
        private const float CONTENT_VERTICAL_MARGIN = 2f;
        private const float TEXT_FIELD_MARGIN = 4f;
        private const float ANGULAR_CORNER_SIZE = 2f;
        private const float BORDER_THICKNESS = 4f;
        private const float BORDER_WIDTH = 2f;
        private const int MAX_CATEGORY_LABEL_LENGTH = 8;
        private const int MAX_VISIBLE_CATEGORIES = 4;
        private const float NAME_RECT_WIDTH_PERCENT = 0.6f;
        private const float NAME_RECT_HEIGHT_PERCENT = 0.6f;
        private const float TYPE_RECT_HEIGHT_PERCENT = 0.4f;
        private const float STATUS_RECT_WIDTH_PERCENT = 0.4f;

        public override Vector2 InitialSize => new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

        private readonly Comp_NetworkNode NetworkNode;
        public RimNet TargetNetwork { get; private set; }
        private readonly string windowID;

        private Vector2 scrollPosition = Vector2.zero;
        private HashSet<NetworkFilterType> activeFilters = new HashSet<NetworkFilterType> { NetworkFilterType.All };
        private HashSet<DesignationCategoryDef> activeCategoryFilters = new HashSet<DesignationCategoryDef>();
        private string searchText = "";
        private List<Comp_NetworkNode> filteredNodes = new List<Comp_NetworkNode>();
        private List<DesignationCategoryDef> availableCategories = new List<DesignationCategoryDef>();

        private int categoryStartIndex = 0;
        private bool showSearchBar = true;

        private NetworkFilterType[] logicalFilters = {
                NetworkFilterType.All,
                NetworkFilterType.Servers,
                NetworkFilterType.Clients,
                NetworkFilterType.Online,
                NetworkFilterType.Offline
            };

        private static readonly Texture2D ArrowLeft = ContentFinder<Texture2D>.Get("UI/Icons/ArrowLeft", false) ?? BaseContent.BadTex;
        private static readonly Texture2D ArrowRight = ContentFinder<Texture2D>.Get("UI/Icons/ArrowRight", false) ?? BaseContent.BadTex;
        private static readonly Texture2D SearchIcon = ContentFinder<Texture2D>.Get("UI/Icons/Search", false) ?? BaseContent.BadTex;

        public Window_NetworkGridView(RimNet targetNetwork, Comp_NetworkNode networkNode)
        {
            TargetNetwork = targetNetwork;
            NetworkNode = networkNode;
            windowID = "NetworkGrid_" + targetNetwork?.ID ?? "Unknown";
            draggable = true;
            doCloseX = true;
            closeOnClickedOutside = false;
            preventCameraMotion = false;
            absorbInputAroundWindow = true;
            drawShadow = false;
            doWindowBackground = false;
            RefreshFilteredNodes();
            RefreshAvailableCategories();
            SubscribeToNetworkEvents();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            bool needsRepaint = false;
            needsRepaint |= ExpanseAnimations.IsAnimating(windowID + "_searchBar");
            needsRepaint |= ExpanseAnimations.IsAnimating(windowID + "_toolbarGlow");

            for (int i = 0; i < filteredNodes.Count; i++)
            {
                string nodeKey = windowID + "_node_" + filteredNodes[i].GetHashCode();
                needsRepaint |= ExpanseAnimations.IsAnimating(nodeKey + "_hover");
                needsRepaint |= ExpanseAnimations.IsAnimating(nodeKey + "_pulse");
            }

            needsRepaint |= ExpanseAnimations.IsAnimating(windowID + "_scroll");

            if (needsRepaint)
                GUI.changed = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            ExpanseUI.DrawBackground(inRect, ExpanseUI.DarkGray);
            GUI.DrawTexture(new Rect(inRect.width / 2 - 150, inRect.height / 2 - 150, 300, 300), ExpanseUI.UIBGIcon);

            float borderGlow = ExpanseAnimations.Pulse(windowID + "_borderGlow", 1f, 0.8f, 1f);
            Color borderColor = Color.Lerp(ExpanseUI.Blue, ExpanseUI.Blue.SetAlpha(0.6f), 1f - borderGlow);
            ExpanseUI.DrawAngularBorder(inRect, borderColor, BORDER_THICKNESS, BORDER_WIDTH);

            if (TargetNetwork?.NetworkNodes == null || TargetNetwork.NetworkNodes.Count == 0)
            {
                ExpanseUI.BeginExpanseStyle();
                GUI.color = ExpanseUI.TextColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                ExpanseUI.DrawFontLabel(inRect, "NO NETWORK OR NODES FOUND", GameFont.Small);
                Text.Anchor = TextAnchor.UpperLeft;
                ExpanseUI.EndExpanseStyle();
                return;
            }

            Rect workingRect = inRect.Inset(WINDOW_MARGIN);

            Rect toolbarRect = new Rect(inRect.x, inRect.y, inRect.width, TOOLBAR_HEIGHT);
            Rect contentRect = new Rect(workingRect.x, workingRect.y + TOOLBAR_HEIGHT + GRID_PADDING,
                                      workingRect.width, workingRect.height - TOOLBAR_HEIGHT - GRID_PADDING);

            DrawToolbar(toolbarRect);
            DrawNodeGrid(contentRect);
        }

        private void DrawToolbar(Rect toolbarRect)
        {
            float currentX = toolbarRect.x;

            foreach (var filter in logicalFilters)
            {
                Rect buttonRect = new Rect(currentX, toolbarRect.y, TOOLBAR_BUTTON_WIDTH, toolbarRect.height);
                bool isActive = activeFilters.Contains(filter);

                Color buttonColor = isActive ? ExpanseUI.Blue : ExpanseUI.Gray;

                if (isActive)
                {
                    float glowPulse = ExpanseAnimations.Pulse(windowID + "_filter_" + filter, 2f);
                    Color glowColor = new Color(buttonColor.r, buttonColor.g, buttonColor.b, 0.1f + 0.1f * glowPulse);
                    GUI.color = glowColor;
                    Rect glowRect = buttonRect.ExpandedBy(2f);
                    GUI.DrawTexture(glowRect, BaseContent.WhiteTex);
                }

                if (ExpanseUIAnimated.DrawAnimatedButton(buttonRect, filter.ToString().ToUpper(),
                    windowID + "_filterBtn_" + filter, buttonColor))
                {
                    ToggleLogicalFilter(filter);
                }

                currentX += TOOLBAR_BUTTON_WIDTH + TOOLBAR_SPACING;
            }

            // Calculate category area
            float searchBarProgress = ExpanseAnimations.Fade(windowID + "_searchBar", showSearchBar);
            float categoryAreaStart = currentX;
            float searchToggleX = toolbarRect.xMax - SEARCH_TOGGLE_WIDTH - TOOLBAR_SPACING;
            float searchBoxX = searchToggleX - (SEARCH_BOX_WIDTH * searchBarProgress) - TOOLBAR_SPACING;
            float categoryAreaEnd = searchBoxX - TOOLBAR_SPACING;
            float categoryAreaWidth = categoryAreaEnd - categoryAreaStart;

            int visibleCategoryCount = Mathf.Min(MAX_VISIBLE_CATEGORIES,
                Mathf.FloorToInt((categoryAreaWidth - ARROW_BUTTON_WIDTH * 2 - TOOLBAR_SPACING * 2) / (CATEGORY_BUTTON_WIDTH + TOOLBAR_SPACING)));

            bool needsArrows = availableCategories.Count > visibleCategoryCount;

            if (needsArrows)
            {
                Rect leftArrowRect = new Rect(currentX, toolbarRect.y, ARROW_BUTTON_WIDTH, toolbarRect.height);
                if (ExpanseUI.DrawIconButton(leftArrowRect, ArrowLeft, ExpanseUI.Orange, true, 2f, "Previous categories"))
                {
                    categoryStartIndex = Mathf.Max(0, categoryStartIndex - visibleCategoryCount);
                }
                currentX += ARROW_BUTTON_WIDTH + TOOLBAR_SPACING;
            }

            int endIndex = Mathf.Min(categoryStartIndex + visibleCategoryCount, availableCategories.Count);
            for (int i = categoryStartIndex; i < endIndex; i++)
            {
                var category = availableCategories[i];
                Rect buttonRect = new Rect(currentX, toolbarRect.y, CATEGORY_BUTTON_WIDTH, toolbarRect.height);
                bool isActive = activeCategoryFilters.Contains(category);

                Color buttonColor = isActive ? ExpanseUI.Orange : ExpanseUI.Gray;
                string labelText = category.label.ToUpper();
                if (labelText.Length > MAX_CATEGORY_LABEL_LENGTH)
                    labelText = labelText.Substring(0, MAX_CATEGORY_LABEL_LENGTH);

                if (ExpanseUIAnimated.DrawAnimatedButton(buttonRect, labelText,
                    windowID + "_catBtn_" + category.defName, buttonColor))
                {
                    ToggleCategoryFilter(category);
                }

                currentX += CATEGORY_BUTTON_WIDTH + TOOLBAR_SPACING;
            }

            if (needsArrows)
            {
                Rect rightArrowRect = new Rect(currentX, toolbarRect.y, ARROW_BUTTON_WIDTH, toolbarRect.height);
                if (ExpanseUI.DrawIconButton(rightArrowRect, ArrowRight, ExpanseUI.Orange, true, 2f, "Next categories"))
                {
                    categoryStartIndex = Mathf.Min(availableCategories.Count - visibleCategoryCount,
                        categoryStartIndex + visibleCategoryCount);
                }
            }

            Rect toggleRect = new Rect(searchToggleX, toolbarRect.y, SEARCH_TOGGLE_WIDTH, toolbarRect.height);
            if (ExpanseUI.DrawIconButton(toggleRect, SearchIcon, showSearchBar ? ExpanseUI.Blue : ExpanseUI.Gray,
                true, 2f, showSearchBar ? "Hide search" : "Show search"))
            {
                showSearchBar = !showSearchBar;
            }


            if (searchBarProgress > 0.01f)
            {
                float searchWidth = SEARCH_BOX_WIDTH * searchBarProgress;
                Rect searchRect = new Rect(searchToggleX - searchWidth - TOOLBAR_SPACING,
                    toolbarRect.y, searchWidth, toolbarRect.height);

                GUI.color = new Color(1f, 1f, 1f, searchBarProgress);
                string newSearchText = GUI.TextField(searchRect.ContractedBy(TEXT_FIELD_MARGIN), searchText);
                if (newSearchText != searchText)
                {
                    searchText = newSearchText;
                    RefreshFilteredNodes();
                }
                GUI.color = Color.white;
            }
        }

        private Rect viewRect;

        private void DrawNodeGrid(Rect contentRect)
        {
            if (filteredNodes.Count == 0)
            {
                ExpanseUI.BeginExpanseStyle();
                GUI.color = ExpanseUI.TextColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                ExpanseUI.DrawFontLabel(contentRect, "NO NODES MATCH CURRENT FILTERS", GameFont.Medium);
                Text.Anchor = TextAnchor.UpperLeft;
                ExpanseUI.EndExpanseStyle();
                return;
            }

            int columns = Mathf.FloorToInt((contentRect.width - GRID_PADDING) / (NODE_BUTTON_WIDTH + GRID_PADDING));
            if (columns < 1) columns = 1;

            int rows = Mathf.CeilToInt((float)filteredNodes.Count / columns);
            float totalHeight = rows * (NODE_BUTTON_HEIGHT + GRID_PADDING) - GRID_PADDING;

            viewRect = new Rect(0, 0, contentRect.width - SCROLLBAR_WIDTH, Mathf.Max(totalHeight, contentRect.height));
            Rect scrollRect = contentRect;

            // Smooth scrolling
            scrollPosition.y = ExpanseAnimations.SmoothScroll(windowID + "_scroll", scrollPosition.y, scrollPosition.y);
            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, viewRect);

            for (int i = 0; i < filteredNodes.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                float x = col * (NODE_BUTTON_WIDTH + GRID_PADDING);
                float y = row * (NODE_BUTTON_HEIGHT + GRID_PADDING);

                Rect nodeRect = new Rect(x, y, NODE_BUTTON_WIDTH, NODE_BUTTON_HEIGHT);

                if (nodeRect.yMax >= scrollPosition.y && nodeRect.y <= scrollPosition.y + contentRect.height)
                {
                    DrawNodeButton(filteredNodes[i], nodeRect, i);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawNodeButton(Comp_NetworkNode node, Rect rect, int index)
        {
            bool isServer = IsServerNode(node);
            bool isConnected = node.IsConnectedToServer(out Comp_NetworkServer _);
            string failReason;
            bool isOperational = node.CanConnect(out failReason);
            string nodeKey = windowID + "_node_" + node.GetHashCode();

            bool mouseOver = Mouse.IsOver(rect);
            float scale = ExpanseAnimations.Scale(nodeKey + "_scale", mouseOver);
            Rect scaledRect = rect.ScaledBy(scale);

            // Determine colors
            Color bgColor = ExpanseUI.DarkGray;
            Color borderColor = ExpanseUI.Gray;
            Color statusColor = ExpanseUI.Red;

            if (isServer)
            {
                bgColor = new Color(ExpanseUI.Blue.r * 0.3f, ExpanseUI.Blue.g * 0.3f, ExpanseUI.Blue.b * 0.8f, 0.4f);
                borderColor = ExpanseUI.Blue;
            }

            if (isConnected && isOperational)
            {
                statusColor = ExpanseUI.Green;
            }
            else if (isConnected && !isOperational)
            {
                statusColor = ExpanseUI.Orange;
            }

            ExpanseUIAnimated.DrawAnimatedPanel(scaledRect, nodeKey, bgColor, borderColor, mouseOver, ANGULAR_CORNER_SIZE);

            // Draw pulsing status strip for offline nodes
            Rect statusStrip = new Rect(scaledRect.x, scaledRect.y, STATUS_STRIP_WIDTH, scaledRect.height);
            bool shouldPulse = !isServer && !isConnected;
            ExpanseUIAnimated.DrawPulsingStatusStrip(statusStrip, nodeKey, statusColor, shouldPulse);


            Rect contentRect = new Rect(scaledRect.x + CONTENT_MARGIN, scaledRect.y + CONTENT_VERTICAL_MARGIN,
                                      scaledRect.width - (CONTENT_MARGIN * 2), scaledRect.height - (CONTENT_VERTICAL_MARGIN * 2));

            Rect nameRect = new Rect(contentRect.x, contentRect.y, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * NAME_RECT_HEIGHT_PERCENT);
            Rect typeRect = new Rect(contentRect.x, nameRect.yMax, contentRect.width * NAME_RECT_WIDTH_PERCENT, contentRect.height * TYPE_RECT_HEIGHT_PERCENT);
            Rect statusRect = new Rect(nameRect.xMax, contentRect.y, contentRect.width * STATUS_RECT_WIDTH_PERCENT, contentRect.height);

            float hoverProgress = ExpanseAnimations.Hover(nodeKey + "_hover", mouseOver);

            ExpanseUI.BeginExpanseStyle();
            Text.Font = GameFont.Small;
            GUI.color = ExpanseUI.TextColor.SetAlpha(0.8f + 0.2f * hoverProgress);
            Text.Anchor = TextAnchor.MiddleLeft;
            ExpanseUI.DrawFontLabel(nameRect, node.NodeLabel.ToUpper(), GameFont.Medium);

            Text.Font = GameFont.Tiny;
            GUI.color = isServer ? ExpanseUI.Blue : ExpanseUI.Gray;
            string nodeType = isServer ? "SERVER" : "CLIENT";
            ExpanseUI.DrawFontLabel(typeRect, nodeType, GameFont.Medium);

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = statusColor;
            string statusText = isConnected ? "ONLINE" : "OFFLINE";
            if (!isOperational && isConnected)
            {
                statusText = "ERROR";
                if (!string.IsNullOrEmpty(failReason))
                    statusText += $"\nISSUE: {failReason}";
            }

            ExpanseUI.DrawFontLabel(statusRect, statusText, GameFont.Small);

            ExpanseUI.EndExpanseStyle();

            // Handle click
            if (Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(new Window_NetworkControlPanel(node));
            }

            // Tooltip
            string tooltipText = $"{nodeType}: {node.NodeLabel}\n" +
                               $"STATUS: {statusText}\n" +
                               $"NETWORK: {node.ConnectedNetwork?.ID ?? "NONE"}";

            TooltipHandler.TipRegion(rect, tooltipText);
        }

        private void ToggleLogicalFilter(NetworkFilterType filter)
        {
            if (filter == NetworkFilterType.All)
            {
                activeFilters.Clear();
                activeFilters.Add(NetworkFilterType.All);
            }
            else
            {
                activeFilters.Remove(NetworkFilterType.All);

                if (activeFilters.Contains(filter))
                {
                    activeFilters.Remove(filter);
                }
                else
                {
                    activeFilters.Add(filter);
                }

                if (activeFilters.Count == 0)
                {
                    activeFilters.Add(NetworkFilterType.All);
                }
            }

            RefreshFilteredNodes();
        }

        private void ToggleCategoryFilter(DesignationCategoryDef category)
        {
            if (activeCategoryFilters.Contains(category))
            {
                activeCategoryFilters.Remove(category);
            }
            else
            {
                activeCategoryFilters.Add(category);
            }

            RefreshFilteredNodes();
        }

        private void RefreshAvailableCategories()
        {
            if (TargetNetwork?.NetworkNodes == null)
            {
                availableCategories.Clear();
                return;
            }

            availableCategories = TargetNetwork.NetworkNodes
                .Select(node => node.parent.def.designationCategory)
                .Where(cat => cat != null)
                .Distinct()
                .OrderBy(cat => cat.label)
                .ToList();
        }

        private void RefreshFilteredNodes()
        {
            if (TargetNetwork?.NetworkNodes == null)
            {
                filteredNodes.Clear();
                return;
            }

            // Reset animations for old nodes
            ExpanseAnimations.ResetGroup(windowID + "_node_");

            filteredNodes = TargetNetwork.NetworkNodes.Where(node =>
            {
                if (!string.IsNullOrEmpty(searchText) &&
                    !node.parent.LabelShort.ToLower().Contains(searchText.ToLower()) &&
                    !node.NodeLabel.ToLower().Contains(searchText.ToLower()))
                {
                    return false;
                }

                if (!activeFilters.Contains(NetworkFilterType.All))
                {
                    bool matchesLogicalFilter = false;

                    if (activeFilters.Contains(NetworkFilterType.Servers) && IsServerNode(node))
                        matchesLogicalFilter = true;
                    if (activeFilters.Contains(NetworkFilterType.Clients) && !IsServerNode(node))
                        matchesLogicalFilter = true;
                    if (activeFilters.Contains(NetworkFilterType.Online) && node.IsConnectedToServer(out _))
                        matchesLogicalFilter = true;
                    if (activeFilters.Contains(NetworkFilterType.Offline) && !node.IsConnectedToServer(out _))
                        matchesLogicalFilter = true;

                    if (!matchesLogicalFilter)
                        return false;
                }

                if (activeCategoryFilters.Count > 0)
                {
                    if (!activeCategoryFilters.Contains(node.parent.def.designationCategory))
                        return false;
                }

                return true;
            }).ToList();

            filteredNodes = filteredNodes.OrderBy(n => !IsServerNode(n))
                                       .ThenBy(n => n.parent.LabelShort)
                                       .ToList();
        }

        private bool IsServerNode(Comp_NetworkNode node)
        {
            return node.parent.TryGetComp<Comp_NetworkServer>() != null;
        }

        private void SubscribeToNetworkEvents()
        {
            if (TargetNetwork != null)
            {
                TargetNetwork.NetworkMessageSent += OnNetworkEvent;
                TargetNetwork.NetworkMessageReceived += OnNetworkEvent;
                TargetNetwork.NodeStatusChanged += OnNodeStatusChanged;
            }
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (TargetNetwork != null)
            {
                TargetNetwork.NetworkMessageSent -= OnNetworkEvent;
                TargetNetwork.NetworkMessageReceived -= OnNetworkEvent;
                TargetNetwork.NodeStatusChanged -= OnNodeStatusChanged;
            }
        }

        private void OnNetworkEvent(object sender, NetworkMessageEventArgs e)
        {
            RefreshFilteredNodes();
        }

        private void OnNodeStatusChanged(object sender, NetworkStatusEventArgs e)
        {
            RefreshFilteredNodes();
            RefreshAvailableCategories();
        }

        public override void PreClose()
        {
            base.PreClose();
            UnsubscribeFromNetworkEvents();
            // Clean up animations for this window
            ExpanseAnimations.ResetGroup(windowID);
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