using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public float SelectionOffset;
    public float SelectorOffset;
    public RectTransform Selector;
    public Image SelectorImage;
    public Credits CreditsUI;

    List<GameObject> Buttons;

    void Awake()
    {
        Buttons = GetComponentsInChildren<Button>().Select(x => x.gameObject).ToList();
    }

    void OnEnable()
    {
        GetComponent<EventSystem>().enabled = true;
        GetComponent<StandaloneInputModule>().enabled = true;
    }

    void Update()
    {
        var selected = GetComponent<EventSystem>().currentSelectedGameObject;

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
        Selector.localPosition = new Vector3(-13.31f, targetTransform.localPosition.y, 0);
    }

    public void StartGame()
    {
        GameController.Instance.StartGame();
        GetComponent<EventSystem>().enabled = false;
        GetComponent<StandaloneInputModule>().enabled = false;
        gameObject.SetActive(false);
    }

    public void Controls()
    {
        // TODO
    }

    public void Credits()
    {
        CreditsUI.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}