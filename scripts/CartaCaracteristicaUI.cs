using UnityEngine;
using UnityEngine.UI;

public class CartaCaracteristicaUI : MonoBehaviour
{
    public int idCarta;
    public Image imagemCarta;
    public GameObject iconeCheck; // Objeto da imagem do Check
    public Button botao;
    void Awake()
    {
        // Garante que o check sempre inicie ESCONDIDO ao criar a carta
        if (iconeCheck != null)
        {
            iconeCheck.SetActive(false);
        }
    }
}