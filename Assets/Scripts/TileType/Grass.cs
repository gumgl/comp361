using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Grass : LandType {
    public string name = "Grass";
    public GameObject tileVisualPrefab;
    public bool isWalkable = true;
    public float movementCost = 1.0f;
}