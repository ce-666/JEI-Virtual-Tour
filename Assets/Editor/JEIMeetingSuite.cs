using UnityEngine;
using F = JEIFurnitureKit;

/// <summary>Metre-scaled eight-person S04 meeting room with a softer window conversation corner.</summary>
public static class JEIMeetingSuite
{
    public static readonly Vector3 TableCentre = new Vector3(13.1f, 0, -9.25f);
    public const float TableWidth = 3.20f;
    public const float TableDepth = 1.35f;
    public static readonly Vector3 MeetingSetOffset = new Vector3(.10f, 0, 0);
    public static readonly Vector3 LoungeTableCentre = new Vector3(16.25f, 0, -10.85f);

    static Material Oak => F.Mat("S04_Honey_Oak", new Color(.72f, .54f, .34f), .23f);
    static Material Cream => F.Mat("S04_Warm_Cream", new Color(.87f, .85f, .76f), .22f);
    static Material Sage => F.Mat("S04_Sage_Linen", new Color(.45f, .56f, .46f), .06f);
    static Material Oatmeal => F.Mat("S04_Oatmeal_Linen", new Color(.72f, .67f, .56f), .06f);
    static Material Hardware => F.Mat("S04_Warm_Hardware", new Color(.48f, .44f, .35f), .38f);
    static Material Paper => F.Mat("S04_Paper", new Color(.94f, .91f, .80f), .06f);
    static Material Blue => F.Mat("S04_Dusty_Blue", new Color(.32f, .43f, .49f), .08f);
    static Material MeetingFabric => F.Mat("S04_Meeting_Taupe_Fabric", new Color(.64f, .61f, .54f), .06f);

    static GameObject R(Transform p, string name, Vector3 pos, Vector3 size, Material material, float radius = .06f)
    { return F.Rounded(p, name, pos, size, material, radius); }
    static GameObject B(Transform p, string name, Vector3 pos, Vector3 size, Material material)
    { return F.Box(p, name, pos, size, material, false); }
    static GameObject P(Transform p, string name, PrimitiveType shape, Vector3 pos, Vector3 size, Material material)
    { return F.Primitive(p, name, shape, pos, size, material); }
    static void BoundsCollider(Transform p, Vector3 centre, Vector3 size)
    { var c = p.gameObject.AddComponent<BoxCollider>(); c.center = centre; c.size = size; }

    static Transform MeetingChair(Transform parent, string name, Vector3 pos, float yaw)
    {
        var chair = F.Group(parent, name, pos, yaw);
        foreach (float x in new[] { -.215f, .215f })
            foreach (float z in new[] { -.205f, .205f })
                B(chair, "Straight oak chair leg", new Vector3(x, .215f, z), new Vector3(.055f, .43f, .055f), Oak);
        R(chair, "Square oak seat frame", new Vector3(0, .415f, .01f), new Vector3(.53f, .06f, .51f), Oak, .018f);
        R(chair, "Tailored square upholstered seat", new Vector3(0, .475f, .02f), new Vector3(.50f, .09f, .49f), MeetingFabric, .025f);
        foreach (float x in new[] { -.225f, .225f })
            B(chair, "Straight oak back upright", new Vector3(x, .69f, -.285f), new Vector3(.038f, .52f, .045f), Oak);
        var back = R(chair, "Square softly upholstered back", new Vector3(0, .745f, -.24f), new Vector3(.50f, .43f, .09f), MeetingFabric, .02f);
        back.transform.localRotation = Quaternion.Euler(-4, 0, 0);
        foreach (float x in new[] { -.265f, .265f })
        {
            foreach (float z in new[] { -.14f, .14f })
                B(chair, "Slim upright arm support", new Vector3(x, .55f, z), new Vector3(.025f, .18f, .028f), Oak);
            R(chair, "Straight low oak armrest", new Vector3(x, .65f, .015f), new Vector3(.05f, .035f, .36f), Oak, .012f);
        }
        BoundsCollider(chair, new Vector3(0, .50f, 0), new Vector3(.62f, 1f, .64f));
        return chair;
    }

