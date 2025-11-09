using UnityEngine;

// Gerencia o sprite de fundo das batalhas dinamicamente
public class BattleBackgroundManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private string backgroundTag = "Background";
    [SerializeField] private Sprite defaultBackground;
    
    void Start()
    {
        if (backgroundRenderer == null)
        {
            GameObject backgroundObject = GameObject.FindWithTag(backgroundTag);
            if (backgroundObject != null)
            {
                backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            }
        }
        
        LoadBattleBackground();
    }
    
    private void LoadBattleBackground()
    {
        
        Sprite battleBackground = GetBattleBackgroundSprite();
        
        if (battleBackground != null)
        {
            backgroundRenderer.sprite = battleBackground;
        }
        else if (defaultBackground != null)
        {
            backgroundRenderer.sprite = defaultBackground;
        }
    }
    
    // Recupera o sprite de background com sistema de prioridades
    private Sprite GetBattleBackgroundSprite()
    {
        if (GameManager.pendingBattleBackground != null)
        {
            Sprite nodeBackground = GameManager.pendingBattleBackground;
            GameManager.pendingBattleBackground = null;
            return nodeBackground;
        }
    
        if (GameManager.Instance?.CurrentEvent is BattleEventSO battleEvent)
        {
            if (battleEvent.battleBackground != null)
            {
                return battleEvent.battleBackground;
            }
        }
    
        return null;
    }
    
    public void SetBackground(Sprite newBackground)
    {
        if (backgroundRenderer != null && newBackground != null)
        {
            backgroundRenderer.sprite = newBackground;
        }
    }
    
    [ContextMenu("Refresh Background Object")]
    public void RefreshBackgroundObject()
    {
        GameObject backgroundObject = GameObject.FindWithTag(backgroundTag);
        if (backgroundObject != null)
        {
            backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError($"Nenhum objeto com a tag '{backgroundTag}' encontrado");
        }
    }
}