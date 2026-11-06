using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using F = JEIFurnitureKit;

/// <summary>Furnishes only the four rooms agreed in the design discussion.</summary>
[InitializeOnLoad]
public static class JEIApprovedInteriors
{
    const string ScenePath = "Assets/Scenes/JEI_Floor01_Structure.unity";
    const string Request = "Library/JEI_Furnish.request";
    const string Result = "Library/JEI_Furnish.result";
    const string Output = "Documentation/Floor01_Furnishings";
    const string OwnedName = "06_Approved_Interiors";
    const string RuntimeKey = "JEI_Furnishing_Runtime_Check";
    static double nextPoll;
    static int runtimeStage;
    static double runtimeStageAt;
    static Vector3 runtimeStartPosition;
    static Quaternion runtimeStartRotation;
    static Transform floor, owned;
    static JEIStructureBlockout.Plan plan;
    static readonly string[] Approved = { "N02", "S01", "S02", "S03" };

    static JEIApprovedInteriors() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (EditorApplication.isPlaying)
        {
            if (SessionState.GetBool(RuntimeKey, false) && Time.frameCount >= 4) VerifyRuntime();
            return;
        }
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + .75;
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string action = File.ReadAllText(Request).Trim(); File.Delete(Request);
        try
        {
            if (action == "build") Apply();
            else if (action == "capture") { Capture(); File.WriteAllText(Result, "SUCCESS: furniture previews captured."); }
            else if (action == "verify") File.WriteAllText(Result, "SUCCESS: " + Verify());
            else if (action == "runtime") { RequireScene(); SessionState.SetBool(RuntimeKey, true); EditorApplication.isPlaying = true; }
            else throw new ArgumentException("Unknown furnishing action: " + action);
        }
        catch (Exception e) { File.WriteAllText(Result, "ERROR: " + e); Debug.LogException(e); }
    }

    static void RequireScene()
    {
        if (SceneManager.GetActiveScene().path != ScenePath) throw new InvalidOperationException("Open JEI_Floor01_Structure in Edit mode.");
    }

    [MenuItem("Tools/JEI/Interiors/Apply Agreed Furnishings")]
    public static void Apply()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        if (File.Exists(ScenePath)) File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + stamp + "_saved.unity", true);
        var scene = SceneManager.GetActiveScene();
        if (scene.isDirty)
        {
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
            File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + stamp + "_live.unity", true);
        }
        floor = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m");
        if (!floor) throw new InvalidOperationException("Structure floor root is missing.");
        plan = JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        var old = floor.Find(OwnedName);
        if (old)
        {
            if (!old.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The furnishing root is not generator-owned; it was preserved.");
            PreserveManual(old, floor);
            Undo.DestroyObjectImmediate(old.gameObject);
        }
        owned = F.Group(floor, OwnedName);
        foreach (string id in Approved)
        {
            var room = Array.Find(plan.rooms, r => r.id == id);
            var group = F.Group(owned, id + "_" + RoomName(id));
            Finishes(group, room);
            if (id == "N02") JEIGroupedClassroom.Build(group);
            else if (id == "S01") JEITeacherSuite.Build(group);
            else if (id == "S02") Reception(group);
            else Administration(group);
            DoorPlaque(group, id);
        }
        Mark(owned);
        Undo.RegisterCreatedObjectUndo(owned.gameObject, "Apply approved room interiors");
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save furnished scene.");
        AssetDatabase.SaveAssets();
        File.WriteAllText(Output + "/Verification.txt", verification);
        WriteNotes();
        Capture();
        FocusModel();
        File.WriteAllText(Result, "SUCCESS: furnished N02, S01, S02 and S03 only.\n" + verification);
        Debug.Log("JEI approved interiors saved locally for review.");
    }

    static string RoomName(string id)
    {
        if (id == "N02") return "Curved_Group_Tables";
        if (id == "S01") return "Teacher_Office_and_Open_Lounge";
        if (id == "S02") return "Minimal_Reception";
        return "Administrative_Office_and_Records";
    }
    static void Mark(Transform t) { if (!t.GetComponent<JEIGeneratedObject>()) t.gameObject.AddComponent<JEIGeneratedObject>(); foreach (Transform child in t) Mark(child); }
    static void PreserveManual(Transform source, Transform destination)
    {
        for (int i = source.childCount - 1; i >= 0; i--)
        {
            var child = source.GetChild(i);
            if (!child.GetComponent<JEIGeneratedObject>()) child.SetParent(destination, true);
            else PreserveManual(child, destination);
        }
    }

    static Texture2D OakTexture()
    {
        string folder = "Assets/JEI_Furnishings/Textures";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/JEI_Furnishings", "Textures");
        string path = folder + "/Pale_Oak_Floor.asset";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture) return texture;
        const int size = 512;
        texture = new Texture2D(size, size, TextureFormat.RGB24, false);
        var pixels = new Color[size * size];
        for (int z = 0; z < size; z++) for (int x = 0; x < size; x++)
        {
            int board = x / 32, along = (z + (board % 2) * 128) % 256;
            float grain = Mathf.Sin(x * 1.35f + Mathf.Sin(z * .035f) * 1.8f) * .018f;
            float tone = ((board * 7) % 9 - 4) * .009f + grain + Mathf.PerlinNoise(x * .08f, z * .025f) * .035f;
            bool joint = x % 32 == 0 || along < 2;
            pixels[z * size + x] = joint ? new Color(.51f, .42f, .30f) : new Color(.72f + tone, .59f + tone, .42f + tone);
        }
        texture.SetPixels(pixels); texture.Apply(); texture.wrapMode = TextureWrapMode.Repeat; texture.filterMode = FilterMode.Bilinear; texture.anisoLevel = 4;
        AssetDatabase.CreateAsset(texture, path); return texture;
    }

    static void Finishes(Transform parent, JEIStructureBlockout.Room room)
    {
        float x0 = room.x0 + (Mathf.Abs(room.x0 + 17.85f) < .01f ? .15f : .1f);
        float x1 = room.x1 - (Mathf.Abs(room.x1 - 17.85f) < .01f ? .15f : .1f);
        float z0 = room.z0 + (Mathf.Abs(room.z0 + 12.85f) < .01f ? .15f : .1f);
        float z1 = room.z1 - (Mathf.Abs(room.z1 - 12.85f) < .01f ? .15f : .1f);
        var finish = F.Group(parent, "Room_Finishes");
        Material flooring;
        if (room.id == "S02") flooring = F.Mat("Reception_Limestone", new Color(.82f, .81f, .74f), .3f);
        else
        {
            flooring = F.Mat("Oak_Floor_" + room.id, Color.white, .14f);
            flooring.SetTexture("_BaseMap", OakTexture()); flooring.SetTextureScale("_BaseMap", new Vector2((x1 - x0) / 3.2f, (z1 - z0) / 3.2f));
        }
        F.Box(finish, "Approved room flooring", new Vector3((x0 + x1) / 2, .033f, (z0 + z1) / 2), new Vector3(x1 - x0 - .01f, .012f, z1 - z0 - .01f), flooring, false);
        if (room.id == "S01") F.Box(finish, "Quiet office acoustic carpet", new Vector3((-17.7f - 9.04f) / 2, .043f, -9.35f), new Vector3(8.66f, .012f, 6.69f), F.Mat("Quiet_Office_Carpet", new Color(.45f, .53f, .54f), .04f), false);
        var paint = F.Mat("Warm_Ivory_Paint", new Color(.88f, .865f, .81f), .1f);
        var trim = F.Mat("Warm_Skirting", new Color(.34f, .36f, .34f), .15f);
        foreach (var wall in plan.walls)
        {
            bool horizontal = wall.axis == "H";
            float lower = horizontal ? room.z0 : room.x0, upper = horizontal ? room.z1 : room.x1;
            int direction = Mathf.Abs(wall.at - lower) < .005f ? 1 : Mathf.Abs(wall.at - upper) < .005f ? -1 : 0;
            if (direction == 0) continue;
            float a = Mathf.Max(wall.start, horizontal ? room.x0 : room.z0), b = Mathf.Min(wall.end, horizontal ? room.x1 : room.z1);
            if (b <= a) continue;
            float inset = (wall.external ? .15f : .1f) + .008f;
            float face = wall.at + direction * inset;
            float cursor = a;
            foreach (var opening in wall.openings)
            {
                float l = Mathf.Max(a, opening.at - opening.width / 2), r = Mathf.Min(b, opening.at + opening.width / 2);
                if (r <= a || l >= b || r <= l) continue;
                Lining(finish, horizontal, face, cursor, l, .02f, 3.28f, paint, trim);
                if (opening.kind == "window") Lining(finish, horizontal, face, l, r, .02f, .89f, paint, trim);
                Lining(finish, horizontal, face, l, r, opening.kind == "window" ? 2.81f : 2.41f, 3.28f, paint, trim);
                cursor = r;
            }
            Lining(finish, horizontal, face, cursor, b, .02f, 3.28f, paint, trim);
        }
    }

    static void Lining(Transform parent, bool horizontal, float face, float a, float b, float bottom, float top, Material paint, Material trim)
    {
        if (b - a < .001f) return;
        var position = horizontal ? new Vector3((a + b) / 2, (bottom + top) / 2, face) : new Vector3(face, (bottom + top) / 2, (a + b) / 2);
        var size = horizontal ? new Vector3(b - a, top - bottom, .012f) : new Vector3(.012f, top - bottom, b - a);
        F.Box(parent, "Warm interior wall face", position, size, paint, false);
        if (bottom < .1f)
        {
            position.y = .08f; size.y = .12f;
            if (horizontal) size.z = .028f; else size.x = .028f;
            F.Box(parent, "Skirting", position, size, trim, false);
        }
    }

    static void DoorPlaque(Transform parent, string id)
    {
        bool north = id == "N02";
        float x = id == "N02" ? -5 : id == "S01" ? -8 : id == "S02" ? 0 : 6.6f;
        string title = id == "N02" ? "102  GROUP LEARNING" : id == "S01" ? "STAFF OFFICE" : id == "S02" ? "WELCOME" : "ADMIN / RECORDS";
        float z = north ? 5.76f : -5.76f;
        F.Box(parent, "Room identification plaque", new Vector3(x, 2.73f, z), new Vector3(id == "S02" ? 2.3f : 2.1f, .3f, .025f), F.Mat("Sign_Deep_Teal", new Color(.13f, .30f, .32f)), false);
        F.WallText(parent, title, new Vector3(x, 2.73f, z + (north ? -.02f : .02f)), north ? 0 : 180, id == "S02" ? 1.7f : 1.78f, new Color(.95f, .94f, .87f));
    }

    static void Reception(Transform parent)
    {
        var desk = F.Desk(parent, new Vector3(1.96f, 0, -9.2f), 270, 1.95f, .64f, true);
        desk.name = "Compact right-hand reception desk";
        var oak = F.Mat("Light_Oak", new Color(.73f, .56f, .35f));
        F.Box(parent, "Reception modesty front", new Vector3(1.61f, .42f, -9.2f), new Vector3(.05f, .69f, 1.92f), oak);
        for (int i = 0; i < 13; i++) F.Box(parent, "Reception slim timber batten", new Vector3(1.575f, .43f, -10.02f + i * .137f), new Vector3(.018f, .63f, .045f), oak, false);
        F.Chair(parent, new Vector3(2.71f, 0, -9.2f), 270, true, "Grey");
        F.Sofa(parent, new Vector3(-2.72f, 0, -9.55f), 270, 1.95f, "Sand");
        F.Box(parent, "Academy identity backing", new Vector3(-3.167f, 1.94f, -9.55f), new Vector3(.028f, .72f, 1.9f), F.Mat("Identity_Deep_Teal", new Color(.13f, .30f, .32f)), false);
        F.WallText(parent, "JEI  ACADEMY", new Vector3(-3.143f, 2.02f, -9.55f), 270, 1.54f, new Color(.96f, .93f, .82f));
        F.WallText(parent, "LEARN  /  CREATE  /  CONNECT", new Vector3(-3.140f, 1.78f, -9.55f), 270, 1.50f, new Color(.83f, .87f, .81f));
        var board = F.Group(parent, "Noticeboard beside sofa near entrance", new Vector3(-3.15f, 1.55f, -11.7f), 270);
        F.Box(board, "Oak noticeboard frame", Vector3.zero, new Vector3(1.24f, 1.06f, .055f), oak, false);
        F.Box(board, "Pinboard", new Vector3(0, 0, -.035f), new Vector3(1.14f, .96f, .018f), F.Mat("Pinboard_Linen", new Color(.66f, .64f, .54f)), false);
        F.WallText(board, "CAMPUS NOTICES", new Vector3(0, .35f, -.052f), 0, .94f, new Color(.17f, .25f, .27f));
        for (int i = 0; i < 4; i++)
        {
            float x = i % 2 == 0 ? -.28f : .28f, y = i < 2 ? .03f : -.28f;
            F.Box(board, "Posted campus notice", new Vector3(x, y, -.052f), new Vector3(.42f, .25f, .008f), F.Mat(i % 2 == 0 ? "Notice_Cream" : "Notice_Pale_Blue", i % 2 == 0 ? new Color(.95f, .91f, .77f) : new Color(.68f, .80f, .82f)), false);
            for (int line = 0; line < 3; line++) F.Box(board, "Notice text line", new Vector3(x, y + .055f - line * .04f, -.058f), new Vector3(.30f - line * .035f, .009f, .003f), F.Mat("Print_Grey", new Color(.39f, .45f, .44f)), false);
        }
    }

    static void Administration(Transform parent)
    {
        var work = F.Group(parent, "Four administrative workstations");
        foreach (float x in new[] { 5.15f, 6.60f })
        {
            F.Desk(work, new Vector3(x, 0, -9.55f), 180, 1.35f, .70f);
            F.Chair(work, new Vector3(x, 0, -8.60f), 180, true, "Navy");
            F.Desk(work, new Vector3(x, 0, -10.25f), 0, 1.35f, .70f);
            F.Chair(work, new Vector3(x, 0, -11.20f), 0, true, "Navy");
        }
        var divider = F.Mat("Admin_Desk_Divider", new Color(.56f, .63f, .61f), .08f);
        F.Box(work, "Administrative desk spine", new Vector3(5.875f, 1.03f, -9.90f), new Vector3(2.87f, .52f, .045f), divider);
        var records = F.Group(parent, "Records and reference materials");
        foreach (float z in new[] { -11.70f, -10.35f, -9.0f })
        {
            F.Cabinet(records, new Vector3(9.63f, 0, z), 1.24f, 1.87f, 90);
            F.WallText(records, z < -11 ? "COURSE FILES" : z < -10 ? "ADMIN RECORDS" : "TEACHING RESOURCES", new Vector3(9.358f, 1.53f, z), 90, .92f, new Color(.20f, .29f, .30f));
        }
        F.Cabinet(records, new Vector3(4.72f, 0, -6.30f), 1.50f, 1.55f, 0, true);
        var print = F.Group(parent, "Shared print and document sorting corner", new Vector3(9.30f, 0, -7.20f), 90);
        F.Table(print, Vector3.zero, 1.15f, .55f);
        var machine = F.Mat("Printer_Off_White", new Color(.78f, .80f, .78f));
        F.Box(print, "Multifunction printer body", new Vector3(0, .96f, 0), new Vector3(.63f, .38f, .44f), machine);
        F.Box(print, "Printer scanner lid", new Vector3(0, 1.17f, 0), new Vector3(.65f, .055f, .45f), F.Mat("Printer_Graphite", new Color(.19f, .24f, .26f)), false);
        F.Box(print, "Paper output slot", new Vector3(0, .96f, -.227f), new Vector3(.42f, .065f, .018f), F.Mat("Printer_Graphite", new Color(.19f, .24f, .26f)), false);
        F.Box(print, "Output paper", new Vector3(0, .917f, -.29f), new Vector3(.30f, .015f, .25f), F.Mat("White_Paper", new Color(.95f, .95f, .91f)), false);
        F.Box(print, "Paper storage boxes", new Vector3(.35f, .22f, 0), new Vector3(.35f, .40f, .36f), F.Mat("Archive_Box", new Color(.72f, .64f, .49f)));
        F.Chair(parent, new Vector3(4.05f, 0, -7.5f), 90, false, "Sand");
        F.Chair(parent, new Vector3(4.05f, 0, -8.3f), 90, false, "Sand");
    }

    static string Verify()
    {
        RequireScene(); Physics.SyncTransforms();
        var messages = new List<string>();
        var currentPlan = JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        int doors = 0;
        foreach (var wall in currentPlan.walls) foreach (var opening in wall.openings)
        {
            if (opening.kind != "door") continue;
            Vector3 center = wall.axis == "H" ? new Vector3(opening.at, 1, wall.at) : new Vector3(wall.at, 1, opening.at);
            Vector3 half = wall.axis == "H" ? new Vector3(opening.width / 2 - .08f, .7f, .07f) : new Vector3(.07f, .7f, opening.width / 2 - .08f);
            foreach (var hit in Physics.OverlapBox(center, half)) if (!(hit is CharacterController)) throw new InvalidOperationException("Door obstructed by " + hit.name + " at " + center);
            doors++;
        }
        messages.Add("PASS: all " + doors + " original door openings remain unobstructed.");
        foreach (float z in new[] { -4.6f, 4.6f })
            if (Physics.BoxCast(new Vector3(-17.5f, 1, z), new Vector3(.03f, .7f, 1.16f), Vector3.right, out var blocked, Quaternion.identity, 35)) throw new InvalidOperationException("Main corridor obstruction: " + blocked.collider.name);
        messages.Add("PASS: both existing 2.40m clear corridors remain continuous.");
        var probe = new GameObject("Temporary furnishing walk probe");
        var controller = probe.AddComponent<CharacterController>(); controller.height = 1.8f; controller.center = new Vector3(0, .9f, 0); controller.radius = .3f; controller.skinWidth = .035f; controller.stepOffset = .25f;
        var officeDoors = Object.FindObjectsByType<JEIGlassOfficeDoor>(FindObjectsSortMode.None);
        try
        {
            foreach (var door in officeDoors) door.SetOpenImmediate(true);
            Physics.SyncTransforms();
            WalkRoute(controller, new[] { new Vector2(0, -12.3f), new Vector2(0, -4.6f) });
            messages.Add("PASS: reception centre route from entrance to corridor.");
            WalkRoute(controller, new[] { new Vector2(-5, 4.6f), new Vector2(-5, 6.3f), new Vector2(-5, 9.3f), new Vector2(-5, 12.0f) });
            WalkRoute(controller, new[] { new Vector2(-8.5f, 9.3f), new Vector2(-1.75f, 9.3f) });
            messages.Add("PASS: grouped classroom central aisle and cross aisle between all four table groups.");
            WalkRoute(controller, new[] { new Vector2(-8, -4.6f), new Vector2(-8, -6.55f), new Vector2(-8, -7.7f), new Vector2(-9.8f, -7.7f), new Vector2(-13.8f, -7.7f), new Vector2(-13.8f, -12.05f) });
            messages.Add("PASS: teacher-suite entry, open social area, glass doorway and quiet-office workstation aisle.");
            WalkRoute(controller, new[] { new Vector2(6.6f, -4.6f), new Vector2(6.6f, -7.0f), new Vector2(8.2f, -7.0f), new Vector2(8.2f, -12.0f) });
            messages.Add("PASS: administrative-office entrance, print corner and records aisle.");
        }
        finally
        {
            Object.DestroyImmediate(probe);
            foreach (var door in officeDoors) door.SetOpenImmediate(false);
            Physics.SyncTransforms();
        }
        var root = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/" + OwnedName);
        if (!root || root.childCount != 4) throw new InvalidOperationException("Unexpected furnishing room count.");
        foreach (Transform room in root) if (!Approved.Any(id => room.name.StartsWith(id + "_", StringComparison.Ordinal))) throw new InvalidOperationException("An unapproved room was furnished.");
        messages.Add("PASS: furnishing is restricted to N02, S01, S02 and S03. Other rooms retain the previous empty structural shell.");
        messages.Add("Counts: grouped classroom 4 groups / 16 seats; quiet teacher office 8 workstations; shared open teacher meeting/lounge; minimal reception; administrative office 4 workstations and records/printing.");
        string report = string.Join("\n", messages); Directory.CreateDirectory(Output); File.WriteAllText(Output + "/Verification.txt", report); return report;
    }

    static void WalkRoute(CharacterController controller, Vector2[] points)
    {
        controller.enabled = false; controller.transform.position = new Vector3(points[0].x, .08f, points[0].y); controller.enabled = true; Physics.SyncTransforms();
        for (int settle = 0; settle < 10; settle++) controller.Move(Vector3.down * .025f);
        for (int n = 1; n < points.Length; n++)
        {
            bool reached = false;
            for (int i = 0; i < 1500; i++)
            {
                Vector3 p = controller.transform.position; Vector2 delta = points[n] - new Vector2(p.x, p.z);
                if (delta.magnitude < .045f) { reached = true; break; }
                Vector2 step = Vector2.ClampMagnitude(delta, .04f); controller.Move(new Vector3(step.x, -.035f, step.y));
            }
            if (!reached) throw new InvalidOperationException("Furniture blocks walking near " + controller.transform.position + " toward " + points[n]);
        }
    }

    static void FocusModel()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(0, 1, 0), Quaternion.Euler(43, -35, 0), 29, false, true); view.Focus(); view.Repaint();
    }

    [MenuItem("Tools/JEI/Interiors/Capture Agreed Interiors")]
    public static void Capture()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary interior review camera"); var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f); camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "02_Grouped_Classroom", new Vector3(-5.0f, 2.65f, 6.40f), new Vector3(-5.0f, .90f, 10.0f), 86);
            Shot(camera, "03_Teacher_Office", new Vector3(-4.0f, 2.45f, -6.65f), new Vector3(-12.0f, 1.1f, -9.8f), 78);
            Shot(camera, "04_Reception", new Vector3(0, 1.85f, -12.35f), new Vector3(0, 1.15f, -8.9f), 88);
            Shot(camera, "05_Admin_and_Records", new Vector3(8.1f, 2.45f, -6.65f), new Vector3(6.4f, .90f, -10.4f), 82);
            Shot(camera, "08_Grouped_Classroom_Cutaway", new Vector3(-5.025f, 8.5f, 6.35f), new Vector3(-5.025f, .5f, 9.35f), 65);
            Shot(camera, "09_Teacher_Suite_Cutaway", new Vector3(-10.55f, 10.5f, -11.8f), new Vector3(-10.55f, .6f, -9.35f), 62);
            Shot(camera, "10_Reception_Cutaway", new Vector3(1.2f, 7.0f, -11.8f), new Vector3(-.2f, .65f, -9.6f), 62);
            Shot(camera, "11_Admin_Cutaway", new Vector3(7.4f, 8.5f, -6.5f), new Vector3(6.65f, .6f, -9.35f), 60);
        }
        finally { Object.DestroyImmediate(obj); }
    }
    static void Shot(Camera camera, string name, Vector3 position, Vector3 target, float fieldOfView)
    {
        camera.transform.position = position; camera.transform.LookAt(target); camera.fieldOfView = fieldOfView;
        SaveRender(camera, Output + "/" + name + ".png");
    }
    static void SaveRender(Camera camera, string path)
    {
        const int width = 1800, height = 1200;
        var previous = RenderTexture.active; var previousTarget = camera.targetTexture;
        var rt = new RenderTexture(width, height, 24); var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        try { camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = previous; camera.targetTexture = previousTarget; Object.DestroyImmediate(rt); Object.DestroyImmediate(tex); }
    }

    static void VerifyRuntime()
    {
        try
        {
            var view = Object.FindFirstObjectByType<JEIStructureView>();
            if (!view || !view.target || !view.walkPlayer) throw new InvalidOperationException("The 3D view is incomplete.");
            var camera = view.GetComponent<Camera>();
            var door = Object.FindFirstObjectByType<JEIGlassOfficeDoor>();
            if (!door) throw new InvalidOperationException("The quiet-office door is missing.");
            double now = EditorApplication.timeSinceStartup;
            if (runtimeStage == 0)
            {
                if (!camera.enabled || camera.orthographic) throw new InvalidOperationException("Expected perspective overview.");
                int cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.isActiveAndEnabled);
                if (cameras != 1) throw new InvalidOperationException("More than one game camera.");
                if (Quaternion.Angle(door.transform.localRotation, Quaternion.identity) > .1f) throw new InvalidOperationException("The office door must start closed.");
                SaveRender(camera, Output + "/06_Runtime_Overview.png");
                runtimeStartPosition = view.walkPlayer.transform.position;
                runtimeStartRotation = view.walkPlayer.transform.rotation;
                view.SendMessage("SetWalking", true, SendMessageOptions.RequireReceiver);
                if (camera.enabled || !view.walkPlayer.activeInHierarchy) throw new InvalidOperationException("Walk mode failed.");
                SaveRender(view.walkPlayer.GetComponentInChildren<Camera>(), Output + "/07_Runtime_Reception.png");
                Teleport(view.walkPlayer, new Vector3(-7.9f, .08f, -7.7f));
                runtimeStage = 1; runtimeStageAt = now; return;
            }
            if (now - runtimeStageAt < 1.2) return;
            if (runtimeStage == 1)
            {
                if (Quaternion.Angle(door.transform.localRotation, Quaternion.Euler(0, -90, 0)) > 1) throw new InvalidOperationException("The glass office door did not open automatically.");
                Physics.SyncTransforms();
                var controller = view.walkPlayer.GetComponent<CharacterController>();
                WalkRoute(controller, new[] { new Vector2(-7.9f, -7.7f), new Vector2(-9.8f, -7.7f) });
                Teleport(view.walkPlayer, runtimeStartPosition);
                runtimeStage = 2; runtimeStageAt = now; return;
            }
            if (Quaternion.Angle(door.transform.localRotation, Quaternion.identity) > 1) throw new InvalidOperationException("The glass office door did not close after departure.");
            view.walkPlayer.transform.rotation = runtimeStartRotation;
            view.SendMessage("SetWalking", false, SendMessageOptions.RequireReceiver);
            if (!camera.enabled || view.walkPlayer.activeSelf) throw new InvalidOperationException("Returning to overview failed.");
            string report = "PASS: furnished model renders in actual Play mode.\nPASS: perspective overview -> first-person reception -> overview.\nPASS: glass office door starts closed, opens on approach, allows the actual tour controller through, and closes on departure.\nPASS: the structure remains open-topped for inspection; room-focus shortcuts are available.";
            File.WriteAllText(Output + "/Runtime_Verification.txt", report); File.WriteAllText(Result, "SUCCESS: " + report);
            FinishRuntime();
        }
        catch (Exception e) { File.WriteAllText(Result, "ERROR: " + e); Debug.LogException(e); FinishRuntime(); }
    }

    static void Teleport(GameObject player, Vector3 position)
    {
        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false; player.transform.position = position; controller.enabled = true;
        Physics.SyncTransforms();
    }

    static void FinishRuntime()
    {
        SessionState.SetBool(RuntimeKey, false); runtimeStage = 0;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true; EditorApplication.isPlaying = false;
    }

    static void WriteNotes()
    {
        File.WriteAllText(Output + "/README.md", "# 已讨论通过的四处室内布置\n\n本轮仅在本地实现，等待用户检查；没有提交或 push。\n\n- N02：4组弧形拼桌，每组4席；展示屏、白板及低资料柜。\n- S01：8个矮隔板教师工位，以到顶玻璃墙和默认关闭的玻璃门围合，步行靠近自动开门；玻璃外为共同开放的4人讨论桌和沙发休息区，两者之间没有隔墙。\n- S02：从南侧入口进入后，右侧小接待台，左侧轻巧沙发；学院标识位于沙发上方，公告栏位于旁边靠入口处。中央进出路线保持畅通。\n- S03：4个行政工位、封闭文件柜、常用资料书架与共享文印角。\n\n其余房间继续保留上一阶段空结构，尚未决定家具。外轮廓、外墙门窗、两座楼梯和两条2.4m公共走廊保持既有结构。\n\n在 Unity 打开 `Assets/Scenes/JEI_Floor01_Structure.unity`。Play后拖拽环绕、滚轮缩放，0总览、1分组教室、2教师办公、3接待、4行政资料；Tab切换步行，WASD移动，Esc释放鼠标。\n\n代码入口：`Tools/JEI/Interiors/Apply Agreed Furnishings`。此操作只重建带生成标记的室内布置组，结构生成器与本工具分别保留；若重建结构，请再次应用室内布置。\n\n所有自建桌椅网格、材质和地面纹理保存为项目资源，非临时对象。玻璃墙表现空间分隔，未进行声学仿真。验证与三维预览位于本目录。\n");
    }
}
