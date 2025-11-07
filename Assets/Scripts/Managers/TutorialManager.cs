// Assets/Scripts/Tutorial/TutorialManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia os tutoriais do jogo, mostrando-os apenas na primeira vez
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableTutorials = true;
    [SerializeField] private float delayBeforeTutorial = 0.5f;
    
    private const string TUTORIAL_PREFIX = "Tutorial_";
    private bool tutorialShownThisScene = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        string currentScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(currentScene))
        {
            StartCoroutine(ShowTutorialAfterDelay(currentScene));
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        tutorialShownThisScene = false;
        
        if (!enableTutorials)
        {
            return;
        }

        StartCoroutine(ShowTutorialAfterDelay(scene.name));
    }

    private System.Collections.IEnumerator ShowTutorialAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(delayBeforeTutorial);
        
        if (!tutorialShownThisScene)
        {
            CheckAndShowTutorial(sceneName);
        }
    }

    private void CheckAndShowTutorial(string sceneName)
    {
        string tutorialKey = GetTutorialKey(sceneName);
        
        if (ShouldShowTutorial(tutorialKey))
        {
            ShowTutorialForScene(sceneName, tutorialKey);
        }
    }

    private string GetTutorialKey(string sceneName)
    {
        string key = sceneName;
        
        if (sceneName.StartsWith("Map"))
        {
            key = "Map";
        }
        else if (sceneName.ToLower().Contains("battle"))
        {
            key = "Battle";
        }
        else if (sceneName.ToLower().Contains("negotiation"))
        {
            key = "Negotiation";
        }
        else if (sceneName.ToLower().Contains("shop"))
        {
            key = "Shop";
        }
        else if (sceneName.ToLower().Contains("skill"))
        {
            key = "Skill";
        }
            
        return key;
    }

    private bool ShouldShowTutorial(string tutorialKey)
    {
        string prefKey = TUTORIAL_PREFIX + tutorialKey;
        int wasShown = PlayerPrefs.GetInt(prefKey, 0);
        return wasShown == 0;
    }

    private void MarkTutorialAsShown(string tutorialKey)
    {
        string prefKey = TUTORIAL_PREFIX + tutorialKey;
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();
    }

    private void ShowTutorialForScene(string sceneName, string tutorialKey)
    {
        tutorialShownThisScene = true;
        
        if (DialogueManager.Instance == null)
        {
            return;
        }

        switch (tutorialKey)
        {
            case "Map":
                ShowMapTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Battle":
                ShowBattleTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Negotiation":
                ShowNegotiationTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Shop":
                ShowShopTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
                
            case "Skill":
                ShowTreasureTutorial(() => MarkTutorialAsShown(tutorialKey));
                break;
        }
    }

    #region Tutorial Definitions

    private void ShowMapTutorial(System.Action onComplete)
    {
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Há muito tempo atrás, o arquidemônio Logrif, após ser traído pelos seus aliados, foi selado.")  
            .AddNarration(
                "O mundo dos humanos o esqueceu, mas ele seguiu trancafiado. Até o dia em que o caos que assola o mundo finalmente romperia seu selo...")  
            .AddLine("Logrif",
                "Humph. O mundo mudou muito desde minha última aparição. Quando foi que meus dominíos ficaram tão enxutos assim?!")
            .AddNarration(
                "Mal sabe o arquidemônio que outras forças agora travam guerra contra os humanos. O território que um dia lhe pertenceu agora é posse de outro Senhor do Escuro...")
            .AddLine("Logrif",
                "Carcaça e Carnificina! E quem são esses moleques?! Tudo isso era mato antes de eu chegar aqui!")
            .AddNarration(
                "Ao norte, os patrulheiros e druidas enfrentam as forças de um Senhor do Escuro. Flecha e fogo rugem pela mata")
            .AddNarration("Avance sob os pontos em marrom para prosseguir")
            .AddNarration("Nós vermelhos estão trancados e nós escuros já foram completados.")
            .AddNarration("Arraste a tela segurando o botão esquerdo do mouse para explorar o mapa.")
            .AddNarration("Pressione 'E' a qualquer momento para abrir o Menu de Status e ver suas ações de batalha.")
            .AddNarration("Pressione 'ESC' para abrir o Menu de Opções.");
        
        tutorial.Show(onComplete);
    }

    private void ShowBattleTutorial(System.Action onComplete)
    {
        var tutorial = DialogueUtils.CreateBuilder()
            .AddLine("Logrif", "VOCÊS SE ATREVEM?! Vão todos morrer!")
            .AddNarration("O sistema de batalha usa ATB (Active Time Battle). Cada personagem tem uma barra que enche ao longo do tempo.")
            .AddNarration("Quando sua barra estiver cheia, você poderá escolher uma ação.")
            .AddNarration("Selecione uma habilidade dos botões disponíveis na parte inferior da tela.")
            .AddNarration("Passe o mouse sobre as habilidades para ver seus efeitos!")
            .AddNarration("Após selecionar uma habilidade, clique no alvo desejado (se necessário).")
            .AddNarration("Cuidado com seu HP e MP! Inspecione os inimigos passando o mouse acima deles!")
            .AddNarration("Lembre-se: você pode pressionar 'E' para ver suas estatísticas durante a batalha!")
            .AddNarration("Derrote todos os inimigos para vencer!");
        
        tutorial.Show(onComplete);
    }

    private void ShowNegotiationTutorial(System.Action onComplete)
    {
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Em sua última aparição no mundo, Logrif fez diversos inimigos. Seus atos causaram a fúria de diversas entidades.")
            .AddNarration(
                "Os homens que o odiavam estão mortos há éons. Mas o universo ainda se lembra. E talvez, coisas ainda mais antigas que o próprio universo.")
            .AddNarration(
                "Elas o observam em sua guerra solitária. E a partir de seus atos, criam os mais diabólicos planos...")
            .AddLine("Logrif", "Ei! Esta palavra é minha!")
            .AddNarration("Os acordos sempre estarão amarrados em grandeza e atributo. ")
            .AddNarration("Cada carta oferece um benefício para VOCÊ e um benefício para seus INIMIGOS.")
            .AddNarration("Algumas cartas permitem escolher qual atributo será afetado.")
            .AddNarration("Enquanto outras permitem escolher a intensidade da mudança.")
            .AddNarration("Você pode pressionar 'E' para ver suas estatísticas atuais antes de decidir!")
            .AddNarration("Estas forças são traiçoeiras. Pense sabiamente.")
            .AddLine("Logrif", "O maldito narrador tem razão. O que escolher...");
        
        tutorial.Show(onComplete);
    }

    private void ShowShopTutorial(System.Action onComplete)
    {
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration("Das profundezas vazias do universo, os poucos 'aliados' de Logrif lhe oferecem uma troca...")
            .AddNarration("Primeiro, clique em um item à venda na parte superior.")
            .AddNarration("Depois, clique em um dos seus 4 slots de habilidade na parte inferior.")
            .AddNarration("Isso substituirá a habilidade atual pelo novo item comprado.")
            .AddNarration("Passe o mouse sobre os itens para ver seus efeitos!")
            .AddNarration("Itens consumíveis têm número limitado de usos e desaparecem quando acabam.")
            .AddNarration("Você pode pressionar 'E' para ver seu inventário atual!")
            .AddNarration("Escolha com cuidado - você só pode ter 4 habilidades de cada vez!");
        
        tutorial.Show(onComplete);
    }

    private void ShowTreasureTutorial(System.Action onComplete)
    {
        var tutorial = DialogueUtils.CreateBuilder()
            .AddNarration(
                "Milagrosamente, os deuses lunares ainda se lembram do arquidemônio e suas ameaças. Ou talvez tenham pena dele. Quem sabe...")
            .AddLine("Logrif", "Finalmente! Tolos que sabem seus lugares.")
            .AddNarration("Escolha uma das habilidades apresentadas para adicionar ao seu arsenal.")
            .AddNarration("Primeiro, clique na habilidade que deseja.")
            .AddNarration("Depois, clique em um dos seus 4 slots para substituir uma habilidade existente.")
            .AddNarration("Passe o mouse sobre as habilidades para ver seus efeitos!")
            .AddNarration("Você pode pressionar 'E' para ver suas habilidades atuais antes de decidir!")
            .AddNarration("Se não quiser nenhuma das opções, você pode Descansar e curar 50 HP e 50 MP.");
        
        tutorial.Show(onComplete);
    }

    #endregion

    #region Public Methods

    [ContextMenu("Reset All Tutorials")]
    public void ResetAllTutorials()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Map");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Battle");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Negotiation");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Shop");
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + "Skill");
        PlayerPrefs.Save();
    }

    public void ResetTutorial(string tutorialKey)
    {
        PlayerPrefs.DeleteKey(TUTORIAL_PREFIX + tutorialKey);
        PlayerPrefs.Save();
    }

    public void SetTutorialsEnabled(bool enabled)
    {
        enableTutorials = enabled;
    }

    public void ForceTutorial(string tutorialKey)
    {
        tutorialShownThisScene = false;
        ShowTutorialForScene(tutorialKey, tutorialKey);
    }

    #endregion

    #region Validation

    void OnValidate()
    {
        if (delayBeforeTutorial < 0)
        {
            delayBeforeTutorial = 0;
            Debug.LogWarning("Delay não pode ser negativo!");
        }
    }

    #endregion
}