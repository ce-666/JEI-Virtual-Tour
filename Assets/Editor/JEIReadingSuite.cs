using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>N04: a six-person project studio with an L booth, movable tables and model-making bench.</summary>
public static class JEIReadingSuite
{
    static Material Oak { get { return F.Mat("N04_Project_Warm_Oak",new Color(.74f,.58f,.38f),.23f); } }
    static Material Cream { get { return F.Mat("N04_Project_Cream_Frame",new Color(.88f,.87f,.80f),.20f); } }
    static Material Graphite { get { return F.Mat("N04_Project_Graphite_Detail",new Color(.24f,.29f,.29f),.27f); } }
    static Material Sage { get { return F.Mat("N04_Project_Sage_Upholstery",new Color(.47f,.61f,.51f),.10f); } }
    static Material Oatmeal { get { return F.Mat("N04_Project_Oatmeal_Upholstery",new Color(.76f,.71f,.60f),.10f); } }
    static Material Blue { get { return F.Mat("N04_Project_Dusty_Blue",new Color(.43f,.57f,.65f),.17f); } }
    static Material Clay { get { return F.Mat("N04_Project_Clay_Accent",new Color(.69f,.43f,.31f),.16f); } }
    static Material Paper { get { return F.Mat("N04_Project_Paper_Ivory",new Color(.96f,.94f,.85f),.09f); } }
    static Material Card { get { return F.Mat("N04_Project_Cardboard",new Color(.76f,.67f,.50f),.13f); } }
    static Material Cork { get { return F.Mat("N04_Project_Warm_Cork",new Color(.65f,.52f,.35f),.08f); } }
    static Material White { get { return F.Mat("N04_Project_Whiteboard",new Color(.96f,.97f,.93f),.36f); } }

    static GameObject B(Transform p,string n,Vector3 pos,Vector3 size,Material m)
    { return F.Box(p,n,pos,size,m,false); }
    static GameObject R(Transform p,string n,Vector3 pos,Vector3 size,Material m,float radius=.025f)
    { return F.Rounded(p,n,pos,size,m,radius); }
    static void Solid(Transform p,Vector3 centre,Vector3 size)
    { var c=p.gameObject.AddComponent<BoxCollider>();c.center=centre;c.size=size; }

    public static void Build(Transform parent)
    {
        var room=F.Group(parent,"N04__Small_group_project_studio__6_places");
        ProjectBooth(room);
        SharedProjectTable(room);
        ProjectChair(room,"N04_Project_chair_01",new Vector3(12.75f,0,9.05f),0,Sage);
        ProjectChair(room,"N04_Project_chair_02",new Vector3(14.05f,0,9.05f),0,Oatmeal);
        ProjectChair(room,"N04_Project_chair_03",new Vector3(15.20f,0,10.25f),270,Sage);
        CollaborationWhiteboard(room);
        TeamMaterialStorage(room);
        ProgressBoard(room);
        WindowWorkbench(room);
        IdentityBoard(room);
        MaterialCart(room);
    }

