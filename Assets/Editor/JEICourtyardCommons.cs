using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using F = JEIFurnitureKit;

/// <summary>A small planted courtyard island with two open, outward-facing timber bench arcs.</summary>
public static class JEICourtyardCommons
{
    const float BaseY=.075f;
    const float IslandRadius=.90f;
    const float BenchInner=1.10f,BenchOuter=1.65f;
    static readonly Vector3 Centre=new Vector3(-1.15f,0,.55f);
    static Material Stone { get { return F.Mat("C04_Warm_Limestone",new Color(.76f,.75f,.69f),.15f); } }
    static Material Rim { get { return F.Mat("C04_Cream_Stone_Rim",new Color(.84f,.83f,.76f),.19f); } }
    static Material Joint { get { return F.Mat("C04_Stone_Grout",new Color(.58f,.59f,.54f),.10f); } }
    static Material Oak { get { return F.Mat("C04_Warm_Outdoor_Oak",new Color(.65f,.47f,.28f),.17f); } }
    static Material OakLight { get { return F.Mat("C04_Light_Oak_Slats",new Color(.72f,.54f,.33f),.18f); } }
    static Material Grain { get { return F.Mat("C04_Subtle_Wood_Grain",new Color(.57f,.40f,.23f),.16f); } }
    static Material Metal { get { return F.Mat("C04_Sage_Graphite_Metal",new Color(.28f,.34f,.30f),.24f); } }
    static Material Soil { get { return F.Mat("C04_Planting_Soil",new Color(.26f,.22f,.16f),.05f); } }
    static Material Bark { get { return F.Mat("C04_Tree_Bark",new Color(.43f,.34f,.24f),.10f); } }
    static Material Branch { get { return F.Mat("C04_Young_Tree_Branches",new Color(.51f,.41f,.28f),.10f); } }
    static Material Leaf { get { return F.Mat("C04_Soft_Green_Canopy",new Color(.39f,.54f,.32f),.08f); } }
    static Material LeafLight { get { return F.Mat("C04_Light_Green_Canopy",new Color(.48f,.61f,.37f),.08f); } }
    static Material LeafDark { get { return F.Mat("C04_Deep_Green_Foliage",new Color(.29f,.44f,.28f),.08f); } }

