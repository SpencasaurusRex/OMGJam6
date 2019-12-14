using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class UIScaler : MonoBehaviour
{
    public PixelPerfectCamera Camera;
    public CanvasScaler Scaler;

    void Awake()
    {
        Scaler.scaleFactor = Camera.pixelRatio;
    }

    void Update()
    {
        Scaler.scaleFactor = Camera.pixelRatio;
    }
}
