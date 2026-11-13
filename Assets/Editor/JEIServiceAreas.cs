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

/// <summary>Local furnishing drafts for the four eastern service spaces.</summary>
[InitializeOnLoad]
public static class JEIServiceAreas
{
    const string ScenePath="Assets/Scenes/JEI_Floor01_Structure.unity";
    const string FloorPath="Floor_01__36x26m__Storey_3.60m";
    const string OwnedName="08_Service_Areas";
    const string Request="Library/JEI_Service.request";
    const string Result="Library/JEI_Service.result";
    const string Output="Documentation/Service_Areas";
    const float FinishTop=.001f;
    const float FinishOffset=FinishTop-.075f;
    static double nextPoll;

    static JEIServiceAreas() { EditorApplication.update+=Poll; }
    static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup<nextPoll) return;
        nextPoll=EditorApplication.timeSinceStartup+.75;
        if(!File.Exists(Request)) return;
        string action=File.ReadAllText(Request).Trim();File.Delete(Request);
        try {
            if(action=="build") Apply();
            else if(action=="verify") File.WriteAllText(Result,"SUCCESS: "+Verify());
            else if(action=="capture") { Capture();File.WriteAllText(Result,"SUCCESS: service-area previews saved."); }
            else if(action=="focus_cleaner") FocusCleaner();
            else if(action=="focus_wc") FocusWashroom();
            else if(action=="focus_it") FocusIT();
            else if(action=="focus_lift") FocusLift();
            else if(action=="focus") Focus();
            else throw new ArgumentException("Unknown service-area action: "+action);
        }
        catch(Exception e) { File.WriteAllText(Result,"ERROR: "+e);Debug.LogException(e); }
    }

    static Transform RequireFloor()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path!=ScenePath)
            throw new InvalidOperationException("Open JEI_Floor01_Structure in Edit mode first.");
        var building=GameObject.Find("JEI_Structure_Study");
        var floor=building?building.transform.Find(FloorPath):null;
        if(!floor) throw new InvalidOperationException("First-floor structure is missing.");
        return floor;
    }

    [MenuItem("Tools/JEI/Service Areas/Apply C05 C06 C07 C08 Draft")]
    public static void Apply()
    {
        var floor=RequireFloor();var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Library/JEI_Service_Backups");
        if(scene.isDirty && !EditorSceneManager.SaveScene(scene)) throw new IOException("Could not preserve existing scene edits.");
        File.Copy(ScenePath,"Library/JEI_Service_Backups/"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+"_before_services.unity",true);
        var previous=floor.Find(OwnedName);
        if(previous) {
            if(!previous.GetComponent<JEIGeneratedObject>()) throw new InvalidOperationException("Existing service-area group is not generator-owned; it was preserved.");
            PreserveManual(previous,floor);Undo.DestroyObjectImmediate(previous.gameObject);
        }
        var owned=F.Group(floor,OwnedName);
        JEIServiceStorage.BuildCleaner(owned).localPosition=Vector3.up*FinishOffset;
        BuildLiftEntrance(owned);
        JEIAccessibleWashroom.Build(owned).localPosition=Vector3.up*FinishOffset;
        JEIServiceStorage.BuildIT(owned).localPosition=Vector3.up*FinishOffset;
        HideServicePlaceholders(floor);
        Mark(owned);Undo.RegisterCreatedObjectUndo(owned.gameObject,"Furnish eastern service areas");
        Physics.SyncTransforms();
        string report=Verify();
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save service-area draft.");
        AssetDatabase.SaveAssets();Capture();Focus();
        File.WriteAllText(Result,"SUCCESS: C05, C07 and C08 furnished; C06 static lift entrance saved with shaft reserve retained.\n"+report);
        Debug.Log("JEI service-area 3D drafts saved locally for review.");
    }

    static void Mark(Transform node)
    {
        if(!node.GetComponent<JEIGeneratedObject>()) node.gameObject.AddComponent<JEIGeneratedObject>();
        foreach(Transform child in node) Mark(child);
    }
    static void PreserveManual(Transform node,Transform destination)
    {
        for(int i=node.childCount-1;i>=0;i--) {
            var child=node.GetChild(i);
            if(!child.GetComponent<JEIGeneratedObject>()) child.SetParent(destination,true);
            else PreserveManual(child,destination);
        }
    }

    static void HideServicePlaceholders(Transform floor)
    {
        var rooms=floor.Find("01_Rooms__numbered");
        foreach(string prefix in new[]{"C05__","C07__","C08__"}) {
            var room=rooms.Cast<Transform>().Single(t=>t.name.StartsWith(prefix,StringComparison.Ordinal));
            // This structural room node owns only its diagrammatic floor and label.
            // Preserve both objects instead of deleting them or lifting the finish.
            if(room.childCount!=2 || room.Cast<Transform>().Any(t=>!t.name.StartsWith("Zone floor__",StringComparison.Ordinal) && !t.name.StartsWith("Label__",StringComparison.Ordinal)))
                throw new InvalidOperationException("Manual service-room content must be preserved before hiding its placeholders: "+room.name);
            Undo.RecordObject(room.gameObject,"Hide furnished service-room diagram floor");room.gameObject.SetActive(false);
        }
    }

    public static bool IsClosedLiftOpening(string axis,float wallAt,float openingAt)
    {
        if(axis!="H" || Mathf.Abs(wallAt-3.3f)>.01f || Mathf.Abs(openingAt-10.9f)>.01f) return false;
        var building=GameObject.Find("JEI_Structure_Study");
        var landing=building?building.transform.Find(FloorPath+"/"+OwnedName+"/C06__Lift_entrance_reserve"):null;
        return landing && landing.GetComponent<JEIGeneratedObject>() && landing.GetComponent<BoxCollider>() && landing.gameObject.activeInHierarchy;
    }

    static void BuildLiftEntrance(Transform parent)
    {
        var g=F.Group(parent,"C06__Lift_entrance_reserve");
        var steel=F.Mat("C06_Brushed_lift_steel",new Color(.60f,.65f,.64f),.55f);
        var trim=F.Mat("C06_Sage_portal_trim",new Color(.28f,.37f,.33f),.30f);
        var joint=F.Mat("C06_Door_seam",new Color(.17f,.22f,.21f),.25f);
        var ivory=F.Mat("C06_Indicator_ivory",new Color(.91f,.91f,.83f),.20f);
        var dark=F.Mat("C06_Indicator_dark",new Color(.055f,.095f,.085f),.25f);
        var led=F.Mat("C06_Indicator_green",new Color(.35f,.65f,.45f),.25f);
        Action<string,Vector3,Vector3,Material> box=(name,pos,size,mat)=>F.Box(g,name,pos,size,mat,false);
        foreach(float x in new[]{10.62f,11.18f}) {
            box("Closed_brushed_steel_landing_door",new Vector3(x,1.14f,3.365f),new Vector3(.548f,2.13f,.055f),steel);
        }
        box("Central_vertical_door_seam",new Vector3(10.9f,1.14f,3.3955f),new Vector3(.010f,2.13f,.002f),joint);
        // Controls and portal sit on the corridor-facing wall surface, not behind it.
        foreach(float x in new[]{10.30f,11.50f}) box("Sage_portal_jamb",new Vector3(x,1.15f,3.41f),new Vector3(.12f,2.15f,.07f),trim);
        box("Sage_portal_header",new Vector3(10.9f,2.28f,3.41f),new Vector3(1.32f,.16f,.07f),trim);
        box("Inset_landing_indicator",new Vector3(10.9f,2.30f,3.449f),new Vector3(.45f,.115f,.008f),dark);
        F.WallText(g,"1  ^",new Vector3(10.9f,2.30f,3.455f),180,.24f,new Color(.82f,.96f,.79f));
        box("Quiet_threshold",new Vector3(10.9f,.065f,3.30f),new Vector3(1.08f,.03f,.24f),steel);
        var barrier=g.gameObject.AddComponent<BoxCollider>();barrier.center=new Vector3(10.9f,1.14f,3.365f);barrier.size=new Vector3(1.08f,2.13f,.06f);
        box("Wall_call_button_plate",new Vector3(11.73f,1.05f,3.425f),new Vector3(.16f,.28f,.035f),steel);
        var button=F.Primitive(g,"Round_lift_call_button",PrimitiveType.Cylinder,new Vector3(11.73f,1.075f,3.449f),new Vector3(.067f,.008f,.067f),ivory);
        button.transform.localRotation=Quaternion.Euler(90,0,0);
        F.Primitive(g,"Call_button_indicator",PrimitiveType.Sphere,new Vector3(11.73f,1.075f,3.461f),new Vector3(.016f,.016f,.007f),led);
        for(int i=0;i<3;i++) F.Primitive(g,"Tactile_raised_dot",PrimitiveType.Sphere,new Vector3(11.707f+i*.023f,.985f,3.446f),Vector3.one*.006f,joint);
        box("Lift_wayfinding_plate",new Vector3(10.9f,2.57f,3.43f),new Vector3(.65f,.18f,.022f),ivory);
        F.WallText(g,"LIFT",new Vector3(10.9f,2.57f,3.445f),180,.42f,new Color(.035f,.065f,.05f));
    }

    [MenuItem("Tools/JEI/Service Areas/Verify Service Areas")]
    public static void VerifyMenu() { File.WriteAllText(Result,"SUCCESS: "+Verify()); }
    public static string Verify()
    {
        var floor=RequireFloor();var owned=floor.Find(OwnedName);
        if(!owned) throw new InvalidOperationException("Service-area draft is missing.");
        Physics.SyncTransforms();
        var expected=new[]{"C05__Cleaner_and_store","C06__Lift_entrance_reserve","C07__Accessible_washroom","C08__IT_services"};
        foreach(string name in expected) if(!owned.Find(name)) throw new InvalidOperationException("Missing service room: "+name);
        for(int i=0;i<expected.Length;i++) {
            var room=owned.Find(expected[i]);
            float x0=i==0||i==2?6.7f:9.5f,x1=i==0||i==2?9.3f:12.3f;
            float z0=i<2?.1f:-3.2f,z1=i==1?3.40f:i==0?3.2f:-.1f;
            foreach(var collider in room.GetComponentsInChildren<Collider>()) {
                var b=collider.bounds;
                if(b.min.x<x0-.011f || b.max.x>x1+.011f || b.min.z<z0-.011f || b.max.z>z1+.011f)
                    throw new InvalidOperationException("Service furnishing exceeds its room: "+collider.name);
            }
        }
        foreach(var mesh in owned.GetComponentsInChildren<MeshFilter>()) if(!mesh.sharedMesh) throw new InvalidOperationException("Missing service mesh: "+mesh.name);
        foreach(var renderer in owned.GetComponentsInChildren<Renderer>()) if(renderer.sharedMaterials.Any(m=>!m)) throw new InvalidOperationException("Missing service material: "+renderer.name);
        var wc=owned.Find(expected[2]);
        // A clear square proves the full 1.525m turning circle is unobstructed.
        ClearBox(new Vector3(7.97f,.505f+FinishOffset,-1.9f),new Vector3(.7625f,.410f,.7625f),"C07 wheelchair turning area");
        ClearBox(new Vector3(8.155f,.4375f+FinishOffset,-.84f),new Vector3(.380f,.3425f,.610f),"C07 clear side-transfer strip");
        ClearBox(new Vector3(7.10f,.4275f+FinishOffset,-.50f),new Vector3(.15f,.3325f,.38f),"C07 lavatory knee space");
        foreach(float z in new[]{-3.50f,-3.30f,-3.10f,-2.90f})
            if(!Physics.Raycast(new Vector3(8,.5f,z),Vector3.down,out var ground,.6f) || Mathf.Abs(ground.point.y)> .003f)
                throw new InvalidOperationException("C07 entrance finish must remain flush with the corridor near Z="+z);
        var plan=JsonUtility.FromJson<JEIStructureBlockout.Plan>(File.ReadAllText("Assets/JEI_Blockout/Floor01_Plan.json"));
        int doors=0;
        foreach(var wall in plan.walls) foreach(var opening in wall.openings) {
            if(opening.kind!="door") continue;
            // C06 now deliberately has a closed static lift landing door.
            if(IsClosedLiftOpening(wall.axis,wall.at,opening.at)) continue;
            bool h=wall.axis=="H";
            var center=h?new Vector3(opening.at,1,wall.at):new Vector3(wall.at,1,opening.at);
            var half=h?new Vector3(opening.width/2-.08f,.7f,.07f):new Vector3(.07f,.7f,opening.width/2-.08f);
            ClearBox(center,half,"Existing door "+opening.at+" / "+wall.at);doors++;
        }
        foreach(float z in new[]{-4.6f,4.6f}) if(Physics.BoxCast(new Vector3(-17.5f,1,z),new Vector3(.03f,.7f,1.16f),Vector3.right,out var blocked,Quaternion.identity,35)) throw new InvalidOperationException("Main corridor blocked: "+blocked.collider.name);
        foreach(float x in new[]{-5.3f,5.3f}) if(Physics.BoxCast(new Vector3(x,1,-3.1f),new Vector3(1.16f,.7f,.03f),Vector3.forward,out var blocked,Quaternion.identity,6.2f)) throw new InvalidOperationException("Courtyard side link blocked: "+blocked.collider.name);
        if(!Physics.Raycast(new Vector3(10.9f,1,4.6f),Vector3.back,out var landing,1.8f) || landing.collider.transform!=owned.Find(expected[1])) throw new InvalidOperationException("Static lift entrance must remain closed toward the reserved shaft.");
        var probe=new GameObject("Temporary service-area walk probe");var controller=probe.AddComponent<CharacterController>();
        controller.height=1.8f;controller.center=new Vector3(0,.9f,0);controller.radius=.30f;controller.skinWidth=.035f;controller.stepOffset=.25f;
        try {
            Walk(controller,new[]{new Vector2(8,4.6f),new Vector2(8,2.8f),new Vector2(8,1.6f),new Vector2(8.05f,1.1f),new Vector2(8,1.6f),new Vector2(8.40f,1.05f),new Vector2(8,1.6f),new Vector2(7.15f,1.6f),new Vector2(7.25f,2.1f),new Vector2(8.20f,2.1f),new Vector2(8,2.8f),new Vector2(8,4.6f)});
            Walk(controller,new[]{new Vector2(8,-4.6f),new Vector2(8,-2.8f),new Vector2(7.97f,-1.9f),new Vector2(8.05f,-1.25f),new Vector2(8.05f,-.65f),new Vector2(7.65f,-.65f),new Vector2(7.65f,-.50f),new Vector2(7.65f,-1.25f),new Vector2(7.97f,-1.9f),new Vector2(8,-2.8f),new Vector2(8,-4.6f)});
            Walk(controller,new[]{new Vector2(10.9f,-4.6f),new Vector2(10.9f,-2.8f),new Vector2(10.9f,-1.6f),new Vector2(10.9f,-.72f),new Vector2(10.3f,-.72f),new Vector2(10.9f,-1.6f),new Vector2(10.9f,-2.8f),new Vector2(10.9f,-4.6f)});
            Walk(controller,new[]{new Vector2(10.9f,4.6f),new Vector2(10.9f,3.78f),new Vector2(10.9f,4.6f)});
        }
        finally { Object.DestroyImmediate(probe);Physics.SyncTransforms(); }
        string report="PASS: C05 cleaner/store, C07 accessible washroom and C08 IT/services are furnished; C06 has a static closed landing-door facade only.\n"+
            "PASS: the full 1.525m C07 turning square, the 0.76 x 1.22m side-transfer strip and a 0.76m-wide lavatory knee space up to 0.685m above the finish remain clear of physical obstacles.\n"+
            "PASS: service finishes are only 1mm above the structural corridor floor; downward raycasts across the C07 doorway confirm no raised tile step.\n"+
            "PASS: actual editor-mode CharacterController routes reach storage, the washroom turning point, WC transfer side, washbasin, network rack and lift approach, then return to the corridors.\n"+
            "PASS: "+doors+" original walking doorways, both 2.40m main corridors and both courtyard-side links remain unobstructed; the reserved shaft is behind a deliberately closed lift entrance.\n"+
            "PASS: furnishing colliders remain within their service rooms, with valid persistent mesh/material references.\n"+
            "Scope: 3D design draft and editor-mode geometry/collision checks, not an accessibility certification; no Play-mode run or functional lift simulation performed.";
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/Verification.txt",report);return report;
    }
    static void ClearBox(Vector3 center,Vector3 half,string label)
    {
        foreach(var hit in Physics.OverlapBox(center,half)) if(!(hit is CharacterController)) throw new InvalidOperationException(label+" blocked by "+hit.name);
    }
    static void Walk(CharacterController c,Vector2[] points)
    {
        c.enabled=false;c.transform.position=new Vector3(points[0].x,.15f,points[0].y);c.enabled=true;Physics.SyncTransforms();
        for(int i=0;i<12;i++) c.Move(Vector3.down*.025f);
        for(int n=1;n<points.Length;n++) {
            bool reached=false;
            for(int i=0;i<800;i++) {
                var p=c.transform.position;var delta=points[n]-new Vector2(p.x,p.z);
                if(delta.magnitude<.035f) { reached=true;break; }
                var step=Vector2.ClampMagnitude(delta,.035f);c.Move(new Vector3(step.x,-.035f,step.y));
            }
            if(!reached) throw new InvalidOperationException("Service route blocked near "+c.transform.position+" toward "+points[n]);
        }
    }

    [MenuItem("Tools/JEI/Service Areas/Focus All Service Areas")]
    public static void Focus() { Look(new Vector3(9.5f,.9f,0),Quaternion.Euler(58,-15,0),5.4f); }
    [MenuItem("Tools/JEI/Service Areas/Focus C05 Cleaner Store")]
    public static void FocusCleaner() { Look(new Vector3(8,.8f,1.5f),Quaternion.Euler(47,165,0),2.6f); }
    [MenuItem("Tools/JEI/Service Areas/Focus C07 Accessible WC")]
    public static void FocusWashroom() { Look(new Vector3(8,.8f,-1.5f),Quaternion.Euler(47,-15,0),2.6f); }
    [MenuItem("Tools/JEI/Service Areas/Focus C08 IT Services")]
    public static void FocusIT() { Look(new Vector3(10.9f,.9f,-1.5f),Quaternion.Euler(47,-15,0),2.6f); }
    [MenuItem("Tools/JEI/Service Areas/Focus C06 Lift Entrance")]
    public static void FocusLift() { Look(new Vector3(10.9f,1.25f,3.25f),Quaternion.Euler(8,180,0),2.2f); }
    static void Look(Vector3 center,Quaternion rotation,float size)
    {
        RequireFloor();Selection.activeGameObject=null;var view=SceneView.lastActiveSceneView;
        if(!view) return;view.in2DMode=false;view.sceneLighting=true;view.LookAt(center,rotation,size,false,true);view.Focus();view.Repaint();
    }

    [MenuItem("Tools/JEI/Service Areas/Capture Service Area Previews")]
    public static void Capture()
    {
        RequireFloor();Directory.CreateDirectory(Output);var obj=new GameObject("Temporary service-area review camera");var camera=obj.AddComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.86f,.89f,.90f);camera.nearClipPlane=.04f;camera.farClipPlane=150;
        try {
            Shot(camera,"Documentation/Floor01_Furnishings/01_Overview.png",new Vector3(24,32,-30),new Vector3(0,.7f,0),46);
            Shot(camera,Output+"/01_Service_Areas_3D.png",new Vector3(15,10,-7),new Vector3(9.5f,.6f,0),58);
            Shot(camera,Output+"/02_C05_Cleaner_Store.png",new Vector3(8,2.10f,3.04f),new Vector3(8,.95f,1.15f),83);
            Shot(camera,Output+"/03_C07_Accessible_WC.png",new Vector3(8,2.10f,-3.04f),new Vector3(8.05f,.90f,-.70f),84);
            Shot(camera,Output+"/04_C08_IT_Services.png",new Vector3(10.9f,2.05f,-3.04f),new Vector3(11.05f,1.0f,-.60f),82);
            Shot(camera,Output+"/05_C06_Lift_Entrance.png",new Vector3(10.9f,1.50f,5.60f),new Vector3(10.9f,1.35f,3.38f),70);
            Shot(camera,Output+"/06_C08_Network_Rack.png",new Vector3(10.35f,1.45f,-1.55f),new Vector3(11.72f,1.05f,-.72f),72);
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
