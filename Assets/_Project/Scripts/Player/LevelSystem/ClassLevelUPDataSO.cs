using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassLevelUPDataSO", menuName = "ScriptableObjects/ClassLevelUPDataSO", order = 1)]
public class ClassLevelUPDataSO : ScriptableObject
{
    public LevelUpData initialLevelData = new();
    public LevelUpData levelUpData = new();
}

[Serializable]
public class LevelUpData {
    public string name;
    public List<AttributeData> attributeDatas = new();
}

[Serializable]
public class AttributeData
{
    public Attribute attribute;
    public float flatAmount;
}

public enum Attribute
{
    CriticalChance,
    Health,
    HealthRegen,
    Cooldown,
    Damage,
    Speed
}

