// Assets/Scripts/Dialogue/DialogueTestScript.cs
// Script para testar o sistema de diálogo

using UnityEngine;
using System.Collections.Generic;

public class DialogueTestScript : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private KeyCode testKey = KeyCode.Space;
    [SerializeField] private DialogueSO testDialogueSO;

    void Update()
    {
        // Testa com tecla T
        if (Input.GetKeyDown(testKey))
        {
            TestDialogueSystem();
        }

        // Testa diferentes tipos de diálogo com números
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestSimpleDialogue();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestConversation();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestLongDialogue();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestDialogueWithCallback();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestBattleIntro();
        }
    }

    void TestDialogueSystem()
    {
        if (testDialogueSO != null)
        {
            DialogueUtils.ShowDialogue(testDialogueSO, () => {
                Debug.Log("✅ Teste do DialogueSO concluído!");
            });
        }
        else
        {
            TestSimpleDialogue();
        }
    }

    void TestSimpleDialogue()
    {
        DialogueUtils.ShowSimpleDialogue("Testador", "Este é um teste simples do sistema de diálogo. Clique uma vez para completar o texto, clique novamente para fechar.", () => {
            Debug.Log("✅ Diálogo simples concluído!");
        });
    }

    void TestConversation()
    {
        DialogueUtils.ShowConversation(
            "NPC", "Olá, aventureiro! Como posso ajudá-lo hoje?",
            "Jogador", "Estou testando o sistema de diálogo. Parece estar funcionando bem!",
            () => {
                Debug.Log("✅ Conversa concluída!");
            }
        );
    }

    void TestLongDialogue()
    {
        var dialogue = DialogueUtils.CreateBuilder()
            .AddNarration("Era uma vez, em um reino muito distante...")
            .AddLine("Rei", "Jovem aventureiro, preciso de sua ajuda!")
            .AddLine("Jogador", "Como posso ajudá-lo, Vossa Majestade?")
            .AddLine("Rei", "Um dragão terrível está aterrorizando nosso reino. Apenas um herói corajoso pode detê-lo.")
            .AddLine("Jogador", "Aceito a missão! Onde posso encontrar este dragão?")
            .AddLine("Rei", "Na montanha sombria, ao norte do reino. Cuidado, pois ele é muito poderoso!")
            .AddNarration("Sua grande aventura começou...");

        dialogue.Show(() => {
            Debug.Log("✅ Diálogo longo concluído!");
        });
    }

    void TestDialogueWithCallback()
    {
        DialogueUtils.ShowSimpleDialogue("Sistema", "Este diálogo irá executar uma ação especial quando terminar.", () => {
            Debug.Log("🎉 AÇÃO ESPECIAL EXECUTADA!");
            
            // Simula uma ação do jogo
            GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.name = "Cubo Criado pelo Diálogo";
            testCube.transform.position = new Vector3(0, 2, 0);
            
            DialogueUtils.ShowSimpleDialogue("Sistema", "Um cubo foi criado como demonstração do callback!");
        });
    }

    void TestBattleIntro()
    {
        DialogueUtils.ShowBattleIntro("Slime Gigante", () => {
            Debug.Log("✅ Introdução de batalha concluída!");
            DialogueUtils.ShowBattleVictory(25, () => {
                Debug.Log("✅ Vitória de batalha mostrada!");
            });
        });
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== TESTE DO SISTEMA DE DIÁLOGO ===", GUI.skin.box);
        GUILayout.Label($"Pressione '{testKey}' ou use os números:");
        GUILayout.Label("1 - Diálogo Simples");
        GUILayout.Label("2 - Conversa");
        GUILayout.Label("3 - Diálogo Longo");
        GUILayout.Label("4 - Diálogo com Callback");
        GUILayout.Label("5 - Introdução de Batalha");
        GUILayout.Label("");
        
        if (DialogueUtils.IsDialogueActive())
        {
            GUILayout.Label("📝 DIÁLOGO ATIVO - Teste o clique!", GUI.skin.box);
            GUILayout.Label("• 1 clique enquanto digita = completa texto");
            GUILayout.Label("• 1 clique com texto completo = próximo");
            GUILayout.Label("• 2 cliques rápidos = pula tudo");
            GUILayout.Label("• ESC = pula tudo (debug)");
        }
        
        GUILayout.EndArea();
    }

    // Para debugar no console
    void Start()
    {
        Debug.Log("🔧 DialogueTestScript ativo!");
        Debug.Log($"Pressione '{testKey}' para testar o sistema de diálogo");
        Debug.Log("Pressione 1-5 para testes específicos");
        
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("❌ DialogueSystem não encontrado! Execute o setup primeiro.");
        }
        else
        {
            Debug.Log("✅ DialogueSystem encontrado e pronto!");
        }
    }
}