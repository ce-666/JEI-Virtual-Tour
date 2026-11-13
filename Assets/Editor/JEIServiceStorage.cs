using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>Metre-scaled C05 cleaning store and C08 low-voltage IT service room furnishings.</summary>
public static class JEIServiceStorage
{
    const float FloorY=.075f;
    static Material C05Cream { get { return F.Mat("C05_Cream_Cabinet",new Color(.88f,.87f,.80f),.22f); } }
    static Material C05Oak { get { return F.Mat("C05_Light_Oak_Shelf",new Color(.74f,.59f,.40f),.20f); } }
    static Material C05Metal { get { return F.Mat("C05_Sage_Metal_Frame",new Color(.40f,.50f,.44f),.28f); } }
    static Material C05Dark { get { return F.Mat("C05_Dark_Detail",new Color(.23f,.28f,.27f),.25f); } }
    static Material C05Tile { get { return F.Mat("C05_Anti_Slip_Grey_Tile",new Color(.74f,.76f,.72f),.12f); } }
    static Material C05Grout { get { return F.Mat("C05_Tile_Grout",new Color(.61f,.64f,.60f),.09f); } }
    static Material C05Ceramic { get { return F.Mat("C05_Mop_Sink_Ceramic",new Color(.93f,.93f,.87f),.30f); } }
    static Material C05Blue { get { return F.Mat("C05_Supply_Dusty_Blue",new Color(.47f,.62f,.65f),.17f); } }
    static Material C05Paper { get { return F.Mat("C05_Supply_Label",new Color(.95f,.94f,.85f),.08f); } }
    static Material C05Grey { get { return F.Mat("C05_Cleaning_Fibres",new Color(.60f,.66f,.61f),.08f); } }
    static Material C08Cream { get { return F.Mat("C08_Cream_Service_Cabinet",new Color(.87f,.87f,.80f),.22f); } }
    static Material C08Graphite { get { return F.Mat("C08_Rack_Graphite",new Color(.19f,.24f,.25f),.28f); } }
    static Material C08Device { get { return F.Mat("C08_Equipment_Brushed_Metal",new Color(.43f,.49f,.49f),.35f); } }
    static Material C08Dark { get { return F.Mat("C08_Recessed_Vent_and_Port",new Color(.08f,.13f,.14f),.20f); } }
    static Material C08Tile { get { return F.Mat("C08_Anti_Slip_Grey_Tile",new Color(.74f,.76f,.72f),.12f); } }
    static Material C08Grout { get { return F.Mat("C08_Tile_Grout",new Color(.61f,.64f,.60f),.09f); } }
    static Material C08Blue { get { return F.Mat("C08_Blue_Data_Cable",new Color(.36f,.53f,.65f),.22f); } }
    static Material C08Sage { get { return F.Mat("C08_Sage_Data_Cable",new Color(.44f,.62f,.49f),.22f); } }
    static Material C08Gold { get { return F.Mat("C08_Muted_Gold_Data_Cable",new Color(.74f,.65f,.35f),.22f); } }
    static Material C08LED { get { return F.Mat("C08_Green_Status_LED",new Color(.34f,.83f,.46f),.25f); } }

    static GameObject B(Transform parent,string name,Vector3 pos,Vector3 size,Material material)
    { return F.Box(parent,name,pos,size,material,false); }
    static GameObject R(Transform parent,string name,Vector3 pos,Vector3 size,Material material,float radius=.02f)
    { return F.Rounded(parent,name,pos,size,material,radius); }
    static void Solid(Transform parent,Vector3 centre,Vector3 size)
    { var collider=parent.gameObject.AddComponent<BoxCollider>();collider.center=centre;collider.size=size; }

