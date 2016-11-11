using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using CreativeSpore.RpgMapEditor;
using stackRPG;

public partial class MGameManager : Singleton<MGameManager>
{
    IEnumerator LoadMap()
    {
        if (SingleGameManager.Instance.NextStage() == false) { ChangeGameState(GameState.Finish); yield break; }

        Map map = SingleGameManager.Instance.m_currentMap;
        AutoTileMap.Instance.Tileset = map.m_autoTileset;
        AutoTileMap.Instance.MapData = map.m_autoTileMapData;

        yield return null;
        MGameCamera.Instance.InitMapData();

        ChangeGameState(GameState.StartStage);
    }
}
