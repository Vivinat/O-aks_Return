using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Gerencia o menu principal do jogo
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitGameButton;
    
    [SerializeField] private string firstMapScene = "Map1";

    void Start()
    {
        SetupButtons();
        Time.timeScale = 1f;
    }

    private void SetupButtons()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGame);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OpenOptions);
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ExitGame);
        }
    }

    public void StartGame()
    {
        AudioConstants.PlayButtonSelect();
        ResetGameData();
        SceneManager.LoadScene(firstMapScene);
    }

    public void OpenOptions()
    {
        if (OptionsMenu.Instance != null)
        {
            OptionsMenu.Instance.OpenOptionsMenu();
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ResetGameData()
    {
        PlayerPrefs.DeleteKey("LastCompletedNode");
        PlayerPrefs.DeleteKey("CompletedBossNode");
        PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearMapData("Map1");
            GameManager.Instance.ClearMapData("Map2");
            GameManager.Instance.ClearMapData("Map3");
        }
        
        PlayerPrefs.Save();
    }
    
}