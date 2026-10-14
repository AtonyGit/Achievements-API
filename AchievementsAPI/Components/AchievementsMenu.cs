using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AchievementsAPI.API;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AchievementsAPI.MenuUi;

namespace AchievementsAPI
{
    [RegisterInIl2Cpp]
    public class AchievementsMenu(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public MainMenuManager mainMenuManager;
        public OptionsMenuBehaviour OptionsMenuBehaviour;
        public string FocusAchievementId;

        private static readonly Color32 SelectedColor = AccentHover;
        private static readonly Color32 HoverColor = new(200, 200, 200, 255);
        private static readonly Color32 PressedTint = new(150, 150, 150, 255);

        private RectTransform _panel = null!;
        private TextMeshProUGUI _title = null!;
        private Image _titleIcon = null!;
        private RectTransform _statsCard = null!;
        private RectTransform _filterRow = null!;
        private ScrollRect _scroll = null!;
        private RectTransform _listContent = null!;
        private GameObject _listScrollbar = null!;
        private RectTransform _categoryContent;
        private const int CategoryPageSize = 7;
        private RectTransform _categoryViewport;
        private GameObject _categoryPager;
        private Button _categoryPrevious;
        private Button _categoryNext;
        private TextMeshProUGUI _categoryPageLabel;
        private int _categoryPage;
        private GameObject _settingsPanel;
        private GameObject _detailPanel;
        private List<AchievementsTab> _selectableTabs = new();
        private List<AchievementCategory> _categories = new();
        private AchievementCategory _currentCategory;   // null = All
        private int _filter;                    // 0 All, 1 Completed, 2 In Progress, 3 Locked
        private readonly List<(Button button, TextMeshProUGUI label)> _filterTabs = new();
        private readonly List<(AchievementCategory category, Button button)> _categoryRows = new();

        private void Start()
        {
            _selectableTabs = AchievementsManager.Tabs.Where(t => t.IsSelectable).ToList();
            foreach (var tab in _selectableTabs) AchievementStorage.AchievementStorageGet(tab);

            // Only categories that actually contain something, in declared order.
            _categories = _selectableTabs
                .SelectMany(tab => tab.GetAchievements().Select(a => AchievementsManager.ResolveCategory(tab, a)))
                .Distinct()
                .OrderBy(c => c.Order)
                .ThenBy(c => AchievementsManager.Categories.IndexOf(c))
                .ToList();

            var focused = FocusAchievementId != null ? AchievementsManager.FindAchievement(FocusAchievementId) : null;
            _currentCategory = _categories.Count == 1 ? _categories[0]
                : focused is { } f && f.Tab.IsSelectable ? AchievementsManager.ResolveCategory(f.Tab, f.Achievement) : null;

            BuildChrome();
            _settingsPanel = AchievementsSettingsPanel.Create(_panel, BackToList);
            RebuildStatsCard();
            RebuildCategories();
            RebuildList();
            SetInitialTitle();

            if (mainMenuManager)
            {
                mainMenuManager.DeactivateMainMenuUI();
            }
            else if (OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(false);
                OptionsMenuBehaviour.Background.enabled = false;
            }

            if (focused != null)
            {
                ShowDetail(focused.Value.Achievement);
            }
        }

        // Scrim, panel, close button, title, filter tabs, left column and the
        // achievement list scroll. Everything below the panel is anchor-based.
        private void BuildChrome()
        {
            var scrim = CreateImage(transform, "Scrim", null, new Color(0f, 0f, 0f, 0f));
            var scrimRect = scrim.rectTransform;
            scrimRect.anchorMin = Vector2.zero;
            scrimRect.anchorMax = Vector2.one;
            scrimRect.offsetMin = Vector2.zero;
            scrimRect.offsetMax = Vector2.zero;

            var panelImg = CreateImage(transform, "Panel", ToastSprites.WikiPanel, Color.white);
            panelImg.type = Image.Type.Sliced;
            panelImg.color = new Color(0.68f, 0.68f, 0.68f, 1f);
            _panel = panelImg.rectTransform;
            _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0.5f);
            _panel.sizeDelta = new Vector2(1560f, 960f);
            _panel.localScale = new Vector3(0.92f, 0.92f, 1f);
            var group = _panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            Coroutines.Start(CoPopIn(_panel, group, scrim));

            // Top-left X like the game's options window
            var closeImg = CreateImage(_panel, "Close", ToastSprites.CloseX, Color.white);
            closeImg.sprite = CloseButtonSprite ?? ToastSprites.CloseX;
            var closeRect = closeImg.rectTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-14f, -48f);
            closeRect.sizeDelta = new Vector2(64f, 64f);
            var close = closeImg.gameObject.AddComponent<Button>();
            close.transition = Selectable.Transition.ColorTint;
            var closeColors = close.colors;
            closeColors.normalColor = Color.white;
            closeColors.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
            closeColors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            closeColors.selectedColor = Color.white;
            close.colors = closeColors;
            close.onClick.AddListener((UnityAction)Close);
            AttachGameSounds(closeImg.gameObject);

            // Wiki-style title: page icon left, title text next to it
            _titleIcon = CreateImage(_panel, "TitleIcon", null, Color.white);
            _titleIcon.preserveAspect = true;
            _titleIcon.raycastTarget = false;
            _titleIcon.enabled = false;
            var iconRect = _titleIcon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 1f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(108f, -70f);
            iconRect.sizeDelta = new Vector2(38f, 38f);

