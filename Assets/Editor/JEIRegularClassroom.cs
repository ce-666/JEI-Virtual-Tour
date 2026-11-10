using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>N01: a 24-seat everyday classroom, facing the east teaching wall.</summary>
public static class JEIRegularClassroom
{
    static Material Oak { get { return F.Mat("N01_Warm_Oak",new Color(.74f,.58f,.38f),.23f); } }
    static Material Cream { get { return F.Mat("N01_Cream_Frame",new Color(.86f,.86f,.79f),.24f); } }
    static Material Graphite { get { return F.Mat("N01_Graphite_Detail",new Color(.22f,.28f,.29f),.30f); } }
    static Material Sage { get { return F.Mat("N01_Chair_Sage",new Color(.46f,.61f,.52f),.17f); } }
    static Material Blue { get { return F.Mat("N01_Chair_Dusty_Blue",new Color(.43f,.57f,.66f),.17f); } }
    static Material Yellow { get { return F.Mat("N01_Chair_Muted_Yellow",new Color(.79f,.70f,.42f),.17f); } }
    static Material White { get { return F.Mat("N01_Whiteboard_White",new Color(.96f,.97f,.93f),.36f); } }
    static Material Paper { get { return F.Mat("N01_Paper_Ivory",new Color(.96f,.94f,.85f),.09f); } }
    static Material Cork { get { return F.Mat("N01_Warm_Cork",new Color(.66f,.52f,.35f),.08f); } }
    static Material Screen { get { return F.Mat("N01_Screen_Pale_Blue",new Color(.61f,.76f,.80f),.25f); } }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material mat)
    { return F.Box(p,n,pos,size,mat,false); }
    static GameObject R(Transform p,string n,Vector3 pos,Vector3 size,Material mat,float radius=.025f)
    { return F.Rounded(p,n,pos,size,mat,radius); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static void Build(Transform parent)
    {
        var room=F.Group(parent,"N01__Everyday_teaching__24_seats_in_pairs");
        var desks=F.Group(room,"Twelve_mobile_double_student_desks__four_rows_three_columns");
        var chairs=F.Group(room,"Twenty_four_lightweight_student_chairs__faces_east");
        int deskNumber=0,chairNumber=0;
        foreach(float x in new[]{-11.45f,-12.80f,-14.15f,-15.50f})
            foreach(float z in new[]{7.30f,9.35f,11.40f}) {
                deskNumber++;
                var desk=Desk(desks,"N01_Double_student_desk_"+deskNumber.ToString("00"),new Vector3(x,0,z),90,1.20f,.55f);
                Notebook(desk,new Vector3(-.30f,.745f,-.015f),deskNumber%2==0?Sage:Blue);
                Notebook(desk,new Vector3(.30f,.745f,.015f),deskNumber%3==0?Yellow:Sage);
                foreach(float offset in new[]{-.30f,.30f}) {
                    chairNumber++;
                    var color=chairNumber%6==0?Yellow:chairNumber%2==0?Blue:Sage;
                    StudentChair(chairs,"N01_Student_chair_"+chairNumber.ToString("00"),new Vector3(x-.70f,0,z+offset),90,color);
                }
            }

        Whiteboard(room);
        TeachingDisplay(room);
        BuildTeachingPodium(room);
        BuildTeacherChair(room);
        ResourceStorage(room);
        WorkBoard(room,"N01_Student_work_board_01",6.85f,"COURSE NOTES",false);
        WorkBoard(room,"N01_Student_work_board_02",11.90f,"STUDENT WORK",true);
    }

    public static Transform BuildTeachingPodium(Transform parent)
    {
        // The closed student-facing front and operator-side storage distinguish this
        // teaching station from the light, open-framed student tables. No raised floor.
        const float width=1.70f,depth=.72f;
        var g=F.Group(parent,"N01_Teaching_podium",new Vector3(-10.55f,0,11.65f),270);
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

        // Slightly inclined teaching controls, with the screen facing the operator.
        B(g,"Raised_AV_control_pedestal",new Vector3(-.54f,.945f,.045f),new Vector3(.43f,.09f,.30f),Cream);
        var controls=F.Group(g,"Angled_teaching_control_panel",new Vector3(-.54f,.999f,.045f));
        controls.localRotation=Quaternion.Euler(-12,0,0);
        R(controls,"Control_panel_bezel",Vector3.zero,new Vector3(.39f,.022f,.26f),Graphite,.020f);
        B(controls,"AV_touch_display",new Vector3(-.045f,.013f,.02f),new Vector3(.25f,.004f,.17f),Screen);
        foreach(float z in new[]{-.065f,0,.065f})
            B(controls,"Physical_AV_button",new Vector3(.145f,.015f,z),new Vector3(.045f,.009f,.035f),z>0?Sage:Paper);

        Notebook(g,new Vector3(-.09f,.906f,-.01f),Blue);
        B(g,"Lesson_folder",new Vector3(.13f,.915f,.025f),new Vector3(.18f,.026f,.25f),Paper);
        R(g,"Teaching_laptop_base",new Vector3(.46f,.914f,-.025f),new Vector3(.40f,.025f,.28f),Graphite,.015f);
        B(g,"Laptop_keyboard",new Vector3(.46f,.929f,.005f),new Vector3(.32f,.004f,.12f),Cream);
        R(g,"Laptop_trackpad",new Vector3(.46f,.929f,-.105f),new Vector3(.11f,.004f,.055f),Cream,.008f);
        var lid=F.Group(g,"Open_teaching_laptop_screen",new Vector3(.46f,1.059f,.128f));
        lid.localRotation=Quaternion.Euler(-12,0,0);
        R(lid,"Laptop_lid",Vector3.zero,new Vector3(.40f,.255f,.018f),Graphite,.010f);
        B(lid,"Laptop_screen",new Vector3(0,0,-.011f),new Vector3(.36f,.215f,.004f),Screen);
        B(lid,"Lesson_on_laptop",new Vector3(0,.05f,-.014f),new Vector3(.28f,.05f,.002f),Paper);

        F.Primitive(g,"Microphone_base",PrimitiveType.Cylinder,new Vector3(.72f,.917f,.22f),new Vector3(.10f,.017f,.10f),Graphite);
        Vector3 a=new Vector3(.72f,.93f,.22f),b=new Vector3(.70f,1.13f,.19f),c=new Vector3(.68f,1.22f,.08f);
        MicrophoneStem(g,a,b); MicrophoneStem(g,b,c);
        var head=F.Primitive(g,"Gooseneck_microphone_head",PrimitiveType.Capsule,c,new Vector3(.028f,.036f,.028f),Graphite);
        head.transform.localRotation=Quaternion.FromToRotation(Vector3.up,c-b);
        Solid(g,new Vector3(0,.45f,0),new Vector3(width,.90f,depth));
        return g;
    }

    static void MicrophoneStem(Transform parent,Vector3 from,Vector3 to)
    {
        var stem=F.Primitive(parent,"Gooseneck_microphone_stem",PrimitiveType.Cylinder,(from+to)/2,new Vector3(.012f,Vector3.Distance(from,to)/2,.012f),Graphite);
        stem.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
    }

    public static Transform BuildTeacherChair(Transform parent)
    {
        var g=F.Group(parent,"N01_Teacher_chair",new Vector3(-9.82f,0,12.05f),270);
        var fabric=F.Mat("N01_Teacher_Sage_Upholstery",new Color(.44f,.57f,.49f),.10f);
        R(g,"Compact_upholstered_seat__H0.56m",new Vector3(0,.515f,0),new Vector3(.50f,.09f,.47f),fabric,.085f);
        R(g,"Seat_support_shell",new Vector3(0,.461f,-.005f),new Vector3(.47f,.025f,.44f),Cream,.065f);
        var backShell=R(g,"Rounded_mid_back_shell",new Vector3(0,.85f,-.211f),new Vector3(.49f,.055f,.48f),Cream,.075f);
        backShell.transform.localRotation=Quaternion.Euler(83,0,0);
        var backCushion=R(g,"Contoured_upholstered_backrest",new Vector3(0,.852f,-.173f),new Vector3(.465f,.065f,.445f),fabric,.075f);
        backCushion.transform.localRotation=Quaternion.Euler(83,0,0);
        B(g,"Continuous_backrest_support",new Vector3(0,.565f,-.205f),new Vector3(.09f,.24f,.035f),Graphite);
        foreach(float x in new[]{-.266f,.266f}) {
            B(g,"Low_armrest_support",new Vector3(x,.592f,-.105f),new Vector3(.025f,.23f,.035f),Graphite);
            R(g,"Slim_padded_armrest",new Vector3(x,.715f,.005f),new Vector3(.052f,.045f,.31f),fabric,.025f);
        }
        F.Primitive(g,"Height_adjustable_column",PrimitiveType.Cylinder,new Vector3(0,.27f,0),new Vector3(.066f,.18f,.066f),Graphite);
        F.Primitive(g,"Column_lower_sleeve",PrimitiveType.Cylinder,new Vector3(0,.147f,0),new Vector3(.087f,.085f,.087f),Cream);
        B(g,"Seat_height_adjustment_lever",new Vector3(.21f,.433f,-.03f),new Vector3(.13f,.018f,.03f),Graphite);
        for(int i=0;i<5;i++) {
            float yaw=18+i*72;
            var ray=F.Group(g,"Five_star_base_arm_"+(i+1),Vector3.zero,yaw);
            R(ray,"Low_base_spoke",new Vector3(0,.082f,.119f),new Vector3(.047f,.035f,.254f),Cream,.018f);
            var caster=F.Group(ray,"Twin_quiet_caster",new Vector3(0,.041f,.25f));
            B(caster,"Caster_fork",new Vector3(0,.012f,0),new Vector3(.035f,.043f,.035f),Graphite);
            foreach(float x in new[]{-.023f,.023f}) {
                var wheel=F.Primitive(caster,"Small_rubber_wheel",PrimitiveType.Cylinder,new Vector3(x,-.010f,0),new Vector3(.055f,.012f,.055f),Graphite);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
            }
        }
        // Compact footprint leaves the side approach to the teacher position open.
        Solid(g,new Vector3(0,.55f,-.006f),new Vector3(.59f,1.10f,.58f));
        return g;
    }

    static Transform Desk(Transform parent,string name,Vector3 pos,float yaw,float width,float depth)
    {
        var g=F.Group(parent,name,pos,yaw);
        R(g,"Light_oak_soft_corner_worktop__H0.74m",new Vector3(0,.7175f,0),new Vector3(width,.045f,depth),Oak,.045f);
        foreach(float x in new[]{-width/2+.085f,width/2-.085f}) {
            foreach(float z in new[]{-depth/2+.075f,depth/2-.075f}) {
                B(g,"Light_steel_table_leg",new Vector3(x,.348f,z),new Vector3(.032f,.688f,.032f),Cream);
                B(g,"Protective_floor_glide",new Vector3(x,.009f,z),new Vector3(.036f,.018f,.036f),Graphite);
            }
            B(g,"End_frame_beneath_worktop",new Vector3(x,.67f,0),new Vector3(.032f,.045f,depth-.12f),Cream);
        }
        B(g,"Slim_rear_stiffening_rail",new Vector3(0,.67f,depth/2-.075f),new Vector3(width-.14f,.045f,.025f),Cream);
        Solid(g,new Vector3(0,.37f,0),new Vector3(width,.74f,depth));
        return g;
    }

    static Transform StudentChair(Transform parent,string name,Vector3 pos,float yaw,Material shell)
    {
        var g=F.Group(parent,name,pos,yaw);
        R(g,"Matte_moulded_seat",new Vector3(0,.4575f,0),new Vector3(.46f,.045f,.445f),shell,.075f);
        // Rotate a rounded extrusion upright so the back has soft corners in elevation.
        var back=R(g,"Light_moulded_backrest",new Vector3(0,.685f,-.201f),new Vector3(.435f,.032f,.35f),shell,.060f);
        back.transform.localRotation=Quaternion.Euler(83,0,0);
        foreach(float x in new[]{-.174f,.174f}) {
            foreach(float z in new[]{-.16f,.16f}) {
                var leg=B(g,"Slender_oak_leg",new Vector3(x,.217f,z),new Vector3(.028f,.434f,.028f),Oak);
                leg.transform.localRotation=Quaternion.Euler(z>0?3:-3,0,x>0?-3:3);
                B(g,"Quiet_floor_glide",new Vector3(x+(x>0?.011f:-.011f),.008f,z+(z>0?.011f:-.011f)),new Vector3(.032f,.016f,.032f),Graphite);
            }
            B(g,"Backrest_support",new Vector3(x,.535f,-.195f),new Vector3(.020f,.19f,.024f),Cream);
        }
        B(g,"Seat_underframe",new Vector3(0,.423f,0),new Vector3(.37f,.025f,.34f),Cream);
        Solid(g,new Vector3(0,.43f,-.006f),new Vector3(.46f,.86f,.48f));
        return g;
    }

    static void Notebook(Transform parent,Vector3 pos,Material cover)
    {
        var g=F.Group(parent,"Exercise_book_and_pencil",pos);
        R(g,"Coloured_notebook_cover",new Vector3(0,.002f,0),new Vector3(.20f,.008f,.255f),cover,.010f);
        B(g,"Paper_block",new Vector3(.004f,.0085f,0),new Vector3(.188f,.008f,.244f),Paper);
        B(g,"Closed_book_top",new Vector3(0,.014f,0),new Vector3(.20f,.003f,.255f),cover);
        B(g,"Pencil",new Vector3(.135f,.003f,-.015f),new Vector3(.008f,.008f,.18f),Oak);
        B(g,"Pencil_graphite_tip",new Vector3(.135f,.003f,-.109f),new Vector3(.006f,.006f,.013f),Graphite);
    }

    static void Whiteboard(Transform parent)
    {
        var g=F.Group(parent,"N01_Main_whiteboard",new Vector3(-9.45f,1.65f,8.85f),90);
        const float width=2.65f,height=1.25f;
        B(g,"Fine_aluminium_frame",Vector3.zero,new Vector3(width+.06f,height+.06f,.035f),Cream);
        B(g,"Writable_white_surface",new Vector3(0,0,-.024f),new Vector3(width,height,.015f),White);
        R(g,"Whiteboard_pen_tray",new Vector3(0,-height/2-.055f,-.082f),new Vector3(1.05f,.028f,.13f),Cream,.012f);
        foreach(float x in new[]{-.25f,0,.25f})
            B(g,"Board_marker",new Vector3(x,-height/2-.032f,-.08f),new Vector3(.115f,.018f,.018f),x<0?Blue:x>0?Sage:Graphite);
        Solid(g,Vector3.zero,new Vector3(width+.06f,height+.06f,.08f));
        F.WallText(g,"TODAY / LEARN + DISCUSS",new Vector3(-.05f,.37f,-.033f),0,1.88f,new Color(.28f,.37f,.38f));
        // A few restrained lesson annotations leave most of the writing area empty.
        B(g,"Lesson_heading_underline",new Vector3(-.15f,.235f,-.034f),new Vector3(1.65f,.008f,.002f),Blue);
        foreach(float y in new[]{.08f,-.08f,-.24f}) {
            F.Primitive(g,"Lesson_bullet",PrimitiveType.Sphere,new Vector3(-.97f,y,-.035f),new Vector3(.022f,.022f,.003f),Sage);
            B(g,"Short_lesson_note",new Vector3(-.56f,y,-.035f),new Vector3(.63f,.012f,.002f),Graphite);
        }
    }

    static void TeachingDisplay(Transform parent)
    {
        var g=F.Group(parent,"N01_Teaching_display",new Vector3(-9.45f,1.85f,11.25f),90);
        B(g,"Slim_display_bezel",Vector3.zero,new Vector3(1.35f,.85f,.045f),Graphite);
        B(g,"Teaching_slide_surface",new Vector3(0,0,-.025f),new Vector3(1.275f,.775f,.008f),Screen);
        B(g,"Slide_title_area",new Vector3(0,.23f,-.031f),new Vector3(1.13f,.15f,.003f),Paper);
        F.WallText(g,"LEARNING TOGETHER",new Vector3(0,.23f,-.034f),0,.96f,new Color(.24f,.34f,.36f));
        foreach(float x in new[]{-.38f,0,.38f}) {
            B(g,"Slide_diagram_card",new Vector3(x,-.08f,-.031f),new Vector3(.28f,.30f,.003f),Paper);
            F.Primitive(g,"Slide_diagram_circle",PrimitiveType.Sphere,new Vector3(x,-.02f,-.034f),new Vector3(.10f,.10f,.003f),x<0?Blue:x>0?Yellow:Sage);
            B(g,"Slide_diagram_caption",new Vector3(x,-.16f,-.035f),new Vector3(.19f,.012f,.002f),Graphite);
        }
        Solid(g,Vector3.zero,new Vector3(1.35f,.85f,.07f));
    }

    static void ResourceStorage(Transform parent)
    {
        const float width=2.20f,height=.76f,depth=.40f;
        var g=F.Group(parent,"N01_Low_resource_storage",new Vector3(-17.43f,0,9.40f),270);
        B(g,"Recessed_plinth",new Vector3(0,.045f,.01f),new Vector3(width-.10f,.09f,depth-.06f),Graphite);
        B(g,"Timber_cabinet_back",new Vector3(0,.405f,.182f),new Vector3(width,.63f,.036f),Oak);
        foreach(float x in new[]{-1.08f,1.08f})
            B(g,"Timber_cabinet_end",new Vector3(x,.405f,0),new Vector3(.04f,.63f,depth),Oak);
        B(g,"Timber_cabinet_base",new Vector3(0,.12f,0),new Vector3(width,.04f,depth),Oak);
        B(g,"Open_cubby_divider",new Vector3(.55f,.405f,0),new Vector3(.03f,.59f,depth),Oak);
        R(g,"Low_oak_top_below_window_sill",new Vector3(0,.74f,0),new Vector3(width+.025f,.04f,depth+.015f),Oak,.020f);
        // Three closed bays and one open book cubby make the low cabinet useful without visual clutter.
        for(int i=0;i<3;i++) {
            float x=-width/2+.275f+i*.55f;
            R(g,"Closed_resource_cupboard_door",new Vector3(x,.405f,-.207f),new Vector3(.525f,.60f,.025f),Cream,.010f);
            B(g,"Recessed_finger_pull",new Vector3(x,.64f,-.222f),new Vector3(.15f,.012f,.003f),Graphite);
        }
        B(g,"Open_cubby_recess",new Vector3(.825f,.405f,.16f),new Vector3(.51f,.59f,.008f),Graphite);
        B(g,"Open_cubby_base",new Vector3(.825f,.12f,-.015f),new Vector3(.51f,.025f,.39f),Oak);
        for(int b=0;b<5;b++) {
            float h=.28f+(b%3)*.025f;
            B(g,"Shared_course_reference_book",new Vector3(.655f+b*.075f,.135f+h/2,-.085f),new Vector3(.055f,h,.23f),b%3==0?Blue:b%3==1?Sage:Paper);
        }
        Solid(g,new Vector3(0,height/2,0),new Vector3(width+.025f,height,depth+.045f));
    }

    static void WorkBoard(Transform parent,string name,float z,string heading,bool artwork)
    {
        var g=F.Group(parent,name,new Vector3(-17.67f,1.65f,z),270);
        B(g,"Thin_timber_notice_frame",Vector3.zero,new Vector3(1.15f,.95f,.035f),Oak);
        B(g,"Cork_display_backing",new Vector3(0,0,-.022f),new Vector3(1.10f,.90f,.012f),Cork);
        F.WallText(g,heading,new Vector3(0,.34f,-.031f),0,.91f,new Color(.96f,.95f,.86f));
        foreach(float x in new[]{-.27f,.27f}) {
            var sheet=F.Group(g,artwork?"Student_project_sheet":"Course_notice_sheet",new Vector3(x,-.08f,-.036f));
            sheet.localRotation=Quaternion.Euler(0,0,x<0?-3:3);
            B(sheet,"Pinned_paper",Vector3.zero,new Vector3(.42f,.53f,.004f),Paper);
            F.Primitive(sheet,"Pin",PrimitiveType.Sphere,new Vector3(0,.23f,-.005f),new Vector3(.022f,.022f,.015f),Blue);
            if(artwork) {
                B(sheet,"Student_colour_study",new Vector3(-.065f,.065f,-.004f),new Vector3(.14f,.18f,.002f),Sage);
                B(sheet,"Student_colour_study",new Vector3(.075f,.015f,-.005f),new Vector3(.14f,.22f,.002f),Blue);
                F.Primitive(sheet,"Student_colour_study",PrimitiveType.Sphere,new Vector3(0,.13f,-.006f),new Vector3(.11f,.11f,.002f),Yellow);
            } else {
                B(sheet,"Notice_heading",new Vector3(0,.14f,-.004f),new Vector3(.30f,.04f,.002f),Blue);
                foreach(float y in new[]{.035f,-.04f,-.115f})
                    B(sheet,"Course_information_line",new Vector3(-.015f,y,-.004f),new Vector3(.28f,.012f,.002f),Graphite);
            }
            B(sheet,"Project_caption",new Vector3(0,-.20f,-.006f),new Vector3(.28f,.009f,.002f),Graphite);
        }
        Solid(g,Vector3.zero,new Vector3(1.15f,.95f,.07f));
    }
}