    public static Transform BuildMeetingSet(Transform parent)
    {
        var meeting = F.Group(parent, "S04_Main_meeting_set", MeetingSetOffset);
        var table = F.Group(meeting, "S04_Eight_seat_rectangular_meeting_table", TableCentre);
        R(table, "Large rectangular honey oak meeting tabletop", new Vector3(0, .73f, 0), new Vector3(TableWidth, .06f, TableDepth), Oak, .045f);
        // The two inset panels sit between the seating bays, leaving every chair's knee space open.
        foreach (float x in new[] { -.60f, .60f })
        {
            R(table, "Inset straight oak panel support", new Vector3(x, .365f, 0), new Vector3(.14f, .67f, .86f), Oak, .012f);
            B(table, "Recessed panel foot", new Vector3(x, .015f, 0), new Vector3(.12f, .03f, .80f), Hardware);
        }
        B(table, "Concealed longitudinal oak support beam", new Vector3(0, .645f, 0), new Vector3(2.64f, .11f, .14f), Oak);
        R(table, "Flush central meeting cable box", new Vector3(0, .763f, 0), new Vector3(.38f, .006f, .13f), Hardware, .012f);
        R(table, "Quiet cream cable box lid", new Vector3(0, .767f, 0), new Vector3(.325f, .003f, .09f), Cream, .008f);
        foreach (float z in new[] { -.41f, .41f })
            foreach (float x in new[] { -1.05f, 0f, 1.05f })
            {
                var notes = F.Group(table, "Simple meeting notepad", new Vector3(x, .76f, z), z > 0 ? 180 : 0);
                R(notes, "Dusty blue notebook cover", new Vector3(0, .006f, 0), new Vector3(.245f, .012f, .30f), Blue, .009f);
                B(notes, "Cream meeting notes", new Vector3(.004f, .014f, .005f), new Vector3(.226f, .005f, .276f), Paper);
                B(notes, "Small warm meeting pencil", new Vector3(.145f, .006f, 0), new Vector3(.009f, .009f, .18f), Hardware);
            }
        BoundsCollider(table, new Vector3(0, .38f, 0), new Vector3(TableWidth, .76f, TableDepth));

        int index = 0;
        foreach (float z in new[] { -8.10f, -10.40f })
            foreach (float x in new[] { 12.05f, 13.1f, 14.15f })
                MeetingChair(meeting, "S04_Meeting_chair_" + (++index).ToString("00"), new Vector3(x, 0, z), z > -9 ? 180 : 0);
        AddEndSeats(meeting);
        AddEndPlaceSettings(table);
        return meeting;
    }

    public static void AddEndSeats(Transform meeting)
    {
        MeetingChair(meeting, "S04_Meeting_chair_07", new Vector3(11.20f, 0, -9.25f), 90);
        MeetingChair(meeting, "S04_Meeting_chair_08", new Vector3(15.00f, 0, -9.25f), 270);
    }

    public static void AddEndPlaceSettings(Transform table)
    {
        foreach (float x in new[] { -1.30f, 1.30f })
        {
            var notes = F.Group(table, "End seat meeting notepad", new Vector3(x, .76f, 0), x < 0 ? 90 : 270);
            R(notes, "Dusty blue notebook cover", new Vector3(0, .006f, 0), new Vector3(.245f, .012f, .30f), Blue, .009f);
            B(notes, "Cream meeting notes", new Vector3(.004f, .014f, .005f), new Vector3(.226f, .005f, .276f), Paper);
            B(notes, "Small warm meeting pencil", new Vector3(.145f, .006f, 0), new Vector3(.009f, .009f, .18f), Hardware);
        }
    }

    static Transform Armchair(Transform parent, string name, Vector3 pos, float yaw, Material fabric)
    {
        var chair = F.Group(parent, name, pos, yaw);
        foreach (float x in new[] { -.25f, .25f })
            foreach (float z in new[] { -.24f, .24f })
                P(chair, "Short warm oak foot", PrimitiveType.Cylinder,
                    new Vector3(x, .115f, z), new Vector3(.065f, .115f, .065f), Oak);
        R(chair, "Rounded upholstered chair base", new Vector3(0, .31f, 0), new Vector3(.74f, .22f, .73f), fabric, .105f);
        R(chair, "Loose soft seat cushion", new Vector3(0, .451f, .035f), new Vector3(.57f, .105f, .57f), fabric, .09f);
        var back = R(chair, "Gently reclined soft back", new Vector3(0, .68f, -.29f), new Vector3(.74f, .52f, .16f), fabric, .10f);
        back.transform.localRotation = Quaternion.Euler(-5, 0, 0);
        foreach (float x in new[] { -.325f, .325f })
            R(chair, "Low rounded arm cushion", new Vector3(x, .56f, .025f), new Vector3(.13f, .22f, .67f), fabric, .062f);
        BoundsCollider(chair, new Vector3(0, .48f, 0), new Vector3(.80f, .96f, .80f));
        return chair;
    }

