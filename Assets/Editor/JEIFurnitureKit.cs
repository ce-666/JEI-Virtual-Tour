using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Editor-only metre-scaled furniture, shared by the first-floor furnishing builders.</summary>
public static class JEIFurnitureKit
{
    const string Root = "Assets/JEI_Furnishings";
    const string Materials = Root + "/Materials";
    const string Meshes = Root + "/Meshes";
    static readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();
    static Font labelFont;

    public static Transform Group(Transform parent, string name, Vector3 position = default(Vector3), float yaw = 0)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        g.transform.localPosition = position; g.transform.localRotation = Quaternion.Euler(0, yaw, 0); return g.transform;
    }

    static void Folders()
    {
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "JEI_Furnishings");
        if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Root, "Materials");
        if (!AssetDatabase.IsValidFolder(Meshes)) AssetDatabase.CreateFolder(Root, "Meshes");
    }

    public static Material Mat(string name, Color color, float smoothness = .25f)
    {
        Material m;
        if (materialCache.TryGetValue(name, out m) && m) return m;
        Folders(); string path = Materials + "/" + name + ".mat";
        m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { var shader = Shader.Find("Universal Render Pipeline/Lit"); if (!shader) shader = Shader.Find("Standard"); m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(m); materialCache[name] = m; return m;
    }

    public static Material MatGlass(string name = "Clear_Glass", float opacity = .16f)
    { return MatGlass(name, new Color(.61f, .82f, .84f, opacity)); }

    public static Material MatGlass(string name, Color color)
    {
        var m = Mat(name, color, .88f);
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1);
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0);
        if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0);
        if (m.HasProperty("_Cull")) m.SetFloat("_Cull", (float)CullMode.Off);
        m.SetOverrideTag("RenderType", "Transparent"); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent; EditorUtility.SetDirty(m); return m;
    }

    static Material Oak { get { return Mat("Light_Oak", new Color(.73f, .56f, .35f)); } }
    static Material White { get { return Mat("Warm_White", new Color(.91f, .90f, .84f)); } }
    static Material Metal { get { return Mat("Graphite_Metal", new Color(.16f, .20f, .23f), .42f); } }
    static Material Fabric(string color)
    {
        Color c = new Color(.15f, .40f, .43f);
        switch (color.ToLowerInvariant()) { case "blue": case "navy": c = new Color(.15f,.25f,.38f); break; case "yellow": case "mustard": c = new Color(.84f,.62f,.20f); break; case "grey": case "gray": c = new Color(.43f,.45f,.46f); break; case "sand": c = new Color(.69f,.61f,.48f); break; }
        return Mat("Fabric_" + color, c, .08f);
    }

    public static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool solid = true)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localScale = size; g.GetComponent<Renderer>().sharedMaterial = mat;
        if (!solid) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
    }

    public static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 size, Material mat)
    {
        var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localScale = size; g.GetComponent<Renderer>().sharedMaterial = mat;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
    }

    static GameObject MeshObject(Transform parent, string name, Vector3 pos, Mesh mesh, Material material)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.localPosition = pos;
        g.AddComponent<MeshFilter>().sharedMesh = mesh; g.AddComponent<MeshRenderer>().sharedMaterial = material; return g;
    }

    static string Key(float n) { return n.ToString("0.####", CultureInfo.InvariantCulture).Replace("-", "m").Replace(".", "p"); }
    static Mesh Cached(string key, Func<Mesh> build)
    {
        Mesh m; if (meshCache.TryGetValue(key, out m) && m) return m;
        Folders(); string path = Meshes + "/" + key + ".asset"; m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (!m) { m = build(); m.name = key; m.RecalculateBounds(); AssetDatabase.CreateAsset(m, path); }
        meshCache[key] = m; return m;
    }

    // Rounded rectangle extrusion with a small bevel on top and bottom; persisted as a reusable mesh.
    static Mesh RoundedMesh(Vector3 size, float radius)
    {
        radius = Mathf.Min(radius, Mathf.Min(size.x, size.z) * .45f);
        string key = "Rounded_" + Key(size.x) + "_" + Key(size.y) + "_" + Key(size.z) + "_" + Key(radius);
        return Cached(key, () => {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            const int steps = 5; int perimeter = 4 * (steps + 1); float bevel = Mathf.Min(.012f, size.y * .2f);
            for (int layer = 0; layer < 4; layer++) {
                float inset = layer == 0 || layer == 3 ? bevel : 0;
                float y = layer == 0 ? -size.y/2 : layer == 1 ? -size.y/2+bevel : layer == 2 ? size.y/2-bevel : size.y/2;
                float r = Mathf.Max(.001f, radius - inset);
                for (int corner = 0; corner < 4; corner++) {
                    float cx = (corner == 0 || corner == 3 ? 1 : -1) * (size.x/2-radius);
                    float cz = (corner < 2 ? 1 : -1) * (size.z/2-radius);
                    for (int s = 0; s <= steps; s++) { float a = (corner * 90f + s * 90f/steps) * Mathf.Deg2Rad; vertices.Add(new Vector3(cx+Mathf.Cos(a)*r,y,cz+Mathf.Sin(a)*r)); }
                }
            }
            for (int layer = 0; layer < 3; layer++) for (int j = 0; j < perimeter; j++) { int next = (j+1)%perimeter, a=layer*perimeter+j,b=layer*perimeter+next,c=(layer+1)*perimeter+j,d=(layer+1)*perimeter+next; triangles.AddRange(new[]{a,c,b,b,c,d}); }
            int bottom = vertices.Count; vertices.Add(new Vector3(0,-size.y/2,0)); int top = vertices.Count; vertices.Add(new Vector3(0,size.y/2,0));
            for (int j=0;j<perimeter;j++) { int n=(j+1)%perimeter; triangles.AddRange(new[]{bottom,j,n,top,3*perimeter+n,3*perimeter+j}); }
            var mesh=new Mesh(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); return mesh;
        });
    }

    public static GameObject Rounded(Transform p,string name,Vector3 pos,Vector3 size,Material m,float radius=.07f)
    { return MeshObject(p,name,pos,RoundedMesh(size,radius),m); }
    static void Collider(Transform t,Vector3 center,Vector3 size) { var c=t.gameObject.AddComponent<BoxCollider>(); c.center=center;c.size=size; }
    static void Legs(Transform p,float width,float depth,float height)
    {
        foreach(float x in new[]{-width/2+.10f,width/2-.10f}) foreach(float z in new[]{-depth/2+.09f,depth/2-.09f}) {
            var leg=Box(p,"Slim metal leg",new Vector3(x,height/2,z),new Vector3(.045f,height,.045f),Metal,false);
            leg.transform.localRotation=Quaternion.Euler(z>0?3:-3,0,x>0?-3:3);
        }
    }

    public static Transform Chair(Transform parent, Vector3 pos, float yaw, bool office = false, string color = "Teal")
    {
        var g=Group(parent,office?"Adjustable task chair":"Upholstered seminar chair",pos,yaw); var fabric=Fabric(color);
        Rounded(g,"Seat cushion",new Vector3(0,.46f,0),new Vector3(.47f,.09f,.47f),fabric,.09f);
        var back=MeshObject(g,"Curved upholstered back",new Vector3(0,.54f,.02f),RingMesh(.285f,.245f,200,340,.38f),fabric);
        if(office) {
            Primitive(g,"Height adjustment column",PrimitiveType.Cylinder,new Vector3(0,.245f,0),new Vector3(.065f,.20f,.065f),Metal);
            for(int i=0;i<5;i++) { float a=i*72*Mathf.Deg2Rad; var foot=Box(g,"Five star base spoke",new Vector3(Mathf.Sin(a)*.12f,.09f,Mathf.Cos(a)*.12f),new Vector3(.035f,.035f,.27f),Metal,false); foot.transform.localRotation=Quaternion.Euler(0,i*72,0); Primitive(g,"Caster",PrimitiveType.Sphere,new Vector3(Mathf.Sin(a)*.24f,.052f,Mathf.Cos(a)*.24f),Vector3.one*.075f,Metal); }
            foreach(float x in new[]{-.275f,.275f}) { Box(g,"Arm support",new Vector3(x,.55f,0),new Vector3(.025f,.18f,.025f),Metal,false); Rounded(g,"Arm rest",new Vector3(x,.645f,.015f),new Vector3(.055f,.04f,.28f),Metal,.023f); }
        } else Legs(g,.46f,.46f,.415f);
        Collider(g,new Vector3(0,.47f,0),new Vector3(.54f,.90f,.54f)); return g;
    }

    public static Transform Table(Transform parent, Vector3 pos, float width, float depth, float yaw = 0)
    {
        var g=Group(parent,"Oak meeting table",pos,yaw);
        Rounded(g,"Soft corner oak tabletop",new Vector3(0,.735f,0),new Vector3(width,.055f,depth),Oak,.10f);
        Box(g,"Under tabletop frame",new Vector3(0,.67f,0),new Vector3(Mathf.Max(.15f,width-.22f),.07f,Mathf.Max(.15f,depth-.20f)),Metal,false);
        Legs(g,width,depth,.70f); Collider(g,new Vector3(0,.39f,0),new Vector3(width,.76f,depth)); return g;
    }

    public static Transform Desk(Transform parent, Vector3 pos, float yaw = 0, float width = 1.4f, float depth = .7f, bool computer = true)
    {
        var g=Table(parent,pos,width,depth,yaw); g.name="Oak workstation";
        Box(g,"Cable tray",new Vector3(0,.625f,depth*.32f),new Vector3(width*.65f,.08f,.09f),Metal,false);
        if(computer) Monitor(g,new Vector3(0,.765f,depth*.22f));
        Rounded(g,"Writing pad",new Vector3(-width*.31f,.771f,-depth*.12f),new Vector3(.25f,.012f,.30f),Mat("Paper_Cream",new Color(.91f,.88f,.74f)),.015f);
        Box(g,"Notebook binding",new Vector3(-width*.31f-.115f,.781f,-depth*.12f),new Vector3(.018f,.016f,.30f),Fabric("Navy"),false);
        return g;
    }

    public static void Monitor(Transform parent, Vector3 pos, float yaw = 0)
    {
        var g=Group(parent,"Desktop computer",pos,yaw);
        Rounded(g,"Monitor foot",new Vector3(0,.015f,0),new Vector3(.24f,.025f,.16f),Metal,.035f);
        Box(g,"Monitor pedestal",new Vector3(0,.13f,.025f),new Vector3(.045f,.22f,.045f),Metal,false);
        Rounded(g,"Thin monitor bezel",new Vector3(0,.325f,.025f),new Vector3(.56f,.34f,.035f),Metal,.022f);
        var screen=Mat("Display_Blue",new Color(.21f,.43f,.57f),.4f);
        Box(g,"Display glass",new Vector3(0,.325f,.004f),new Vector3(.515f,.292f,.008f),screen,false);
        Box(g,"Display header",new Vector3(0,.43f,-.001f),new Vector3(.47f,.025f,.002f),White,false);
        Box(g,"Display window",new Vector3(-.10f,.325f,-.002f),new Vector3(.22f,.115f,.002f),Mat("Display_Light",new Color(.69f,.81f,.83f)),false);
        Rounded(g,"Keyboard",new Vector3(0,.018f,-.20f),new Vector3(.35f,.026f,.12f),Metal,.015f);
        for(int i=0;i<3;i++) Box(g,"Keyboard key row",new Vector3(0,.033f,-.238f+i*.035f),new Vector3(.31f,.003f,.018f),Mat("Keyboard_Keys",new Color(.40f,.43f,.45f)),false);
        Primitive(g,"Mouse",PrimitiveType.Sphere,new Vector3(.265f,.025f,-.19f),new Vector3(.065f,.04f,.095f),Metal);
    }

    public static Transform Sofa(Transform parent, Vector3 pos, float yaw = 0, float width = 1.9f, string color = "Teal")
    {
        var g=Group(parent,"Light framed lounge sofa",pos,yaw); var fabric=Fabric(color);
        Rounded(g,"Sofa base",new Vector3(0,.29f,0),new Vector3(width,.24f,.76f),fabric,.12f);
        Rounded(g,"Upholstered back",new Vector3(0,.68f,.29f),new Vector3(width-.07f,.62f,.20f),fabric,.07f);
        foreach(float x in new[]{-width/2+.09f,width/2-.09f}) Rounded(g,"Soft arm",new Vector3(x,.49f,0),new Vector3(.17f,.46f,.75f),fabric,.075f);
        int seats=width>1.65f?3:2; float cushionWidth=(width-.38f)/seats;
        for(int i=0;i<seats;i++) { float x=-(width-.38f)/2+cushionWidth*(i+.5f); Rounded(g,"Separate seat cushion",new Vector3(x,.445f,-.055f),new Vector3(cushionWidth-.018f,.15f,.58f),fabric,.065f); Rounded(g,"Separate back cushion",new Vector3(x,.705f,.17f),new Vector3(cushionWidth-.023f,.42f,.15f),fabric,.055f); }
        Legs(g,width-.08f,.65f,.18f); Collider(g,new Vector3(0,.48f,.01f),new Vector3(width,.96f,.77f)); return g;
    }

    public static Transform Cabinet(Transform parent, Vector3 pos, float width = 1.2f, float height = 1.8f, float yaw = 0, bool open = false)
    {
        var g=Group(parent,open?"Open resource bookcase":"Closed document cabinet",pos,yaw); const float depth=.42f;
        Box(g,"Recessed plinth",new Vector3(0,.045f,0),new Vector3(width-.08f,.09f,depth-.05f),Metal,false);
        Box(g,"Cabinet back",new Vector3(0,height/2,.19f),new Vector3(width,height,.035f),Oak,false);
        foreach(float x in new[]{-width/2+.018f,width/2-.018f}) Box(g,"Cabinet side",new Vector3(x,height/2,0),new Vector3(.036f,height,depth),White,false);
        int shelves=Mathf.Max(2,Mathf.RoundToInt(height/.36f));
        for(int i=0;i<=shelves;i++) { float y=.10f+(height-.12f)*i/shelves; Box(g,"Oak shelf",new Vector3(0,y,0),new Vector3(width-.06f,.035f,depth),Oak,false);
            if(open && i<shelves) for(int b=0;b<Mathf.Min(7,Mathf.FloorToInt(width/.14f));b++) { float bh=.19f+(b%3)*.035f; Box(g,"Bound reference volume",new Vector3(-width/2+.14f+b*.135f,y+.02f+bh/2,.015f),new Vector3(.075f,bh,.25f),Fabric(b%3==0?"Navy":b%3==1?"Sand":"Teal"),false); }
        }
        if(!open) for(int i=0;i<2;i++) { float x=(i==0?-1:1)*width*.25f;
            Rounded(g,"Closed cabinet door",new Vector3(x,height/2+.04f,-.222f),new Vector3(width/2-.012f,height-.11f,.028f),White,.008f);
            float handleX=(i==0?-.05f:.05f); Box(g,"Brushed metal handle",new Vector3(handleX,height*.56f,-.25f),new Vector3(.018f,.16f,.025f),Metal,false);
        }
        Collider(g,new Vector3(0,height/2,0),new Vector3(width,height,depth+.06f)); return g;
    }

    public static Transform Plant(Transform parent, Vector3 pos, float scale = 1)
    {
        var g=Group(parent,"Architectural indoor plant",pos); g.localScale=Vector3.one*scale;
        Primitive(g,"Ceramic planter",PrimitiveType.Cylinder,new Vector3(0,.19f,0),new Vector3(.34f,.19f,.34f),Mat("Planter_Stone",new Color(.69f,.69f,.64f)));
        Primitive(g,"Soil",PrimitiveType.Cylinder,new Vector3(0,.376f,0),new Vector3(.30f,.008f,.30f),Mat("Potting_Soil",new Color(.18f,.14f,.10f)));
        var green=Mat("Plant_Leaf",new Color(.23f,.41f,.25f),.15f);
        for(int i=0;i<7;i++) { float a=i*137.5f*Mathf.Deg2Rad; var leaf=Primitive(g,"Upright leaf",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.14f,.68f+(i%3)*.10f,Mathf.Sin(a)*.14f),new Vector3(.105f,.65f+(i%3)*.10f,.07f),green); leaf.transform.localRotation=Quaternion.Euler(Mathf.Cos(a)*20,i*137.5f,Mathf.Sin(a)*20); }
        Collider(g,new Vector3(0,.20f,0),new Vector3(.35f,.40f,.35f)); return g;
    }

    public static void WallText(Transform parent, string text, Vector3 pos, float yaw, float width, Color color)
    {
        var g=Group(parent,"Sign - "+text.Replace('\n',' '),pos,yaw); var t=g.gameObject.AddComponent<TextMesh>();
        if(!labelFont) labelFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.font=labelFont; t.text=text; t.fontSize=64; t.characterSize=.02f; t.anchor=TextAnchor.MiddleCenter; t.alignment=TextAlignment.Center; t.color=color;
        var renderer=g.GetComponent<MeshRenderer>();
        Folders(); string path=Materials+"/Depth_Tested_Font.mat"; var fontMaterial=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!fontMaterial) { var shader=Shader.Find("JEI/Depth Tested Text"); fontMaterial=new Material(shader?shader:Shader.Find("GUI/Text Shader")); fontMaterial.mainTexture=labelFont.material.mainTexture; AssetDatabase.CreateAsset(fontMaterial,path); }
        renderer.sharedMaterial=fontMaterial; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
        float actual=renderer.localBounds.size.x; if(actual>.001f) g.localScale=Vector3.one*(width/actual);
    }

    public static Transform RoundTable(Transform parent, Vector3 pos, float radius = .6f)
    {
        var g=Group(parent,"Round occasional table",pos);
        Primitive(g,"Rounded circular oak tabletop",PrimitiveType.Cylinder,new Vector3(0,.735f,0),new Vector3(radius*2,.0275f,radius*2),Oak);
        Primitive(g,"Pedestal",PrimitiveType.Cylinder,new Vector3(0,.36f,0),new Vector3(.075f,.35f,.075f),Metal);
        Primitive(g,"Pedestal foot",PrimitiveType.Cylinder,new Vector3(0,.035f,0),new Vector3(radius*.85f,.025f,radius*.85f),Metal);
        Collider(g,new Vector3(0,.38f,0),new Vector3(radius*2,.76f,radius*2)); return g;
    }

    public static Mesh RingMesh(float outer,float inner,float start,float end,float thickness)
    {
        string key="Arc_v2_"+Key(outer)+"_"+Key(inner)+"_"+Key(start)+"_"+Key(end)+"_"+Key(thickness);
        return Cached(key,()=>{
            int steps=Mathf.Max(6,Mathf.CeilToInt(Mathf.Abs(end-start)/5)); var v=new List<Vector3>(); var tri=new List<int>();
            for(int i=0;i<=steps;i++) { float a=Mathf.Lerp(start,end,(float)i/steps)*Mathf.Deg2Rad; float x=Mathf.Cos(a),z=Mathf.Sin(a); v.Add(new Vector3(x*inner,0,z*inner));v.Add(new Vector3(x*outer,0,z*outer));v.Add(new Vector3(x*inner,thickness,z*inner));v.Add(new Vector3(x*outer,thickness,z*outer)); }
            for(int i=0;i<steps;i++) {int a=i*4,b=(i+1)*4;tri.AddRange(new[]{a,b,a+1,a+1,b,b+1,a+2,a+3,b+2,a+3,b+3,b+2,a,a+2,b,a+2,b+2,b,a+1,b+1,a+3,a+3,b+1,b+3});}
            int e=steps*4;tri.AddRange(new[]{0,1,2,1,3,2,e,e+2,e+1,e+1,e+2,e+3});
            for(int i=0;i<tri.Count;i+=3) { int swap=tri[i+1];tri[i+1]=tri[i+2];tri[i+2]=swap; }
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();return mesh;
        });
    }

    public static Transform ArcTable(Transform parent, Vector3 pos, float startDegrees, float endDegrees, float outerRadius = .85f, float innerRadius = .30f, float height = .75f)
    {
        var g=Group(parent,"Modular curved collaboration desk",pos); const float thick=.05f;
        var top=MeshObject(g,"True annular sector tabletop",new Vector3(0,height-thick,0),RingMesh(outerRadius,innerRadius,startDegrees,endDegrees,thick),White);
        top.AddComponent<MeshCollider>().sharedMesh=top.GetComponent<MeshFilter>().sharedMesh;
        foreach(float a0 in new[]{Mathf.Lerp(startDegrees,endDegrees,.14f),Mathf.Lerp(startDegrees,endDegrees,.86f)}) foreach(float r in new[]{Mathf.Lerp(innerRadius,outerRadius,.20f),Mathf.Lerp(innerRadius,outerRadius,.80f)}) { float a=a0*Mathf.Deg2Rad; Box(g,"Light oak table leg",new Vector3(Mathf.Cos(a)*r,(height-thick)/2,Mathf.Sin(a)*r),new Vector3(.045f,height-thick,.045f),Oak,false); }
        return g;
    }
}
