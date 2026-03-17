/*
This is a really simple text editor to be used within Unity
It's useful to write memos, notes or ideas inside your Unity Editor window
Kinda like Blender's Text Editor
*/
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using SandboxNetwork;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Collections;

public class AnnounceEditor : EditorWindow
{
    class AnnounceEditorData
    {
        int KEY = -1;
        eActionType ACTION_TYPE = eActionType.NONE;
        int ACTION_PARAM = 0;
        DateTime START;
        DateTime END;

        JObject data = null;
        public AnnounceEditorData(JObject jobject)
        {
            data = jobject;
        }

        public string GetKORTitle() { return data["KOR_TITLE"].Value<string>(); }
        public string GetTitle(string lang) { return SBFunc.Replace(data[lang + "_TITLE"].Value<string>()); }
        public string GetMessage(string lang) { return SBFunc.Replace(data[lang + "_MSG"].Value<string>()); }
    }
    string titleText;
    string msgText;
    string lang;
    enum STATE { 
        DEV_LOAD_FAIL,

        INIT,
        DEV_LOADING,        
        DEV_LIST,
        DEV_EDITING,
        DEV_SAVE,

        LIVE_SYNC
    }

    List<AnnounceEditorData> AnnounceDataList = new List<AnnounceEditorData>();
    STATE curState = STATE.INIT;
    AnnounceEditorData curSelected = null;
    [MenuItem("SB Tools/Announce Editor")]
    static void Init()
    {
        var win = GetWindow<AnnounceEditor>(false, "Announce Editor");
        
        win.LoadDev();
    }

    void Clear()
    {
        curState = STATE.INIT;
        AnnounceDataList.Clear();

        curSelected = null;

        titleText = "";
        msgText = "";
        lang = "KOR";

        DefocusAndRepaint();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        var toolbar_rect = DrawToolbar();
        float y_offset = toolbar_rect.height + toolbar_rect.y;
        
        switch (curState)
        {
            case STATE.INIT:
                break;
            case STATE.DEV_LOADING:
                break;
            case STATE.DEV_LOAD_FAIL:                
                break;
            case STATE.DEV_LIST:
                
                foreach (var data in AnnounceDataList)
                {
                    if(GUILayout.Button(data.GetKORTitle(), EditorStyles.miniButtonMid, GUILayout.Width(position.width)))
                    {
                        curSelected = data;
                        curState = STATE.DEV_EDITING;

                        ResetTextArea();
                    }
                }                
                break;

            case STATE.DEV_EDITING:
            case STATE.DEV_SAVE:
                if(curSelected != null)
                {
                    var rect = DrawLanguageMenu();
                    y_offset += rect.height;

                    var style = EditorStyles.textArea;
                    style.richText = true;

                    float title_height = 40.0f;
                    var text_rect = new Rect(toolbar_rect.x, y_offset, position.width, title_height);
                    titleText = EditorGUI.TextArea(text_rect, titleText, style);

                    text_rect = new Rect(toolbar_rect.x, y_offset + title_height, position.width, position.height - title_height - y_offset - 4);
                    msgText = EditorGUI.TextArea(text_rect, msgText, style);
                }
                break;
            case STATE.LIVE_SYNC:
                break;
        }

        EditorGUILayout.EndVertical();
    }

    Rect DrawToolbar()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(rect, Color.white * 0.5f);
        switch(curState)
        {
            case STATE.INIT:
                GUILayout.Label("시작 중..");
                break;
            case STATE.DEV_LOADING:
                GUILayout.Label("개발 서버 로드 중..");
                break;
            case STATE.DEV_LOAD_FAIL:
                GUILayout.Label("개발 서버 로드 오류!");
                Button(new GUIContent("Reload Dev"), LoadDev, GUILayout.Width(150));
                break;
            case STATE.DEV_LIST:
            case STATE.DEV_EDITING:
            case STATE.DEV_SAVE:
            case STATE.LIVE_SYNC:
                DrawMenu();
                break;
        }

        EditorGUILayout.EndHorizontal();
        return rect;
    }

    void DrawMenu()
    {
        Button(new GUIContent("Reload Dev"), LoadDev, GUILayout.Width(150));
        Button(new GUIContent("Save Dev"), SaveDev, GUILayout.Width(150));
        Button(new GUIContent("Sync Live"), SyncLive, GUILayout.Width(150));
    }

    Rect DrawLanguageMenu()
    {
        Rect rect = EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(rect, Color.white * 0.5f);

        Button(new GUIContent("KOR"), () => { lang = "KOR"; ResetTextArea(); }, GUILayout.Width(150));
        Button(new GUIContent("ENG"), () => { lang = "ENG"; ResetTextArea(); }, GUILayout.Width(150));
        Button(new GUIContent("JPN"), () => { lang = "JPN"; ResetTextArea(); }, GUILayout.Width(150));
        Button(new GUIContent("PRT"), () => { lang = "PRT"; ResetTextArea(); }, GUILayout.Width(150));

        EditorGUILayout.EndHorizontal();
        return rect;
    }

    void ResetTextArea()
    {
        titleText = curSelected.GetTitle(lang);
        msgText = curSelected.GetMessage(lang);
    }
    void LoadDev()
    {
        Clear();

        curState = STATE.DEV_LOADING;
        EditorCoroutineUtility.StartCoroutine(OnDevLoad(), this);
    }

    IEnumerator OnDevLoad()
    {
        WWWForm param = new WWWForm();
        using (UnityWebRequest req = UnityWebRequest.Post(NetworkManager.WEB_SERVER + "system/opload", param))
        {
            req.timeout = 10;

            yield return req.SendWebRequest();

            JObject jsonData = null;
            if (!string.IsNullOrEmpty(req.downloadHandler.text))
            {
                try
                {
                    jsonData = JObject.Parse(req.downloadHandler.text);
                }
                catch
                {
                    Debug.LogError("데이터 파싱 실패");
                }
            }

            if (jsonData == null || !jsonData.ContainsKey("rs") || jsonData["rs"].Value<int>() != 0)
            {
                curState = STATE.DEV_LOAD_FAIL;
            }
            else
            {
                if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                {
                    var arr = (JArray)jsonData["announcement"];

                    for (int i = 0, count = arr.Count; i < count; ++i)
                    {
                        AnnounceDataList.Add(new AnnounceEditorData((JObject)arr[i]));
                    }

                    curState = STATE.DEV_LIST;
                }
                else
                {
                    curState = STATE.DEV_LOAD_FAIL;
                }                
            }
        }
    }

    void SaveDev()
    {
        
        DefocusAndRepaint();
    }

    void SyncLive()
    {

        DefocusAndRepaint();
    }

    void DefocusAndRepaint()
    {
        GUI.FocusControl(null);
        Repaint();
    }
    // Function used to draw buttons in one line, Copy it and use it  elsewhere if you want ;)
    void Button(GUIContent content, Action action, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(content, EditorStyles.miniButtonLeft, options))
        {
            action();
        }
    }
}
#endif