    static void ClosedStorage(Transform parent)
    {
        const float width = 2f, depth = .48f, height = .90f;
        var cabinet = F.Group(parent, "S04_Closed_meeting_storage", new Vector3(16f, 0, -6.35f));
        B(cabinet, "Recessed warm plinth", new Vector3(0, .05f, .025f), new Vector3(width - .12f, .10f, depth - .07f), Hardware);
        R(cabinet, "Cream closed cabinet body", new Vector3(0, .47f, 0), new Vector3(width, .80f, depth), Cream, .025f);
        R(cabinet, "Rounded solid oak countertop", new Vector3(0, height - .027f, 0), new Vector3(width + .035f, .054f, depth + .025f), Oak, .026f);
        for (int i = 0; i < 4; i++)
        {
            float x = -.75f + i * .50f;
            R(cabinet, "Flush closed meeting supply door", new Vector3(x, .465f, -.246f), new Vector3(.481f, .74f, .025f), Cream, .012f);
            B(cabinet, "Inset warm finger pull", new Vector3(x, .73f, -.260f), new Vector3(.095f, .012f, .003f), Hardware);
        }
        BoundsCollider(cabinet, new Vector3(0, height / 2, -.006f), new Vector3(width + .035f, height, depth + .04f));
        var tray = F.Group(cabinet, "Small meeting stationery tray", new Vector3(-.57f, .905f, 0));
        R(tray, "Oak tray", new Vector3(0, .013f, 0), new Vector3(.34f, .026f, .22f), Oak, .03f);
        B(tray, "A few stacked meeting notes", new Vector3(0, .035f, 0), new Vector3(.27f, .018f, .18f), Paper);
        var plant = F.Plant(cabinet, new Vector3(.67f, .903f, .015f), .32f);
        plant.name = "S04_Countertop_plant";
    }

