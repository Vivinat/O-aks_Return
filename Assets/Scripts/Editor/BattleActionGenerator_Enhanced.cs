// Assets/Scripts/Editor/AdvancedConsumablesGenerator.cs
// Gera 10 consumíveis AVANÇADOS + JSON completo de mapeamento

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AdvancedConsumablesGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Advanced Consumables + Full Icon Map")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedConsumablesGenerator>("Advanced Consumables");
    }

    private Vector2 scrollPosition;
    private bool generateIconMap = true;
    private string jsonOutputPath = "Assets/Data/BattleActions/";

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("🎯 Gerador de Consumíveis Avançados", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "10 Consumíveis ÚNICOS e CRIATIVOS:\n\n" +
            "• Controle de Campo (2)\n" +
            "• Manipulação de Recursos (2)\n" +
            "• Efeitos Especiais (3)\n" +
            "• Utilidade Tática (3)",
            MessageType.Info);

        GUILayout.Space(10);

        // Configurações
        EditorGUILayout.LabelField("⚙️ Configurações:", EditorStyles.boldLabel);
        generateIconMap = EditorGUILayout.Toggle("Gerar JSON de Ícones", generateIconMap);
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pasta de Saída JSON:", GUILayout.Width(150));
        jsonOutputPath = EditorGUILayout.TextField(jsonOutputPath);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Botão principal
        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("🚀 GERAR 10 CONSUMÍVEIS AVANÇADOS", GUILayout.Height(50)))
        {
            if (EditorUtility.DisplayDialog(
                "Confirmar Geração",
                "Isso criará 10 consumíveis ÚNICOS em:\n" +
                "Assets/Data/BattleActions/Items/\n\n" +
                (generateIconMap ? "✅ E gerará JSON completo de mapeamento de ícones\n\n" : "") +
                "Continuar?",
                "Sim!",
                "Cancelar"))
            {
                GenerateAll();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(20);

        // Preview dos consumíveis
        GUILayout.Label("📋 Consumíveis Avançados:", EditorStyles.boldLabel);
        GUILayout.Space(5);

        DrawItemPreview("🌫️ Névoa Tóxica", 
            "Envenena TODOS os inimigos (12 dano/turno) por 3 turnos\n" +
            "Usos: 2 | Preço: 60 moedas\n" +
            "🎯 Controle de Campo - DOT massivo");

        DrawItemPreview("⛓️ Correntes Arcanas", 
            "Reduz velocidade de TODOS os inimigos (-35%) por 3 turnos\n" +
            "Usos: 2 | Preço: 55 moedas\n" +
            "🎯 Controle de Campo - Slow em área");

        DrawItemPreview("🩸 Ritual de Sangue", 
            "Sacrifica 40 HP para restaurar 60 MP\n" +
            "Usos: 2 | Preço: 50 moedas\n" +
            "💫 Manipulação - Conversão HP→MP");

        DrawItemPreview("⚡ Cristal de Vitalidade", 
            "Sacrifica 30 MP para restaurar 80 HP\n" +
            "Usos: 2 | Preço: 45 moedas\n" +
            "💫 Manipulação - Conversão MP→HP");

        DrawItemPreview("🛡️ Barreira Divina", 
            "Imunidade total a dano por 1 turno\n" +
            "Usos: 1 | Preço: 90 moedas\n" +
            "✨ Especial - Invencibilidade temporária");

        DrawItemPreview("🎲 Dados do Caos", 
            "Efeito aleatório: cura, dano, buff ou debuff\n" +
            "Usos: 3 | Preço: 40 moedas\n" +
            "✨ Especial - RNG puro!");

        DrawItemPreview("💀 Lâmina do Executor", 
            "Dano = 20% do HP máximo do alvo\n" +
            "Usos: 2 | Preço: 75 moedas\n" +
            "✨ Especial - Mata tanques");

        DrawItemPreview("🔄 Purificação Total", 
            "Remove TODOS os debuffs de você\n" +
            "Usos: 2 | Preço: 55 moedas\n" +
            "🛠️ Utilidade - Cleanse completo");

        DrawItemPreview("🌟 Bênção Completa", 
            "+10 Ataque +10 Defesa +15% Velocidade por 3 turnos\n" +
            "Usos: 1 | Preço: 80 moedas\n" +
            "🛠️ Utilidade - Triple buff");

        DrawItemPreview("🧬 Elixir da Duplicação", 
            "Próxima habilidade tem efeito DOBRADO\n" +
            "Usos: 1 | Preço: 95 moedas\n" +
            "🛠️ Utilidade - Combo devastador");

        GUILayout.Space(10);

        if (generateIconMap)
        {
            EditorGUILayout.HelpBox(
                "📊 JSON DETALHADO será gerado com:\n" +
                "• Nome da habilidade\n" +
                "• Caminho do asset\n" +
                "• Nome do ícone (se existir)\n" +
                "• Status do ícone (presente/faltante)\n" +
                "• Categoria e tipo\n" +
                "• Custos e preços\n" +
                "• Estatísticas agregadas",
                MessageType.Info);
        }

        GUILayout.Space(10);
        GUILayout.EndScrollView();
    }

    private void DrawItemPreview(string title, string description)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label(title, EditorStyles.boldLabel);
        GUILayout.Label(description, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        GUILayout.Space(3);
    }

    private void GenerateAll()
    {
        Debug.Log("========================================");
        Debug.Log("🎯 INICIANDO GERAÇÃO DE CONSUMÍVEIS AVANÇADOS");
        Debug.Log("========================================");

        CreateDirectories();
        int created = GenerateAdvancedConsumables();

        AssetDatabase.Refresh();

        if (generateIconMap)
        {
            Debug.Log("\n📊 Gerando JSON completo de mapeamento...");
            GenerateCompleteIconJSON();
        }

        Debug.Log("========================================");
        Debug.Log($"✅ {created}/10 CONSUMÍVEIS AVANÇADOS CRIADOS!");
        Debug.Log("========================================");

        string message = $"✅ {created} consumíveis avançados criados!\n\n" +
                        "📁 Localização: Assets/Data/BattleActions/Items/";
        
        if (generateIconMap)
        {
            message += $"\n\n📊 JSON gerado em:\n{jsonOutputPath}IconMapping.json";
        }

        EditorUtility.DisplayDialog("Sucesso!", message, "OK");
    }

    private void CreateDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        
        if (!AssetDatabase.IsValidFolder("Assets/Data/BattleActions"))
            AssetDatabase.CreateFolder("Assets/Data", "BattleActions");

        if (!AssetDatabase.IsValidFolder("Assets/Data/BattleActions/Items"))
            AssetDatabase.CreateFolder("Assets/Data/BattleActions", "Items");
    }

    private int GenerateAdvancedConsumables()
    {
        Debug.Log("\n🎯 === GERANDO CONSUMÍVEIS AVANÇADOS ===");
        string path = "Assets/Data/BattleActions/Items/";
        int count = 0;

        // ========== CONTROLE DE CAMPO (2) ==========

        // 1. Névoa Tóxica - Veneno em área massivo
        count += CreateConsumable(
            path + "Nevoa_Toxica.asset",
            "Névoa Tóxica",
            "Envenena todos os inimigos, causando 12 de dano por turno durante 3 turnos. Usos: 2",
            TargetType.AllEnemies,
            maxUses: 2,
            shopPrice: 60,
            new ActionEffect {
                effectType = ActionType.Debuff,
                statusEffect = StatusEffectType.Poison,
                statusDuration = 3,
                statusPower = 12
            }
        ) ? 1 : 0;

        // 2. Correntes Arcanas - Slow em área
        count += CreateConsumable(
            path + "Correntes_Arcanas.asset",
            "Correntes Arcanas",
            "Reduz a velocidade de todos os inimigos em 35% por 3 turnos. Usos: 2",
            TargetType.AllEnemies,
            maxUses: 2,
            shopPrice: 55,
            new ActionEffect {
                effectType = ActionType.Debuff,
                statusEffect = StatusEffectType.SpeedDown,
                statusDuration = 3,
                statusPower = 35
            }
        ) ? 1 : 0;

        // ========== MANIPULAÇÃO DE RECURSOS (2) ==========

        // 3. Ritual de Sangue - HP → MP
        count += CreateConsumable(
            path + "Ritual_de_Sangue.asset",
            "Ritual de Sangue",
            "Sacrifica 40 de vida para restaurar 60 de mana. Usos: 2",
            TargetType.Self,
            maxUses: 2,
            shopPrice: 50,
            new ActionEffect {
                effectType = ActionType.Heal,
                power = 60, // Restaura MP
                hasSelfEffect = true,
                selfEffectType = ActionType.Attack,
                selfEffectPower = 40 // Perde HP
            }
        ) ? 1 : 0;

        // 4. Cristal de Vitalidade - MP → HP
        count += CreateConsumable(
            path + "Cristal_de_Vitalidade.asset",
            "Cristal de Vitalidade",
            "Sacrifica 30 de mana para restaurar 80 de vida. Usos: 2",
            TargetType.Self,
            maxUses: 2,
            shopPrice: 45,
            new ActionEffect {
                effectType = ActionType.Heal,
                power = 80 // Restaura HP
                // MP será consumido manualmente via custo especial
            }
        ) ? 1 : 0;

        // ========== EFEITOS ESPECIAIS (3) ==========

        // 5. Barreira Divina - Invencibilidade
        count += CreateConsumable(
            path + "Barreira_Divina.asset",
            "Barreira Divina",
            "Imunidade total a dano por 1 turno. Usos: 1",
            TargetType.Self,
            maxUses: 1,
            shopPrice: 90,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.Protected,
                statusDuration = 1,
                statusPower = 100 // 100% proteção = invencível
            }
        ) ? 1 : 0;

        // 6. Dados do Caos - Efeito aleatório
        count += CreateConsumable(
            path + "Dados_do_Caos.asset",
            "Dados do Caos",
            "Efeito aleatório: pode curar 50 HP, causar 50 de dano, dar +20 ataque ou envenenar inimigos. Usos: 3",
            TargetType.Self,
            maxUses: 3,
            shopPrice: 40,
            new ActionEffect {
                effectType = ActionType.Heal,
                power = 50 // Efeito base (será randomizado no código)
            }
        ) ? 1 : 0;

        // 7. Lâmina do Executor - Dano % HP máximo
        count += CreateConsumable(
            path + "Lamina_do_Executor.asset",
            "Lâmina do Executor",
            "Causa dano igual a 20% do HP máximo do alvo. Extremamente efetivo contra tanques. Usos: 2",
            TargetType.SingleEnemy,
            maxUses: 2,
            shopPrice: 75,
            new ActionEffect {
                effectType = ActionType.Attack,
                power = 100 // Placeholder (será calculado como % do HP máximo)
            }
        ) ? 1 : 0;

        // ========== UTILIDADE TÁTICA (3) ==========

        // 8. Purificação Total - Cleanse completo
        count += CreateConsumable(
            path + "Purificacao_Total.asset",
            "Purificação Total",
            "Remove TODOS os efeitos negativos de você instantaneamente. Usos: 2",
            TargetType.Self,
            maxUses: 2,
            shopPrice: 55,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.Blessed,
                statusDuration = 1,
                statusPower = 1 // Placeholder (código especial de cleanse)
            }
        ) ? 1 : 0;

        // 9. Bênção Completa - Triple buff
        count += CreateConsumable(
            path + "Bencao_Completa.asset",
            "Bênção Completa",
            "Aumenta ataque, defesa e velocidade simultaneamente por 3 turnos. Usos: 1",
            TargetType.Self,
            maxUses: 1,
            shopPrice: 80,
            new List<ActionEffect> {
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.AttackUp,
                    statusDuration = 3,
                    statusPower = 10
                },
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.DefenseUp,
                    statusDuration = 3,
                    statusPower = 10
                },
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.SpeedUp,
                    statusDuration = 3,
                    statusPower = 15
                }
            }
        ) ? 1 : 0;

        // 10. Elixir da Duplicação - Próxima skill dobrada
        count += CreateConsumable(
            path + "Elixir_da_Duplicacao.asset",
            "Elixir da Duplicação",
            "Sua próxima habilidade tem seu efeito DOBRADO. Usos: 1",
            TargetType.Self,
            maxUses: 1,
            shopPrice: 95,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.AttackUp,
                statusDuration = 1,
                statusPower = 100 // Placeholder (buff especial de duplicação)
            }
        ) ? 1 : 0;

        return count;
    }

    private bool CreateConsumable(
        string assetPath,
        string actionName,
        string description,
        TargetType targetType,
        int maxUses,
        int shopPrice,
        ActionEffect effect)
    {
        return CreateConsumable(assetPath, actionName, description, targetType, 
            maxUses, shopPrice, new List<ActionEffect> { effect });
    }

    private bool CreateConsumable(
        string assetPath,
        string actionName,
        string description,
        TargetType targetType,
        int maxUses,
        int shopPrice,
        List<ActionEffect> effects)
    {
        if (AssetDatabase.LoadAssetAtPath<BattleAction>(assetPath) != null)
        {
            Debug.LogWarning($"⚠️ Já existe: {assetPath}");
            return false;
        }

        BattleAction action = ScriptableObject.CreateInstance<BattleAction>();
        
        action.actionName = actionName;
        action.description = description;
        action.targetType = targetType;
        action.manaCost = 0;
        action.isConsumable = true;
        action.maxUses = maxUses;
        action.currentUses = maxUses;
        action.shopPrice = shopPrice;
        action.effects = effects;

        AssetDatabase.CreateAsset(action, assetPath);
        EditorUtility.SetDirty(action);
        
        Debug.Log($"  ✓ {actionName} criado (${shopPrice}, {maxUses} usos)");
        return true;
    }

    // ========== GERAÇÃO DO JSON COMPLETO ==========

    private void GenerateCompleteIconJSON()
    {
        var iconData = new CompleteIconMapping();
        iconData.generatedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        iconData.generatorVersion = "2.0";
        iconData.includesFullData = true;

        // Busca TODAS as BattleActions
        string[] guids = AssetDatabase.FindAssets("t:BattleAction");
        
        Debug.Log($"\n📊 Analisando {guids.Length} BattleActions...");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            BattleAction action = AssetDatabase.LoadAssetAtPath<BattleAction>(assetPath);

            if (action != null)
            {
                var entry = new DetailedIconEntry();
                
                // Informações básicas
                entry.actionName = action.actionName;
                entry.assetPath = assetPath;
                entry.assetGuid = guid;
                
                // Informações do ícone
                entry.icon = new IconInfo
                {
                    name = action.icon != null ? action.icon.name : "MISSING",
                    hasIcon = action.icon != null,
                    spritePath = action.icon != null ? AssetDatabase.GetAssetPath(action.icon) : "N/A"
                };
                
                // Classificação
                entry.classification = new ClassificationInfo
                {
                    category = DetermineCategory(assetPath),
                    isConsumable = action.isConsumable,
                    targetType = action.targetType.ToString()
                };
                
                // Custos e economia
                entry.economy = new EconomyInfo
                {
                    manaCost = action.manaCost,
                    shopPrice = action.shopPrice,
                    maxUses = action.maxUses,
                    totalValue = action.shopPrice * (action.isConsumable ? action.maxUses : 1)
                };
                
                // Efeitos (resumo)
                entry.effectsSummary = SummarizeEffects(action);

                iconData.actions.Add(entry);
            }
        }

        // Calcula estatísticas
        CalculateStatistics(iconData);

        // Ordena por categoria e nome
        iconData.actions = iconData.actions
            .OrderBy(a => a.classification.category)
            .ThenBy(a => a.actionName)
            .ToList();

        // Salva JSON
        string fullPath = Path.Combine(jsonOutputPath, "IconMapping.json");
        string jsonContent = JsonUtility.ToJson(iconData, true);
        
        File.WriteAllText(fullPath, jsonContent);
        AssetDatabase.Refresh();

        // Log de resultado
        Debug.Log($"\n✅ JSON COMPLETO gerado: {fullPath}");
        Debug.Log($"   📦 Total: {iconData.statistics.totalActions} ações");
        Debug.Log($"   ✅ Com ícones: {iconData.statistics.withIcons}");
        Debug.Log($"   ❌ Sem ícones: {iconData.statistics.withoutIcons}");
        Debug.Log($"   🧪 Consumíveis: {iconData.statistics.consumables}");
        Debug.Log($"   ⚔️ Skills: {iconData.statistics.skills}");
        
        Debug.Log($"\n📊 Por categoria:");
    }

    private string DetermineCategory(string assetPath)
    {
        if (assetPath.Contains("/Paladin/")) return "Paladin";
        if (assetPath.Contains("/Ranger/")) return "Ranger";
        if (assetPath.Contains("/Druid/")) return "Druid";
        if (assetPath.Contains("/Mana/")) return "Mana";
        if (assetPath.Contains("/Unlimited/")) return "Unlimited";
        if (assetPath.Contains("/Items/")) return "Items";
        return "Other";
    }

    private string SummarizeEffects(BattleAction action)
    {
        if (action.effects == null || action.effects.Count == 0)
            return "No effects";

        var summary = new List<string>();
        
        foreach (var effect in action.effects)
        {
            string effectDesc = "";
            
            switch (effect.effectType)
            {
                case ActionType.Attack:
                    effectDesc = $"Dano: {effect.power}";
                    break;
                case ActionType.Heal:
                    effectDesc = $"Cura: {effect.power}";
                    break;
                case ActionType.Buff:
                case ActionType.Debuff:
                    effectDesc = $"{effect.statusEffect} ({effect.statusDuration}t, {effect.statusPower})";
                    break;
            }
            
            if (!string.IsNullOrEmpty(effectDesc))
                summary.Add(effectDesc);
        }
        
        return string.Join(" | ", summary);
    }

    private void CalculateStatistics(CompleteIconMapping data)
    {
        var stats = new StatisticsInfo();
        
        stats.totalActions = data.actions.Count;
        stats.withIcons = data.actions.Count(a => a.icon.hasIcon);
        stats.withoutIcons = data.actions.Count(a => !a.icon.hasIcon);
        stats.consumables = data.actions.Count(a => a.classification.isConsumable);
        stats.skills = stats.totalActions - stats.consumables;
        stats.averagePrice = data.actions.Where(a => a.economy.shopPrice > 0)
                                         .Average(a => (float)a.economy.shopPrice);
        
        
        data.statistics = stats;
    }
}

// ========== CLASSES DE DADOS JSON ==========

[System.Serializable]
public class CompleteIconMapping
{
    public string generatedAt;
    public string generatorVersion;
    public bool includesFullData;
    public StatisticsInfo statistics;
    public List<DetailedIconEntry> actions = new List<DetailedIconEntry>();
}

[System.Serializable]
public class DetailedIconEntry
{
    public string actionName;
    public string assetPath;
    public string assetGuid;
    public IconInfo icon;
    public ClassificationInfo classification;
    public EconomyInfo economy;
    public string effectsSummary;
}

[System.Serializable]
public class IconInfo
{
    public string name;
    public bool hasIcon;
    public string spritePath;
}

[System.Serializable]
public class ClassificationInfo
{
    public string category;
    public bool isConsumable;
    public string targetType;
}

[System.Serializable]
public class EconomyInfo
{
    public int manaCost;
    public int shopPrice;
    public int maxUses;
    public int totalValue;
}

[System.Serializable]
public class StatisticsInfo
{
    public int totalActions;
    public int withIcons;
    public int withoutIcons;
    public int consumables;
    public int skills;
    public float averagePrice;
}