using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>C07: single-user washroom, modeled around the retained turning and transfer spaces.</summary>
public static class JEIAccessibleWashroom
{
    const float FloorTop=.075f;
    const float WCX=8.87f;
    const string MeshFolder="Assets/JEI_Furnishings/Meshes";
    static readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
    static Material Porcelain { get { return F.Mat("C07_Cream_Glazed_Porcelain",new Color(.92f,.925f,.865f),.63f); } }
    static Material Seat { get { return F.Mat("C07_Warm_White_Toilet_Seat",new Color(.95f,.955f,.91f),.42f); } }
    static Material Sage { get { return F.Mat("C07_Sage_Grab_Rails",new Color(.40f,.57f,.48f),.34f); } }
    static Material Chrome { get { return F.Mat("C07_Brushed_Chrome",new Color(.62f,.69f,.68f),.78f); } }
    static Material Tile { get { return F.Mat("C07_Pale_Grey_Floor_Tile",new Color(.73f,.77f,.76f),.14f); } }
    static Material Grout { get { return F.Mat("C07_Floor_Tile_Grout",new Color(.56f,.62f,.60f),.12f); } }
    static Material Dark { get { return F.Mat("C07_Dark_Fitting_Detail",new Color(.20f,.28f,.26f),.24f); } }
    static Material Paper { get { return F.Mat("C07_Paper_White",new Color(.97f,.965f,.91f),.10f); } }
    static Material Water { get { return F.Mat("C07_Still_Pale_Water",new Color(.63f,.77f,.77f),.82f); } }
    static Material Alarm { get { return F.Mat("C07_Emergency_Red",new Color(.76f,.22f,.16f),.24f); } }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,n,pos,size,m,false); }
    static GameObject P(Transform p,string n,PrimitiveType type,Vector3 pos,Vector3 size,Material m)
    { return F.Primitive(p,n,type,pos,size,m); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static Transform Build(Transform parent)
    {
        var room=F.Group(parent,"C07__Accessible_washroom");
        Floor(room);
        WaterCloset(room);
        WallHungLavatory(room);
        FixedGrabRails(room);
        RaisedTransferRail(room);
        MirrorAndWashroomAccessories(room);
        EmergencyCord(room);
        return room;
    }

    static void Floor(Transform parent)
    {
        var g=F.Group(parent,"C07_Light_grey_tile_floor");
        B(g,"C07_Thin_tiled_floor__top_Y0.075m",new Vector3(8.0f,.0625f,-1.65f),new Vector3(2.58f,.025f,3.08f),Tile);
        foreach(float x in new[]{7.355f,8.0f,8.645f})
            B(g,"Fine_longitudinal_tile_joint",new Vector3(x,.07535f,-1.65f),new Vector3(.003f,.0007f,3.08f),Grout);
        foreach(float z in new[]{-2.42f,-1.65f,-.88f})
            B(g,"Fine_cross_tile_joint",new Vector3(8.0f,.07535f,z),new Vector3(2.58f,.0007f,.003f),Grout);
        Solid(g,new Vector3(8.0f,.0625f,-1.65f),new Vector3(2.58f,.025f,3.08f));
        // No marker, pedestal or collider is placed in the D1.525m turning circle
        // centred at (7.97,-1.90). The south doorway retains its open structural gap.
    }

    static void WaterCloset(Transform parent)
    {
        var g=F.Group(parent,"C07_Water_closet",new Vector3(WCX,0,-.625f));
        // World seat top .515 minus finish floor .075 = .440m. The centreline is
        // .430m from the east wall; the rounded pan is an actual open concave shell.
        var panProfile=new[]{
            new Vector3(.140f,.085f,.180f),new Vector3(.135f,.105f,.170f),
            new Vector3(.120f,.220f,.160f),new Vector3(.180f,.300f,.240f),
            new Vector3(.215f,.410f,.300f),new Vector3(.225f,.490f,.315f),
            new Vector3(.175f,.490f,.250f),new Vector3(.165f,.425f,.235f),
            new Vector3(.115f,.355f,.175f),new Vector3(.040f,.330f,.065f),
            new Vector3(.001f,.327f,.001f)
        };
        MeshObject(g,"Sculpted_concave_ceramic_pan",Vector3.zero,ProfileMesh("C07_Toilet_Open_Pan_Shell",panProfile,false,true),Porcelain);
        var seatProfile=new[]{new Vector3(.225f,.490f,.315f),new Vector3(.223f,.515f,.313f),new Vector3(.162f,.515f,.240f),new Vector3(.162f,.490f,.240f)};
        MeshObject(g,"Open_oval_toilet_seat_ring__top_Y0.515m",Vector3.zero,ProfileMesh("C07_Toilet_Oval_Seat_Ring",seatProfile,true,false),Seat);
        P(g,"Recessed_water_in_bowl",PrimitiveType.Cylinder,new Vector3(0,.337f,0),new Vector3(.10f,.0015f,.17f),Water);

        Round(g,"Compact_ceramic_cistern",new Vector3(0,.680f,.395f),new Vector3(.395f,.400f,.190f),Porcelain,.035f);
        Round(g,"Separate_cistern_lid",new Vector3(0,.890f,.395f),new Vector3(.405f,.025f,.198f),Seat,.035f);
        // Side-operated flush is on the open transfer side rather than against the wall.
        Tube(g,"Open_side_flush_pivot",new Vector3(-.213f,.815f,.395f),new Vector3(-.190f,.815f,.395f),.025f,Chrome,false);
        Round(g,"Easy_push_flush_lever",new Vector3(-.220f,.816f,.357f),new Vector3(.018f,.022f,.095f),Chrome,.008f);
        var lid=MeshObject(g,"Raised_oval_seat_lid",new Vector3(0,.705f,.280f),ProfileMesh("C07_Raised_Toilet_Lid",new[]{
            new Vector3(.195f,-.014f,.245f),new Vector3(.198f,.004f,.248f),new Vector3(.190f,.014f,.240f),new Vector3(.001f,.014f,.001f)
        },false,true),Seat);
        lid.transform.localRotation=Quaternion.Euler(90,0,0);
        foreach(float x in new[]{-.10f,.10f})
            Tube(g,"Seat_hinge",new Vector3(x-.022f,.493f,.293f),new Vector3(x+.022f,.493f,.293f),.023f,Chrome,false);

        // Three compact colliders describe the base, pan and tank without treating
        // the neighbouring transfer space as one large solid fixture volume.
        Solid(g,new Vector3(0,.225f,0),new Vector3(.28f,.30f,.46f));
        Solid(g,new Vector3(0,.430f,0),new Vector3(.45f,.17f,.63f));
        Solid(g,new Vector3(0,.690f,.395f),new Vector3(.405f,.425f,.198f));
    }

    static void WallHungLavatory(Transform parent)
    {
        var g=F.Group(parent,"C07_Wall_hung_lavatory",new Vector3(6.985f,0,-.50f),90);
        // Local +Z is east, toward the user. Projected bounds are world
        // X[6.72,7.25], Z[-.80,-.20]. No cabinet or pedestal is placed underneath.
        MeshObject(g,"Shallow_sculpted_lavatory_with_open_bowl",Vector3.zero,ProfileMesh("C07_Wall_Hung_Lavatory_Shell",new[]{
            new Vector3(.215f,.795f,.180f),new Vector3(.270f,.810f,.230f),
            new Vector3(.297f,.855f,.262f),new Vector3(.300f,.880f,.265f),
            new Vector3(.225f,.880f,.180f),new Vector3(.215f,.855f,.175f),
            new Vector3(.160f,.830f,.130f),new Vector3(.050f,.815f,.050f),
            new Vector3(.001f,.814f,.001f)
        },false,true),Porcelain);
        P(g,"Rear_basin_drain",PrimitiveType.Cylinder,new Vector3(0,.816f,-.085f),new Vector3(.037f,.0015f,.037f),Chrome);
        P(g,"Drain_opening",PrimitiveType.Cylinder,new Vector3(0,.818f,-.085f),new Vector3(.018f,.001f,.018f),Dark);
        foreach(float x in new[]{-.19f,.19f})
            Round(g,"High_wall_mounting_arm",new Vector3(x,.803f,-.065f),new Vector3(.030f,.022f,.385f),Chrome,.009f);
        Tube(g,"Sensor_tap_body",new Vector3(0,.880f,-.195f),new Vector3(0,1.002f,-.195f),.028f,Chrome,false);
        Tube(g,"Tap_curved_neck",new Vector3(0,1.002f,-.195f),new Vector3(0,1.026f,-.151f),.026f,Chrome,false);
        Tube(g,"Tap_forward_spout",new Vector3(0,1.026f,-.151f),new Vector3(0,1.026f,-.025f),.026f,Chrome,false);
        Tube(g,"Downward_aerator",new Vector3(0,1.026f,-.025f),new Vector3(0,1.010f,-.025f),.024f,Chrome,false);
        B(g,"Touch_free_tap_sensor",new Vector3(0,.946f,-.179f),new Vector3(.016f,.014f,.004f),Dark);

        // Insulated drainage turns back to the west wall. Its frontmost extent
        // is X6.925, behind the full-height front knee zone X[6.95,7.25].
        Tube(g,"Protected_drain_outlet",new Vector3(0,.795f,-.085f),new Vector3(0,.735f,-.085f),.045f,Porcelain,true);
        Tube(g,"Insulated_rear_sloping_drain",new Vector3(0,.735f,-.085f),new Vector3(0,.680f,-.215f),.045f,Porcelain,true);
        Tube(g,"Rear_wall_drain_connection",new Vector3(0,.680f,-.215f),new Vector3(0,.580f,-.215f),.050f,Porcelain,true);
        // Separate, high shell collisions keep the underside open to knee height.
        // The bottom of every front-facing shell is worldY >= .795 (> .760m).
        Solid(g,new Vector3(0,.855f,0),new Vector3(.60f,.050f,.53f));
        Solid(g,new Vector3(0,.815f,.015f),new Vector3(.46f,.040f,.38f));
    }

    static void FixedGrabRails(Transform parent)
    {
        var g=F.Group(parent,"C07_Fixed_grab_rails");
        // Fixed bars have a .036m round grip, with approximately .04m wall gap.
        Tube(g,"East_wall_horizontal_support_rail",new Vector3(9.24f,.930f,-.200f),new Vector3(9.24f,.930f,-1.350f),.036f,Sage,true);
        foreach(float z in new[]{-.30f,-1.23f}) {
            Tube(g,"East_rail_wall_return",new Vector3(9.24f,.930f,z),new Vector3(9.285f,.930f,z),.028f,Chrome,true);
            MountPlate(g,"East_rail_wall_fixing",new Vector3(9.285f,.930f,z),new Vector3(.020f,.080f,.080f));
        }
        Tube(g,"North_wall_rear_support_rail",new Vector3(7.99f,.930f,-.158f),new Vector3(9.10f,.930f,-.158f),.036f,Sage,true);
        foreach(float x in new[]{8.10f,9.03f}) {
            Tube(g,"Rear_rail_wall_return",new Vector3(x,.930f,-.158f),new Vector3(x,.930f,-.115f),.028f,Chrome,true);
            MountPlate(g,"Rear_rail_wall_fixing",new Vector3(x,.930f,-.115f),new Vector3(.080f,.080f,.020f));
        }
    }

    static void RaisedTransferRail(Transform parent)
    {
        var g=F.Group(parent,"C07_Fold_up_transfer_rail__raised");
        // The optional west-side arm is shown folded UP. The entire hinge plate
        // and all rail colliders are above Y.80, preserving the low transfer strip.
        Round(g,"Reinforced_fold_rail_wall_plate",new Vector3(8.48f,.880f,-.133f),new Vector3(.14f,.14f,.025f),Sage,.016f);
        Solid(g,new Vector3(8.48f,.880f,-.133f),new Vector3(.14f,.14f,.025f));
        Tube(g,"Visible_fold_rail_hinge_axis",new Vector3(8.425f,.870f,-.196f),new Vector3(8.535f,.870f,-.196f),.067f,Chrome,true);
        Tube(g,"Raised_outer_arm",new Vector3(8.48f,.875f,-.255f),new Vector3(8.48f,1.520f,-.255f),.036f,Sage,true);
        Tube(g,"Raised_inner_return_arm",new Vector3(8.48f,.875f,-.180f),new Vector3(8.48f,1.520f,-.180f),.036f,Sage,true);
        // Rounded U at the upper end communicates the movable arm's full length.
        Vector3 previous=new Vector3(8.48f,1.520f,-.255f);
        for(int i=1;i<=8;i++) {
            float angle=i*Mathf.PI/8;
            Vector3 next=new Vector3(8.48f,1.520f+Mathf.Sin(angle)*.0375f,-.2175f-Mathf.Cos(angle)*.0375f);
            Tube(g,"Rounded_upper_fold_arm_return",previous,next,.036f,Sage,true);previous=next;
        }
    }

    static void MountPlate(Transform parent,string name,Vector3 pos,Vector3 size)
    {
        Round(parent,name,pos,size,Chrome,.009f);
        // Fixing plates project very little and receive their own small collider.
        var g=F.Group(parent,name+"_collision",pos);Solid(g,Vector3.zero,size);
    }

    static void MirrorAndWashroomAccessories(Transform parent)
    {
        var mirror=F.Group(parent,"C07_Low_edge_wall_mirror",new Vector3(6.726f,1.435f,-.50f),270);
        Round(mirror,"Soft_corner_sage_mirror_frame",Vector3.zero,new Vector3(.68f,.035f,.79f),Sage,.025f).transform.localRotation=Quaternion.Euler(90,0,0);
        var glass=F.Mat("C07_Mirror_Silver_Blue",new Color(.69f,.78f,.78f),.96f);
        if(glass.HasProperty("_Metallic")) glass.SetFloat("_Metallic",.80f);
        EditorUtility.SetDirty(glass);
        B(mirror,"Reflective_mirror_panel",new Vector3(0,0,-.021f),new Vector3(.63f,.74f,.006f),glass);
        Solid(mirror,Vector3.zero,new Vector3(.68f,.79f,.043f));

        var soap=F.Group(parent,"C07_Wall_soap_dispenser",new Vector3(6.767f,1.030f,-1.00f),270);
        Round(soap,"Cream_touch_free_soap_housing",Vector3.zero,new Vector3(.14f,.22f,.09f),Porcelain,.020f);
        B(soap,"Soap_level_window",new Vector3(0,-.02f,-.047f),new Vector3(.05f,.10f,.004f),Sage);
        Tube(soap,"Soap_outlet",new Vector3(0,-.098f,-.030f),new Vector3(0,-.125f,-.045f),.016f,Chrome,false);
        Solid(soap,Vector3.zero,new Vector3(.14f,.22f,.09f));

        var towel=F.Group(parent,"C07_Paper_towel_dispenser",new Vector3(6.777f,1.150f,-1.40f),270);
        Round(towel,"Slim_cream_towel_dispenser",Vector3.zero,new Vector3(.28f,.32f,.10f),Porcelain,.020f);
        B(towel,"Dark_paper_outlet",new Vector3(0,-.125f,-.055f),new Vector3(.22f,.020f,.008f),Dark);
        B(towel,"Continuous_paper_towel",new Vector3(0,-.205f,-.063f),new Vector3(.20f,.14f,.004f),Paper);
        Solid(towel,Vector3.zero,new Vector3(.28f,.32f,.10f));

        var roll=F.Group(parent,"C07_Toilet_paper_holder",new Vector3(9.275f,.660f,-1.04f));
        Round(roll,"East_wall_roll_mount",Vector3.zero,new Vector3(.025f,.20f,.18f),Sage,.010f);
        Tube(roll,"Roll_spindle",new Vector3(-.125f,0,0),new Vector3(0,0,0),.017f,Chrome,false);
        var paperRoll=P(roll,"Continuous_toilet_tissue_roll",PrimitiveType.Cylinder,new Vector3(-.100f,0,0),new Vector3(.13f,.038f,.13f),Paper);
        paperRoll.transform.localRotation=Quaternion.Euler(0,0,90);
        var core=P(roll,"Visible_roll_core",PrimitiveType.Cylinder,new Vector3(-.139f,0,0),new Vector3(.034f,.001f,.034f),Grout);
        core.transform.localRotation=Quaternion.Euler(0,0,90);
        B(roll,"Free_hanging_tissue_sheet",new Vector3(-.12f,-.115f,-.015f),new Vector3(.004f,.15f,.10f),Paper);
        Solid(roll,new Vector3(-.07f,-.02f,0),new Vector3(.165f,.24f,.18f));

        var bin=F.Group(parent,"C07_Small_closed_waste_bin",new Vector3(6.90f,FloorTop,-2.93f));
        Round(bin,"Compact_closed_bin_body",new Vector3(0,.155f,0),new Vector3(.25f,.31f,.24f),Porcelain,.025f);
        Round(bin,"Small_sage_bin_lid",new Vector3(0,.322f,0),new Vector3(.26f,.025f,.25f),Sage,.025f);
        B(bin,"Bin_lid_finger_slot",new Vector3(0,.337f,-.055f),new Vector3(.08f,.005f,.012f),Dark);
        Solid(bin,new Vector3(0,.17f,0),new Vector3(.26f,.34f,.25f));

        var hook=F.Group(parent,"C07_Low_reach_coat_hook",new Vector3(6.725f,1.200f,-2.48f));
        Round(hook,"Hook_wall_pad",Vector3.zero,new Vector3(.025f,.09f,.07f),Sage,.008f);
        Tube(hook,"Short_coat_hook",new Vector3(0,0,0),new Vector3(.075f,0,0),.016f,Chrome,false);
        Tube(hook,"Rounded_upturned_hook_tip",new Vector3(.075f,0,0),new Vector3(.075f,.025f,0),.016f,Chrome,false);
    }

    static void EmergencyCord(Transform parent)
    {
        var g=F.Group(parent,"C07_Emergency_pull_cord");
        // Both red pull rings remain against the east side, away from the west
        // transfer strip. The lower ring centre is .095m above the tiled floor.
        Round(g,"Emergency_alarm_cord_head",new Vector3(9.258f,2.170f,-.61f),new Vector3(.055f,.10f,.09f),Alarm,.012f);
        Tube(g,"Freely_hanging_alarm_pull_cord",new Vector3(9.235f,2.130f,-.61f),new Vector3(9.235f,.135f,-.61f),.005f,Alarm,false);
        foreach(float y in new[]{.170f,.950f})
            MeshObject(g,"Red_emergency_pull_ring",new Vector3(9.235f,y,-.61f),OvalTubeMesh("C07_Emergency_Pull_Ring",.026f,.036f,.005f),Alarm);
        var reset=F.Group(g,"C07_Emergency_reset_button",new Vector3(9.267f,1.010f,-1.33f),90);
        Round(reset,"Alarm_reset_wall_plate",Vector3.zero,new Vector3(.12f,.12f,.035f),Porcelain,.012f);
        P(reset,"Large_red_reset_button",PrimitiveType.Sphere,new Vector3(0,0,-.024f),new Vector3(.057f,.057f,.022f),Alarm);
    }

    static Transform Tube(Transform parent,string name,Vector3 from,Vector3 to,float diameter,Material material,bool collision)
    {
        var g=F.Group(parent,name,(from+to)/2);
        float length=Vector3.Distance(from,to);
        g.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
        P(g,"Round_tube",PrimitiveType.Cylinder,Vector3.zero,new Vector3(diameter,length/2,diameter),material);
        foreach(float y in new[]{-length/2,length/2})
            P(g,"Soft_tube_end",PrimitiveType.Sphere,new Vector3(0,y,0),Vector3.one*diameter,material);
        if(collision) {
            var c=g.gameObject.AddComponent<CapsuleCollider>();c.direction=1;
            c.radius=diameter/2;c.height=length+diameter;
        }
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
        string path=MeshFolder+"/"+name+".asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(!mesh) { mesh=create();mesh.name=name;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path); }
        meshes[name]=mesh;return mesh;
    }

    static Mesh ProfileMesh(string name,Vector3[] profile,bool closedProfile,bool bottomCap)
    {
        return SavedMesh(name,()=>{
            const int segments=72;var vertices=new List<Vector3>();var triangles=new List<int>();
            foreach(Vector3 row in profile)
                for(int i=0;i<segments;i++) {
                    float a=i*2*Mathf.PI/segments;
                    vertices.Add(new Vector3(Mathf.Cos(a)*row.x,row.y,Mathf.Sin(a)*row.z));
                }
            int bands=closedProfile?profile.Length:profile.Length-1;
            for(int row=0;row<bands;row++) for(int i=0;i<segments;i++) {
                int a=row*segments+i,b=row*segments+(i+1)%segments;
                int c=((row+1)%profile.Length)*segments+i,d=((row+1)%profile.Length)*segments+(i+1)%segments;
                triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            if(bottomCap) {
                int centre=vertices.Count;vertices.Add(new Vector3(0,profile[0].y,0));
                for(int i=0;i<segments;i++) triangles.AddRange(new[]{centre,i,(i+1)%segments});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        });
    }

    static Mesh OvalTubeMesh(string name,float radiusX,float radiusY,float tube)
    {
        return SavedMesh(name,()=>{
            const int around=48,sides=10;var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<around;i++) {
                float a=i*2*Mathf.PI/around;Vector3 centre=new Vector3(radiusX*Mathf.Cos(a),radiusY*Mathf.Sin(a),0);
                Vector3 outward=new Vector3(radiusY*Mathf.Cos(a),radiusX*Mathf.Sin(a),0).normalized;
                for(int j=0;j<sides;j++) {
                    float b=j*2*Mathf.PI/sides;Vector3 n=outward*Mathf.Cos(b)+Vector3.forward*Mathf.Sin(b);
                    vertices.Add(centre+n*tube);normals.Add(n);
                }
            }
            for(int i=0;i<around;i++) for(int j=0;j<sides;j++) {
                int a=i*sides+j,b=((i+1)%around)*sides+j,c=i*sides+(j+1)%sides,d=((i+1)%around)*sides+(j+1)%sides;
                triangles.AddRange(new[]{a,b,c,c,b,d});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);return mesh;
        });
    }

    static string Key(float value)
    { return value.ToString("0.####",CultureInfo.InvariantCulture).Replace("-","m").Replace(".","p"); }

    static GameObject Round(Transform parent,string name,Vector3 pos,Vector3 size,Material material,float radius)
    {
        // Use C07-owned persistent meshes rather than creating shared-kit resources.
        radius=Mathf.Min(radius,Mathf.Min(size.x,size.z)*.45f);
        string key="C07_Rounded_"+Key(size.x)+"_"+Key(size.y)+"_"+Key(size.z)+"_"+Key(radius);
        Mesh mesh=SavedMesh(key,()=>{
            const int steps=5;int perimeter=4*(steps+1);
            var vertices=new List<Vector3>();var triangles=new List<int>();
            float bevel=Mathf.Min(.008f,size.y*.2f);
            for(int layer=0;layer<4;layer++) {
                float inset=layer==0 || layer==3?bevel:0;
                float y=layer==0?-size.y/2:layer==1?-size.y/2+bevel:layer==2?size.y/2-bevel:size.y/2;
                float r=Mathf.Max(.001f,radius-inset);
                for(int corner=0;corner<4;corner++) {
                    float cx=(corner==0 || corner==3?1:-1)*(size.x/2-radius);
                    float cz=(corner<2?1:-1)*(size.z/2-radius);
                    for(int i=0;i<=steps;i++) {
                        float a=(corner*90f+i*90f/steps)*Mathf.Deg2Rad;
                        vertices.Add(new Vector3(cx+Mathf.Cos(a)*r,y,cz+Mathf.Sin(a)*r));
                    }
                }
            }
            for(int layer=0;layer<3;layer++) for(int i=0;i<perimeter;i++) {
                int next=(i+1)%perimeter,a=layer*perimeter+i,b=layer*perimeter+next,c=(layer+1)*perimeter+i,d=(layer+1)*perimeter+next;
                triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            int bottom=vertices.Count;vertices.Add(new Vector3(0,-size.y/2,0));int top=vertices.Count;vertices.Add(new Vector3(0,size.y/2,0));
            for(int i=0;i<perimeter;i++) { int n=(i+1)%perimeter;triangles.AddRange(new[]{bottom,i,n,top,3*perimeter+n,3*perimeter+i}); }
            var result=new Mesh();result.SetVertices(vertices);result.SetTriangles(triangles,0);result.RecalculateNormals();return result;
        });
        return MeshObject(parent,name,pos,mesh,material);
    }
}
