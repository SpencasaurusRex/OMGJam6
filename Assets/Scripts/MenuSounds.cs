using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSounds : MonoBehaviour
{
    public AudioClip Sound;
    GameObject lastSelected;

    void Update()
    {
        var sel = EventSystem.current?.currentSelectedGameObject;
        if (sel != null)
        {
            if (lastSelected != sel)
            {
                Factory.Instance.PlaySound(Sound, 1, 0.4f);
            }

            lastSelected = sel;
        }
    }
}
