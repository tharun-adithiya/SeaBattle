using NUnit.Framework.Internal.Execution;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField]private GameObject currentWindow;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject difficultyUI;
    [SerializeField] private GameObject playerShipPlacementUI;
    [SerializeField] private GameObject botShipPlacementUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject playerShootUI;
    [SerializeField] private GameObject botShootUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject winUI;

    private void Awake()
    {
        if (Instance!=null&&Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        SetWindowState(WindowStates.Main);                                   //Always start with Start/MainMenu
    }

    public void SetWindowState(WindowStates nextWindow)
    {
        if (currentWindow != null) currentWindow.SetActive(false);          //Disables the current active window

        switch (nextWindow)                                                  //Switch based on the selected window state.
        {
            case WindowStates.Main:
                currentWindow=mainMenu;
                currentWindow.SetActive(true);
                break;
            case WindowStates.DifficultySelection:
                currentWindow=Instantiate(difficultyUI);
                currentWindow.SetActive(true);
                currentWindow.transform.SetParent(transform);
                RectTransform rect = currentWindow.GetComponent<RectTransform>();
                if (rect != null) rect.anchoredPosition = Vector2.zero;
                break;
            case WindowStates.PlayerShipPlacementUI:
                currentWindow=playerShipPlacementUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.BotShipPlacementUI:
                currentWindow=botShipPlacementUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.GameUI:
                currentWindow=gameUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.PlayerShootUI:
                currentWindow=playerShootUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.BotShootUI:
                currentWindow=botShootUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.Win:
                currentWindow=winUI;
                currentWindow.SetActive(true);
                break;
            case WindowStates.GameOver:
                currentWindow = gameOverUI;
                currentWindow.SetActive(true);
                break;

        }
    }
}
