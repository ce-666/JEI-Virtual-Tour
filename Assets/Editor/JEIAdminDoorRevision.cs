using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Moves only S03's north doorway from X6.60 to X7.30, preserving its 1.20 m width.</summary>
public static class JEIAdminDoorRevision
{
    const string PlanPath = "Assets/JEI_Blockout/Floor01_Plan.json";
    const string WallPrefix = "{\"axis\":\"H\",\"at\":-5.9,";
    const string OldOpening = "{\"at\":6.6,\"width\":1.2,\"kind\":\"door\"}";
    const string NewOpening = "{\"at\":7.3,\"width\":1.2,\"kind\":\"door\"}";

    [Serializable] sealed class PlanData { public WallData[] walls; }
    [Serializable] sealed class WallData
    {
        public string axis;
        public float at, start, end;
        public bool external;
        public OpeningData[] openings;
    }
    [Serializable] sealed class OpeningData { public float at, width; public string kind; }

    sealed class Change
    {
        public string name;
        public Vector3 beforePosition, afterPosition, beforeScale, afterScale;
        public Change(string name, float oldX, float newX, float y, float oldWidth,
            float newWidth, float height, float depth)
        {
            this.name = name;
            beforePosition = new Vector3(oldX, y, -5.9f);
            afterPosition = new Vector3(newX, y, -5.9f);
            beforeScale = new Vector3(oldWidth, height, depth);
            afterScale = new Vector3(newWidth, height, depth);
        }
    }

    public static void Apply(Transform floor)
    {
        if (!floor) throw new ArgumentNullException(nameof(floor));
        Transform partition = null;
        foreach (Transform item in floor.GetComponentsInChildren<Transform>(true))
        {
            if (item.name != "Partition_H_-5.9") continue;
            if (partition) throw new InvalidOperationException("More than one south-corridor partition exists.");
            partition = item;
        }
        if (!partition) throw new InvalidOperationException("Partition_H_-5.9 was not found below the supplied floor.");

        var changes = new[] {
            new Change("Wall", 3.6f, 3.95f, 1.65f, 4.8f, 5.5f, 3.3f, .2f),
            new Change("Wall", 6.6f, 7.3f, 2.85f, 1.2f, 1.2f, .9f, .2f),
            new Change("Wall", 9.95f, 10.3f, 1.65f, 5.5f, 4.8f, 3.3f, .2f),
            new Change("Visible wall section edge", 3.6f, 3.95f, 3.312f, 4.8f, 5.5f, .024f, .2f),
            new Change("Visible wall section edge", 6.6f, 7.3f, 3.312f, 1.2f, 1.2f, .024f, .2f),
            new Change("Visible wall section edge", 9.95f, 10.3f, 3.312f, 5.5f, 4.8f, .024f, .2f),
            new Change("Opening jamb", 5.975f, 6.675f, 1.2f, .05f, .05f, 2.4f, .24f),
            new Change("Opening jamb", 7.225f, 7.925f, 1.2f, .05f, .05f, 2.4f, .24f),
            new Change("Opening head or sill", 6.6f, 7.3f, 2.4f, 1.2f, 1.2f, .05f, .24f),
            new Change("Open door__clear_1.20m", 6.6f, 7.3f, .027f, 1.2f, 1.2f, .02f, .25f)
        };

        string original = File.ReadAllText(PlanPath);
        float planDoorX = ValidatePlan(original);
        Transform[] before, after;
        bool isBefore = TryCollect(partition, changes, false, out before);
        bool isAfter = TryCollect(partition, changes, true, out after);
        if (isAfter && Near(planDoorX, 7.3f)) return;
        if (!isBefore || !Near(planDoorX, 6.6f))
            throw new InvalidOperationException("S03 doorway geometry and plan are not the expected complete original or revised state. Nothing was changed.");

        // Preserve every other JSON byte: replace the exact opening token only
        // within the already parsed and validated H/-5.9 wall's existing line.
        int wallStart = original.IndexOf(WallPrefix, StringComparison.Ordinal);
        if (wallStart < 0 || original.IndexOf(WallPrefix, wallStart + 1, StringComparison.Ordinal) >= 0)
            throw new InvalidDataException("The exact south-corridor wall text is missing or duplicated.");
        int wallEnd = original.IndexOf('\n', wallStart);
        if (wallEnd < 0) wallEnd = original.Length;
        string line = original.Substring(wallStart, wallEnd - wallStart);
        int openingIndex = line.IndexOf(OldOpening, StringComparison.Ordinal);
        if (openingIndex < 0 || line.IndexOf(OldOpening, openingIndex + 1, StringComparison.Ordinal) >= 0)
            throw new InvalidDataException("The exact original S03 opening text is missing or duplicated.");
        int at = wallStart + openingIndex;
        string revised = original.Substring(0, at) + NewOpening + original.Substring(at + OldOpening.Length);
        if (!Near(ValidatePlan(revised), 7.3f)) throw new InvalidDataException("The revised plan did not validate.");

        // File I/O completes before scene mutation, so a write failure leaves all
        // ten original transforms intact. No other wall object is rebuilt.
        byte[] bytes = File.ReadAllBytes(PlanPath);
        bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        File.WriteAllText(PlanPath, revised, new UTF8Encoding(bom));
        for (int i = 0; i < changes.Length; i++)
        {
            before[i].localPosition = changes[i].afterPosition;
            before[i].localScale = changes[i].afterScale;
        }
        EditorSceneManager.MarkSceneDirty(floor.gameObject.scene);
        AssetDatabase.ImportAsset(PlanPath, ImportAssetOptions.ForceUpdate);
        Physics.SyncTransforms();
    }

