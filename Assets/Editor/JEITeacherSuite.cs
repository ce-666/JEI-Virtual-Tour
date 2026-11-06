using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// S01: eight screened workstations inside full-height glazing, with a shared,
/// open discussion and lounge area outside. Dimensions and positions are metres.
/// This builder only adds furniture; the structural shell remains untouched.
/// </summary>
public static class JEITeacherSuite
{
    const float GlassLine = -9.0f;
    const float GlassHeight = 3.3f;

    public static void Build(Transform parent)
    {
        var suite = JEIFurnitureKit.Group(parent, "S01__Teacher_suite__8_quiet_workstations__open_discussion_and_lounge");
        BuildOfficeGlazing(suite);
        BuildQuietWorkstations(suite);
        BuildOpenDiscussion(suite);
        BuildOpenLounge(suite);
    }

    static void BuildQuietWorkstations(Transform parent)
    {
        var office = JEIFurnitureKit.Group(parent, "01__Quiet_office__8_individually_screened_workstations");
        var fabric = JEIFurnitureKit.Mat("S01_Partition_Fabric", new Color(.29f, .45f, .46f), .08f);
        var edge = JEIFurnitureKit.Mat("S01_Partition_Edge", new Color(.16f, .22f, .24f), .35f);
        float[] islandX = { -15.9f, -11.75f };
        int number = 1;

        for (int island = 0; island < islandX.Length; island++)
        {
            var cluster = JEIFurnitureKit.Group(office, "Four_person_workstation_island_" + (island + 1));
            float centreX = islandX[island];
            for (int row = 0; row < 2; row++)
            {
                bool north = row == 0;
                float deskZ = north ? -9.45f : -10.15f;
                float chairZ = north ? -8.4f : -11.2f;
                float yaw = north ? 180 : 0;
                for (int column = 0; column < 2; column++)
                {
                    float deskX = centreX + (column == 0 ? -.7f : .7f);
                    var station = JEIFurnitureKit.Group(cluster, "Teacher_workstation_" + number++);
                    JEIFurnitureKit.Desk(station, new Vector3(deskX, 0, deskZ), yaw, 1.4f, .7f, true);
                    JEIFurnitureKit.Chair(station, new Vector3(deskX, 0, chairZ), yaw, true, "Navy");
                }
            }

            // A cross of 1.25 m high screens gives every desk its own bay while
            // retaining daylight and views through the glazed enclosure.
            JEIFurnitureKit.Box(cluster, "Shared_fabric_privacy_screen__H1.25m",
                new Vector3(centreX, .99f, -9.8f), new Vector3(2.8f, .52f, .05f), fabric);
            JEIFurnitureKit.Box(cluster, "Individual_bay_dividers__H1.25m",
                new Vector3(centreX, .99f, -9.8f), new Vector3(.05f, .52f, 1.4f), fabric);
            JEIFurnitureKit.Box(cluster, "Horizontal_screen_top_edge",
                new Vector3(centreX, 1.255f, -9.8f), new Vector3(2.83f, .025f, .055f), edge, false);
            JEIFurnitureKit.Box(cluster, "Cross_screen_top_edge",
                new Vector3(centreX, 1.255f, -9.8f), new Vector3(.055f, .025f, 1.4f), edge, false);
        }

        // Storage stays on the solid north wall, leaving a 1.6 m clear band
        // behind the north-row task chairs and a continuous route to the door.
        JEIFurnitureKit.Cabinet(office, new Vector3(-15.8f, 0, -6.30f), 1.5f, 1.15f);
        JEIFurnitureKit.Cabinet(office, new Vector3(-13.9f, 0, -6.30f), 1.5f, 1.15f);
    }

    static void BuildOpenDiscussion(Transform parent)
    {
        var discussion = JEIFurnitureKit.Group(parent, "02__Open_discussion__4_seats__no_enclosing_partition");
        JEIFurnitureKit.Table(discussion, new Vector3(-5.4f, 0, -7.65f), 1.9f, .9f);
        foreach (float x in new[] { -5.95f, -4.85f })
        {
            JEIFurnitureKit.Chair(discussion, new Vector3(x, 0, -6.65f), 180, false, "Sand");
            JEIFurnitureKit.Chair(discussion, new Vector3(x, 0, -8.65f), 0, false, "Teal");
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
