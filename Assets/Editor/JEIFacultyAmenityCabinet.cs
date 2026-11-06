using UnityEngine;

/// <summary>
/// S01 built-in faculty tea and print station. All geometry stays inside the
/// supplied width/depth and the 2.10 m cabinet height; the fronts face local -Z.
/// Position is the centre of the ground footprint, in local metres.
/// </summary>
public static class JEIFacultyAmenityCabinet
{
    const float Height=2.10f;
    const float CounterHeight=.90f;
    static Material Oak { get { return JEIFurnitureKit.Mat("S01_Faculty_Honey_Oak",new Color(.72f,.54f,.34f),.23f); } }
    static Material Cream { get { return JEIFurnitureKit.Mat("S01_Faculty_Cream_Lacquer",new Color(.87f,.85f,.76f),.22f); } }
    static Material Metal { get { return JEIFurnitureKit.Mat("S01_Amenity_Brushed_Metal",new Color(.61f,.64f,.62f),.52f); } }
    static Material Dark { get { return JEIFurnitureKit.Mat("S01_Amenity_Charcoal",new Color(.18f,.22f,.22f),.20f); } }
    static Material Paper { get { return JEIFurnitureKit.Mat("S01_Amenity_Paper_and_Ceramic",new Color(.95f,.94f,.88f),.16f); } }
    static Material Blue { get { return JEIFurnitureKit.Mat("S01_Amenity_Soft_Blue",new Color(.31f,.53f,.59f),.24f); } }

