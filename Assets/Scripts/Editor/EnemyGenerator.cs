// Assets/Scripts/Editor/EnemyGenerator.cs

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemyGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Enemy Characters")]
    public static void ShowWindow()
    {
        GetWindow<EnemyGenerator>("Enemy Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Gerador de Inimigos", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Gerar 30 Inimigos + 3 Chefes", GUILayout.Height(40)))
        {
            GenerateAllEnemies();
        }

        GUILayout.Space(10);
        GUILayout.Label("Isso criará:", EditorStyles.helpBox);
        GUILayout.Label("• 10 Druidas/Rangers");
        GUILayout.Label("• 10 Paladinos/Guerreiros");
        GUILayout.Label("• 10 Monstruosidades");
        GUILayout.Label("• 3 Chefes: Mawron, Valdemor, Fentho");
        GUILayout.Label("");
        GUILayout.Label("Local: Assets/Data/Characters/Enemies/");
    }

    void GenerateAllEnemies()
    {
        CreateDirectories();
        
        // Gera inimigos normais
        GenerateDruidRangers();
        GeneratePaladinWarriors();
        GenerateMonsters();
        
        // Gera chefes
        GenerateBosses();

        AssetDatabase.Refresh();
        Debug.Log("✅ 33 Inimigos gerados com sucesso! (30 normais + 3 chefes)");
    }

    void CreateDirectories()
    {
        string basePath = "Assets/Data/Characters";
        
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/Data/Characters"))
                AssetDatabase.CreateFolder("Assets/Data", "Characters");
        }

        if (!AssetDatabase.IsValidFolder(basePath + "/Enemies"))
            AssetDatabase.CreateFolder(basePath, "Enemies");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Enemies/Druids"))
            AssetDatabase.CreateFolder(basePath + "/Enemies", "Druids");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Enemies/Warriors"))
            AssetDatabase.CreateFolder(basePath + "/Enemies", "Warriors");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Enemies/Monsters"))
            AssetDatabase.CreateFolder(basePath + "/Enemies", "Monsters");
        
        if (!AssetDatabase.IsValidFolder(basePath + "/Enemies/Bosses"))
            AssetDatabase.CreateFolder(basePath + "/Enemies", "Bosses");
    }

    // ========== DRUIDAS E RANGERS ==========
    void GenerateDruidRangers()
    {
        string path = "Assets/Data/Characters/Enemies/Druids/";
        
        string[] names = {
            "Druida", "Ranger", "Xamã da Floresta",
            "Caçador", "Druida Corrupto", "Arqueiro Sombrio",
            "Guardião da Mata", "Rastreador Feroz", "Druida Ancião",
            "Atirador de Elite"
        };

        for (int i = 0; i < names.Length; i++)
        {
            CreateEnemy(
                path + names[i].Replace(" ", "_") + ".asset",
                names[i],
                hp: Random.Range(60, 90),
                mp: Random.Range(40, 60),
                defense: Random.Range(5, 12),
                speed: Random.Range(1.2f, 1.6f),
                GetDruidActions()
            );
        }
    }

    List<string> GetDruidActions()
    {
        return new List<string>
        {
            "Assets/Data/BattleActions/Druid/Espinhos.asset",
            "Assets/Data/BattleActions/Druid/Raizes_Estranguladoras.asset",
            "Assets/Data/BattleActions/Druid/Enxame_de_Insetos.asset"
        };
    }

    // ========== PALADINOS E GUERREIROS ==========
    void GeneratePaladinWarriors()
    {
        string path = "Assets/Data/Characters/Enemies/Warriors/";
        
        string[] names = {
            "Paladino", "Escudeiro", "Inquisidor",
            "Soldado", "Cruzado", "Templário",
            "Gladiador", "Campeão", "Sentinela",
            "Guardião"
        };

        for (int i = 0; i < names.Length; i++)
        {
            CreateEnemy(
                path + names[i].Replace(" ", "_") + ".asset",
                names[i],
                hp: Random.Range(80, 120),
                mp: Random.Range(30, 50),
                defense: Random.Range(12, 20),
                speed: Random.Range(0.8f, 1.2f),
                GetWarriorActions()
            );
        }
    }

    List<string> GetWarriorActions()
    {
        return new List<string>
        {
            "Assets/Data/BattleActions/Paladin/Golpe_Divino.asset",
            "Assets/Data/BattleActions/Paladin/Julgamento.asset",
            "Assets/Data/BattleActions/Paladin/Luz_Sagrada.asset"
        };
    }

    // ========== MONSTRUOSIDADES ==========
    void GenerateMonsters()
    {
        string path = "Assets/Data/Characters/Enemies/Monsters/";
        
        string[] names = {
            "Servo", "Monstruosidade", "Aberração",
            "Demônio", "Demônio Menor"
        };

        for (int i = 0; i < names.Length; i++)
        {
            CreateEnemy(
                path + names[i].Replace(" ", "_") + ".asset",
                names[i],
                hp: Random.Range(70, 110),
                mp: Random.Range(20, 40),
                defense: Random.Range(8, 15),
                speed: Random.Range(0.9f, 1.4f),
                GetMonsterActions()
            );
        }
    }

    List<string> GetMonsterActions()
    {
        return new List<string>
        {
            "Assets/Data/BattleActions/Druid/Espinhos.asset",
            "Assets/Data/BattleActions/Ranger/Tiro_Preciso.asset",
            "Assets/Data/BattleActions/Paladin/Golpe_Divino.asset"
        };
    }

    // ========== CHEFES ==========
    void GenerateBosses()
    {
        string path = "Assets/Data/Characters/Enemies/Bosses/";

        // MAWRON - Chefe Guerreiro Brutal
        CreateEnemy(
            path + "Mawron.asset",
            "Mawron, o Senhor do Escuro",
            hp: 300,
            mp: 80,
            defense: 25,
            speed: 1.1f,
            new List<string>
            {
                "Assets/Data/BattleActions/Paladin/Golpe_Divino.asset",
                "Assets/Data/BattleActions/Paladin/Julgamento.asset",
                "Assets/Data/BattleActions/Paladin/Retribuicao_Divina.asset",
                "Assets/Data/BattleActions/Paladin/Escudo_da_Fe.asset"
            }
        );

        // VALDEMOR - Chefe Mago das Sombras
        CreateEnemy(
            path + "Valdemor.asset",
            "Valdemor, Senhor das Trevas",
            hp: 250,
            mp: 150,
            defense: 15,
            speed: 1.3f,
            new List<string>
            {
                "Assets/Data/BattleActions/Druid/Espinhos.asset",
                "Assets/Data/BattleActions/Druid/Raizes_Estranguladoras.asset",
                "Assets/Data/BattleActions/Druid/Enxame_de_Insetos.asset",
                "Assets/Data/BattleActions/Druid/Tranquilidade.asset"
            }
        );

        // FENTHO - Chefe Besta Colossal
        CreateEnemy(
            path + "Fentho.asset",
            "Fentho, o Sonhador",
            hp: 400,
            mp: 60,
            defense: 30,
            speed: 0.9f,
            new List<string>
            {
                "Assets/Data/BattleActions/Paladin/Golpe_Divino.asset",
                "Assets/Data/BattleActions/Ranger/Chuva_de_Flechas.asset",
                "Assets/Data/BattleActions/Druid/Enxame_de_Insetos.asset",
                "Assets/Data/BattleActions/Paladin/Retribuicao_Divina.asset"
            }
        );

        Debug.Log("🔥 Chefes criados: Mawron, Valdemor e Fentho!");
    }

    // ========== HELPER METHODS ==========
    void CreateEnemy(string assetPath, string enemyName, int hp, int mp, int defense, float speed, List<string> actionPaths)
    {
        Character enemy = ScriptableObject.CreateInstance<Character>();
        
        enemy.characterName = enemyName;
        enemy.team = Team.Enemy;
        enemy.maxHp = hp;
        enemy.maxMp = mp;
        enemy.defense = defense;
        enemy.speed = speed;
        enemy.battleActions = new List<BattleAction>();

        // Carrega as ações
        foreach (string actionPath in actionPaths)
        {
            BattleAction action = AssetDatabase.LoadAssetAtPath<BattleAction>(actionPath);
            if (action != null)
            {
                enemy.battleActions.Add(action);
            }
            else
            {
                Debug.LogWarning($"Ação não encontrada: {actionPath}");
            }
        }

        AssetDatabase.CreateAsset(enemy, assetPath);
        EditorUtility.SetDirty(enemy);
        
        Debug.Log($"✅ Criado: {enemyName} (HP:{hp}, MP:{mp}, DEF:{defense}, SPD:{speed:F1})");
    }
}