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

    [Header("Colores")]
    public Color codeColor = Color.white;
    public Color blankColor = new Color(1f, 0.85f, 0.3f);
    public Color correctColor = new Color(0.3f, 1f, 0.4f);
    public Color wrongColor = new Color(1f, 0.3f, 0.3f);

    private const string Blank = "[ ___ ]";
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        ShowEmpty();
    }

    // Siempre mira hacia la camara, sea cual sea su angulo
    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
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
                return "\"" + block.value + "\"";
            case DataType.Float:
                return block.value.EndsWith("f") ? block.value : block.value + "f";
            default:
                return block.value;
        }
    }
}