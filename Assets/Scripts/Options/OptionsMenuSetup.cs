using UnityEngine;

/// <summary>
/// Garante que existe um OptionsMenu na cena
/// </summary>
public class OptionsMenuSetup : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("Nome da cena do menu principal")]
    public string menuSceneName = "MainMenu";
    
    [Tooltip("Se deve mostrar avisos no Console")]
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
            if (showDebugMessages)
                Debug.Log($"OptionsMenu encontrado na cena '{gameObject.scene.name}'");
                
            if (!string.IsNullOrEmpty(menuSceneName))
            {
                var field = typeof(OptionsMenu).GetField("menuSceneName", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (field != null)
                {
                    field.SetValue(optionsMenu, menuSceneName);
                    if (showDebugMessages)
                        Debug.Log($"Menu scene configurado para: {menuSceneName}");
                }
            }
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.LogWarning($"Nenhum OptionsMenu encontrado na cena '{gameObject.scene.name}'!");
                Debug.LogWarning("Certifique-se de ter criado o menu manualmente e adicionado o script OptionsMenu.");
            }
        }
    }

    [ContextMenu("Testar Menu")]
    public void TestMenu()
    {
        OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (optionsMenu != null)
        {
            optionsMenu.ToggleOptionsMenu();
            Debug.Log("Teste do menu executado!");
        }
        else
        {
            Debug.LogError("Nenhum OptionsMenu encontrado para testar!");
        }
    }

    [ContextMenu("Verificar Configuração")]
    public void ValidateSetup()
    {
        OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (optionsMenu == null)
        {
            Debug.LogError("OptionsMenu não encontrado!");
            return;
        }

        Debug.Log("Verificando configuração do OptionsMenu...");
        Debug.Log("Configuração parece estar correta!");
        Debug.Log("Verifique o Console para warnings do OptionsMenu.OnValidate()");
    }

    void OnValidate()
    {
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("OptionsMenuSetup: menuSceneName está vazio!");
        }
    }
}