using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour
{
    // Configuration
    public Vector2 FirstOrbPos;
    public Vector2 OrbOffset;
    public List<Image> Orbs;
    public Image StoredOrb;
    public Player Player;
    public float SlideSpeed = 50;
    public List<Sprite> LargeOrbs;
    public Image SuperShotIndicator;
    public List<Sprite> SuperShotSprites;

    // Runtime 
    float SwapSize = 1.0f;
    Queue<Image> orbQueue;
    float buffer;

    void Awake()
    {
        Player.OnShootOrb += OnShootOrb;
        Player.OnSwapStore += OnSwap;

        orbQueue = new Queue<Image>();
        foreach (var orbImage in Orbs)
        {
            orbQueue.Enqueue(orbImage);
        }
    }

    void OnShootOrb(int[] newTypes)
    {
        buffer = -OrbOffset.x;
        var endOrb = orbQueue.Dequeue();
        orbQueue.Enqueue(endOrb);

        endOrb.GetComponent<RectTransform>().localPosition = FirstOrbPos;

        var array = orbQueue.ToArray();
        for (int i = 0; i < newTypes.Length; i++)
        {
            array[i].sprite = Factory.Instance.OrbSprites[newTypes[i]];
        }
    }

    void OnSwap(int newStoreType, int newFrontType, bool skipAnimation)
    {
        SwapSize = 1.2f;
        var scale = new Vector3(SwapSize, SwapSize, 1);

        var frontOrb = orbQueue.Peek();
        frontOrb.sprite = Factory.Instance.OrbSprites[newFrontType];
        if (!skipAnimation) frontOrb.rectTransform.localScale = scale;

        StoredOrb.sprite = LargeOrbs[newStoreType];
        if (!skipAnimation) StoredOrb.rectTransform.localScale = scale;
    }

    void Update()
    {
        buffer = Mathf.Max(buffer - SlideSpeed * Time.deltaTime, 0);
        SwapSize = Mathf.Max(SwapSize - 1f * Time.deltaTime, 1);

        var scale = new Vector3(SwapSize, SwapSize, 1);
        orbQueue.Peek().rectTransform.localScale = scale;
        StoredOrb.rectTransform.localScale = scale;

        var array = orbQueue.ToArray();
        Vector2 pos = FirstOrbPos;
        foreach (var image in array)
        {
            image.GetComponent<RectTransform>().localPosition = pos - new Vector2(buffer, 0);
            pos += OrbOffset;
        }

        SuperShotIndicator.sprite = SuperShotSprites[Player.Charge];
    }

}