            _title = CreateText(_panel, "Title", "Achievements", 38f, Color.white,
                FontStyles.Bold, TextAlignmentOptions.Left);
            _title.overflowMode = TextOverflowModes.Overflow;
            var titleRect = _title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(134f, -44f);
            titleRect.sizeDelta = new Vector2(900f, 52f);

            // Filter pills centered over the right column (x 430..1436 -> 183)
            var rowGo = new GameObject("FilterRow");
            _filterRow = rowGo.AddComponent<RectTransform>();
            _filterRow.SetParent(_panel, false);
            _filterRow.anchorMin = _filterRow.anchorMax = new Vector2(0.5f, 1f);
            _filterRow.pivot = new Vector2(0.5f, 0.5f);
            _filterRow.anchoredPosition = new Vector2(183f, -70f);
            _filterRow.sizeDelta = new Vector2(836f, 50f);
            var names = new[] { "All", "Completed", "In Progress", "Locked" };
            for (var i = 0; i < names.Length; i++)
            {
                var pill = CreateImage(_filterRow, names[i], ToastSprites.ButtonBox, Color.white);
                pill.type = Image.Type.Sliced;
                var pillRect = pill.rectTransform;
                pillRect.anchorMin = pillRect.anchorMax = new Vector2(0.5f, 0.5f);
                pillRect.pivot = new Vector2(0.5f, 0.5f);
                pillRect.anchoredPosition = new Vector2((i - 1.5f) * 212f, 0f);
                pillRect.sizeDelta = new Vector2(200f, 50f);
                var label = CreateText(pillRect, "Label", names[i], 22f, Color.white,
                    FontStyles.Bold, TextAlignmentOptions.Center);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                var button = pill.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = HoverColor;
                colors.pressedColor = PressedTint;
                colors.selectedColor = Color.white;
                colors.disabledColor = SelectedColor;
                button.colors = colors;
                var index = i;
                button.onClick.AddListener((UnityAction)(() => SetFilter(index)));
                AttachGameSounds(pill.gameObject);
                _filterTabs.Add((button, label));
            }

            var leftGo = new GameObject("LeftColumn");
            var left = leftGo.AddComponent<RectTransform>();
            left.SetParent(_panel, false);
            left.anchorMin = new Vector2(0f, 0f);
            left.anchorMax = new Vector2(0f, 1f);
            left.offsetMin = new Vector2(60f, 60f);
            left.offsetMax = new Vector2(420f, -126f);

            var stats = CreateImage(left, "StatsCard", ToastSprites.ButtonBox, Color.white);
            stats.type = Image.Type.Sliced;
            stats.raycastTarget = false;
            _statsCard = stats.rectTransform;
            _statsCard.anchorMin = new Vector2(0f, 1f);
            _statsCard.anchorMax = new Vector2(1f, 1f);
            _statsCard.offsetMin = new Vector2(0f, -220f);
            _statsCard.offsetMax = new Vector2(0f, 0f);

            if (_categories.Count >= 2)
            {
                var catGo = new GameObject("CategoryScroll");
                _categoryViewport = catGo.AddComponent<RectTransform>();
                _categoryViewport.SetParent(left, false);
                _categoryViewport.anchorMin = Vector2.zero;
                _categoryViewport.anchorMax = Vector2.one;
                _categoryViewport.offsetMin = new Vector2(0f, 68f);
                _categoryViewport.offsetMax = new Vector2(-22f, -236f);
                var catScroll = catGo.AddComponent<ScrollRect>();
                catGo.AddComponent<RectMask2D>();
                var catRaycast = CreateImage(_categoryViewport, "Raycast", null, new Color(0f, 0f, 0f, 0f));
                catRaycast.rectTransform.anchorMin = Vector2.zero;
                catRaycast.rectTransform.anchorMax = Vector2.one;
                catRaycast.rectTransform.offsetMin = Vector2.zero;
                catRaycast.rectTransform.offsetMax = Vector2.zero;

                var catContentGo = new GameObject("Content");
                _categoryContent = catContentGo.AddComponent<RectTransform>();
                _categoryContent.SetParent(_categoryViewport, false);
                _categoryContent.anchorMin = new Vector2(0f, 1f);
                _categoryContent.anchorMax = new Vector2(1f, 1f);
                _categoryContent.pivot = new Vector2(0.5f, 1f);
                _categoryContent.offsetMin = Vector2.zero;
                _categoryContent.offsetMax = Vector2.zero;
                var catVlg = catContentGo.AddComponent<VerticalLayoutGroup>();
                catVlg.spacing = 8f;
                catVlg.childControlWidth = true;
                catVlg.childControlHeight = true;
                catVlg.childForceExpandWidth = true;
                catVlg.childForceExpandHeight = false;
                var catFitter = catContentGo.AddComponent<ContentSizeFitter>();
                catFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                catScroll.viewport = _categoryViewport;
                catScroll.content = _categoryContent;
                catScroll.horizontal = false;
                catScroll.vertical = false;
                catScroll.movementType = ScrollRect.MovementType.Clamped;
                catScroll.scrollSensitivity = 56f;

                _categoryPager = new GameObject("CategoryPager");
                var pagerRect = _categoryPager.AddComponent<RectTransform>();
                pagerRect.SetParent(left, false);
                pagerRect.anchorMin = new Vector2(0f, 0f);
                pagerRect.anchorMax = new Vector2(1f, 0f);
                pagerRect.offsetMin = new Vector2(0f, 64f);
                pagerRect.offsetMax = new Vector2(0f, 108f);

                _categoryPrevious = CreatePagerArrow(pagerRect, "Previous", "<", new Vector2(-58f, 0f));
                _categoryPrevious.onClick.AddListener((UnityAction)(() => ChangeCategoryPage(-1)));
                _categoryNext = CreatePagerArrow(pagerRect, "Next", ">", new Vector2(58f, 0f));
                _categoryNext.onClick.AddListener((UnityAction)(() => ChangeCategoryPage(1)));

                _categoryPageLabel = CreateText(pagerRect, "PageLabel", "1 / 1", 18f, ValueColor,
                    FontStyles.Normal, TextAlignmentOptions.Center);
                var pageLabelRect = _categoryPageLabel.rectTransform;
                pageLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
                pageLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
                pageLabelRect.pivot = new Vector2(0.5f, 0.5f);
                pageLabelRect.anchoredPosition = Vector2.zero;
                pageLabelRect.sizeDelta = new Vector2(70f, 40f);

                _categoryPager.SetActive(false);
            }

