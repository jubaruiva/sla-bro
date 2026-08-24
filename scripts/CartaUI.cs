using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CartaUI : MonoBehaviour
{
    public int idCarta;
    public Image imagemVerso;
    public Image imagemFrente;

    public IEnumerator Routine_Girar()
    {
        float duracao = 0.2f;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(1f, 0f, tempo / duracao), 1f, 1f);
            yield return null;
        }

        if (imagemVerso != null) imagemVerso.gameObject.SetActive(false);
        if (imagemFrente != null) imagemFrente.gameObject.SetActive(true);

        tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(0f, 1f, tempo / duracao), 1f, 1f);
            yield return null;
        }
    }
}