using UnityEditor;
using UnityEngine;
using F = JEIFurnitureKit;
using K = JEIRestroomFixtureKit;

/// <summary>C02/C03 shared restrooms, with west-facing cubicle backs and open central circulation.</summary>
public static class JEIRestroomRooms
{
    const float FloorTop=.001f;
    const float FrontX=-10.50f;
    static Material Cream(string r) { return F.Mat(r+"_Cream_Wainscot",new Color(.88f,.88f,.82f),.22f); }
    static Material Sage(string r) { return F.Mat(r+"_Sage_Partition",new Color(.56f,.66f,.60f),.20f); }
    static Material Frame(string r) { return F.Mat(r+"_Sage_Metal_Frame",new Color(.36f,.46f,.40f),.32f); }
    static Material Dark(string r) { return F.Mat(r+"_Dark_Fitting",new Color(.16f,.23f,.20f),.24f); }
    static Material Metal(string r) { return F.Mat(r+"_Satin_Hardware",new Color(.62f,.68f,.66f),.55f); }
    static Material Tile(string r) { return F.Mat(r+"_Non_slip_Grey_Tile",new Color(.74f,.77f,.74f),.12f); }
    static Material Grout(string r) { return F.Mat(r+"_Fine_Tile_Grout",new Color(.61f,.65f,.61f),.08f); }
    static Material Paper(string r) { return F.Mat(r+"_White_Tissue",new Color(.97f,.96f,.90f),.08f); }
    static Material Green(string r) { return F.Mat(r+"_Vacant_Indicator",new Color(.31f,.62f,.42f),.18f); }
    static GameObject B(Transform p,string name,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,name,pos,size,m,false); }
    static GameObject R(Transform p,string name,Vector3 pos,Vector3 size,Material m,float radius=.018f)
    { return F.Rounded(p,name,pos,size,m,radius); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static Transform BuildWomen(Transform parent)
    {
        const string r="C02";
        var room=F.Group(parent,"C02__Womens_restroom");
        Floor(room,r,1.65f);
        Wainscot(room,r,.10f,3.20f,true);
        int number=0;
        foreach(float low in new[]{.10f,1.15f,2.20f}) {
            number++;
            Cubicle(room,r,number,low);
            K.Toilet(room,"C02_Toilet_"+number.ToString("00"),new Vector3(-11.84f,FloorTop,low+.50f),270);
        }
        Divider(room,r,"C02_Shared_partition_01",1.125f);
        Divider(room,r,"C02_Shared_partition_02",2.175f);
        number=0;
        foreach(float z in new[]{.90f,2.20f}) {
            number++;
            K.Basin(room,"C02_Basin_"+number.ToString("00"),new Vector3(-6.98f,FloorTop,z),90);
            BasinMounts(room,r,new Vector3(-6.98f,FloorTop,z),90);
            Mirror(room,r,"C02_Mirror_"+number.ToString("00"),new Vector3(-6.721f,0,z),90);
        }
        Soap(room,r,new Vector3(-6.74f,0,1.55f),90);
        Towels(room,r,new Vector3(-6.75f,0,2.85f),90);
        Bin(room,r,new Vector3(-6.94f,0,2.90f));
        RoomSign(room,r,"WOMEN",new Vector3(-8.65f,1.55f,3.178f),0);
        RoomSign(room,r,"WOMEN",new Vector3(-8.65f,1.55f,3.408f),180,true);
        return room;
    }

    public static Transform BuildMen(Transform parent)
    {
        const string r="C03";
        var room=F.Group(parent,"C03__Mens_restroom");
        Floor(room,r,-1.65f);
        Wainscot(room,r,-3.20f,-.10f,false);
        int number=0;
        foreach(float low in new[]{-1.10f,-2.15f}) {
            number++;
            Cubicle(room,r,number,low);
            K.Toilet(room,"C03_Toilet_"+number.ToString("00"),new Vector3(-11.84f,FloorTop,low+.50f),270);
        }
        Divider(room,r,"C03_Shared_partition",-1.125f);
        Divider(room,r,"C03_South_end_partition",-2.175f);
        number=0;
        foreach(float x in new[]{-9.30f,-8.25f}) {
            number++;
            K.Basin(room,"C03_Basin_"+number.ToString("00"),new Vector3(x,FloorTop,-.39f),0);
            BasinMounts(room,r,new Vector3(x,FloorTop,-.39f),0);
            Mirror(room,r,"C03_Mirror_"+number.ToString("00"),new Vector3(x,0,-.119f),0);
        }
        K.Urinal(room,"C03_Urinal_01",new Vector3(-6.925f,FloorTop-.10f,-.95f),90);
        K.Urinal(room,"C03_Urinal_02",new Vector3(-6.925f,FloorTop,-2.05f),90);
        UrinalDivider(room,r);
        PrivacyScreen(room,r);
        Soap(room,r,new Vector3(-8.775f,0,-.15f),0);
        Towels(room,r,new Vector3(-7.45f,0,-.16f),0);
        Bin(room,r,new Vector3(-7.60f,0,-.35f));
        RoomSign(room,r,"MEN",new Vector3(-8.65f,1.55f,-3.178f),180);
        RoomSign(room,r,"MEN",new Vector3(-8.65f,1.55f,-3.408f),0,true);
        return room;
    }

    static void Floor(Transform parent,string r,float centreZ)
    {
        var g=F.Group(parent,r+"_Non_slip_tile_floor",new Vector3(-9.50f,0,centreZ));
        B(g,"Thin_tile_finish__top_Y0.001m",new Vector3(0,-.0115f,0),new Vector3(5.58f,.025f,3.08f),Tile(r));
        foreach(float x in new[]{-2.0f,-1.0f,0,1.0f,2.0f})
            B(g,"Fine_tile_joint",new Vector3(x,.0012f,0),new Vector3(.004f,.0004f,3.08f),Grout(r));
        foreach(float z in new[]{-1.0f,-.5f,0,.5f,1.0f})
            B(g,"Fine_tile_joint",new Vector3(0,.0012f,z),new Vector3(5.58f,.0004f,.004f),Grout(r));
        Solid(g,new Vector3(0,-.0115f,0),new Vector3(5.58f,.025f,3.08f));
    }

    static void Wainscot(Transform parent,string r,float minZ,float maxZ,bool northDoor)
    {
        var g=F.Group(parent,r+"_Cream_lower_wall_protection");
        float centreZ=(minZ+maxZ)/2,depth=maxZ-minZ-.02f;
        foreach(float x in new[]{-12.292f,-6.708f}) {
            B(g,"Cream_side_wall_panel",new Vector3(x,FloorTop+.55f,centreZ),new Vector3(.016f,1.10f,depth),Cream(r));
            B(g,"Sage_side_wall_skirt",new Vector3(x,FloorTop+.045f,centreZ),new Vector3(.016f,.09f,depth),Frame(r));
        }
        float fullWall=northDoor?minZ+.008f:maxZ-.008f;
        B(g,"Cream_full_end_wall_panel",new Vector3(-9.50f,FloorTop+.55f,fullWall),new Vector3(5.58f,1.10f,.016f),Cream(r));
        B(g,"Sage_full_end_wall_skirt",new Vector3(-9.50f,FloorTop+.045f,fullWall),new Vector3(5.58f,.09f,.016f),Frame(r));
        float doorWall=northDoor?maxZ-.008f:minZ+.008f;
        foreach(float x in new[]{-11.155f,-7.845f}) {
            B(g,"Door_flanking_cream_panel",new Vector3(x,FloorTop+.55f,doorWall),new Vector3(2.27f,1.10f,.016f),Cream(r));
            B(g,"Door_flanking_sage_skirt",new Vector3(x,FloorTop+.045f,doorWall),new Vector3(2.27f,.09f,.016f),Frame(r));
        }
    }

    static void Divider(Transform parent,string r,string name,float z)
    {
        const float backX=-12.29f;
        var g=F.Group(parent,name,new Vector3((backX+FrontX)/2,0,z));
        float depth=FrontX-backX;
        B(g,"Sage_cubicle_side_panel__bottom_gap_0.15m",new Vector3(0,FloorTop+1.10f,0),new Vector3(depth,1.90f,.05f),Sage(r));
        B(g,"Fine_panel_top_rail",new Vector3(0,FloorTop+2.025f,0),new Vector3(depth,.05f,.05f),Frame(r));
        foreach(float x in new[]{-depth/2+.09f,depth/2-.09f}) {
            B(g,"Adjustable_partition_leg",new Vector3(x,FloorTop+.075f,0),new Vector3(.032f,.15f,.032f),Metal(r));
            R(g,"Bolted_floor_pad",new Vector3(x,FloorTop+.009f,0),new Vector3(.055f,.018f,.045f),Frame(r),.009f);
        }
        Solid(g,new Vector3(0,FloorTop+1.10f,0),new Vector3(depth,1.90f,.05f));
    }

    static void Cubicle(Transform parent,string r,int number,float lowZ)
    {
        var g=F.Group(parent,r+"_Cubicle_"+number.ToString("00"));
        var front=F.Group(g,"Front_posts_and_header",new Vector3(FrontX,0,lowZ));
        foreach(Vector2 post in new[]{new Vector2(.04f,.08f),new Vector2(.93f,.14f)}) {
            var side=F.Group(front,"Closed_front_jamb",new Vector3(0,0,post.x));
            B(side,"Sage_front_post",new Vector3(0,FloorTop+1.10f,0),new Vector3(.05f,1.90f,post.y),Sage(r));
            B(side,"Short_metal_post_foot",new Vector3(0,FloorTop+.075f,0),new Vector3(.04f,.15f,Mathf.Min(post.y,.06f)),Frame(r));
            Solid(side,new Vector3(0,FloorTop+1.10f,0),new Vector3(.05f,1.90f,post.y));
        }
        B(front,"Slim_cubicle_header",new Vector3(0,FloorTop+2.015f,.50f),new Vector3(.055f,.085f,1.0f),Frame(r));
        F.WallText(front,number.ToString("00"),new Vector3(.032f,FloorTop+2.015f,.50f),270,.17f,new Color(.93f,.95f,.89f));
        var door=F.Group(g,r+"_Cubicle_"+number.ToString("00")+"_Door",new Vector3(FrontX,0,lowZ+.08f),180);
        R(door,"Door_leaf__fixed_open_inward_90_degrees",new Vector3(.39f,FloorTop+1.05f,0),new Vector3(.78f,1.80f,.035f),Sage(r),.014f);
        foreach(float y in new[]{.48f,1.55f})
            R(door,"Visible_barrel_hinge",new Vector3(.012f,FloorTop+y,.018f),new Vector3(.028f,.09f,.03f),Metal(r),.009f);
        foreach(float side in new[]{-1f,1f}) {
            var rose=F.Primitive(door,"Lever_handle_rosette",PrimitiveType.Cylinder,new Vector3(.65f,FloorTop+.96f,side*.025f),new Vector3(.055f,.004f,.055f),Metal(r));
            rose.transform.localRotation=Quaternion.Euler(90,0,0);
            R(door,"Actual_door_lever_handle",new Vector3(.625f,FloorTop+.96f,side*.034f),new Vector3(.105f,.018f,.014f),Metal(r),.008f);
        }
        R(door,"Privacy_turn_lock_plate",new Vector3(.65f,FloorTop+1.10f,-.025f),new Vector3(.078f,.12f,.012f),Cream(r),.012f);
        F.Primitive(door,"Vacant_lock_indicator",PrimitiveType.Sphere,new Vector3(.65f,FloorTop+1.125f,-.034f),new Vector3(.025f,.025f,.006f),Green(r));
        B(door,"Turn_lock_thumb_piece",new Vector3(.65f,FloorTop+1.075f,.027f),new Vector3(.039f,.016f,.017f),Metal(r));
        B(door,"Inner_coat_hook_backplate",new Vector3(.20f,FloorTop+1.66f,.022f),new Vector3(.04f,.07f,.01f),Metal(r));
        B(door,"Short_inner_coat_hook",new Vector3(.20f,FloorTop+1.66f,.035f),new Vector3(.015f,.018f,.026f),Metal(r));
        Solid(door,new Vector3(.39f,FloorTop+1.05f,0),new Vector3(.78f,1.80f,.035f));
        ToiletPaper(g,r,new Vector3(FrontX-.85f,FloorTop+.65f,lowZ+.010f));
    }

    static void ToiletPaper(Transform parent,string r,Vector3 pos)
    {
        var g=F.Group(parent,r+"_Cubicle_toilet_paper",pos);
        R(g,"Wall_paper_holder_mount",Vector3.zero,new Vector3(.13f,.15f,.02f),Frame(r),.012f);
        B(g,"Paper_spindle_support",new Vector3(0,0,.06f),new Vector3(.02f,.025f,.12f),Metal(r));
        var roll=F.Primitive(g,"Real_tissue_roll",PrimitiveType.Cylinder,new Vector3(0,0,.095f),new Vector3(.105f,.038f,.105f),Paper(r));
        roll.transform.localRotation=Quaternion.Euler(90,0,0);
        B(g,"Hanging_tissue_sheet",new Vector3(.043f,-.085f,.095f),new Vector3(.005f,.13f,.075f),Paper(r));
    }

    static void PrivacyScreen(Transform parent,string r)
    {
        var g=F.Group(parent,"C03_Entry_privacy_screen",new Vector3(-8.10f,0,-2.175f));
        B(g,"Sage_entry_sightline_screen",new Vector3(0,FloorTop+1.0f,0),new Vector3(.06f,1.70f,1.25f),Sage(r));
        foreach(float z in new[]{-.545f,.545f}) {
            B(g,"Bolted_screen_support",new Vector3(0,FloorTop+.925f,z),new Vector3(.035f,1.85f,.035f),Frame(r));
            R(g,"Narrow_floor_anchor_plate",new Vector3(0,FloorTop+.016f,z),new Vector3(.06f,.032f,.12f),Metal(r),.008f);
        }
        Solid(g,new Vector3(0,FloorTop+.925f,0),new Vector3(.06f,1.85f,1.25f));
    }

    static void UrinalDivider(Transform parent,string r)
    {
        var g=F.Group(parent,"C03_Urinal_modesty_divider",new Vector3(-6.93f,0,-1.50f));
        R(g,"Soft_corner_wall_mounted_modesty_panel",new Vector3(0,FloorTop+.90f,0),new Vector3(.43f,1.10f,.045f),Sage(r),.018f);
        foreach(float y in new[]{.65f,1.20f})
            B(g,"Small_wall_mounting_bracket",new Vector3(.16f,FloorTop+y,0),new Vector3(.09f,.035f,.035f),Metal(r));
        Solid(g,new Vector3(0,FloorTop+.90f,0),new Vector3(.43f,1.10f,.045f));
    }

    static void Mirror(Transform parent,string r,string name,Vector3 pos,float yaw)
    {
        var g=F.Group(parent,name,pos,yaw);
        R(g,"Sage_mirror_edge",new Vector3(0,FloorTop+1.50f,0),new Vector3(.66f,.035f,.82f),Frame(r),.025f).transform.localRotation=Quaternion.Euler(90,0,0);
        var glass=F.Mat(r+"_Soft_Silver_Mirror",new Color(.68f,.77f,.77f),.96f);
        if(glass.HasProperty("_Metallic")) glass.SetFloat("_Metallic",.82f);
        EditorUtility.SetDirty(glass);
        B(g,"Mirror_face",new Vector3(0,FloorTop+1.50f,-.022f),new Vector3(.61f,.77f,.006f),glass);
    }

    static void BasinMounts(Transform parent,string r,Vector3 pos,float yaw)
    {
        var g=F.Group(parent,r+"_Basin_wall_spacers",pos,yaw);
        foreach(float x in new[]{-.18f,.18f})
            B(g,"Cream_wall_mounting_spacer",new Vector3(x,.735f,.255f),new Vector3(.055f,.07f,.05f),Cream(r));
    }

    static void Soap(Transform parent,string r,Vector3 pos,float yaw)
    {
        var g=F.Group(parent,r+"_Shared_soap_dispenser",pos,yaw);
        R(g,"Cream_soap_housing",new Vector3(0,FloorTop+1.03f,-.02f),new Vector3(.13f,.22f,.08f),Cream(r),.018f);
        B(g,"Sage_soap_level_window",new Vector3(0,FloorTop+1.01f,-.062f),new Vector3(.045f,.095f,.004f),Sage(r));
        B(g,"Sensor_and_soap_outlet",new Vector3(0,FloorTop+.915f,-.052f),new Vector3(.028f,.025f,.028f),Dark(r));
    }

    static void Towels(Transform parent,string r,Vector3 pos,float yaw)
    {
        var g=F.Group(parent,r+"_Paper_towel_dispenser",pos,yaw);
        R(g,"Cream_towel_dispenser",new Vector3(0,FloorTop+1.20f,-.015f),new Vector3(.24f,.31f,.10f),Cream(r),.02f);
        B(g,"Dark_paper_outlet",new Vector3(0,FloorTop+1.08f,-.068f),new Vector3(.19f,.018f,.008f),Dark(r));
        B(g,"Visible_towel_sheet",new Vector3(0,FloorTop+.995f,-.075f),new Vector3(.16f,.15f,.004f),Paper(r));
    }

    static void Bin(Transform parent,string r,Vector3 pos)
    {
        var g=F.Group(parent,r+"_Small_closed_waste_bin",pos);
        R(g,"Closed_bin_body",new Vector3(0,FloorTop+.17f,0),new Vector3(.27f,.34f,.29f),Cream(r),.025f);
        R(g,"Sage_bin_lid",new Vector3(0,FloorTop+.353f,0),new Vector3(.28f,.026f,.30f),Frame(r),.025f);
        B(g,"Bin_finger_slot",new Vector3(0,FloorTop+.369f,-.055f),new Vector3(.08f,.005f,.015f),Dark(r));
        Solid(g,new Vector3(0,FloorTop+.185f,0),new Vector3(.28f,.37f,.30f));
    }

    static void RoomSign(Transform parent,string r,string text,Vector3 pos,float yaw,bool external=false)
    {
        var g=F.Group(parent,r+(external?"_Corridor_room_identification":"_Inner_room_identification"),pos,yaw);
        R(g,"Sage_room_name_plate",Vector3.zero,new Vector3(.62f,.22f,.016f),Sage(r),.015f);
        F.WallText(g,text,new Vector3(0,0,-.011f),0,.46f,new Color(.035f,.055f,.045f));
    }
}
