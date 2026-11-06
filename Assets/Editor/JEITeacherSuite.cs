using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// S01: six screened workstations and a shared refreshment/printing cabinet
/// inside full-height glazing, with an open discussion and lounge area outside.
/// Dimensions and positions are metres.
/// This builder only adds furniture; the structural shell remains untouched.
/// </summary>
public static class JEITeacherSuite
{
    const float GlassLine = -9.0f;
    const float GlassHeight = 3.3f;

    public static void Build(Transform parent)
    {
        var suite = JEIFurnitureKit.Group(parent, "S01__Teacher_suite__6_quiet_workstations__refreshment_and_printing__open_discussion_and_lounge");
        BuildOfficeGlazing(suite);
        BuildQuietWorkstations(suite);
        BuildOpenDiscussion(suite);
        BuildOpenLounge(suite);
    }

    static void BuildQuietWorkstations(Transform parent)
    {
        var office = JEIFurnitureKit.Group(parent, "01__Quiet_office__6_warm_wood_writing_desks_along_south_and_west_windows");
        var fabric = JEIFurnitureKit.Mat("Faculty_Privacy_Sage", new Color(.48f, .59f, .51f), .08f);
        var edge = JEIFurnitureKit.Mat("Faculty_Privacy_Oak_Edge", new Color(.64f, .48f, .30f), .24f);
        int number = 1;

        // Four generous writing desks face the south windows. Chairs are tucked
        // close enough to use the desk while keeping the central floor open.
        float[] southX = { -15.9f, -13.95f, -12f, -10.05f };
        for (int i = 0; i < southX.Length; i++)
        {
            WritingStation(office, number++, new Vector3(southX[i], 0, -12.15f), 180, 1.8f, .8f,
                new Vector3(southX[i], 0, -11.25f), i % 2 == 0 ? "Sage" : "Oatmeal");
            if (i < southX.Length - 1)
                SideScreen(office, "South_window_desk_side_screen__H1.12m",
                    new Vector3((southX[i] + southX[i + 1]) / 2, .93f, -12.15f),
                    new Vector3(.04f, .34f, .8f), fabric, edge);
        }

        // Two west-window desks stay clear of the south row, with 1.03 m
        // between the neighbouring task-chair envelopes at the southwest turn.
        float[] westZ = { -7.9f, -9.6f };
        for (int i = 0; i < westZ.Length; i++)
            WritingStation(office, number++, new Vector3(-17.2f, 0, westZ[i]), -90, 1.6f, .8f,
                new Vector3(-16.25f, 0, westZ[i]), i == 0 ? "Oatmeal" : "Sage");
        SideScreen(office, "West_window_desk_side_screen__H1.12m",
            new Vector3(-17.2f, .93f, -8.75f), new Vector3(.8f, .34f, .04f), fabric, edge);

        // Full-height document storage occupies only the solid north wall.
        // Together these measure 3.20 W x 2.10 H x 0.50 D metres; neither west
        // nor south daylight openings receive tall furniture in front of them.
        JEIFacultyFurniture.StorageCabinet(office, new Vector3(-16.2f, 0, -6.30f), 0, 1.6f, 2.1f, .5f);
        JEIFacultyFurniture.StorageCabinet(office, new Vector3(-14.6f, 0, -6.30f), 0, 1.6f, 2.1f, .5f);

        // The former north-wall pair of desks becomes one continuous cabinet:
        // refreshment facilities on the left, an open worktop, printing on the right.
        JEIFacultyAmenityCabinet.Build(office, new Vector3(-12.025f, 0, -6.36f), 3.55f, .62f);
    }

    static void WritingStation(Transform parent, int number, Vector3 deskPosition, float yaw,
        float width, float depth, Vector3 chairPosition, string color)
    {
        var station = JEIFurnitureKit.Group(parent, "Teacher_workstation_" + number);
        JEIFacultyFurniture.WritingDesk(station, deskPosition, yaw, width, depth);
        JEIFacultyFurniture.StudyChair(station, chairPosition, yaw, true, color);
    }

    static void SideScreen(Transform parent, string name, Vector3 position, Vector3 size,
        Material fabric, Material edge)
    {
        JEIFurnitureKit.Box(parent, name, position, size, fabric);
        Vector3 railSize = size;
        railSize.y = .02f;
        railSize.x += .015f;
        railSize.z += .015f;
        position.y = 1.11f;
        JEIFurnitureKit.Box(parent, "Warm_oak_screen_top_edge", position, railSize, edge, false);
    }

    static void BuildOpenDiscussion(Transform parent)
    {
        var discussion = JEIFurnitureKit.Group(parent, "02__Open_discussion__4_seats__no_enclosing_partition");
        JEIFacultyFurniture.SocialTable(discussion, new Vector3(-5.4f, 0, -7.65f), 0, 1.9f, 1.05f);
        foreach (float x in new[] { -5.95f, -4.85f })
        {
            JEIFacultyFurniture.StudyChair(discussion, new Vector3(x, 0, -6.65f), 180, false, "Oatmeal");
            JEIFacultyFurniture.StudyChair(discussion, new Vector3(x, 0, -8.65f), 0, false, "Sage");
        }

        var frame = JEIFurnitureKit.Mat("S01_Whiteboard_Frame", new Color(.48f, .53f, .54f), .4f);
        var white = JEIFurnitureKit.Mat("S01_Whiteboard", new Color(.92f, .94f, .90f), .5f);
        JEIFurnitureKit.Box(discussion, "Wall_mounted_discussion_whiteboard_frame",
            new Vector3(-3.465f, 1.6f, -7.65f), new Vector3(.055f, .94f, 1.55f), frame, false);
        JEIFurnitureKit.Box(discussion, "Whiteboard_writing_surface",
            new Vector3(-3.501f, 1.6f, -7.65f), new Vector3(.02f, .86f, 1.47f), white, false);
    }

