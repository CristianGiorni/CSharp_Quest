using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Crea toda la jerarquia de la terminal flotante en la escena con un clic.
// Menu: CSharp Quest > Crear Terminal UI
public static class TerminalUIBuilder
{
    private static readonly Color CodeColor = new Color(0.85f, 0.95f, 0.85f);
    private static readonly Color CyanColor = new Color(0.4f, 0.9f, 1f);

    [MenuItem("CSharp Quest/Crear Terminal UI")]
    public static void Build()
    {
        if (Object.FindFirstObjectByType<TerminalUI>() != null)
        {
            EditorUtility.DisplayDialog("Terminal UI", "Ya existe un TerminalCanvas en la escena.", "OK");
            return;
        }

        // EventSystem (necesario para arrastrar y hacer clic en la UI)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Crear EventSystem");
        }

        // Canvas
        GameObject canvasGO = new GameObject("TerminalCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Terminal UI");

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        TerminalUI ui = canvasGO.AddComponent<TerminalUI>();

        // Panel (abajo a la derecha, como pide el GDD)
        RectTransform panel = NewRect("Panel", canvasGO.transform);
        panel.anchorMin = new Vector2(1f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.pivot = new Vector2(1f, 0f);
        panel.anchoredPosition = new Vector2(-40f, 40f);
        panel.sizeDelta = new Vector2(900f, 640f);

        Image panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.06f, 0.1f, 0.93f);

        Outline border = panel.gameObject.AddComponent<Outline>();
        border.effectColor = CyanColor;
        border.effectDistance = new Vector2(4f, -4f);

        VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureGroup(panelLayout, 12f, new RectOffset(24, 24, 20, 20), TextAnchor.UpperLeft, true, true, true, false);

        // Titulo
        NewText("Titulo", panel, "terminal.cs  -  consola averiada", 26, CyanColor);

        // Zona de codigo
        RectTransform codeArea = NewRect("Codigo", panel);
        Image codeBg = codeArea.gameObject.AddComponent<Image>();
        codeBg.color = new Color(0f, 0f, 0f, 0.55f);
        VerticalLayoutGroup codeLayout = codeArea.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureGroup(codeLayout, 6f, new RectOffset(18, 18, 14, 14), TextAnchor.UpperLeft, true, true, false, false);

        TextMeshProUGUI topText = NewText("TextoSuperior", codeArea, "int energia = 100;\n\nvoid Update() {", 30, CodeColor);

        // Linea con el hueco: texto izquierdo + hueco + texto derecho
        RectTransform slotLine = NewRect("LineaDelHueco", codeArea);
        HorizontalLayoutGroup slotLineLayout = slotLine.gameObject.AddComponent<HorizontalLayoutGroup>();
        ConfigureGroup(slotLineLayout, 10f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, true, true, false, false);

        TextMeshProUGUI leftText = NewText("TextoIzquierdo", slotLine, "    if (energia ==", 30, CodeColor);

        RectTransform slotRect = NewRect("Hueco", slotLine);
        Image slotImg = slotRect.gameObject.AddComponent<Image>();
        slotImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        Outline slotOutline = slotRect.gameObject.AddComponent<Outline>();
        slotOutline.effectColor = new Color(1f, 1f, 1f, 0.8f);
        slotOutline.effectDistance = new Vector2(2f, -2f);
        LayoutElement slotLE = slotRect.gameObject.AddComponent<LayoutElement>();
        slotLE.preferredWidth = 150f;
        slotLE.preferredHeight = 60f;
        TerminalSlot slot = slotRect.gameObject.AddComponent<TerminalSlot>();

        TextMeshProUGUI placeholder = NewText("Marcador", slotRect, "[ ___ ]", 28, new Color(0.6f, 0.6f, 0.65f));
        Stretch(placeholder.rectTransform);
        placeholder.alignment = TextAlignmentOptions.Center;

        slot.background = slotImg;
        slot.placeholder = placeholder.gameObject;

        TextMeshProUGUI rightText = NewText("TextoDerecho", slotLine, ") {", 30, CodeColor);

        TextMeshProUGUI bottomText = NewText("TextoInferior", codeArea, "        Ascensor.Abrir();\n    }\n}", 30, CodeColor);

        // Fila de piezas
        RectTransform optionsArea = NewRect("Piezas", panel);
        HorizontalLayoutGroup optionsLayout = optionsArea.gameObject.AddComponent<HorizontalLayoutGroup>();
        ConfigureGroup(optionsLayout, 24f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter, false, false, false, false);
        LayoutElement optionsLE = optionsArea.gameObject.AddComponent<LayoutElement>();
        optionsLE.minHeight = 90f;
        optionsLE.preferredHeight = 90f;

        // Mensajes
        TextMeshProUGUI feedback = NewText("Mensaje", panel, "", 24, new Color(1f, 0.85f, 0.3f));
        LayoutElement feedbackLE = feedback.gameObject.AddComponent<LayoutElement>();
        feedbackLE.minHeight = 44f;
        feedbackLE.preferredHeight = 44f;

        // Botones
        RectTransform buttons = NewRect("Botones", panel);
        HorizontalLayoutGroup buttonsLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
        ConfigureGroup(buttonsLayout, 20f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleRight, false, false, false, false);
        LayoutElement buttonsLE = buttons.gameObject.AddComponent<LayoutElement>();
        buttonsLE.minHeight = 64f;
        buttonsLE.preferredHeight = 64f;

        Button execute = NewButton("BotonEjecutar", buttons, "Ejecutar", new Color(0.15f, 0.6f, 0.25f));
        Button close = NewButton("BotonCerrar", buttons, "Cerrar", new Color(0.35f, 0.35f, 0.4f));

        // Plantilla de pieza (inactiva): la terminal la copia para crear las piezas
        RectTransform template = NewRect("PlantillaPieza", panel);
        template.sizeDelta = new Vector2(150f, 60f);
        Image templateImg = template.gameObject.AddComponent<Image>();
        TerminalOption option = template.gameObject.AddComponent<TerminalOption>();
        TextMeshProUGUI optionLabel = NewText("Valor", template, "valor", 30, Color.white);
        Stretch(optionLabel.rectTransform);
        optionLabel.alignment = TextAlignmentOptions.Center;
        optionLabel.fontStyle = FontStyles.Bold;
        option.background = templateImg;
        option.label = optionLabel;
        template.gameObject.SetActive(false);

        // Conexiones del controlador
        ui.panel = panel.gameObject;
        ui.topText = topText;
        ui.leftText = leftText;
        ui.rightText = rightText;
        ui.bottomText = bottomText;
        ui.slot = slot;
        ui.optionsContainer = optionsArea;
        ui.optionTemplate = option;
        ui.executeButton = execute;
        ui.closeButton = close;
        ui.feedbackText = feedback;

        Selection.activeGameObject = canvasGO;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, int size, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button NewButton(string name, Transform parent, string text, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        rt.sizeDelta = new Vector2(200f, 56f);

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        TextMeshProUGUI label = NewText("Texto", rt, text, 28, Color.white);
        Stretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void ConfigureGroup(HorizontalOrVerticalLayoutGroup group, float spacing, RectOffset padding,
        TextAnchor alignment, bool controlWidth, bool controlHeight, bool expandWidth, bool expandHeight)
    {
        group.spacing = spacing;
        group.padding = padding;
        group.childAlignment = alignment;
        group.childControlWidth = controlWidth;
        group.childControlHeight = controlHeight;
        group.childForceExpandWidth = expandWidth;
        group.childForceExpandHeight = expandHeight;
    }
}