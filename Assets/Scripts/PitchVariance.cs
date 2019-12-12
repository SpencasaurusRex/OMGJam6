using UnityEngine;

public class PitchVariance : MonoBehaviour
{
    public float Min;
    public float Max;

    public float GetRandomPitch()
    {
        return Random.Range(Min, Max);
    }
}
