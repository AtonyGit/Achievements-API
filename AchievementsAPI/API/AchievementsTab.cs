using JetBrains.Annotations;
using UnityEngine;

namespace AchievementsAPI.API;

public abstract class AchievementsTab
{
    public abstract string Name { get; }
    
    public virtual Sprite GetIcon()
    {
        return null;
    }
}