    static void ProjectBooth(Transform parent)
    {
        var g=F.Group(parent,"N04_Project_booth");
        var north=F.Group(g,"Booth_north_segment",new Vector3(12.775f,0,11.65f));
        B(north,"Inset_cream_plinth",new Vector3(0,.07f,0),new Vector3(2.81f,.14f,.61f),Cream);
        R(north,"Warm_oak_seat_frame",new Vector3(0,.275f,0),new Vector3(2.95f,.27f,.72f),Oak,.035f);
        R(north,"Low_north_upholstered_back",new Vector3(0,.715f,.28f),new Vector3(2.95f,.16f,.77f),Sage,.055f).transform.localRotation=Quaternion.Euler(90,0,0);
        R(north,"Continuous_west_corner_back",new Vector3(-1.40f,.715f,-.08f),new Vector3(.15f,.77f,.56f),Sage,.045f);
        int number=0;
        foreach(float x in new[]{12.65f,13.60f}) {
            number++;
            var seat=F.Group(north,"N04_Booth_seat_"+number.ToString("00"),new Vector3(x-12.775f,0,0));
            R(seat,"North_individual_seat_cushion",new Vector3(0,.4325f,-.07f),new Vector3(.925f,.075f,.54f),Sage,.055f);
            R(seat,"North_individual_back_pad",new Vector3(0,.76f,.165f),new Vector3(.91f,.075f,.46f),Sage,.05f).transform.localRotation=Quaternion.Euler(90,0,0);
        }
        R(north,"L_corner_seat_transition",new Vector3(-.975f,.4325f,-.07f),new Vector3(.70f,.075f,.54f),Sage,.045f);
        B(north,"Exposed_oak_front_edge",new Vector3(0,.365f,-.359f),new Vector3(2.95f,.055f,.018f),Oak);
        Solid(north,new Vector3(0,.55f,0),new Vector3(2.95f,1.10f,.72f));

        // The return ends exactly at the north segment's front edge. Its base
        // and collider leave the inside of the L available to the project table.
        var west=F.Group(g,"Booth_west_return",new Vector3(11.65f,0,10.47f));
        B(west,"Inset_cream_plinth",new Vector3(0,.07f,0),new Vector3(.61f,.14f,1.52f),Cream);
        R(west,"Warm_oak_return_frame",new Vector3(0,.275f,0),new Vector3(.72f,.27f,1.64f),Oak,.035f);
        R(west,"Low_west_upholstered_back",new Vector3(-.28f,.715f,0),new Vector3(.16f,.77f,1.64f),Sage,.05f);
        var westSeat=F.Group(west,"N04_Booth_seat_03",new Vector3(0,0,-.12f));
        R(westSeat,"West_individual_seat_cushion",new Vector3(.07f,.4325f,0),new Vector3(.54f,.075f,1.055f),Sage,.055f);
        R(westSeat,"West_individual_back_pad",new Vector3(-.165f,.76f,0),new Vector3(.075f,.46f,1.02f),Sage,.045f);
        R(west,"West_corner_seat_transition",new Vector3(.07f,.4325f,.62375f),new Vector3(.54f,.075f,.3925f),Sage,.04f);
        B(west,"Exposed_oak_inner_edge",new Vector3(.359f,.365f,0),new Vector3(.018f,.055f,1.64f),Oak);
        Solid(west,new Vector3(0,.55f,0),new Vector3(.72f,1.10f,1.64f));
    }