    static GameObject B(Transform p,string name,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,name,pos,size,m,false); }
    static GameObject R(Transform p,string name,Vector3 pos,Vector3 size,Material m,float radius=.02f)
    { return F.Rounded(p,name,pos,size,m,radius); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static Transform Build(Transform parent)
    {
        var root=F.Group(parent,"C04__Courtyard_Commons");
        Paving(root);
        PlantingIsland(root);
        CurvedBench(root,"C04_Curved_bench_01",25,100);
        CurvedBench(root,"C04_Curved_bench_02",155,230);
        AddDetails(root);
        AddLighting(root);
        return root;
    }

    /// <summary>Five warm real spot lights, appended independently of established courtyard geometry.</summary>
    public static Transform AddLighting(Transform parent)
    {
        var root=F.Group(parent,"C04_Warm_Lighting");
        WallSconce(root,"C04_North_wall_sconce_01","C04_Wall_light_01",-3.55f);
        WallSconce(root,"C04_North_wall_sconce_02","C04_Wall_light_02",3.55f);
        BenchLighting(root,"C04_Bench_LED_strip_01","C04_Bench_LED_light_01",25,100);
        BenchLighting(root,"C04_Bench_LED_strip_02","C04_Bench_LED_light_02",155,230);
        ArtLighting(root);
        return root;
    }

    static Material LightingMetal { get { return F.Mat("C04_Lighting_Deep_Sage_Metal",new Color(.24f,.32f,.28f),.35f); } }
    static Material LightingCream { get { return F.Mat("C04_Lighting_Cream_Housing",new Color(.88f,.86f,.77f),.24f); } }
    static Material LightingGlow
    {
        get {
            var material=F.Mat("C04_Lighting_Warm_Emissive_Diffuser",new Color(.97f,.86f,.66f),.25f);
            material.EnableKeyword("_EMISSION");
            if(material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor",new Color(1f,.694f,.431f)*1.7f);
            material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);return material;
        }
    }

    static void WallSconce(Transform parent,string name,string lightName,float x)
    {
        var g=F.Group(parent,name,new Vector3(x,2.12f,3.09f));
        R(g,"Slim_sage_wall_plate",new Vector3(0,0,.035f),new Vector3(.23f,.34f,.035f),LightingMetal,.012f);
        foreach(float y in new[]{-.15f,.15f})
            R(g,"Deep_sage_lamp_cap",new Vector3(0,y,-.025f),new Vector3(.25f,.035f,.16f),LightingMetal,.025f);
        foreach(float edge in new[]{-.104f,.104f})
            B(g,"Slender_sconce_side_frame",new Vector3(edge,0,-.045f),new Vector3(.018f,.28f,.085f),LightingMetal);
        R(g,"Cream_inner_lamp_housing",new Vector3(0,-.005f,-.049f),new Vector3(.195f,.035f,.26f),LightingCream,.025f).transform.localRotation=Quaternion.Euler(90,0,0);
        R(g,"Warm_cream_emissive_lamp_shade",new Vector3(0,-.005f,-.075f),new Vector3(.18f,.026f,.245f),LightingGlow,.025f).transform.localRotation=Quaternion.Euler(90,0,0);
        R(g,"Cream_underside_diffuser",new Vector3(0,-.17f,-.03f),new Vector3(.17f,.012f,.09f),LightingGlow,.015f);
        float inward=x<0?.80f:-.80f;
        Spot(g,lightName,new Vector3(0,-.18f,-.07f),new Vector3(inward,-2.045f,-1.74f),4.0f,2.0f,100,58);
    }

    static void BenchLighting(Transform parent,string name,string lightName,float start,float end)
    {
        var g=F.Group(parent,name,Centre);
        Arc(g,"Hidden_curved_LED_channel",Vector3.up*.478f,1.568f,1.535f,start+4,end-4,.006f,LightingMetal);
        Arc(g,"Warm_white_emissive_LED_arc",Vector3.up*.469f,1.560f,1.544f,start+4,end-4,.009f,LightingGlow);
        float angle=(start+end)/2;
        Spot(g,lightName,Polar(1.45f,.455f,angle),Polar(1.65f,BaseY,angle),1.60f,.16f,110,70);
    }

    static void ArtLighting(Transform parent)
    {
        var g=F.Group(parent,"C04_Recessed_art_light_fixture",new Vector3(3.02f,0,1.10f));
        F.Primitive(g,"Small_flush_metal_uplight_trim",PrimitiveType.Cylinder,new Vector3(0,.727f,0),new Vector3(.078f,.005f,.078f),LightingMetal);
        F.Primitive(g,"Warm_emissive_uplight_lens",PrimitiveType.Cylinder,new Vector3(0,.732f,0),new Vector3(.056f,.0015f,.056f),LightingGlow);
        Spot(g,"C04_Art_uplight",new Vector3(0,.738f,0),new Vector3(.23f,1.30f,.30f),1.30f,1.0f,48,28);
    }

    static Light Spot(Transform parent,string name,Vector3 position,Vector3 target,float range,float intensity,float angle,float innerAngle)
    {
        var g=F.Group(parent,name,position);
        g.localRotation=Quaternion.LookRotation(target-position,Vector3.up);
        var light=g.gameObject.AddComponent<Light>();light.type=LightType.Spot;light.lightmapBakeType=LightmapBakeType.Realtime;
        light.color=Color.white;light.useColorTemperature=true;light.colorTemperature=3000;
        light.range=range;light.intensity=intensity;light.spotAngle=angle;light.innerSpotAngle=innerAngle;
        light.shadows=LightShadows.Soft;light.shadowStrength=.70f;
        light.shadowBias=.03f;light.shadowNormalBias=.06f;light.shadowNearPlane=.03f;
        // Five modest local shadow maps fit the existing atlas without changing
        // pipeline assets or asking URP to downgrade the whole set at render time.
        var data=light.GetUniversalAdditionalLightData();data.usePipelineSettings=false;
        var serialized=new SerializedObject(data);
        serialized.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierMedium;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return light;
    }

    /// <summary>Append-only detail group, also usable without rebuilding the established courtyard.</summary>
    public static Transform AddDetails(Transform parent)
    {
        var root=F.Group(parent,"C04_Art_and_Corner_Planting");
        StudentArtDisplay(root);
        CornerPlanter(root,"C04_Corner_planter_01",new Vector3(2.85f,0,2.80f),false);
        CornerPlanter(root,"C04_Corner_planter_02",new Vector3(2.85f,0,-2.80f),true);
        return root;
    }

    static Material ArtStone { get { return F.Mat("C04_Art_Warm_Ivory_Plinth",new Color(.85f,.84f,.77f),.18f); } }
    static Material ArtRecess { get { return F.Mat("C04_Art_Recessed_Stone_Base",new Color(.63f,.64f,.59f),.15f); } }
    static Material ArtClay { get { return F.Mat("C04_Art_Warm_Clay_Ring",new Color(.66f,.38f,.26f),.27f); } }
    static Material ArtSage { get { return F.Mat("C04_Art_Sage_Metal_Ring",new Color(.37f,.49f,.43f),.46f); } }
    static Material ArtLabel { get { return F.Mat("C04_Art_Label_Ivory",new Color(.94f,.92f,.84f),.12f); } }
    static Material CornerStone { get { return F.Mat("C04_Corner_Cream_Stone",new Color(.84f,.83f,.76f),.18f); } }
    static Material CornerSoil { get { return F.Mat("C04_Corner_Recessed_Soil",new Color(.26f,.22f,.16f),.05f); } }
    static Material CornerGrass { get { return F.Mat("C04_Corner_Ornamental_Grass",new Color(.43f,.55f,.34f),.08f); } }
    static Material CornerGrassLight { get { return F.Mat("C04_Corner_Grass_Highlights",new Color(.56f,.64f,.40f),.08f); } }
    static Material CornerGround { get { return F.Mat("C04_Corner_Low_Groundcover",new Color(.34f,.48f,.32f),.08f); } }
    static Material CornerSeed { get { return F.Mat("C04_Corner_Soft_Seedheads",new Color(.68f,.64f,.44f),.10f); } }

    static void StudentArtDisplay(Transform parent)
    {
        var g=F.Group(parent,"C04_Student_art_display",new Vector3(3.25f,0,1.35f),90);
        R(g,"Recessed_stone_foot",new Vector3(0,.09f,0),new Vector3(.74f,.03f,.60f),ArtRecess,.025f);
        R(g,"Quiet_ivory_exhibition_pedestal",new Vector3(0,.40f,0),new Vector3(.80f,.61f,.66f),ArtStone,.035f);
        R(g,"Fine_stone_display_top__worldH0.725m",new Vector3(0,.705f,0),new Vector3(.86f,.04f,.72f),ArtStone,.03f);
        R(g,"Small_sculpture_mount",new Vector3(-.035f,.740f,0),new Vector3(.43f,.03f,.24f),ArtSage,.025f);

        // Two genuinely hollow oval tubes interlock in perpendicular planes.
        // The primary rests on the mount; the fine welded stem carries the
        // upper ring and keeps this small work visibly grounded on the plinth.
        DetailMesh(g,"Connections_primary_terracotta_oval",new Vector3(-.08f,1.18f,0),OvalTube("C04_Art_Tall_Oval_Tube",.22f,.40f,.027f),ArtClay);
        var sage=DetailMesh(g,"Connections_interlocking_sage_oval",new Vector3(.07f,1.395f,0),OvalTube("C04_Art_Short_Oval_Tube",.18f,.265f,.027f),ArtSage);
        sage.transform.localRotation=Quaternion.Euler(0,90,0);
        Segment(g,"Fine_sculpture_support_weld",new Vector3(.07f,.752f,0),new Vector3(.07f,1.115f,0),.024f,ArtSage);
        R(g,"Inset_student_work_label",new Vector3(0,.43f,-.347f),new Vector3(.64f,.17f,.008f),ArtLabel,.01f);
        F.WallText(g,"STUDENT WORK\nCONNECTIONS",new Vector3(0,.43f,-.354f),0,.52f,new Color(.04f,.055f,.05f));
        // After yaw90: x[2.89,3.61], z[.92,1.78], y[.075,1.75].
        Solid(g,new Vector3(0,.9125f,0),new Vector3(.86f,1.675f,.72f));
    }

    static void CornerPlanter(Transform parent,string name,Vector3 pos,bool alternate)
    {
        var g=F.Group(parent,name,pos);
        R(g,"Low_stone_trough_base",new Vector3(0,.09f,0),new Vector3(1.60f,.03f,.45f),CornerStone,.025f);
        foreach(float z in new[]{-.2325f,.2325f})
            B(g,"Long_cream_stone_wall",new Vector3(0,.2275f,z),new Vector3(1.65f,.245f,.035f),CornerStone);
        foreach(float x in new[]{-.8075f,.8075f})
            B(g,"Stone_trough_end",new Vector3(x,.2275f,0),new Vector3(.035f,.245f,.43f),CornerStone);
        foreach(float z in new[]{-.2225f,.2225f})
            R(g,"Soft_long_coping",new Vector3(0,.3625f,z),new Vector3(1.65f,.025f,.055f),CornerStone,.012f);
        foreach(float x in new[]{-.7975f,.7975f})
            R(g,"Soft_end_coping",new Vector3(x,.3625f,0),new Vector3(.055f,.025f,.39f),CornerStone,.012f);
        B(g,"Recessed_planting_bed",new Vector3(0,.3375f,0),new Vector3(1.52f,.015f,.37f),CornerSoil);

        var blade=GrassBlade();
        int tuftNumber=0;
        foreach(float x in new[]{-.61f,-.30f,.02f,.34f,.63f}) {
            tuftNumber++;
            float z=tuftNumber%2==0?.045f:-.035f;
            var tuft=F.Group(g,"Ornamental_grass_tuft_"+tuftNumber,new Vector3(x,.345f,z),alternate?37:0);
            for(int i=0;i<7;i++) {
                var leaf=DetailMesh(tuft,"Fine_arching_grass_blade_"+(i+1),Vector3.zero,blade,i%3==0?CornerGrassLight:CornerGrass);
                leaf.transform.localRotation=Quaternion.Euler(0,i*137.5f+tuftNumber*23,0);
                leaf.transform.localScale=new Vector3(1,.80f+((i+tuftNumber)%4)*.06f,1);
            }
            if(tuftNumber==2 || tuftNumber==4) {
                Segment(tuft,"Slender_seedhead_stem",Vector3.zero,new Vector3(.015f,.43f,.025f),.007f,CornerGrassLight);
                var seed=F.Primitive(tuft,"Soft_ornamental_grass_seedhead",PrimitiveType.Sphere,new Vector3(.015f,.447f,.025f),new Vector3(.027f,.067f,.027f),CornerSeed);
                seed.transform.localRotation=Quaternion.Euler(8,0,-5);
            }
        }
        foreach(float x in new[]{-.52f,0,.53f})
            F.Primitive(g,"Low_cushion_groundcover",PrimitiveType.Sphere,new Vector3(x,.391f,.025f),new Vector3(.25f,.12f,.19f),CornerGround);
        // Foliage stays inside the .50m deep trough footprint and below .85m.
        Solid(g,new Vector3(0,.225f,0),new Vector3(1.65f,.30f,.50f));
    }

    static GameObject DetailMesh(Transform parent,string name,Vector3 pos,Mesh mesh,Material material)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=material;return g;
    }

    static Mesh DetailMeshAsset(string name,System.Func<Mesh> create)
    {
        const string root="Assets/JEI_Furnishings",folder=root+"/Meshes";
        if(!AssetDatabase.IsValidFolder(root)) AssetDatabase.CreateFolder("Assets","JEI_Furnishings");
        if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(root,"Meshes");
        string path=folder+"/"+name+".asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh) return mesh;
        mesh=create();mesh.name=name;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }

    static Mesh OvalTube(string name,float radiusX,float radiusY,float tube)
    {
        return DetailMeshAsset(name,()=>{
            const int around=64,sides=12;
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<around;i++) {
                float a=i*2*Mathf.PI/around;
                Vector3 centre=new Vector3(radiusX*Mathf.Cos(a),radiusY*Mathf.Sin(a),0);
                Vector3 outward=new Vector3(radiusY*Mathf.Cos(a),radiusX*Mathf.Sin(a),0).normalized;
                for(int j=0;j<sides;j++) {
                    float b=j*2*Mathf.PI/sides;
                    Vector3 normal=outward*Mathf.Cos(b)+Vector3.forward*Mathf.Sin(b);
                    vertices.Add(centre+normal*tube);normals.Add(normal);
                }
            }
            for(int i=0;i<around;i++) for(int j=0;j<sides;j++) {
                int a=i*sides+j,b=((i+1)%around)*sides+j,c=i*sides+(j+1)%sides,d=((i+1)%around)*sides+(j+1)%sides;
                triangles.AddRange(new[]{a,b,c,c,b,d});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);return mesh;
        });
    }

    static Mesh GrassBlade()
    {
        return DetailMeshAsset("C04_Corner_Arching_Grass_Blade",()=>{
            const int sections=8;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=sections;i++) {
                float t=(float)i/sections;
                float halfWidth=Mathf.Max(.0006f,Mathf.Sin((.15f+.85f*t)*Mathf.PI)*.013f);
                float y=.43f*t,z=.11f*t*t;
                vertices.Add(new Vector3(-halfWidth,y,z-.0015f));vertices.Add(new Vector3(halfWidth,y,z-.0015f));
                vertices.Add(new Vector3(-halfWidth,y,z+.0015f));vertices.Add(new Vector3(halfWidth,y,z+.0015f));
            }
            for(int i=0;i<sections;i++) {
                int a=i*4,b=(i+1)*4;
                triangles.AddRange(new[]{a,b,a+1,a+1,b,b+1,a+2,a+3,b+2,a+3,b+3,b+2,a,a+2,b,a+2,b+2,b,a+1,b+1,a+3,a+3,b+1,b+3});
            }
            int e=sections*4;triangles.AddRange(new[]{0,1,2,1,3,2,e,e+2,e+1,e+1,e+2,e+3});
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        });
    }

    static void Paving(Transform parent)
    {
        var g=F.Group(parent,"C04_Stone_paving");
        B(g,"Shallow_warm_stone_paving__7.78x6.38m",new Vector3(0,.0625f,0),new Vector3(7.78f,.025f,6.38f),Stone);
        foreach(float x in new[]{-2.60f,-1.30f,0,1.30f,2.60f})
            B(g,"Fine_north_south_stone_joint",new Vector3(x,.0754f,0),new Vector3(.007f,.001f,6.38f),Joint);
        foreach(float z in new[]{-2.12f,-1.06f,0,1.06f,2.12f})
            B(g,"Fine_east_west_stone_joint",new Vector3(0,.0754f,z),new Vector3(7.78f,.001f,.007f),Joint);
        Solid(g,new Vector3(0,.0625f,0),new Vector3(7.78f,.025f,6.38f));
    }

    static void PlantingIsland(Transform parent)
    {
        var g=F.Group(parent,"C04_Low_planting_island",Centre+Vector3.up*BaseY);
        F.Primitive(g,"Inset_stone_base",PrimitiveType.Cylinder,new Vector3(0,.020f,0),new Vector3(1.72f,.020f,1.72f),Stone);
        Arc(g,"Low_circular_stone_planter_wall",Vector3.up*.035f,.89f,.79f,0,360,.29f,Stone);
        Arc(g,"Soft_cream_stone_coping",Vector3.up*.32f,IslandRadius,.775f,0,360,.03f,Rim);
        F.Primitive(g,"Recessed_soil_surface",PrimitiveType.Cylinder,new Vector3(0,.309f,0),new Vector3(1.55f,.008f,1.55f),Soil);

        // Unity has no CylinderCollider. A scaled built-in cylinder gives an
        // exact low round obstacle; a vertical capsule would be at least 1.8m tall.
        var collision=F.Primitive(g,"Circular_planter_collision__R0.90_H0.35",PrimitiveType.Cylinder,new Vector3(0,.175f,0),new Vector3(1.80f,.175f,1.80f),Stone);
        var mesh=collision.GetComponent<MeshFilter>().sharedMesh;
        UnityEngine.Object.DestroyImmediate(collision.GetComponent<MeshRenderer>());
        var collider=collision.AddComponent<MeshCollider>();collider.sharedMesh=mesh;collider.convex=true;
        CourtyardTree(g);
        Underplanting(g);
    }

    static void CourtyardTree(Transform parent)
    {
        var g=F.Group(parent,"C04_Courtyard_tree");
        Segment(g,"Lower_gently_bending_trunk",new Vector3(0,.31f,0),new Vector3(.03f,1.00f,-.02f),.125f,Bark);
        Segment(g,"Middle_trunk",new Vector3(.03f,1.00f,-.02f),new Vector3(-.04f,1.55f,.03f),.100f,Bark);
        Segment(g,"Upper_tapering_trunk",new Vector3(-.04f,1.55f,.03f),new Vector3(.04f,2.10f,.02f),.075f,Branch);
        BranchPair(g,"West_branch",new Vector3(-.02f,1.30f,.02f),new Vector3(-.40f,1.75f,.12f),new Vector3(-.60f,2.10f,.04f));
        BranchPair(g,"East_branch",new Vector3(.01f,1.48f,0),new Vector3(.33f,1.90f,-.08f),new Vector3(.50f,2.20f,-.04f));
        BranchPair(g,"North_branch",new Vector3(0,1.65f,0),new Vector3(-.10f,2.05f,.33f),new Vector3(-.18f,2.30f,.53f));
        BranchPair(g,"South_branch",new Vector3(-.03f,1.50f,.02f),new Vector3(.18f,1.98f,-.34f),new Vector3(.24f,2.33f,-.55f));
        BranchPair(g,"High_east_branch",new Vector3(.03f,1.85f,0),new Vector3(.39f,2.20f,.20f),new Vector3(.50f,2.43f,.34f));

        // Several offset, overlapping crowns have a natural uneven silhouette.
        // Each projected cluster remains within R1.15; highest worldY is 2.935.
        Crown(g,"West_low_crown",new Vector3(-.52f,2.10f,.02f),new Vector3(.85f,.74f,.70f),LeafDark,12);
        Crown(g,"East_low_crown",new Vector3(.48f,2.14f,-.02f),new Vector3(.90f,.72f,.76f),Leaf,48);
        Crown(g,"North_crown",new Vector3(-.18f,2.26f,.54f),new Vector3(.82f,.70f,.78f),LeafLight,103);
        Crown(g,"South_crown",new Vector3(.18f,2.32f,-.55f),new Vector3(.78f,.72f,.77f),Leaf,19);
        Crown(g,"High_west_crown",new Vector3(-.52f,2.44f,-.33f),new Vector3(.76f,.65f,.73f),Leaf,62);
        Crown(g,"High_east_crown",new Vector3(.51f,2.42f,.34f),new Vector3(.77f,.66f,.74f),LeafLight,116);
        Crown(g,"Irregular_top_crown",new Vector3(-.08f,2.54f,.10f),new Vector3(.94f,.64f,.86f),LeafLight,33);
        Crown(g,"Small_outer_leaf_cluster",new Vector3(.68f,2.28f,.17f),new Vector3(.46f,.46f,.44f),Leaf,70);
        Crown(g,"Small_low_leaf_cluster",new Vector3(-.37f,1.94f,.40f),new Vector3(.44f,.48f,.43f),LeafDark,26);
    }

    static void BranchPair(Transform parent,string name,Vector3 start,Vector3 fork,Vector3 tip)
    {
        Segment(parent,name+"_primary",start,fork,.052f,Bark);
        Segment(parent,name+"_fine_tip",fork,tip,.030f,Branch);
    }
    static void Segment(Transform parent,string name,Vector3 from,Vector3 to,float diameter,Material m)
    {
        var piece=F.Primitive(parent,name,PrimitiveType.Cylinder,(from+to)/2,new Vector3(diameter,Vector3.Distance(from,to)/2,diameter),m);
        piece.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
    }
    static void Crown(Transform parent,string name,Vector3 pos,Vector3 size,Material m,float yaw)
    {
        var crown=F.Primitive(parent,name,PrimitiveType.Sphere,pos,size,m);
        crown.transform.localRotation=Quaternion.Euler(0,yaw,0);
    }

    static void Underplanting(Transform parent)
    {
        var g=F.Group(parent,"C04_Underplanting");
        for(int i=0;i<7;i++) {
            float angle=(i*137.5f+10)*Mathf.Deg2Rad;
            float radius=i%2==0?.53f:.60f;
            var tuft=F.Group(g,"Low_evergreen_tuft_"+(i+1),new Vector3(Mathf.Cos(angle)*radius,.31f,Mathf.Sin(angle)*radius),i*47);
            for(int leaf=0;leaf<4;leaf++) {
                float a=leaf*90+20;
                var shape=F.Primitive(tuft,"Small_lanceolate_leaf",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a*Mathf.Deg2Rad)*.045f,.125f,Mathf.Sin(a*Mathf.Deg2Rad)*.045f),new Vector3(.048f,.27f,.095f),leaf%2==0?LeafDark:Leaf);
                shape.transform.localRotation=Quaternion.Euler(23,a,12);
            }
        }
        foreach(float angle in new[]{35f,130f,235f}) {
            float a=angle*Mathf.Deg2Rad;
            F.Primitive(g,"Small_natural_stone",PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.42f,.321f,Mathf.Sin(a)*.42f),new Vector3(.13f,.055f,.09f),Stone);
        }
    }

    static void CurvedBench(Transform parent,string name,float start,float end)
    {
        var g=F.Group(parent,name,Centre);
        // Seven separate curved timber strips, with open drainage gaps.
        for(int i=0;i<7;i++) {
            float inner=BenchInner+.001f+i*.080f,outer=inner+.068f;
            Arc(g,"Individual_curved_oak_seat_slat_"+(i+1),Vector3.up*(BaseY+.435f),outer,inner,start,end,.035f,i%2==0?OakLight:Oak);
            float grainRadius=(inner+outer)/2;
            Arc(g,"Fine_curving_wood_grain_"+(i+1),Vector3.up*(BaseY+.4703f),grainRadius+.001f,grainRadius-.001f,start+.6f,end-.6f,.0008f,Grain);
        }
        foreach(float radius in new[]{1.17f,1.57f})
            Arc(g,"Light_curved_metal_underframe",Vector3.up*(BaseY+.407f),radius+.015f,radius-.015f,start+2,end-2,.028f,Metal);
        foreach(float angle in new[]{start+7,(start+end)/2,end-7}) {
            var support=F.Group(g,"Radial_leg_pair",Polar(1.375f,0,angle),90-angle);
            B(support,"Slim_radial_seat_support",new Vector3(0,BaseY+.421f,0),new Vector3(.038f,.028f,.49f),Metal);
            foreach(float radial in new[]{-.195f,.195f}) {
                B(support,"Slender_metal_leg",new Vector3(0,BaseY+.225f,radial),new Vector3(.040f,.420f,.040f),Metal);
                R(support,"Quiet_foot_pad",new Vector3(0,BaseY+.0075f,radial),new Vector3(.055f,.015f,.055f),Metal,.01f);
            }
        }
        // The back is on the inner edge, so seated visitors face the open court.
        foreach(float y in new[]{.59f,.7325f,.875f})
            Arc(g,"Thin_curved_timber_back_rail",Vector3.up*(BaseY+y),1.155f,1.11f,start+3,end-3,.075f,OakLight);
        foreach(float angle in new[]{start+6,(start+end)/2,end-6})
            Segment(g,"Fine_inner_backrest_stay",Polar(1.13f,BaseY+.425f,angle),Polar(1.13f,BaseY+.93f,angle),.026f,Metal);

        // A single AABB would incorrectly fill the open outside of this arc.
        // Eight shallow tangent boxes preserve it; outer corner R is <1.656m.
        int pieces=Mathf.CeilToInt((end-start)/10);
        float step=(end-start)/pieces,half=step*Mathf.Deg2Rad/2;
        float innerProjected=BenchInner*Mathf.Cos(half);
        float mid=(innerProjected+BenchOuter)/2;
        float radialDepth=BenchOuter-innerProjected;
        float tangentWidth=2*BenchOuter*Mathf.Sin(half);
        for(int i=0;i<pieces;i++) {
            float angle=start+(i+.5f)*step;
            var segment=F.Group(g,"Arc_collision_segment_"+(i+1).ToString("00"),Polar(mid,0,angle),90-angle);
            Solid(segment,new Vector3(0,BaseY+.475f,0),new Vector3(tangentWidth,.95f,radialDepth));
        }
    }

    static Vector3 Polar(float radius,float y,float angle)
    { float a=angle*Mathf.Deg2Rad;return new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius); }
    static GameObject Arc(Transform parent,string name,Vector3 pos,float outer,float inner,float start,float end,float height,Material m)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;
        g.AddComponent<MeshFilter>().sharedMesh=F.RingMesh(outer,inner,start,end,height);
        g.AddComponent<MeshRenderer>().sharedMaterial=m;return g;
    }
}