    static bool TryCollect(Transform partition, Change[] changes, bool revised, out Transform[] result)
    {
        result = new Transform[changes.Length];
        for (int i = 0; i < changes.Length; i++)
        {
            Change change = changes[i];
            Vector3 position = revised ? change.afterPosition : change.beforePosition;
            Vector3 scale = revised ? change.afterScale : change.beforeScale;
            int count = 0;
            foreach (Transform child in partition)
            {
                if (child.name != change.name || !Near(child.localPosition, position) ||
                    !Near(child.localScale, scale) || Quaternion.Angle(child.localRotation, Quaternion.identity) > .01f) continue;
                result[i] = child;
                count++;
            }
            if (count != 1) return false;
        }
        return true;
    }

    static float ValidatePlan(string json)
    {
        var plan = JsonUtility.FromJson<PlanData>(json);
        WallData target = null;
        if (plan == null || plan.walls == null) throw new InvalidDataException("The plan has no walls.");
        foreach (WallData wall in plan.walls)
        {
            if (wall.axis != "H" || !Near(wall.at, -5.9f)) continue;
            if (target != null) throw new InvalidDataException("The south-corridor plan wall is duplicated.");
            target = wall;
        }
        if (target == null || target.external || !Near(target.start, -17.85f) || !Near(target.end, 17.85f) ||
            target.openings == null || target.openings.Length != 4)
            throw new InvalidDataException("The south-corridor wall definition is unexpected.");
        float[] otherX = { -8f, 0f, 0f, 13.3f };
        float[] widths = { 1.2f, 2.4f, 1.2f, 1.2f };
        for (int i = 0; i < 4; i++)
        {
            OpeningData opening = target.openings[i];
            if (opening == null || opening.kind != "door" || !Near(opening.width, widths[i]) ||
                (i != 2 && !Near(opening.at, otherX[i])))
                throw new InvalidDataException("A neighbouring opening differs from the expected plan.");
        }
        float x = target.openings[2].at;
        if (!Near(x, 6.6f) && !Near(x, 7.3f)) throw new InvalidDataException("S03 door is neither at X6.60 nor X7.30.");
        return x;
    }

    static bool Near(float a, float b) { return Mathf.Abs(a - b) < .001f; }
    static bool Near(Vector3 a, Vector3 b) { return (a - b).sqrMagnitude < .000001f; }
}