    static void SharedProjectTable(Transform parent)
    {
        var g=F.Group(parent,"N04_Shared_project_table",new Vector3(13.45f,0,10.30f));
        int moduleNumber=0;
        foreach(float x in new[]{-.605f,.605f}) {
            moduleNumber++;
            var module=F.Group(g,"N04_Mobile_table_module_"+moduleNumber.ToString("00"),new Vector3(x,0,0));
            R(module,"Light_oak_module_top__1.19x1.50m__H0.75m",new Vector3(0,.7225f,0),new Vector3(1.19f,.055f,1.50f),Oak,.055f);
            foreach(float legX in new[]{-.48f,.48f}) {
                B(module,"Inset_end_frame",new Vector3(legX,.662f,0),new Vector3(.04f,.055f,1.16f),Cream);
                foreach(float z in new[]{-.56f,.56f}) {
                    B(module,"Slim_cream_mobile_table_leg",new Vector3(legX,.375f,z),new Vector3(.04f,.60f,.04f),Cream);
                    B(module,"Locking_caster_fork",new Vector3(legX,.058f,z),new Vector3(.04f,.065f,.04f),Graphite);
                    var wheel=F.Primitive(module,"Quiet_locking_caster",PrimitiveType.Cylinder,new Vector3(legX,.035f,z),new Vector3(.07f,.018f,.07f),Graphite);
                    wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                    B(module,"Engaged_caster_brake",new Vector3(legX,.072f,z-.025f),new Vector3(.035f,.012f,.035f),Cream);
                }
            }
            B(module,"Recessed_central_support",new Vector3(0,.655f,0),new Vector3(.96f,.055f,.045f),Cream);
        }
        foreach(float z in new[]{-.48f,.48f})
            B(g,"Under_table_joining_clip",new Vector3(0,.675f,z),new Vector3(.16f,.025f,.05f),Graphite);
        var drawing=F.Group(g,"Shared_campus_plan",new Vector3(-.35f,.753f,-.05f));
        R(drawing,"Large_shared_project_sheet",Vector3.zero,new Vector3(1.15f,.004f,.68f),Paper,.008f);
        foreach(float x in new[]{-.27f,.02f,.28f}) {
            B(drawing,"Plan_building_outline",new Vector3(x,.003f,.035f),new Vector3(.17f,.002f,.22f),Sage);
            B(drawing,"Plan_building_infill",new Vector3(x,.005f,.035f),new Vector3(.14f,.002f,.19f),Paper);
        }
        B(drawing,"Plan_shared_walkway",new Vector3(0,.003f,-.14f),new Vector3(.85f,.002f,.04f),Card);
        foreach(float x in new[]{-.43f,.43f})
            F.Primitive(drawing,"Plan_landscape_circle",PrimitiveType.Cylinder,new Vector3(x,.004f,.20f),new Vector3(.08f,.002f,.08f),Sage);
        CampusModel(g,new Vector3(.55f,.756f,.23f));
        SharedLaptop(g,new Vector3(.56f,.756f,-.47f));
        var tools=F.Group(g,"Shared_pens_and_material_cards",new Vector3(-1.01f,.753f,.50f));
        F.Primitive(tools,"Shared_pencil_cup",PrimitiveType.Cylinder,new Vector3(0,.052f,0),new Vector3(.08f,.052f,.08f),Cream);
        foreach(float x in new[]{-.02f,0,.02f})
            B(tools,"Shared_marker",new Vector3(x,.125f,0),new Vector3(.009f,.16f,.009f),x<0?Blue:x>0?Clay:Graphite);
        foreach(float x in new[]{-.095f,.015f,.125f})
            R(g,"Small_material_sample",new Vector3(x-1.0f,.756f,-.52f),new Vector3(.085f,.009f,.105f),x<0?Card:x>0?Sage:Cream,.006f);
        Solid(g,new Vector3(0,.375f,0),new Vector3(2.40f,.75f,1.50f));
    }

    static void ProjectChair(Transform parent,string name,Vector3 pos,float yaw,Material fabric)
    {
        var g=F.Group(parent,name,pos,yaw);
        R(g,"Padded_seat__H0.47m",new Vector3(0,.43f,.015f),new Vector3(.51f,.08f,.49f),fabric,.075f);
        R(g,"Thin_oak_seat_shell",new Vector3(0,.382f,.015f),new Vector3(.50f,.025f,.48f),Oak,.065f);
        R(g,"Padded_mid_back",new Vector3(0,.745f,-.222f),new Vector3(.50f,.075f,.39f),fabric,.065f).transform.localRotation=Quaternion.Euler(83,0,0);
        R(g,"Oak_back_shell",new Vector3(0,.744f,-.268f),new Vector3(.50f,.025f,.39f),Oak,.065f).transform.localRotation=Quaternion.Euler(83,0,0);
        foreach(float x in new[]{-.20f,.20f}) {
            B(g,"Backrest_support",new Vector3(x,.51f,-.235f),new Vector3(.028f,.26f,.03f),Oak);
            foreach(float z in new[]{-.18f,.18f}) {
                var leg=R(g,"Slender_splayed_oak_leg",new Vector3(x,.19f,z),new Vector3(.035f,.38f,.035f),Oak,.007f);
                leg.transform.localRotation=Quaternion.Euler(z>0?4:-4,0,x>0?-4:4);
                B(g,"Quiet_floor_glide",new Vector3(x+(x>0?.013f:-.013f),.008f,z+(z>0?.013f:-.013f)),new Vector3(.04f,.016f,.04f),Graphite);
            }
        }
        Solid(g,new Vector3(0,.4775f,-.02f),new Vector3(.54f,.955f,.58f));
    }

