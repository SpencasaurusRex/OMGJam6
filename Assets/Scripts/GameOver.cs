using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Time = UnityEngine.Time;

public class GameOver : MonoBehaviour
{
    public List<Image> Images;
    public List<TextMeshProUGUI> Text;
    public float FadeTime;
    public List<GameObject> Buttons;
    public float SelectionOffset;

    public RectTransform Selector;
    public Image SelectorImage;

    float fadeAmount;
    bool over;

    public void Over()
    {
        over = true;
    }

    void Update()
    {
        if (over)
        {
            foreach (var image in Images)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, fadeAmount);
            }

            foreach (var text in Text)
            {
                text.color = new Color(1, 1, 1, fadeAmount);
            }
        }

        if (fadeAmount < 1)
        {
            fadeAmount += Time.deltaTime / FadeTime;
            SelectorImage.enabled = false;
            return;
        }
        
        GetComponent<EventSystem>().enabled = true;
        GetComponent<StandaloneInputModule>().enabled = true;

        var selected = EventSystem.current.currentSelectedGameObject;

        // Rebind if none selected
        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.3f && selected == null)
        {
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        }

        foreach (var button in Buttons)
        {
            var childRect = button.GetComponentsInChildren<RectTransform>().First(x => x.gameObject != button);
            childRect.localPosition = selected == button.gameObject ? new Vector2(SelectionOffset, 0) : new Vector2(0, 0);
        }

        SelectorImage.enabled = selected != null;
        if (selected == null) return;

        var targetTransform = selected.GetComponent<RectTransform>();
        Selector.localPosition = new Vector3(-14.35f, targetTransform.localPosition.y, 0);
    }

    public void MainMenu()
    {
        Cleanup();
        GameController.Instance.GoToMainMenu();
        gameObject.SetActive(false);
    }

    public void TryAgain()
    {
        Cleanup();
        GameController.Instance.StartGame();
        gameObject.SetActive(false);
    }

    void Cleanup()
    {
        GetComponent<EventSystem>().enabled = false;
        GetComponent<StandaloneInputModule>().enabled = false;
        fadeAmount = 0;

        foreach (var image in Images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, fadeAmount);
        }
        foreach (var text in Text)
        {
            text.color = new Color(1, 1, 1, fadeAmount);
        }
        over = false;
    }
}
