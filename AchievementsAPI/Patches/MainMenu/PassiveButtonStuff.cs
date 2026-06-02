using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementsAPI.MainMenu;

public static class PassiveButtonStuff
{
    public static PassiveButton CreatePassiveButton(string ObjectName, Sprite active, Sprite inactive,
        Vector2 ColliderSize, Action listener, string Layer = "UI", bool HasText = false, string Text = "")

    {
        GameObject go = new GameObject(ObjectName);
        go.layer = LayerMask.NameToLayer(Layer);
        var passive = go.AddComponent<PassiveButton>();

        passive.activeSprites = new GameObject("Active");
        passive.activeSprites.transform.SetParent(go.transform);
        passive.activeSprites.transform.localScale = Vector3.one;
        passive.activeSprites.transform.localPosition = Vector3.zero;
        passive.activeSprites.AddComponent<SpriteRenderer>().sprite = active;

        passive.inactiveSprites = new GameObject("Inactive");
        passive.inactiveSprites.transform.SetParent(go.transform);
        passive.inactiveSprites.transform.localScale = Vector3.one;
        passive.inactiveSprites.transform.localPosition = Vector3.zero;
        passive.inactiveSprites.AddComponent<SpriteRenderer>().sprite = inactive;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = ColliderSize;
        col.isTrigger = true;
        passive.ClickMask = col;
        passive.Colliders = new Il2CppReferenceArray<Collider2D>([passive.ClickMask]);

        passive.OnMouseOver = new();
        passive.OnMouseOut = new();

        passive.OnClick = new Button.ButtonClickedEvent();
        passive.OnClick.AddListener(listener);

        if (HasText)
        {
            var text = new GameObject("Text").AddComponent<TextMeshPro>();
            text.transform.SetParent(go.transform);
            text.transform.localScale = Vector3.one;
            text.transform.localPosition = Vector3.zero;
            text.text = Text;
            passive.buttonText = text;
        }
        return passive;
    }
}