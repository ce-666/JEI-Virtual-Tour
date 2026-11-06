using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Softer residential-style faculty furniture. All dimensions are local metres;
/// desk/cabinet fronts face -Z, and seated people face +Z before applying yaw.
/// </summary>
public static class JEIFacultyFurniture
{
    const string MeshFolder = "Assets/JEI_Furnishings/Meshes";
    static Material Wood { get { return JEIFurnitureKit.Mat("S01_Faculty_Honey_Oak",new Color(.72f,.54f,.34f),.23f); } }
    static Material Cream { get { return JEIFurnitureKit.Mat("S01_Faculty_Cream_Lacquer",new Color(.87f,.85f,.76f),.22f); } }
    static Material Paper { get { return JEIFurnitureKit.Mat("S01_Faculty_Book_Pages",new Color(.94f,.91f,.80f),.06f); } }
    static Material Sage { get { return JEIFurnitureKit.Mat("S01_Faculty_Sage_Fabric",new Color(.45f,.56f,.46f),.06f); } }
    static Material Oatmeal { get { return JEIFurnitureKit.Mat("S01_Faculty_Oatmeal_Fabric",new Color(.72f,.67f,.56f),.06f); } }
    static Material Hardware { get { return JEIFurnitureKit.Mat("S01_Faculty_Warm_Hardware",new Color(.48f,.44f,.35f),.38f); } }
    static Material BookBlue { get { return JEIFurnitureKit.Mat("S01_Faculty_Book_Dusty_Blue",new Color(.32f,.43f,.49f),.08f); } }

