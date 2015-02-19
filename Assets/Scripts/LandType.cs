using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public abstract static class LandType {

    public string name;
    public GameObject tileVisualPrefab;
    public bool isWalkable;
    public float movementCost;


}