    static void BuildOpenLounge(Transform parent)
    {
        var lounge = JEIFurnitureKit.Group(parent, "03__Open_exchange_lounge__two_facing_sofas__shared_with_discussion");
        JEIFurnitureKit.Sofa(lounge, new Vector3(-8.2f, 0, -11.55f), -90, 1.65f, "Teal");
        JEIFurnitureKit.Sofa(lounge, new Vector3(-3.95f, 0, -11.55f), 90, 1.65f, "Teal");
        var coffee = JEIFurnitureKit.Table(lounge, new Vector3(-6.05f, 0, -11.55f), 1.1f, .65f);
        coffee.name = "Low_oak_coffee_table__H0.46m";
        coffee.localScale = new Vector3(1, .6f, 1);

        // On the east solid wall; no tall storage is placed in front of the
        // south windows. Both open social functions share the same floor area.
        JEIFurnitureKit.Cabinet(lounge, new Vector3(-3.65f, 0, -9.65f), 1.2f, 1.8f, 90, true);
    }

    static void BuildOfficeGlazing(Transform parent)
    {
        var glazing = JEIFurnitureKit.Group(parent, "04__Full_height_quiet_office_glazing__H3.30m__clear_door1.20m");
        var glass = JEIFurnitureKit.MatGlass("S01_Clear_Office_Glass", .08f);
        var frame = JEIFurnitureKit.Mat("S01_Glazing_Graphite", new Color(.13f, .19f, .21f), .5f);

        // Clear door opening Z = [-8.30, -7.10]. Jamb centres are outside that
        // interval, so their 60 mm thickness does not reduce its 1.20 m width.
        BuildGlassRun(glazing, -12.67f, -8.33f, 4, glass, frame);
        BuildGlassRun(glazing, -7.07f, -6.03f, 1, glass, frame);
        JEIFurnitureKit.Box(glazing, "Continuous_ceiling_level_head_rail",
            new Vector3(GlassLine, 3.27f, -9.35f), new Vector3(.065f, .06f, 6.7f), frame);

        const float openingBottom = 2.3f;
        GlassBox(glazing, "Fixed_glass_transom_above_door",
            new Vector3(GlassLine, (openingBottom + 3.24f) / 2, -7.7f),
            new Vector3(.032f, 3.24f - openingBottom, 1.2f), glass, true);
        JEIFurnitureKit.Box(glazing, "Door_head__clear_height2.30m",
            new Vector3(GlassLine, 2.33f, -7.7f), new Vector3(.065f, .06f, 1.2f), frame);

        // Closed in the saved scene to express the quiet-office separation.
        // The complete framed leaf pivots inward when the tour player approaches;
        // no sill or trigger collider reduces the fixed 1.20 m clear opening.
        var door = JEIFurnitureKit.Group(glazing, "Glass_door_hinge__closed_by_default__automatic_inward_opening",
            new Vector3(GlassLine, 0, -8.3f));
        GlassBox(door, "Operable_glass_door_leaf__physical_collider", new Vector3(0, 1.13f, .58f),
            new Vector3(.032f, 2.2f, 1.16f), glass, true);
        foreach (float z in new[] { .015f, 1.145f })
            JEIFurnitureKit.Box(door, "Door_leaf_vertical_stile", new Vector3(0, 1.13f, z),
                new Vector3(.045f, 2.23f, .035f), frame, false);
        foreach (float y in new[] { .03f, 2.23f })
            JEIFurnitureKit.Box(door, "Door_leaf_horizontal_edge", new Vector3(0, y, .58f),
                new Vector3(.045f, .035f, 1.16f), frame, false);
        JEIFurnitureKit.Box(door, "Glass_door_pull_handle", new Vector3(.065f, 1.06f, 1.02f),
            new Vector3(.025f, .3f, .025f), frame, false);
        foreach (float y in new[] { .94f, 1.18f })
            JEIFurnitureKit.Box(door, "Door_handle_standoff", new Vector3(.035f, y, 1.02f),
                new Vector3(.065f, .025f, .025f), frame, false);
        door.gameObject.AddComponent<JEIGlassOfficeDoor>();
    }

    static void BuildGlassRun(Transform parent, float startZ, float endZ, int panes,
        Material glass, Material frame)
    {
        float bay = (endZ - startZ) / panes;
        for (int i = 0; i < panes; i++)
        {
            float z = startZ + (i + .5f) * bay;
            GlassBox(parent, "Fixed_full_height_glass_panel", new Vector3(GlassLine, 1.65f, z),
                new Vector3(.032f, 3.18f, bay - .06f), glass, true);
        }
        for (int i = 0; i <= panes; i++)
            JEIFurnitureKit.Box(parent, "Slender_full_height_glazing_mullion",
                new Vector3(GlassLine, GlassHeight / 2, startZ + i * bay),
                new Vector3(.065f, GlassHeight, .06f), frame);
        JEIFurnitureKit.Box(parent, "Glazing_bottom_rail__stops_at_door_jamb",
            new Vector3(GlassLine, .045f, (startZ + endZ) / 2),
            new Vector3(.065f, .09f, endZ - startZ), frame);
    }

    static void GlassBox(Transform parent, string name, Vector3 position, Vector3 size,
        Material material, bool solid)
    {
        var pane = JEIFurnitureKit.Box(parent, name, position, size, material, solid);
        var renderer = pane.GetComponent<Renderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }
}