    static GameObject R(Transform p,string n,Vector3 pos,Vector3 size,Material mat,float radius=.07f)
    { return JEIFurnitureKit.Rounded(p,n,pos,size,mat,radius); }
    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material mat,bool solid=false)
    { return JEIFurnitureKit.Box(p,n,pos,size,mat,solid); }
    static GameObject P(Transform p,string n,PrimitiveType type,Vector3 pos,Vector3 size,Material mat)
    { return JEIFurnitureKit.Primitive(p,n,type,pos,size,mat); }
    static void BoundsCollider(Transform t,Vector3 centre,Vector3 size)
    { var c=t.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    static void TimberLeg(Transform p,float x,float z,float height,bool solid)
    {
        var leg=P(p,"Splayed solid oak leg",PrimitiveType.Cylinder,new Vector3(x,height/2,z),new Vector3(.065f,height/2,.065f),Wood);
        leg.transform.localRotation=Quaternion.Euler(z>0?-4:4,0,x>0?4:-4);
        if(solid) { var c=leg.AddComponent<BoxCollider>(); c.size=new Vector3(1,2,1); }
    }

    public static Transform WritingDesk(Transform p,Vector3 pos,float yaw,float width,float depth)
    {
        var g=JEIFurnitureKit.Group(p,"Residential faculty writing desk",pos,yaw);
        R(g,"Thick rounded honey oak desktop",new Vector3(0,.728f,0),new Vector3(width,.062f,depth),Wood,.15f);
        BoundsCollider(g,new Vector3(0,.728f,0),new Vector3(width,.062f,depth));
        foreach(float x in new[]{-width/2+.13f,width/2-.13f})
            foreach(float z in new[]{-depth/2+.12f,depth/2-.12f}) TimberLeg(g,x,z,.697f,true);
        B(g,"Rear timber desk apron",new Vector3(0,.655f,depth/2-.085f),new Vector3(width-.19f,.09f,.035f),Wood);
        float drawerWidth=Mathf.Min(.43f,width*.27f);float drawerX=-width/2+drawerWidth/2+.10f;
        R(g,"Cream suspended stationery drawer",new Vector3(drawerX,.607f,-.015f),new Vector3(drawerWidth,.18f,depth-.16f),Cream,.025f);
        B(g,"Recessed finger pull",new Vector3(drawerX,.643f,-depth/2+.057f),new Vector3(drawerWidth*.58f,.014f,.006f),Hardware);
        BoundsCollider(g,new Vector3(drawerX,.607f,-.015f),new Vector3(drawerWidth,.18f,depth-.16f));
        // The centre/right kneehole stays open from the floor to the desktop.
        Laptop(g,new Vector3(.10f,.764f,.025f));
        Book(g,new Vector3(-width*.29f,.766f,-depth*.12f),new Vector3(.235f,.035f,.29f),BookBlue);
        B(g,"Loose writing sheet",new Vector3(width*.29f,.764f,-depth*.18f),new Vector3(.21f,.004f,.28f),Paper);
        var pencil=B(g,"Wooden pencil",new Vector3(width*.29f+.025f,.772f,-depth*.18f),new Vector3(.009f,.009f,.16f),Wood);
        pencil.transform.localRotation=Quaternion.Euler(0,-18,0);
        if(width>=1.55f) {
            var lamp=JEIFurnitureKit.Group(g,"Small ceramic reading lamp",new Vector3(width/2-.20f,.762f,depth/2-.18f));
            P(lamp,"Stone lamp foot",PrimitiveType.Cylinder,new Vector3(0,.014f,0),new Vector3(.14f,.014f,.14f),Cream);
            P(lamp,"Slender brass stem",PrimitiveType.Cylinder,new Vector3(0,.15f,0),new Vector3(.013f,.14f,.013f),Hardware);
            P(lamp,"Soft rounded ceramic shade",PrimitiveType.Sphere,new Vector3(0,.29f,0),new Vector3(.23f,.13f,.23f),Cream);
        }
        return g;
    }

    static void Laptop(Transform p,Vector3 pos)
    {
        var g=JEIFurnitureKit.Group(p,"Compact light coloured laptop",pos);
        var shell=JEIFurnitureKit.Mat("S01_Faculty_Laptop_Silver",new Color(.72f,.75f,.72f),.42f);
        var keys=JEIFurnitureKit.Mat("S01_Faculty_Laptop_Keys",new Color(.49f,.53f,.50f),.15f);
        var display=JEIFurnitureKit.Mat("S01_Faculty_Laptop_Paper_Screen",new Color(.79f,.86f,.80f),.25f);
        R(g,"Rounded laptop base",new Vector3(0,.012f,0),new Vector3(.38f,.022f,.255f),shell,.015f);
        for(int row=0;row<3;row++) B(g,"Keyboard strip",new Vector3(0,.025f,.013f+row*.025f),new Vector3(.29f,.003f,.013f),keys);
        R(g,"Touch pad",new Vector3(0,.026f,-.074f),new Vector3(.105f,.003f,.052f),keys,.006f);
        var lid=JEIFurnitureKit.Group(g,"Gently reclined display",new Vector3(0,.024f,.117f));
        lid.localRotation=Quaternion.Euler(12,0,0);
        R(lid,"Light monitor lid",new Vector3(0,.122f,0),new Vector3(.38f,.244f,.014f),shell,.014f);
        B(lid,"Soft light document screen",new Vector3(0,.122f,-.009f),new Vector3(.345f,.209f,.003f),display);
        B(lid,"Document window",new Vector3(.025f,.12f,-.011f),new Vector3(.20f,.15f,.002f),Paper);
        for(int line=0;line<4;line++) B(lid,"Document text line",new Vector3(.025f,.165f-line*.025f,-.013f),new Vector3(.15f,.005f,.001f),keys);
    }

    static void Book(Transform p,Vector3 bottom,Vector3 size,Material cover)
    {
        R(p,"Lower cloth book cover",bottom+Vector3.up*.003f,new Vector3(size.x,.006f,size.z),cover,.007f);
        R(p,"Upper cloth book cover",bottom+Vector3.up*(size.y-.003f),new Vector3(size.x,.006f,size.z),cover,.007f);
        B(p,"Visible cream page block",bottom+new Vector3(.004f,size.y/2,0),new Vector3(size.x-.012f,size.y-.012f,size.z-.013f),Paper);
        B(p,"Cloth book spine",bottom+new Vector3(-size.x/2+.005f,size.y/2,0),new Vector3(.010f,size.y,size.z),cover);
    }

    public static Transform StudyChair(Transform p,Vector3 pos,float yaw,bool task,string color)
    {
        var g=JEIFurnitureKit.Group(p,task?"Soft faculty task chair":"Timber and fabric discussion chair",pos,yaw);
        bool oatmeal=color!=null && (color.ToLowerInvariant()=="oatmeal" || color.ToLowerInvariant()=="sand" || color.ToLowerInvariant()=="cream");
        Material fabric=oatmeal?Oatmeal:Sage;
        R(g,"Sculpted oak seat support",new Vector3(0,.42f,0),new Vector3(.49f,.055f,.47f),Wood,.10f);
        R(g,"Soft linen seat pad",new Vector3(0,.472f,-.012f),new Vector3(.49f,.09f,.47f),fabric,.11f);
        Curved(g,"Low rounded upholstered back",new Vector3(0,.55f,.015f),.30f,.246f,200,340,.34f,fabric);
        Curved(g,"Oak curved lower back rail",new Vector3(0,.536f,.015f),.306f,.244f,200,340,.045f,Wood);
        foreach(float x in new[]{-.20f,.20f}) B(g,"Back support in warm oak",new Vector3(x,.505f,-.16f),new Vector3(.028f,.19f,.03f),Wood);
        if(task) {
            P(g,"Light adjustable chair stem",PrimitiveType.Cylinder,new Vector3(0,.245f,0),new Vector3(.065f,.16f,.065f),Cream);
            for(int i=0;i<5;i++) {
                float a=i*72*Mathf.Deg2Rad;
                var spoke=R(g,"Cream five star foot",new Vector3(Mathf.Sin(a)*.13f,.095f,Mathf.Cos(a)*.13f),new Vector3(.045f,.035f,.28f),Cream,.018f);
                spoke.transform.localRotation=Quaternion.Euler(0,i*72,0);
                P(g,"Quiet warm grey caster",PrimitiveType.Sphere,new Vector3(Mathf.Sin(a)*.26f,.052f,Mathf.Cos(a)*.26f),new Vector3(.07f,.07f,.07f),Hardware);
            }
            BoundsCollider(g,new Vector3(0,.455f,0),new Vector3(.62f,.91f,.62f));
        } else {
            foreach(float x in new[]{-.18f,.18f}) foreach(float z in new[]{-.17f,.17f}) TimberLeg(g,x,z,.405f,false);
            BoundsCollider(g,new Vector3(0,.455f,0),new Vector3(.56f,.91f,.58f));
        }
        return g;
    }

    static GameObject Curved(Transform p,string name,Vector3 pos,float outer,float inner,float start,float end,float height,Material mat)
    {
        var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.localPosition=pos;
        g.AddComponent<MeshFilter>().sharedMesh=JEIFurnitureKit.RingMesh(outer,inner,start,end,height);
        g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
    }

    public static Transform SocialTable(Transform p,Vector3 pos,float yaw,float width,float depth)
    {
        var g=JEIFurnitureKit.Group(p,"Informal oval oak discussion table",pos,yaw);
        var top=new GameObject("True elliptical bevelled oak tabletop");top.transform.SetParent(g,false);top.transform.localPosition=new Vector3(0,.73f,0);
        var mesh=EllipseMesh(width,depth,.06f);
        top.AddComponent<MeshFilter>().sharedMesh=mesh;top.AddComponent<MeshRenderer>().sharedMaterial=Wood;top.AddComponent<MeshCollider>().sharedMesh=mesh;
        foreach(float x in new[]{-width*.28f,width*.28f}) foreach(float z in new[]{-depth*.23f,depth*.23f}) TimberLeg(g,x,z,.70f,true);
        B(g,"Oak table stretcher",new Vector3(0,.60f,0),new Vector3(width*.56f,.07f,.045f),Wood);
        Book(g,new Vector3(-width*.20f,.763f,0),new Vector3(.21f,.025f,.27f),Oatmeal);
        P(g,"Small unlit ceramic table vase",PrimitiveType.Cylinder,new Vector3(width*.18f,.805f,0),new Vector3(.09f,.045f,.09f),Cream);
        return g;
    }

    static Mesh EllipseMesh(float width,float depth,float thickness)
    {
        string key="FacultyOval_v1_"+width.ToString("0.###",CultureInfo.InvariantCulture).Replace('.','p')+"_"+depth.ToString("0.###",CultureInfo.InvariantCulture).Replace('.','p')+"_"+thickness.ToString("0.###",CultureInfo.InvariantCulture).Replace('.','p');
        if(!AssetDatabase.IsValidFolder("Assets/JEI_Furnishings")) AssetDatabase.CreateFolder("Assets","JEI_Furnishings");
        if(!AssetDatabase.IsValidFolder(MeshFolder)) AssetDatabase.CreateFolder("Assets/JEI_Furnishings","Meshes");
        string path=MeshFolder+"/"+key+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
        const int count=64;var v=new List<Vector3>();var tri=new List<int>();float bevel=Mathf.Min(.012f,thickness*.20f);
        for(int ring=0;ring<4;ring++) {
            float inset=(ring==0||ring==3)?bevel:0;
            float y=ring==0?-thickness/2:ring==1?-thickness/2+bevel:ring==2?thickness/2-bevel:thickness/2;
            for(int j=0;j<count;j++) { float a=j*Mathf.PI*2/count;v.Add(new Vector3(Mathf.Cos(a)*(width/2-inset),y,Mathf.Sin(a)*(depth/2-inset))); }
        }
        for(int ring=0;ring<3;ring++) for(int j=0;j<count;j++) { int n=(j+1)%count,a=ring*count+j,b=ring*count+n,c=(ring+1)*count+j,d=(ring+1)*count+n;tri.AddRange(new[]{a,c,b,b,c,d}); }
        int bottom=v.Count;v.Add(new Vector3(0,-thickness/2,0));int upper=v.Count;v.Add(new Vector3(0,thickness/2,0));
        for(int j=0;j<count;j++) {int n=(j+1)%count;tri.AddRange(new[]{bottom,j,n,upper,3*count+n,3*count+j});}
        var mesh=new Mesh();mesh.name=key;mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }

    public static Transform StorageCabinet(Transform p,Vector3 pos,float yaw,float width,float height,float depth)
    {
        var g=JEIFurnitureKit.Group(p,"Full height practical faculty storage",pos,yaw);
        float lowerTop=height*.405f,openTop=height*.585f,baseY=.10f,topY=height-.035f;
        B(g,"Recessed furniture plinth",new Vector3(0,.045f,.015f),new Vector3(width-.10f,.09f,depth-.07f),Hardware);
        B(g,"Warm oak backing",new Vector3(0,height/2,depth/2-.018f),new Vector3(width,height,.035f),Wood);
        foreach(float x in new[]{-width/2+.02f,width/2-.02f}) B(g,"Solid oak full height side",new Vector3(x,height/2,0),new Vector3(.04f,height,depth),Wood);
        foreach(float y in new[]{baseY,lowerTop,openTop,topY}) B(g,"Oak storage division shelf",new Vector3(0,y,0),new Vector3(width-.055f,.035f,depth),Wood);
        float doorZ=-depth/2+.018f;
        // Flush sliding fronts and inset finger pulls occupy no extra opening arc.
        for(int half=0;half<2;half++) {
            float x=(half==0?-1:1)*width*.25f;
            float panelW=width/2-.036f;
            float lowerH=lowerTop-baseY-.045f;
            R(g,"Lower closed sliding storage front",new Vector3(x,(baseY+lowerTop)/2,doorZ),new Vector3(panelW,lowerH,.032f),Cream,.016f);
            B(g,"Inset lower finger pull",new Vector3(x+(half==0?.14f:-.14f),lowerTop-.15f,doorZ-.017f),new Vector3(.10f,.014f,.002f),Hardware);
            float upperH=topY-openTop-.045f;
            R(g,"Upper closed sliding archive front",new Vector3(x,(openTop+topY)/2,doorZ),new Vector3(panelW,upperH,.032f),Cream,.016f);
            B(g,"Inset upper finger pull",new Vector3(x+(half==0?.14f:-.14f),openTop+.17f,doorZ-.017f),new Vector3(.10f,.014f,.002f),Hardware);
        }
        // Open daily-use slot sits around 0.85--1.23 m on a 2.1 m cabinet.
        float shelfBase=lowerTop+.02f;float available=openTop-lowerTop-.04f;
        for(int i=0;i<5;i++) {
            float bookW=.065f+(i%2)*.015f,bookH=Mathf.Min(.28f,available-.04f);
            float x=-width/2+.16f+i*.105f;
            B(g,"Daily reference binder",new Vector3(x,shelfBase+bookH/2,-.035f),new Vector3(bookW,bookH,depth*.62f),i%3==0?BookBlue:i%3==1?Sage:Oatmeal);
            B(g,"Binder spine label",new Vector3(x,shelfBase+bookH*.60f,-.035f-depth*.31f-.002f),new Vector3(bookW*.55f,.065f,.003f),Paper);
        }
        float trayWidth=Mathf.Min(.42f,width*.27f);
        for(int i=0;i<3;i++) {
            float y=shelfBase+.026f+i*.058f;
            B(g,"Open shelf document tray",new Vector3(width*.22f,y,-.025f),new Vector3(trayWidth,.025f,depth*.65f),Cream);
            B(g,"Sorted paper stack",new Vector3(width*.22f,y+.018f,-.025f),new Vector3(trayWidth-.025f,.015f,depth*.59f),Paper);
        }
        BoundsCollider(g,new Vector3(0,height/2,0),new Vector3(width,height,depth));return g;
    }
}
