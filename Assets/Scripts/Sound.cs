using UnityEngine;

public class Sound : MonoBehaviour
{
    AudioSource source;
    bool playing;

    public AudioClip Clip;
    public float Pitch;

    float countdown;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!playing)
        {
            source.clip = Clip;
            source.pitch = Pitch;
            source.Play();
            playing = true;
            countdown = Clip.length;
        }

        countdown -= Time.deltaTime;

        if (countdown <= 0)
        {
            Destroy(gameObject);
        }
    }
}