    public static Transform BuildCleaner(Transform parent)
    {
        var root=F.Group(parent,"C05__Cleaner_and_store");
        Floor(root,"C05_Non_slip_tile_floor",new Vector3(8,0,1.65f),2.58f,3.08f,C05Tile,C05Grout);
        SupplyShelving(root);
        ToolCabinet(root);
        MopSink(root);
        HangingTools(root);
        CleaningTrolley(root);
        return root;
    }

    public static Transform BuildIT(Transform parent)
    {
        var root=F.Group(parent,"C08__IT_services");
        Floor(root,"C08_Non_slip_tile_floor",new Vector3(10.9f,0,-1.65f),2.78f,3.08f,C08Tile,C08Grout);
        NetworkRack(root);
        WeakCurrentBox(root);
        DataTrunking(root);
        return root;
    }

    static void Floor(Transform parent,string name,Vector3 centre,float width,float depth,Material tile,Material grout)
    {
        var g=F.Group(parent,name,centre);
        B(g,"Shallow_non_slip_tile_finish",new Vector3(0,.0625f,0),new Vector3(width,.025f,depth),tile);
        foreach(float x in new[]{-.60f,0,.60f})
            B(g,"Fine_tile_joint",new Vector3(x,FloorY+.0003f,0),new Vector3(.005f,.0006f,depth),grout);
        foreach(float z in new[]{-1.0f,-.5f,0,.5f,1.0f})
            B(g,"Fine_tile_joint",new Vector3(0,FloorY+.0003f,z),new Vector3(width,.0006f,.005f),grout);
        Solid(g,new Vector3(0,.0625f,0),new Vector3(width,.025f,depth));
    }

    static void SupplyShelving(Transform parent)
    {
        var g=F.Group(parent,"C05_Supply_shelving",new Vector3(8.05f,0,.40f));
        foreach(float x in new[]{-.50f,.50f})
            foreach(float z in new[]{-.235f,.235f}) {
                B(g,"Slender_sage_shelf_upright",new Vector3(x,1.0f,z),new Vector3(.03f,1.84f,.03f),C05Metal);
                B(g,"Floor_protection_pad",new Vector3(x,.083f,z),new Vector3(.05f,.016f,.05f),C05Dark);
            }
        foreach(float y in new[]{.18f,.61f,1.05f,1.49f,1.89f})
            R(g,"Oak_utility_shelf",new Vector3(0,y,0),new Vector3(1.05f,.035f,.49f),C05Oak,.012f);
        int level=0;
        foreach(float y in new[]{.20f,.63f,1.07f,1.51f}) {
            foreach(float x in new[]{-.25f,.25f}) {
                float height=level==3?.24f:.29f;
                var box=F.Group(g,"Closed_consumable_box",new Vector3(x,y+height/2,0));
                R(box,"Useful_closed_storage_box",Vector3.zero,new Vector3(.41f,height,.37f),level%2==0?C05Cream:C05Blue,.018f);
                B(box,"Box_lid",new Vector3(0,height/2,0),new Vector3(.42f,.025f,.38f),C05Cream);
                B(box,"Supply_label",new Vector3(0,.012f,.189f),new Vector3(.26f,.07f,.008f),C05Paper);
                F.WallText(box,level==0?"PAPER":level==1?"GLOVES":level==2?"REFILLS":"SUPPLIES",new Vector3(0,.012f,.196f),180,.22f,new Color(.035f,.055f,.045f));
            }
            level++;
        }
        Solid(g,new Vector3(0,1.0f,0),new Vector3(1.05f,1.85f,.52f));
    }

