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

/// <summary>Furnishes only the rooms agreed in the design discussion.</summary>
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
    static readonly string[] Approved = { "N01", "N02", "N03", "N04", "S01", "S02", "S03", "S04" };

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
            else if (action == "build_n01") ApplyRegularClassroom();
            else if (action == "build_n03") ApplyDigitalClassroom();
            else if (action == "build_n04") ApplyReadingSuite();
            else if (action == "screen_n03") UpdateDigitalTeachingScreen();
            else if (action == "podium_n01") UpdateTeachingPodium();
            else if (action == "teacher_chair_n01") UpdateRegularTeacherChair();
            else if (action == "build_s01") ApplyTeacherSuite();
            else if (action == "build_s03") ApplyAdministrativeSuite();
            else if (action == "build_s04") ApplyMeetingSuite();
            else if (action == "meeting_set_s04") UpdateMeetingTableAndChairs();
            else if (action == "eight_seats_s04") UpdateMeetingEndSeatsAndBoard();
            else if (action == "oval_s03") UpdateAdministrativeCoffeeTable();
            else if (action == "capture") { Capture(); File.WriteAllText(Result, "SUCCESS: furniture previews captured."); }
            else if (action == "capture_n01") { CaptureRegularClassroom(); File.WriteAllText(Result, "SUCCESS: regular-classroom previews captured."); }
            else if (action == "capture_n03") { CaptureDigitalClassroom(); File.WriteAllText(Result, "SUCCESS: digital-classroom previews captured."); }
            else if (action == "capture_n04") { CaptureReadingSuite(); File.WriteAllText(Result, "SUCCESS: project-studio previews captured."); }
            else if (action == "capture_s01") { CaptureTeacherRevision(); File.WriteAllText(Result, "SUCCESS: teacher-suite previews captured."); }
            else if (action == "capture_s03") { CaptureAdminRevision(); File.WriteAllText(Result, "SUCCESS: administrative-suite previews captured."); }
            else if (action == "capture_s04") { CaptureMeetingRevision(); File.WriteAllText(Result, "SUCCESS: meeting-suite previews captured."); }
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
    public static void Apply() { ApplyRooms(null); }

    [MenuItem("Tools/JEI/Interiors/Update Regular Classroom Only")]
    public static void ApplyRegularClassroom() { ApplyRooms("N01"); }

    [MenuItem("Tools/JEI/Interiors/Update Digital Classroom Only")]
    public static void ApplyDigitalClassroom() { ApplyRooms("N03"); }

    [MenuItem("Tools/JEI/Interiors/Update N04 Project Studio Only")]
    public static void ApplyReadingSuite() { ApplyRooms("N04"); }

    [MenuItem("Tools/JEI/Interiors/Update N03 Main Teaching Screen Only")]
    public static void UpdateDigitalTeachingScreen()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var suite=GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/"+OwnedName+"/N03_"+RoomName("N03")+"/N03__Digital_learning__16_seats");
        if (!suite || !suite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The generated N03 classroom is missing.");
        var previous=suite.GetComponentsInChildren<Transform>().Where(t=>t.name=="N03_Main_whiteboard" || t.name=="N03_Teaching_display" || t.name=="N03_Main_teaching_screen").ToArray();
        bool original=previous.Length==2 && previous.Any(t=>t.name=="N03_Main_whiteboard") && previous.Any(t=>t.name=="N03_Teaching_display");
        bool revised=previous.Length==1 && previous[0].name=="N03_Main_teaching_screen";
        if ((!original && !revised) || previous.Any(t=>!t.GetComponent<JEIGeneratedObject>())) throw new InvalidOperationException("Expected the generated whiteboard/display pair or one main teaching screen; existing furniture was preserved.");
        var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath,"Library/JEI_Furnishing_Backups/"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_before_n03_screen.unity",true);
        foreach (var piece in previous) { PreserveManual(piece,suite); Undo.DestroyObjectImmediate(piece.gameObject); }
        var screen=JEIDigitalClassroom.BuildMainTeachingScreen(suite);
        Mark(screen);
        Undo.RegisterCreatedObjectUndo(screen.gameObject,"Replace N03 whiteboard and side display with main screen");
        Physics.SyncTransforms();
        string verification=Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the main teaching screen.");
        AssetDatabase.SaveAssets();
        WriteNotes(); CaptureDigitalClassroom(); FocusDigitalClassroom();
        Selection.activeGameObject=screen.gameObject;
        File.WriteAllText(Result,"SUCCESS: replaced only the N03 whiteboard and small display with one 3.20 x 1.80m teaching screen.\n"+verification);
    }

    [MenuItem("Tools/JEI/Interiors/Update N01 Teacher Chair Only")]
    public static void UpdateRegularTeacherChair()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var suite=GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/"+OwnedName+"/N01_"+RoomName("N01")+"/N01__Everyday_teaching__24_seats_in_pairs");
        if (!suite || !suite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The generated N01 classroom is missing.");
        var chairs=suite.GetComponentsInChildren<Transform>().Where(t=>t.name=="N01_Teacher_chair").ToArray();
        if (chairs.Length!=1 || !chairs[0].GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("Expected one generated teacher chair; existing furniture was preserved.");
        var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath,"Library/JEI_Furnishing_Backups/"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_before_n01_teacher_chair.unity",true);
        PreserveManual(chairs[0],suite);
        Undo.DestroyObjectImmediate(chairs[0].gameObject);
        var chair=JEIRegularClassroom.BuildTeacherChair(suite);
        Mark(chair);
        Undo.RegisterCreatedObjectUndo(chair.gameObject,"Replace N01 teacher chair");
        Physics.SyncTransforms();
        string verification=Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the teacher chair.");
        AssetDatabase.SaveAssets();
        WriteNotes(); CaptureRegularClassroom(); FocusRegularClassroom();
        Selection.activeGameObject=chair.gameObject;
        File.WriteAllText(Result,"SUCCESS: replaced only the N01 teacher chair with a compact upholstered task chair.\n"+verification);
    }

    [MenuItem("Tools/JEI/Interiors/Update N01 Teaching Podium Only")]
    public static void UpdateTeachingPodium()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var suite=GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/"+OwnedName+"/N01_"+RoomName("N01")+"/N01__Everyday_teaching__24_seats_in_pairs");
        if (!suite || !suite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The generated N01 classroom is missing.");
        var previous=suite.GetComponentsInChildren<Transform>().Where(t=>t.name=="N01_Teacher_desk" || t.name=="N01_Teaching_podium").ToArray();
        if (previous.Length!=1 || !previous[0].GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("Expected one generated teacher desk or podium; existing furniture was preserved.");
        var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath,"Library/JEI_Furnishing_Backups/"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_before_n01_podium.unity",true);
        PreserveManual(previous[0],suite);
        Undo.DestroyObjectImmediate(previous[0].gameObject);
        var podium=JEIRegularClassroom.BuildTeachingPodium(suite);
        Mark(podium);
        Undo.RegisterCreatedObjectUndo(podium.gameObject,"Replace N01 teacher table with teaching podium");
        Physics.SyncTransforms();
        string verification=Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the teaching podium.");
        AssetDatabase.SaveAssets();
        WriteNotes(); CaptureRegularClassroom(); FocusRegularClassroom();
        Selection.activeGameObject=podium.gameObject;
        File.WriteAllText(Result,"SUCCESS: replaced only the N01 teacher table with a 1.70 x 0.72m teaching podium.\n"+verification);
    }

    [MenuItem("Tools/JEI/Interiors/Update Teacher Suite Only")]
    public static void ApplyTeacherSuite() { ApplyRooms("S01"); }

    [MenuItem("Tools/JEI/Interiors/Update Administrative Suite Only")]
    public static void ApplyAdministrativeSuite() { ApplyRooms("S03"); }

    [MenuItem("Tools/JEI/Interiors/Update Meeting Suite Only")]
    public static void ApplyMeetingSuite() { ApplyRooms("S04"); }

    [MenuItem("Tools/JEI/Interiors/Update Meeting Table and Chairs Only")]
    public static void UpdateMeetingTableAndChairs()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var room = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/" + OwnedName + "/S04_" + RoomName("S04"));
        var suite = room ? room.Find("S04__Six_person_meeting_and_window_conversation") : null;
        if (!suite || !suite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The generated S04 room is missing.");
        var previousSet = suite.Find("S04_Main_meeting_set");
        var previousPieces = suite.GetComponentsInChildren<Transform>().Where(t => t.name == "S04_Six_seat_oval_meeting_table" || t.name == "S04_Six_seat_rectangular_meeting_table" || t.name == "S04_Eight_seat_rectangular_meeting_table" || t.name.StartsWith("S04_Meeting_chair_", StringComparison.Ordinal)).ToArray();
        if ((previousPieces.Length != 7 && previousPieces.Length != 9) || previousPieces.Any(t => !t.GetComponent<JEIGeneratedObject>())) throw new InvalidOperationException("Expected one generated meeting table and six or eight chairs; existing furniture was preserved.");
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_meeting_set.unity", true);
        if (previousSet)
        {
            if (!previousSet.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The meeting set is not generator-owned; it was preserved.");
            PreserveManual(previousSet, suite);
            Undo.DestroyObjectImmediate(previousSet.gameObject);
        }
        else foreach (var piece in previousPieces) { PreserveManual(piece, suite); Undo.DestroyObjectImmediate(piece.gameObject); }
        var replacement = JEIMeetingSuite.BuildMeetingSet(suite);
        Mark(replacement);
        Undo.RegisterCreatedObjectUndo(replacement.gameObject, "Update S04 meeting table and chairs");
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the meeting furniture.");
        AssetDatabase.SaveAssets();
        WriteNotes();
        CaptureMeetingRevision();
        FocusMeetingSuite();
        File.WriteAllText(Result, "SUCCESS: replaced only the S04 meeting table and eight chairs.\n" + verification);
    }

    [MenuItem("Tools/JEI/Interiors/Add Meeting End Seats and Main Whiteboard")]
    public static void UpdateMeetingEndSeatsAndBoard()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var suite = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/" + OwnedName + "/S04_" + RoomName("S04") + "/S04__Six_person_meeting_and_window_conversation");
        if (!suite || !suite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The generated S04 room is missing.");
        var set = suite.Find("S04_Main_meeting_set");
        if (!set || !set.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The rectangular meeting set is missing.");
        var objects = suite.GetComponentsInChildren<Transform>();
        var table = objects.Single(t => t.name == "S04_Six_seat_rectangular_meeting_table" || t.name == "S04_Eight_seat_rectangular_meeting_table");
        var board = objects.Single(t => t.name == "S04_Wall_display" || t.name == "S04_Main_whiteboard");
        var previousAdditions = objects.Where(t => t.name == "S04_Meeting_chair_07" || t.name == "S04_Meeting_chair_08" || t.name == "End seat meeting notepad").ToArray();
        if (!table.GetComponent<JEIGeneratedObject>() || !board.GetComponent<JEIGeneratedObject>() || previousAdditions.Any(t => !t.GetComponent<JEIGeneratedObject>())) throw new InvalidOperationException("Existing manual furniture was preserved.");
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_eight_seats.unity", true);
        foreach (var previous in previousAdditions) { PreserveManual(previous, suite); Undo.DestroyObjectImmediate(previous.gameObject); }
        PreserveManual(board, suite); Undo.DestroyObjectImmediate(board.gameObject);
        Undo.RecordObject(set, "Shift meeting set for end-seat circulation");
        Undo.RecordObject(table.gameObject, "Name eight-seat meeting table");
        set.localPosition = JEIMeetingSuite.MeetingSetOffset;
        table.name = "S04_Eight_seat_rectangular_meeting_table";
        JEIMeetingSuite.AddEndSeats(set);
        JEIMeetingSuite.AddEndPlaceSettings(table);
        var mainBoard = JEIMeetingSuite.BuildMainWhiteboard(suite);
        var additions = suite.GetComponentsInChildren<Transform>().Where(t => t == mainBoard || t.name == "S04_Meeting_chair_07" || t.name == "S04_Meeting_chair_08" || t.name == "End seat meeting notepad").ToArray();
        foreach (var addition in additions) { Mark(addition); Undo.RegisterCreatedObjectUndo(addition.gameObject, "Add end seats and main whiteboard"); }
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the eight-seat meeting room.");
        AssetDatabase.SaveAssets();
        WriteNotes(); CaptureMeetingRevision(); FocusMeetingSuite();
        File.WriteAllText(Result, "SUCCESS: added two meeting end seats, shifted the set by 0.10m and replaced the display with a large whiteboard.\n" + verification);
    }

    [MenuItem("Tools/JEI/Interiors/Update Administrative Coffee Table Only")]
    public static void UpdateAdministrativeCoffeeTable()
    {
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        var room = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/" + OwnedName + "/S03_" + RoomName("S03"));
        var coffee = room.GetComponentsInChildren<Transform>().Single(t => t.name.StartsWith("Small_round_lounge_coffee_table__", StringComparison.Ordinal) || t.name.StartsWith("Small_oval_lounge_coffee_table__", StringComparison.Ordinal));
        var scene = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        string backup = "Library/JEI_Furnishing_Backups/" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_before_oval.unity";
        if (scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath, backup, true);
        Undo.RecordObjects(coffee.GetComponentsInChildren<Transform>().Cast<Object>().Concat(new Object[] { coffee.gameObject, coffee.GetComponent<BoxCollider>(), coffee.GetChild(0).gameObject }).ToArray(), "Make administrative coffee table oval");
        JEIAdministrativeSuite.ConfigureOvalCoffeeTable(coffee);
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the oval coffee table.");
        WriteNotes();
        CaptureAdminRevision();
        FocusAdministrativeSuite();
        Selection.activeGameObject = coffee.gameObject;
        File.WriteAllText(Result, "SUCCESS: only the S03 coffee table was changed to a 0.60 x 1.00m oval.\n" + verification);
    }

    static void ApplyRooms(string targetRoom)
    {
        bool partial = targetRoom != null;
        if (partial && !Approved.Contains(targetRoom)) throw new ArgumentException("Room has not been approved: " + targetRoom);
        RequireScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory("Library/JEI_Furnishing_Backups");
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        if (File.Exists(ScenePath)) File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + stamp + "_saved.unity", true);
        File.Copy("Assets/JEI_Blockout/Floor01_Plan.json", "Library/JEI_Furnishing_Backups/" + stamp + "_plan.json", true);
        var scene = SceneManager.GetActiveScene();
        if (scene.isDirty)
        {
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
            File.Copy(ScenePath, "Library/JEI_Furnishing_Backups/" + stamp + "_live.unity", true);
        }
        floor = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m");
        if (!floor) throw new InvalidOperationException("Structure floor root is missing.");
        if (targetRoom == "S03" || !partial) JEIAdminDoorRevision.Apply(floor);
        plan = JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        var old = floor.Find(OwnedName);
        if (partial)
        {
            if (!old || !old.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("An existing generated furnishing root is required for a room update.");
            owned = old;
            var oldSuite = owned.Find(targetRoom + "_" + RoomName(targetRoom));
            if (oldSuite)
            {
                if (!oldSuite.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The existing room is not generator-owned; it was preserved.");
                PreserveManual(oldSuite, floor);
                Undo.DestroyObjectImmediate(oldSuite.gameObject);
            }
        }
        else
        {
            if (old)
            {
                if (!old.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The furnishing root is not generator-owned; it was preserved.");
                PreserveManual(old, floor);
                Undo.DestroyObjectImmediate(old.gameObject);
            }
            owned = F.Group(floor, OwnedName);
        }
        foreach (string id in partial ? new[] { targetRoom } : Approved)
        {
            var room = Array.Find(plan.rooms, r => r.id == id);
            var group = F.Group(owned, id + "_" + RoomName(id));
            Finishes(group, room);
            if (id == "N01") JEIRegularClassroom.Build(group);
            else if (id == "N02") JEIGroupedClassroom.Build(group);
            else if (id == "N03") JEIDigitalClassroom.Build(group);
            else if (id == "N04") JEIReadingSuite.Build(group);
            else if (id == "S01") JEITeacherSuite.Build(group);
            else if (id == "S02") Reception(group);
            else if (id == "S03") Administration(group);
            else if (id == "S04") JEIMeetingSuite.Build(group);
            DoorPlaque(group, id);
            if (partial)
            {
                Mark(group);
                Undo.RegisterCreatedObjectUndo(group.gameObject, "Update " + targetRoom + " furniture");
            }
        }
        if (!partial)
        {
            Mark(owned);
            Undo.RegisterCreatedObjectUndo(owned.gameObject, "Apply approved room interiors");
        }
        Physics.SyncTransforms();
        string verification = Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save furnished scene.");
        AssetDatabase.SaveAssets();
        File.WriteAllText(Output + "/Verification.txt", verification);
        WriteNotes();
        if (targetRoom == "N04") CaptureReadingSuite(); else if (targetRoom == "N03") CaptureDigitalClassroom(); else if (targetRoom == "N01") CaptureRegularClassroom(); else if (targetRoom == "S01") CaptureTeacherRevision(); else if (targetRoom == "S03") CaptureAdminRevision(); else if (targetRoom == "S04") CaptureMeetingRevision(); else Capture();
        if (targetRoom == "N04") FocusReadingSuite(); else if (targetRoom == "N03") FocusDigitalClassroom(); else if (targetRoom == "N01") FocusRegularClassroom(); else if (targetRoom == "S01") FocusTeacherSuite(); else if (targetRoom == "S03") FocusAdministrativeSuite(); else if (targetRoom == "S04") FocusMeetingSuite(); else FocusModel();
        File.WriteAllText(Result, (partial ? "SUCCESS: updated " + targetRoom + " only; the other rooms were preserved.\n" : "SUCCESS: furnished " + string.Join(", ", Approved) + " only.\n") + verification);
        Debug.Log("JEI approved interiors saved locally for review.");
    }

    static string RoomName(string id)
    {
        if (id == "N01") return "Regular_Classroom";
        if (id == "N02") return "Curved_Group_Tables";
        if (id == "N03") return "Digital_Learning_Classroom";
        if (id == "N04") return "Reading_and_Seminar";
        if (id == "S01") return "Teacher_Office_and_Open_Lounge";
        if (id == "S02") return "Minimal_Reception";
        if (id == "S04") return "Meeting_and_Tutorial";
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
        bool north = id == "N01" || id == "N02" || id == "N03" || id == "N04";
        float x = id == "N01" ? -10.7f : id == "N02" ? -5 : id == "N03" ? 4.5f : id == "N04" ? 13.3f : id == "S01" ? -8 : id == "S02" ? 0 : id == "S04" ? 13.3f : 7.3f;
        string title = id == "N01" ? "101  CLASSROOM" : id == "N02" ? "102  GROUP LEARNING" : id == "N03" ? "103  DIGITAL LEARNING" : id == "N04" ? "104  TEAM PROJECT STUDIO" : id == "S01" ? "STAFF OFFICE" : id == "S02" ? "WELCOME" : id == "S04" ? "MEETING / TUTORIAL" : "ADMIN / RECORDS";
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

    static void Administration(Transform parent) { JEIAdministrativeSuite.Build(parent); }

    static string Verify()
    {
        RequireScene(); Physics.SyncTransforms();
        var messages = new List<string>();
        var currentPlan = JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        int doors = 0, liftReserves = 0;
        foreach (var wall in currentPlan.walls) foreach (var opening in wall.openings)
        {
            if (opening.kind != "door") continue;
            if (JEIServiceAreas.IsClosedLiftOpening(wall.axis, wall.at, opening.at)) { liftReserves++; continue; }
            Vector3 center = wall.axis == "H" ? new Vector3(opening.at, 1, wall.at) : new Vector3(wall.at, 1, opening.at);
            Vector3 half = wall.axis == "H" ? new Vector3(opening.width / 2 - .08f, .7f, .07f) : new Vector3(.07f, .7f, opening.width / 2 - .08f);
            foreach (var hit in Physics.OverlapBox(center, half)) if (!(hit is CharacterController)) throw new InvalidOperationException("Door obstructed by " + hit.name + " at " + center);
            doors++;
        }
        messages.Add("PASS: all " + doors + " walking door openings remain unobstructed; " + liftReserves + " reserved lift entrance(s) remain deliberately closed.");
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
            WalkRoute(controller, new[] { new Vector2(-8, -4.6f), new Vector2(-8, -6.55f), new Vector2(-8, -7.7f), new Vector2(-9.65f, -7.7f), new Vector2(-10.25f, -7.7f), new Vector2(-10.45f, -7.95f), new Vector2(-10.85f, -8.55f), new Vector2(-13.8f, -8.85f) });
            WalkRoute(controller, new[] { new Vector2(-13.8f, -8.85f), new Vector2(-14.6f, -7.1f), new Vector2(-16.2f, -7.1f) });
            WalkRoute(controller, new[] { new Vector2(-13.8f, -8.85f), new Vector2(-13, -7.35f), new Vector2(-11, -7.35f) });
            WalkRoute(controller, new[] { new Vector2(-13.8f, -8.85f), new Vector2(-15.5f, -8.85f), new Vector2(-15.5f, -7.9f), new Vector2(-15.5f, -9.6f) });
            WalkRoute(controller, new[] { new Vector2(-13.8f, -8.85f), new Vector2(-13.8f, -10.45f), new Vector2(-15.9f, -10.45f), new Vector2(-13.95f, -10.45f), new Vector2(-12, -10.45f), new Vector2(-10.05f, -10.45f) });
            messages.Add("PASS: teacher-suite entry and glass doorway; approaches to all six window-side writing desks, both large storage cabinets and the beverage/printing counter.");
            WalkRoute(controller, new[] { new Vector2(7.3f, -4.6f), new Vector2(7.3f, -7.4f), new Vector2(6.4f, -8.7f), new Vector2(5.875f, -9.05f) });
            WalkRoute(controller, new[] { new Vector2(7.3f, -7.1f), new Vector2(4.9f, -7.15f) });
            WalkRoute(controller, new[] { new Vector2(7.3f, -7.4f), new Vector2(8.4f, -7.5f), new Vector2(8.4f, -11.7f), new Vector2(6.6f, -12.05f), new Vector2(5.15f, -12.05f) });
            WalkRoute(controller, new[] { new Vector2(6.4f, -8.7f), new Vector2(5.7f, -8.15f) });
            messages.Add("PASS: administrative entrance moved to X=7.30m; routes reach the two desks, sofa, print/water counter and closed storage.");
            VerifyMeetingRoutes(controller);
            messages.Add("PASS: S04 entrance, both rows of meeting seats, both whiteboards, closed cabinet and window-side conversation area are reachable.");
            VerifyRegularClassroomRoutes(controller);
            messages.Add("PASS: N01 entrance, two main aisles, all 24 student-seat side approaches, teaching front, teacher position and rear resource/display area are reachable.");
            VerifyDigitalClassroomRoutes(controller);
            messages.Add("PASS: N03 entrance, central and outer aisles, all sixteen computer-seat side approaches, teaching wall, teacher position, rear storage and both project displays are reachable.");
            VerifyReadingSuiteRoutes(controller);
            messages.Add("PASS: N04 entrance and open collaboration area, all three movable-chair approaches, both booth access ends, writable wall, window project workbench and team storage are reachable.");
        }
        finally
        {
            Object.DestroyImmediate(probe);
            foreach (var door in officeDoors) door.SetOpenImmediate(false);
            Physics.SyncTransforms();
        }
        var root = GameObject.Find("JEI_Structure_Study").transform.Find("Floor_01__36x26m__Storey_3.60m/" + OwnedName);
        if (!root || root.childCount != Approved.Length) throw new InvalidOperationException("Unexpected furnishing room count.");
        foreach (Transform room in root) if (!Approved.Any(id => room.name.StartsWith(id + "_", StringComparison.Ordinal))) throw new InvalidOperationException("An unapproved room was furnished.");
        var suite = root.Find("S01_" + RoomName("S01"));
        var suiteObjects = suite.GetComponentsInChildren<Transform>();
        if (suiteObjects.Count(t => t.name.StartsWith("Teacher_workstation_", StringComparison.Ordinal)) != 6) throw new InvalidOperationException("Expected six teacher writing desks; the two north-wall desks were removed.");
        if (suiteObjects.Count(t => t.name == "Full height practical faculty storage") != 2) throw new InvalidOperationException("Expected two enlarged faculty storage cabinets.");
        foreach (string name in new[] { "S01_Built_in_tea_and_print_cabinet__", "Countertop_water_dispenser__", "Built_in_compact_coffee_machine__", "Multifunction_printer__" })
            if (suiteObjects.Count(t => t.name.StartsWith(name, StringComparison.Ordinal)) != 1) throw new InvalidOperationException("The integrated amenity cabinet is incomplete: " + name);
        if (suiteObjects.Count(t => t.name == "Informal oval oak discussion table") != 1 || suiteObjects.Count(t => t.name == "Timber and fabric discussion chair") != 4) throw new InvalidOperationException("The informal four-seat discussion set is incomplete.");
        messages.Add("PASS: six writing desks, two large cabinets, one integrated water/coffee/print cabinet and one oval table with four informal chairs are present.");
        var adminObjects = root.Find("S03_" + RoomName("S03")).GetComponentsInChildren<Transform>();
        var adminDesks = adminObjects.Where(t => t.name == "Oak workstation").ToArray();
        if (adminDesks.Length != 2 || adminDesks.Any(t => Quaternion.Angle(t.localRotation, Quaternion.identity) > .1f)) throw new InvalidOperationException("Expected two administrative desks facing the north entrance.");
        foreach (string name in new[] { "Closed_reference_storage_near_entrance", "Small_oval_lounge_coffee_table__", "S03_North_wall_print_and_water_cabinet__", "S03_Multifunction_printer__", "S03_Countertop_water_dispenser__" })
            if (adminObjects.Count(t => t.name.StartsWith(name, StringComparison.Ordinal)) != 1) throw new InvalidOperationException("Administrative furniture is incomplete: " + name);
        if (!Physics.CheckBox(new Vector3(6.2f, 1, -5.9f), new Vector3(.05f, .3f, .05f))) throw new InvalidOperationException("The previous administrative doorway has not been closed by the moved wall segment.");
        messages.Add("PASS: two entrance-facing administrative desks, sofa/coffee table, closed entrance cabinet and integrated print/water cabinet; the previous doorway is filled.");
        var meeting = root.Find("S04_" + RoomName("S04"));
        var meetingObjects = meeting.GetComponentsInChildren<Transform>();
        if (meetingObjects.Count(t => t.name == "S04_Eight_seat_rectangular_meeting_table") != 1 || meetingObjects.Count(t => t.name.StartsWith("S04_Meeting_chair_", StringComparison.Ordinal)) != 8 || meetingObjects.Count(t => t.name.StartsWith("S04_Lounge_armchair_", StringComparison.Ordinal)) != 2) throw new InvalidOperationException("S04 must contain one rectangular meeting table, eight meeting chairs and two lounge armchairs.");
        foreach (string name in new[] { "S04_Main_whiteboard", "S04_Whiteboard", "S04_Closed_meeting_storage", "S04_Lounge_side_table" })
            if (meetingObjects.Count(t => t.name == name) != 1) throw new InvalidOperationException("Meeting furniture is incomplete: " + name);
        foreach (var collider in meeting.GetComponentsInChildren<Collider>())
        {
            var bounds = collider.bounds;
            if (bounds.min.x < 10.09f || bounds.max.x > 17.71f || bounds.min.z < -12.71f || bounds.max.z > -5.99f) throw new InvalidOperationException("Meeting furniture extends beyond the room: " + collider.name);
        }
        messages.Add("PASS: S04 has a 3.20 x 1.35m rectangular meeting table, eight square-backed upholstered meeting chairs, two lounge armchairs, a 2.10 x 1.25m main whiteboard, secondary whiteboard, side table and closed storage; furniture colliders stay inside the room.");
        var classroom = root.Find("N01_" + RoomName("N01"));
        var classroomObjects = classroom.GetComponentsInChildren<Transform>();
        var studentDesks = classroomObjects.Where(t => t.name.StartsWith("N01_Double_student_desk_", StringComparison.Ordinal)).ToArray();
        var studentChairs = classroomObjects.Where(t => t.name.StartsWith("N01_Student_chair_", StringComparison.Ordinal)).ToArray();
        if (studentDesks.Length != 12 || studentChairs.Length != 24) throw new InvalidOperationException("N01 must contain twelve double desks and twenty-four student chairs.");
        if (studentDesks.Concat(studentChairs).Any(t => Quaternion.Angle(t.rotation, Quaternion.Euler(0, 90, 0)) > .1f)) throw new InvalidOperationException("N01 desks and student chairs must face the east teaching wall.");
        foreach (string name in new[] { "N01_Main_whiteboard", "N01_Teaching_display", "N01_Teaching_podium", "N01_Teacher_chair", "N01_Low_resource_storage" })
            if (classroomObjects.Count(t => t.name == name) != 1) throw new InvalidOperationException("N01 classroom furniture is incomplete: " + name);
        if (classroomObjects.Count(t => t.name.StartsWith("N01_Student_work_board_", StringComparison.Ordinal)) != 2) throw new InvalidOperationException("N01 needs two rear student-work displays.");
        foreach (var collider in classroom.GetComponentsInChildren<Collider>())
        {
            var bounds = collider.bounds;
            if (bounds.min.x < -17.71f || bounds.max.x > -9.39f || bounds.min.z < 5.99f || bounds.max.z > 12.71f) throw new InvalidOperationException("N01 furniture extends beyond the room: " + collider.name);
        }
        messages.Add("PASS: N01 has twelve movable double desks, twenty-four student chairs, a 1.70 x 0.72m teaching podium with closed front/storage and AV controls, teacher chair, whiteboard/display, low storage and two student-work boards; furniture stays inside the room.");
        var digital = root.Find("N03_" + RoomName("N03"));
        var digitalObjects = digital.GetComponentsInChildren<Transform>();
        var digitalDesks = digitalObjects.Where(t => t.name.StartsWith("N03_Double_computer_desk_", StringComparison.Ordinal)).ToArray();
        var digitalChairs = digitalObjects.Where(t => t.name.StartsWith("N03_Student_task_chair_", StringComparison.Ordinal)).ToArray();
        if (digitalDesks.Length != 8 || digitalChairs.Length != 16 || digitalObjects.Count(t => t.name.StartsWith("N03_Student_computer_", StringComparison.Ordinal)) != 16) throw new InvalidOperationException("N03 needs eight double computer desks, sixteen student task chairs and sixteen computers.");
        if (digitalDesks.Concat(digitalChairs).Any(t => Quaternion.Angle(t.rotation, Quaternion.Euler(0, 270, 0)) > .1f)) throw new InvalidOperationException("N03 desks and student chairs must face the west teaching wall.");
        foreach (string name in new[] { "N03_Teaching_podium", "N03_Teacher_chair", "N03_Main_teaching_screen", "N03_Low_resource_storage", "N03_Project_board_01", "N03_Project_board_02" })
            if (digitalObjects.Count(t => t.name == name) != 1) throw new InvalidOperationException("N03 classroom furniture is incomplete: " + name);
        if (digitalObjects.Any(t => t.name=="N03_Main_whiteboard" || t.name=="N03_Teaching_display")) throw new InvalidOperationException("N03 should have one main teaching screen; the old whiteboard and smaller display must be removed.");
        foreach (var collider in digital.GetComponentsInChildren<Collider>())
        {
            var bounds = collider.bounds;
            if (bounds.min.x < -.66f || bounds.max.x > 10.31f || bounds.min.z < 5.99f || bounds.max.z > 12.71f) throw new InvalidOperationException("N03 furniture extends beyond the room: " + collider.name);
        }
        messages.Add("PASS: N03 has eight 2.00 x 0.80m warm-oak double computer desks, sixteen properly upholstered task chairs and computers, a cabinet-style teaching podium with teacher chair, one 3.20 x 1.80m main teaching screen, low storage and two student-project boards; furniture stays inside the room.");
        var reading = root.Find("N04_" + RoomName("N04"));
        var readingObjects = reading.GetComponentsInChildren<Transform>();
        var projectSeats = readingObjects.Where(t => t.name.StartsWith("N04_Project_chair_", StringComparison.Ordinal)).ToArray();
        if (projectSeats.Length != 3 || readingObjects.Count(t => t.name.StartsWith("N04_Booth_seat_", StringComparison.Ordinal)) != 3) throw new InvalidOperationException("N04 needs three movable project chairs and three booth seats.");
        if (readingObjects.Count(t => t.name.StartsWith("N04_Mobile_table_module_", StringComparison.Ordinal)) != 2 || readingObjects.Count(t => t.name.StartsWith("N04_Member_cubby_", StringComparison.Ordinal)) != 6) throw new InvalidOperationException("N04 needs two joinable project-table modules and six member storage cubbies.");
        foreach (string name in new[] { "N04_Project_booth", "Booth_north_segment", "Booth_west_return", "N04_Shared_project_table", "N04_Collaboration_whiteboard", "N04_Team_material_storage", "N04_Project_progress_board", "N04_Window_project_workbench", "N04_Team_identity_board", "N04_Team_material_cart" })
            if (readingObjects.Count(t => t.name == name) != 1) throw new InvalidOperationException("N04 project-studio furniture is incomplete: " + name);
        if (readingObjects.Any(t => t.name.StartsWith("N04_Study_chair_", StringComparison.Ordinal) || t.name.StartsWith("N04_Reading_armchair_", StringComparison.Ordinal) || t.name.StartsWith("N04_Resource_bookcase_", StringComparison.Ordinal) || t.name=="N04_Oval_reading_coffee_table")) throw new InvalidOperationException("The previous N04 reading-room furniture must be removed.");
        foreach (var seat in projectSeats)
        {
            float yaw = seat.name.EndsWith("03", StringComparison.Ordinal) ? 270 : 0;
            if (Quaternion.Angle(seat.rotation, Quaternion.Euler(0, yaw, 0)) > .1f) throw new InvalidOperationException("N04 project chairs must face the shared workspace.");
        }
        foreach (var collider in reading.GetComponentsInChildren<Collider>())
        {
            var bounds = collider.bounds;
            if (bounds.min.x < 10.49f || bounds.max.x > 17.71f || bounds.min.z < 5.99f || bounds.max.z > 12.71f) throw new InvalidOperationException("N04 furniture extends beyond the room: " + collider.name);
        }
        var workbench = readingObjects.Single(t => t.name=="N04_Window_project_workbench");
        if (workbench.GetComponentsInChildren<Collider>().Any(c => c.bounds.max.y > .90f)) throw new InvalidOperationException("N04 window workbench must remain below the window sill.");
        messages.Add("PASS: N04 has a three-seat low L-shaped booth, three movable project chairs, two joinable table modules forming a 2.40 x 1.50m workspace, a large writable wall, project-progress board, six member cubbies, window project workbench and material cart; furniture stays inside the room.");
        messages.Add("PASS: 06_Approved_Interiors remains restricted to " + string.Join(", ", Approved) + ". Additional courtyard and service-area groups are managed separately.");
        messages.Add("Counts: N04 project studio 3 booth seats / 3 movable chairs / 2 table modules / 6 member cubbies; N03 digital classroom 8 double computer desks / 16 students; N01 regular classroom 12 double desks / 24 students; grouped classroom 4 groups / 16 seats; quiet teacher office 6 workstations plus integrated amenities; shared open teacher meeting/lounge; minimal reception; administrative office 2 entrance-facing workstations, sofa/coffee table and closed storage plus print/water counter; S04 eight-seat meeting table and two lounge armchairs.");
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

    static void VerifyMeetingRoutes(CharacterController controller)
    {
        WalkRoute(controller, new[] { new Vector2(13.3f, -4.6f), new Vector2(13.3f, -7.25f), new Vector2(14.25f, -7.25f), new Vector2(13.2f, -7.25f), new Vector2(12.15f, -7.25f) });
        WalkRoute(controller, new[] { new Vector2(13.3f, -7.25f), new Vector2(11.4f, -7.25f), new Vector2(10.645f, -8.1f), new Vector2(10.645f, -9.25f), new Vector2(10.645f, -10.4f), new Vector2(10.645f, -11.25f), new Vector2(12.15f, -11.25f), new Vector2(13.2f, -11.25f), new Vector2(14.25f, -11.25f) });
        WalkRoute(controller, new[] { new Vector2(14.25f, -11.25f), new Vector2(15.6f, -11.25f), new Vector2(15.6f, -11.65f), new Vector2(15.6f, -10.85f), new Vector2(15.6f, -10.4f), new Vector2(15.75f, -10.1f), new Vector2(15.75f, -9.25f), new Vector2(15.75f, -7.25f), new Vector2(16, -7.25f) });
    }

    static void VerifyRegularClassroomRoutes(CharacterController controller)
    {
        WalkRoute(controller, new[] { new Vector2(-10.7f, 4.6f), new Vector2(-10.7f, 6.35f), new Vector2(-16.835f, 6.35f), new Vector2(-16.835f, 12.35f) });
        // Every pair is entered from its desk ends. The two inner aisles serve seats on both sides.
        foreach (float z in new[] { 6.35f, 8.325f, 10.375f, 12.35f })
            WalkRoute(controller, new[] { new Vector2(-16.835f, z), new Vector2(-16.2f, z), new Vector2(-14.85f, z), new Vector2(-13.5f, z), new Vector2(-12.15f, z), new Vector2(-11.25f, z) });
        WalkRoute(controller, new[] { new Vector2(-10.7f, 6.35f), new Vector2(-10.7f, 8.325f), new Vector2(-10.1f, 8.85f), new Vector2(-10.1f, 10.375f) });
        WalkRoute(controller, new[] { new Vector2(-10.7f, 10.375f), new Vector2(-9.855f, 10.375f), new Vector2(-9.855f, 11.25f) });
    }

    static void VerifyDigitalClassroomRoutes(CharacterController controller)
    {
        WalkRoute(controller, new[] { new Vector2(4.5f, 4.6f), new Vector2(4.5f, 6.4f), new Vector2(1.4f, 6.4f), new Vector2(1.4f, 12.3f) });
        foreach (float z in new[] { 6.4f, 9.35f, 12.3f })
            WalkRoute(controller, new[] { new Vector2(1.4f, z), new Vector2(3.2f, z), new Vector2(5.05f, z), new Vector2(6.9f, z), new Vector2(8.75f, z), new Vector2(9.43f, z) });
        // Enter seats from desk ends; the narrow chair-back gaps are not circulation routes.
        foreach (float x in new[] { 3.2f, 5.05f, 6.9f, 8.75f })
        {
            WalkRoute(controller, new[] { new Vector2(x, 6.4f), new Vector2(x, 6.65f) });
            WalkRoute(controller, new[] { new Vector2(x, 9.35f), new Vector2(x, 8.95f) });
            WalkRoute(controller, new[] { new Vector2(x, 9.35f), new Vector2(x, 9.75f) });
            WalkRoute(controller, new[] { new Vector2(x, 12.3f), new Vector2(x, 12.05f) });
        }
        WalkRoute(controller, new[] { new Vector2(9.43f, 6.4f), new Vector2(9.43f, 7.25f), new Vector2(9.43f, 9.35f), new Vector2(9.43f, 11.45f), new Vector2(9.43f, 12.3f) });
        WalkRoute(controller, new[] { new Vector2(1.4f, 9.35f), new Vector2(.35f, 9.35f), new Vector2(.35f, 8.35f) });
        WalkRoute(controller, new[] { new Vector2(1.4f, 9.35f), new Vector2(1.4f, 10.15f), new Vector2(-.18f, 10.15f), new Vector2(-.18f, 11.0f) });
    }

    static void VerifyReadingSuiteRoutes(CharacterController controller)
    {
        // The booth knee space is for seated use; approaches are from its two open ends.
        // The entry route turns east before reaching the south chairs.
        WalkRoute(controller, new[] { new Vector2(13.3f, 4.6f), new Vector2(13.3f, 6.4f), new Vector2(13.3f, 7.55f), new Vector2(16.35f, 7.55f), new Vector2(16.35f, 8.8f), new Vector2(16.35f, 10.2f), new Vector2(16.35f, 11.65f) });
        foreach (float x in new[] { 12.05f, 14.75f })
            WalkRoute(controller, new[] { new Vector2(13.3f, 7.55f), new Vector2(x, 7.55f), new Vector2(x, 9.05f) });
        WalkRoute(controller, new[] { new Vector2(16.35f, 7.55f), new Vector2(16.35f, 9.55f), new Vector2(15.20f, 9.55f) });
        WalkRoute(controller, new[] { new Vector2(13.3f, 7.55f), new Vector2(11.65f, 7.55f), new Vector2(11.65f, 9.25f) });
        WalkRoute(controller, new[] { new Vector2(16.35f, 11.65f), new Vector2(14.70f, 11.65f) });
        WalkRoute(controller, new[] { new Vector2(13.3f, 7.55f), new Vector2(11.10f, 7.55f), new Vector2(11.10f, 8.35f), new Vector2(11.10f, 9.10f) });
        WalkRoute(controller, new[] { new Vector2(16.35f, 7.55f), new Vector2(16.35f, 8.8f), new Vector2(16.35f, 10.2f), new Vector2(16.35f, 11.4f) });
        foreach (float x in new[] { 14.75f, 15.55f, 16.35f, 11.65f })
            WalkRoute(controller, new[] { new Vector2(13.3f, 7.55f), new Vector2(x, 7.55f), new Vector2(x, 7.15f) });
    }

    static void FocusModel()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(0, 1, 0), Quaternion.Euler(43, -35, 0), 29, false, true); view.Focus(); view.Repaint();
    }

    static void FocusTeacherSuite()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(-10.55f, 1, -9.35f), Quaternion.Euler(58, -10, 0), 10.5f, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureTeacherRevision()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary teacher furniture review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "03_Teacher_Office", new Vector3(-4, 2.45f, -6.65f), new Vector3(-12, 1.1f, -9.8f), 78);
            Shot(camera, "09_Teacher_Suite_Cutaway", new Vector3(-10.55f, 10.5f, -11.8f), new Vector3(-10.55f, .6f, -9.35f), 62);
            Shot(camera, "12_Warm_Faculty_Study", new Vector3(-10.2f, 2.6f, -7.5f), new Vector3(-14.1f, .85f, -10.1f), 84);
            Shot(camera, "13_Larger_Faculty_Storage", new Vector3(-14.8f, 1.85f, -9.4f), new Vector3(-15.8f, 1.05f, -6.32f), 68);
            Shot(camera, "14_Casual_Discussion", new Vector3(-7.7f, 2.7f, -9.3f), new Vector3(-5.35f, .85f, -7.7f), 73);
            Shot(camera, "15_Integrated_Storage_and_Amenities", new Vector3(-13.45f, 2.0f, -9.7f), new Vector3(-13.45f, 1.05f, -6.36f), 75);
            Shot(camera, "16_Beverage_and_Print_Counter", new Vector3(-12.025f, 1.9f, -8.6f), new Vector3(-12.025f, 1.15f, -6.36f), 70);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    static void FocusAdministrativeSuite()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(6.65f, 1, -9.35f), Quaternion.Euler(58, -10, 0), 8.5f, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureAdminRevision()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary administrative review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "05_Admin_and_Records", new Vector3(7.3f, 2.3f, -6.5f), new Vector3(6.4f, .9f, -10.4f), 82);
            Shot(camera, "11_Admin_Cutaway", new Vector3(7.4f, 8.5f, -6.5f), new Vector3(6.65f, .6f, -9.35f), 60);
            Shot(camera, "17_Admin_Lounge_and_Service_Cabinet", new Vector3(8.3f, 2.1f, -9.1f), new Vector3(4.7f, 1.0f, -7.7f), 78);
            Shot(camera, "18_Admin_Print_and_Water", new Vector3(5.3f, 1.8f, -8.7f), new Vector3(4.9f, 1.0f, -6.38f), 68);
            Shot(camera, "19_Admin_Moved_Entrance", new Vector3(7.3f, 1.8f, -4.3f), new Vector3(7.3f, 1.3f, -6.0f), 78);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    static void FocusMeetingSuite()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(13.9f, .8f, -9.35f), Quaternion.Euler(64, -12, 0), 7, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureMeetingRevision()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary meeting suite review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "20_S04_Entrance_View", new Vector3(13.3f, 2.2f, -6.65f), new Vector3(14, .95f, -10.1f), 88);
            Shot(camera, "21_S04_Cutaway", new Vector3(14.2f, 9.6f, -7.9f), new Vector3(13.9f, .5f, -9.35f), 61);
            Shot(camera, "22_S04_Meeting_Table", new Vector3(16.1f, 2.5f, -11.75f), new Vector3(12.8f, 1.1f, -9.25f), 78);
            Shot(camera, "23_S04_Window_Conversation", new Vector3(14.7f, 1.8f, -8.6f), new Vector3(16.65f, .85f, -10.85f), 76);
            Shot(camera, "24_S04_Storage_and_Entry", new Vector3(14.9f, 1.8f, -9.2f), new Vector3(15.2f, 1.1f, -6.3f), 82);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    static void FocusRegularClassroom()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(-13.55f, .7f, 9.35f), Quaternion.Euler(60, 90, 0), 7.8f, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureRegularClassroom()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary regular classroom review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "26_N01_Cutaway", new Vector3(-13.9f, 9.4f, 7.9f), new Vector3(-13.55f, .5f, 9.35f), 62);
            Shot(camera, "27_N01_Teaching_Front", new Vector3(-16.8f, 2.1f, 9.4f), new Vector3(-10.3f, 1.0f, 9.35f), 82);
            Shot(camera, "28_N01_Back_of_Classroom", new Vector3(-11.9f, 2.15f, 8.325f), new Vector3(-17.15f, 1.0f, 9.35f), 83);
            Shot(camera, "29_N01_Student_Desks", new Vector3(-14.8f, 1.85f, 10.375f), new Vector3(-11.4f, .8f, 11.4f), 76);
            Shot(camera, "31_N01_Teaching_Podium", new Vector3(-12.15f, 2.35f, 10.0f), new Vector3(-10.5f, .72f, 11.65f), 58);
            Shot(camera, "32_N01_Teacher_Chair", new Vector3(-10.0f, 2.25f, 10.5f), new Vector3(-9.82f, .58f, 12.05f), 58);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    static void FocusDigitalClassroom()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(4.825f, .7f, 9.35f), Quaternion.Euler(60, 270, 0), 9.6f, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureDigitalClassroom()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary digital classroom review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "33_N03_Cutaway", new Vector3(5.2f, 10.6f, 7.7f), new Vector3(4.825f, .5f, 9.35f), 68);
            Shot(camera, "34_N03_Teaching_Front", new Vector3(9.43f, 2.15f, 9.35f), new Vector3(.5f, 1.05f, 9.35f), 82);
            Shot(camera, "35_N03_Workstations", new Vector3(7.1f, 2.25f, 9.35f), new Vector3(4.15f, .85f, 10.9f), 76);
            Shot(camera, "36_N03_Resources_and_Projects", new Vector3(6.9f, 2.05f, 9.35f), new Vector3(10.15f, 1.25f, 9.35f), 83);
            Shot(camera, "38_N03_Main_Teaching_Screen", new Vector3(3.6f, 2.30f, 7.1f), new Vector3(-.55f, 1.50f, 9.3f), 70);
        }
        finally { Object.DestroyImmediate(obj); }
    }

    static void FocusReadingSuite()
    {
        Selection.activeGameObject = null;
        var view = SceneView.lastActiveSceneView;
        if (!view) return;
        view.in2DMode = false; view.sceneLighting = true;
        view.LookAt(new Vector3(14.1f, .7f, 9.35f), Quaternion.Euler(57, -10, 0), 9.0f, false, true);
        view.Focus(); view.Repaint();
    }

    static void CaptureReadingSuite()
    {
        RequireScene(); Directory.CreateDirectory(Output);
        var obj = new GameObject("Temporary team project studio review camera");
        var camera = obj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.86f, .89f, .90f);
        camera.nearClipPlane = .05f; camera.farClipPlane = 150;
        try
        {
            Shot(camera, "01_Overview", new Vector3(24, 32, -30), new Vector3(0, .7f, 0), 46);
            Shot(camera, "45_N04_Project_Studio_Cutaway", new Vector3(14.1f, 9.5f, 7.3f), new Vector3(14.1f, .65f, 9.6f), 67);
            Shot(camera, "46_N04_Team_Booth", new Vector3(15.7f, 2.3f, 7.55f), new Vector3(12.9f, .95f, 10.65f), 76);
            Shot(camera, "47_N04_Discussion_Wall", new Vector3(14.3f, 2.0f, 8.0f), new Vector3(10.8f, 1.45f, 8.4f), 76);
            Shot(camera, "48_N04_Window_Workbench", new Vector3(14.95f, 2.05f, 7.7f), new Vector3(17.1f, .98f, 10.1f), 78);
            Shot(camera, "49_N04_Studio_From_Entry", new Vector3(13.3f, 2.1f, 6.6f), new Vector3(14.0f, 1.0f, 10.45f), 88);
            Shot(camera, "51_N04_Team_Storage_and_Progress", new Vector3(14.2f, 2.1f, 9.3f), new Vector3(15.4f, 1.4f, 6.35f), 78);
        }
        finally { Object.DestroyImmediate(obj); }
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
        CaptureMeetingRevision();
        CaptureRegularClassroom();
        CaptureDigitalClassroom();
        CaptureReadingSuite();
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
            VerifyMeetingRoutes(view.walkPlayer.GetComponent<CharacterController>());
            Teleport(view.walkPlayer, new Vector3(15.75f, .08f, -10.1f));
            view.walkPlayer.transform.rotation = Quaternion.Euler(0, 288, 0);
            SaveRender(view.walkPlayer.GetComponentInChildren<Camera>(), Output + "/25_S04_Runtime_Walk_View.png");
            VerifyRegularClassroomRoutes(view.walkPlayer.GetComponent<CharacterController>());
            Teleport(view.walkPlayer, new Vector3(-16.835f, .08f, 8.325f));
            view.walkPlayer.transform.rotation = Quaternion.Euler(0, 90, 0);
            SaveRender(view.walkPlayer.GetComponentInChildren<Camera>(), Output + "/30_N01_Runtime_Classroom.png");
            VerifyDigitalClassroomRoutes(view.walkPlayer.GetComponent<CharacterController>());
            Teleport(view.walkPlayer, new Vector3(9.43f, .08f, 9.35f));
            view.walkPlayer.transform.rotation = Quaternion.Euler(0, 270, 0);
            SaveRender(view.walkPlayer.GetComponentInChildren<Camera>(), Output + "/37_N03_Runtime_Digital_Classroom.png");
            VerifyReadingSuiteRoutes(view.walkPlayer.GetComponent<CharacterController>());
            Teleport(view.walkPlayer, new Vector3(16.35f, .08f, 7.55f));
            view.walkPlayer.transform.rotation = Quaternion.Euler(0, 315, 0);
            SaveRender(view.walkPlayer.GetComponentInChildren<Camera>(), Output + "/50_N04_Runtime_Project_Studio.png");
            Teleport(view.walkPlayer, runtimeStartPosition);
            view.walkPlayer.transform.rotation = runtimeStartRotation;
            view.SendMessage("SetWalking", false, SendMessageOptions.RequireReceiver);
            if (!camera.enabled || view.walkPlayer.activeSelf) throw new InvalidOperationException("Returning to overview failed.");
            string report = "PASS: furnished model renders in actual Play mode.\nPASS: perspective overview -> first-person reception -> overview.\nPASS: glass office door starts closed, opens on approach, allows the actual tour controller through, and closes on departure.\nPASS: the actual first-person controller reaches the S04 entrance, all eight meeting-seat approaches, display/whiteboard, closed cabinet and both lounge-chair approaches.\nPASS: the actual first-person controller reaches the N01 entrance, both main aisles, all 24 student-seat side approaches, teacher position and rear storage/display area.\nPASS: the actual first-person controller reaches the N03 entrance, all sixteen computer-seat side approaches, front teaching wall, teacher position and rear storage/project displays.\nPASS: the actual first-person controller reaches the N04 open project area, all three movable-chair approaches, both booth ends, writable wall, window workbench, member storage and material cart.\nPASS: the structure remains open-topped for inspection; room-focus shortcuts 0--8 are available.";
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
        File.WriteAllText(Output + "/README.md", "# 已讨论通过的室内布置\n\n调整前版本已备份至个人 fork（7851235），可用于对照和回退。\n\n- N01：24席日常授课教室，12张1.20×0.55m可移动双人桌，4排3列朝东侧实墙；轻便彩色学生椅，东墙白板与教学屏、前侧1.70×0.72m柜式教学讲台（台高0.90m，带前挡板、教材柜、电脑和教学控制面板），后部窗下低资料柜与两块学生作品公告板。\n- N02：4组弧形拼桌，每组4席；展示屏、白板及低资料柜。\n- N03：16席数字学习室，8张2.00×0.80m暖木双人电脑桌朝西侧教学墙，每席配固定电脑、键鼠和软包工作椅；中央通道约1.10m，两侧约0.80m；前方有柜式讲台、教师椅和一块3.20×1.80m主教学屏（已替换原白板并移除侧面小屏），东侧后部为低资料柜和两块学生项目展示板，走线收于桌下。\n- N04：6人小组项目工作室，低背L形软包卡座3席与3把活动椅，围合两张可拼接移动桌（总2.40×1.50m）；桌上为共用图纸和低矮模型。西墙大白板供起身讨论，南墙有项目进度板、6个组员储物格及材料抽屉，东窗下为0.86m高共用项目操作台。原阅读扶手椅、茶几、成排书柜、阅读灯与个人书本布置已撤除，保留入口至开放活动区和设备前的通路。\n- S01：6张暖木书桌沿南窗（4）与西窗（2）布置，桌面宽1.6–1.8m、深0.8m；相邻桌间为1.12m侧隔板。北墙两大柜各1.6×2.1×0.5m，保留奶油门与蜂蜜木框；原北墙两工位已撤除，向东延伸3.55m一体式茶水/文印柜，包含饮水机、咖啡机、0.65m净操作台、打印机、上下储物和茶水/文印分隔片。安静区仍由到顶玻璃与自动玻璃门围合；玻璃外换为椭圆木讨论桌和4把软包木椅，与原沙发休息区保持共同开放。\n- S02：从南侧入口进入后，右侧小接待台，左侧轻巧沙发；学院标识位于沙发上方，公告栏位于旁边靠入口处。中央进出路线保持畅通。\n- S03：2个行政工位面向+Z入口；西侧为双人沙发和0.60×1.00m椭圆茶几，长边与沙发平行。北侧原书架处改为打印/饮水一体柜，原打印处放封闭资料柜，保留东侧档案柜。北门中心沿+X移至7.3m，宽1.2m不变。\n- S04：3.20×1.35m浅木长方形会议桌和8把方正软包靠背椅，两端各加一席，整套沿+X微移0.10m；东南窗边两把扶手椅与小边几；西侧正对会议桌改为2.10×1.25m大白板，保留旁边小白板，北侧门旁为封闭矮柜。配收拢侧帘及少量绿植，作为小型会议和师生面谈空间的初稿。\n\n其余房间继续保留上一阶段空结构，尚未决定家具。外轮廓、外墙门窗、两座楼梯和两条2.4m公共走廊保持既有结构；S03北门及相邻墙段仅作上述局部调整。\n\n在 Unity 打开 `Assets/Scenes/JEI_Floor01_Structure.unity`。Play后拖拽环绕、滚轮缩放，0总览、1分组教室、2教师办公、3接待、4行政资料、5会议面谈、6普通教室、7数字学习室、8项目工作室；Tab切换步行，WASD移动，Esc释放鼠标。\n\n单独更新N04：`Tools/JEI/Interiors/Update N04 Project Studio Only`；单独更新N03：`Tools/JEI/Interiors/Update Digital Classroom Only`；单独更新N01：`Tools/JEI/Interiors/Update Regular Classroom Only`；单独更新S04：`Tools/JEI/Interiors/Update Meeting Suite Only`；单独更新行政区：`Tools/JEI/Interiors/Update Administrative Suite Only`；仅更新教师区：`Tools/JEI/Interiors/Update Teacher Suite Only`。全量布置入口：`Tools/JEI/Interiors/Apply Agreed Furnishings`。此操作只重建带生成标记的室内布置组，结构生成器与本工具分别保留；若重建结构，请再次应用室内布置。\n\n所有自建桌椅网格、材质和地面纹理保存为项目资源，非临时对象。玻璃墙表现空间分隔，未进行声学仿真。验证与三维预览位于本目录。\n");
    }
}
