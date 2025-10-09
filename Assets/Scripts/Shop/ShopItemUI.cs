// Assets/Scripts/UI/ShopItemUI.cs (Simplificado - sem estado vendido)

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI usesText; // Para mostrar usos de consumíveis

    private BattleAction actionData;
    private ShopManager shopManager;
    private bool isForSale; // Se true, mostra preço. Se false, é slot do jogador
    private int displayPrice = 0;


    /// <summary>
    /// Setup para itens à venda (mostra preço)
    /// </summary>
    public void SetupForSale(BattleAction action, ShopManager manager, int displayPrice = -1)
    {
        this.actionData = action;
        this.shopManager = manager;
        this.isForSale = true;
    
        // NOVO: Usa preço modificado se fornecido
        this.displayPrice = displayPrice > 0 ? displayPrice : action.shopPrice;
    
        SetupUI();
    }

    /// <summary>
    /// Setup para slots do jogador (não mostra preço)
    /// </summary>
    public void SetupPlayerSlot(BattleAction action, ShopManager manager)
    {
        this.actionData = action;
        this.shopManager = manager;
        this.isForSale = false;
        SetupUI();
    }

    private void SetupUI()
    {
        // Configura o ícone
        if (iconImage != null && actionData != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = (actionData.icon != null);
        }

        // Configura o preço (apenas para itens à venda)
        if (priceText != null)
        {
            if (isForSale && actionData != null)
            {
                int priceToShow = displayPrice > 0 ? displayPrice : actionData.shopPrice;
                priceText.text = $"{priceToShow}";
                priceText.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false);
            }
        }

        // Configura usos se for consumível
        if (usesText != null && actionData != null)
        {
            if (actionData.isConsumable)
            {
                if (isForSale)
                {
                    // Para itens à venda, mostra usos máximos
                    usesText.text = $"{actionData.maxUses}/{actionData.maxUses}";
                }
                else
                {
                    // Para slots do jogador, mostra usos atuais
                    usesText.text = $"{actionData.currentUses}/{actionData.maxUses}";
                }
                usesText.gameObject.SetActive(true);
            }
            else
            {
                usesText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Retorna a BattleAction associada a este botão
    /// </summary>
    public BattleAction GetAction()
    {
        return actionData;
    }

    // Quando o mouse entra na área do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (shopManager != null && actionData != null)
        {
            string description = actionData.description;
            
            // Adiciona informações extras para consumíveis
            if (actionData.isConsumable)
            {
                if (isForSale)
                {
                    // Para itens à venda, mostra usos máximos
                    description += $"\n\nUsos: {actionData.maxUses}";
                    description += "\n(Consumível - será removido quando esgotado)";
                }
                else
                {
                    // Para slots do jogador, mostra usos atuais
                    description += $"\n\nUsos: {actionData.currentUses}/{actionData.maxUses}";
                    description += "\n(Consumível - será removido quando esgotado)";
                }
            }
            else if (actionData.manaCost > 0)
            {
                description += $"\n\nCusto de MP: {actionData.manaCost}";
            }
            
            // Adiciona preço apenas para itens à venda
            if (isForSale && actionData.shopPrice > 0)
            {
                if (GameManager.Instance.CurrencySystem.HasEnoughCoins(actionData.shopPrice))
                {
                    description += $"\n\nPreço: {actionData.shopPrice} moedas";
                }
                else
                {
                    description += $"\n\nPreço: {actionData.shopPrice} moedas (Moedas insuficientes!)";
                }
            }
            
            shopManager.ShowTooltip(actionData.actionName, description);
        }
    }

    // Quando o mouse sai da área do botão
    public void OnPointerExit(PointerEventData eventData)
    {
        if (shopManager != null)
        {
            shopManager.HideTooltip();
        }
    }
}