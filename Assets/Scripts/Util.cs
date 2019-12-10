using UnityEngine;

public static class Util
{
    public static Vector2 Rotate(this Vector2 vec, float degrees)
    {
        return (Quaternion.Euler(0,0, degrees) * vec);
    }
}