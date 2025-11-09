using UnityEngine;

public class OptionsMenuSetup : MonoBehaviour
{
    public string menuSceneName = "MainMenu";
    
    public bool showDebugMessages = true;

    void Start()
    {
        CheckOptionsMenu();
    }

    private void CheckOptionsMenu()
    {
        OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (optionsMenu != null)
        {
                
            if (!string.IsNullOrEmpty(menuSceneName))
            {
                var field = typeof(OptionsMenu).GetField("menuSceneName", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (field != null)
                {
                    field.SetValue(optionsMenu, menuSceneName);
                }
            }
        }
    }
}