            var settingsImg = CreateImage(left, "SettingsButton", ToastSprites.ButtonBox, Color.white);
            settingsImg.type = Image.Type.Sliced;
            var settingsRect = settingsImg.rectTransform;
            settingsRect.anchorMin = new Vector2(0f, 0f);
            settingsRect.anchorMax = new Vector2(1f, 0f);
            settingsRect.offsetMin = Vector2.zero;
            settingsRect.offsetMax = new Vector2(0f, 52f);
            var gear = CreateImage(settingsRect, "Gear", ToastSprites.Gear, Color.white);
            gear.raycastTarget = false;
            var gearRect = gear.rectTransform;
            gearRect.anchorMin = gearRect.anchorMax = new Vector2(0f, 0.5f);
            gearRect.pivot = new Vector2(0.5f, 0.5f);
            gearRect.anchoredPosition = new Vector2(36f, 0f);
            gearRect.sizeDelta = new Vector2(28f, 28f);
            var settingsLabel = CreateText(settingsRect, "Label", "Settings", 22f, Color.white,
                FontStyles.Bold, TextAlignmentOptions.Center);
            settingsLabel.rectTransform.anchorMin = Vector2.zero;
            settingsLabel.rectTransform.anchorMax = Vector2.one;
            settingsLabel.rectTransform.offsetMin = new Vector2(34f, 0f);
            settingsLabel.rectTransform.offsetMax = Vector2.zero;
            var settingsButton = settingsImg.gameObject.AddComponent<Button>();
            settingsButton.transition = Selectable.Transition.ColorTint;
            var settingsColors = settingsButton.colors;
            settingsColors.normalColor = Color.white;
            settingsColors.highlightedColor = HoverColor;
            settingsColors.pressedColor = PressedTint;
            settingsColors.selectedColor = Color.white;
            settingsButton.colors = settingsColors;
            settingsButton.onClick.AddListener((UnityAction)ShowSettingsPage);
            AttachGameSounds(settingsImg.gameObject);

            var scrollGo = new GameObject("ListScroll");
            var scrollRect = scrollGo.AddComponent<RectTransform>();
            scrollRect.SetParent(_panel, false);
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(450f, 60f);
            scrollRect.offsetMax = new Vector2(-84f, -126f);
            _scroll = scrollGo.AddComponent<ScrollRect>();
            scrollGo.AddComponent<RectMask2D>();
            var scrollRaycast = CreateImage(scrollRect, "Raycast", null, new Color(0f, 0f, 0f, 0f));
            scrollRaycast.rectTransform.anchorMin = Vector2.zero;
            scrollRaycast.rectTransform.anchorMax = Vector2.one;
            scrollRaycast.rectTransform.offsetMin = Vector2.zero;
            scrollRaycast.rectTransform.offsetMax = Vector2.zero;

