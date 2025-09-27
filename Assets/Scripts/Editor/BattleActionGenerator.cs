// Assets/Scripts/Editor/PaladinRangerDruidActionGenerator.cs

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BattleActionGenerator : EditorWindow
{
    [MenuItem("Tools/Generate PRD Battle Actions")]
    public static void ShowWindow()
    {
        GetWindow<BattleActionGenerator>("PRD Action Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Gerador de Ações (Paladino/Ranger/Druida)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Gerar Habilidades de Classes", GUILayout.Height(30)))
        {
            GenerateAllActions();
        }

        GUILayout.Space(10);
        GUILayout.Label("Isso criará 21 ações de batalha temáticas:", EditorStyles.helpBox);
        GUILayout.Label("• Assets/Data/BattleActions/Paladin");
        GUILayout.Label("• Assets/Data/BattleActions/Ranger");
        GUILayout.Label("• Assets/Data/BattleActions/Druid");
    }

    void GenerateAllActions()
    {
        CreateDirectories();
        GeneratePaladinActions();
        GenerateRangerActions();
        GenerateDruidActions();

        AssetDatabase.Refresh();
        Debug.Log("21 Ações de Batalha (Paladino, Ranger, Druida) geradas com sucesso!");
    }

    void CreateDirectories()
    {
        string basePath = "Assets/Data/BattleActions";
        
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder("Assets/Data", "BattleActions");
        }

        if (!AssetDatabase.IsValidFolder(basePath + "/Paladin"))
            AssetDatabase.CreateFolder(basePath, "Paladin");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Ranger"))
            AssetDatabase.CreateFolder(basePath, "Ranger");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Druid"))
            AssetDatabase.CreateFolder(basePath, "Druid");
    }

    void GeneratePaladinActions()
    {
        string path = "Assets/Data/BattleActions/Paladin/";

        // Habilidades Ofensivas
        CreateBattleAction(path + "Golpe_Divino.asset",
            "Golpe Divino", "Causa 35 de dano sagrado. Custo: 0 MP",
            TargetType.SingleEnemy, 0,
            new ActionEffect { effectType = ActionType.Attack, power = 35 });

        CreateBattleAction(path + "Julgamento.asset",
            "Julgamento", "Causa 60 de dano e deixa o alvo Vulnerável por 2 turnos. Custo: 15 MP",
            TargetType.SingleEnemy, 15,
            new ActionEffect { 
                effectType = ActionType.Attack, 
                power = 60,
                statusEffect = StatusEffectType.Vulnerable,
                statusDuration = 2,
                statusPower = 15 // Alvo toma 15% a mais de dano
            });

        CreateBattleAction(path + "Retribuicao_Divina.asset",
            "Retribuição Divina", "Causa 130 de dano massivo, mas sacrifica 25 de sua vida. Custo: 30 MP",
            TargetType.SingleEnemy, 30,
            new ActionEffect {
                effectType = ActionType.Attack,
                power = 130,
                hasSelfEffect = true,
                selfEffectType = ActionType.Attack, // Dano a si mesmo
                selfEffectPower = 25
            });

        // Habilidades de Suporte e Cura
        CreateBattleAction(path + "Luz_Sagrada.asset",
            "Luz Sagrada", "Cura 50 de vida de um aliado. Custo: 10 MP",
            TargetType.SingleAlly, 10,
            new ActionEffect { effectType = ActionType.Heal, power = 50 });

        CreateBattleAction(path + "Bencao_dos_Reis.asset",
            "Bênção dos Reis", "Aumenta o ataque e a defesa de um aliado por 4 turnos. Custo: 18 MP",
            TargetType.SingleAlly, 18,
            new List<ActionEffect> {
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.AttackUp,
                    statusDuration = 4,
                    statusPower = 10
                },
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.DefenseUp,
                    statusDuration = 4,
                    statusPower = 10
                }
            });

        CreateBattleAction(path + "Escudo_da_Fe.asset",
            "Escudo da Fé", "Aplica 'Protegido' em si mesmo, reduzindo o dano recebido por 3 turnos. Custo: 12 MP",
            TargetType.Self, 12,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.Protected,
                statusDuration = 3,
                statusPower = 25 // Reduz 25% do dano
            });
            
        CreateBattleAction(path + "Aura_da_Devocao.asset",
            "Aura da Devoção", "Cura 25 de vida de todos os aliados. Custo: 20 MP",
            TargetType.AllAllies, 20,
            new ActionEffect { effectType = ActionType.Heal, power = 25 });
    }

    void GenerateRangerActions()
    {
        string path = "Assets/Data/BattleActions/Ranger/";

        // Habilidades Ofensivas
        CreateBattleAction(path + "Tiro_Preciso.asset",
            "Tiro Preciso", "Um tiro rápido que causa 30 de dano. Custo: 0 MP",
            TargetType.SingleEnemy, 0,
            new ActionEffect { effectType = ActionType.Attack, power = 30 });

        CreateBattleAction(path + "Flecha_Venenosa.asset",
            "Flecha Venenosa", "Causa 20 de dano e envenena o alvo por 4 turnos. Custo: 10 MP",
            TargetType.SingleEnemy, 10,
            new ActionEffect {
                effectType = ActionType.Attack,
                power = 20,
                statusEffect = StatusEffectType.Poison,
                statusDuration = 4,
                statusPower = 10 // 10 de dano de veneno por turno
            });
            
        CreateBattleAction(path + "Chuva_de_Flechas.asset",
            "Chuva de Flechas", "Causa 30 de dano a todos os inimigos. Custo: 22 MP",
            TargetType.AllEnemies, 22,
            new ActionEffect { effectType = ActionType.Attack, power = 30 });

        CreateBattleAction(path + "Tiro_Incapacitante.asset",
            "Tiro Incapacitante", "Causa 25 de dano e reduz a velocidade do alvo por 3 turnos. Custo: 12 MP",
            TargetType.SingleEnemy, 12,
            new ActionEffect {
                effectType = ActionType.Attack,
                power = 25,
                statusEffect = StatusEffectType.SpeedDown,
                statusDuration = 3,
                statusPower = 20
            });

        // Habilidades de Suporte e Buffs
        CreateBattleAction(path + "Olho_de_Aguia.asset",
            "Olho de Águia", "Aumenta seu próprio ataque por 3 turnos. Custo: 8 MP",
            TargetType.Self, 8,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.AttackUp,
                statusDuration = 3,
                statusPower = 20
            });

        CreateBattleAction(path + "Primeiros_Socorros.asset",
            "Primeiros Socorros", "Restaura 30 de sua própria vida. Custo: 5 MP",
            TargetType.Self, 5,
            new ActionEffect { effectType = ActionType.Heal, power = 30 });
            
        CreateBattleAction(path + "Marca_do_Cacador.asset",
            "Marca do Caçador", "Marca um inimigo, reduzindo sua defesa por 3 turnos. Custo: 10 MP",
            TargetType.SingleEnemy, 10,
            new ActionEffect {
                effectType = ActionType.Debuff,
                statusEffect = StatusEffectType.DefenseDown,
                statusDuration = 3,
                statusPower = 15
            });
    }

    void GenerateDruidActions()
    {
        string path = "Assets/Data/BattleActions/Druid/";

        // Habilidades de Dano e Debuff
        CreateBattleAction(path + "Espinhos.asset",
            "Espinhos", "Causa 28 de dano perfurante. Custo: 0 MP",
            TargetType.SingleEnemy, 0,
            new ActionEffect { effectType = ActionType.Attack, power = 28 });
            
        CreateBattleAction(path + "Raizes_Estranguladoras.asset",
            "Raízes Estranguladoras", "Causa 15 de dano e reduz a velocidade do alvo por 4 turnos. Custo: 12 MP",
            TargetType.SingleEnemy, 12,
            new ActionEffect {
                effectType = ActionType.Attack,
                power = 15,
                statusEffect = StatusEffectType.SpeedDown,
                statusDuration = 4,
                statusPower = 25
            });

        CreateBattleAction(path + "Enxame_de_Insetos.asset",
            "Enxame de Insetos", "Amaldiçoa todos os inimigos, causando dano por 3 turnos. Custo: 18 MP",
            TargetType.AllEnemies, 18,
            new ActionEffect {
                effectType = ActionType.Debuff,
                statusEffect = StatusEffectType.Cursed,
                statusDuration = 3,
                statusPower = 12 // 12 de dano por turno
            });

        // Habilidades de Cura e Suporte
        CreateBattleAction(path + "Toque_Restaurador.asset",
            "Toque Restaurador", "Cura 45 de vida de um alvo. Custo: 9 MP",
            TargetType.SingleAlly, 9,
            new ActionEffect { effectType = ActionType.Heal, power = 45 });
            
        CreateBattleAction(path + "Semente_da_Vida.asset",
            "Semente da Vida", "Aplica 'Abençoado' a um aliado, curando-o por 4 turnos. Custo: 15 MP",
            TargetType.SingleAlly, 15,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.Blessed,
                statusDuration = 4,
                statusPower = 15 // 15 de cura por turno
            });
        
        CreateBattleAction(path + "Pele_de_Casca.asset",
            "Pele de Casca", "Aumenta drasticamente a defesa de um aliado por 3 turnos. Custo: 14 MP",
            TargetType.SingleAlly, 14,
            new ActionEffect {
                effectType = ActionType.Buff,
                statusEffect = StatusEffectType.DefenseUp,
                statusDuration = 3,
                statusPower = 25
            });

        CreateBattleAction(path + "Tranquilidade.asset",
            "Tranquilidade", "Cura 20 de vida e aplica Regeneração a todos os aliados por 2 turnos. Custo: 28 MP",
            TargetType.AllAllies, 28,
            new List<ActionEffect> {
                new ActionEffect {
                    effectType = ActionType.Heal,
                    power = 20
                },
                new ActionEffect {
                    effectType = ActionType.Buff,
                    statusEffect = StatusEffectType.Regeneration,
                    statusDuration = 2,
                    statusPower = 10
                }
            });
    }

    // Helper method for single effect actions
    void CreateBattleAction(string assetPath, string actionName, string description, 
                           TargetType targetType, int manaCost, ActionEffect effect)
    {
        CreateBattleAction(assetPath, actionName, description, targetType, manaCost, new List<ActionEffect> { effect });
    }

    // Main helper method for potentially multiple effects
    void CreateBattleAction(string assetPath, string actionName, string description, 
                           TargetType targetType, int manaCost, List<ActionEffect> effects)
    {
        BattleAction action = ScriptableObject.CreateInstance<BattleAction>();
        
        action.actionName = actionName;
        action.description = description;
        action.targetType = targetType;
        action.manaCost = manaCost;
        action.isConsumable = false;

        action.effects = effects;

        AssetDatabase.CreateAsset(action, assetPath);
        EditorUtility.SetDirty(action);
    }
}