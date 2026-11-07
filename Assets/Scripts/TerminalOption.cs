using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Pieza arrastrable de la terminal (true, 0, 100...)
[RequireComponent(typeof(CanvasGroup))]
public class TerminalOption : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Dato que representa la pieza")]
    public DataType type = DataType.Int;
    public string value = "0";

    [HideInInspector] public string feedback;

    [Header("Referencias")]
    public Image background;
    public TMP_Text label;

    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private Transform homeParent;
    private int order;
    private TerminalSlot currentSlot;
    private bool droppedInSlot;

    // Color segun el tipo (mismo criterio que las cajas del mundo)
    public static Color ColorFor(DataType t)
    {
        switch (t)
        {
            case DataType.Bool: return new Color(0.2f, 0.45f, 0.95f);
            case DataType.Int: return new Color(0.95f, 0.55f, 0.1f);
            case DataType.Float: return new Color(0.95f, 0.85f, 0.2f);
            default: return new Color(0.7f, 0.35f, 0.9f);
        }
    }

    void Init()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rect == null) rect = (RectTransform)transform;
    }

    public void Setup(DataType newType, string newValue, string newFeedback, Transform home, int newOrder)
    {
        Init();
        type = newType;
        value = newValue;
        feedback = newFeedback;
        homeParent = home;
        order = newOrder;

        if (label != null) label.text = value;
        if (background != null) background.color = ColorFor(type);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Init();
        droppedInSlot = false;

        // Si estaba en el hueco, lo libera
        if (currentSlot != null)
        {
            currentSlot.Release(this);
            currentSlot = null;
        }

        // Pasa al canvas raiz para poder moverse libremente por encima de todo
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) transform.SetParent(canvas.rootCanvas.transform, true);
        transform.SetAsLastSibling();

        // Asi el puntero "ve" el hueco que esta debajo de la pieza
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Si no se solto sobre un hueco valido, vuelve a su lugar
        if (!droppedInSlot) ReturnHome();
    }

    // Lo llama el hueco cuando acepta la pieza
    public void AttachTo(TerminalSlot slot)
    {
        Init();
        currentSlot = slot;
        droppedInSlot = true;

        transform.SetParent(slot.transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    // Vuelve a la fila de piezas, respetando el orden original
    public void ReturnHome()
    {
        Init();
        currentSlot = null;

        if (homeParent == null) return;

        transform.SetParent(homeParent, false);

        int index = 0;
        foreach (Transform child in homeParent)
        {
            TerminalOption other = child.GetComponent<TerminalOption>();
            if (other != null && other != this && other.order < order) index++;
        }
        transform.SetSiblingIndex(index);

        canvasGroup.blocksRaycasts = true;
    }
}