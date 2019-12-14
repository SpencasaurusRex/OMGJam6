using UnityEngine;
using UnityEngine.EventSystems;

public class Credits : MonoBehaviour
{
    public MainMenu MainMenu;

    void OnEnable()
    {
        print("OnEnable");
        GetComponent<EventSystem>().enabled = true;
        GetComponent<StandaloneInputModule>().enabled = true;
    }

    public void BackToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