    static void CollaborationWhiteboard(Transform parent)
    {
        var g=F.Group(parent,"N04_Collaboration_whiteboard",new Vector3(10.54f,0,8.35f),270);
        B(g,"Cream_wall_board_frame",new Vector3(0,1.75f,0),new Vector3(2.76f,1.46f,.035f),Cream);
        B(g,"Writable_collaboration_surface",new Vector3(0,1.75f,-.026f),new Vector3(2.70f,1.40f,.015f),White);
        F.WallText(g,"HOW CAN OUR CAMPUS WORK BETTER?",new Vector3(0,2.235f,-.038f),0,2.22f,new Color(.045f,.065f,.055f));
        foreach(float x in new[]{-.86f,0,.86f}) {
            B(g,"Process_box_top",new Vector3(x,1.94f,-.039f),new Vector3(.56f,.012f,.003f),Blue);
            B(g,"Process_box_bottom",new Vector3(x,1.60f,-.039f),new Vector3(.56f,.012f,.003f),Blue);
            foreach(float edge in new[]{-.28f,.28f})
                B(g,"Process_box_side",new Vector3(x+edge,1.77f,-.039f),new Vector3(.012f,.34f,.003f),Blue);
            F.WallText(g,x<0?"OBSERVE":x>0?"PROTOTYPE":"IDEATE",new Vector3(x,1.77f,-.043f),0,.43f,new Color(.035f,.055f,.055f));
        }
        foreach(float x in new[]{-.43f,.43f}) {
            B(g,"Workflow_arrow_shaft",new Vector3(x,1.77f,-.040f),new Vector3(.22f,.012f,.003f),Graphite);
            foreach(float angle in new[]{-35f,35f}) {
                var arrow=B(g,"Workflow_arrow_head",new Vector3(x+.08f,1.77f+(angle>0?-.022f:.022f),-.040f),new Vector3(.075f,.010f,.003f),Graphite);
                arrow.transform.localRotation=Quaternion.Euler(0,0,angle);
            }
        }
        foreach(float x in new[]{-.85f,-.50f,.35f,.70f})
            B(g,"Team_sticky_note",new Vector3(x,1.30f,-.042f),new Vector3(.23f,.20f,.006f),x<0?Paper:Sage);
        R(g,"Wide_marker_tray",new Vector3(0,1.01f,-.105f),new Vector3(1.22f,.03f,.14f),Cream,.012f);
        foreach(float x in new[]{-.28f,0,.28f})
            B(g,"Board_marker",new Vector3(x,1.033f,-.105f),new Vector3(.13f,.018f,.018f),x<0?Blue:x>0?Clay:Graphite);
        Solid(g,new Vector3(0,1.735f,-.06f),new Vector3(2.76f,1.49f,.20f));
    }

    static void TeamMaterialStorage(Transform parent)
    {
        var g=F.Group(parent,"N04_Team_material_storage",new Vector3(15.55f,0,6.35f),180);
        B(g,"Inset_cream_plinth",new Vector3(0,.04f,.015f),new Vector3(2.28f,.08f,.36f),Cream);
        B(g,"Oak_storage_back",new Vector3(0,.565f,.205f),new Vector3(2.40f,.97f,.04f),Oak);
        foreach(float x in new[]{-1.18f,1.18f})
            B(g,"Oak_outer_side",new Vector3(x,.565f,0),new Vector3(.04f,.97f,.45f),Oak);
        foreach(float y in new[]{.10f,.415f,.725f,1.03f})
            B(g,"Oak_storage_shelf",new Vector3(0,y,0),new Vector3(2.40f,.04f,.45f),Oak);
        foreach(float x in new[]{-.40f,.40f})
            B(g,"Cubby_column_divider",new Vector3(x,.725f,0),new Vector3(.025f,.59f,.45f),Oak);
        int cubby=0;
        foreach(float y in new[]{.57f,.8775f})
            foreach(float x in new[]{-.80f,0,.80f}) {
                cubby++;
                var cell=F.Group(g,"N04_Member_cubby_"+cubby.ToString("00"),new Vector3(x,y,0));
                B(cell,"Member_label_holder",new Vector3(0,-.126f,-.232f),new Vector3(.14f,.035f,.009f),Cream);
                F.WallText(cell,"0"+cubby,new Vector3(0,-.126f,-.239f),0,.07f,new Color(.035f,.050f,.045f));
                if(cubby%2==0) {
                    R(cell,"Personal_material_box",new Vector3(.06f,-.027f,-.015f),new Vector3(.34f,.19f,.28f),cubby==2?Sage:cubby==4?Blue:Oatmeal,.012f);
                    B(cell,"Box_label",new Vector3(.06f,-.015f,-.160f),new Vector3(.13f,.055f,.005f),Paper);
                }
            }
        foreach(float x in new[]{-.80f,0,.80f}) {
            R(g,"Closed_lower_material_drawer",new Vector3(x,.255f,-.218f),new Vector3(.77f,.265f,.025f),Cream,.01f);
            B(g,"Material_drawer_pull",new Vector3(x,.29f,-.236f),new Vector3(.16f,.014f,.018f),Graphite);
        }
        Solid(g,new Vector3(0,.525f,0),new Vector3(2.40f,1.05f,.49f));
    }

