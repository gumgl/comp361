using UnityEngine;
using System.Collections.Generic;
/*
 * A class where a bunch of constants about the game get set
 */
public static class Config {
    private static List<int> unitUpkeep = new List<int>();
    private static List<int> landMovementCost = new List<int>();
    private static int unitUpgradeCost = 10;
    private static int villageUpgradeCost = 8;

    public static int test;

    public static GameObject[] landPrefabs;

    public const int maxTiles = 300;

    static Config() {
        unitUpkeep[(int)UnitType.Peasant] = 2;
        unitUpkeep[(int)UnitType.Infantry] = 6;
        unitUpkeep[(int)UnitType.Soldier] = 18;
        unitUpkeep[(int)UnitType.Knight] = 54;
        
        landMovementCost[(int)LandType.Grass] = 1;
        landMovementCost[(int)LandType.Meadow] = 1;
        landMovementCost[(int)LandType.Tree] = 4;
        landMovementCost[(int)LandType.Water] = 99;
    }
    
    public static int getUpkeep(this UnitType ut) {
        return unitUpkeep[(int)ut];
    }
    
    public static int getUpgradeCost(this UnitType ut) {
        return unitUpgradeCost;
    }
    
    public static int getUpgradeCost(this VillageType vt) {
        return villageUpgradeCost;
    }
    
    public static int getMovementCost(this LandType lt) {
        return landMovementCost[(int)lt];
    }

    public static GameObject getPrefab(this LandType lt) {
        return landPrefabs[(int)lt];
    }
}