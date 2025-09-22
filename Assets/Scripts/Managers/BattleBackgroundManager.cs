
using UnityEngine;

/// <summary>
/// Gerencia o sprite de fundo das batalhas dinamicamente
/// Suporta configuração via BattleEventSO e override via MapNode
/// </summary>
public class BattleBackgroundManager : MonoBehaviour
{
    [Header("Background Configuration")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private string backgroundTag = "Background";
    [SerializeField] private Sprite defaultBackground; // Fallback se nenhum for configurado
    
    void Start()
    {
        // Tenta encontrar o objeto com a tag se não foi atribuído manualmente
        if (backgroundRenderer == null)
        {
            GameObject backgroundObject = GameObject.FindWithTag(backgroundTag);
            if (backgroundObject != null)
            {
                backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            }
        }
        
        // Carrega o sprite de fundo configurado no nó
        LoadBattleBackground();
    }
    
    /// <summary>
    /// Carrega o sprite de fundo baseado na configuração do evento de batalha
    /// Prioridade: 1. BattleEventSO, 2. MapNode override, 3. Default
    /// </summary>
    private void LoadBattleBackground()
    {
        if (backgroundRenderer == null)
        {
            Debug.LogWarning("BattleBackgroundManager: SpriteRenderer do background não encontrado!");
            return;
        }
        
        Debug.Log("=== BATTLE BACKGROUND DEBUG ===");
        Debug.Log($"GameManager.Instance: {(GameManager.Instance != null ? "Found" : "NULL")}");
        Debug.Log($"GameManager.CurrentEvent: {(GameManager.Instance?.CurrentEvent?.name ?? "NULL")}");
        Debug.Log($"GameManager.pendingBattleBackground: {(GameManager.pendingBattleBackground?.name ?? "NULL")}");
        
        Sprite battleBackground = GetBattleBackgroundSprite();
        
        if (battleBackground != null)
        {
            backgroundRenderer.sprite = battleBackground;
            Debug.Log($"✅ Background da batalha definido: {battleBackground.name}");
        }
        else if (defaultBackground != null)
        {
            backgroundRenderer.sprite = defaultBackground;
            Debug.Log("✅ Usando background padrão");
        }
        else
        {
            Debug.Log("⚠️ Nenhum background configurado - mantendo sprite atual");
        }
        
        Debug.Log("=== END BACKGROUND DEBUG ===");
    }
    
// Assets/Scripts/Battle/BattleBackgroundManager.cs

    /// <summary>
    /// Recupera o sprite de background com sistema de prioridades
    /// PRIORIDADE CORRIGIDA: 1. MapNode override, 2. BattleEventSO, 3. Default
    /// </summary>
    private Sprite GetBattleBackgroundSprite()
    {
        // PRIORIDADE 1: Background pendente do MapNode (via GameManager)
        if (GameManager.pendingBattleBackground != null)
        {
            Sprite nodeBackground = GameManager.pendingBattleBackground;
            GameManager.pendingBattleBackground = null; // Limpa após usar para não afetar batalhas futuras
            Debug.Log($"Usando background configurado no MapNode: {nodeBackground.name}");
            return nodeBackground;
        }
    
        // PRIORIDADE 2: Background do BattleEventSO (fallback se o MapNode não tiver um)
        if (GameManager.Instance?.CurrentEvent is BattleEventSO battleEvent)
        {
            if (battleEvent.battleBackground != null)
            {
                Debug.Log($"Usando background do BattleEventSO: {battleEvent.battleBackground.name}");
                return battleEvent.battleBackground;
            }
        }
    
        // PRIORIDADE 3: Nenhum configurado - será usado o default
        Debug.Log("Nenhum background específico encontrado");
        return null; // Retorna null para que LoadBattleBackground use o default
    }
    
    /// <summary>
    /// Define um sprite de background manualmente (útil para testes)
    /// </summary>
    public void SetBackground(Sprite newBackground)
    {
        if (backgroundRenderer != null && newBackground != null)
        {
            backgroundRenderer.sprite = newBackground;
            Debug.Log($"Background alterado manualmente para: {newBackground.name}");
        }
    }
    
    /// <summary>
    /// Força uma nova busca pelo objeto de background
    /// </summary>
    [ContextMenu("Refresh Background Object")]
    public void RefreshBackgroundObject()
    {
        GameObject backgroundObject = GameObject.FindWithTag(backgroundTag);
        if (backgroundObject != null)
        {
            backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            Debug.Log($"Background object atualizado: {backgroundObject.name}");
        }
        else
        {
            Debug.LogError($"Nenhum objeto com a tag '{backgroundTag}' encontrado!");
        }
    }
}