    static void ToolCabinet(Transform parent)
    {
        var g=F.Group(parent,"C05_Tool_cabinet",new Vector3(9.02f,0,.70f),90);
        B(g,"Inset_plinth",new Vector3(0,.105f,.015f),new Vector3(.76f,.06f,.37f),C05Metal);
        R(g,"Closed_tall_tool_cabinet",new Vector3(0,1.075f,0),new Vector3(.85f,1.90f,.44f),C05Cream,.015f);
        foreach(float x in new[]{-.208f,.208f}) {
            R(g,"Full_height_closed_tool_door",new Vector3(x,1.095f,-.225f),new Vector3(.407f,1.80f,.025f),C05Cream,.01f);
            B(g,"Vertical_door_pull",new Vector3(x<0?-.048f:.048f,1.08f,-.246f),new Vector3(.014f,.18f,.008f),C05Metal);
            foreach(float y in new[]{.39f,.44f,.49f})
                B(g,"Low_door_vent",new Vector3(x,y,-.240f),new Vector3(.25f,.009f,.004f),C05Metal);
        }
        F.WallText(g,"TOOLS",new Vector3(0,1.74f,-.242f),0,.32f,new Color(.035f,.055f,.045f));
        Solid(g,new Vector3(0,1.05f,0),new Vector3(.85f,1.95f,.50f));
    }

    static void MopSink(Transform parent)
    {
        var g=F.Group(parent,"C05_Mop_sink",new Vector3(7.10f,0,.72f));
        foreach(float x in new[]{-.24f,.24f})
            foreach(float z in new[]{-.25f,.25f})
                B(g,"Short_sink_support_foot",new Vector3(x,.145f,z),new Vector3(.065f,.14f,.065f),C05Metal);
        R(g,"Basin_floor_inside_real_cavity",new Vector3(0,.2275f,0),new Vector3(.68f,.045f,.74f),C05Ceramic,.03f);
        foreach(float x in new[]{-.3025f,.3025f})
            R(g,"Deep_ceramic_basin_side",new Vector3(x,.4275f,0),new Vector3(.075f,.355f,.74f),C05Ceramic,.025f);
        R(g,"Deep_ceramic_basin_back",new Vector3(0,.4275f,-.3325f),new Vector3(.53f,.355f,.075f),C05Ceramic,.025f);
        R(g,"Lowered_front_loading_lip",new Vector3(0,.3975f,.3325f),new Vector3(.53f,.295f,.075f),C05Ceramic,.025f);
        F.Primitive(g,"Visible_basin_drain",PrimitiveType.Cylinder,new Vector3(0,.253f,0),new Vector3(.075f,.003f,.075f),C05Dark);
        foreach(float x in new[]{-.022f,0,.022f})
            B(g,"Drain_strainer_slot",new Vector3(x,.257f,0),new Vector3(.006f,.002f,.042f),C05Ceramic);
        Pipe(g,"Waste_pipe_below_basin",new Vector3(0,.215f,0),new Vector3(0,.10f,-.20f),.048f,C05Metal);
        Pipe(g,"Wall_tap_riser",new Vector3(0,.61f,-.55f),new Vector3(0,.96f,-.55f),.025f,C05Metal);
        Pipe(g,"Wall_mounted_gooseneck_spout",new Vector3(0,.96f,-.55f),new Vector3(0,.96f,-.15f),.025f,C05Metal);
        Pipe(g,"Downward_water_outlet",new Vector3(0,.96f,-.15f),new Vector3(0,.88f,-.15f),.025f,C05Metal);
        B(g,"Tap_wall_escutcheon",new Vector3(0,.73f,-.585f),new Vector3(.15f,.16f,.035f),C05Cream);
        B(g,"Tap_lever",new Vector3(.07f,.78f,-.53f),new Vector3(.13f,.018f,.025f),C05Metal);
        Solid(g,new Vector3(0,.34f,0),new Vector3(.68f,.53f,.74f));
    }

