using System;
public struct Hex {
	public int q, r;
	public Hex(int iq, int ir) {
		q = iq;
		r = ir;
	}
	public enum Direction {
		Up,
		RightUp,
		RightDown,
		Down,
		LeftDown,
		LeftUp
	}
	public static bool operator==(Hex h1, Hex h2) {
		return (h1.q == h2.q && h1.r == h2.r);
	}
	public static bool operator!=(Hex h1, Hex h2) {
		return (h1.q != h2.q || h1.r != h2.r);
	}
	public static Hex operator+(Hex h1, Hex h2) {
		return new Hex(h1.q + h2.q, h1.r + h2.r);
	}
	public static Hex operator-(Hex h1, Hex h2) {
		return new Hex(h1.q - h2.q, h1.r - h2.r);
	}
	public static Hex operator*(Hex h1, int factor) {
		return new Hex(h1.q * factor, h1.r * factor);
	}
	public static Hex operator/(Hex h1, int factor) {
		return new Hex(h1.q / factor, h1.r / factor);
	}
	public static Hex operator-(Hex h1) {
		return h1 * -1;
	}
}