    static void ProgressBoard(Transform parent)
    {
        var g=F.Group(parent,"N04_Project_progress_board",new Vector3(15.55f,1.95f,6.07f),180);
        B(g,"Oak_progress_frame",Vector3.zero,new Vector3(2.56f,1.31f,.04f),Oak);
        B(g,"Cork_work_in_progress_surface",new Vector3(0,0,-.025f),new Vector3(2.50f,1.25f,.015f),Cork);
        int column=0;
        foreach(float x in new[]{-.82f,0,.82f}) {
            F.WallText(g,column==0?"TO DO":column==1?"MAKING":"REVIEW",new Vector3(x,.43f,-.039f),0,.56f,new Color(.97f,.94f,.86f));
            if(column<2) B(g,"Workflow_column_divider",new Vector3(x+.41f,-.03f,-.037f),new Vector3(.012f,1.03f,.003f),Cream);
            for(int row=0;row<2;row++) {
                var note=F.Group(g,"Pinned_project_task",new Vector3(x+(row==0?-.06f:.07f),.14f-row*.39f,-.041f));
                note.localRotation=Quaternion.Euler(0,0,row==0?3:-3);
                B(note,"Task_card",Vector3.zero,new Vector3(.56f,.29f,.005f),column==1?Sage:Paper);
                foreach(float y in new[]{.07f,0,-.07f})
                    B(note,"Task_sketch_line",new Vector3(-.015f,y,-.004f),new Vector3(.36f,.010f,.002f),Graphite);
                F.Primitive(note,"Drawing_pin",PrimitiveType.Sphere,new Vector3(0,.125f,-.010f),new Vector3(.025f,.025f,.012f),Clay);
            }
            column++;
        }
        Solid(g,Vector3.zero,new Vector3(2.56f,1.31f,.085f));
    }

