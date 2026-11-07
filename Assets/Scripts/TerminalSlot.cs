using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hueco [ ___ ] de la terminal donde se sueltan las piezas
public class TerminalSlot : MonoBehaviour, IDropHandler
{
    [Header("Referencias")]
    public Image background;
    public GameObject placeholder;

    [Header("Tipo que acepta (lo configura la consola al abrirse)")]
    public DataType expectedType = DataType.Int;
    public bool checkType = true;

    [Header("Colores")]
    public Color normalColor = new Color(0.15f, 0.15f, 0.2f, 1f);
    public Color rejectColor = new Color(0.9f, 0.15f, 0.15f, 1f);

    public TerminalOption Current { get; private set; }

    private Coroutine flashRoutine;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        TerminalOption option = eventData.pointerDrag.GetComponent<TerminalOption>();
        if (option == null) return;

        // Tipo equivocado: parpadea en rojo y la pieza vuelve a su lugar
        if (checkType && option.type != expectedType)
        {
            Flash();
            if (TerminalUI.Instance != null)
            {
                TerminalUI.Instance.Say("El tipo no coincide: este hueco espera un " + expectedType.ToString().ToLower() + ".");
            }
            return;
        }

        // Si ya habia una pieza, vuelve a la fila
        if (Current != null && Current != option) Current.ReturnHome();

        Current = option;
        option.AttachTo(this);
        if (placeholder != null) placeholder.SetActive(false);
        if (TerminalUI.Instance != null) TerminalUI.Instance.Say("");
    }

    // La pieza fue sacada del hueco
    public void Release(TerminalOption option)
    {
        if (Current != option) return;

        Current = null;
        if (placeholder != null) placeholder.SetActive(true);
    }

    public void Clear()
    {
        Current = null;
        if (placeholder != null) placeholder.SetActive(true);
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;
        if (background != null) background.color = normalColor;
    }

    public void Flash()
    {
        if (!gameObject.activeInHierarchy) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        background.color = rejectColor;
        yield return new WaitForSeconds(0.5f);
        background.color = normalColor;
        flashRoutine = null;
    }
}