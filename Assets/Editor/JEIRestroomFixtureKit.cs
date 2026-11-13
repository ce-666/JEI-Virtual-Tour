using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>Reusable sanitary fixtures. Position is the finish floor; every fixture faces local -Z.</summary>
public static class JEIRestroomFixtureKit
{
    const string MeshFolder="Assets/JEI_Furnishings/Meshes";
    static readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
    static Material Porcelain { get { return F.Mat("WC_Cream_Glazed_Porcelain",new Color(.92f,.925f,.865f),.63f); } }
    static Material Seat { get { return F.Mat("WC_Warm_White_Toilet_Seat",new Color(.95f,.955f,.91f),.42f); } }
    static Material Metal { get { return F.Mat("WC_Satin_Fitting_Metal",new Color(.62f,.69f,.68f),.59f); } }
    static Material Sage { get { return F.Mat("WC_Muted_Sage_Fitting",new Color(.40f,.57f,.48f),.31f); } }
    static Material Dark { get { return F.Mat("WC_Dark_Fitting_Detail",new Color(.20f,.28f,.26f),.24f); } }
    static Material Water { get { return F.Mat("WC_Still_Pale_Water",new Color(.63f,.77f,.77f),.82f); } }

    /// <summary>Compact WC: X[-.215,.215], Z[-.300,.429], seat Y.44; front is -Z.</summary>
    public static Transform Toilet(Transform parent,string name,Vector3 position,float yaw=0)
    {
        var g=F.Group(parent,name,position,yaw);
        MeshObject(g,"WC_Sculpted_open_ceramic_pan",Vector3.zero,ProfileMesh("WC_Compact_Open_Toilet_Pan_v1",new[]{
            new Vector3(.135f,0,.170f),new Vector3(.130f,.030f,.160f),
            new Vector3(.115f,.145f,.150f),new Vector3(.172f,.225f,.228f),
            new Vector3(.205f,.335f,.285f),new Vector3(.215f,.415f,.300f),
            new Vector3(.165f,.415f,.235f),new Vector3(.155f,.350f,.220f),
            new Vector3(.108f,.280f,.162f),new Vector3(.038f,.255f,.060f),
            new Vector3(.001f,.252f,.001f)
        },false,true),Porcelain);
        MeshObject(g,"WC_Open_oval_seat_ring__AFF0.44m",Vector3.zero,ProfileMesh("WC_Compact_Oval_Toilet_Seat_v1",new[]{
            new Vector3(.215f,.415f,.300f),new Vector3(.213f,.440f,.298f),
            new Vector3(.155f,.440f,.225f),new Vector3(.155f,.415f,.225f)
        },true,false),Seat);
        P(g,"WC_Recessed_bowl_water",PrimitiveType.Cylinder,new Vector3(0,.262f,0),new Vector3(.095f,.0015f,.158f),Water);
        Round(g,"WC_Compact_ceramic_cistern",new Vector3(0,.605f,.340f),new Vector3(.375f,.400f,.170f),Porcelain,.032f);
        Round(g,"WC_Separate_cistern_lid",new Vector3(0,.815f,.340f),new Vector3(.385f,.025f,.178f),Seat,.032f);
        Tube(g,"WC_Flush_lever_pivot",new Vector3(-.195f,.740f,.340f),new Vector3(-.180f,.740f,.340f),.020f,Metal,false);
        Round(g,"WC_Push_flush_lever",new Vector3(-.205f,.741f,.307f),new Vector3(.016f,.022f,.085f),Metal,.007f);
        var lid=MeshObject(g,"WC_Raised_oval_lid",new Vector3(0,.630f,.235f),ProfileMesh("WC_Compact_Raised_Toilet_Lid_v1",new[]{
            new Vector3(.185f,-.014f,.235f),new Vector3(.188f,.004f,.238f),
            new Vector3(.180f,.014f,.230f),new Vector3(.001f,.014f,.001f)
        },false,true),Seat);
        lid.transform.localRotation=Quaternion.Euler(90,0,0);
        Solid(lid.transform,Vector3.zero,new Vector3(.376f,.028f,.476f));
        foreach(float x in new[]{-.095f,.095f})
            Tube(g,"WC_Seat_hinge",new Vector3(x-.020f,.418f,.247f),new Vector3(x+.020f,.418f,.247f),.022f,Metal,false);
        // The pan, foot, tank and raised lid have separate compact collisions.
        Solid(g,new Vector3(0,.150f,0),new Vector3(.27f,.30f,.44f));
        Solid(g,new Vector3(0,.355f,0),new Vector3(.43f,.17f,.60f));
        Solid(g,new Vector3(0,.61625f,.340f),new Vector3(.385f,.4225f,.178f));
        return g;
    }

