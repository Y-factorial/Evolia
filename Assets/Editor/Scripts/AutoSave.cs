using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Evolia.Editor
{
    /// <summary>
    /// 現在のシーンを定期的に自動保存する。
    /// Unity がフリーズしてシーンロストが頻発したので作った。
    /// </summary>
    [InitializeOnLoad]
    public class AutoSave
    {
        // 保存間隔 (秒)
        private static double saveInterval = 300;
        
        // 最後に保存した時刻
        private static double lastSaveTime;

        // プレイモード中かどうか
        // プレイ中はシーンを保存できないので。
        private static bool isPlayingOrWillPlay = false;

        static AutoSave()
        {
            lastSaveTime = EditorApplication.timeSinceStartup;

            // エディタ更新イベントにフック
            EditorApplication.update += OnEditorUpdate;

            // プレイモード状態変更イベントにフック
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnEditorUpdate()
        {
            // プレイモード中は保存を無効化
            if (isPlayingOrWillPlay) return;

            if (EditorApplication.timeSinceStartup> lastSaveTime+ saveInterval)
            {
                SaveScene();
                lastSaveTime = EditorApplication.timeSinceStartup;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    SaveScene(); // プレイモードに入る前にシーンを保存
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    isPlayingOrWillPlay = true; // プレイモード中は自動保存を停止
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    isPlayingOrWillPlay = false; // プレイモード終了時に再び自動保存を有効化
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    lastSaveTime = EditorApplication.timeSinceStartup; // タイマーをリセット
                    break;
            }
        }

        private static void SaveScene()
        {
            // シーンが開かれていない場合は無視
            if (EditorSceneManager.sceneCount == 0) return;

            // 全てのシーンを保存
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[AutoSave] Auto-saved scene: {scene.name}");
                }
            }
        }

    }

}