using UnityEngine;

// Converts pixel coordinates of the 480x270 title-screen art (origin top-left, y down)
// into world units (16 pixels per unit, origin at screen centre).
public static class TitlePixels
{
    public const float PPU = 16f;
    public const float Width = 480f;
    public const float Height = 270f;

    public static Vector3 ToWorld(float px, float py, float z = 0f)
    {
        return new Vector3((px - Width / 2f) / PPU, (Height / 2f - py) / PPU, z);
    }
}