    static void WindowWorkbench(Transform parent)
    {
        var g=F.Group(parent,"N04_Window_project_workbench",new Vector3(17.25f,0,10.10f),90);
        R(g,"Oak_standing_worktop__3.40x0.60m__H0.86m",new Vector3(0,.8325f,0),new Vector3(3.40f,.055f,.60f),Oak,.035f);
        foreach(float x in new[]{-1.22f,1.22f}) {
            var bay=F.Group(g,"Workbench_end_material_drawers",new Vector3(x,0,0));
            B(bay,"Inset_plinth",new Vector3(0,.045f,.015f),new Vector3(.77f,.09f,.43f),Graphite);
            B(bay,"Cream_drawer_carcase",new Vector3(0,.445f,.025f),new Vector3(.84f,.71f,.51f),Cream);
            foreach(float y in new[]{.24f,.59f}) {
                R(bay,"Wide_material_tray_drawer",new Vector3(0,y,-.246f),new Vector3(.80f,.32f,.027f),Cream,.01f);
                B(bay,"Drawer_pull",new Vector3(0,y+.07f,-.269f),new Vector3(.20f,.014f,.018f),Graphite);
            }
        }
        B(g,"Recessed_rear_stiffening_rail",new Vector3(0,.72f,.25f),new Vector3(3.25f,.10f,.045f),Oak);
        var drawing=F.Group(g,"Workbench_sketch_and_cutting_mat",new Vector3(-.18f,.863f,-.015f));
        R(drawing,"Muted_sage_cutting_mat",Vector3.zero,new Vector3(.73f,.005f,.46f),Sage,.01f);
        B(drawing,"Working_cardboard_sheet",new Vector3(-.10f,.006f,-.02f),new Vector3(.36f,.006f,.28f),Card);
        foreach(float x in new[]{-.20f,-.08f,.04f})
            B(drawing,"Sketch_cutting_guide",new Vector3(x,.010f,-.02f),new Vector3(.003f,.002f,.22f),Graphite);
        WorkInProgressModel(g,new Vector3(.67f,.864f,0));
        R(g,"Open_material_tray",new Vector3(-1.10f,.889f,0),new Vector3(.52f,.05f,.36f),Cream,.02f);
        foreach(float x in new[]{-1.25f,-1.10f,-.95f})
            B(g,"Foam_card_sample_stack",new Vector3(x,.928f,0),new Vector3(.115f,.03f,.25f),x<-1.15f?Card:x>-1?Sage:Paper);
        B(g,"Shared_wooden_ruler",new Vector3(-.30f,.872f,.21f),new Vector3(.42f,.008f,.025f),Oak);
        Solid(g,new Vector3(0,.43f,0),new Vector3(3.40f,.86f,.60f));
    }

    static void IdentityBoard(Transform parent)
    {
        var g=F.Group(parent,"N04_Team_identity_board",new Vector3(11.65f,1.80f,6.07f),180);
        B(g,"Oak_team_board_frame",Vector3.zero,new Vector3(1.86f,.96f,.04f),Oak);
        B(g,"Cream_team_project_board",new Vector3(0,0,-.025f),new Vector3(1.80f,.90f,.015f),Cream);
        F.WallText(g,"CAMPUS COMMONS / TEAM 04",new Vector3(0,.28f,-.039f),0,1.50f,new Color(.055f,.075f,.060f));
        int member=0;
        foreach(float y in new[]{.025f,-.23f})
            foreach(float x in new[]{-.56f,0,.56f}) {
                member++;
                R(g,"Team_member_card_"+member,new Vector3(x,y,-.039f),new Vector3(.46f,.19f,.008f),Paper,.008f);
                F.Primitive(g,"Member_avatar",PrimitiveType.Sphere,new Vector3(x-.145f,y,-.048f),new Vector3(.075f,.075f,.003f),member%2==0?Sage:Blue);
                F.WallText(g,"MEMBER 0"+member,new Vector3(x+.04f,y,-.049f),0,.24f,new Color(.035f,.050f,.045f));
            }
        Solid(g,Vector3.zero,new Vector3(1.86f,.96f,.085f));
    }

    static void MaterialCart(Transform parent)
    {
        var g=F.Group(parent,"N04_Team_material_cart",new Vector3(11.65f,0,6.35f),180);
        foreach(float x in new[]{-.66f,.66f})
            foreach(float z in new[]{-.145f,.145f}) {
                B(g,"Small_caster_fork",new Vector3(x,.065f,z),new Vector3(.04f,.065f,.04f),Graphite);
                var wheel=F.Primitive(g,"Quiet_cart_wheel",PrimitiveType.Cylinder,new Vector3(x,.035f,z),new Vector3(.07f,.018f,.07f),Graphite);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
            }
        B(g,"Cream_cart_body",new Vector3(0,.37f,.015f),new Vector3(1.50f,.57f,.39f),Cream);
        R(g,"Oak_cart_worktop",new Vector3(0,.695f,0),new Vector3(1.50f,.05f,.42f),Oak,.025f);
        foreach(float x in new[]{-.37f,.37f}) {
            R(g,"Closed_shared_supply_door",new Vector3(x,.38f,-.195f),new Vector3(.72f,.53f,.025f),Cream,.01f);
            B(g,"Supply_door_pull",new Vector3(x,.53f,-.220f),new Vector3(.14f,.014f,.018f),Graphite);
        }
        R(g,"Group_material_folder",new Vector3(-.42f,.729f,0),new Vector3(.35f,.018f,.29f),Sage,.008f);
        B(g,"Material_folder_label",new Vector3(-.42f,.741f,0),new Vector3(.18f,.003f,.06f),Paper);
        Solid(g,new Vector3(0,.36f,0),new Vector3(1.50f,.72f,.46f));
    }

