using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Importante para carregar outras telas/cenas do projeto!
using TMPro;

[System.Serializable]
public class ViolenciaData
{
    public int id;
    public string nomeViolencia;
    public Sprite arteCarta;               // Arte para o Mestre
    public Sprite arteBotaoVotacao;        // Design / Arte do botão de voto
    [TextArea(3, 5)] public string descricao;
}

[System.Serializable]
public class CaracteristicaData
{
    public int id;
    public Sprite arteCarta;
}

public class VulnusGameManagerLocal : MonoBehaviour
{
    [Header("Simulação Multiplayer (Local)")]
    public bool souOMestre = true;

    [Header("1. Nomes das Cenas externas (Amiga)")]
    [Tooltip("Nome exato da cena de Tela de Usuário/Login para onde o botão Sair deve ir.")]
    public string nomeCenaUsuario = "TelaUsuario";
    [Tooltip("Nome exato da cena do Lobby para onde o botão Voltar pro Lobby deve ir.")]
    public string nomeCenaLobby = "Lobby";

    [Header("2. Referências das Telas do Jogo")]
    public GameObject telaViolencia;
    public GameObject telaCaracteristicas;
    public GameObject telaMesa; // Sua única tela de Mesa (Tela_EsperaMesa)
    public GameObject telaPlacar;

    [Header("3. Texto de Status Único (Central)")]
    public TextMeshProUGUI textStatusGlobal;

    [Header("4. Timers (4 Minutos)")]
    public TextMeshProUGUI textTimerCaracteristicas;
    public TextMeshProUGUI textTimerMesa;
    private float tempoRestante = 240f;
    private bool timerAtivo = false;

    [Header("5. Fase 1: Seleção da Violência (Mestre)")]
    public GameObject prefabCarta;
    public Transform containerGrid;
    public List<ViolenciaData> bancoViolencias = new List<ViolenciaData>();
    public GameObject popUpDetalhe;
    public Image imgCartaGrande;

    [Header("6. Fase 2: Seleção das Características (Mestre)")]
    public GameObject prefabCartaCaracteristica;
    public Transform containerGridCaracteristicas;
    public List<CaracteristicaData> bancoCaracteristicas = new List<CaracteristicaData>();
    public TextMeshProUGUI textContadorCaracteristicas;
    public GameObject btnConfirmar;
    public Button btnSetaEsquerda;
    public Button btnSetaDireita;

    [Header("7. Fase 3: Cartas da Mesa")]
    public Image imgMesaCarac1;
    public Image imgMesaCarac2;
    public Image imgMesaCarac3;

    [Header("8. Votação das Violências (6 Botões)")]
    public Button[] botoesVotoViolencia;
    public TextMeshProUGUI[] textosContadorVotos;
    public GameObject btnConfirmarVotoMesa;

    // Controladores Internos
    private bool cartaJaSelecionada = false;
    private ViolenciaData violenciaEscolhidaPeloMestre;
    private List<ViolenciaData> violenciasRodadaAtual = new List<ViolenciaData>();
    private List<CaracteristicaData> caracteristicasSelecionadas = new List<CaracteristicaData>();

    private int[] contagemVotos = new int[6];
    private int indexVotoSelecionado = -1;
    private int tentativaAtual = 1;
    private int paginaAtualCaracteristicas = 0;
    private const int CARTAS_POR_PAGINA = 3;

    void Start()
    {
        IniciarNovaRodada();
    }

    void Update()
    {
        if (timerAtivo && tempoRestante > 0)
        {
            tempoRestante -= Time.deltaTime;
            int min = Mathf.FloorToInt(tempoRestante / 60);
            int seg = Mathf.FloorToInt(tempoRestante % 60);
            string tempoFormatado = string.Format("{0:00}:{1:00}", min, seg);

            if (telaCaracteristicas != null && telaCaracteristicas.activeSelf && textTimerCaracteristicas != null)
                textTimerCaracteristicas.text = tempoFormatado;

            if (telaMesa != null && telaMesa.activeSelf && textTimerMesa != null)
                textTimerMesa.text = tempoFormatado;

            if (tempoRestante <= 0)
            {
                tempoRestante = 0;
                timerAtivo = false;
            }
        }
    }