    static void HangingTools(Transform parent)
    {
        var g=F.Group(parent,"C05_Wall_hung_cleaning_tools",new Vector3(6.755f,0,2.05f),270);
        B(g,"Wall_mounted_tool_rail",new Vector3(0,1.61f,0),new Vector3(1.27f,.075f,.035f),C05Oak);
        int number=0;
        foreach(float x in new[]{-.44f,0,.44f}) {
            number++;
            B(g,"Tool_rail_hook",new Vector3(x,1.60f,-.035f),new Vector3(.04f,.06f,.07f),C05Metal);
            Pipe(g,"Hanging_tool_handle",new Vector3(x,.38f,-.08f),new Vector3(x,1.66f,-.08f),.024f,number==2?C05Blue:C05Oak);
            R(g,"Mop_or_broom_crosshead",new Vector3(x,.38f,-.08f),new Vector3(.25f,.045f,.08f),C05Metal,.012f);
            for(int i=0;i<7;i++) {
                float strandX=x-.105f+i*.035f;
                F.Primitive(g,number==3?"Broom_bristle_bundle":"Loose_mop_fibre_bundle",PrimitiveType.Capsule,new Vector3(strandX,.285f,-.08f),new Vector3(.024f,.08f,.045f),number==3?C05Oak:C05Grey);
            }
        }
    }

    static void CleaningTrolley(Transform parent)
    {
        var g=F.Group(parent,"C05_Cleaning_trolley",new Vector3(8.91f,0,2.10f),90);
        R(g,"Light_sage_trolley_base",new Vector3(0,.185f,0),new Vector3(.75f,.035f,.57f),C05Metal,.035f);
        foreach(float x in new[]{-.31f,.31f})
            foreach(float z in new[]{-.23f,.23f}) {
                var wheel=F.Primitive(g,"Quiet_trolley_caster",PrimitiveType.Cylinder,new Vector3(x,.115f,z),new Vector3(.08f,.018f,.08f),C05Dark);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                B(g,"Caster_fork",new Vector3(x,.153f,z),new Vector3(.03f,.055f,.035f),C05Metal);
            }
        foreach(float x in new[]{-.335f,.335f})
            B(g,"Carry_handle_upright",new Vector3(x,.61f,.23f),new Vector3(.025f,.74f,.025f),C05Metal);
        R(g,"Rounded_push_handle",new Vector3(0,.99f,.23f),new Vector3(.70f,.03f,.04f),C05Metal,.014f);
        Arc(g,"Open_mop_bucket",new Vector3(-.17f,.23f,0),.15f,.126f,.26f,C05Blue);
        F.Primitive(g,"Bucket_bottom",PrimitiveType.Cylinder,new Vector3(-.17f,.24f,0),new Vector3(.28f,.01f,.28f),C05Blue);
        R(g,"Bucket_wringer_frame",new Vector3(-.04f,.51f,0),new Vector3(.14f,.10f,.25f),C05Cream,.018f);
        foreach(float z in new[]{-.075f,-.025f,.025f,.075f})
            B(g,"Wringer_slot",new Vector3(-.04f,.562f,z),new Vector3(.10f,.004f,.015f),C05Dark);
        R(g,"Closed_cleaning_supply_bin",new Vector3(.17f,.405f,0),new Vector3(.28f,.36f,.43f),C05Cream,.025f);
        R(g,"Closed_bin_lid",new Vector3(.17f,.595f,0),new Vector3(.30f,.025f,.45f),C05Metal,.018f);
        foreach(float z in new[]{-.085f,.085f}) {
            F.Primitive(g,"Small_refill_bottle",PrimitiveType.Cylinder,new Vector3(.17f,.68f,z),new Vector3(.06f,.0725f,.06f),z<0?C05Blue:C05Cream);
            F.Primitive(g,"Bottle_cap",PrimitiveType.Cylinder,new Vector3(.17f,.763f,z),new Vector3(.032f,.011f,.032f),C05Metal);
        }
        Solid(g,new Vector3(0,.55f,0),new Vector3(.76f,.95f,.58f));
    }

