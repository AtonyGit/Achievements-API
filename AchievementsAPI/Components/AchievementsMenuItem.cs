using System.Collections;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AchievementsAPI
{
    [RegisterInIl2Cpp]
    public class AchievementsMenuItem : MonoBehaviour
    {
        public Il2CppReferenceField<TextMeshProUGUI> nameText;
        public Il2CppReferenceField<TextMeshProUGUI> descriptionText;
        public Il2CppReferenceField<Image> iconImage;
    }
}
