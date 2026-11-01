using System.Collections;
using UnityEngine;

public class SlotFeedback : MonoBehaviour
{
    [Header("Objeto visual de la ranura (el que cambia de color)")]
    public Renderer target;

    [Header("Colores")]
    public Color successColor = new Color(0.2f, 0.85f, 0.3f);
    public Color rejectColor = new Color(0.95f, 0.2f, 0.2f);

    [Header("Duracion del destello de rechazo (segundos)")]
    public float flashDuration = 0.5f;

    private Material mat;
    private Color startColor;
    private bool solved;

    void Awake()
    {
        if (target == null) target = GetComponentInChildren<Renderer>();

        if (target == null)
        {
            Debug.LogWarning("SlotFeedback: falta asignar el Target (el objeto visual de la ranura).");
            enabled = false;
            return;
        }

        // .material crea una copia propia, asi cada ranura cambia de color por separado
        mat = target.material;
        startColor = mat.color;
    }

    // Conectar al evento On Success de SlotReceiver
    public void ShowSuccess()
    {
        solved = true;
        StopAllCoroutines();
        mat.color = successColor;
    }

    // Conectar al evento On Reject de SlotReceiver
    public void ShowReject()
    {
        if (solved) return;
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        float t = 0f;
        mat.color = rejectColor;

        while (t < 1f)
        {
            t += Time.deltaTime / flashDuration;
            mat.color = Color.Lerp(rejectColor, startColor, t);
            yield return null;
        }

        mat.color = startColor;
    }
}