    /// <summary>Suspended .58 x .48m basin; rim Y.81. Back wall is at local +Z.</summary>
    public static Transform Basin(Transform parent,string name,Vector3 position,float yaw=0)
    {
        var g=F.Group(parent,name,position,yaw);
        MeshObject(g,"WC_Suspended_lavatory_with_real_bowl",Vector3.zero,ProfileMesh("WC_Wall_Hung_Basin_Shell_v1",new[]{
            new Vector3(.205f,.725f,.160f),new Vector3(.265f,.740f,.205f),
            new Vector3(.288f,.785f,.236f),new Vector3(.290f,.810f,.240f),
            new Vector3(.215f,.810f,.165f),new Vector3(.205f,.785f,.160f),
            new Vector3(.150f,.760f,.115f),new Vector3(.045f,.745f,.045f),
            new Vector3(.001f,.744f,.001f)
        },false,true),Porcelain);
        P(g,"WC_Rear_basin_drain",PrimitiveType.Cylinder,new Vector3(0,.746f,.075f),new Vector3(.035f,.0015f,.035f),Metal);
        P(g,"WC_Drain_opening",PrimitiveType.Cylinder,new Vector3(0,.748f,.075f),new Vector3(.017f,.001f,.017f),Dark);
        foreach(float x in new[]{-.18f,.18f})
            Round(g,"WC_High_wall_mount_arm",new Vector3(x,.733f,.060f),new Vector3(.028f,.020f,.34f),Metal,.008f);
        Tube(g,"WC_Sensor_faucet_body",new Vector3(0,.810f,.175f),new Vector3(0,.933f,.175f),.027f,Metal,false);
        Tube(g,"WC_Faucet_curved_neck",new Vector3(0,.933f,.175f),new Vector3(0,.957f,.130f),.025f,Metal,false);
        Tube(g,"WC_Forward_faucet_spout",new Vector3(0,.957f,.130f),new Vector3(0,.957f,.015f),.025f,Metal,false);
        Tube(g,"WC_Downward_faucet_aerator",new Vector3(0,.957f,.015f),new Vector3(0,.940f,.015f),.023f,Metal,false);
        B(g,"WC_Touch_free_sensor",new Vector3(0,.874f,.159f),new Vector3(.015f,.013f,.004f),Dark);
        Round(g,"WC_Sage_faucet_base_ring",new Vector3(0,.816f,.175f),new Vector3(.048f,.012f,.048f),Sage,.014f);
        // Pipework turns back to the wall, leaving the front underside open.
        Tube(g,"WC_Protected_drain_outlet",new Vector3(0,.725f,.075f),new Vector3(0,.665f,.075f),.043f,Porcelain,true);
        Tube(g,"WC_Insulated_rear_drain",new Vector3(0,.665f,.075f),new Vector3(0,.610f,.205f),.043f,Porcelain,true);
        Tube(g,"WC_Rear_wall_drain_connection",new Vector3(0,.610f,.205f),new Vector3(0,.510f,.205f),.048f,Porcelain,true);
        Solid(g,new Vector3(0,.785f,0),new Vector3(.58f,.050f,.48f));
        Solid(g,new Vector3(0,.745f,-.010f),new Vector3(.43f,.040f,.34f));
        return g;
    }

