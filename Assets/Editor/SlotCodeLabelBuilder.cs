using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Agrega a una ranura el texto de codigo flotante, ya configurado.
// Menu: CSharp Quest > Agregar texto de codigo a la ranura
public static class SlotCodeLabelBuilder
{
    [MenuItem("CSharp Quest/Agregar texto de codigo a la ranura")]
    public static void Build()
    {
        GameObject slot = Selection.activeGameObject;

        if (slot == null || slot.GetComponent<SlotReceiver>() == null)
        {
            EditorUtility.DisplayDialog("Texto de codigo",
                "Selecciona el objeto que tiene el SlotReceiver (la ranura) y volve a probar.", "OK");
            return;
        }

        if (slot.GetComponentInChildren<SlotCodeLabel>(true) != null)
        {
            EditorUtility.DisplayDialog("Texto de codigo", "Esta ranura ya tiene un texto de codigo.", "OK");
            return;
        }

        Material bgMat = DarkMaterial();

        // Objeto raiz: siempre mira a la camara
        GameObject root = new GameObject("TextoCodigo");
        Undo.RegisterCreatedObjectUndo(root, "Agregar texto de codigo");
        root.transform.SetParent(slot.transform, false);
        root.transform.localPosition = new Vector3(0f, 2.2f, 0f);

        // Fondo oscuro para que se lea sobre cualquier piso
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "Fondo";
        Object.DestroyImmediate(bg.GetComponent<Collider>());
        bg.transform.SetParent(root.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 0.03f);
        bg.transform.localScale = new Vector3(5.6f, 1.5f, 0.05f);
        bg.GetComponent<Renderer>().sharedMaterial = bgMat;

        // Texto (TextMeshPro 3D)
        GameObject textGO = new GameObject("Texto", typeof(RectTransform));
        textGO.transform.SetParent(root.transform, false);
        textGO.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        TextMeshPro tmp = textGO.AddComponent<TextMeshPro>();
        tmp.text = "bool puenteActivo = [ ___ ];\nif (puenteActivo) BajarPuente();";
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(5.2f, 1.4f);

        SlotCodeLabel label = root.AddComponent<SlotCodeLabel>();
        label.label = tmp;

        Selection.activeGameObject = root;

        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null) EditorSceneManager.MarkSceneDirty(stage.scene);
        else EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static Material DarkMaterial()
    {
        string path = "Assets/Materials/Mat_Codigo_Fondo.mat";

        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Shader shader = temp.GetComponent<Renderer>().sharedMaterial.shader;
        Object.DestroyImmediate(temp);

        Material mat = new Material(shader);
        mat.color = new Color(0.05f, 0.07f, 0.12f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}