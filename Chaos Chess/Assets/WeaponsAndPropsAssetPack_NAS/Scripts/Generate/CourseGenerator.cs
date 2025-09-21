using UnityEngine;
using Unity.Netcode;
using Random = System.Random;
using System.Collections.Generic;

public class CourseGenerator : MonoBehaviour
{
    [Header("Gen Settings")]
    public int seed = 12345;
    public int segments = 12;
    public Vector3 cellSize = new Vector3(2f, 0.3f, 2f);
    public ModuleDef startModule;
    public ModuleDef holeModule;
    public List<ModuleDef> palette;

    [Header("Spawn Root (networked parents are fine)")]
    public Transform courseRoot;

    // Internal
}