            var contentGo = new GameObject("Content");
            _listContent = contentGo.AddComponent<RectTransform>();
            _listContent.SetParent(scrollRect, false);
            _listContent.anchorMin = new Vector2(0f, 1f);
            _listContent.anchorMax = new Vector2(1f, 1f);
            _listContent.pivot = new Vector2(0.5f, 1f);
            _listContent.offsetMin = Vector2.zero;
            _listContent.offsetMax = Vector2.zero;
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _scroll.viewport = scrollRect;
            _scroll.content = _listContent;
            _scroll.horizontal = false;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 100f;
            _listScrollbar = CreateScrollbar(_panel, _scroll, new Vector2(-76f, 64f), new Vector2(-62f, -122f));
        }

        // Wiki-scroller-style vertical scrollbar: dark translucent track with a
        // light handle that brightens on hover. ScrollRect drives the handle;
        // it auto-hides when the content fits the viewport.
        private static GameObject CreateScrollbar(Transform parent, ScrollRect scroll,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var track = CreateImage(parent, "Scrollbar", ToastSprites.RoundedRectTiny,
                new Color(0f, 0f, 0f, 0.35f));
            track.type = Image.Type.Sliced;
            track.raycastTarget = false;
            var trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(1f, 0f);
            trackRect.anchorMax = new Vector2(1f, 1f);
            trackRect.offsetMin = offsetMin;
            trackRect.offsetMax = offsetMax;

            var slideGo = new GameObject("Sliding Area");
            var slideRect = slideGo.AddComponent<RectTransform>();
            slideRect.SetParent(trackRect, false);
            slideRect.anchorMin = Vector2.zero;
            slideRect.anchorMax = Vector2.one;
            slideRect.offsetMin = new Vector2(2f, 3f);
            slideRect.offsetMax = new Vector2(-2f, -3f);

            var handle = CreateImage(slideRect, "Handle", ToastSprites.RoundedRectTiny, Color.white);
            handle.type = Image.Type.Sliced;
            var handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(1f, 0f);
            handleRect.pivot = new Vector2(0.5f, 0f);
            handleRect.sizeDelta = new Vector2(0f, 60f);

            // UnityEngine.UI.Scrollbar: a plain `Scrollbar` here resolves to
            // the game's own scrollbar class
            var bar = track.gameObject.AddComponent<UnityEngine.UI.Scrollbar>();
            bar.handleRect = handleRect;
            bar.targetGraphic = handle;
            bar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
            bar.transition = Selectable.Transition.ColorTint;
            var colors = bar.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.75f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = AccentHover;
            colors.selectedColor = new Color(1f, 1f, 1f, 0.75f);
            colors.fadeDuration = 0.1f;
            bar.colors = colors;

            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            return track.gameObject;
        }

        [HideFromIl2Cpp]
        private Button CreatePagerArrow(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var img = CreateImage(parent, name, ToastSprites.ButtonBox, Color.white);
            img.type = Image.Type.Sliced;
            var rect = img.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(44f, 40f);

            var text = CreateText(rect, "Label", label, 22f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor = PressedTint;
            colors.selectedColor = Color.white;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            AttachGameSounds(img.gameObject);
            return button;
        }

        [HideFromIl2Cpp]
        private IEnumerable<(AchievementsTab Tab, BaseAchievement Achievement)> EnumerateAchievements()
        {
            foreach (var tab in _selectableTabs)
            {
                foreach (var a in tab.GetAchievements())
                {
                    if (_currentCategory == null || AchievementsManager.ResolveCategory(tab, a) == _currentCategory)
                        yield return (tab, a);
                }
            }
        }

        private static bool InProgress(BaseAchievement a)
            => a is CountAchievement c && !a.Unlocked && c.CurrentValue > 0 && !(c.HideProgress && c.Hidden);

        private static bool PassesFilter(BaseAchievement a, int filter) => filter switch
        {
            1 => a.Unlocked,
            2 => InProgress(a),
            3 => !a.Unlocked,
            _ => true
        };

        private static int SortGroup(BaseAchievement a) => a.Unlocked ? 0 : InProgress(a) ? 1 : 2;

        private static float Fraction(BaseAchievement a)
            => a is CountAchievement c && c.RequiredValue > 0
                ? Mathf.Clamp01((float) c.CurrentValue / c.RequiredValue) : 0f;

        private void RebuildStatsCard()
        {
            ClearChildren(_statsCard);
            var all = EnumerateAchievements().Select(x => x.Achievement).ToList();
            var earned = all.Where(a => a.Unlocked).Sum(a => a.Points);
            var total = all.Sum(a => a.Points);
            var done = all.Count(a => a.Unlocked);

            // Row 1: gold star, "Skeld Score" caption over the big earned/total
            var emblem = CreateScoreEmblem(_statsCard, 64f);
            emblem.anchorMin = emblem.anchorMax = new Vector2(0f, 1f);
            emblem.pivot = new Vector2(0f, 1f);
            emblem.anchoredPosition = new Vector2(16f, -28f);

            var cap = CreateText(_statsCard, "PointsLabel", "Skeld Score", 17f, ValueColor, FontStyles.Bold);
            cap.overflowMode = TextOverflowModes.Overflow;
            var capRect = cap.rectTransform;
            capRect.anchorMin = new Vector2(0f, 1f);
            capRect.anchorMax = new Vector2(1f, 1f);
            capRect.offsetMin = new Vector2(94f, -44f);
            capRect.offsetMax = new Vector2(-16f, -20f);

            // Two texts bottom-aligned in a layout row so the small "/ total"
            // sits on the big number's baseline instead of floating mid-line
            var pointsRowGo = new GameObject("Points");
            var pointsRow = pointsRowGo.AddComponent<RectTransform>();
            pointsRow.SetParent(_statsCard, false);
            pointsRow.anchorMin = new Vector2(0f, 1f);
            pointsRow.anchorMax = new Vector2(1f, 1f);
            pointsRow.offsetMin = new Vector2(94f, -92f);
            pointsRow.offsetMax = new Vector2(-16f, -44f);
            var pointsHlg = pointsRowGo.AddComponent<HorizontalLayoutGroup>();
            pointsHlg.spacing = 8f;
            pointsHlg.childAlignment = TextAnchor.LowerLeft;
            pointsHlg.childControlWidth = true;
            pointsHlg.childControlHeight = true;
            pointsHlg.childForceExpandWidth = false;
            pointsHlg.childForceExpandHeight = false;
            // TMP's Ellipsis mode drops a single line taller than its rect,
            // which hid the number entirely; Overflow keeps it visible
            var earnedText = CreateText(pointsRow, "Earned", earned.ToString(), 36f, Color.white,
                FontStyles.Bold, TextAlignmentOptions.BottomLeft);
            earnedText.overflowMode = TextOverflowModes.Overflow;
            var totalText = CreateText(pointsRow, "Total", $"/ {total}", 22f, ValueColor,
                FontStyles.Normal, TextAlignmentOptions.BottomLeft);
            totalText.overflowMode = TextOverflowModes.Overflow;
            totalText.margin = new Vector4(0f, 0f, 0f, 6f);

            // Row 2: completed count with an animated bar
            var completed = CreateText(_statsCard, "Completed", "Completed", 20f, Color.white);
            var completedRect = completed.rectTransform;
            completedRect.anchorMin = new Vector2(0f, 1f);
            completedRect.anchorMax = new Vector2(1f, 1f);
            completedRect.offsetMin = new Vector2(20f, -130f);
            completedRect.offsetMax = new Vector2(-20f, -104f);

            var completedValue = CreateText(_statsCard, "CompletedValue", $"{done}/{all.Count}",
                20f, ValueColor, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
            var cvRect = completedValue.rectTransform;
            cvRect.anchorMin = new Vector2(0f, 1f);
            cvRect.anchorMax = new Vector2(1f, 1f);
            cvRect.offsetMin = new Vector2(20f, -130f);
            cvRect.offsetMax = new Vector2(-20f, -104f);

            var bar = CreateProgressBar(_statsCard, AccentHover, out var fill);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.offsetMin = new Vector2(20f, -144f);
            bar.offsetMax = new Vector2(-20f, -134f);
            Coroutines.Start(CoFill(fill, all.Count > 0 ? (float) done / all.Count : 0f, 0f, 0.5f));

            // Row 3: per-rarity completion as a 2x2 grid of labeled chips
            // (legendary, epic, rare, common; rarities with none are skipped)
            var chips = new List<(int rarity, int done, int total)>();
            for (var rarity = 3; rarity >= 0; rarity--)
            {
                var items = all.Where(a => a.Rarity == rarity).ToList();
                if (items.Count == 0) continue;
                chips.Add((rarity, items.Count(a => a.Unlocked), items.Count));
            }

            for (var i = 0; i < chips.Count; i++)
            {
                var x = 20f + (i % 2) * 170f;
                var chipY = -168f - (i / 2) * 30f;
                var rarityColor = chips[i].rarity == 0 ? (Color) new Color32(230, 230, 230, 255) : RarityColor(chips[i].rarity);
                var hex = ColorUtility.ToHtmlStringRGB(rarityColor);
                var chip = CreateText(_statsCard, "RarityChip",
                    $"<b><color=#{hex}>{RarityName(chips[i].rarity)}</color></b> <color=#BEBEBE>{chips[i].done}/{chips[i].total}</color>",
                    18f, Color.white);
                chip.overflowMode = TextOverflowModes.Overflow;
                var chipRect = chip.rectTransform;
                chipRect.anchorMin = chipRect.anchorMax = new Vector2(0f, 1f);
                chipRect.pivot = new Vector2(0f, 0.5f);
                chipRect.anchoredPosition = new Vector2(x, chipY);
                chipRect.sizeDelta = new Vector2(166f, 24f);
            }
        }

        private void RebuildCategories()
        {
            if (_categoryContent == null) return;
            ClearChildren(_categoryContent);
            _categoryRows.Clear();

            var totalEntries = _categories.Count + 1;
            var pageCount = Mathf.Max(1, Mathf.CeilToInt(totalEntries / (float)CategoryPageSize));
            _categoryPage = Mathf.Clamp(_categoryPage, 0, pageCount - 1);

            var pagerVisible = pageCount > 1;
            if (_categoryPager != null) _categoryPager.SetActive(pagerVisible);
            if (_categoryViewport != null)
            {
                _categoryViewport.offsetMin = new Vector2(0f, pagerVisible ? 120f : 68f);
            }

            var startIndex = _categoryPage * CategoryPageSize;
            var endIndex = Mathf.Min(startIndex + CategoryPageSize, totalEntries);
            for (var i = startIndex; i < endIndex; i++)
            {
                AddCategoryRow(i == 0 ? null : _categories[i - 1]);
            }

            if (_categoryPageLabel != null) _categoryPageLabel.text = $"{_categoryPage + 1} / {pageCount}";
            if (_categoryPrevious != null) _categoryPrevious.interactable = _categoryPage > 0;
            if (_categoryNext != null) _categoryNext.interactable = _categoryPage < pageCount - 1;

            UpdateCategorySelection();
        }

        private void ChangeCategoryPage(int delta)
        {
            _categoryPage += delta;
            RebuildCategories();
        }

        [HideFromIl2Cpp]
        private void ShowCategoryPage(AchievementCategory category)
        {
            var index = category == null ? 0 : _categories.IndexOf(category) + 1;
            if (index < 0) return;
            _categoryPage = index / CategoryPageSize;
            RebuildCategories();
        }

        [HideFromIl2Cpp]
        private void AddCategoryRow(AchievementCategory category)
        {
            var name = category?.Name ?? "All";
            var img = CreateImage(_categoryContent!, name, ToastSprites.ButtonBox, Color.white);
            img.type = Image.Type.Sliced;
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 52f;
            var rect = img.rectTransform;

            var icon = CreateImage(rect, "Icon", category?.GetIcon() ?? AchievementIcon, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(30f, 0f);
            iconRect.sizeDelta = new Vector2(32f, 32f);
            var textX = 56f;

            var label = CreateText(rect, "Label", name, 20f, Color.white, FontStyles.Bold);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(textX, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);

            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = HoverColor;
            colors.pressedColor = PressedTint;
            colors.selectedColor = Color.white;
            colors.disabledColor = SelectedColor;
            button.colors = colors;
            button.onClick.AddListener((UnityAction)(() => SelectCategory(category)));
            AttachGameSounds(img.gameObject);
            _categoryRows.Add((category, button));
        }

        private void UpdateCategorySelection()
        {
            foreach (var (category, button) in _categoryRows)
            {
                button.interactable = category != _currentCategory;
            }
        }

        [HideFromIl2Cpp]
        private void SelectCategory(AchievementCategory category)
        {
            _currentCategory = category;
            BackToList();
            UpdateCategorySelection();
            RebuildStatsCard();
            RebuildList();
        }

        private void SetInitialTitle()
        {
            SetTitle(_currentCategory?.Name ?? "Achievements", _currentCategory?.GetIcon() ?? AchievementIcon);
        }

        private void SetFilter(int filter)
        {
            _filter = filter;
            for (var i = 0; i < _filterTabs.Count; i++)
            {
                _filterTabs[i].button.interactable = i != filter;
            }
            RebuildList();
        }

        private void RebuildList()
        {
            ClearChildren(_listContent);

            var counts = EnumerateAchievements().Select(x => x.Achievement).ToList();
            var filterCounts = new[]
            {
                counts.Count,
                counts.Count(a => a.Unlocked),
                counts.Count(InProgress),
                counts.Count(a => !a.Unlocked)
            };
            var names = new[] { "All", "Completed", "In Progress", "Locked" };
            for (var i = 0; i < _filterTabs.Count; i++)
            {
                _filterTabs[i].label.text =
                    $"{names[i]} <color=#BEBEBE>({filterCounts[i]})</color>";
                _filterTabs[i].button.interactable = i != _filter;
            }

            var filter = _filter;
            // Grouped by the mod that owns each achievement; headers only when the
            // current view mixes several mods.
            var allGroups = EnumerateAchievements()
                .GroupBy(x => x.Tab.ModName)
                .ToList();
            var groups = allGroups
                .Select(g => (tab: g.First().Tab, items: g.Where(x => PassesFilter(x.Achievement, filter))
                    .Select(x => x.Achievement).ToList()))
                .Where(g => g.items.Count > 0)
                .ToList();

            var index = 0;
            var any = false;
            var showHeaders = allGroups.Count >= 2;
            foreach (var (tab, items) in groups)
            {
                items.Sort(CompareRows);
                if (showHeaders) AddGroupHeader(tab);
                foreach (var achievement in items)
                {
                    BuildRow(achievement, index++);
                    any = true;
                }
            }

            if (!any)
            {
                var go = new GameObject("Empty");
                var rect = go.AddComponent<RectTransform>();
                rect.SetParent(_listContent, false);
                var le = go.AddComponent<LayoutElement>();
                le.preferredHeight = 300f;
                var text = CreateText(rect, "Text",
                    _filter == 3 ? "Everything unlocked!" : "Nothing here yet",
                    26f, ValueColor, FontStyles.Normal, TextAlignmentOptions.Center);
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = Vector2.zero;
                text.rectTransform.offsetMax = Vector2.zero;
            }

            _scroll.verticalNormalizedPosition = 1f;
        }

        // Unlocked first (most recent first), then in-progress by fraction, then
        // locked; hidden achievements last.
        private static int CompareRows(BaseAchievement x, BaseAchievement y)
        {
            var c = (x.Hidden && !x.Unlocked ? 1 : 0).CompareTo(y.Hidden && !y.Unlocked ? 1 : 0);
            if (c != 0) return c;
            c = SortGroup(x).CompareTo(SortGroup(y));
            if (c != 0) return c;
            if (x.Unlocked && y.Unlocked)
            {
                return (y.UnlockedAt ?? DateTime.MinValue).CompareTo(x.UnlockedAt ?? DateTime.MinValue);
            }
            return Fraction(y).CompareTo(Fraction(x));
        }

        [HideFromIl2Cpp]
        private void AddGroupHeader(AchievementsTab tab)
        {
            var go = new GameObject($"Header_{tab.ModName}");
            var rect = go.AddComponent<RectTransform>();
            rect.SetParent(_listContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;

            var icon = CreateImage(rect, "Icon", tab.ModIcon, Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);

            var name = CreateText(rect, "Name", tab.ModName, 20f, ValueColor, FontStyles.Bold);
            var nameRect = name.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(44f, 0f);
            nameRect.offsetMax = Vector2.zero;
        }

        [HideFromIl2Cpp]
        private void BuildRow(BaseAchievement achievement, int index)
        {
            var revealed = !achievement.Hidden || achievement.Unlocked;
            var showRarity = revealed || !achievement.HideRarity;
            var card = CreateRarityCard(_listContent, achievement.Name, showRarity ? achievement.Rarity : 0);
            var le = card.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 128f;
            var row = card.rectTransform;

            var disc = CreateRarityDisc(row, "Disc", 76f,
                showRarity ? RarityColor(achievement.Rarity) : RimColor, out var inner);
            disc.anchorMin = disc.anchorMax = new Vector2(0f, 0.5f);
            disc.pivot = new Vector2(0.5f, 0.5f);
            disc.anchoredPosition = new Vector2(56f, 0f);

            var iconSprite = revealed ? achievement.Icon
                : SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png",
                    typeof(AchievementsMenu).Assembly, 100);
            var icon = CreateImage(inner, "Icon", ToastSprites.TrimTransparent(iconSprite),
                achievement.Unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f));
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(54f, 54f);

            if (achievement.MenuSubIcon != null)
            {
                var sub = CreateImage(disc, "SubIcon", achievement.MenuSubIcon, Color.white);
                sub.raycastTarget = false;
                var subRect = sub.rectTransform;
                subRect.anchorMin = subRect.anchorMax = new Vector2(1f, 0f);
                subRect.pivot = new Vector2(0.5f, 0.5f);
                subRect.anchoredPosition = new Vector2(-2f, 2f);
                subRect.sizeDelta = new Vector2(36f, 36f);
                subRect.localScale = achievement.MenuSubIconScale;
            }

            var name = CreateText(row, "Name", revealed ? achievement.Name : "Hidden Achievement",
                26f, achievement.Unlocked ? Color.white : new Color(0.56f, 0.56f, 0.56f), FontStyles.Bold);
            name.overflowMode = TextOverflowModes.Overflow;
            var nameRect = name.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(112f, -52f);
            nameRect.offsetMax = new Vector2(-180f, -22f);

            var desc = CreateText(row, "Description",
                revealed ? achievement.Description : "Hidden Achievement", 19f,
                achievement.Unlocked ? ValueColor : new Color(0.42f, 0.42f, 0.42f));
            desc.enableWordWrapping = false;
            var descRect = desc.rectTransform;
            descRect.anchorMin = new Vector2(0f, 1f);
            descRect.anchorMax = new Vector2(1f, 1f);
            descRect.offsetMin = new Vector2(112f, -80f);
            descRect.offsetMax = new Vector2(-180f, -56f);

            var bar = CreateProgressBar(row, showRarity ? ProgressColor(achievement.Rarity) : RimColor,
                out var fill);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.offsetMin = new Vector2(112f, 30f);
            bar.offsetMax = new Vector2(-180f, 40f);

            float target;
            string progressLabel = null;
            var progressLabelColor = (Color) ValueColor;
            if (achievement is CountAchievement count && count.RequiredValue > 0)
            {
                var hiddenProgress = count.HideProgress && count.Hidden && !achievement.Unlocked;
                target = hiddenProgress ? 0f : Mathf.Clamp01((float) count.CurrentValue / count.RequiredValue);
                if (!hiddenProgress)
                {
                    progressLabel = $"{Percent(count.CurrentValue, count.RequiredValue)}%  {count.CurrentValue}/{count.RequiredValue}";
                }
            }
            else
            {
                target = achievement.Unlocked ? 1f : 0f;
                if (achievement.Unlocked)
                {
                    progressLabel = "100%";
                    progressLabelColor = AccentHover;
                }
            }
            Coroutines.Start(CoFill(fill, target, index * 0.03f));

            if (progressLabel != null)
            {
                var label = CreateText(row, "ProgressLabel", progressLabel, 16f, progressLabelColor,
                    FontStyles.Normal, TextAlignmentOptions.MidlineRight);
                var labelRect = label.rectTransform;
                labelRect.anchorMin = labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(1f, 0f);
                labelRect.anchoredPosition = new Vector2(-180f, 42f);
                labelRect.sizeDelta = new Vector2(200f, 20f);
            }

            var pill = CreateImage(row, "PointsPill", ToastSprites.ButtonBox,
                achievement.Unlocked ? RarityColor(achievement.Rarity) : new Color(0.55f, 0.55f, 0.55f));
            pill.type = Image.Type.Sliced;
            pill.raycastTarget = false;
            var pillRect = pill.rectTransform;
            pillRect.anchorMin = pillRect.anchorMax = new Vector2(1f, 0.5f);
            pillRect.pivot = new Vector2(1f, 0.5f);
            pillRect.anchoredPosition = new Vector2(-22f, 0f);
            pillRect.sizeDelta = new Vector2(96f, 44f);

            var pillEmblem = CreateScoreEmblem(pillRect, 22f);
            pillEmblem.anchorMin = pillEmblem.anchorMax = new Vector2(0f, 0.5f);
            pillEmblem.pivot = new Vector2(0.5f, 0.5f);
            pillEmblem.anchoredPosition = new Vector2(20f, 0f);

            var pillText = CreateText(pillRect, "Label", $"{achievement.Points}", 22f,
                achievement.Unlocked ? Color.white : (Color) ValueColor, FontStyles.Bold,
                TextAlignmentOptions.Center);
            var pillTextRect = pillText.rectTransform;
            pillTextRect.anchorMin = Vector2.zero;
            pillTextRect.anchorMax = Vector2.one;
            pillTextRect.offsetMin = new Vector2(34f, 0f);
            pillTextRect.offsetMax = Vector2.zero;

            // Translucent overlay carries the hover tint so the rarity card
            // color stays unchanged; it also gives the ScrollRect a raycast hit.
            var overlay = CreateImage(row, "HoverOverlay", ToastSprites.Card, Color.white);
            overlay.type = Image.Type.Sliced;
            var overlayRect = overlay.rectTransform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlay.raycastTarget = true;

            var button = card.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = overlay;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(0f, 0f, 0f, 0.25f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.4f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener((UnityAction)(() => ShowDetail(achievement)));
            AttachGameSounds(card.gameObject, hoverSound: false);
        }

        // Hides the list and shows `page` over the right column (null = list back).
        private void ShowPage(GameObject page)
        {
            var listsVisible = page == null;
            _scroll.gameObject.SetActive(listsVisible);
            _listScrollbar?.SetActive(listsVisible);
            _filterRow.gameObject.SetActive(listsVisible);
            _settingsPanel?.SetActive(page == _settingsPanel);
            _detailPanel?.SetActive(page == _detailPanel);
            if (page != null)
            {
                MatchRect(page.GetComponent<RectTransform>(), _scroll.GetComponent<RectTransform>());
            }
        }

        // Wiki-style title with the page icon to its left (icon can be null).
        private void SetTitle(string text, Sprite icon)
        {
            _title.text = text;
            if (icon != null)
            {
                _titleIcon.sprite = ToastSprites.TrimTransparent(icon);
                _titleIcon.enabled = true;
                _titleIcon.rectTransform.anchoredPosition = new Vector2(108f, -70f);
                _title.rectTransform.anchoredPosition = new Vector2(154f, -44f);
            }
            else
            {
                _titleIcon.enabled = false;
                _title.rectTransform.anchoredPosition = new Vector2(110f, -44f);
            }
        }

        private void BackToList()
        {
            if (_detailPanel != null)
            {
                _detailPanel.Destroy();
                _detailPanel = null;
            }
            SetTitle(_currentCategory?.Name ?? "Achievements", _currentCategory?.GetIcon() ?? AchievementIcon);
            ShowPage(null);
        }

        private void ShowSettingsPage()
        {
            if (_detailPanel != null)
            {
                _detailPanel.Destroy();
                _detailPanel = null;
            }
            SetTitle("Settings", ToastSprites.Gear);
            ShowPage(_settingsPanel);
        }

        [HideFromIl2Cpp]
        internal void FocusAchievement(string achievementId)
        {
            var found = AchievementsManager.FindAchievement(achievementId);
            if (found == null) return;

            var (tab, achievement) = found.Value;
            var category = AchievementsManager.ResolveCategory(tab, achievement);
            if (tab.IsSelectable && _categories.Contains(category))
            {
                _currentCategory = category;
                ShowCategoryPage(category);
                RebuildStatsCard();
                RebuildList();
            }
            ShowDetail(achievement);
        }

        [HideFromIl2Cpp]
        private void ShowDetail(BaseAchievement achievement)
        {
            if (_detailPanel != null) _detailPanel.Destroy();
            var revealed = !achievement.Hidden || achievement.Unlocked;
            SetTitle(revealed ? achievement.Name : "Hidden Achievement",
                revealed ? achievement.Icon : null);
            _detailPanel = AchievementDetailPanel.Create(_panel, achievement, BackToList);
            ShowPage(_detailPanel);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if ((_detailPanel != null && _detailPanel.activeSelf) ||
                (_settingsPanel != null && _settingsPanel.activeSelf))
            {
                BackToList();
                return;
            }
            Close();
        }

        public void Close()
        {
            if (mainMenuManager)
            {
                mainMenuManager.ActivateMainMenuUI();
            }
            else if (OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(true);
                OptionsMenuBehaviour.Background.enabled = true;
            }
            gameObject.Destroy();
        }

        [HideFromIl2Cpp]
        private IEnumerator CoPopIn(RectTransform rect, CanvasGroup group, Image scrim)
        {
            const float duration = 0.18f;
            for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (rect == null) yield break;
                var k = Mathf.Clamp01(t / duration);
                var s = Mathf.Lerp(0.92f, 1f, EaseOutBack(k));
                rect.localScale = new Vector3(s, s, 1f);
                group.alpha = k;
                scrim.color = new Color(0f, 0f, 0f, 0.5f * k);
                yield return null;
            }
            if (rect == null) yield break;
            rect.localScale = Vector3.one;
            group.alpha = 1f;
            scrim.color = new Color(0f, 0f, 0f, 0.5f);
        }

        [HideFromIl2Cpp]
        private IEnumerator CoFill(RectTransform fill, float target, float delay = 0f, float duration = 0.45f)
        {
            for (var t = -delay; t < duration; t += Time.unscaledDeltaTime)
            {
                if (fill == null) yield break;
                if (t >= 0f) SetProgress(fill, EaseOutCubic(Mathf.Clamp01(t / duration)) * target);
                yield return null;
            }
            if (fill == null) yield break;
            SetProgress(fill, target);
        }

        private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        private static float EaseOutBack(float t)
        {
            const float c = 1.70158f;
            return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
        }
    }
}
