using System;
using SimpleJSON;
using UnityEngine;
public struct Hex {
	public int q, r;
	public Hex(int iq, int ir) {
		q = iq;
		r = ir;
	}
	public Hex(JSONNode node)
	{
		q = node["q"].AsInt;
		r = node["r"].AsInt;
	}
	public enum Direction {
		Up,
		RightUp,
		RightDown,
		Down,
		LeftDown,
		LeftUp
	}
	static public int Distance(Hex a, Hex b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.q + a.r - b.q - b.r) + Mathf.Abs(a.r - b.r)) / 2;
    }
	public static bool operator==(Hex a, Hex b) {
		return (a.q == b.q && a.r == b.r);
	}
	public static bool operator!=(Hex a, Hex b) {
		return (a.q != b.q || a.r != b.r);
	}
	public static Hex operator+(Hex a, Hex b) {
		return new Hex(a.q + b.q, a.r + b.r);
	}
	public static Hex operator-(Hex a, Hex b) {
		return new Hex(a.q - b.q, a.r - b.r);
	}
	public static Hex operator*(Hex a, int factor) {
		return new Hex(a.q * factor, a.r * factor);
	}
	public static Hex operator/(Hex a, int factor) {
		return new Hex(a.q / factor, a.r / factor);
	}
	public static Hex operator-(Hex a) {
		return a * -1;
	}
}