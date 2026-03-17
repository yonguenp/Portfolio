
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
	public class LocalizeString : MonoBehaviour, EventListener<SettingEvent>
	{
		public int index = -1;//지우고싶음

		[SerializeField]
		private string key = "";

		public string Key { get { return key; } }

		StringData data = null;
		string Text { 
			get {
				
				if (data == null)
				{
					DataLoad();
				}

				if (data != null)
					return data.TEXT;

				Debug.LogWarning("not found StringData : index " + index.ToString() + ", key : " + key);
				return (index > 0 ? index.ToString() : key);
			} 
		}

		public string format = "";
		public string stringArgs = "";

		Text textlabel = null;

		private void DataLoad()
        {
			if (index > 0)
			{
				data = StringData.Get(index);
			}
			else
			{
				data = StringData.Get(key.Trim());
			}
		}
		private void Awake()
		{
			EventManager.AddListener(this);
			RefreshLabel();
		}
        
        private void OnDestroy()
		{
			EventManager.RemoveListener(this);
		}

		private void Start()
		{
			RefreshLabel();
		}

		void RefreshLabel()
		{
			if(textlabel == null)
            {
				textlabel = GetComponent<Text>();
            }

			if (textlabel != null)
			{
				if (format != "")
					textlabel.text = string.Format(format, Text);
				else
					textlabel.text = Text;

				if (stringArgs != "")
				{
					string tempStr = textlabel.text;
					string[] strArr = stringArgs.Split(",");

					textlabel.text = string.Format(tempStr, strArr);
				}
			}
		}

		public void SetIndex(int idx)
		{
			index = idx;
			DataLoad();
			RefreshLabel();
		}
		public void OnEvent(SettingEvent eventType)
		{
			switch(eventType.Event)
			{
				case SettingEvent.eSettingEventEnum.REFRESH_STRING:
					DataLoad();
					RefreshLabel();
					break;

				default:
					break;
			}
		}

		public string GetText()
        {
			if (format != "")
				return string.Format(format, Text);
			else
				return Text;
		}

#if UNITY_EDITOR
		public string KEY { set { key = value; } }

		[UnityEditor.MenuItem("Custom/LocalizeStringCheckScene")]
		public static void LocalizeStringCheck()
		{
			var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
			GameObject[] roots = scene.GetRootGameObjects();
			foreach (GameObject root in roots)
			{
				foreach (var ls in root.GetComponentsInChildren<LocalizeString>(true))
				{
					Debug.Log("has LocalizeString : " + ls.name);
					if (ls.index > 0)
					{
						StringData data = StringData.Get(ls.index);
						if (data == null)
						{
							Debug.LogError("not found index(" + ls.name + ") : " + ls.index);
						}
						else
						{
							if (string.IsNullOrEmpty(data.STR_KEY))
							{
								Debug.LogError("str_key is empty(" + ls.name + ") : " + ls.index);
							}
							else
							{
								ls.index = -1;
								ls.KEY = data.STR_KEY;
                                UnityEditor.Undo.RecordObject(ls, "Change STR KEY");
							}
						}
					}
				}
			}

			UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
		}


		[ContextMenu("LocalizeStringCheckPrefab")]
		public void LocalizeStringCheckPrefab()
		{
			SBGameData.GetCompactLocalDatas();

			foreach (var ls in gameObject.transform.GetComponentsInChildren<LocalizeString>(true))
			{
				if (ls.index > 0)
				{
					//Debug.Log("<color=yellow>has LocalizeString</color> : " + ls.name);

					StringData data = StringData.Get(ls.index);
					if (data == null)
					{
						Debug.Log("<color=red>not found index(" + ls.name + ")</color> : " + ls.index);
					}
					else
					{
						if (string.IsNullOrEmpty(data.STR_KEY))
						{
							Debug.Log("<color=red>str_key is empty(" + ls.name + ")</color> : " + ls.index);
						}
						else
						{
							UnityEditor.Undo.RecordObject(ls, "Change STR KEY");
							ls.index = -1;
							ls.KEY = data.STR_KEY;
							Debug.Log("<color=green>str_key changed(" + ls.name + ")</color> : " + data.STR_KEY);
						}
					}
				}
				else
                {
					Debug.Log("<color=green>Already LocalizeString Done</color> (" + ls.name + ") : " + ls.key);
				}
			}
		}
#endif
	}
}