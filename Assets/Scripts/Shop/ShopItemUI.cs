// Assets/Scripts/UI/ShopItemUI.cs (FIXED - Com Tooltips Dinâmicos)

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI usesText;
    public Image typeIndicator; // NOVO: Indicador visual de tipo (opcional)
    
    [Header("Type Colors")]
    public Color battleActionColor = new Color(0.8f, 0.8f, 1f);
    public Color powerupColor = new Color(1f, 0.8f, 0.5f);

    // Variáveis internas organizadas
    private ShopItem shopItem;
    private ShopManager shopManager;
    private bool isForSale;
    private int displayPrice = 0;

    /// <summary>
    /// Setup para itens à venda usando ShopItem wrapper
    /// </summary>
    public void SetupForSale(ShopItem item, ShopManager manager, int displayPrice = -1)
    {
        this.shopItem = item;
        this.shopManager = manager;
        this.isForSale = true;
        this.displayPrice = displayPrice > 0 ? displayPrice : item.Price;
        
        SetupUI();
    }

    /// <summary>
    /// Setup para BattleAction à venda (compatibilidade)
    /// </summary>
    public void SetupForSale(BattleAction action, ShopManager manager, int displayPrice = -1)
    {
        SetupForSale(new ShopItem(action), manager, displayPrice);
    }

    /// <summary>
    /// Setup para slots do jogador (apenas BattleActions)
    /// </summary>
    public void SetupPlayerSlot(BattleAction action, ShopManager manager)
    {
        this.shopItem = new ShopItem(action);
        this.shopManager = manager;
        this.isForSale = false;
        SetupUI();
    }

    private void SetupUI()
    {
        if (shopItem == null) return;
        
        // Configura o ícone
        if (iconImage != null)
        {
            iconImage.sprite = shopItem.Icon;
            iconImage.enabled = (shopItem.Icon != null);
        }

        // Configura o preço (apenas para itens à venda)
        if (priceText != null)
        {
            if (isForSale)
            {
                int priceToShow = displayPrice > 0 ? displayPrice : shopItem.Price;
                priceText.text = $"{priceToShow}";
                priceText.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false);
            }
        }

        // Configura usos (apenas para BattleActions consumíveis)
        if (usesText != null)
        {
            if (shopItem.type == ShopItem.ItemType.BattleAction && 
                shopItem.battleAction != null && 
                shopItem.battleAction.isConsumable)
            {
                if (isForSale)
                {
                    usesText.text = $"{shopItem.battleAction.maxUses}/{shopItem.battleAction.maxUses}";
                }
                else
                {
                    usesText.text = $"{shopItem.battleAction.currentUses}/{shopItem.battleAction.maxUses}";
                }
                usesText.gameObject.SetActive(true);
            }
            else
            {
                usesText.gameObject.SetActive(false);
            }
        }
        
        // NOVO: Configura indicador de tipo
        if (typeIndicator != null && isForSale)
        {
            typeIndicator.gameObject.SetActive(true);
            typeIndicator.color = shopItem.type == ShopItem.ItemType.Powerup 
                ? powerupColor 
                : battleActionColor;
        }
        else if (typeIndicator != null)
        {
            typeIndicator.gameObject.SetActive(false);
        }
    }

    public ShopItem GetShopItem()
    {
        return shopItem;
    }

    // Compatibilidade - retorna BattleAction se for desse tipo
    public BattleAction GetAction()
    {
        return shopItem?.battleAction;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shopManager == null || shopItem == null) return;
        
        // ===== USA DESCRIÇÃO DINÂMICA PARA BATTLEACTIONS =====
        if (shopItem.type == ShopItem.ItemType.BattleAction && shopItem.battleAction != null)
        {
            string description = shopItem.battleAction.GetDynamicDescription();
            
            // Adiciona informações extras se for item à venda
            if (isForSale)
            {
                if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(displayPrice))
                {
                    description += " | <color=red>Moedas insuficientes!</color>";
                }
            }
            
            shopManager.ShowTooltip(shopItem.battleAction.actionName, description);
        }
        // ===== POWERUPS USAM DESCRIÇÃO FORMATADA =====
        else if (shopItem.type == ShopItem.ItemType.Powerup && shopItem.powerup != null)
        {
            string description = shopItem.powerup.GetFormattedDescription();
            
            // Adiciona informações extras se for item à venda
            if (isForSale)
            {
                description += "\n\n(Clique para aplicar imediatamente)";
                
                if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(displayPrice))
                {
                    description += "\n<color=red>Moedas insuficientes!</color>";
                }
            }
            
            shopManager.ShowTooltip(shopItem.powerup.powerupName, description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (shopManager != null)
        {
            shopManager.HideTooltip();
        }
    }
}