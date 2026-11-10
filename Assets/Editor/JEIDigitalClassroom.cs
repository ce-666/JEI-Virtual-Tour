using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>N03: sixteen computer places in a warm, west-facing digital learning room.</summary>
public static class JEIDigitalClassroom
{
    static Material Oak { get { return F.Mat("N03_Warm_Oak",new Color(.74f,.58f,.38f),.23f); } }
    static Material Cream { get { return F.Mat("N03_Cream_Frame",new Color(.86f,.86f,.79f),.24f); } }
    static Material Graphite { get { return F.Mat("N03_Graphite_Detail",new Color(.22f,.28f,.29f),.30f); } }
    static Material Sage { get { return F.Mat("N03_Sage_Upholstery",new Color(.46f,.61f,.52f),.12f); } }
    static Material Blue { get { return F.Mat("N03_Dusty_Blue_Upholstery",new Color(.43f,.57f,.66f),.12f); } }
    static Material Yellow { get { return F.Mat("N03_Muted_Yellow",new Color(.79f,.70f,.42f),.17f); } }
    static Material Paper { get { return F.Mat("N03_Paper_Ivory",new Color(.96f,.94f,.85f),.09f); } }
    static Material Cork { get { return F.Mat("N03_Warm_Cork",new Color(.66f,.52f,.35f),.08f); } }
    static Material Screen { get { return F.Mat("N03_Screen_Pale_Blue",new Color(.61f,.76f,.80f),.25f); } }
    static Material Keys { get { return F.Mat("N03_Keyboard_Keys",new Color(.48f,.52f,.51f),.25f); } }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,n,pos,size,m,false); }
    static GameObject R(Transform p,string n,Vector3 pos,Vector3 size,Material m,float radius=.025f)
    { return F.Rounded(p,n,pos,size,m,radius); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static void Build(Transform parent)
    {
        var room=F.Group(parent,"N03__Digital_learning__16_seats");
        var tables=F.Group(room,"Eight_double_computer_desks__four_rows_two_columns");
        var chairs=F.Group(room,"Sixteen_upholstered_student_task_chairs__faces_west");
        int deskNumber=0,seatNumber=0;
        // Each table offers a full metre of width per learner. The 1.10 m central
        // aisle and 0.80 m outside aisles run across every row, towards the front.
        foreach(float x in new[]{2.30f,4.15f,6.00f,7.85f})
            foreach(float z in new[]{7.80f,10.90f}) {
                deskNumber++;
                var desk=ComputerTable(tables,"N03_Double_computer_desk_"+deskNumber.ToString("00"),new Vector3(x,0,z));
                foreach(float offset in new[]{-.50f,.50f}) {
                    seatNumber++;
                    Computer(desk,"N03_Student_computer_"+seatNumber.ToString("00"),offset,seatNumber);
                    // At yaw 270 local +X points north; using the same offsets
                    // pairs the chair and computer exactly in world coordinates.
                    TaskChair(chairs,"N03_Student_task_chair_"+seatNumber.ToString("00"),new Vector3(x+.90f,0,z+offset),270,seatNumber%2==0?Blue:Sage,false);
                }
            }
        BuildMainTeachingScreen(room);
        TeachingPodium(room);
        TaskChair(room,"N03_Teacher_chair",new Vector3(-.23f,0,11.80f),90,Sage,true);
        ResourceStorage(room);
        ProjectBoard(room,"N03_Project_board_01",7.25f,"DIGITAL PROJECTS",false);
        ProjectBoard(room,"N03_Project_board_02",11.45f,"STUDENT SHOWCASE",true);
    }

    static Transform ComputerTable(Transform parent,string name,Vector3 pos)
    {
        const float width=2.0f,depth=.80f;
        var g=F.Group(parent,name,pos,270);
        R(g,"Soft_corner_oak_worktop__2.00x0.80m__H0.75m",new Vector3(0,.7225f,0),new Vector3(width,.055f,depth),Oak,.045f);
        foreach(float x in new[]{-.90f,.90f}) {
            foreach(float z in new[]{-.30f,.30f}) {
                B(g,"Slim_cream_steel_leg",new Vector3(x,.345f,z),new Vector3(.035f,.69f,.035f),Cream);
                B(g,"Quiet_floor_glide",new Vector3(x,.008f,z),new Vector3(.042f,.016f,.042f),Graphite);
            }
            B(g,"End_frame_under_worktop",new Vector3(x,.66f,0),new Vector3(.035f,.055f,.64f),Cream);
        }
        B(g,"Rear_frame",new Vector3(0,.66f,.29f),new Vector3(1.80f,.055f,.032f),Cream);
        B(g,"Recessed_shared_cable_tray",new Vector3(0,.633f,.28f),new Vector3(1.66f,.065f,.14f),Graphite);
        B(g,"Hidden_power_distribution_strip",new Vector3(0,.66f,.28f),new Vector3(1.30f,.03f,.07f),Cream);
        // A modeled rear-leg channel keeps the service run inside the table footprint.
        B(g,"Covered_cable_route_on_rear_leg",new Vector3(-.90f,.36f,.276f),new Vector3(.025f,.59f,.014f),Graphite);
        Solid(g,new Vector3(0,.375f,0),new Vector3(width,.75f,depth));
        return g;
    }

    static void Computer(Transform parent,string name,float x,int number)
    {
        var g=F.Group(parent,name,new Vector3(x,0,0));
        R(g,"Monitor_foot",new Vector3(0,.765f,.16f),new Vector3(.23f,.025f,.17f),Graphite,.025f);
        B(g,"Monitor_height_adjustable_stand",new Vector3(0,.885f,.20f),new Vector3(.044f,.22f,.042f),Graphite);
        R(g,"Understated_flat_monitor_bezel",new Vector3(0,1.105f,.20f),new Vector3(.54f,.33f,.035f),Graphite,.016f);
        B(g,"Monitor_screen_facing_student",new Vector3(0,1.105f,.1795f),new Vector3(.502f,.291f,.006f),Screen);
        B(g,"Application_title_strip",new Vector3(0,1.216f,.1755f),new Vector3(.467f,.035f,.002f),Paper);
        B(g,"Project_navigation_panel",new Vector3(-.168f,1.09f,.1755f),new Vector3(.105f,.17f,.002f),Cream);
        B(g,"Digital_project_canvas",new Vector3(.062f,1.09f,.1755f),new Vector3(.297f,.17f,.002f),Paper);
        B(g,"Student_project_coloured_block",new Vector3(.03f,1.11f,.1735f),new Vector3(.11f,.09f,.002f),number%2==0?Blue:Sage);
        F.Primitive(g,"Project_diagram_circle",PrimitiveType.Sphere,new Vector3(.12f,1.065f,.1725f),new Vector3(.075f,.075f,.002f),Yellow);
        R(g,"Compact_keyboard",new Vector3(0,.769f,-.16f),new Vector3(.35f,.028f,.13f),Graphite,.015f);
        for(int row=0;row<3;row++)
            B(g,"Keyboard_key_row",new Vector3(0,.785f,-.201f+row*.034f),new Vector3(.31f,.004f,.018f),Keys);
        B(g,"Keyboard_space_bar",new Vector3(0,.785f,-.111f),new Vector3(.15f,.004f,.012f),Keys);
        R(g,"Mouse_pad",new Vector3(.275f,.753f,-.15f),new Vector3(.17f,.005f,.19f),Cream,.015f);
        F.Primitive(g,"Compact_mouse",PrimitiveType.Sphere,new Vector3(.275f,.774f,-.15f),new Vector3(.065f,.04f,.10f),Graphite);
        B(g,"Mouse_centre_seam",new Vector3(.275f,.792f,-.174f),new Vector3(.002f,.002f,.035f),Cream);
        F.Primitive(g,"Flush_cable_grommet",PrimitiveType.Cylinder,new Vector3(.21f,.753f,.31f),new Vector3(.062f,.004f,.062f),Graphite);
        B(g,"Short_monitor_cable_to_grommet",new Vector3(.105f,.758f,.29f),new Vector3(.21f,.008f,.008f),Graphite);
        var notebook=F.Group(g,"Notebook_beside_keyboard",new Vector3(-.35f,.756f,-.12f));
        R(notebook,"Coloured_notepad_cover",Vector3.zero,new Vector3(.17f,.009f,.22f),number%2==0?Sage:Blue,.008f);
        B(notebook,"Ivory_notepad_pages",new Vector3(.004f,.008f,0),new Vector3(.157f,.009f,.207f),Paper);
        B(notebook,"Pencil",new Vector3(-.104f,.001f,-.005f),new Vector3(.007f,.007f,.16f),Oak);
        // Compact suspended PCs and their service loops remain above the floor.
        R(g,"Underdesk_compact_PC",new Vector3(.28f,.445f,.13f),new Vector3(.14f,.42f,.32f),Cream,.015f);
        B(g,"PC_hanging_bracket",new Vector3(.28f,.665f,.13f),new Vector3(.18f,.035f,.33f),Graphite);
        B(g,"PC_front_panel",new Vector3(.28f,.445f,-.032f),new Vector3(.12f,.38f,.006f),Graphite);
        F.Primitive(g,"PC_power_button",PrimitiveType.Sphere,new Vector3(.28f,.595f,-.037f),new Vector3(.018f,.018f,.004f),Sage);
        foreach(float y in new[]{.30f,.325f,.35f,.375f})
            B(g,"PC_front_vent",new Vector3(.28f,y,-.037f),new Vector3(.082f,.006f,.002f),Keys);
        B(g,"PC_power_loop_inside_cable_tray",new Vector3(.28f,.62f,.29f),new Vector3(.012f,.06f,.012f),Graphite);
    }

    static Transform TaskChair(Transform parent,string name,Vector3 pos,float yaw,Material fabric,bool teacher)
    {
        var g=F.Group(parent,name,pos,yaw);
        float lift=teacher?.08f:0f;
        R(g,teacher?"Padded_teacher_seat__top_H0.56m":"Padded_student_seat__top_H0.48m",new Vector3(0,.435f+lift,0),new Vector3(.48f,.09f,.46f),fabric,.080f);
        R(g,"Seat_support_shell",new Vector3(0,.381f+lift,-.004f),new Vector3(.46f,.025f,.435f),Cream,.065f);
        // Upright rounded extrusions give the seat an actual upholstered mid-back,
        // rather than a thin paddle or an open ring at seat height.
        var shell=R(g,"Upright_mid_back_shell",new Vector3(0,.765f+lift,-.212f),new Vector3(.475f,.048f,.455f),Cream,.07f);
        shell.transform.localRotation=Quaternion.Euler(85,0,0);
        var back=R(g,"Deep_upholstered_back_cushion",new Vector3(0,.766f+lift,-.176f),new Vector3(.45f,.067f,.42f),fabric,.07f);
        back.transform.localRotation=Quaternion.Euler(85,0,0);
        B(g,"Backrest_support",new Vector3(0,.49f+lift,-.205f),new Vector3(.075f,.25f,.03f),Graphite);
        F.Primitive(g,"Seat_height_adjustment_column",PrimitiveType.Cylinder,new Vector3(0,.225f+lift/2,0),new Vector3(.066f,.155f+lift/2,.066f),Graphite);
        F.Primitive(g,"Column_lower_sleeve",PrimitiveType.Cylinder,new Vector3(0,.13f,0),new Vector3(.085f,.065f,.085f),Cream);
        B(g,"Height_adjustment_lever",new Vector3(.205f,.365f+lift,-.025f),new Vector3(.11f,.018f,.025f),Graphite);
        if(teacher) foreach(float x in new[]{-.25f,.25f}) {
            B(g,"Low_armrest_support",new Vector3(x,.505f+lift,-.09f),new Vector3(.023f,.23f,.03f),Graphite);
            R(g,"Padded_armrest",new Vector3(x,.63f+lift,.005f),new Vector3(.045f,.04f,.29f),fabric,.02f);
        }
        for(int i=0;i<5;i++) {
            var ray=F.Group(g,"Five_star_base_spoke_"+(i+1),Vector3.zero,18+i*72);
            R(ray,"Low_base_spoke",new Vector3(0,.075f,.113f),new Vector3(.042f,.032f,.238f),Cream,.016f);
            var caster=F.Group(ray,"Twin_quiet_caster",new Vector3(0,.037f,.235f));
            B(caster,"Caster_fork",new Vector3(0,.01f,0),new Vector3(.028f,.035f,.03f),Graphite);
            foreach(float x in new[]{-.020f,.020f}) {
                var wheel=F.Primitive(caster,"Small_rubber_wheel",PrimitiveType.Cylinder,new Vector3(x,-.009f,0),new Vector3(.05f,.009f,.05f),Graphite);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
            }
        }
        // The complete body and wheels fit this compact footprint in every yaw.
        float colliderHeight=teacher?1.10f:1.0f;
        Solid(g,new Vector3(0,colliderHeight/2,0),new Vector3(.56f,colliderHeight,.56f));
        return g;
    }

    public static Transform BuildMainTeachingScreen(Transform parent)
    {
        // One 16:9 display replaces both the whiteboard and the former small screen.
        // Its lower edge is 1.10m above the floor; lesson content stays above monitor height.
        var g=F.Group(parent,"N03_Main_teaching_screen",new Vector3(-.59f,2.0f,9.0f),270);
        var glass=F.Mat("N03_Main_Display_Glass",new Color(.075f,.145f,.19f),.65f);
        var canvas=F.Mat("N03_Main_Display_Canvas",new Color(.84f,.88f,.86f),.18f);
        if(glass.HasProperty("_EmissionColor")) {
            glass.SetColor("_EmissionColor",new Color(.035f,.07f,.09f));
            glass.EnableKeyword("_EMISSION");
        }
        B(g,"Slim_graphite_display_frame__3.20x1.80m",Vector3.zero,new Vector3(3.20f,1.80f,.075f),Graphite);
        B(g,"Large_glass_display_surface",new Vector3(0,0,-.041f),new Vector3(3.08f,1.68f,.008f),glass);
        B(g,"Lesson_header_divider",new Vector3(0,.48f,-.047f),new Vector3(2.88f,.008f,.002f),Blue);
        F.WallText(g,"DIGITAL LEARNING",new Vector3(-.37f,.66f,-.049f),0,1.86f,new Color(.97f,.97f,.92f));
        F.WallText(g,"LESSON 03",new Vector3(1.15f,.65f,-.049f),0,.57f,new Color(.78f,.87f,.86f));
        B(g,"Shared_project_canvas",new Vector3(-.42f,-.10f,-.048f),new Vector3(2.04f,1.02f,.003f),canvas);
        F.WallText(g,"STUDENT PROJECT / 01",new Vector3(-.42f,.27f,-.052f),0,1.58f,new Color(.20f,.30f,.32f));
        foreach(float x in new[]{-1.03f,-.43f,.17f}) {
            B(g,"Project_workflow_card",new Vector3(x,.015f,-.052f),new Vector3(.42f,.24f,.003f),x<-.7f?Sage:x<0?Blue:Yellow);
            if(x<0) B(g,"Project_workflow_link",new Vector3(x+.30f,.015f,-.054f),new Vector3(.14f,.012f,.002f),Graphite);
        }
        F.WallText(g,"DESIGN   >   BUILD   >   SHARE",new Vector3(-.42f,-.40f,-.053f),0,1.72f,new Color(.23f,.34f,.36f));
        B(g,"Lesson_steps_sidebar",new Vector3(1.10f,-.10f,-.048f),new Vector3(.64f,1.02f,.003f),Graphite);
        F.WallText(g,"TODAY",new Vector3(1.10f,.28f,-.052f),0,.45f,new Color(.97f,.97f,.92f));
        string[] steps={"01  EXPLORE","02  CREATE","03  REVIEW"};
        for(int i=0;i<steps.Length;i++)
            F.WallText(g,steps[i],new Vector3(1.10f,.04f-i*.21f,-.052f),0,.52f,new Color(.82f,.88f,.84f));
        F.Primitive(g,"Display_status_light",PrimitiveType.Sphere,new Vector3(1.45f,-.867f,-.040f),new Vector3(.018f,.009f,.004f),Sage);
        Solid(g,Vector3.zero,new Vector3(3.20f,1.80f,.08f));
        return g;
    }

    static void TeachingPodium(Transform parent)
    {
        const float width=1.70f,depth=.72f;
        var g=F.Group(parent,"N03_Teaching_podium",new Vector3(.55f,0,11.50f),90);
        B(g,"Recessed_dark_plinth",new Vector3(0,.055f,.025f),new Vector3(1.58f,.11f,.61f),Graphite);
        R(g,"Substantial_oak_worktop__1.70x0.72m__H0.90m",new Vector3(0,.8725f,0),new Vector3(width,.055f,depth),Oak,.035f);
        B(g,"Closed_student_facing_front",new Vector3(0,.465f,.286f),new Vector3(1.62f,.76f,.045f),Cream);
        foreach(float x in new[]{-.79f,.79f})
            B(g,"Oak_side_panel",new Vector3(x,.465f,0),new Vector3(.05f,.76f,.62f),Oak);
        B(g,"Oak_front_header",new Vector3(0,.795f,.314f),new Vector3(1.62f,.10f,.025f),Oak);
        foreach(float x in new[]{-.57f,-.48f,-.39f})
            B(g,"Oak_front_accent",new Vector3(x,.43f,.314f),new Vector3(.035f,.53f,.014f),Oak);
        var cabinet=F.Group(g,"Integrated_teacher_storage",new Vector3(-.525f,0,0));
        B(cabinet,"Storage_inner_side",new Vector3(.24f,.455f,0),new Vector3(.035f,.74f,.60f),Oak);
        B(cabinet,"Storage_base",new Vector3(0,.12f,0),new Vector3(.47f,.04f,.60f),Oak);
        B(cabinet,"Drawer_divider",new Vector3(0,.66f,0),new Vector3(.47f,.025f,.60f),Oak);
        B(cabinet,"Closed_lower_cabinet_door",new Vector3(0,.38f,-.315f),new Vector3(.46f,.49f,.028f),Cream);
        B(cabinet,"Teaching_material_drawer",new Vector3(0,.743f,-.315f),new Vector3(.46f,.145f,.028f),Cream);
        foreach(float y in new[]{.55f,.744f})
            B(cabinet,"Recessed_storage_pull",new Vector3(0,y,-.333f),new Vector3(.15f,.014f,.009f),Graphite);
        B(g,"Raised_AV_control_pedestal",new Vector3(-.54f,.945f,.045f),new Vector3(.43f,.09f,.30f),Cream);
        var controls=F.Group(g,"Angled_teaching_control_panel",new Vector3(-.54f,.999f,.045f));
        controls.localRotation=Quaternion.Euler(-12,0,0);
        R(controls,"Control_panel_bezel",Vector3.zero,new Vector3(.39f,.022f,.26f),Graphite,.020f);
        B(controls,"AV_touch_display",new Vector3(-.045f,.013f,.02f),new Vector3(.25f,.004f,.17f),Screen);
        foreach(float z in new[]{-.065f,0,.065f})
            B(controls,"Physical_AV_button",new Vector3(.145f,.015f,z),new Vector3(.045f,.009f,.035f),z>0?Sage:Paper);
        B(g,"Lesson_folder",new Vector3(-.01f,.914f,.025f),new Vector3(.22f,.027f,.28f),Blue);
        R(g,"Teaching_laptop_base",new Vector3(.43f,.914f,-.025f),new Vector3(.40f,.025f,.28f),Graphite,.015f);
        B(g,"Laptop_keyboard",new Vector3(.43f,.929f,.005f),new Vector3(.32f,.004f,.12f),Cream);
        R(g,"Laptop_trackpad",new Vector3(.43f,.929f,-.105f),new Vector3(.11f,.004f,.055f),Cream,.008f);
        var lid=F.Group(g,"Open_teaching_laptop_screen",new Vector3(.43f,1.059f,.128f));
        lid.localRotation=Quaternion.Euler(-12,0,0);
        R(lid,"Laptop_lid",Vector3.zero,new Vector3(.40f,.255f,.018f),Graphite,.010f);
        B(lid,"Laptop_screen",new Vector3(0,0,-.011f),new Vector3(.36f,.215f,.004f),Screen);
        B(lid,"Digital_lesson_on_laptop",new Vector3(0,.05f,-.014f),new Vector3(.28f,.05f,.002f),Paper);
        F.Primitive(g,"Microphone_base",PrimitiveType.Cylinder,new Vector3(.72f,.917f,.22f),new Vector3(.10f,.017f,.10f),Graphite);
        Vector3 a=new Vector3(.72f,.93f,.22f),b=new Vector3(.70f,1.13f,.19f),c=new Vector3(.68f,1.22f,.08f);
        MicrophoneStem(g,a,b);MicrophoneStem(g,b,c);
        var head=F.Primitive(g,"Gooseneck_microphone_head",PrimitiveType.Capsule,c,new Vector3(.028f,.036f,.028f),Graphite);
        head.transform.localRotation=Quaternion.FromToRotation(Vector3.up,c-b);
        Solid(g,new Vector3(0,.45f,0),new Vector3(width,.90f,depth));
    }

    static void MicrophoneStem(Transform parent,Vector3 from,Vector3 to)
    {
        var stem=F.Primitive(parent,"Gooseneck_microphone_stem",PrimitiveType.Cylinder,(from+to)/2,new Vector3(.012f,Vector3.Distance(from,to)/2,.012f),Graphite);
        stem.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
    }

    static void ResourceStorage(Transform parent)
    {
        const float width=3.0f,height=.82f,depth=.42f;
        var g=F.Group(parent,"N03_Low_resource_storage",new Vector3(10.055f,0,9.35f),90);
        B(g,"Recessed_plinth",new Vector3(0,.045f,.01f),new Vector3(width-.10f,.09f,depth-.06f),Graphite);
        B(g,"Timber_cabinet_back",new Vector3(0,.435f,.192f),new Vector3(width,.69f,.036f),Oak);
        foreach(float x in new[]{-1.48f,1.48f})
            B(g,"Timber_cabinet_end",new Vector3(x,.435f,0),new Vector3(.04f,.69f,depth),Oak);
        B(g,"Timber_cabinet_base",new Vector3(0,.12f,0),new Vector3(width,.04f,depth),Oak);
        R(g,"Low_oak_worktop",new Vector3(0,.80f,0),new Vector3(width+.025f,.04f,depth+.015f),Oak,.020f);
        // Three closed resource bays plus one open cubby keep the rear wall useful.
        for(int i=0;i<3;i++) {
            float x=-1.125f+i*.75f;
            R(g,"Closed_resource_cupboard_door",new Vector3(x,.435f,-.217f),new Vector3(.725f,.66f,.025f),Cream,.010f);
            B(g,"Recessed_finger_pull",new Vector3(x,.69f,-.232f),new Vector3(.18f,.012f,.003f),Graphite);
        }
        B(g,"Open_cubby_divider",new Vector3(.75f,.435f,0),new Vector3(.03f,.65f,depth),Oak);
        B(g,"Open_cubby_recess",new Vector3(1.125f,.435f,.17f),new Vector3(.70f,.65f,.008f),Graphite);
        B(g,"Open_cubby_middle_shelf",new Vector3(1.125f,.46f,0),new Vector3(.70f,.025f,.39f),Oak);
        for(int i=0;i<6;i++) {
            float h=.23f+(i%3)*.02f;
            B(g,"Shared_digital_course_book",new Vector3(.87f+i*.085f,.14f+h/2,-.085f),new Vector3(.06f,h,.23f),i%3==0?Blue:i%3==1?Sage:Paper);
        }
        foreach(float y in new[]{.50f,.555f,.61f})
            R(g,"Stacked_project_document_box",new Vector3(1.11f,y,-.015f),new Vector3(.51f,.045f,.29f),y>.59f?Blue:Cream,.009f);
        Solid(g,new Vector3(0,height/2,0),new Vector3(width+.025f,height,depth+.04f));
    }

    static void ProjectBoard(Transform parent,string name,float z,string heading,bool diagram)
    {
        var g=F.Group(parent,name,new Vector3(10.255f,1.70f,z),90);
        B(g,"Thin_timber_project_frame",Vector3.zero,new Vector3(1.6f,1.15f,.035f),Oak);
        B(g,"Cork_project_backing",new Vector3(0,0,-.022f),new Vector3(1.55f,1.10f,.012f),Cork);
        F.WallText(g,heading,new Vector3(0,.43f,-.031f),0,1.30f,new Color(.96f,.95f,.86f));
        foreach(float x in new[]{-.39f,.39f}) {
            var sheet=F.Group(g,"Pinned_student_project_sheet",new Vector3(x,-.07f,-.036f));
            sheet.localRotation=Quaternion.Euler(0,0,x<0?-3:3);
            B(sheet,"Ivory_project_paper",Vector3.zero,new Vector3(.60f,.70f,.004f),Paper);
            F.Primitive(sheet,"Coloured_board_pin",PrimitiveType.Sphere,new Vector3(0,.31f,-.005f),new Vector3(.025f,.025f,.015f),Blue);
            B(sheet,"Student_project_title",new Vector3(0,.23f,-.004f),new Vector3(.44f,.045f,.002f),Blue);
            B(sheet,"Project_image_area",new Vector3(0,.015f,-.004f),new Vector3(.45f,.28f,.002f),Cream);
            if(diagram) {
                foreach(float dx in new[]{-.12f,0,.12f}) {
                    B(sheet,"Student_diagram_node",new Vector3(dx,.02f,-.007f),new Vector3(.09f,.10f,.002f),dx<0?Sage:dx>0?Blue:Yellow);
                    if(dx<.1f) B(sheet,"Student_diagram_link",new Vector3(dx+.06f,.02f,-.008f),new Vector3(.05f,.01f,.002f),Graphite);
                }
            } else {
                B(sheet,"Student_designed_interface",new Vector3(-.075f,.025f,-.007f),new Vector3(.19f,.19f,.002f),Sage);
                F.Primitive(sheet,"Student_designed_icon",PrimitiveType.Sphere,new Vector3(.105f,.025f,-.008f),new Vector3(.14f,.14f,.002f),Yellow);
            }
            foreach(float y in new[]{-.19f,-.24f})
                B(sheet,"Student_project_caption",new Vector3(-.01f,y,-.006f),new Vector3(.41f,.009f,.002f),Graphite);
        }
        Solid(g,Vector3.zero,new Vector3(1.60f,1.15f,.07f));
    }
}
