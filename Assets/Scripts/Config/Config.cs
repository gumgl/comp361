using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A class where a bunch of constants about the game get set
/// </summary>
public static class Config {
	private static Dictionary<UnitType, int> unitUpkeep = new Dictionary<UnitType, int>();
	private static Dictionary<LandType, bool> tileMovementAllowed = new Dictionary<LandType, bool>();
	private static Dictionary<Hex.Direction, Hex> hexDirectionDelta = new Dictionary<Hex.Direction, Hex>();
	private static int unitUpgradeCost = 10;
	private static int villageUpgradeCost = 8;

	public static int test;

	public const int maxTiles = 300;

	static Config() {
		unitUpkeep[UnitType.Peasant] = 2;
		unitUpkeep[UnitType.Infantry] = 6;
		unitUpkeep[UnitType.Soldier] = 18;
		unitUpkeep[UnitType.Knight] = 54;
		
		tileMovementAllowed[LandType.Grass] = true;
		tileMovementAllowed[LandType.Meadow] = true;
		tileMovementAllowed[LandType.Road] = true;
		tileMovementAllowed[LandType.Tree] = false;
		tileMovementAllowed[LandType.Water] = false;
		tileMovementAllowed[LandType.Tombstone] = false;
		
		hexDirectionDelta[Hex.Direction.Up] = new Hex(0, -1);
		hexDirectionDelta[Hex.Direction.RightUp] = new Hex(1, -1);
		hexDirectionDelta[Hex.Direction.RightDown] = new Hex(1, 0);
		hexDirectionDelta[Hex.Direction.Down] = new Hex(0, 1);
		hexDirectionDelta[Hex.Direction.LeftDown] = new Hex(-1, 1);
		hexDirectionDelta[Hex.Direction.LeftUp] = new Hex(-1, 0);
	}
	
	public static int getUpkeep(this UnitType ut) {
		return unitUpkeep[ut];
	}
	
	public static int getUpgradeCost(this UnitType ut) {
		return unitUpgradeCost;
	}
	
	public static int getUpgradeCost(this VillageType vt) {
		return villageUpgradeCost;
	}
	
	public static bool isMovementAllowed(this LandType lt) {
		return tileMovementAllowed[lt];
	}

	/*public static GameObject getPrefab(this LandType lt) {
		return landPrefabs[lt];
	}*/
	
	static public Hex.Direction opposite(this Hex.Direction hd) {
		return (Hex.Direction)((((int)hd) + 3) % 6);
	}
	
	static public Hex.Direction getCW(this Hex.Direction hd) {
		return (Hex.Direction)((((int)hd) + 1) % 6);
	}
	
	static public Hex.Direction getCCW(this Hex.Direction hd) {
		return (Hex.Direction)((((int)hd) + 5) % 6);
	}
	
	static public Hex getDelta(this Hex.Direction hd) {
		return hexDirectionDelta[hd];
	}
}