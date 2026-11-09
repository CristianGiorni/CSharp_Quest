using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Crea un ascensor basico con cubos, ya configurado.
// Menu: CSharp Quest > Crear Ascensor
public static class ElevatorBuilder
{
    [MenuItem("CSharp Quest/Crear Ascensor")]
    public static void Build()
    {
        // Lo coloca donde esta mirando la vista Scene, a nivel del suelo
        Vector3 pos = Vector3.zero;
        if (SceneView.lastActiveSceneView != null) pos = SceneView.lastActiveSceneView.pivot;
        pos.y = 0f;

        GameObject root = new GameObject("Ascensor");
        Undo.RegisterCreatedObjectUndo(root, "Crear Ascensor");
        root.transform.position = pos;

        Material wallMat = NewMaterial("Mat_Ascensor_Pared", new Color(0.30f, 0.34f, 0.42f));
        Material doorMat = NewMaterial("Mat_Ascensor_Puerta", new Color(0.55f, 0.58f, 0.65f));
        Material lightMat = NewMaterial("Mat_Ascensor_Luz", new Color(0.85f, 0.15f, 0.15f));

        // Cabina: la entrada mira hacia -Z y el fondo esta en +Z
        NewCube("Fondo", root.transform, new Vector3(0f, 1f, 3f), new Vector3(3f, 2f, 0.2f), wallMat, true);
        NewCube("Pared_Izq", root.transform, new Vector3(-1.4f, 1f, 1.5f), new Vector3(0.2f, 2f, 3f), wallMat, true);
        NewCube("Pared_Der", root.transform, new Vector3(1.4f, 1f, 1.5f), new Vector3(0.2f, 2f, 3f), wallMat, true);
        NewCube("Dintel", root.transform, new Vector3(0f, 2.15f, 0f), new Vector3(3f, 0.3f, 0.3f), wallMat, true);

        // Puertas
        GameObject leftDoor = NewCube("Puerta_Izq", root.transform, new Vector3(-0.75f, 1f, 0f), new Vector3(1.5f, 2f, 0.2f), doorMat, true);
        GameObject rightDoor = NewCube("Puerta_Der", root.transform, new Vector3(0.75f, 1f, 0f), new Vector3(1.5f, 2f, 0.2f), doorMat, true);

        // Luces indicadoras sobre las puertas (sin collider)
        GameObject lightL = NewCube("Indicador_Izq", root.transform, new Vector3(-0.75f, 2.15f, -0.18f), new Vector3(0.5f, 0.2f, 0.1f), lightMat, false);
        GameObject lightR = NewCube("Indicador_Der", root.transform, new Vector3(0.75f, 2.15f, -0.18f), new Vector3(0.5f, 0.2f, 0.1f), lightMat, false);

        // Luz real dentro de la cabina
        GameObject lightGO = new GameObject("Luz_Ascensor");
        lightGO.transform.SetParent(root.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 2.5f, 1.5f);
        Light cabinLight = lightGO.AddComponent<Light>();
        cabinLight.type = LightType.Point;
        cabinLight.range = 6f;
        cabinLight.intensity = 2f;
        cabinLight.color = new Color(0.85f, 0.15f, 0.15f);

        // Trigger que detecta al jugador dentro de la cabina
        BoxCollider trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1f, 1.5f);
        trigger.size = new Vector3(2.2f, 2f, 2.4f);

        // Script y conexiones
        Elevator elevator = root.AddComponent<Elevator>();
        elevator.leftDoor = leftDoor.transform;
        elevator.rightDoor = rightDoor.transform;
        elevator.lightRenderers = new Renderer[]
        {
            lightL.GetComponent<Renderer>(),
            lightR.GetComponent<Renderer>()
        };
        elevator.realLights = new Light[] { cabinLight };

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static GameObject NewCube(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat, bool keepCollider)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = mat;

        if (!keepCollider) Object.DestroyImmediate(cube.GetComponent<Collider>());

        return cube;
    }

    // Crea (o reutiliza) un material en Assets/Materials con el shader por defecto del proyecto
    private static Material NewMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";

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
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}