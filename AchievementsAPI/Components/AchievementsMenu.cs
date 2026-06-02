using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public MainMenuManager mainMenuManager;

        private void Start()
        {
            foreach (var tab in AchievementsManager.Tabs)
            {
                var go = Object.Instantiate(tabPrefab.Value, tabsParent);
                var btn = go.GetComponent<Button>();
                var sprite = tab.GetIcon();
                if (sprite) go.GetComponent<Image>().sprite = sprite;
                btn.onClick.AddListener(new Action((() =>
                {
                    SetTab(tab);
                })));
            }
            SetTab(AchievementsManager.Tabs[0]);
            mainMenuManager?.DeactivateMainMenuUI();
        }
        private void SetTab(AchievementsTab tab)
        {
            foreach (var element in GetComponentsInChildren<AchievementsMenuItem>())
            {
                element.gameObject.Destroy();
            }

            titleText.Value.text = tab.Name;
            foreach (var propInfo in tab.GetType().GetProperties().Where(x => x.PropertyType.IsSubclassOf(typeof(BaseAchievement)) || x.PropertyType == typeof(BaseAchievement)))
            {
                var achievement = (BaseAchievement) propInfo.GetValue(tab);
                if (achievement == null) continue; 

                var parent = achievement.Unlocked ? unlockedContentParent.Value : lockedContentParent.Value;
                var uiElement = Object.Instantiate(achievementItemPrefab.Value, parent).GetComponent<AchievementsMenuItem>();
                uiElement.nameText.Value.text = achievement.Name;
                uiElement.descriptionText.Value.text = achievement.Description;
                uiElement.iconImage.Value.sprite =
                    SpriteTools.LoadSpriteFromPath(achievement.IconPath, achievement.Assembly, 100);
                
                if (achievement is CountAchievement countAchievement && countAchievement.RequiredValue > 0)
                {
                    uiElement.iconImage.Value.fillAmount = (float) countAchievement.CurrentValue / countAchievement.RequiredValue;
                }
            }

            if (!transform.GetChild(0).TryGetComponent<Image>(out var image)) return;
            Coroutines.Start(FadeColor(image, image.color, tab.GetTabColor(), 0.3f));
        }

        public void Close()
        {
            mainMenuManager?.ActivateMainMenuUI();
            gameObject.Destroy();
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
