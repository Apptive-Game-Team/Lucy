#if UNITY_EDITOR
using System.IO;
using System.Text;
using Scripts_door;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class LucyDoorAudit
{
    [MenuItem("Lucy/Diagnostics/Audit Authored Doors")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Door audit requires Edit Mode.");
            return;
        }
        var report = new StringBuilder("Authored door audit (read-only; adjacency is tile occupancy, not a physics route test)\n");
        foreach (var entry in EditorBuildSettings.scenes)
        {
            if (!entry.enabled) continue;
            var scene = SceneManager.GetSceneByPath(entry.path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            try
            {
                if (opened) scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Additive);
                report.AppendLine("SCENE " + entry.path);
                foreach (var root in scene.GetRootGameObjects())
                foreach (var door in root.GetComponentsInChildren<DoorObject>(true))
                {
                    var serialized = new SerializedObject(door);
                    var leaf = serialized.FindProperty("door")?.objectReferenceValue as GameObject;
                    report.AppendLine("DOOR " + door.name + " position=" + door.transform.position.ToString("F3") +
                        " rotation=" + door.transform.eulerAngles.ToString("F1") +
                        " locked=" + serialized.FindProperty("doorInfo.isLocked")?.boolValue +
                        " opened=" + serialized.FindProperty("doorInfo.isOpened")?.boolValue);
                    if (leaf != null)
                    {
                        var renderer = leaf.GetComponent<SpriteRenderer>();
                        report.AppendLine("  LEAF active=" + leaf.activeSelf + " position=" + leaf.transform.position.ToString("F3") +
                            " rotation=" + leaf.transform.eulerAngles.ToString("F1") +
                            " sprite=" + (renderer != null ? AssetDatabase.GetAssetPath(renderer.sprite) : "none"));
                        foreach (var collider in leaf.GetComponents<Collider2D>())
                            report.AppendLine("  COLLIDER " + collider.GetType().Name + " trigger=" + collider.isTrigger +
                                " bounds=" + collider.bounds);
                    }
                    foreach (var mapRoot in scene.GetRootGameObjects())
                    foreach (var map in mapRoot.GetComponentsInChildren<Tilemap>(true))
                    {
                        if (map.name == "Floor_tilemap")
                        {
                            report.AppendLine("  FLOOR transform=" + map.transform.position + " scale=" + map.transform.lossyScale + " cell=" + map.layoutGrid.cellSize);
                            for (int y=4; y>=-3; y--)
                            {
                                string row = "  y=" + y + " ";
                                for (int x=-8; x<=2; x++) row += map.HasTile(map.WorldToCell(new Vector3(x,y,0))) ? "." : "#";
                                report.AppendLine(row);
                            }
                        }
                        var position = door.transform.position;
                        bool center = map.HasTile(map.WorldToCell(position));
                        bool west = map.HasTile(map.WorldToCell(position + Vector3.left));
                        bool east = map.HasTile(map.WorldToCell(position + Vector3.right));
                        bool north = map.HasTile(map.WorldToCell(position + Vector3.up));
                        bool south = map.HasTile(map.WorldToCell(position + Vector3.down));
                        if (center || west || east || north || south)
                            report.AppendLine("  MAP " + map.name + " C/W/E/N/S=" + center + "/" + west + "/" + east + "/" + north + "/" + south);
                    }
                }
            }
            finally
            {
                if (opened && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/LucyDoorAudit.txt", report.ToString());
        Debug.Log("Door audit written to Logs/LucyDoorAudit.txt");
    }
}
#endif
