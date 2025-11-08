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
    public Image typeIndicator;
    
    [Header("Type Colors")]
    public Color battleActionColor = new Color(0.8f, 0.8f, 1f);
    public Color powerupColor = new Color(1f, 0.8f, 0.5f);

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
    /// Setup para BattleAction à venda
    /// </summary>
    public void SetupForSale(BattleAction action, ShopManager manager, int displayPrice = -1)
    {
        SetupForSale(new ShopItem(action), manager, displayPrice);
    }

    /// <summary>
    /// Setup para slots do jogador
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
        
        if (iconImage != null)
        {
            iconImage.sprite = shopItem.Icon;
            iconImage.enabled = (shopItem.Icon != null);
        }

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

    public BattleAction GetAction()
    {
        return shopItem?.battleAction;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shopManager == null || shopItem == null) return;
        
        if (shopItem.type == ShopItem.ItemType.BattleAction && shopItem.battleAction != null)
        {
            string description = shopItem.battleAction.GetDynamicDescription();
            
            if (isForSale)
            {
                description += $"\n\n<color=#FFD700>Preço: {displayPrice} moedas</color>";
                
                if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(displayPrice))
                {
                    description += "\n<color=red>Moedas insuficientes!</color>";
                }
            }
            
            shopManager.ShowTooltip(shopItem.battleAction.actionName, description);
        }
        else if (shopItem.type == ShopItem.ItemType.Powerup && shopItem.powerup != null)
        {
            string description = shopItem.powerup.description;
            
            if (isForSale)
            {
                description += $"\n\n<color=#FFD700>Preço: {displayPrice} moedas</color>";
                description += "\n(Clique para aplicar imediatamente)";
                
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