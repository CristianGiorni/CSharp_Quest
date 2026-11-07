using UnityEngine;

public class SlotFeedback : MonoBehaviour
{
    [Header("Objeto visual de la ranura (el que cambia de color)")]
    public Renderer target;

    [Header("Colores")]
    public Color successColor = new Color(0.2f, 0.85f, 0.3f);
    public Color rejectColor = new Color(0.95f, 0.2f, 0.2f);

    private Material mat;
    private Color startColor;

    void Awake()
    {
        if (target == null) target = GetComponentInChildren<Renderer>();

        if (target == null)
        {
            Debug.LogWarning("SlotFeedback: falta asignar el Target (el objeto visual de la ranura).");
            return;
        }

        // .material crea una copia propia, asi cada ranura cambia de color por separado
        mat = target.material;
        startColor = mat.color;
    }

    // Verde fijo: la ranura quedo resuelta
    public void ShowSuccess()
    {
        SetColor(successColor);
    }

    // Rojo fijo hasta que el jugador retire la caja equivocada
    public void ShowReject()
    {
        SetColor(rejectColor);
    }

    // Vuelve al color de reposo
    public void ShowEmpty()
    {
        SetColor(startColor);
    }

    private void SetColor(Color c)
    {
        if (mat != null) mat.color = c;
    }
}