    // Inicializa ou reinicia uma partida
    private void IniciarNovaRodada()
    {
        // Alterna ou reativa as telas
        if (telaViolencia != null) telaViolencia.SetActive(true);
        if (telaCaracteristicas != null) telaCaracteristicas.SetActive(false);
        if (telaMesa != null) telaMesa.SetActive(false);
        if (telaPlacar != null) telaPlacar.SetActive(false);

        if (popUpDetalhe != null) popUpDetalhe.SetActive(false);

        // Limpa seleções anteriores
        cartaJaSelecionada = false;
        violenciaEscolhidaPeloMestre = null;
        caracteristicasSelecionadas.Clear();
        tentativaAtual = 1;
        indexVotoSelecionado = -1;

        // Atualiza a mensagem inicial de status conforme quem é o Mestre
        if (souOMestre)
        {
            AtualizarTextoStatus("Você é o Mestre! Escolha uma carta de Violência.");
        }
        else
        {
            AtualizarTextoStatus("O Mestre está escolhendo as cartas...");
        }

        timerAtivo = false;
        SorteareGerarCartas();
    }

    public void AtualizarTextoStatus(string novaMensagem)
    {
        if (textStatusGlobal != null)
        {
            textStatusGlobal.text = novaMensagem;
        }
    }

    // ==========================================
    // FASE 1: SELEÇÃO DA VIOLÊNCIA
    // ==========================================
    void SorteareGerarCartas()
    {
        foreach (Transform c in containerGrid) Destroy(c.gameObject);

        violenciasRodadaAtual.Clear();
        List<ViolenciaData> baralhoEmbaralhado = new List<ViolenciaData>(bancoViolencias);

        for (int i = 0; i < baralhoEmbaralhado.Count; i++)
        {
            ViolenciaData temp = baralhoEmbaralhado[i];
            int randomIndex = Random.Range(i, baralhoEmbaralhado.Count);
            baralhoEmbaralhado[i] = baralhoEmbaralhado[randomIndex];
            baralhoEmbaralhado[randomIndex] = temp;
        }

        int quantidadeExibir = Mathf.Min(6, baralhoEmbaralhado.Count);

        for (int i = 0; i < quantidadeExibir; i++)
        {
            ViolenciaData dados = baralhoEmbaralhado[i];
            violenciasRodadaAtual.Add(dados);

            GameObject cartaObj = Instantiate(prefabCarta, containerGrid);
            CartaUI scriptCarta = cartaObj.GetComponent<CartaUI>();

            if (scriptCarta != null)
            {
                scriptCarta.idCarta = dados.id;
                scriptCarta.imagemFrente.sprite = dados.arteCarta;
                scriptCarta.imagemVerso.gameObject.SetActive(true);
                scriptCarta.imagemFrente.gameObject.SetActive(false);
            }

            cartaObj.GetComponent<Button>().onClick.AddListener(() => {
                if (!cartaJaSelecionada)
                {
                    cartaJaSelecionada = true;
                    violenciaEscolhidaPeloMestre = dados;
                    StartCoroutine(Routine_SequenciaAcao(scriptCarta, dados));
                }
            });
        }
    }

    private IEnumerator Routine_SequenciaAcao(CartaUI carta, ViolenciaData dados)
    {
        if (carta != null) yield return StartCoroutine(carta.Routine_Girar());
        yield return new WaitForSeconds(0.1f);

        if (imgCartaGrande != null && dados != null) imgCartaGrande.sprite = dados.arteCarta;
        if (popUpDetalhe != null) popUpDetalhe.SetActive(true);
    }

    public void Botao_IrParaCaracteristicas()
    {
        if (popUpDetalhe != null) popUpDetalhe.SetActive(false);
        if (telaViolencia != null) telaViolencia.SetActive(false);
        if (telaCaracteristicas != null) telaCaracteristicas.SetActive(true);

        AtualizarTextoStatus("O Mestre está escolhendo as cartas...");

        tempoRestante = 240f;
        timerAtivo = true;

        paginaAtualCaracteristicas = 0;
        caracteristicasSelecionadas.Clear();
        AtualizarContadorEBotaoConfirmar();
        RenderizarPaginaCaracteristicas();
    }