    static void CampusModel(Transform parent,Vector3 pos)
    {
        var g=F.Group(parent,"Shared_campus_project_model",pos);
        R(g,"Small_campus_site_model_base",new Vector3(0,.009f,0),new Vector3(.68f,.018f,.50f),Card,.012f);
        B(g,"Campus_path",new Vector3(0,.021f,-.12f),new Vector3(.56f,.006f,.045f),Paper);
        int n=0;
        foreach(Vector3 p in new[]{new Vector3(-.19f,.075f,.05f),new Vector3(.02f,.10f,.065f),new Vector3(.23f,.06f,.065f)}) {
            n++;
            float height=n==2?.15f:n==1?.10f:.07f;
            B(g,"Campus_building_volume",new Vector3(p.x,.025f+height/2,p.z),new Vector3(.14f,height,.17f),Paper);
            B(g,"Thin_card_roof",new Vector3(p.x,.03f+height,p.z),new Vector3(.16f,.01f,.19f),Oak);
            foreach(float x in new[]{-.035f,.035f})
                B(g,"Miniature_window",new Vector3(p.x+x,.04f+height*.5f,p.z-.087f),new Vector3(.035f,.028f,.003f),Blue);
        }
        foreach(float x in new[]{-.23f,0,.23f})
            F.Primitive(g,"Model_landscape_tree",PrimitiveType.Sphere,new Vector3(x,.052f,-.19f),new Vector3(.065f,.07f,.065f),Sage);
    }

    static void SharedLaptop(Transform parent,Vector3 pos)
    {
        var g=F.Group(parent,"One_shared_project_laptop",pos);
        R(g,"Laptop_base",new Vector3(0,.012f,0),new Vector3(.38f,.024f,.26f),Graphite,.012f);
        B(g,"Keyboard",new Vector3(0,.026f,.01f),new Vector3(.30f,.004f,.11f),Cream);
        R(g,"Trackpad",new Vector3(0,.027f,-.082f),new Vector3(.095f,.004f,.045f),Cream,.006f);
        var lid=F.Group(g,"Open_laptop_lid",new Vector3(0,.139f,.115f));
        lid.localRotation=Quaternion.Euler(-12,0,0);
        R(lid,"Thin_laptop_screen_bezel",Vector3.zero,new Vector3(.38f,.24f,.018f),Graphite,.01f);
        B(lid,"Shared_project_canvas",new Vector3(0,0,-.011f),new Vector3(.343f,.202f,.004f),Paper);
        B(lid,"Shared_model_on_screen",new Vector3(0,-.015f,-.014f),new Vector3(.22f,.105f,.002f),Sage);
    }

    static void WorkInProgressModel(Transform parent,Vector3 pos)
    {
        var g=F.Group(parent,"Cardboard_model_under_construction",pos);
        B(g,"Working_foamboard_base",new Vector3(0,.0075f,0),new Vector3(.58f,.015f,.40f),Paper);
        B(g,"Partially_built_long_card_wall",new Vector3(0,.13f,.12f),new Vector3(.40f,.23f,.012f),Card);
        B(g,"Partially_built_end_card_wall",new Vector3(-.195f,.13f,0),new Vector3(.012f,.23f,.25f),Card);
        B(g,"Cardboard_partition",new Vector3(.06f,.09f,.02f),new Vector3(.012f,.15f,.21f),Card);
        foreach(float x in new[]{-.14f,0,.14f})
            B(g,"Open_roof_structure_strip",new Vector3(x,.255f,.005f),new Vector3(.018f,.018f,.30f),Oak);
        B(g,"Loose_card_roof_panel",new Vector3(.18f,.028f,-.09f),new Vector3(.15f,.012f,.16f),Card);
    }
}
