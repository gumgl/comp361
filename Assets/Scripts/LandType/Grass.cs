using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grass : LandType {
    public static string name = "Grass";
    public static GameObject tileVisualPrefab;
    public static bool isWalkable = true;
    public static float movementCost = 1.0f;
}