using DG.Tweening.Core.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG && UNITY_EDITOR
namespace SandboxNetwork
{
	public class SBDevelop
	{
		public enum SBScene
		{
			START,
			TOOL
		}

		public static SBScene sceneMode = SBScene.START;

		[UnityEditor.MenuItem("SB Tools/Scene/일반 모드")]
		public static void StartMode()
		{
			Debug.Log("일반 개발 모드로 설정");
			sceneMode = SBScene.START;
		}

		[UnityEditor.MenuItem("SB Tools/Scene/툴 개발 모드")]
		public static void TOOLMode()
		{
			Debug.Log("툴 개발 모드로 설정");
			sceneMode = SBScene.TOOL;
		}

		[UnityEditor.MenuItem("SB Tools/PlayerPrefs 초기화")]
		public static void ResetPlayerPrefs()
		{
			Debug.Log("PlayerPrefs 초기화");
			PlayerPrefs.DeleteAll();
		}
		[UnityEditor.MenuItem("SB Tools/강제 세션 해제")]
		public static void ResetSession()
		{
			SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002674),
											() =>
											{
												UIManager.Instance.InitUI(eUIType.None);
												LoadingManager.Instance.LoadStartScene();
												PopupManager.AllClosePopup();
											},
											null,
											() =>
											{
												UIManager.Instance.InitUI(eUIType.None);
												LoadingManager.Instance.LoadStartScene();
												PopupManager.AllClosePopup();
											},
										true, false, false);
		}

		[UnityEditor.MenuItem("SB Tools/CDN 리소스 최신화")]
		public static void OnRefreshCDNResource()
        {
			if(Town.Instance == null)
            {
				Debug.LogError("타운에서만 실행해주셈.");
				return;
            }

			string ImagesFolderPath = Application.dataPath + "/Resources/AssetBundle/Images/";
			List<string> resources = new List<string>();
			string[] langFolders = { "/kr", "/en", "/jp", "/prt" };

			resources.Clear();
			//Gacha

			//GachaGroupData
			foreach (var data in GachaGroupData.GetAll())
            {
				resources.Add("gacha/" + data.resource);
			}
			//GachaMenuData
			foreach (var data in TableManager.GetTable<GachaMenu>().GetAllList())
			{
				resources.Add("gacha/" + data.resource);
			}

			foreach (var file in System.IO.Directory.GetFiles(ImagesFolderPath + "gacha", "*.*", System.IO.SearchOption.TopDirectoryOnly))
			{
				string fileName = file.Replace(ImagesFolderPath, "").Replace("\\", "/");
				if (!resources.Contains(fileName))
				{
					System.IO.File.Delete(ImagesFolderPath + fileName);
				}
			}

			resources.Clear();
			//event

			//EventBannerData
			foreach (var data in TableManager.GetTable<EventBannerTable>().GetAllList())
			{
				foreach(var lang in langFolders)
					resources.Add("event" + lang + "/" + data.RESOURCE);
			}

			//EventScheduleData
			foreach (var data in TableManager.GetTable<EventScheduleTable>().GetAllList())
			{
				foreach (var lang in langFolders)
					resources.Add("event" + lang + "/" + data.RESOURCE);
			}

			foreach (var lang in langFolders)
			{
				foreach (var file in System.IO.Directory.GetFiles(ImagesFolderPath + "event" + lang, "*.*", System.IO.SearchOption.TopDirectoryOnly))
				{
					string fileName = file.Replace(ImagesFolderPath, "").Replace("\\", "/");
					if (!resources.Contains(fileName))
					{
						System.IO.File.Delete(ImagesFolderPath + fileName);
					}
				}
			}

			resources.Clear();
			//shop
			//ShopGoodsData
			foreach (var data in TableManager.GetTable<ShopGoodsTable>().GetAllList())
			{
				foreach (var lang in langFolders)
					resources.Add("store" + lang + "/" + data.RESOURCE);
			}

			foreach (var data in TableManager.GetTable<ShopBannerTable>().GetAllList())
			{
				foreach (var lang in langFolders)
					resources.Add("store" + lang + "/" + data.RESOURCE);
			}

			foreach (var lang in langFolders)
			{
				foreach (var file in System.IO.Directory.GetFiles(ImagesFolderPath + "store" + lang, "*.*", System.IO.SearchOption.TopDirectoryOnly))
				{
					string fileName = file.Replace(ImagesFolderPath, "").Replace("\\", "/");
					if (!resources.Contains(fileName))
					{
						System.IO.File.Delete(ImagesFolderPath + fileName);
					}
				}
			}
		}
	}
}
#endif