    public static Transform BuildMainWhiteboard(Transform parent)
    {
        var board = F.Group(parent, "S04_Main_whiteboard", new Vector3(10.16f, 1.62f, -9.65f), 270);
        var surface = F.Mat("S04_Main_Whiteboard_Surface", new Color(.975f, .977f, .968f), .32f);
        var frame = F.Mat("S04_Main_Whiteboard_Warm_Metal", new Color(.74f, .71f, .63f), .42f);
        var charcoal = F.Mat("S04_Whiteboard_Charcoal_Marker", new Color(.18f, .20f, .21f), .12f);
        var markerBlue = F.Mat("S04_Whiteboard_Blue_Marker", new Color(.16f, .33f, .57f), .12f);
        var markerGreen = F.Mat("S04_Whiteboard_Green_Marker", new Color(.20f, .43f, .31f), .12f);
        R(board, "Slim warm metal whiteboard frame", Vector3.zero, new Vector3(2.10f, 1.25f, .065f), frame, .018f);
        B(board, "Large clean white writing surface", new Vector3(0, 0, -.036f), new Vector3(2.02f, 1.17f, .008f), surface);
        R(board, "Full width warm marker tray", new Vector3(0, -.645f, -.075f), new Vector3(1.86f, .035f, .13f), frame, .012f);
        var markerColours = new[] { charcoal, markerBlue, markerGreen };
        for (int i = 0; i < markerColours.Length; i++)
        {
            float x = -.58f + i * .22f;
            P(board, "Whiteboard marker barrel", PrimitiveType.Cylinder, new Vector3(x, -.611f, -.085f), new Vector3(.018f, .075f, .018f), surface)
                .transform.localRotation = Quaternion.Euler(0, 0, 90);
            P(board, "Whiteboard marker coloured cap", PrimitiveType.Cylinder, new Vector3(x - .066f, -.611f, -.085f), new Vector3(.021f, .018f, .021f), markerColours[i])
                .transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        // At yaw 270 the writing face points into the room (+X); the shallow collider stays inside the clear wall line.
        BoundsCollider(board, new Vector3(0, -.035f, -.06f), new Vector3(2.10f, 1.32f, .18f));
        return board;
    }

    static void PresentationWall(Transform parent)
    {
        BuildMainWhiteboard(parent);

        var board = F.Group(parent, "S04_Whiteboard", new Vector3(10.16f, 1.60f, -7.40f), 270);
        board.localScale = new Vector3(1.40f / 1.72f, .85f, 1);
        R(board, "Light oak whiteboard frame", Vector3.zero, new Vector3(1.72f, 1.12f, .065f), Oak, .022f);
        B(board, "Clean warm white writing surface", new Vector3(0, 0, -.036f), new Vector3(1.64f, 1.04f, .008f), Paper);
        B(board, "Small sage planning heading", new Vector3(-.37f, .29f, -.043f), new Vector3(.60f, .018f, .003f), Sage);
        for (int i = 0; i < 3; i++)
            B(board, "A few quiet blue planning notes", new Vector3(-.31f, .09f - i * .14f, -.043f), new Vector3(.72f - i * .09f, .011f, .003f), Blue);
        B(board, "Cream meeting reminder", new Vector3(.48f, .13f, -.046f), new Vector3(.22f, .25f, .008f), Cream);
        R(board, "Slim oak marker ledge", new Vector3(0, -.58f, -.055f), new Vector3(1.36f, .035f, .10f), Oak, .014f);
        BoundsCollider(board, new Vector3(0, -.018f, -.012f), new Vector3(1.72f, 1.19f, .09f));
    }

    static void WindowCurtains(Transform parent)
    {
        var curtains = F.Group(parent, "S04_Tied_back_window_side_curtains");
        var fabric = F.Mat("S04_Ivory_Curtain_Linen", new Color(.82f, .81f, .72f), .04f);
        // Only narrow gathered side drops: glazing remains open and the interior stays bright.
        foreach (float x in new[] { 11.47f, 16.38f })
            CurtainDrop(curtains, new Vector3(x, 1.66f, -12.56f), 0, fabric);
        B(curtains, "South window curtain rail", new Vector3(13.925f, 2.77f, -12.56f), new Vector3(5.24f, .026f, .035f), Hardware);
        foreach (float z in new[] { -11.27f, -7.53f })
            CurtainDrop(curtains, new Vector3(17.60f, 1.66f, z), 90, fabric);
        B(curtains, "East window curtain rail", new Vector3(17.60f, 2.77f, -9.40f), new Vector3(.035f, .026f, 4.04f), Hardware);
    }

    static void CurtainDrop(Transform parent, Vector3 pos, float yaw, Material fabric)
    {
        var drop = F.Group(parent, "Gathered ivory side curtain", pos, yaw);
        for (int fold = 0; fold < 5; fold++)
            R(drop, "Soft vertical linen fold", new Vector3((fold - 2) * .058f, 0, fold % 2 == 0 ? .012f : -.012f), new Vector3(.075f, 2.12f, .085f), fabric, .035f);
        R(drop, "Quiet linen tieback", new Vector3(0, -.29f, -.047f), new Vector3(.305f, .045f, .022f), Cream, .01f);
    }

    public static void Build(Transform parent)
    {
        var suite = F.Group(parent, "S04__Six_person_meeting_and_window_conversation");
        BuildMeetingSet(suite);
        Armchair(suite, "S04_Lounge_armchair_01", new Vector3(16.65f, 0, -10f), 210, Sage);
        Armchair(suite, "S04_Lounge_armchair_02", new Vector3(16.65f, 0, -11.7f), 330, Oatmeal);
        var side = F.Group(suite, "S04_Lounge_side_table", LoungeTableCentre);
        P(side, "Round warm oak side tabletop", PrimitiveType.Cylinder, new Vector3(0, .495f, 0), new Vector3(.54f, .025f, .54f), Oak);
        P(side, "Cream round side table pedestal", PrimitiveType.Cylinder, new Vector3(0, .255f, 0), new Vector3(.075f, .23f, .075f), Cream);
        P(side, "Stable warm oak round foot", PrimitiveType.Cylinder, new Vector3(0, .025f, 0), new Vector3(.32f, .025f, .32f), Oak);
        BoundsCollider(side, new Vector3(0, .26f, 0), new Vector3(.54f, .52f, .54f));
        R(side, "Small cloth covered reading book", new Vector3(-.02f, .535f, .01f), new Vector3(.19f, .025f, .23f), Cream, .01f);
        ClosedStorage(suite);
        PresentationWall(suite);
        WindowCurtains(suite);
        var plant = F.Plant(suite, new Vector3(10.72f, 0, -6.64f), .90f);
        plant.name = "S04_Quiet_corner_plant";
    }
}