    // ==========================================
    // FASE 2: SELEÇÃO DAS CARACTERÍSTICAS
    // ==========================================
    public void RenderizarPaginaCaracteristicas()
    {
        foreach (Transform c in containerGridCaracteristicas) Destroy(c.gameObject);

        int indexInicio = paginaAtualCaracteristicas * CARTAS_POR_PAGINA;
        int indexFim = Mathf.Min(indexInicio + CARTAS_POR_PAGINA, bancoCaracteristicas.Count);

        for (int i = indexInicio; i < indexFim; i++)
        {
            CaracteristicaData dados = bancoCaracteristicas[i];
            GameObject cartaObj = Instantiate(prefabCartaCaracteristica, containerGridCaracteristicas);
            CartaCaracteristicaUI scriptCarta = cartaObj.GetComponent<CartaCaracteristicaUI>();

            if (scriptCarta != null)
            {
                scriptCarta.idCarta = dados.id;
                scriptCarta.imagemCarta.sprite = dados.arteCarta;

                bool estaSelecionada = caracteristicasSelecionadas.Exists(c => c.id == dados.id);
                if (scriptCarta.iconeCheck != null) scriptCarta.iconeCheck.SetActive(estaSelecionada);

                scriptCarta.botao.onClick.AddListener(() => {
                    AlternarSelecaoCaracteristica(dados, scriptCarta);
                });
            }
        }

        AtualizarBotoesNavegacao();
    }

    void AlternarSelecaoCaracteristica(CaracteristicaData dados, CartaCaracteristicaUI scriptCarta)
    {
        bool jaSelecionada = caracteristicasSelecionadas.Exists(c => c.id == dados.id);

        if (jaSelecionada)
        {
            caracteristicasSelecionadas.RemoveAll(c => c.id == dados.id);
            if (scriptCarta.iconeCheck != null) scriptCarta.iconeCheck.SetActive(false);
        }
        else
        {
            if (caracteristicasSelecionadas.Count < 3)
            {
                caracteristicasSelecionadas.Add(dados);
                if (scriptCarta.iconeCheck != null) scriptCarta.iconeCheck.SetActive(true);
            }
        }

        AtualizarContadorEBotaoConfirmar();
    }

    void AtualizarContadorEBotaoConfirmar()
    {
        int qtd = caracteristicasSelecionadas.Count;
        if (textContadorCaracteristicas != null) textContadorCaracteristicas.text = string.Format("{0}/3", qtd);
        if (btnConfirmar != null) btnConfirmar.SetActive(qtd == 3);
    }

    void AtualizarBotoesNavegacao()
    {
        int totalPaginas = Mathf.CeilToInt((float)bancoCaracteristicas.Count / CARTAS_POR_PAGINA);
        if (btnSetaEsquerda != null) btnSetaEsquerda.interactable = (paginaAtualCaracteristicas > 0);
        if (btnSetaDireita != null) btnSetaDireita.interactable = (paginaAtualCaracteristicas < totalPaginas - 1);
    }

    public void Botao_SetaEsquerda()
    {
        if (paginaAtualCaracteristicas > 0)
        {
            paginaAtualCaracteristicas--;
            RenderizarPaginaCaracteristicas();
        }
    }

    public void Botao_SetaDireita()
    {
        int totalPaginas = Mathf.CeilToInt((float)bancoCaracteristicas.Count / CARTAS_POR_PAGINA);
        if (paginaAtualCaracteristicas < totalPaginas - 1)
        {
            paginaAtualCaracteristicas++;
            RenderizarPaginaCaracteristicas();
        }
    }

    // ==========================================
    // FASE 3: MESA DE JOGO
    // ==========================================
    public void Botao_ConfirmarCaracteristicas()
    {
        if (caracteristicasSelecionadas.Count == 3)
        {
            if (telaCaracteristicas != null) telaCaracteristicas.SetActive(false);
            if (telaMesa != null) telaMesa.SetActive(true);

            if (souOMestre)
            {
                AtualizarTextoStatus("Aguardando os votos dos jogadores...");
            }
            else
            {
                AtualizarTextoStatus("Aguardando votos... Escolha a violência (Tentativa 1/2)");
            }

            tempoRestante = 240f;
            timerAtivo = true;

            tentativaAtual = 1;
            indexVotoSelecionado = -1;

            if (imgMesaCarac1 != null) imgMesaCarac1.sprite = caracteristicasSelecionadas[0].arteCarta;
            if (imgMesaCarac2 != null) imgMesaCarac2.sprite = caracteristicasSelecionadas[1].arteCarta;
            if (imgMesaCarac3 != null) imgMesaCarac3.sprite = caracteristicasSelecionadas[2].arteCarta;

            ConfigurarBotoesDeVotacao();
        }
    }

