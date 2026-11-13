using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
using F=JEIFurnitureKit;

/// <summary>Append-only C02/C03 restroom drafts, independently of the eastern services.</summary>
[InitializeOnLoad]
public static class JEIRestroomTools
{
    const string ScenePath="Assets/Scenes/JEI_Floor01_Structure.unity";
    const string FloorPath="Floor_01__36x26m__Storey_3.60m";
    const string OwnedName="09_Public_Restrooms";
    const string Request="Library/JEI_Restrooms.request";
    const string Result="Library/JEI_Restrooms.result";
    const string Output="Documentation/Public_Restrooms";
    static double nextPoll;
    static JEIRestroomTools() { EditorApplication.update+=Poll; }
    static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<nextPoll) return;
        nextPoll=EditorApplication.timeSinceStartup+.75;
        if(!File.Exists(Request)) return;
        string action=File.ReadAllText(Request).Trim();File.Delete(Request);
        try {
            if(action=="build") Apply();
            else if(action=="verify") File.WriteAllText(Result,"SUCCESS: "+Verify());
            else if(action=="capture") { Capture();File.WriteAllText(Result,"SUCCESS: restroom 3D previews saved."); }
            else if(action=="focus_women") FocusWomen();
            else if(action=="focus_men") FocusMen();
            else if(action=="focus") Focus();
            else throw new ArgumentException("Unknown restroom action: "+action);
        }
        catch(Exception e) { File.WriteAllText(Result,"ERROR: "+e);Debug.LogException(e); }
    }
    static Transform RequireFloor()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path!=ScenePath) throw new InvalidOperationException("Open JEI_Floor01_Structure in Edit mode first.");
        var building=GameObject.Find("JEI_Structure_Study");var floor=building?building.transform.Find(FloorPath):null;
        if(!floor) throw new InvalidOperationException("The existing first-floor structure is missing.");return floor;
    }

    [MenuItem("Tools/JEI/Public Restrooms/Apply C02 and C03 Draft")]
    public static void Apply()
    {
        var floor=RequireFloor();var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Restroom_Backups");
        if(scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath,"Library/JEI_Restroom_Backups/"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_before_public_restrooms.unity",true);
        var previous=floor.Find(OwnedName);
        if(previous) {
            if(!previous.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("The restroom group is not generator-owned; it was preserved.");
            PreserveManual(previous,floor);Undo.DestroyObjectImmediate(previous.gameObject);
        }
        var owned=F.Group(floor,OwnedName);JEIRestroomRooms.BuildWomen(owned);JEIRestroomRooms.BuildMen(owned);
        HidePlaceholders(floor);Mark(owned);Undo.RegisterCreatedObjectUndo(owned.gameObject,"Furnish C02 and C03 restrooms");
        Physics.SyncTransforms();string report=Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save restroom drafts.");
        AssetDatabase.SaveAssets();Capture();Focus();
        File.WriteAllText(Result,"SUCCESS: C02 and C03 3D restroom drafts saved; eastern services and lift reserve were not rebuilt.\n"+report);
        Debug.Log("JEI C02 and C03 restroom drafts saved locally for review.");
    }
    static void HidePlaceholders(Transform floor)
    {
        var rooms=floor.Find("01_Rooms__numbered");
        foreach(string prefix in new[]{"C02__","C03__"}) {
            var room=rooms.Cast<Transform>().Single(t=>t.name.StartsWith(prefix,StringComparison.Ordinal));
            if(room.childCount!=2 || room.Cast<Transform>().Any(t=>!t.name.StartsWith("Zone floor__",StringComparison.Ordinal) && !t.name.StartsWith("Label__",StringComparison.Ordinal))) throw new InvalidOperationException("Manual content found in structural restroom node: "+room.name);
            Undo.RecordObject(room.gameObject,"Hide furnished restroom diagram floor");room.gameObject.SetActive(false);
        }
    }
    static void Mark(Transform node)
    {
        if(!node.GetComponent<JEIGeneratedObject>()) node.gameObject.AddComponent<JEIGeneratedObject>();foreach(Transform child in node) Mark(child);
    }
    static void PreserveManual(Transform node,Transform destination)
    {
        for(int i=node.childCount-1;i>=0;i--) { var child=node.GetChild(i);if(!child.GetComponent<JEIGeneratedObject>()) child.SetParent(destination,true);else PreserveManual(child,destination); }
    }

    [MenuItem("Tools/JEI/Public Restrooms/Verify Restroom Routes")]
    public static void VerifyMenu() { File.WriteAllText(Result,"SUCCESS: "+Verify()); }
    public static string Verify()
    {
        var floor=RequireFloor();var owned=floor.Find(OwnedName);if(!owned) throw new InvalidOperationException("Public restroom furnishings are missing.");Physics.SyncTransforms();
        var women=owned.Find("C02__Womens_restroom");var men=owned.Find("C03__Mens_restroom");
        if(!women || !men) throw new InvalidOperationException("Both public restroom groups are required.");
        var nodes=owned.GetComponentsInChildren<Transform>();
        foreach(string id in new[]{"C02_Toilet_01","C02_Toilet_02","C02_Toilet_03","C03_Toilet_01","C03_Toilet_02","C02_Basin_01","C02_Basin_02","C03_Basin_01","C03_Basin_02","C03_Urinal_01","C03_Urinal_02"})
            if(nodes.Count(t=>t.name==id)!=1) throw new InvalidOperationException("Unexpected fixture count: "+id);
        foreach(var room in new[]{women,men}) {
            bool north=room==women;float z0=north?.1f:-3.2f,z1=north?3.2f:-.1f;
            foreach(var collider in room.GetComponentsInChildren<Collider>()) {
                var b=collider.bounds;
                if(b.min.x< -12.311f || b.max.x> -6.689f || b.min.z<z0-.011f || b.max.z>z1+.011f) throw new InvalidOperationException("Restroom collider exceeds its room: "+collider.name);
            }
        }
        foreach(var mesh in owned.GetComponentsInChildren<MeshFilter>()) if(!mesh.sharedMesh) throw new InvalidOperationException("Missing restroom mesh: "+mesh.name);
        foreach(var renderer in owned.GetComponentsInChildren<Renderer>()) if(renderer.sharedMaterials.Any(m=>!m)) throw new InvalidOperationException("Missing restroom material: "+renderer.name);
        var plan=JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));int doors=0;
        foreach(var wall in plan.walls) foreach(var opening in wall.openings) {
            if(opening.kind!="door" || JEIServiceAreas.IsClosedLiftOpening(wall.axis,wall.at,opening.at)) continue;
            bool h=wall.axis=="H";var center=h?new Vector3(opening.at,1,wall.at):new Vector3(wall.at,1,opening.at);
            var half=h?new Vector3(opening.width/2-.08f,.7f,.07f):new Vector3(.07f,.7f,opening.width/2-.08f);
            foreach(var hit in Physics.OverlapBox(center,half)) if(!(hit is CharacterController)) throw new InvalidOperationException("Existing doorway obstructed by "+hit.name);
            doors++;
        }
        foreach(float z in new[]{-4.6f,4.6f}) if(Physics.BoxCast(new Vector3(-17.5f,1,z),new Vector3(.03f,.7f,1.16f),Vector3.right,out var blocked,Quaternion.identity,35)) throw new InvalidOperationException("Main corridor obstructed by "+blocked.collider.name);
        foreach(float x in new[]{-5.3f,5.3f}) if(Physics.BoxCast(new Vector3(x,1,-3.1f),new Vector3(1.16f,.7f,.03f),Vector3.forward,out var blocked,Quaternion.identity,6.2f)) throw new InvalidOperationException("Courtyard side link obstructed by "+blocked.collider.name);
        foreach(float side in new[]{-1f,1f}) foreach(float z in new[]{3.50f,3.30f,3.10f,2.90f})
            if(!Physics.Raycast(new Vector3(-9.5f,.5f,z*side),Vector3.down,out var ground,.6f) || Mathf.Abs(ground.point.y)>.003f) throw new InvalidOperationException("Public restroom entrance finish is raised near Z="+z*side);
        foreach(float x in new[]{-9.92f,-9.08f}) foreach(float z in new[]{-.95f,-2.05f})
            if(!Physics.Linecast(new Vector3(x,1.40f,-3.18f),new Vector3(-7.10f,1.10f,z),out var sight) || sight.collider.name!="C03_Entry_privacy_screen") throw new InvalidOperationException("The men's entry sightline does not reach the intended privacy screen.");
        var probe=new GameObject("Temporary public-restroom walk probe");var c=probe.AddComponent<CharacterController>();c.height=1.8f;c.center=new Vector3(0,.9f,0);c.radius=.30f;c.skinWidth=.035f;c.stepOffset=.25f;
        try {
            Walk(c,new[]{new Vector2(-9.5f,4.6f),new Vector2(-9.5f,2.70f),new Vector2(-10.05f,2.70f),new Vector2(-11.05f,2.70f),new Vector2(-10.05f,2.70f),new Vector2(-10.05f,1.65f),new Vector2(-11.05f,1.65f),new Vector2(-10.05f,1.65f),new Vector2(-10.05f,.60f),new Vector2(-11.05f,.60f),new Vector2(-10.05f,.60f),new Vector2(-9.5f,.60f),new Vector2(-7.65f,.90f),new Vector2(-7.65f,2.20f),new Vector2(-9.5f,2.70f),new Vector2(-9.5f,4.6f)});
            Walk(c,new[]{new Vector2(-9.5f,-4.6f),new Vector2(-9.5f,-2.65f),new Vector2(-9.5f,-1.10f),new Vector2(-10.05f,-1.65f),new Vector2(-11.05f,-1.65f),new Vector2(-10.05f,-1.65f),new Vector2(-10.05f,-.60f),new Vector2(-11.05f,-.60f),new Vector2(-10.05f,-.60f),new Vector2(-10.05f,-1.10f),new Vector2(-9.30f,-1.10f),new Vector2(-8.25f,-1.10f),new Vector2(-7.65f,-1.10f),new Vector2(-7.65f,-.95f),new Vector2(-7.65f,-2.05f),new Vector2(-7.65f,-1.10f),new Vector2(-9.5f,-1.10f),new Vector2(-9.5f,-2.65f),new Vector2(-9.5f,-4.6f)});
        }
        finally { Object.DestroyImmediate(probe);Physics.SyncTransforms(); }
        string report="PASS: C02 has three toilet cubicles and two washbasins; C03 has two toilet cubicles, two screened urinals and two washbasins.\n"+
            "PASS: actual editor-mode CharacterController routes enter all five open cubicles, reach every washbasin and both urinals, and return to the public corridors without passing through partitions or fixtures.\n"+
            "PASS: both restroom entrances retain a near-flush 1mm finish; raycasts across each door confirm no tile step.\n"+
            "PASS: sightline casts from both sides of the men's entrance toward both urinals first strike the entry privacy screen.\n"+
            "PASS: "+doors+" walking doorways, both 2.40m main corridors and both courtyard-side links remain unobstructed; the existing reserved lift entrance was not rebuilt.\n"+
            "PASS: furnishing colliders stay within C02/C03 and all mesh/material references resolve.\n"+
            "Scope: 3D furnishing draft with cubicle doors shown fixed open for inspection; editor-mode geometry and collision checks, no Play-mode or occupancy/whole-building code certification.";
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/Verification.txt",report);return report;
    }
    static void Walk(CharacterController c,Vector2[] points)
    {
        c.enabled=false;c.transform.position=new Vector3(points[0].x,.15f,points[0].y);c.enabled=true;Physics.SyncTransforms();for(int i=0;i<12;i++) c.Move(Vector3.down*.025f);
        for(int n=1;n<points.Length;n++) {
            bool reached=false;
            for(int i=0;i<900;i++) { var p=c.transform.position;var delta=points[n]-new Vector2(p.x,p.z);if(delta.magnitude<.035f) { reached=true;break; }var step=Vector2.ClampMagnitude(delta,.035f);c.Move(new Vector3(step.x,-.035f,step.y)); }
            if(!reached) throw new InvalidOperationException("Restroom route blocked near "+c.transform.position+" toward "+points[n]);
        }
    }

    [MenuItem("Tools/JEI/Public Restrooms/Focus Both Restrooms")]
    public static void Focus() { Look(new Vector3(-9.5f,.75f,0),Quaternion.Euler(68,-15,0),5.5f); }
    [MenuItem("Tools/JEI/Public Restrooms/Focus C02 Women")]
    public static void FocusWomen() { Look(new Vector3(-9.5f,.65f,1.65f),Quaternion.Euler(72,160,0),3.8f); }
    [MenuItem("Tools/JEI/Public Restrooms/Focus C03 Men")]
    public static void FocusMen() { Look(new Vector3(-9.5f,.65f,-1.65f),Quaternion.Euler(72,-15,0),3.8f); }
    static void Look(Vector3 center,Quaternion rotation,float size)
    {
        RequireFloor();Selection.activeGameObject=null;var view=SceneView.lastActiveSceneView;if(!view) return;
        view.in2DMode=false;view.sceneLighting=true;view.LookAt(center,rotation,size,false,true);view.Focus();view.Repaint();
    }
    [MenuItem("Tools/JEI/Public Restrooms/Capture Restroom Previews")]
    public static void Capture()
    {
        RequireFloor();Directory.CreateDirectory(Output);var obj=new GameObject("Temporary public-restroom review camera");var camera=obj.AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.86f,.89f,.90f);camera.nearClipPlane=.04f;camera.farClipPlane=150;
        try {
            Shot(camera,"Documentation/Floor01_Furnishings/01_Overview.png",new Vector3(24,32,-30),new Vector3(0,.7f,0),46);
            Shot(camera,Output+"/01_Restrooms_3D_Overview.png",new Vector3(-5.0f,9.5f,-7.5f),new Vector3(-9.5f,.6f,0),59);
            Shot(camera,Output+"/02_C02_Womens_Restroom.png",new Vector3(-9.50f,2.45f,3.04f),new Vector3(-9.50f,.85f,1.15f),92);
            Shot(camera,Output+"/03_C03_Mens_Restroom.png",new Vector3(-9.45f,2.40f,-2.95f),new Vector3(-9.05f,.95f,-1.05f),94);
            Shot(camera,Output+"/04_C02_Cubicles.png",new Vector3(-8.75f,2.25f,1.65f),new Vector3(-11.60f,.90f,1.65f),88);
            Shot(camera,Output+"/05_C03_Urinals_and_Washbasins.png",new Vector3(-7.85f,1.65f,-1.50f),new Vector3(-6.925f,.78f,-1.50f),85);
            Shot(camera,Output+"/06_C02_Room_Layout.png",new Vector3(-9.5f,5.8f,3.8f),new Vector3(-9.5f,.5f,1.65f),45);
        }
        finally { Object.DestroyImmediate(obj); }
    }
    static void Shot(Camera camera,string path,Vector3 position,Vector3 target,float fov)
    {
        camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=fov;
        var previous=RenderTexture.active;var previousTarget=camera.targetTexture;var rt=new RenderTexture(1800,1200,24);var tex=new Texture2D(1800,1200,TextureFormat.RGB24,false);
        try { camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1800,1200),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG()); }
        finally { RenderTexture.active=previous;camera.targetTexture=previousTarget;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex); }
    }
}
