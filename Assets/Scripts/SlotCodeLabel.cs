using TMPro;
using UnityEngine;

// Texto de codigo que flota sobre la ranura. Tiene un hueco [ ___ ] que se completa
// con el valor de la caja colocada: verde si es correcta, rojo si es incorrecta.
public class SlotCodeLabel : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_Text label;

    [Header("Codigo (usa [ ___ ] donde va la caja)")]
    [TextArea(1, 6)] public string codeTemplate = "bool puenteActivo = [ ___ ];\nif (puenteActivo) BajarPuente();";

    [Header("Posicion")]
    [Tooltip("Cuanto se adelanta el cartel hacia la camara. Con camara ortografica no cambia como se ve, pero evita que lo tapen las paredes.")]
    public float cameraOffset = 6f;

    [Header("Colores")]
    public Color codeColor = Color.white;
    public Color blankColor = new Color(1f, 0.85f, 0.3f);
    public Color correctColor = new Color(0.3f, 1f, 0.4f);
    public Color wrongColor = new Color(1f, 0.3f, 0.3f);

    private const string Blank = "[ ___ ]";
    private Camera cam;
    private Vector3 baseLocalPosition;

    void Awake()
    {
        cam = Camera.main;
        baseLocalPosition = transform.localPosition;
        ShowEmpty();
    }

    // Siempre mira hacia la camara, sea cual sea su angulo
    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        transform.rotation = cam.transform.rotation;

        // Se adelanta hacia la camara para quedar siempre delante de las paredes
        Vector3 anchor = transform.parent != null ? transform.parent.TransformPoint(baseLocalPosition) : baseLocalPosition;
        transform.position = anchor - cam.transform.forward * cameraOffset;
    }

    // Hueco vacio: la ranura esta esperando una caja
    [ContextMenu("Actualizar texto")]
    public void ShowEmpty()
    {
        Render(Blank, blankColor);
    }

    // Hueco completado con la caja colocada
    public void ShowFilled(DataBlock block, bool correct)
    {
        Render(FormatLiteral(block), correct ? correctColor : wrongColor);
    }

    private void Render(string word, Color color)
    {
        if (label == null) return;

        string hex = ColorUtility.ToHtmlStringRGB(color);
        string filled = "<color=#" + hex + ">" + word + "</color>";

        label.color = codeColor;
        label.text = codeTemplate.Replace(Blank, filled);
    }

    // Muestra el valor como se escribiria en C#: "hola" entre comillas, 2.5 como 2.5f
    private static string FormatLiteral(DataBlock block)
    {
        switch (block.type)
        {
            case DataType.String:
                // Si el valor ya trae comillas, no se duplican
                if (block.value.Length >= 2 && block.value.StartsWith("\"") && block.value.EndsWith("\""))
                    return block.value;
                return "\"" + block.value + "\"";
            case DataType.Float:
                return block.value.EndsWith("f") ? block.value : block.value + "f";
            default:
                return block.value;
        }
    }
}