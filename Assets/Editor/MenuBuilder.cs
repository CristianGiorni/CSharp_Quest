using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Crea el menu principal (escena nueva) y el menu de pausa (en la escena del juego).
// Menus: CSharp Quest > Crear escena de menu principal / Crear menu de pausa
public static class MenuBuilder
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

    private static readonly Color Background = new Color(0.04f, 0.06f, 0.1f);
    private static readonly Color Cyan = new Color(0.4f, 0.9f, 1f);

    // ---------------------------------------------------------------- MENU PRINCIPAL

    [MenuItem("CSharp Quest/Crear escena de menu principal")]
    public static void BuildMainMenu()
    {
        // Guarda lo que haya abierto antes de cambiar de escena
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Busca la escena del juego
        string gameScenePath = "";
        string[] guids = AssetDatabase.FindAssets("VerticalSlice t:Scene");
        if (guids.Length > 0) gameScenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camara con fondo liso
        GameObject camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        Camera cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Background;

        EnsureEventSystem();
        GameObject canvasGO = NewCanvas("MenuCanvas", 0);
        MainMenu menu = canvasGO.AddComponent<MainMenu>();

        // Panel principal
        RectTransform main = NewRect("PanelPrincipal", canvasGO.transform);
        Stretch(main);

        TextMeshProUGUI title = NewText("Titulo", main, "C# QUEST", 120, Cyan);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        Place(title.rectTransform, 0.5f, 0.72f, new Vector2(1400f, 170f));

        TextMeshProUGUI subtitle = NewText("Subtitulo", main, "Aprende programacion jugando", 38, Color.white);
        subtitle.alignment = TextAlignmentOptions.Center;
        Place(subtitle.rectTransform, 0.5f, 0.6f, new Vector2(1200f, 60f));

        RectTransform list = NewRect("Botones", main);
        Place(list, 0.5f, 0.36f, new Vector2(500f, 260f));
        VerticalLayoutGroup listLayout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 20f;
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = false;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = false;
        listLayout.childForceExpandHeight = false;

        Button play = NewButton("BotonJugar", list, "JUGAR", new Color(0.15f, 0.6f, 0.25f), new Vector2(420f, 70f));
        Button controlsBtn = NewButton("BotonControles", list, "CONTROLES", new Color(0.2f, 0.4f, 0.7f), new Vector2(420f, 70f));
        Button quit = NewButton("BotonSalir", list, "SALIR", new Color(0.35f, 0.35f, 0.4f), new Vector2(420f, 70f));

        // Panel de controles
        RectTransform controls = NewRect("PanelControles", canvasGO.transform);
        Stretch(controls);

        TextMeshProUGUI cTitle = NewText("TituloControles", controls, "CONTROLES", 80, Cyan);
        cTitle.alignment = TextAlignmentOptions.Center;
        cTitle.fontStyle = FontStyles.Bold;
        Place(cTitle.rectTransform, 0.5f, 0.78f, new Vector2(1200f, 110f));

        TextMeshProUGUI cText = NewText("TextoControles", controls,
            "WASD  -  Moverse\n" +
            "E  -  Interactuar (recoger y colocar cajas, usar la consola)\n" +
            "Q o 1, 2, 3  -  Cambiar de caja\n" +
            "Mouse  -  Arrastrar piezas en la terminal\n" +
            "Esc  -  Pausa / cerrar la terminal", 36, Color.white);
        cText.alignment = TextAlignmentOptions.Center;
        Place(cText.rectTransform, 0.5f, 0.52f, new Vector2(1600f, 360f));

        Button back = NewButton("BotonVolver", controls, "VOLVER", new Color(0.35f, 0.35f, 0.4f), new Vector2(420f, 70f));
        Place((RectTransform)back.transform, 0.5f, 0.2f, new Vector2(420f, 70f));

        controls.gameObject.SetActive(false);

        // Conexiones
        menu.mainPanel = main.gameObject;
        menu.controlsPanel = controls.gameObject;
        menu.playButton = play;
        menu.controlsButton = controlsBtn;
        menu.quitButton = quit;
        menu.backButton = back;

        // Registra las escenas en el build (menu primero)
        if (!string.IsNullOrEmpty(gameScenePath))
        {
            menu.gameSceneName = System.IO.Path.GetFileNameWithoutExtension(gameScenePath);
        }

        EditorSceneManager.SaveScene(scene, MenuScenePath);

        if (!string.IsNullOrEmpty(gameScenePath))
        {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(gameScenePath, true)
            };

            EditorUtility.DisplayDialog("Menu principal",
                "Listo. Se creo Assets/Scenes/MainMenu.unity y se registro junto con " + menu.gameSceneName +
                " en el build.\n\nAhora abri la escena del nivel y usa CSharp Quest > Crear menu de pausa.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Menu principal",
                "Se creo la escena del menu, pero no encontre una escena llamada VerticalSlice. " +
                "Escribi el nombre de tu escena en Game Scene Name del MenuCanvas y agregala a File > Build Profiles.", "OK");
        }
    }

    // ---------------------------------------------------------------- PAUSA

    [MenuItem("CSharp Quest/Crear menu de pausa")]
    public static void BuildPauseMenu()
    {
        if (Object.FindFirstObjectByType<PauseMenu>() != null)
        {
            EditorUtility.DisplayDialog("Menu de pausa", "Esta escena ya tiene un menu de pausa.", "OK");
            return;
        }

        EnsureEventSystem();
        GameObject canvasGO = NewCanvas("PauseCanvas", 50);
        PauseMenu pause = canvasGO.AddComponent<PauseMenu>();

        // Fondo oscuro de toda la pantalla
        RectTransform panel = NewRect("Panel", canvasGO.transform);
        Stretch(panel);
        Image overlay = panel.gameObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.7f);

        // Caja central
        RectTransform box = NewRect("Caja", panel);
        Place(box, 0.5f, 0.5f, new Vector2(560f, 560f));
        Image boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = new Color(0.04f, 0.06f, 0.1f, 0.97f);
        Outline border = box.gameObject.AddComponent<Outline>();
        border.effectColor = Cyan;
        border.effectDistance = new Vector2(4f, -4f);

        VerticalLayoutGroup layout = box.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI title = NewText("Titulo", box, "PAUSA", 56, Cyan);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        title.rectTransform.sizeDelta = new Vector2(500f, 80f);

        Vector2 btnSize = new Vector2(420f, 64f);
        Button resume = NewButton("BotonContinuar", box, "CONTINUAR", new Color(0.15f, 0.6f, 0.25f), btnSize);
        Button restart = NewButton("BotonReiniciar", box, "REINICIAR NIVEL", new Color(0.2f, 0.4f, 0.7f), btnSize);
        Button menuBtn = NewButton("BotonMenu", box, "MENU PRINCIPAL", new Color(0.35f, 0.35f, 0.4f), btnSize);
        Button quit = NewButton("BotonSalir", box, "SALIR", new Color(0.6f, 0.2f, 0.2f), btnSize);

        TextMeshProUGUI hint = NewText("Controles", box, "WASD mover  -  E interactuar  -  Q cambiar caja  -  Esc continuar", 20, new Color(0.7f, 0.7f, 0.75f));
        hint.alignment = TextAlignmentOptions.Center;
        hint.rectTransform.sizeDelta = new Vector2(500f, 60f);

        pause.panel = panel.gameObject;
        pause.resumeButton = resume;
        pause.restartButton = restart;
        pause.menuButton = menuBtn;
        pause.quitButton = quit;

        Selection.activeGameObject = canvasGO;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    // ---------------------------------------------------------------- AYUDAS

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Crear EventSystem");
    }

    private static GameObject NewCanvas(string name, int sortingOrder)
    {
        GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(go, "Crear " + name);

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return go;
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

    private static Button NewButton(string name, Transform parent, string text, Color color, Vector2 size)
    {
        RectTransform rt = NewRect(name, parent);
        rt.sizeDelta = size;

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        TextMeshProUGUI label = NewText("Texto", rt, text, 30, Color.white);
        Stretch(label.rectTransform);
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;

        return btn;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Ancla el objeto en un punto de la pantalla (0 a 1) con un tamano fijo
    private static void Place(RectTransform rt, float anchorX, float anchorY, Vector2 size)
    {
        rt.anchorMin = new Vector2(anchorX, anchorY);
        rt.anchorMax = new Vector2(anchorX, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }
}