    static GameObject B(Transform p,string name,Vector3 pos,Vector3 size,Material mat)
    { return JEIFurnitureKit.Box(p,name,pos,size,mat,false); }
    static GameObject R(Transform p,string name,Vector3 pos,Vector3 size,Material mat,float radius=.025f)
    { return JEIFurnitureKit.Rounded(p,name,pos,size,mat,radius); }
    static GameObject P(Transform p,string name,PrimitiveType type,Vector3 pos,Vector3 size,Material mat)
    { return JEIFurnitureKit.Primitive(p,name,type,pos,size,mat); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static Transform Build(Transform parent,Vector3 position,float width=3.55f,float depth=.62f)
    {
        // The design is intended for a 3.55 m run. Smaller/larger runs retain
        // appliance sizes and proportionally move their centre positions.
        if(width<3.30f || depth<.57f) throw new System.ArgumentOutOfRangeException("width/depth","The tea/print cabinet needs at least 3.30 m width and 0.57 m depth.");
        var g=JEIFurnitureKit.Group(parent,"S01_Built_in_tea_and_print_cabinet__W"+width+"__H2.10",position);
        BuildCabinet(g,width,depth);
        float shift=(width-3.55f)/2;
        WaterDispenser(g,new Vector3(-1.36f-shift,CounterHeight,.015f));
        CoffeeMachine(g,new Vector3(-.70f-shift,CounterHeight,.025f));
        Printer(g,new Vector3(.99f+shift,CounterHeight,.025f));
        // A 0.65 m clear central surface is intentionally left unfurnished.
        // This short timber divider prevents casual cup/paper mixing without
        // closing off the counter or occupying the room's walking route.
        B(g,"Tea_print_timber_separator",new Vector3(.40f,.111f+CounterHeight,.015f),new Vector3(.025f,.222f,depth-.12f),Oak);
        return g;
    }

    static void BuildCabinet(Transform g,float width,float depth)
    {
        B(g,"Recessed_warm_plinth",new Vector3(0,.045f,.015f),new Vector3(width-.08f,.09f,depth-.06f),Dark);
        B(g,"Honey_oak_full_height_back",new Vector3(0,Height/2,depth/2-.018f),new Vector3(width,Height,.035f),Oak);
        foreach(float x in new[]{-width/2+.02f,width/2-.02f}) {
            B(g,"Full_height_oak_end_frame",new Vector3(x,Height/2,0),new Vector3(.04f,Height,depth),Oak);
            Solid(g,new Vector3(x,Height/2,0),new Vector3(.04f,Height,depth));
        }
        B(g,"Cabinet_base_shelf",new Vector3(0,.105f,0),new Vector3(width-.06f,.035f,depth),Oak);
        R(g,"Continuous_rounded_honey_oak_worktop__H0.90m",new Vector3(0,.875f,0),new Vector3(width,.05f,depth),Oak,.045f);
        Solid(g,new Vector3(0,.45f,0),new Vector3(width,.90f,depth));
        float bay=width/4;
        for(int i=0;i<4;i++) {
            float x=-width/2+bay*(i+.5f);float z=-depth/2+.019f;
            R(g,"Closed_lower_storage_front",new Vector3(x,.398f,z),new Vector3(bay-.032f,.555f,.033f),Cream,.013f);
            R(g,"Closed_stationery_drawer",new Vector3(x,.757f,z),new Vector3(bay-.032f,.125f,.033f),Cream,.012f);
            foreach(float y in new[]{.633f,.78f}) B(g,"Flush_recessed_finger_pull",new Vector3(x,y,-depth/2+.0015f),new Vector3(.18f,.013f,.002f),Dark);
        }
        const float upperBottom=1.66f;
        float upperDepth=Mathf.Min(.36f,depth-.12f);float upperZ=depth/2-upperDepth/2;
        B(g,"Upper_cabinet_oak_bottom",new Vector3(0,upperBottom+.018f,upperZ),new Vector3(width,.036f,upperDepth),Oak);
        B(g,"Upper_cabinet_oak_top",new Vector3(0,Height-.018f,upperZ),new Vector3(width,.036f,upperDepth),Oak);
        for(int i=0;i<4;i++) {
            float x=-width/2+bay*(i+.5f);
            R(g,"Cream_overhead_closed_sliding_front",new Vector3(x,(upperBottom+Height)/2,depth/2-upperDepth+.02f),new Vector3(bay-.032f,Height-upperBottom-.065f,.032f),Cream,.012f);
            B(g,"Overhead_flush_pull",new Vector3(x,upperBottom+.10f,depth/2-upperDepth+.0025f),new Vector3(.16f,.012f,.002f),Dark);
        }
        Solid(g,new Vector3(0,(upperBottom+Height)/2,upperZ),new Vector3(width,Height-upperBottom,upperDepth));
        // Backing is solid, but no slab collider spans the open working alcove.
        Solid(g,new Vector3(0,1.28f,depth/2-.018f),new Vector3(width,.76f,.035f));
    }

    static void WaterDispenser(Transform parent,Vector3 pos)
    {
        var g=JEIFurnitureKit.Group(parent,"Countertop_water_dispenser__spout_and_cup_recess",pos);
        R(g,"Compact_water_body_back",new Vector3(0,.29f,.14f),new Vector3(.38f,.56f,.18f),Cream,.025f);
        foreach(float x in new[]{-.17f,.17f}) R(g,"Water_body_side",new Vector3(x,.29f,-.015f),new Vector3(.04f,.56f,.34f),Cream,.016f);
        R(g,"Water_body_top",new Vector3(0,.555f,-.012f),new Vector3(.38f,.045f,.35f),Cream,.022f);
        B(g,"Dark_recessed_cup_well",new Vector3(0,.235f,.045f),new Vector3(.29f,.30f,.012f),Dark);
        R(g,"Water_control_fascia",new Vector3(0,.468f,-.175f),new Vector3(.35f,.125f,.025f),Metal,.013f);
        foreach(float x in new[]{-.075f,.075f}) {
            var mat=x<0?Blue:JEIFurnitureKit.Mat("S01_Amenity_Muted_Red",new Color(.67f,.35f,.28f),.24f);
            P(g,"Temperature_button",PrimitiveType.Sphere,new Vector3(x,.48f,-.192f),new Vector3(.034f,.034f,.012f),mat);
        }
        P(g,"Chrome_water_spout",PrimitiveType.Cylinder,new Vector3(0,.344f,-.08f),new Vector3(.026f,.037f,.026f),Metal);
        R(g,"Removable_cup_tray",new Vector3(0,.056f,-.03f),new Vector3(.31f,.025f,.29f),Metal,.02f);
        for(int i=0;i<4;i++) B(g,"Tray_drain_slot",new Vector3(-.09f+i*.06f,.070f,-.035f),new Vector3(.012f,.003f,.17f),Dark);
        Cup(g,new Vector3(0,.071f,-.085f),.075f,.12f);
        JEIFurnitureKit.WallText(g,"WATER",new Vector3(0,.525f,-.190f),0,.115f,Dark.color);
    }

    static void CoffeeMachine(Transform parent,Vector3 pos)
    {
        var g=JEIFurnitureKit.Group(parent,"Built_in_compact_coffee_machine__dual_spouts_and_cup",pos);
        R(g,"Brushed_metal_coffee_body",new Vector3(0,.258f,.067f),new Vector3(.43f,.49f,.35f),Metal,.035f);
        B(g,"Coffee_recess_dark_back",new Vector3(0,.235f,-.113f),new Vector3(.33f,.25f,.012f),Dark);
        R(g,"Metal_espresso_group_head",new Vector3(0,.325f,-.155f),new Vector3(.20f,.075f,.10f),Metal,.018f);
        foreach(float x in new[]{-.052f,.052f}) P(g,"Twin_coffee_outlet",PrimitiveType.Cylinder,new Vector3(x,.266f,-.175f),new Vector3(.022f,.037f,.022f),Metal);
        R(g,"Espresso_cup_platform",new Vector3(0,.043f,-.14f),new Vector3(.36f,.028f,.21f),Metal,.02f);
        for(int i=0;i<5;i++) B(g,"Coffee_tray_drain_slot",new Vector3(-.12f+i*.06f,.059f,-.14f),new Vector3(.012f,.003f,.14f),Dark);
        Cup(g,new Vector3(0,.061f,-.148f),.09f,.115f);
        R(g,"Small_status_display",new Vector3(0,.437f,-.112f),new Vector3(.16f,.059f,.006f),Blue,.006f);
        foreach(float x in new[]{-.15f,.15f}) P(g,"Coffee_selector_button",PrimitiveType.Sphere,new Vector3(x,.435f,-.113f),new Vector3(.032f,.032f,.013f),Cream);
        B(g,"Coffee_top_bean_hopper",new Vector3(0,.514f,.067f),new Vector3(.28f,.045f,.23f),Dark);
    }

    static void Cup(Transform parent,Vector3 bottom,float diameter,float height)
    {
        P(parent,"Ceramic_cup",PrimitiveType.Cylinder,bottom+new Vector3(0,height/2,0),new Vector3(diameter,height/2,diameter),Paper);
        P(parent,"Cup_open_dark_interior",PrimitiveType.Cylinder,bottom+new Vector3(0,height+.001f,0),new Vector3(diameter*.70f,.0015f,diameter*.70f),Dark);
        // A slim three-part C handle reads clearly without a heavy torus mesh.
        B(parent,"Cup_handle_outer",bottom+new Vector3(diameter*.67f,height*.52f,0),new Vector3(.012f,height*.48f,.014f),Paper);
        foreach(float y in new[]{height*.29f,height*.75f}) B(parent,"Cup_handle_join",bottom+new Vector3(diameter*.53f,y,0),new Vector3(diameter*.30f,.012f,.014f),Paper);
    }

    static void Printer(Transform parent,Vector3 pos)
    {
        var g=JEIFurnitureKit.Group(parent,"Multifunction_printer__scanner_touchscreen_and_paper_output",pos);
        R(g,"Cream_printer_main_body",new Vector3(0,.175f,.025f),new Vector3(.73f,.29f,.38f),Cream,.035f);
        B(g,"Printer_lower_dark_plinth",new Vector3(0,.052f,.025f),new Vector3(.68f,.045f,.37f),Dark);
        R(g,"Printer_scanner_lid",new Vector3(0,.345f,.025f),new Vector3(.75f,.06f,.39f),Cream,.025f);
        B(g,"Dark_scanner_lid_seam",new Vector3(0,.309f,.025f),new Vector3(.71f,.012f,.38f),Dark);
        R(g,"Scanner_lid_grip",new Vector3(0,.385f,-.125f),new Vector3(.20f,.025f,.035f),Metal,.009f);
        B(g,"Recessed_paper_output_slot",new Vector3(-.035f,.167f,-.185f),new Vector3(.47f,.06f,.012f),Dark);
        R(g,"Paper_output_tray",new Vector3(-.035f,.08f,-.18f),new Vector3(.49f,.023f,.19f),Metal,.016f);
        for(int i=0;i<3;i++) B(g,"Printed_sheet_on_output_tray",new Vector3(-.035f+i*.007f,.095f+i*.006f,-.18f-i*.003f),new Vector3(.39f,.004f,.17f),Paper);
        R(g,"Inset_touch_control_screen",new Vector3(.245f,.259f,-.185f),new Vector3(.13f,.07f,.007f),Blue,.006f);
        P(g,"Printer_power_button",PrimitiveType.Sphere,new Vector3(.31f,.205f,-.184f),new Vector3(.023f,.023f,.012f),Blue);
        B(g,"Upper_document_feeder",new Vector3(0,.40f,.115f),new Vector3(.42f,.035f,.23f),Metal);
        var feed=B(g,"Paper_in_upper_feeder",new Vector3(0,.452f,.15f),new Vector3(.34f,.075f,.006f),Paper);
        feed.transform.localRotation=Quaternion.Euler(-12,0,0);
        B(g,"Closed_lower_paper_drawer_seam",new Vector3(0,.072f,-.182f),new Vector3(.60f,.007f,.003f),Dark);
    }
}
