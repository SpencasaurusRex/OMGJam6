using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class LightController : MonoBehaviour
{
    // Configuration
    public Light2D CampfireLight;
    public float SinVariation;
    public float SinSpeed = Mathf.PI;
    public float RandomVariation;
    public float RandomChangeTimeMin;
    public float RandomChangeTimeMax;

    // Runtime
    public static LightController Instance { get; private set; }
    float startingRadius;
    float startingInnerRadius;
    float randomAmount;
    float randomCountdown;
    float lightLeftTarget = 1;
    float lightLeft = 1;

    void Awake()
    {
        Instance = this;
        startingRadius = CampfireLight.pointLightOuterRadius;
        startingInnerRadius = CampfireLight.pointLightInnerRadius;
        randomCountdown = Random.Range(RandomChangeTimeMin, RandomChangeTimeMax);
    }

    public void LightAmount(float amount)
    {
        lightLeftTarget = Mathf.Max(Mathf.Min(amount * 1.75f, 1f), 0.3f);
    }

    void Update()
    {
        lightLeft = Mathf.Lerp(lightLeft, lightLeftTarget, 1 - Mathf.Exp(-3f * Time.deltaTime));

        randomCountdown -= Time.deltaTime;
        if (randomCountdown <= 0)
        {
            randomCountdown = Random.Range(RandomChangeTimeMin, RandomChangeTimeMax);
            randomAmount = Random.Range(0, RandomVariation);
        }

        //float innerRadius = startingInnerRadius * Mathf.Min(1f, lightLeft * 1.5f);
        float outerRadius = lightLeft * (startingRadius + Mathf.Sin(Time.realtimeSinceStartup * SinSpeed) * SinVariation + randomAmount);
        
        CampfireLight.transform.localScale = new Vector3(outerRadius / startingRadius, outerRadius / startingRadius, 1);
    }
}