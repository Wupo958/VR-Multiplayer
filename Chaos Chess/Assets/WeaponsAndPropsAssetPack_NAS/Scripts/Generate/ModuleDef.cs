using UnityEngine;

[CreateAssetMenu(fileName = "ModuleDef", menuName = "Scriptable Objects/ModuleDef")]
public class ModuleDef : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public Vector2Int size = Vector2Int.one;
    public int heightDelta;
    public bool isStart;
    public bool isHole;
    public ModuleDef[] compatibleNext;
}
