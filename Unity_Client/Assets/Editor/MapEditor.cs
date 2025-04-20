using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditor
{
    [MenuItem("Tools/GenerateMap")]
    public static void GenerateMap()
    {
        GenerateMapByPath("../Common/MapData");
        GenerateMapByPath("Assets/Resources/Map");
    }

    public static void GenerateMapByPath(string prefix)
    {
        // Prefab으로 저장된 맵 정보 로드
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");
        foreach (GameObject go in gameObjects)
        {
            GameObject tileBase = Util.FindChild(go, "Tilemap_Base");
            GameObject collision = Util.FindChild(go, "Tilemap_Collision");

            Tilemap baseTile = tileBase.GetComponent<Tilemap>();
            Tilemap collisionTile = collision.GetComponent<Tilemap>();
            if (baseTile == null || collisionTile == null)
                continue;

            using StreamWriter sw = File.CreateText($"{prefix}/{go.name}.txt");
            sw.WriteLine(baseTile.cellBounds.xMax);
            sw.WriteLine(baseTile.cellBounds.xMin);
            sw.WriteLine(baseTile.cellBounds.yMax);
            sw.WriteLine(baseTile.cellBounds.yMin);

            for (int y = baseTile.cellBounds.yMax - 1; y >= baseTile.cellBounds.yMin; --y)
            {
                for (int x = baseTile.cellBounds.xMin; x < baseTile.cellBounds.xMax; ++x)
                {
                    TileBase tile = collisionTile.GetTile(new Vector3Int(x, y));
                    sw.Write(tile != null ? 1 : 0);
                }
                sw.WriteLine();
            }

            sw.Flush();
        }
    }

    [MenuItem("Tools/MultiPlayer/4 Player")]
    private static void Perform4Player()
    {
        PerformWin64Build(4);
    }

    [MenuItem("Tools/MultiPlayer/3 Player")]
    private static void Perform3Player()
    {
        PerformWin64Build(3);
    }

    [MenuItem("Tools/MultiPlayer/2 Player")]
    private static void Perform2Player()
    {
        PerformWin64Build(2);
    }

    private static void PerformWin64Build(int playerCount)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        for (int i = 1; i <= playerCount; ++i)
        {
            BuildPipeline.BuildPlayer(GetScenePaths(),
                "TestBuilds/" + GetProjectName() + i.ToString() + "/" + GetProjectName() + i.ToString() + ".exe",
                BuildTarget.StandaloneWindows64,
                BuildOptions.AutoRunPlayer
                );
        }
    }

    private static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; ++i)
            scenes[i] = EditorBuildSettings.scenes[i].path;

        return scenes;
    }
}
