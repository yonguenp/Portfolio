#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SandboxNetwork
{
    public class TutorialCheatEditor : EditorWindow
    {
        int tutorialGroup = 0;
        int tutorialSeq = 0;

        [MenuItem("SB Tools/Tutorial Cheat")]
        static void Init()
        {
            TutorialCheatEditor cheatWindow = (TutorialCheatEditor)GetWindow(typeof(TutorialCheatEditor));
            cheatWindow.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            tutorialGroup = EditorGUILayout.IntField("Tutorial Group", tutorialGroup);
            tutorialSeq = EditorGUILayout.IntField("Tutorial Seq", tutorialSeq);

            if (GUILayout.Button("Play"))
            {
                StartTutorial();
                Debug.LogFormat("Tutorial Cheat Start ==> Group:{0} / Seq:{1}", tutorialGroup, tutorialSeq);
            }
            if (GUILayout.Button("Stop"))
            {
                StopTutorial();
                Debug.LogFormat("Tutorial Cheat Stop ==> Group:{0} / Seq:{1}", tutorialGroup, tutorialSeq);
            }
            if (GUILayout.Button("Clear Contents Tutorial"))
            {
                ClearContentsTutorial();
                Debug.Log("[Travel, Exchange, Arena, DailyDungeon, Subway, GemDungeon] Tutorials are cleared!");
            }
        }

        void StartTutorial()
        {
            TutorialManager.tutorialManagement?.StartTutorial(tutorialGroup, tutorialSeq, true);
        }

        void StopTutorial()
        {
            TutorialManager.tutorialManagement?.EndTutorialEvent();
        }

        void ClearContentsTutorial()
        {   
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.Travel, false);
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.Exchange, false);
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.Arena, false);
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.DailyDungeon, false);
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.Subway, false);
            CacheUserData.SetBoolean("Tutoiral" + (int)TutorialDefine.GemDungeon, false);
        }
    }
}
#endif