    void ConfigurarBotoesDeVotacao()
    {
        for (int i = 0; i < 6; i++)
        {
            contagemVotos[i] = 0;

            if (i < violenciasRodadaAtual.Count)
            {
                int index = i;
                botoesVotoViolencia[i].gameObject.SetActive(true);

                Image imgBotao = botoesVotoViolencia[i].GetComponent<Image>();
                if (imgBotao != null && violenciasRodadaAtual[i].arteBotaoVotacao != null)
                {
                    imgBotao.sprite = violenciasRodadaAtual[i].arteBotaoVotacao;
                }

                if (i < textosContadorVotos.Length && textosContadorVotos[i] != null)
                {
                    textosContadorVotos[i].text = "0";
                }

                botoesVotoViolencia[i].interactable = !souOMestre;

                botoesVotoViolencia[i].onClick.RemoveAllListeners();
                botoesVotoViolencia[i].onClick.AddListener(() => SelecionarVotoViolencia(index));
            }
            else
            {
                botoesVotoViolencia[i].gameObject.SetActive(false);
            }
        }

        if (btnConfirmarVotoMesa != null) btnConfirmarVotoMesa.SetActive(false);
    }

    public void SelecionarVotoViolencia(int indexBotao)
    {
        if (souOMestre) return;

        indexVotoSelecionado = indexBotao;
        contagemVotos[indexBotao]++;

        if (indexBotao < textosContadorVotos.Length && textosContadorVotos[indexBotao] != null)
            textosContadorVotos[indexBotao].text = contagemVotos[indexBotao].ToString();

        if (btnConfirmarVotoMesa != null) btnConfirmarVotoMesa.SetActive(true);
    }

    public void Botao_ConfirmarVotoMesa()
    {
        if (indexVotoSelecionado < 0) return;

        ViolenciaData violenciaVotada = violenciasRodadaAtual[indexVotoSelecionado];

        if (violenciaVotada.id == violenciaEscolhidaPeloMestre.id)
        {
            StartCoroutine(Routine_ResultadoVotacao("Parabéns! Vocês acertaram a Violência!"));
        }
        else
        {
            if (tentativaAtual == 1)
            {
                tentativaAtual = 2;
                AtualizarTextoStatus("Incorreto! Vocês têm mais 1 tentativa.");

                botoesVotoViolencia[indexVotoSelecionado].interactable = false;
                indexVotoSelecionado = -1;

                if (btnConfirmarVotoMesa != null) btnConfirmarVotoMesa.SetActive(false);
            }
            else
            {
                StartCoroutine(Routine_ResultadoVotacao("Incorreto novamente! Revelando o Placar..."));
            }
        }
    }

    private IEnumerator Routine_ResultadoVotacao(string mensagemFinal)
    {
        AtualizarTextoStatus(mensagemFinal);

        if (btnConfirmarVotoMesa != null) btnConfirmarVotoMesa.SetActive(false);

        yield return new WaitForSeconds(2.5f);

        timerAtivo = false;
        if (telaMesa != null) telaMesa.SetActive(false);
        if (telaPlacar != null) telaPlacar.SetActive(true);
    }

    // ==========================================
    // FASE 4: PLACAR (BOTÕES DA TELA DO PLACAR)
    // ==========================================

    // 1. Botão Sair (Leva para a Tela de Usuário)
    public void Botao_Sair()
    {
        if (!string.IsNullOrEmpty(nomeCenaUsuario))
        {
            SceneManager.LoadScene(nomeCenaUsuario);
        }
        else
        {
            Debug.LogWarning("O nome da cena de usuário não foi configurado no Inspector!");
        }
    }

    // 2. Botão Voltar pro Lobby (Leva para a Tela de Lobby)
    public void Botao_VoltarLobby()
    {
        if (!string.IsNullOrEmpty(nomeCenaLobby))
        {
            SceneManager.LoadScene(nomeCenaLobby);
        }
        else
        {
            Debug.LogWarning("O nome da cena de Lobby não foi configurado no Inspector!");
        }
    }

    // 3. Botão Jogar Novamente (Sorteia novo mestre e reinicia a partida)
    public void Botao_JogarNovamente()
    {
        // Alterna/Sorteia quem será o novo Mestre para a próxima rodada
        souOMestre = !souOMestre;

        // Reinicia todo o fluxo do jogo
        IniciarNovaRodada();
    }
}