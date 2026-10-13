using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System;
using System.Linq;
using AchievementsAPI.API;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AchievementsAPI
{
    [RegisterInIl2Cpp]
    public class AchievementsMenu(IntPtr ptr) : MonoBehaviour(ptr)
    {
        public Il2CppReferenceField<GameObject> achievementItemPrefab;
        public Il2CppReferenceField<Transform> unlockedContentParent;
        public Il2CppReferenceField<Transform> lockedContentParent;
        public Il2CppReferenceField<Transform> tabsParent;
        public Il2CppReferenceField<GameObject> tabPrefab;
        public Il2CppReferenceField<TextMeshProUGUI> titleText;
        public Il2CppReferenceField<TextMeshProUGUI> percentageText;
        public Il2CppReferenceField<Image> progressBar;
        public MainMenuManager mainMenuManager;
        public OptionsMenuBehaviour OptionsMenuBehaviour;
        public List<AchievementsMenuItem> items = new();
        public string? FocusAchievementId;
        private GameObject? _settingsPanel;
        private GameObject? _detailPanel;
        private AchievementsTab? _defaultTab;
        private void Start()
        {
            foreach (var tab in AchievementsManager.Tabs)
            {
                if (!tab.IsSelectable) continue;
                var go = Instantiate(tabPrefab.Value, tabsParent);
                var btn = go.GetComponent<Button>();
                var sprite = tab.GetIcon();
                if (sprite) go.GetComponent<Image>().sprite = sprite;
                btn.onClick.AddListener(new Action(() =>
                {
                    SetTab(tab);
                    if (mainMenuManager)
                    {
                        mainMenuManager.StartCoroutine(
                            Effects.ActionAfterDelay(0.01f, new System.Action(() => SetTab(tab))));
                    }
                    else if (OptionsMenuBehaviour)
                    {
                        OptionsMenuBehaviour.gameObject.SetActive(true);
                        OptionsMenuBehaviour.StartCoroutine(Effects.ActionAfterDelay(0.01f, new System.Action(() => SetTab(tab, true))));
                    }
                }));
                AchievementStorage.AchievementStorageGet(tab);
            }

            var focused = FocusAchievementId != null ? AchievementsManager.FindAchievement(FocusAchievementId) : null;
            var firstTab = focused?.Tab ?? AchievementsManager.Tabs.FirstOrDefault(x => x.IsSelectable) ?? AchievementsManager.Tabs[0];
            _defaultTab = AchievementsManager.Tabs.FirstOrDefault(x => x.IsSelectable) ?? AchievementsManager.Tabs[0];
            SetTab(firstTab);
            if (mainMenuManager)
            {
                mainMenuManager.DeactivateMainMenuUI();
            }
            else if (OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(false);
                OptionsMenuBehaviour.Background.enabled = false;
            }

            _settingsPanel = AchievementsSettingsPanel.Create(transform);

            // Settings is a tab-style button pinned as the last tab
            var gearTab = Instantiate(tabPrefab.Value, tabsParent);
            gearTab.name = "SettingsTab";
            gearTab.transform.SetAsLastSibling();
            gearTab.GetComponent<Image>().sprite = ToastSprites.Gear;
            gearTab.GetComponent<Button>().onClick.AddListener(new Action(ShowSettingsPage));

            if (focused != null)
            {
                ShowDetail(focused.Value.Achievement);
            }
        }

        private AchievementsTab? _currentTab;
        private RectTransform? ContentArea => GetComponentInChildren<ScrollRect>(true)?.GetComponent<RectTransform>();

        // Hides the achievement lists and shows `page` in their place (null = lists back).
        private void ShowPage(GameObject? page)
        {
            var listsVisible = page == null;
            foreach (var scroll in GetComponentsInChildren<ScrollRect>(true))
            {
                scroll.gameObject.SetActive(listsVisible);
            }
            progressBar.Value.gameObject.SetActive(listsVisible);
            percentageText.Value.gameObject.SetActive(listsVisible);
            _settingsPanel?.SetActive(page == _settingsPanel);
            _detailPanel?.SetActive(page == _detailPanel);
            if (page != null && ContentArea != null)
            {
                MenuUi.MatchRect(page.GetComponent<RectTransform>(), ContentArea);
            }
        }

        private void ShowSettingsPage()
        {
            titleText.Value.text = "Settings";
            ShowPage(_settingsPanel);
        }

        private void ShowDetail(BaseAchievement achievement)
        {
            if (_detailPanel != null) _detailPanel.Destroy();
            _detailPanel = AchievementDetailPanel.Create(transform, achievement, () => SetTab(_currentTab!));
            ShowPage(_detailPanel);
        }

        private void SetTab(AchievementsTab tab, bool inOptionsMenu = false)
        {
            _currentTab = tab;
            if (_detailPanel != null)
            {
                _detailPanel.Destroy();
                _detailPanel = null;
            }
            ShowPage(null);
            foreach (var element in items)
            {
                element.gameObject.Destroy();
            }

            items = new();
            AchievementStorage.AchievementStorageGet(tab);

            titleText.Value.text = tab.Name;
            int achievementCount = 0;
            int completedAchievementCount = 0;
            foreach (var achievement in tab.GetAchievements())
            {
                var parent = achievement.Unlocked ? unlockedContentParent.Value : lockedContentParent.Value;
                var uiElement = Object.Instantiate(achievementItemPrefab.Value, parent).GetComponent<AchievementsMenuItem>();
                uiElement.nameText.Value.text = (!achievement.Hidden || achievement.Unlocked) ? achievement.Name : "Hidden Achievement";
                uiElement.descriptionText.Value.text = (!achievement.Hidden || achievement.Unlocked) ? achievement.Description : "Hidden Achievement";
                uiElement.iconImage.Value.sprite = (!achievement.Hidden || achievement.Unlocked) ?
                    achievement.Icon : SpriteTools.LoadSpriteFromPath("AchievementsAPI.Resources.ExampleIcon.png", Assembly.GetCallingAssembly(), 100);
                uiElement.grayscaleImage.Value.sprite = uiElement.iconImage.Value.sprite;
                if (achievement is CountAchievement countAchievement && countAchievement.RequiredValue > 0 && !(countAchievement.HideProgress && countAchievement.Hidden))
                {
                    uiElement.iconImage.Value.fillAmount = (float) countAchievement.CurrentValue / countAchievement.RequiredValue;
                    uiElement.descriptionText.Value.text += $" ({countAchievement.CurrentValue}/{countAchievement.RequiredValue})";
                }

                uiElement.nameText.Value.transform.localPosition += achievement.MenuTitleOffset;
                uiElement.descriptionText.Value.transform.localPosition += achievement.MenuDescOffset;
                uiElement.grayscaleImage.Value.transform.localPosition += achievement.MenuIconOffset;
                uiElement.iconImage.Value.transform.localPosition += achievement.MenuIconOffset;
                if (achievement.MenuSubIcon != null)
                {
                    var subIcon = Instantiate(uiElement.iconImage.Value, uiElement.iconImage.Value.transform.parent);
                    subIcon.fillAmount = 1;
                    subIcon.m_Sprite = achievement.MenuSubIcon;
                    subIcon.transform.localScale = achievement.MenuSubIconScale;
                    subIcon.transform.localPosition += achievement.MenuSubIconOffset;
                }

                var img = uiElement.GetComponent<Image>();
                if (achievement.MenuBgSprite != null)
                {
                    img.m_Sprite = achievement.MenuBgSprite;
                }
                if (achievement.RarityOnBgSprite && !(!achievement.HideRarity && !achievement.Unlocked && achievement.Hidden))
                {
                    if (achievement.Rarity == 1)
                    {
                        img.color = new Color32(112, 208, 255, 255);
                    }
                    else if (achievement.Rarity == 2)
                    {
                        img.color = new Color32(187, 88, 255, 255);
                    }
                    else if (achievement.Rarity == 3)
                    {
                        img.color = new Color32(255, 226, 64, 255);
                    }
                }
                
                if (achievement is CountAchievement count && count.RequiredValue > 0 && !(count.HideProgress && count.Hidden))
                {
                    var bar = MenuUi.CreateProgressBar(uiElement.transform, MenuUi.RarityColor(achievement.Rarity), out var fill);
                    bar.anchorMin = new Vector2(0f, 0f);
                    bar.anchorMax = new Vector2(1f, 0f);
                    bar.pivot = new Vector2(0.5f, 0f);
                    bar.anchoredPosition = new Vector2(0f, 6f);
                    bar.sizeDelta = new Vector2(-24f, 6f);
                    MenuUi.SetProgress(fill, (float) count.CurrentValue / count.RequiredValue);
                }

                var button = uiElement.gameObject.AddComponent<Button>();
                button.targetGraphic = img;
                button.transition = Selectable.Transition.ColorTint;
                var buttonColors = button.colors;
                buttonColors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
                buttonColors.colorMultiplier = 1f;
                button.colors = buttonColors;
                button.onClick.AddListener(new Action(() => ShowDetail(achievement)));

                achievementCount++;
                if (achievement.Unlocked) completedAchievementCount++;
                
                items.Add(uiElement);
            }

            if (inOptionsMenu && OptionsMenuBehaviour)
            {
                OptionsMenuBehaviour.gameObject.SetActive(false);
            }
            if (!transform.GetChild(0).TryGetComponent<Image>(out var image)) return;
            Coroutines.Start(FadeColor(image, image.color, tab.GetTabColor(), 0.3f));
            progressBar.Value.fillMethod = Image.FillMethod.Horizontal;
            if (achievementCount != 0 || completedAchievementCount != 0) progressBar.Value.fillAmount = (float) completedAchievementCount / achievementCount;
            else
            {
                progressBar.Value.fillAmount = 0;
                return;
            }

            double percent = (double) completedAchievementCount / achievementCount * 100f;
            percentageText.Value.text = $"{percent.ToString("0.00", CultureInfo.InvariantCulture)}%";
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if ((_detailPanel != null && _detailPanel.activeSelf) || (_settingsPanel != null && _settingsPanel.activeSelf))
            {
                SetTab(_currentTab ?? _defaultTab!);
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
        public void OnSearchbarChanged(string val)
        {
            int unlockedCount = 0;
            int lockedCount = 0;
            foreach (var element in items)
            {
                element.gameObject.SetActive(element.nameText.Value.text.ToLower().Contains(val.ToLower()));

                if (!element.gameObject.activeSelf) continue;
                
                if (element.transform.IsChildOf(unlockedContentParent)) unlockedCount++;
                if (element.transform.IsChildOf(lockedContentParent)) lockedCount++;
            }
            unlockedContentParent.Value.gameObject.SetActive(unlockedCount > 0);
            lockedContentParent.Value.gameObject.SetActive(lockedCount > 0);
            
            foreach (var scroll in GetComponentsInChildren<ScrollRect>())
            {
                scroll.UpdateBounds();
            }
        }
        public IEnumerator FadeColor(Image img, Color origin, Color target, float duration)
        {
            for (float i = 0; i < duration; i += Time.deltaTime)
            {
                img.color = Color.Lerp(origin, target, i/duration);
                yield return null;
            }
            img.color = target;
            yield break;
        }
    }
}
