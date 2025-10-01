// Assets/Scripts/Tutorial/TutorialManager.cs (VERSÃO COM DEBUG COMPLETO)

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Gerencia os tutoriais do jogo, mostrando-os apenas na primeira vez
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableTutorials = true;
    [SerializeField] private float delayBeforeTutorial = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // NOVO: Controle de logs
    
    // PlayerPrefs keys para rastrear tutoriais mostrados
    private const string TUTORIAL_PREFIX = "Tutorial_";
    
    // Flag para evitar múltiplos tutoriais na mesma cena
    private bool tutorialShownThisScene = false;

    void Awake()
    {
        DebugLog("=== AWAKE CHAMADO ===");
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("✅ TutorialManager Instance criada e configurada como DontDestroyOnLoad");
        }
        else
        {
            DebugLog("⚠️ Outra instância de TutorialManager já existe - destruindo este objeto");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DebugLog("=== START CHAMADO ===");
        DebugLog($"Tutoriais habilitados: {enableTutorials}");
        DebugLog($"Delay antes do tutorial: {delayBeforeTutorial}s");
        
        // Registra o evento de cena carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
        DebugLog("✅ Evento SceneManager.sceneLoaded registrado");
        
        // NOVO: Tenta mostrar tutorial da cena atual se acabou de ser criado
        string currentScene = SceneManager.GetActiveScene().name;
        DebugLog($"Cena atual no Start: {currentScene}");
        
        // Se estamos em uma cena que precisa de tutorial, inicia
        if (!string.IsNullOrEmpty(currentScene))
        {
            DebugLog("Iniciando verificação de tutorial para cena atual...");
            StartCoroutine(ShowTutorialAfterDelay(currentScene));
        }
    }

    void OnDestroy()
    {
        DebugLog("=== ONDESTROY CHAMADO ===");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugLog($"=== ONSCENELOADED DISPARADO ===");
        DebugLog($"Cena carregada: {scene.name}");
        DebugLog($"Modo de carregamento: {mode}");
        
        tutorialShownThisScene = false;
        
        if (!enableTutorials)
        {
            DebugLog("⚠️ Tutoriais DESABILITADOS - pulando");
            return;
        }

        DebugLog($"Iniciando corrotina com delay de {delayBeforeTutorial}s");
        StartCoroutine(ShowTutorialAfterDelay(scene.name));
    }

    private System.Collections.IEnumerator ShowTutorialAfterDelay(string sceneName)
    {
        DebugLog($"=== CORROTINA INICIADA ===");
        DebugLog($"Aguardando {delayBeforeTutorial} segundos...");
        
        yield return new WaitForSeconds(delayBeforeTutorial);
        
        DebugLog($"Delay completado! tutorialShownThisScene = {tutorialShownThisScene}");
        
        if (!tutorialShownThisScene)
        {
            DebugLog("Chamando CheckAndShowTutorial...");
            CheckAndShowTutorial(sceneName);
        }
        else
        {
            DebugLog("⚠️ Tutorial já foi mostrado nesta cena - pulando");
        }
    }

    /// <summary>
    /// Verifica se deve mostrar tutorial e o mostra se necessário
    /// </summary>
    private void CheckAndShowTutorial(string sceneName)
    {
        DebugLog($"=== CHECK AND SHOW TUTORIAL ===");
        DebugLog($"Nome da cena recebido: {sceneName}");
        
        // Normaliza o nome da cena (remove números de variações)
        string tutorialKey = GetTutorialKey(sceneName);
        
        DebugLog($"Tutorial key gerada: {tutorialKey}");
        
        if (ShouldShowTutorial(tutorialKey))
        {
            DebugLog($"✅ Tutorial '{tutorialKey}' DEVE ser mostrado");
            ShowTutorialForScene(sceneName, tutorialKey);
        }
        else
        {
            DebugLog($"⚠️ Tutorial '{tutorialKey}' JÁ foi mostrado anteriormente");
        }
    }

    /// <summary>
    /// Gera a chave do tutorial baseada no nome da cena
    /// </summary>
    private string GetTutorialKey(string sceneName)
    {
        DebugLog($"=== GET TUTORIAL KEY ===");
        DebugLog($"Nome da cena: {sceneName}");
        
        string key = sceneName;
        
        if (sceneName.StartsWith("Map"))
        {
            key = "Map";
            DebugLog("Tipo detectado: MAP");
        }
        else if (sceneName.ToLower().Contains("battle"))
        {
            key = "Battle";
            DebugLog("Tipo detectado: BATTLE");
        }
        else if (sceneName.ToLower().Contains("negotiation"))
        {
            key = "Negotiation";
            DebugLog("Tipo detectado: NEGOTIATION");
        }
        else if (sceneName.ToLower().Contains("shop"))
        {
            key = "Shop";
            DebugLog("Tipo detectado: SHOP");
        }
        else if (sceneName.ToLower().Contains("skill"))
        {
            key = "Skill";
            DebugLog("Tipo detectado: SKILL");
        }
        else
        {
            DebugLog($"⚠️ Tipo NÃO RECONHECIDO - usando nome completo: {key}");
        }
            
        return key;
    }

    /// <summary>
    /// Verifica se o tutorial deve ser mostrado
    /// </summary>
    private bool ShouldShowTutorial(string tutorialKey)
    {
        string prefKey = TUTORIAL_PREFIX + tutorialKey;
        int wasShown = PlayerPrefs.GetInt(prefKey, 0);
        
        DebugLog($"=== SHOULD SHOW TUTORIAL ===");
        DebugLog($"Chave PlayerPrefs: {prefKey}");
        DebugLog($"Valor salvo: {wasShown} (0 = nunca mostrado, 1 = já mostrado)");
        
        return wasShown == 0;
    }

    /// <summary>
    /// Marca o tutorial como mostrado
    /// </summary>
    private void MarkTutorialAsShown(string tutorialKey)
    {
        string prefKey = TUTORIAL_PREFIX + tutorialKey;
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();
        
        DebugLog($"=== TUTORIAL MARCADO COMO MOSTRADO ===");
        DebugLog($"Chave: {prefKey}");
    }

    /// <summary>
    /// Mostra o tutorial apropriado para a cena
    /// </summary>
    private void ShowTutorialForScene(string sceneName, string tutorialKey)
    {
        DebugLog($"=== SHOW TUTORIAL FOR SCENE ===");
        DebugLog($"Scene: {sceneName}, Key: {tutorialKey}");
        
        tutorialShownThisScene = true;
        
        // NOVO: Verifica se o DialogueManager existe
        if (DialogueManager.Instance == null)
        {
            DebugError("❌ DIALOGUEMANAGER.INSTANCE É NULL!");
            DebugError("O DialogueManager precisa estar na cena ou ser DontDestroyOnLoad!");
            return;
        }
        else
        {
            DebugLog("✅ DialogueManager encontrado!");
        }

        switch (tutorialKey)
        {
            case "Map":
                DebugLog("Mostrando tutorial de MAPA");
                ShowMapTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Battle":
                DebugLog("Mostrando tutorial de BATALHA");
                ShowBattleTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Negotiation":
                DebugLog("Mostrando tutorial de NEGOCIAÇÃO");
                ShowNegotiationTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Shop":
                DebugLog("Mostrando tutorial de LOJA");
                ShowShopTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Skill":
                DebugLog("Mostrando tutorial de TESOURO");
                ShowTreasureTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            default:
                DebugLog($"⚠️ Nenhum tutorial configurado para: {tutorialKey}");
                break;
        }
    }

    #region Tutorial Definitions

    /// <summary>
    /// Tutorial do Mapa
    /// </summary>
    private void ShowMapTutorial(System.Action onComplete)
    {
        DebugLog("=== CONSTRUINDO TUTORIAL DE MAPA ===");
        
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Há muito tempo atrás, o arquidemônio Logrif, desgraçado pelo próprio universo, traído pelos seus aliados foi selado.")  
            .AddNarration(
                "O mundo dos humanos o esqueceu, mas ele seguiu trancafiado. Até o dia em que o caos que assola o mundo finalmente rompeu seu selo...")  
            .AddLine("Logrif",
                "Humph. O mundo mudou muito desde minha última aparição. Quando foi que meus dominíos ficaram tão enxutos assim?!")
            .AddNarration(
                "Mal sabe o arquidemônio que outras forças agora travam guerra contra os humanos. O território que um dia lhe pertenceu agora é posse de outro Senhor do Escuro...")
            .AddLine("Logrif",
                "Carcaça e Carnificina! E quem são esses moleques?! Tudo isso era mato antes de eu chegar aqui!")
            .AddNarration(
                "Ao norte, os patrulheiros e druidas enfrentam as forças de um Senhor do Escuro. Flecha e fogo rugem pela mata")
            .AddLine("Logrif", "Homem ou demônio, pouco importa! Todos morrerão!")
            .AddNarration("Avance sob os pontos em marrom para prosseguir")
            .AddNarration("Nós vermelhos estão trancados e nós escuros já foram completados.")
            .AddNarration("Arraste a tela segurando o botão esquerdo do mouse para explorar o mapa.")
            .AddNarration("Pressione 'E' a qualquer momento para abrir o Menu de Status e ver suas ações de batalha.")
            .AddNarration("Pressione 'ESC' para abrir o Menu de Opções.");
        
        DebugLog("✅ Tutorial construído, chamando Show()...");
        tutorial.Show(onComplete);
    }

    /// <summary>
    /// Tutorial de Batalha
    /// </summary>
    private void ShowBattleTutorial(System.Action onComplete)
    {
        DebugLog("=== CONSTRUINDO TUTORIAL DE BATALHA ===");
        
        var tutorial = DialogueUtils.CreateBuilder()
            .AddLine("Logrif", "VOCÊS SE ATREVEM?! Vão todos morrer!")
            .AddNarration("Inimigos se colocam diante do arquidemônio.")
            .AddNarration("O sistema de batalha usa ATB (Active Time Battle). Cada personagem tem uma barra que enche ao longo do tempo.")
            .AddNarration("Quando sua barra estiver cheia, você poderá escolher uma ação.")
            .AddNarration("Selecione uma habilidade dos botões disponíveis na parte inferior da tela.")
            .AddNarration("Passe o mouse sobre as habilidades para ver seus efeitos!")
            .AddNarration("Após selecionar uma habilidade, clique no alvo desejado (se necessário).")
            .AddNarration("Cuidado com seu HP e MP!")
            .AddNarration("Lembre-se: você pode pressionar 'E' para ver suas estatísticas durante a batalha!")
            .AddNarration("Derrote todos os inimigos para vencer!");
        
        DebugLog("✅ Tutorial construído, chamando Show()...");
        tutorial.Show(onComplete);
    }

    /// <summary>
    /// Tutorial de Negociação
    /// </summary>
    private void ShowNegotiationTutorial(System.Action onComplete)
    {
        DebugLog("=== CONSTRUINDO TUTORIAL DE NEGOCIAÇÃO ===");
        
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Em sua última aparição no mundo, Logrif fez diversos inimigos. Seus atos causaram a fúria de diversas entidades.")
            .AddNarration(
                "Os homens que o odiavam estão mortos há éons. Mas o universo ainda se lembra. E talvez, coisas ainda mais antigas que o próprio universo.")
            .AddNarration(
                "Elas o observam em sua guerra solitária. E a partir de seus atos, criam os mais diabólicos planos...")
            .AddLine("Logrif", "Ei! Esta palavra é minha!")
            .AddNarration("Os acordos sempre estarão amarrados em grandeza e atributo. ")
            .AddNarration("Cada carta oferece um benefício para VOCÊ e um benefício para seus INIMIGOS ou um malefício para VOCÊ.")
            .AddNarration("Algumas cartas permitem escolher qual atributo será afetado.")
            .AddNarration("Enquanto outras permitem escolher a intensidade da mudança.")
            .AddNarration("Você pode pressionar 'E' para ver suas estatísticas atuais antes de decidir!")
            .AddNarration("Estas forças são traiçoeiras. Pense sabiamente.")
            .AddLine("Logrif", "O maldito narrador tem razão. O que escolher...");
        
        DebugLog("✅ Tutorial construído, chamando Show()...");
        tutorial.Show(onComplete);
    }

    /// <summary>
    /// Tutorial da Loja
    /// </summary>
    private void ShowShopTutorial(System.Action onComplete)
    {
        DebugLog("=== CONSTRUINDO TUTORIAL DE LOJA ===");
        
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration("Das profundezas vazias do universo, os poucos 'aliados' de Logrif lhe oferecem uma troca...")
            .AddNarration("Primeiro, clique em um item à venda na parte superior.")
            .AddNarration("Depois, clique em um dos seus 4 slots de habilidade na parte inferior.")
            .AddNarration("Isso substituirá a habilidade atual pelo novo item comprado.")
            .AddNarration("Passe o mouse sobre os itens para ver seus efeitos!")
            .AddNarration("Itens consumíveis têm número limitado de usos e desaparecem quando acabam.")
            .AddNarration("Você pode pressionar 'E' para ver seu inventário atual!")
            .AddNarration("Escolha com cuidado - você só pode ter 4 habilidades de cada vez!");
        
        DebugLog("✅ Tutorial construído, chamando Show()...");
        tutorial.Show(onComplete);
    }

    /// <summary>
    /// Tutorial do Tesouro
    /// </summary>
    private void ShowTreasureTutorial(System.Action onComplete)
    {
        DebugLog("=== CONSTRUINDO TUTORIAL DE TESOURO ===");
        
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Milagrosamente, os deuses lunares ainda se lembram do arquidemônio e suas ameaças. Ou talvez tenham pena dele. Quem sabe...")
            .AddLine("Logrif", "Finalmente! Tolos que sabem seus lugares.")
            .AddNarration("Escolha uma das habilidades apresentadas para adicionar ao seu arsenal.")
            .AddNarration("Primeiro, clique na habilidade que deseja.")
            .AddNarration("Depois, clique em um dos seus 4 slots para substituir uma habilidade existente.")
            .AddNarration("Passe o mouse sobre as habilidades para ver seus efeitos!")
            .AddNarration("Você pode pressionar 'E' para ver suas habilidades atuais antes de decidir!")
            .AddNarration("Se não quiser nenhuma das opções, você pode clicar em 'Sair'.");
        
        DebugLog("✅ Tutorial construído, chamando Show()...");
        tutorial.Show(onComplete);
    }

    #endregion

    #region Debug Helpers

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[TutorialManager]</color> {message}");
        }
    }

    private void DebugError(string message)
    {
        Debug.LogError($"<color=red>[TutorialManager]</color> {message}");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reseta todos os tutoriais (útil para debug)
    /// </summary>
    [ContextMenu("Reset All Tutorials")]
    public void ResetAllTutorials()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Map");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Battle");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Negotiation");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Shop");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Skill");
        PlayerPrefs.Save();
        
        DebugLog("✅ Todos os tutoriais foram resetados!");
    }

    /// <summary>
    /// Reseta um tutorial específico
    /// </summary>
    public void ResetTutorial(string tutorialKey)
    {
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + tutorialKey);
        PlayerPrefs.Save();
        
        DebugLog($"✅ Tutorial '{tutorialKey}' resetado!");
    }

    /// <summary>
    /// Ativa ou desativa os tutoriais
    /// </summary>
    public void SetTutorialsEnabled(bool enabled)
    {
        enableTutorials = enabled;
        DebugLog($"Tutoriais {(enabled ? "ativados" : "desativados")}");
    }

    /// <summary>
    /// Força a exibição de um tutorial específico
    /// </summary>
    public void ForceTutorial(string tutorialKey)
    {
        DebugLog($"=== FORÇANDO TUTORIAL: {tutorialKey} ===");
        tutorialShownThisScene = false;
        ShowTutorialForScene(tutorialKey, tutorialKey);
    }

    #endregion

    #region Validation

    void OnValidate()
    {
        // Validações no Editor
        if (delayBeforeTutorial < 0)
        {
            delayBeforeTutorial = 0;
            Debug.LogWarning("Delay não pode ser negativo!");
        }
    }

    #endregion
}