    static void NetworkRack(Transform parent)
    {
        var g=F.Group(parent,"C08_Network_rack",new Vector3(11.86f,0,-.72f),90);
        foreach(float x in new[]{-.29f,.29f})
            foreach(float z in new[]{-.25f,.25f})
                B(g,"Rack_leveling_foot",new Vector3(x,.105f,z),new Vector3(.06f,.06f,.06f),C08Graphite);
        B(g,"Recessed_rack_plinth",new Vector3(0,.15f,.01f),new Vector3(.71f,.07f,.63f),C08Graphite);
        B(g,"Rack_rear_panel",new Vector3(0,1.06f,.33f),new Vector3(.74f,1.83f,.04f),C08Graphite);
        foreach(float x in new[]{-.35f,.35f}) {
            B(g,"Rack_side_panel",new Vector3(x,1.06f,0),new Vector3(.04f,1.83f,.70f),C08Graphite);
            B(g,"Standard_equipment_mounting_rail",new Vector3(x*.87f,1.06f,-.27f),new Vector3(.026f,1.68f,.035f),C08Device);
        }
        B(g,"Rack_top_with_ventilation",new Vector3(0,1.95f,0),new Vector3(.76f,.05f,.70f),C08Graphite);
        foreach(float x in new[]{-.22f,-.11f,0,.11f,.22f})
            B(g,"Roof_vent_slot",new Vector3(x,1.977f,0),new Vector3(.045f,.004f,.42f),C08Dark);
        int unit=0;
        foreach(float y in new[]{.46f,.73f,1.01f,1.29f,1.57f}) {
            unit++;
            var device=F.Group(g,"C08_Network_equipment_layer_"+unit,new Vector3(0,y,0));
            R(device,"Rack_network_appliance",new Vector3(0,0,-.015f),new Vector3(.58f,.14f,.48f),C08Device,.01f);
            for(int port=0;port<8;port++)
                B(device,"Recessed_ethernet_or_vent_port",new Vector3(-.231f+port*.066f,.019f,-.258f),new Vector3(.042f,.027f,.007f),C08Dark);
            foreach(float x in new[]{-.20f,-.15f,-.10f})
                F.Primitive(device,"Small_green_status_indicator",PrimitiveType.Sphere,new Vector3(x,-.035f,-.263f),new Vector3(.012f,.012f,.004f),C08LED);
            F.WallText(device,unit==5?"FIBRE PATCH":unit==4?"CORE SWITCH":unit==3?"ROUTER":"NETWORK",new Vector3(.11f,-.035f,-.265f),0,.19f,new Color(.91f,.94f,.88f));
        }
        for(int i=0;i<3;i++) {
            float shift=(i-1)*.11f;
            Vector3 from=new Vector3(-.13f+shift,1.59f,-.286f),to=new Vector3(.15f+shift,1.31f,-.286f);
            B(g,"Seated_data_patch_connector",from+new Vector3(0,0,.015f),new Vector3(.025f,.022f,.032f),C08Cream);
            B(g,"Seated_data_patch_connector",to+new Vector3(0,0,.015f),new Vector3(.025f,.022f,.032f),C08Cream);
            PatchCable(g,"C08_Organised_patch_cable_"+(i+1),from,to,i==0?C08Blue:i==1?C08Sage:C08Gold);
        }
        var door=F.Group(g,"C08_Network_rack_door");
        foreach(float x in new[]{-.345f,.345f})
            B(door,"Sage_rack_door_side",new Vector3(x,1.06f,-.366f),new Vector3(.045f,1.70f,.035f),C08Graphite);
        foreach(float y in new[]{.23f,1.89f})
            B(door,"Sage_rack_door_rail",new Vector3(0,y,-.366f),new Vector3(.735f,.04f,.035f),C08Graphite);
        var glass=F.MatGlass("C08_Clear_Rack_Door",new Color(.56f,.69f,.66f,.12f));
        B(door,"Transparent_equipment_door_window",new Vector3(0,1.06f,-.367f),new Vector3(.60f,1.60f,.008f),glass);
        B(door,"Rack_door_latch",new Vector3(.29f,1.10f,-.382f),new Vector3(.025f,.17f,.014f),C08Cream);
        Solid(g,new Vector3(0,1.025f,0),new Vector3(.82f,1.90f,.78f));
    }

