#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SandboxNetwork
{
    public class ScriptCheatEditor : EditorWindow
    {
        int ScriptKey = 0;

        [MenuItem("SB Tools/Script Cheat")]
        static void Init()
        {
            ScriptCheatEditor cheatWindow = (ScriptCheatEditor)GetWindow(typeof(ScriptCheatEditor));
            cheatWindow.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            ScriptKey = EditorGUILayout.IntField("Script Key", ScriptKey);

            if (GUILayout.Button("Play"))
            {
                StartScript();
                Debug.LogFormat("Script Cheat Start ==> Key : {0}", ScriptKey);
            }
            if (GUILayout.Button("Stop"))
            {
                StopScript();
            }
            if (GUILayout.Button("Next"))
            {
                NextScript();
            }
            if (GUILayout.Button("Clear First Intro Data ( account exp should be 0 )"))
            {
                ResetIntro();
            }
        }

        void StartScript()
        {
            ScenarioManager.Instance.OnScript(ScriptTriggerData.Get(ScriptKey));
        }

        void StopScript()
        {
            ScenarioManager.Instance.StopScript();
        }

        void NextScript()
        {
            ScenarioManager.Instance.NexstScript();
        }

        void ExitScript()
        {
            ScenarioManager.Instance.NexstScript();
        }

        void ResetIntro()
        {
            CacheUserData.SetBoolean("intro_show", false);
            CacheUserData.SetBoolean("first_show", false);
        }
    }
}
#endif
