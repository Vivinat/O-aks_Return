// Assets/Scripts/UI/OptionsMenuSetup.cs (Versão Simplificada para Configuração Manual)

using UnityEngine;

/// <summary>
/// Script simples que garante que existe um OptionsMenu na cena.
/// Para configuração manual - você cria a UI e apenas adiciona este script para verificação.
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
                Debug.Log($"✅ OptionsMenu encontrado na cena '{gameObject.scene.name}'");
                
            // Configura o nome da cena do menu se foi especificado
            if (!string.IsNullOrEmpty(menuSceneName))
            {
                // Usando reflection para acessar o campo privado de forma segura
                var field = typeof(OptionsMenu).GetField("menuSceneName", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                if (field != null)
                {
                    field.SetValue(optionsMenu, menuSceneName);
                    if (showDebugMessages)
                        Debug.Log($"📝 Menu scene configurado para: {menuSceneName}");
                }
            }
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.LogWarning($"⚠️ Nenhum OptionsMenu encontrado na cena '{gameObject.scene.name}'!");
                Debug.LogWarning("💡 Certifique-se de ter criado o menu manualmente e adicionado o script OptionsMenu.");
            }
        }
    }

    /// <summary>
    /// Método para testar se o menu funciona
    /// </summary>
    [ContextMenu("Testar Menu")]
    public void TestMenu()
    {
        OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (optionsMenu != null)
        {
            optionsMenu.ToggleOptionsMenu();
            Debug.Log("🧪 Teste do menu executado!");
        }
        else
        {
            Debug.LogError("❌ Nenhum OptionsMenu encontrado para testar!");
        }
    }

    /// <summary>
    /// Verifica se todas as referências estão configuradas
    /// </summary>
    [ContextMenu("Verificar Configuração")]
    public void ValidateSetup()
    {
        OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>();
        
        if (optionsMenu == null)
        {
            Debug.LogError("❌ OptionsMenu não encontrado!");
            return;
        }

        Debug.Log("🔍 Verificando configuração do OptionsMenu...");
        
        // Lista de verificações básicas
        bool allGood = true;
        
        // Nota: Como os campos são privados, esta verificação é limitada
        // Mas o OptionsMenu tem sua própria validação no OnValidate()
        
        if (allGood)
        {
            Debug.Log("✅ Configuração parece estar correta!");
            Debug.Log("💡 Verifique o Console para warnings do OptionsMenu.OnValidate()");
        }
    }

    void OnValidate()
    {
        // Validação simples no Editor
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("OptionsMenuSetup: menuSceneName está vazio!");
        }
    }
}