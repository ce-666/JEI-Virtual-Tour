using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Independent, local-only courtyard furnishing and review tools.</summary>
[InitializeOnLoad]
public static class JEICourtyardTools
{
    const string ScenePath = "Assets/Scenes/JEI_Floor01_Structure.unity";
    const string FloorPath = "Floor_01__36x26m__Storey_3.60m";
    const string OwnedName = "07_Courtyard_Commons";
    const string Request = "Library/JEI_Courtyard.request";
    const string Result = "Library/JEI_Courtyard.result";
    const string Output = "Documentation/Courtyard_Commons";
    static double nextPoll;

    static JEICourtyardTools() { EditorApplication.update += Poll; }

    static void Poll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + .75;
        if (!File.Exists(Request)) return;
        string action = File.ReadAllText(Request).Trim();
        File.Delete(Request);
        try
        {
            if (action == "build") Apply();
            else if (action == "details") ApplyDetails();
            else if (action == "lighting") ApplyLighting();
            else if (action == "verify") File.WriteAllText(Result, "SUCCESS: " + Verify());
            else if (action == "capture") { Capture(); File.WriteAllText(Result, "SUCCESS: courtyard 3D previews saved."); }
            else if (action == "focus") { Focus(); File.WriteAllText(Result, "SUCCESS: courtyard focused in Scene view."); }
            else throw new ArgumentException("Unknown courtyard action: " + action);
        }
        catch (Exception error) { File.WriteAllText(Result, "ERROR: " + error); Debug.LogException(error); }
    }

    static Transform RequireFloor()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
            throw new InvalidOperationException("Open JEI_Floor01_Structure in Edit mode first.");
        var building = GameObject.Find("JEI_Structure_Study");
        var floor = building ? building.transform.Find(FloorPath) : null;
        if (!floor) throw new InvalidOperationException("The first-floor structure is missing.");
        return floor;
    }

    [MenuItem("Tools/JEI/Courtyard/Apply Green Island and Curved Benches")]
    public static void Apply()
    {
        var floor = RequireFloor();
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Courtyard_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, "Library/JEI_Courtyard_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_courtyard.unity", true);
        var previous = floor.Find(OwnedName);
        if (previous)
        {
            if (!previous.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The courtyard group is not generator-owned; existing work was preserved.");
            PreserveManual(previous, floor);
            Undo.DestroyObjectImmediate(previous.gameObject);
        }
        var owned = JEIFurnitureKit.Group(floor, OwnedName);
        JEICourtyardCommons.Build(owned);
        Mark(owned);
        Undo.RegisterCreatedObjectUndo(owned.gameObject, "Add courtyard green island and curved benches");
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save courtyard furnishings.");
        AssetDatabase.SaveAssets();
        Capture(); Focus();
        File.WriteAllText(Result, "SUCCESS: courtyard saved locally; existing rooms and structure preserved.\n" + verification);
        Debug.Log("JEI courtyard saved locally for review.");
    }

    [MenuItem("Tools/JEI/Courtyard/Add Student Artwork and Corner Planting")]
    public static void ApplyDetails()
    {
        var floor = RequireFloor();
        var owned = floor.Find(OwnedName);
        var courtyard = owned ? owned.Find("C04__Courtyard_Commons") : null;
        if (!courtyard || !courtyard.GetComponent<JEIGeneratedObject>())
            throw new InvalidOperationException("Apply the generated courtyard before adding its details.");
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Courtyard_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, "Library/JEI_Courtyard_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_courtyard_details.unity", true);
        var previous = courtyard.Find("C04_Art_and_Corner_Planting");
        if (previous)
        {
            if (!previous.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The detail group is not generator-owned; it was preserved.");
            PreserveManual(previous, courtyard);
            Undo.DestroyObjectImmediate(previous.gameObject);
        }
        var details = JEICourtyardCommons.AddDetails(courtyard);
        Mark(details);
        Undo.RegisterCreatedObjectUndo(details.gameObject, "Add courtyard student artwork and low corner planting");
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save courtyard details.");
        AssetDatabase.SaveAssets();
        Capture(); Focus();
        File.WriteAllText(Result, "SUCCESS: student artwork and two corner planters saved; existing courtyard furniture preserved.\n" + verification);
        Debug.Log("JEI courtyard artwork and corner planting saved locally for review.");
    }

    [MenuItem("Tools/JEI/Courtyard/Add Warm Courtyard Lighting")]
    public static void ApplyLighting()
    {
        var floor = RequireFloor();
        var owned = floor.Find(OwnedName);
        var courtyard = owned ? owned.Find("C04__Courtyard_Commons") : null;
        if (!courtyard || !courtyard.GetComponent<JEIGeneratedObject>())
            throw new InvalidOperationException("Apply the generated courtyard before adding its lighting.");
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Courtyard_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, "Library/JEI_Courtyard_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_courtyard_lighting.unity", true);
        var previous = courtyard.Find("C04_Warm_Lighting");
        if (previous)
        {
            if (!previous.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The lighting group is not generator-owned; it was preserved.");
            PreserveManual(previous, courtyard);
            Undo.DestroyObjectImmediate(previous.gameObject);
        }
        var lighting = JEICourtyardCommons.AddLighting(courtyard);
        Mark(lighting);
        Undo.RegisterCreatedObjectUndo(lighting.gameObject, "Add warm courtyard lighting");
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save courtyard lighting.");
        AssetDatabase.SaveAssets();
        Capture(); Focus();
        File.WriteAllText(Result, "SUCCESS: warm courtyard lighting saved; existing courtyard furniture and daylight settings preserved.\n" + verification);
        Debug.Log("JEI warm courtyard lighting saved locally for review.");
    }

    static void Mark(Transform node)
    {
        if (!node.GetComponent<JEIGeneratedObject>()) node.gameObject.AddComponent<JEIGeneratedObject>();
        foreach (Transform child in node) Mark(child);
    }

    static void PreserveManual(Transform source, Transform destination)
    {
        for (int i = source.childCount - 1; i >= 0; i--)
        {
            var child = source.GetChild(i);
            if (!child.GetComponent<JEIGeneratedObject>()) child.SetParent(destination, true);
            else PreserveManual(child, destination);
        }
    }

    [MenuItem("Tools/JEI/Courtyard/Verify Courtyard Routes")]
    public static void VerifyMenu() { File.WriteAllText(Result, "SUCCESS: " + Verify()); }

    public static string Verify()
    {
        var floor = RequireFloor();
        var owned = floor.Find(OwnedName);
        if (!owned) throw new InvalidOperationException("Courtyard furnishings are missing.");
        Physics.SyncTransforms();
        var nodes = owned.GetComponentsInChildren<Transform>();
        foreach (string name in new[] { "C04__Courtyard_Commons", "C04_Stone_paving", "C04_Low_planting_island", "C04_Courtyard_tree", "C04_Underplanting", "C04_Curved_bench_01", "C04_Curved_bench_02", "C04_Art_and_Corner_Planting", "C04_Student_art_display", "C04_Corner_planter_01", "C04_Corner_planter_02" })
            if (nodes.Count(t => t.name == name) != 1) throw new InvalidOperationException("Courtyard furnishing is incomplete: " + name);
        foreach (var collider in owned.GetComponentsInChildren<Collider>())
        {
            var b = collider.bounds;
            if (b.min.x < -3.91f || b.max.x > 3.91f || b.min.z < -3.21f || b.max.z > 3.21f)
                throw new InvalidOperationException("Courtyard furnishings extend outside C04: " + collider.name);
        }
        var tree = nodes.Single(t => t.name == "C04_Courtyard_tree");
        foreach (var renderer in tree.GetComponentsInChildren<Renderer>())
        {
            var b = renderer.bounds;
            if (b.max.y > 2.96f || b.min.x < -2.31f || b.max.x > .01f || b.min.z < -.61f || b.max.z > 1.71f)
                throw new InvalidOperationException("The courtyard tree exceeds its agreed canopy envelope: " + renderer.name);
        }
        var artwork = nodes.Single(t => t.name == "C04_Student_art_display");
        foreach (var renderer in artwork.GetComponentsInChildren<Renderer>())
        {
            var b = renderer.bounds;
            if (b.max.y > 1.76f || b.min.x < 2.88f || b.max.x > 3.62f || b.min.z < .91f || b.max.z > 1.79f)
                throw new InvalidOperationException("The student artwork exceeds its display footprint: " + renderer.name);
        }
        foreach (var planter in nodes.Where(t => t.name == "C04_Corner_planter_01" || t.name == "C04_Corner_planter_02"))
        {
            float centreZ = planter.position.z;
            foreach (var renderer in planter.GetComponentsInChildren<Renderer>())
            {
                var b = renderer.bounds;
                if (b.max.y > .86f || b.min.x < 2.015f || b.max.x > 3.685f || b.min.z < centreZ-.26f || b.max.z > centreZ+.26f)
                    throw new InvalidOperationException("Corner planting exceeds its low window-side footprint: " + renderer.name);
            }
        }
        foreach (var mesh in owned.GetComponentsInChildren<MeshFilter>())
            if (!mesh.sharedMesh) throw new InvalidOperationException("A courtyard mesh is missing: " + mesh.name);
        foreach (var renderer in owned.GetComponentsInChildren<Renderer>())
            if (renderer.sharedMaterials.Any(m => !m)) throw new InvalidOperationException("A courtyard material is missing: " + renderer.name);
        var plan = JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        int doors = 0;
        foreach (var wall in plan.walls) foreach (var opening in wall.openings)
        {
            if (opening.kind != "door") continue;
            bool horizontal = wall.axis == "H";
            var center = horizontal ? new Vector3(opening.at, 1, wall.at) : new Vector3(wall.at, 1, opening.at);
            var half = horizontal ? new Vector3(opening.width / 2 - .08f, .7f, .07f) : new Vector3(.07f, .7f, opening.width / 2 - .08f);
            foreach (var hit in Physics.OverlapBox(center, half))
                if (!(hit is CharacterController)) throw new InvalidOperationException("Door obstructed: " + hit.name);
            doors++;
        }
        foreach (float z in new[] { -4.6f, 4.6f })
            if (Physics.BoxCast(new Vector3(-17.5f, 1, z), new Vector3(.03f, .7f, 1.16f), Vector3.right, out var blocked, Quaternion.identity, 35))
                throw new InvalidOperationException("Main corridor obstruction: " + blocked.collider.name);
        foreach (float x in new[] { -5.3f, 5.3f })
            if (Physics.BoxCast(new Vector3(x, 1, -3.1f), new Vector3(1.16f, .7f, .03f), Vector3.forward, out var blocked, Quaternion.identity, 6.2f))
                throw new InvalidOperationException("Courtyard-side link obstruction: " + blocked.collider.name);
        var probe = new GameObject("Temporary courtyard walk probe");
        var controller = probe.AddComponent<CharacterController>();
        controller.height = 1.8f; controller.center = new Vector3(0, .9f, 0); controller.radius = .3f; controller.skinWidth = .035f; controller.stepOffset = .25f;
        try
        {
            WalkRoute(controller, new[] { new Vector2(0, -4.6f), new Vector2(0, -2.55f), new Vector2(1.35f, -1.80f), new Vector2(1.35f, .55f), new Vector2(.97f, .55f) });
            var ring = new List<Vector2>();
            for (int i = 0; i <= 144; i++)
            {
                float angle = i * 2.5f * Mathf.Deg2Rad;
                ring.Add(new Vector2(-1.15f + 2.12f * Mathf.Cos(angle), .55f + 2.12f * Mathf.Sin(angle)));
            }
            WalkRoute(controller, ring.ToArray());
            WalkRoute(controller, new[] { new Vector2(.97f, .55f), new Vector2(1.35f, .55f), new Vector2(2.8f, .55f), new Vector2(2.8f, -2.1f), new Vector2(1.70f, -2.10f), new Vector2(0, -2.55f), new Vector2(0, -4.6f) });
            WalkRoute(controller, new[] { new Vector2(1.35f, .55f), new Vector2(2.35f, .55f), new Vector2(2.35f, 1.35f), new Vector2(2.10f, 1.35f), new Vector2(2.10f, 2.05f), new Vector2(2.10f, .55f), new Vector2(2.10f, -2.05f), new Vector2(2.80f, -2.10f), new Vector2(1.70f, -2.10f), new Vector2(0, -2.55f), new Vector2(0, -4.6f) });
        }
        finally { Object.DestroyImmediate(probe); Physics.SyncTransforms(); }
        string lightingReport = "";
        var lighting = nodes.SingleOrDefault(t => t.name == "C04_Warm_Lighting");
        if (lighting)
        {
            var lights = lighting.GetComponentsInChildren<Light>();
            if (lights.Length != 5 || lighting.GetComponentsInChildren<Collider>().Length != 0)
                throw new InvalidOperationException("Warm lighting must contain five real lights and no new walking obstacles.");
            foreach (var light in lights)
                if (!light.enabled || light.type != LightType.Spot || !light.useColorTemperature || Mathf.Abs(light.colorTemperature-3000) > 1 || light.intensity <= 0 || light.range > 4.1f || light.lightmapBakeType != LightmapBakeType.Realtime)
                    throw new InvalidOperationException("Invalid courtyard warm-light configuration: " + light.name);
            foreach (var renderer in lighting.GetComponentsInChildren<Renderer>())
            {
                var b = renderer.bounds;
                if (b.min.x < -3.91f || b.max.x > 3.91f || b.min.z < -3.21f || b.max.z > 3.21f || b.max.y > 2.50f)
                    throw new InvalidOperationException("A courtyard lamp exceeds its agreed envelope: " + renderer.name);
            }
            var emitters = lighting.GetComponentsInChildren<Renderer>().Where(r => r.sharedMaterials.Any(m => m.IsKeywordEnabled("_EMISSION") && m.GetColor("_EmissionColor").maxColorComponent > 0)).ToArray();
            if (emitters.Length < 5) throw new InvalidOperationException("Warm lamps are missing their luminous diffusers.");
            lightingReport = "PASS: five real-time 3000K spot lights and luminous diffusers are present: two wall lamps, two hidden bench lights and one artwork accent; no new colliders or blocked routes.\n";
        }
        string report = "PASS: one low planted island, one small tree, two outward-facing curved timber benches and stone paving are present.\n" +
            "PASS: the actual CharacterController reaches C04 through its existing south door, follows the complete outer seating loop, reaches the open east side and returns to the public corridor.\n" +
            "PASS: all " + doors + " original doors, both 2.40m main corridors and both 2.40m courtyard-side links remain unobstructed.\n" +
            "PASS: all courtyard colliders remain inside C04; the tree canopy stays clear of the walls and below 2.96m.\n" +
            "PASS: all generated courtyard mesh and material references resolve.\n" +
            "PASS: one student-art display and two low corner planters are present; the actual CharacterController reaches the artwork viewing point and both planter approaches without blocking the original seating loop or entrance.\n" +
            "PASS: the artwork remains within its display footprint and below 1.76m; corner planting remains below the existing 0.90m window sills.\n" +
            lightingReport +
            "Scope: editor-mode geometry and collision checks; no Play-mode run performed for this courtyard draft.";
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "/Verification.txt", report);
        return report;
    }

    static void WalkRoute(CharacterController controller, Vector2[] points)
    {
        controller.enabled = false;
        controller.transform.position = new Vector3(points[0].x, .15f, points[0].y);
        controller.enabled = true;
        Physics.SyncTransforms();
        for (int i = 0; i < 12; i++) controller.Move(Vector3.down * .025f);
        for (int n = 1; n < points.Length; n++)
        {
            bool reached = false;
            for (int i = 0; i < 1500; i++)
            {
                var p = controller.transform.position;
                Vector2 delta = points[n] - new Vector2(p.x, p.z);
                if (delta.magnitude < .035f) { reached = true; break; }
                var step = Vector2.ClampMagnitude(delta, .035f);
                controller.Move(new Vector3(step.x, -.035f, step.y));
            }
            if (!reached) throw new InvalidOperationException("Courtyard route blocked near " + controller.transform.position + " toward " + points[n]);
        }
    }

    [MenuItem("Tools/JEI/Courtyard/Focus Courtyard")]
    public static void Focus()
    {
        RequireFloor(); Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(-.3f, .8f, .25f), Quaternion.Euler(55, -15, 0), 6.2f, false, true);
        view.Focus(); view.Repaint();
    }

    [MenuItem("Tools/JEI/Courtyard/Capture Courtyard Previews")]
    public static void Capture()
    {
        RequireFloor(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary courtyard review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "Documentation/Floor01_Furnishings/01_Overview.png", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, Output + "/01_Courtyard_3D_Overview.png", new Vector3(6.4f, 8.1f, -8.8f), new Vector3(-.3f, .7f, .2f), 58);
            Shot(camera, Output + "/02_From_Entrance.png", new Vector3(.05f, 2.0f, -2.85f), new Vector3(.15f, 1.30f, .55f), 84);
            Shot(camera, Output + "/03_Curved_Benches.png", new Vector3(1.55f, 1.75f, -.95f), new Vector3(-1.15f, .85f, .55f), 76);
            Shot(camera, Output + "/04_East_Open_Space.png", new Vector3(-.2f, 2.1f, -1.1f), new Vector3(2.5f, 1.0f, 1.4f), 74);
            Shot(camera, Output + "/05_Student_Artwork.png", new Vector3(1.7f, 1.60f, -.20f), new Vector3(3.23f, 1.05f, 1.33f), 70);
            Shot(camera, Output + "/06_Low_Corner_Planting.png", new Vector3(1.1f, 1.50f, -.5f), new Vector3(2.85f, .45f, -2.8f), 68);
            if (RequireFloor().Find(OwnedName + "/C04__Courtyard_Commons/C04_Warm_Lighting")) CaptureEvening(camera);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    // Review-only dusk exposure. Every global setting is restored before returning;
    // the saved scene retains its original daylight and the new local lamps.
    static void CaptureEvening(Camera camera)
    {
        var mode = RenderSettings.ambientMode;
        var ambient = RenderSettings.ambientLight;
        float ambientIntensity = RenderSettings.ambientIntensity;
        Color background = camera.backgroundColor;
        var directional = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l => l.type == LightType.Directional).ToArray();
        var intensities = directional.Select(l => l.intensity).ToArray();
        try
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.14f, .17f, .22f);
            RenderSettings.ambientIntensity = 1;
            foreach (var light in directional) light.intensity *= .12f;
            camera.backgroundColor = new Color(.12f, .16f, .22f);
            Shot(camera, Output + "/07_Warm_Lighting_Evening.png", new Vector3(.05f, 2.0f, -2.85f), new Vector3(.15f, 1.30f, .55f), 84);
            Shot(camera, Output + "/08_Artwork_Lighting_Evening.png", new Vector3(-.2f, 2.1f, -1.1f), new Vector3(2.5f, 1.0f, 1.4f), 74);
        }
        finally
        {
            for (int i = 0; i < directional.Length; i++) if (directional[i]) directional[i].intensity = intensities[i];
            RenderSettings.ambientMode = mode; RenderSettings.ambientLight = ambient; RenderSettings.ambientIntensity = ambientIntensity;
            camera.backgroundColor = background;
        }
    }

    static void Shot(Camera camera, string path, Vector3 position, Vector3 target, float fieldOfView)
    {
        camera.transform.position = position; camera.transform.LookAt(target); camera.fieldOfView = fieldOfView;
        var previous = RenderTexture.active; var previousTarget = camera.targetTexture;
        var rt = new RenderTexture(1800, 1200, 24); var tex = new Texture2D(1800, 1200, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1800, 1200), 0, 0); tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }
        finally { RenderTexture.active = previous; camera.targetTexture = previousTarget; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex); }
    }
}
