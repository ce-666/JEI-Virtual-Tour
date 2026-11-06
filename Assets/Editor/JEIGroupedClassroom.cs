using UnityEngine;

/// <summary>
/// The agreed collaborative classroom in N02. Coordinates are in the existing
/// first-floor structure frame; the south door and north window stay clear.
/// </summary>
public static class JEIGroupedClassroom
{
    public static void Build(Transform parent)
    {
        Transform room = JEIFurnitureKit.Group(parent, "N02__Arc_Table_Classroom__16_Seats");
        Material ink = JEIFurnitureKit.Mat("Classroom_Graphite", new Color(.075f, .105f, .12f));
        Material ivory = JEIFurnitureKit.Mat("Classroom_Warm_White", new Color(.94f, .935f, .89f));
        Material teal = JEIFurnitureKit.Mat("Classroom_Teal", new Color(.08f, .30f, .33f));
        Material blue = JEIFurnitureKit.Mat("Classroom_Display", new Color(.16f, .31f, .39f), .38f);
        Material mustard = JEIFurnitureKit.Mat("Classroom_Mustard", new Color(.85f, .63f, .23f));

        // Four groups give sixteen seats without crowding the central route.
        // Every tabletop is a curved solid mesh, rather than a painted circle.
        float[] columns = { -7.05f, -2.95f };
        float[] rows = { 7.65f, 10.95f };
        int groupNumber = 0;
        foreach (float z in rows)
        {
            foreach (float x in columns)
            {
                groupNumber++;
                Transform pod = JEIFurnitureKit.Group(room,
                    "Group_" + groupNumber + "__Four_Curved_Tables", new Vector3(x, 0, z));
                for (int quadrant = 0; quadrant < 4; quadrant++)
                {
                    // A narrow joint makes the individual movable modules visible.
                    JEIFurnitureKit.ArcTable(pod, Vector3.zero,
                        quadrant * 90f + 1.5f, (quadrant + 1) * 90f - 1.5f,
                        .85f, .30f, .75f);

                    float angle = (quadrant * 90f + 45f) * Mathf.Deg2Rad;
                    Vector3 chairPosition = new Vector3(Mathf.Cos(angle) * 1.25f,
                        0, Mathf.Sin(angle) * 1.25f);
                    float chairYaw = 270f - (quadrant * 90f + 45f);
                    JEIFurnitureKit.Chair(pod, chairPosition, chairYaw, false,
                        quadrant % 2 == 0 ? "Teal" : "Mustard");
                }
            }
        }

        // Presentations use an internal west wall, preserving the external window.
        Transform display = JEIFurnitureKit.Group(room, "West_Wall_Shared_Display",
            new Vector3(-9.12f, 0, 9.30f), -90f);
        JEIFurnitureKit.Box(display, "Display_Frame", new Vector3(0, 1.82f, 0),
            new Vector3(2.35f, 1.34f, .09f), ink);
        JEIFurnitureKit.Box(display, "Display_Glass", new Vector3(0, 1.82f, -.052f),
            new Vector3(2.22f, 1.21f, .018f), blue, false);
        JEIFurnitureKit.WallText(display, "JEI  /  COLLABORATE",
            new Vector3(0, 2.10f, -.066f), 0, 1.95f, Color.white);
        JEIFurnitureKit.WallText(display, "DISCUSS  •  CREATE  •  SHARE",
            new Vector3(0, 1.59f, -.067f), 0, 1.86f,
            new Color(.87f, .91f, .88f));
        JEIFurnitureKit.Box(display, "Screen_Status_Light", new Vector3(1.03f, 1.18f, -.052f),
            new Vector3(.025f, .018f, .012f), mustard, false);

        // A shallow low cupboard and wall whiteboard occupy only the east wall's
        // gap between rows. No raised teaching platform or extra student desks.
        JEIFurnitureKit.Cabinet(room, new Vector3(-1.12f, 0, 9.30f),
            1.40f, .72f, 90f, false);
        Transform board = JEIFurnitureKit.Group(room, "East_Wall_Ideas_Board",
            new Vector3(-.99f, 0, 9.30f), 90f);
        JEIFurnitureKit.Box(board, "Whiteboard_Frame", new Vector3(0, 1.64f, 0),
            new Vector3(1.98f, 1.10f, .055f), ink, false);
        JEIFurnitureKit.Box(board, "Whiteboard_Surface", new Vector3(0, 1.64f, -.035f),
            new Vector3(1.88f, 1.00f, .018f), ivory, false);
        JEIFurnitureKit.WallText(board, "IDEAS TOGETHER", new Vector3(0, 1.91f, -.048f),
            0, 1.55f, new Color(.08f, .30f, .33f));
        JEIFurnitureKit.Box(board, "Board_Marker_Tray", new Vector3(0, 1.07f, -.08f),
            new Vector3(1.90f, .035f, .16f), ink, false);
        for (int index = 0; index < 3; index++)
        {
            JEIFurnitureKit.Box(board, "Discussion_Note_" + index,
                new Vector3(-.51f + index * .51f, 1.50f, -.051f),
                new Vector3(.31f, .25f, .012f), index == 1 ? teal : mustard, false);
        }
    }
}
