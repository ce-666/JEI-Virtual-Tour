using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>S03: two north/entrance-facing work bays, records, lounge and a north-wall service cabinet.</summary>
public static class JEIAdministrativeSuite
{
    static Material Wood { get { return F.Mat("S03_Service_Honey_Oak",new Color(.72f,.54f,.34f),.23f); } }
    static Material Cream { get { return F.Mat("S03_Service_Cream",new Color(.87f,.85f,.76f),.22f); } }
    static Material Dark { get { return F.Mat("S03_Appliance_Graphite",new Color(.18f,.23f,.24f),.22f); } }
    static Material Metal { get { return F.Mat("S03_Appliance_Metal",new Color(.63f,.66f,.64f),.45f); } }
    static Material Paper { get { return F.Mat("S03_Appliance_Paper_Ceramic",new Color(.95f,.94f,.88f),.15f); } }
    static Material Blue { get { return F.Mat("S03_Appliance_Blue",new Color(.32f,.53f,.60f),.25f); } }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material mat)
    { return F.Box(p,n,pos,size,mat,false); }
    static GameObject R(Transform p,string n,Vector3 pos,Vector3 size,Material mat,float radius=.025f)
    { return F.Rounded(p,n,pos,size,mat,radius); }
    static GameObject P(Transform p,string n,PrimitiveType type,Vector3 pos,Vector3 size,Material mat)
    { return F.Primitive(p,n,type,pos,size,mat); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static void Build(Transform parent)
    {
        var suite=F.Group(parent,"S03__Two_administrative_workstations__records_and_lounge");
        var work=F.Group(suite,"Two_retained_south_row_workstations__faces_entrance");
        foreach(float x in new[]{5.15f,6.60f}) {
            F.Desk(work,new Vector3(x,0,-10.25f),0,1.35f,.70f);
            F.Chair(work,new Vector3(x,0,-11.20f),0,true,"Navy");
        }
        var records=F.Group(suite,"Records_and_reference_materials");
        foreach(float z in new[]{-11.70f,-10.35f,-9.0f}) {
            F.Cabinet(records,new Vector3(9.63f,0,z),1.24f,1.87f,90);
            F.WallText(records,z < -11?"COURSE FILES":z < -10?"ADMIN RECORDS":"TEACHING RESOURCES",
                new Vector3(9.358f,1.53f,z),90,.92f,new Color(.20f,.29f,.30f));
        }
        var entranceCabinet=F.Cabinet(records,new Vector3(9.63f,0,-7.20f),1.50f,1.55f,90,false);
        entranceCabinet.name="Closed_reference_storage_near_entrance";
        F.Sofa(suite,new Vector3(3.95f,0,-8.15f),270,1.55f,"Sand");
        var coffee=F.RoundTable(suite,new Vector3(4.90f,0,-8.15f),.30f);
        ConfigureOvalCoffeeTable(coffee);
        ServiceCabinet(suite);
    }

    public static void ConfigureOvalCoffeeTable(Transform coffee)
    {
        var top=coffee.Find("Rounded circular oak tabletop") ?? coffee.Find("Elliptical oak tabletop");
        var foot=coffee.Find("Pedestal foot");
        var collider=coffee.GetComponent<BoxCollider>();
        if(!top || !foot || !collider) throw new System.InvalidOperationException("The lounge coffee table is incomplete.");
        coffee.name="Small_oval_lounge_coffee_table__W0.60_L1.00_H0.46m";
        coffee.localScale=new Vector3(1,.6f,1);
        // The sofa runs along Z. Elongate the top and foot without stretching the stem.
        top.name="Elliptical oak tabletop";
        top.localScale=new Vector3(.60f,.0275f,1.00f);
        foot.localScale=new Vector3(.255f,.025f,.40f);
        collider.size=new Vector3(.60f,.76f,1.00f);
    }

    static void ServiceCabinet(Transform parent)
    {
        const float width=2.30f,depth=.60f,height=1.75f,counter=.90f;
        var g=F.Group(parent,"S03_North_wall_print_and_water_cabinet__W2.30_D0.60_H1.75",new Vector3(4.90f,0,-6.38f));
        B(g,"Recessed_plinth",new Vector3(0,.045f,.015f),new Vector3(width-.08f,.09f,depth-.06f),Dark);
        B(g,"Full_height_oak_back",new Vector3(0,height/2,depth/2-.018f),new Vector3(width,height,.035f),Wood);
        foreach(float x in new[]{-width/2+.02f,width/2-.02f}) {
            B(g,"Tall_oak_end_frame",new Vector3(x,height/2,0),new Vector3(.04f,height,depth),Wood);
            Solid(g,new Vector3(x,height/2,0),new Vector3(.04f,height,depth));
        }
        R(g,"Continuous_timber_service_counter__H0.90m",new Vector3(0,.875f,0),new Vector3(width,.05f,depth),Wood,.045f);
        Solid(g,new Vector3(0,counter/2,0),new Vector3(width,counter,depth));
        for(int i=0;i<3;i++) {
            float x=-width/2+(i+.5f)*width/3;
            R(g,"Closed_lower_storage_front",new Vector3(x,.40f,-.281f),new Vector3(width/3-.032f,.56f,.033f),Cream,.013f);
            R(g,"Closed_stationery_drawer",new Vector3(x,.757f,-.281f),new Vector3(width/3-.032f,.125f,.033f),Cream,.012f);
            foreach(float y in new[]{.635f,.78f}) B(g,"Flush_finger_pull",new Vector3(x,y,-.2985f),new Vector3(.16f,.013f,.002f),Dark);
        }
        const float upperBottom=1.52f,upperDepth=.31f;
        float upperZ=depth/2-upperDepth/2;
        B(g,"Overhead_oak_base",new Vector3(0,upperBottom+.0175f,upperZ),new Vector3(width,.035f,upperDepth),Wood);
        B(g,"Overhead_oak_top",new Vector3(0,height-.0175f,upperZ),new Vector3(width,.035f,upperDepth),Wood);
        for(int i=0;i<3;i++) {
            float x=-width/2+(i+.5f)*width/3;
            R(g,"Cream_upper_storage_front",new Vector3(x,(upperBottom+height)/2,.007f),new Vector3(width/3-.032f,height-upperBottom-.055f,.032f),Cream,.012f);
        }
        Solid(g,new Vector3(0,(upperBottom+height)/2,upperZ),new Vector3(width,height-upperBottom,upperDepth));
        Solid(g,new Vector3(0,1.21f,.282f),new Vector3(width,.62f,.035f));
        // Separate paper and drinks within the cabinet; the divider never
        // projects beyond the worktop or into the clear room circulation.
        B(g,"Timber_print_water_separator",new Vector3(.02f,1.02f,.015f),new Vector3(.025f,.24f,.45f),Wood);
        Printer(g,new Vector3(-.60f,counter,.025f));
        Water(g,new Vector3(.50f,counter,.015f));
    }

    static void Printer(Transform parent,Vector3 pos)
    {
        var g=F.Group(parent,"S03_Multifunction_printer__scanner_touchscreen_paper",pos);
        R(g,"Cream_printer_body",new Vector3(0,.175f,.025f),new Vector3(.71f,.29f,.38f),Cream,.032f);
        B(g,"Lower_dark_plinth",new Vector3(0,.052f,.025f),new Vector3(.66f,.045f,.35f),Dark);
        B(g,"Scanner_lid_seam",new Vector3(0,.309f,.025f),new Vector3(.69f,.012f,.38f),Dark);
        R(g,"Scanner_top_lid",new Vector3(0,.345f,.025f),new Vector3(.73f,.06f,.39f),Cream,.025f);
        R(g,"Top_scanner_grip",new Vector3(0,.385f,-.125f),new Vector3(.20f,.025f,.035f),Metal,.009f);
        B(g,"Recessed_paper_output_slot",new Vector3(-.035f,.167f,-.171f),new Vector3(.46f,.055f,.012f),Dark);
        R(g,"Output_tray_inside_cabinet_depth",new Vector3(-.035f,.08f,-.18f),new Vector3(.48f,.023f,.19f),Metal,.016f);
        for(int i=0;i<3;i++) B(g,"Printed_paper",new Vector3(-.035f+i*.007f,.095f+i*.006f,-.18f-i*.003f),new Vector3(.38f,.004f,.17f),Paper);
        R(g,"Blue_touch_control_screen",new Vector3(.235f,.259f,-.171f),new Vector3(.12f,.07f,.007f),Blue,.006f);
        P(g,"Power_button",PrimitiveType.Sphere,new Vector3(.29f,.205f,-.171f),new Vector3(.023f,.023f,.012f),Blue);
        B(g,"Upper_document_feeder",new Vector3(0,.40f,.115f),new Vector3(.41f,.035f,.22f),Metal);
        var feed=B(g,"Paper_in_document_feeder",new Vector3(0,.452f,.15f),new Vector3(.33f,.075f,.006f),Paper);
        feed.transform.localRotation=Quaternion.Euler(-12,0,0);
    }

    static void Water(Transform parent,Vector3 pos)
    {
        var g=F.Group(parent,"S03_Countertop_water_dispenser__spout_and_cup_recess",pos);
        R(g,"Water_dispenser_body_back",new Vector3(0,.29f,.14f),new Vector3(.38f,.56f,.18f),Cream,.025f);
        foreach(float x in new[]{-.17f,.17f}) R(g,"Water_dispenser_side",new Vector3(x,.29f,-.015f),new Vector3(.04f,.56f,.34f),Cream,.016f);
        R(g,"Water_dispenser_top",new Vector3(0,.555f,-.012f),new Vector3(.38f,.045f,.35f),Cream,.022f);
        B(g,"Recessed_cup_well",new Vector3(0,.235f,.045f),new Vector3(.29f,.30f,.012f),Dark);
        R(g,"Metal_control_fascia",new Vector3(0,.468f,-.175f),new Vector3(.35f,.125f,.025f),Metal,.013f);
        foreach(float x in new[]{-.075f,.075f}) {
            var mat=x<0?Blue:F.Mat("S03_Appliance_Muted_Red",new Color(.67f,.35f,.28f),.24f);
            P(g,"Temperature_button",PrimitiveType.Sphere,new Vector3(x,.48f,-.192f),new Vector3(.034f,.034f,.012f),mat);
        }
        P(g,"Metal_water_spout",PrimitiveType.Cylinder,new Vector3(0,.344f,-.08f),new Vector3(.026f,.037f,.026f),Metal);
        R(g,"Cup_tray",new Vector3(0,.056f,-.03f),new Vector3(.31f,.025f,.29f),Metal,.02f);
        for(int i=0;i<4;i++) B(g,"Tray_drain_slot",new Vector3(-.09f+i*.06f,.070f,-.035f),new Vector3(.012f,.003f,.17f),Dark);
        P(g,"Ceramic_drinking_cup",PrimitiveType.Cylinder,new Vector3(0,.131f,-.085f),new Vector3(.075f,.060f,.075f),Paper);
        P(g,"Cup_open_interior",PrimitiveType.Cylinder,new Vector3(0,.192f,-.085f),new Vector3(.053f,.0015f,.053f),Dark);
    }
}