    /// <summary>Wall-hung urinal: W.38 x D.35m, open bowl rim Y.55, front -Z.</summary>
    public static Transform Urinal(Transform parent,string name,Vector3 position,float yaw=0)
    {
        var g=F.Group(parent,name,position,yaw);
        MeshObject(g,"WC_Urinal_sculpted_open_catch_bowl",Vector3.zero,ProfileMesh("WC_Urinal_Open_Catch_Bowl_v1",new[]{
            new Vector3(.075f,.325f,.080f),new Vector3(.090f,.350f,.100f),
            new Vector3(.150f,.440f,.140f),new Vector3(.185f,.510f,.170f),
            new Vector3(.190f,.550f,.175f),new Vector3(.145f,.550f,.135f),
            new Vector3(.125f,.500f,.115f),new Vector3(.075f,.435f,.070f),
            new Vector3(.001f,.420f,.001f)
        },false,true),Porcelain);
        MeshObject(g,"WC_Urinal_upright_concave_splash_back",Vector3.zero,UrinalBackMesh(),Porcelain);
        P(g,"WC_Urinal_recessed_waste_grate",PrimitiveType.Cylinder,new Vector3(0,.428f,0),new Vector3(.065f,.002f,.065f),Metal);
        foreach(float x in new[]{-.017f,0,.017f})
            B(g,"WC_Waste_grate_slot",new Vector3(x,.431f,0),new Vector3(.005f,.002f,.042f),Dark);
        Tube(g,"WC_Urinal_flush_pipe",new Vector3(0,1.050f,.145f),new Vector3(0,1.180f,.145f),.025f,Metal,false);
        Round(g,"WC_Automatic_flush_control",new Vector3(0,1.150f,.145f),new Vector3(.070f,.090f,.045f),Metal,.010f);
        B(g,"WC_Urinal_flush_sensor",new Vector3(0,1.150f,.120f),new Vector3(.025f,.020f,.004f),Dark);
        Round(g,"WC_Sage_flush_control_band",new Vector3(0,1.118f,.120f),new Vector3(.053f,.012f,.004f),Sage,.0015f);
        Tube(g,"WC_Protected_urinal_waste_pipe",new Vector3(0,.325f,.075f),new Vector3(0,.235f,.075f),.042f,Porcelain,true);
        Tube(g,"WC_Wall_return_waste_pipe",new Vector3(0,.235f,.075f),new Vector3(0,.235f,.145f),.042f,Porcelain,true);
        // A hollow front remains hollow in physics: back, lower cup, side walls
        // and the leading lip receive their own boxes, never one full-height block.
        Solid(g,new Vector3(0,.795f,.143f),new Vector3(.30f,.55f,.050f));
        Solid(g,new Vector3(0,.385f,.020f),new Vector3(.23f,.12f,.21f));
        foreach(float x in new[]{-.160f,.160f})
            Solid(g,new Vector3(x,.470f,-.015f),new Vector3(.060f,.16f,.29f));
        Solid(g,new Vector3(0,.535f,-.151f),new Vector3(.30f,.030f,.048f));
        return g;
    }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,n,pos,size,m,false); }
    static GameObject P(Transform p,string n,PrimitiveType type,Vector3 pos,Vector3 size,Material m)
    { return F.Primitive(p,n,type,pos,size,m); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    static Transform Tube(Transform parent,string name,Vector3 from,Vector3 to,float diameter,Material material,bool collision)
    {
        var g=F.Group(parent,name,(from+to)/2);float length=Vector3.Distance(from,to);
        g.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
        P(g,"Round_tube",PrimitiveType.Cylinder,Vector3.zero,new Vector3(diameter,length/2,diameter),material);
        foreach(float y in new[]{-length/2,length/2})
            P(g,"Soft_tube_end",PrimitiveType.Sphere,new Vector3(0,y,0),Vector3.one*diameter,material);
        if(collision) { var c=g.gameObject.AddComponent<CapsuleCollider>();c.direction=1;c.radius=diameter/2;c.height=length+diameter; }
        return g;
    }

    static GameObject MeshObject(Transform parent,string name,Vector3 pos,Mesh mesh,Material material)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;return g;
    }

    static Mesh SavedMesh(string name,Func<Mesh> create)
    {
        Mesh cached;if(meshes.TryGetValue(name,out cached) && cached) return cached;
        if(!AssetDatabase.IsValidFolder(MeshFolder)) AssetDatabase.CreateFolder("Assets/JEI_Furnishings","Meshes");
        string path=MeshFolder+"/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(!mesh) { mesh=create();mesh.name=name;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path); }
        meshes[name]=mesh;return mesh;
    }

    static Mesh ProfileMesh(string name,Vector3[] profile,bool closedProfile,bool bottomCap)
    {
        return SavedMesh(name,()=>{
            const int segments=72;var vertices=new List<Vector3>();var triangles=new List<int>();
            foreach(Vector3 row in profile) for(int i=0;i<segments;i++) {
                float a=i*2*Mathf.PI/segments;vertices.Add(new Vector3(Mathf.Cos(a)*row.x,row.y,Mathf.Sin(a)*row.z));
            }
            int bands=closedProfile?profile.Length:profile.Length-1;
            for(int row=0;row<bands;row++) for(int i=0;i<segments;i++) {
                int a=row*segments+i,b=row*segments+(i+1)%segments,c=((row+1)%profile.Length)*segments+i,d=((row+1)%profile.Length)*segments+(i+1)%segments;
                triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            if(bottomCap) {
                int centre=vertices.Count;vertices.Add(new Vector3(0,profile[0].y,0));
                for(int i=0;i<segments;i++) triangles.AddRange(new[]{centre,i,(i+1)%segments});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        });
    }

    static Mesh UrinalBackMesh()
    {
        return SavedMesh("WC_Urinal_Concave_Upright_Back_v1",()=>{
            const int steps=32;
            // X=inner half-width, Y=elevation, Z=front edge of the curved side.
            var rows=new[]{new Vector3(.145f,.540f,-.080f),new Vector3(.173f,.650f,-.020f),new Vector3(.155f,.900f,.045f),new Vector3(.125f,1.040f,.095f),new Vector3(.080f,1.070f,.120f)};
            var vertices=new List<Vector3>();var triangles=new List<int>();int stride=steps+1,skin=rows.Length*stride;
            for(int side=0;side<2;side++) foreach(Vector3 row in rows) for(int i=0;i<=steps;i++) {
                float a=-Mathf.PI/2+i*Mathf.PI/steps;
                float x=(row.x+side*.017f)*Mathf.Sin(a);
                float z=row.z+(.150f-row.z+side*.018f)*Mathf.Cos(a);
                vertices.Add(new Vector3(x,row.y,z));
            }
            for(int side=0;side<2;side++) for(int row=0;row<rows.Length-1;row++) for(int i=0;i<steps;i++) {
                int a=side*skin+row*stride+i,b=a+1,c=a+stride,d=c+1;
                if(side==0) triangles.AddRange(new[]{a,c,b,b,c,d});
                else triangles.AddRange(new[]{a,b,c,b,d,c});
            }
            // Seal the exposed top, lower edge and the two side lips.
            for(int row=0;row<rows.Length-1;row++) foreach(int i in new[]{0,steps}) {
                int a=row*stride+i,b=a+stride,c=skin+a,d=skin+b;
                if(i==0) triangles.AddRange(new[]{a,c,b,b,c,d});else triangles.AddRange(new[]{a,b,c,b,d,c});
            }
            foreach(int row in new[]{0,rows.Length-1}) for(int i=0;i<steps;i++) {
                int a=row*stride+i,b=a+1,c=skin+a,d=skin+b;
                if(row==0) triangles.AddRange(new[]{a,b,c,b,d,c});else triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        });
    }

    static string Key(float value)
    { return value.ToString("0.####",CultureInfo.InvariantCulture).Replace("-","m").Replace(".","p"); }

    static GameObject Round(Transform parent,string name,Vector3 pos,Vector3 size,Material material,float radius)
    {
        radius=Mathf.Min(radius,Mathf.Min(size.x,size.z)*.45f);
        string key="WC_Rounded_"+Key(size.x)+"_"+Key(size.y)+"_"+Key(size.z)+"_"+Key(radius);
        Mesh mesh=SavedMesh(key,()=>{
            const int steps=5;int perimeter=4*(steps+1);var vertices=new List<Vector3>();var triangles=new List<int>();float bevel=Mathf.Min(.008f,size.y*.2f);
            for(int layer=0;layer<4;layer++) {
                float inset=layer==0 || layer==3?bevel:0;
                float y=layer==0?-size.y/2:layer==1?-size.y/2+bevel:layer==2?size.y/2-bevel:size.y/2;
                float r=Mathf.Max(.001f,radius-inset);
                for(int corner=0;corner<4;corner++) {
                    float cx=(corner==0 || corner==3?1:-1)*(size.x/2-radius),cz=(corner<2?1:-1)*(size.z/2-radius);
                    for(int i=0;i<=steps;i++) {float a=(corner*90f+i*90f/steps)*Mathf.Deg2Rad;vertices.Add(new Vector3(cx+Mathf.Cos(a)*r,y,cz+Mathf.Sin(a)*r));}
                }
            }
            for(int layer=0;layer<3;layer++) for(int i=0;i<perimeter;i++) {int next=(i+1)%perimeter,a=layer*perimeter+i,b=layer*perimeter+next,c=(layer+1)*perimeter+i,d=(layer+1)*perimeter+next;triangles.AddRange(new[]{a,c,b,b,c,d});}
            int bottom=vertices.Count;vertices.Add(new Vector3(0,-size.y/2,0));int top=vertices.Count;vertices.Add(new Vector3(0,size.y/2,0));
            for(int i=0;i<perimeter;i++) {int n=(i+1)%perimeter;triangles.AddRange(new[]{bottom,i,n,top,3*perimeter+n,3*perimeter+i});}
            var result=new Mesh();result.SetVertices(vertices);result.SetTriangles(triangles,0);result.RecalculateNormals();return result;
        });
        return MeshObject(parent,name,pos,mesh,material);
    }
}