    static void WeakCurrentBox(Transform parent)
    {
        var g=F.Group(parent,"C08_Weak_current_wall_cabinet",new Vector3(9.58f,1.38f,-1.25f),270);
        R(g,"Cream_wall_box",Vector3.zero,new Vector3(.68f,.62f,.13f),C08Cream,.018f);
        R(g,"Low_voltage_access_door",new Vector3(0,0,-.075f),new Vector3(.62f,.56f,.018f),C08Cream,.01f);
        B(g,"Small_door_latch",new Vector3(.25f,-.035f,-.089f),new Vector3(.025f,.09f,.012f),C08Graphite);
        F.WallText(g,"DATA / ACCESS",new Vector3(0,.16f,-.088f),0,.43f,new Color(.035f,.055f,.045f));
        foreach(float y in new[]{-.16f,-.12f,-.08f})
            B(g,"Low_voltage_box_vent",new Vector3(-.025f,y,-.088f),new Vector3(.38f,.009f,.004f),C08Device);
        F.Primitive(g,"Data_box_status_light",PrimitiveType.Sphere,new Vector3(-.22f,.025f,-.091f),new Vector3(.022f,.022f,.007f),C08LED);
    }

    static void DataTrunking(Transform parent)
    {
        var g=F.Group(parent,"C08_Organised_low_voltage_trunking");
        B(g,"West_wall_high_level_data_channel",new Vector3(9.56f,2.15f,-1.55f),new Vector3(.08f,.095f,2.65f),C08Cream);
        B(g,"West_channel_cover_seam",new Vector3(9.603f,2.15f,-1.55f),new Vector3(.004f,.010f,2.60f),C08Device);
        B(g,"Wall_box_vertical_data_channel",new Vector3(9.56f,1.91f,-1.25f),new Vector3(.07f,.48f,.065f),C08Cream);
        B(g,"North_wall_data_channel",new Vector3(10.855f,2.15f,-.155f),new Vector3(2.61f,.095f,.08f),C08Cream);
        B(g,"North_channel_cover_seam",new Vector3(10.855f,2.15f,-.198f),new Vector3(2.56f,.010f,.004f),C08Device);
        B(g,"Rack_high_level_service_drop",new Vector3(12.16f,2.055f,-.155f),new Vector3(.07f,.19f,.07f),C08Cream);
        B(g,"Rack_top_data_entry",new Vector3(12.16f,1.982f,-.25f),new Vector3(.07f,.045f,.20f),C08Cream);
    }

    static void PatchCable(Transform parent,string name,Vector3 from,Vector3 to,Material material)
    {
        var g=F.Group(parent,name);
        Vector3 control=new Vector3((from.x+to.x)/2,Mathf.Min(from.y,to.y)-.11f,-.34f);
        Vector3 previous=from;
        for(int i=1;i<=10;i++) {
            float t=i/10f;
            Vector3 point=(1-t)*(1-t)*from+2*(1-t)*t*control+t*t*to;
            Pipe(g,"Short_neatly_looped_patch_segment",previous,point,.012f,material);previous=point;
        }
    }
    static void Pipe(Transform parent,string name,Vector3 from,Vector3 to,float diameter,Material material)
    {
        var g=F.Primitive(parent,name,PrimitiveType.Cylinder,(from+to)/2,new Vector3(diameter,Vector3.Distance(from,to)/2,diameter),material);
        g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
    }
    static void Arc(Transform parent,string name,Vector3 pos,float outer,float inner,float height,Material material)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;
        g.AddComponent<MeshFilter>().sharedMesh=F.RingMesh(outer,inner,0,360,height);
        g.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
}
