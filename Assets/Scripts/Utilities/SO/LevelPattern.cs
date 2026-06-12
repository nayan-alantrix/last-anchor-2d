using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelPattern", menuName = "Game/LevelPattern")]
public class LevelPattern : ScriptableObject
{
    public List<PatternItem> items;
}

[System.Serializable]
public class PatternItem
{
    public GameObject prefab;
    public Vector3 localPosition;  // position relative to block center
}