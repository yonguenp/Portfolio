using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandboxNetwork
{

	public class QuestManager : IManagerBase
	{
		public const string QuestSubTypeParam = "ALL";//Quest_Trigger_Group에서 논외타입(아무거나~) SUB_TYPE 체크용도
		

		private static QuestManager instance = null;
		public static QuestManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new QuestManager();
				}
				return instance;
			}
		}

		Dictionary<int, Quest> _questList;//모든 퀘스트 리스트
		Dictionary<eQuestType, Dictionary<int, Quest>> _proceedDic;//각 타입 별로 진행중인 퀘스트 데이터
		Dictionary<int, Quest> _completeDic;//(key : _qID, value : quest)

		//only UI
		Dictionary<KeyValuePair<eQuestType, eQuestGroup>, Dictionary<int, Quest>> _uiProceedDic;
		Dictionary<KeyValuePair<eQuestType, eQuestGroup>, Dictionary<int, Quest>> _uiCompleteDic;

		// 바로가기 관련
		public bool CheckQuestMove { get; set; } = false;

		public void Initialize()
		{
			_questList = new Dictionary<int, Quest>();
			_proceedDic = new Dictionary<eQuestType, Dictionary<int, Quest>>();
			_completeDic = new Dictionary<int, Quest>();
			_uiProceedDic = new();
			_uiCompleteDic = new();
		}

		//로그인시 퀘스트 정보 동기화
		public void UserDataSync(JObject jsonData)
		{
			Initialize();
			//서버 로그인 데이터와 PlayerPrefs도 같이 처리

			if (jsonData.ContainsKey("current"))
			{
				JArray current = (JArray)jsonData["current"];

				foreach (JToken token in current)
				{
					JObject datas = (JObject)token;

					int qID = datas["quest"].Value<int>();
					Quest quest = GetQuest(qID);
					if (quest == null)
						continue;
					int questTimeStamp = datas["accepted_at"].Value<int>();

					quest.SetTimeStamp(questTimeStamp);
					quest.SetState(eQuestState.PROCEEDING);

					JArray questData = (JArray)datas["data"];
					var arrayCount = questData.Count;
					for (int i = 0; i < arrayCount; i++)
					{
						int condID = questData[i][0].ToObject<int>();
						int condValue = questData[i][1].ToObject<int>();

						quest.UpdateCondition(condID, condValue);
					}

					eQuestType qType = (eQuestType)datas["type"].Value<int>();
					if (!IsContainProceedIndex(qType, qID))
						SetDataInProceedDic(qType, qID);
				}
			}

			if (jsonData.ContainsKey("accomplish"))
			{
				JArray current = (JArray)jsonData["accomplish"];

				for (int i = 0; i < current.Count; i++)
				{
					int qID = current[i].Value<int>();
					Quest quest = GetQuest(qID);
					if (quest == null)
						continue;

					quest.SetState(eQuestState.TERMINATE);
					SetDataInCompleteDic(qID);
				}
			}

			RefreshReddot();
		}

		public void ProgressUpdate(JToken jsonData)
		{
			//{"api":"quest_update","data":[{"quest":1100009,"type":1,"state":3,"data":[[21009001,1]],"accepted_at":1683788758}]}
			JArray updates = (JArray)jsonData;

			for (int i = 0; i < updates.Count; i++)
			{
				var rowData = updates[i];
				var questListKey = rowData["quest"].Value<int>();
				var questTimeStamp = rowData["accepted_at"].Value<int>();
				eQuestType qType = (eQuestType)rowData["type"].Value<int>();

				var conditionArr = (JArray)rowData["data"];
				for (int j = 0; j < conditionArr.Count; j++)
				{
					var conditionData = conditionArr[j];
					Quest quest = GetQuest(questListKey);
					if (quest == null)
						continue;

					var qID = conditionData[0].Value<int>();
					var qValue = conditionData[1].Value<int>();
					quest.UpdateCondition(qID, qValue);
					quest.SetTimeStamp(questTimeStamp);

					if (!IsContainProceedIndex(qType, questListKey))//신규 퀘스트 들어올 때
					{
						quest.SetState(eQuestState.PROCEEDING);
						SetDataInProceedDic(qType, questListKey);
						RemoveDataInCompleteDic(questListKey);//기존에 완료했었던 퀘스트가 다시 진행중으로 상태가 바뀔 때(일일미션, 이벤트퀘 등), 완료 목록 갱신

						RefreshReddot();
					}
				}
			}
		}

		public void SetQuestComplete(int qID)
		{
			Quest quest = GetQuest(qID);
			if (quest == null)
				return;

			SetQuestState(quest);
            RemoveDataInProceedDic(qID);
			SetDataInCompleteDic(qID);
			SetQuestProceedingPref(new List<int>() { qID });
		}

		public void SetQuestComplete(List<int> _qIdList)
		{
			foreach (var qID in _qIdList)
			{
				Quest quest = GetQuest(qID);
				if (quest == null)
					continue;

                SetQuestState(quest);
                RemoveDataInProceedDic(qID);
				SetDataInCompleteDic(qID);
			}

			SetQuestProceedingPref(_qIdList);
		}

        void SetQuestState(Quest _quest)
        {
            switch (_quest.Type)
            {
                case eQuestType.CHAIN:
                {
                    _quest.SetState(eQuestState.PROCEEDING);
                }
                break;
                default:
                    _quest.SetState(eQuestState.PROCESS_DONE);
                    break;
            }
        }

        void SetQuestProceedingPref(List<int> _qIdList)
		{
			JObject prefsObj = JObject.Parse(CacheUserData.GetString("QUEST_PROCEEDING"));
			JArray prefs = JArray.Parse(prefsObj["proc"].ToString());

			for (int i = 0; i < prefs.Count; i++)
			{
				JToken value = prefs[i];
				for (int j = 0; j < _qIdList.Count; j++)
				{
					if (value.Value<int>() == _qIdList[j])
						prefs.Remove(value);
				}
			}

			prefsObj["proc"] = prefs;
			CacheUserData.SetString("QUEST_PROCEEDING", prefsObj.ToString());
		}

		public void ReadReddot(int qID)
		{
			JObject prefsObj = JObject.Parse(CacheUserData.GetString("QUEST_PROCEEDING"));
			JArray prefs = (JArray)prefsObj["proc"];

			Quest quest = GetQuest(qID);
			if (quest != null)
			{
				quest.SetState(eQuestState.PROCEEDING);
			}

			prefs.Add(qID);
			prefsObj["proc"] = prefs;
			CacheUserData.SetString("QUEST_PROCEEDING", prefsObj.ToString());
		}

		private void RefreshReddot()
		{
			JObject prefsObj = JObject.Parse(CacheUserData.GetString("QUEST_PROCEEDING", "{proc:[]}"));
			JArray prefs = JArray.Parse(prefsObj["proc"].ToString());

			var currentMainUIProceedList = GetMainUIQuestProceedList();
			for (int i = 0; i < currentMainUIProceedList.Count; i++)
			{
				var qID = currentMainUIProceedList[i].ID;
				Quest quest = GetQuest(qID);
				if (quest != null)
				{
					if (prefs.ToList().Find(x => qID == x.Value<int>()) == null)
						quest.SetState(eQuestState.NEW_QUEST);//새로운 퀘
				}
			}

			prefs.Clear();
			CacheUserData.DeleteKey("QUEST_PROCEEDING");

			for (int i = 0; i < currentMainUIProceedList.Count; i++)
			{
				var qID = currentMainUIProceedList[i].ID;
				Quest quest = GetQuest(qID);
				if (quest != null)
				{
					if (quest.State == eQuestState.PROCEEDING)
						prefs.Add(qID);
				}
			}
			prefsObj["proc"] = prefs;
			CacheUserData.SetString("QUEST_PROCEEDING", prefsObj.ToString());
		}

		public List<int> GetQuestMarked()
		{
			//sort
			//Jen 이후 proceedList가 아닌 마크찍은 퀘스트 최대 5개까지만 사용
			var currentMainUIProceedList = GetMainUIQuestProceedList();
			currentMainUIProceedList.Sort((a, b) =>
			{
				var ret = (b.Type == eQuestType.MAIN).CompareTo(a.Type == eQuestType.MAIN);
				if (ret == 0)
				{
					ret = b.IsQuestClear().CompareTo(a.IsQuestClear());
				}
				if(ret == 0)
                {
					ret = b.IsNewQuest().CompareTo(a.IsNewQuest());
					if (ret == 0)
					{
						ret = b.GetTypeWeight().CompareTo(a.GetTypeWeight());
					}
				}

				return ret;
			});

			return currentMainUIProceedList.Select(x => x.ID).ToList();
		}

		public Quest GetQuest(int qID)
		{
			if (!_questList.ContainsKey(qID))
			{
				var baseData = QuestData.Get(qID);
				if (baseData == null)
					return null;
				Quest data = new Quest(baseData);
				if (data == null)
					return null;

				_questList.Add(qID, data);
			}

			return _questList[qID];
		}

	

		public List<Quest> GetProceedQuestDataByType(eQuestType _type, int key = -1)
		{
			return GetProceedQuestListByType(_type, key);
		}

		public List<Quest> GetTotalQuestDataByType(eQuestType _type)
		{
			return GetTotalQuestListByType(_type);
        }

		public List<Quest> GetTotalQuestDataByGroup(eQuestGroup _group)
		{
			return GetTotalQuestListByGroup(_group);
        }


		public List<Quest> GetCompleteQuestDataByType(eQuestType _type, int eventKey = -1)//각 타입별로 완료 퀘스트 인덱스를 따로 관리 하게 되면 분리해야함
		{
			List<Quest> tempQuestList = new List<Quest>();
			if (_completeDic == null || _completeDic.Count <= 0)
				return tempQuestList;

			foreach (KeyValuePair<int, Quest> questPair in _completeDic)
			{
				var key = questPair.Key;//완료 index
				var value = questPair.Value;//quest data

				var type = QuestData.GetQuestType(key);
				if (type == _type)
				{
					if(eventKey > 0)
                    {
						if (value.QuestTableData.EVENT_KEY != eventKey)
							continue;
					}
					tempQuestList.Add(value);					
				}
			}

            return tempQuestList;
		}

		public bool IsCompleteQuest(int _qID)//이미 완료(보상을 받은)퀘스트 인지
		{
			if (_completeDic == null || _completeDic.Count <= 0)
				return false;
			return _completeDic.ContainsKey(_qID);
		}

		void SetDataInCompleteDic(int _qID)//완료 퀘스트 등록
		{
			if (_completeDic == null)
				_completeDic = new Dictionary<int, Quest>();

			var questData = GetQuest(_qID);
			if (questData == null)
			{
				Debug.LogError("퀘스트 데이터 누락 : qID " + _qID);
				return;
			}

			if (_completeDic.ContainsKey(_qID))
				_completeDic[_qID] = questData;
			else
				_completeDic.Add(_qID, questData);

			//ui
			KeyValuePair<eQuestType, eQuestGroup> uikey = new(questData.Type, questData.Group);
			if (!_uiCompleteDic.TryGetValue(uikey, out var uiDic))
			{
				uiDic = new();
				_uiCompleteDic.Add(uikey, uiDic);
				uiDic[_qID] = questData;
			}
            else
            {
				var dic = _uiCompleteDic[uikey];
				if (dic.ContainsKey(_qID))
					dic[_qID] = questData;
				else
					uiDic.Add(_qID, questData);
			}
		}

		void SetDataInProceedDic(eQuestType _questType, int _qID)//없으면 추가
		{
			if (_proceedDic == null)
				_proceedDic = new Dictionary<eQuestType, Dictionary<int, Quest>>();

			var questData = GetQuest(_qID);
			if (questData == null)
			{
				Debug.LogError("퀘스트 데이터 누락");
				return;
			}

			if (!_proceedDic.ContainsKey(_questType))
            {
				_proceedDic[_questType] = new Dictionary<int, Quest>();
			}
			_proceedDic[_questType][_qID] = questData;

			//ui
			KeyValuePair<eQuestType, eQuestGroup> uikey = new(_questType, questData.Group);
			if (!_uiProceedDic.TryGetValue(uikey, out var uiDic))
            {
				uiDic = new();
				_uiProceedDic.Add(uikey, uiDic);
				uiDic[_qID] = questData;
			}
			else
			{
				var dic = _uiProceedDic[uikey];
				if (dic.ContainsKey(_qID))
					dic[_qID] = questData;
				else
					uiDic.Add(_qID, questData);
			}
		}

		void RemoveDataInProceedDic(int _qID)
		{
			var questBase = QuestData.Get(_qID);
			if (questBase == null)
				return;

			if (_proceedDic != null && _proceedDic.ContainsKey(questBase.TYPE))
			{
				if(_proceedDic[questBase.TYPE].ContainsKey(_qID))
					_proceedDic[questBase.TYPE].Remove(_qID);
			}

			KeyValuePair<eQuestType, eQuestGroup> uikey = new(questBase.TYPE, questBase.GROUP);
			if (_uiProceedDic != null && _uiProceedDic.ContainsKey(uikey))
			{
				if (_uiProceedDic[uikey].ContainsKey(_qID))
					_uiProceedDic[uikey].Remove(_qID);
			}
		}
		/// <summary>
		/// networkManager에서 'quest_update' 가 들어올 때, (ex. dailyMission, eventMission)  같은 경우에 이미 완료된 퀘스트를 초기화 해줘야하는 데,
		/// proceeding만 갱신하고 있고, 이전 완료된 상태의 퀘스트는 갱신(삭제처리) 안하고 있어서 같은게 2줄 떴었던 이슈.
		/// </summary>
		/// <param name="_qID"></param>
		void RemoveDataInCompleteDic(int _qID)
		{
			var questBase = QuestData.Get(_qID);
			if (questBase == null)
				return;

			if (_completeDic != null && _completeDic.ContainsKey(_qID))
			{
				if (_completeDic.ContainsKey(_qID))
					_completeDic.Remove(_qID);
			}

			KeyValuePair<eQuestType, eQuestGroup> uikey = new(questBase.TYPE, questBase.GROUP);
			if (_uiCompleteDic != null && _uiCompleteDic.ContainsKey(uikey))
			{
				if (_uiCompleteDic[uikey].ContainsKey(_qID))
					_uiCompleteDic[uikey].Remove(_qID);
			}
		}

		bool IsContainProceedIndex(eQuestType _questType, int _qID)
		{
			if (_proceedDic == null || _proceedDic.Count <= 0 || !_proceedDic.ContainsKey(_questType))
				return false;

			var proceedList = _proceedDic[_questType];
			if (proceedList == null || proceedList.Count <= 0)
				return false;

			return proceedList.ContainsKey(_qID);
		}

		List<Quest> GetProceedQuestListByType(eQuestType _type, int key = -1)
		{
			if (_proceedDic == null || _proceedDic.Count <= 0)
				return new List<Quest>();

			if (_proceedDic.ContainsKey(_type))
			{
				var ret = _proceedDic[_type].Values.ToList();
				
				if(key > 0)
					return ret.FindAll(elem => elem.QuestTableData.EVENT_KEY == key);

				return ret;
			}
			return new List<Quest>();
		}

		List<Quest> GetTotalQuestListByType(eQuestType _type)
		{
            List<Quest> ret =new List<Quest>();

			if(_questList != null)
			{
				var it = _questList.GetEnumerator();
				while (it.MoveNext())
				{
					if (it.Current.Value.Type == _type)
						ret.Add(it.Current.Value);
				}
			}

			return ret;
		}
        List<Quest> GetTotalQuestListByGroup(eQuestGroup group)
        {
            List<Quest> ret = new List<Quest>();

            if (_questList != null)
            {
                var it = _questList.GetEnumerator();
                while (it.MoveNext())
                {
                    if (it.Current.Value.Group == group)
                        ret.Add(it.Current.Value);
                }
            }

            return ret;
        }

        List<Quest> GetMainUIQuestProceedList()//main,sub,chain 만 가져옴 // + 길드 퀘스트는 제외
		{
			var mainProceedList = GetProceedQuestListByType(eQuestType.MAIN);
			var subProceedList = GetProceedQuestListByType(eQuestType.SUB);
			var chainProceedList = GetProceedQuestListByType(eQuestType.CHAIN);

			List<Quest> result = new List<Quest>();
			result.AddRange(mainProceedList);
			result.AddRange(subProceedList);
			result.AddRange(chainProceedList);

			result.RemoveAll(quest => quest.Group == eQuestGroup.Guild);
            return result;
		}

		public List<Quest> GetProceedUIData(eQuestType _type, eQuestGroup _group)
        {
			List<Quest> ret = new List<Quest>();

			if (_uiProceedDic == null || _uiProceedDic.Count <= 0)
				return ret;
			
			KeyValuePair<eQuestType, eQuestGroup> uikey = new(_type, _group);
			if (_uiProceedDic.ContainsKey(uikey))
				return _uiProceedDic[uikey].Values.ToList();
			else
				return ret;
		}
		public List<Quest> GetCompleteUIData(eQuestType _type, eQuestGroup _group)
		{
			List<Quest> ret = new List<Quest>();

			if (_uiCompleteDic == null || _uiCompleteDic.Count <= 0)
				return ret;

			KeyValuePair<eQuestType, eQuestGroup> uikey = new(_type, _group);
			if(_uiCompleteDic.ContainsKey(uikey))
				return _uiCompleteDic[uikey].Values.ToList();
			else
				return ret;
		}


		#region 퀘스트 완료 보상 요청 및 보상 시스템 팝업

		int requestCompleteQuestID = -1;
		List<int> requestCompleteQuestList = null;
		VoidDelegate successResponse = null;
		bool isSuccessRequest = false;

		public void RequestQuestComplete(int _qID, VoidDelegate _successResponse = null, string log = "")
		{
			if (isSuccessRequest)
				return;

			isSuccessRequest = true;
			requestCompleteQuestID = _qID;

			LoginManager.Instance.SetFirebaseEvent("quest_clear", "quest_no", requestCompleteQuestID);

			successResponse = _successResponse;
			WWWForm param = new WWWForm();
			param.AddField("quest", _qID);
			param.AddField("ad_log", log);
			if (_qID == 1100007)
            {
				AppsFlyerSDK.AppsFlyer.sendEvent("funnel_first", new Dictionary<string, string>());
			}
			if (_qID == 1100016)
			{
				AppsFlyerSDK.AppsFlyer.sendEvent("funnel_second", new Dictionary<string, string>());
			}
			if (_qID == 1100026)
			{
				AppsFlyerSDK.AppsFlyer.sendEvent("funnel_third", new Dictionary<string, string>());
			}

			NetworkManager.Send("quest/accomplish", param, OnResponseSuccessReward, (jsonData) => {
				isSuccessRequest = false;
				requestCompleteQuestID = -1;
				successResponse = null;
			});
		}
		public void RequestQuestComplete(List<int> _qIDList, VoidDelegate _successResponse = null)//다중 보상 기능
		{
			if (isSuccessRequest)
				return;

			isSuccessRequest = true;
			requestCompleteQuestList = _qIDList.ToList();
			successResponse = _successResponse;
			WWWForm param = new WWWForm();
			param.AddField("quest", JsonConvert.SerializeObject(_qIDList.ToArray()));
			NetworkManager.Send("quest/accomplishall", param, OnResponseSuccessALLReward,(jsonData) => {
				isSuccessRequest = false;
				requestCompleteQuestList = null;
				successResponse = null;
			});
		}
		/*
		 * {"err":0,"ts":1683538770,"acc_exp":520,"gold":1000,"energy":16,"items":[[3,40000002,16]],//
		 */
		void OnResponseSuccessReward(JObject JsonData)
		{
			List<Asset> rewards = GetRewards(JsonData);//서버 기반 데이터로 변경 //quest.GetReward();
			if (rewards.Count > 0)
			{
				var tempQuestID = requestCompleteQuestID;
				SystemRewardPopup.OpenPopup(rewards, () => {
					if (tempQuestID > 0)
                    {
						var questData = QuestManager.instance.GetQuest(tempQuestID);
						if(questData != null)
                        {
							if(questData.Type != eQuestType.DAILY)
								QuestEvent.Event(QuestEvent.eEvent.QUEST_DONE, tempQuestID);
							else
								QuestEvent.Event(QuestEvent.eEvent.QUEST_REQUEST_REFRESH);
						}
					}
				});

				QuestManager.Instance.SetQuestComplete(requestCompleteQuestID);
			}
			requestCompleteQuestID = -1;

			if (successResponse != null)
			{
				successResponse();
				successResponse = null;
			}
			isSuccessRequest = false;
		}
		void OnResponseSuccessALLReward(JObject JsonData)
		{
			List<Asset> tempTotalList = new List<Asset>();
			List<int> questCompleteList = new List<int>();

			if (JsonData.ContainsKey("reward_list"))
			{
				JArray rewardInfoList = (JArray)JsonData["reward_list"];
				foreach (JToken token in rewardInfoList)
				{
					JObject datas = (JObject)token;
					questCompleteList.Add(datas["qid"].Value<int>());
					var rewardList = GetRewards((JObject)datas["reward"]);//보상 아이템 가져오기
					tempTotalList.AddRange(rewardList);
				}
			}

			if (tempTotalList.Count > 0)
			{
				var tempRewardDic = new Dictionary<int, Asset>();//리스트 보상 데이터 합치기
				foreach (var rewardData in tempTotalList)
				{
					var itemNo = rewardData.ItemNo;
					var amount = rewardData.Amount;
					var type = rewardData.GoodType;

					if (!tempRewardDic.ContainsKey(rewardData.ItemNo))
						tempRewardDic.Add(itemNo, rewardData);
					else
					{
						var currentReward = tempRewardDic[itemNo];
						var currentAmount = currentReward.Amount;
						tempRewardDic[itemNo] = new Asset(type, itemNo, currentAmount + amount);
					}
				}

				tempTotalList = tempRewardDic.Values.ToList();
				tempTotalList.OrderBy(x => x.ItemNo == 10000003)//기존 리스트 세팅방식으로 정렬
					.ThenBy(x => x.ItemNo == 10000001)
					.ThenBy(x => x.ItemNo == 10000002)
					.ThenBy(x => x.ItemNo == 10000007);

				SystemRewardPopup.OpenPopup(tempTotalList, ()=> {

					if(questCompleteList != null && questCompleteList.Count > 0)
                    {
						var tempQuestID = questCompleteList[0];
						if (tempQuestID > 0)
						{
							var questData = QuestManager.instance.GetQuest(tempQuestID);
							if (questData != null)
							{
								if (questData.Type != eQuestType.DAILY)
									QuestEvent.Event(QuestEvent.eEvent.QUEST_UPDATE);
								else
									QuestEvent.Event(QuestEvent.eEvent.QUEST_REQUEST_REFRESH);
							}
						}
					}
				});
				QuestManager.Instance.SetQuestComplete(questCompleteList);
			}

			requestCompleteQuestList.Sort();
			questCompleteList.Sort();
			var arrayCheck = IsArrayEqual(requestCompleteQuestList, questCompleteList);
#if DEBUG || UNITY_EDITOR
			if (!arrayCheck)
			{
				Debug.Log("완료 요청한 퀘스트 인덱스 리스트가 서로 다름!  클라 : " + string.Format("[{0}]" , string.Join(", ", requestCompleteQuestList)) + "   서버 : " + string.Format("[{0}]", string.Join(",", questCompleteList)) );
			}
#endif
			requestCompleteQuestList = null;
			if (successResponse != null)
			{
				successResponse();
				successResponse = null;
			}
			isSuccessRequest = false;
		}
		bool IsArrayEqual(List<int> list1, List<int> list2)
		{
			if (list1.Count != list2.Count)
				return false;

			var areListsEqual = true;
			for (var i = 0; i < list1.Count; i++)
			{
				if (list2[i] != list1[i])
				{
					areListsEqual = false;
				}
			}

			return areListsEqual;
		}

		List<Asset> GetRewards(JObject JsonData)
		{
			List<Asset> rewards = new List<Asset>();

			if (JsonData.ContainsKey("acc_exp"))
			{
				var accValue = JsonData["acc_exp"].Value<int>();
				if (accValue > 0)
					rewards.Add(new Asset(eGoodType.NONE, 10000003, accValue));
			}
			if (JsonData.ContainsKey("gold"))
			{
				var goldValue = JsonData["gold"].Value<int>();
				if (goldValue > 0)
					rewards.Add(new Asset(eGoodType.GOLD, 10000001, goldValue));
			}
			if (JsonData.ContainsKey("energy"))
			{
				var energyValue = JsonData["energy"].Value<int>();
				if (energyValue > 0)
					rewards.Add(new Asset(eGoodType.ENERGY, 10000002, energyValue));
			}
			if (JsonData.ContainsKey("pvp_ticket"))
			{
				var arenaValue = JsonData["pvp_ticket"].Value<int>();
				if (arenaValue > 0)
					rewards.Add(new Asset(eGoodType.ARENA_TICKET, 10000007, arenaValue));
			}

			List<Asset> itemRewardsList = null;
			if (JsonData.ContainsKey("items"))
				itemRewardsList = SBFunc.ConvertSystemRewardDataList(JsonData["items"].ToObject<JArray>());

			if (itemRewardsList != null && itemRewardsList.Count > 0)
				rewards.AddRange(itemRewardsList);

			return rewards;
		}

		#endregion

		#region 바로가기 기능
		/** <summary>임시 코드, 변경 예정</summary> */
		public void DirectGotoTarget(QuestTriggerData _triggerData)
		{
			if (_triggerData == null)
			{
				Debug.Log(_triggerData);
				return;
			}

			string triggerType = _triggerData.TYPE;
			string typeKey = _triggerData.TYPE_KEY;
			string subType = _triggerData.SUB_TYPE;

			eQuestCompleteCondType completeState = (eQuestCompleteCondType)QuestCondition.strCondition.IndexOf(triggerType);

			switch (completeState)
			{
				case eQuestCompleteCondType.GACHA_PET:
				case eQuestCompleteCondType.GACHA_DRAGON:
				case eQuestCompleteCondType.CHECK_DRAGON:
				case eQuestCompleteCondType.CHECK_PET:
                case eQuestCompleteCondType.GACHA_LUCKY:
				case eQuestCompleteCondType.CHECK_TARGET_DRAGON:
				{
					switch(completeState)
                    {
						case eQuestCompleteCondType.GACHA_PET:
						case eQuestCompleteCondType.CHECK_PET:
							SBFunc.MoveGachaScene(eGachaGroupMenu.PET_PREMIUM);
							break;
						case eQuestCompleteCondType.CHECK_DRAGON:
						case eQuestCompleteCondType.GACHA_DRAGON:
							SBFunc.MoveGachaScene(eGachaGroupMenu.DRAGON_PREMIUM);
							break;
						case eQuestCompleteCondType.CHECK_TARGET_DRAGON:
							SBFunc.MoveGachaScene(eGachaGroupMenu.PICKUP_DRAGON);
							break;
						case eQuestCompleteCondType.GACHA_LUCKY:
							SBFunc.MoveGachaScene(eGachaGroupMenu.LUCKYBOX);
							break;
					}

					CheckQuestMove = true;
				}
				break;
				case eQuestCompleteCondType.BUILD://랜드마크도 같이 들어오기 때문에 분기 처리해야함.
				{
					SBFunc.MoveScene(() => SBFunc.RequestBuildingPopup(subType));
				}
				break;
				case eQuestCompleteCondType.EQUIPMENT_PET:
				case eQuestCompleteCondType.EQUIPMENT_EQUIP:
				case eQuestCompleteCondType.LEVEL_DRAGON:
				case eQuestCompleteCondType.LEVEL_DRAGON_SKILL:
				case eQuestCompleteCondType.MERGE_DRAGON:
				case eQuestCompleteCondType.LEVEL_TARGET_DRAGON:
				case eQuestCompleteCondType.LEVEL_TARGET_DRAGON_SKILL:
				case eQuestCompleteCondType.CHECK_TRANSCENDENCE_TARGET_DRAGON:
				{
					int subidx = -1;
					switch (completeState)
					{
						case eQuestCompleteCondType.MERGE_DRAGON:
							subidx = 6;
							break;
						case eQuestCompleteCondType.LEVEL_TARGET_DRAGON:
						case eQuestCompleteCondType.LEVEL_TARGET_DRAGON_SKILL:
						case eQuestCompleteCondType.CHECK_TRANSCENDENCE_TARGET_DRAGON:
							if (!User.Instance.DragonData.IsContainsDragon(int.Parse(typeKey)))
                            {
								SBFunc.MoveGachaScene(eGachaGroupMenu.PICKUP_DRAGON);
								return;
							}
							break;
					}
					SBFunc.MoveScene(() => { DragonManagePopup.OpenPopup(0, subidx); });
				}
				break;
				case eQuestCompleteCondType.EQUIP_DECOM:
				case eQuestCompleteCondType.ENHANCE_EQUIP:
				case eQuestCompleteCondType.MERGE_EQUIP://장비 탭 따로 없어져서 일단 드래곤 리스트로 보냄(바로가기 논의 해봐야함)
				{
					//int subidx = -1;
					//switch (completeState)
					//{
					//	case eQuestCompleteCondType.EQUIP_DECOM:
					//		subidx = 3;
					//		break;
					//	case eQuestCompleteCondType.ENHANCE_EQUIP:
					//		subidx = 1;
					//		break;
					//	case eQuestCompleteCondType.MERGE_EQUIP:
					//		subidx = 2;
					//		break;
					//}
					SBFunc.MoveScene(() => { DragonManagePopup.OpenPopup(0, -1); });
				}
				break;
				case eQuestCompleteCondType.LEVEL_PET:
				case eQuestCompleteCondType.ENHANCE_PET:
				case eQuestCompleteCondType.MERGE_PET://펫관련 전부 드래곤 리스트로 보냄
				{
					//int subidx = 1;//정보창 띄우기 위해서 기본 1로 보냄(펫 리스트 삭제)
					//switch (completeState)
					//{
					//	case eQuestCompleteCondType.MERGE_PET:
					//		subidx = 4;
					//		break;
					//}
					SBFunc.MoveScene(() => { DragonManagePopup.OpenPopup(0, -1); });
				}
				break;
				case eQuestCompleteCondType.ADD_FLOOR:
				case eQuestCompleteCondType.LEVEL_TOWN:
				{
					SBFunc.MoveScene(() => {
						var townManagePopup = PopupManager.OpenPopup<TownManagePopup>();
						if (townManagePopup != null && completeState == eQuestCompleteCondType.ADD_FLOOR)
						{
							townManagePopup.OnClickFloorExtensionButton();
						}
					});
				}
				break;
				case eQuestCompleteCondType.GAIN_DOZER:
				{
					SBFunc.MoveScene(() => SBFunc.RequestBuildingPopup(SBFunc.LANDMARK_DOZER_SUBTYPE_KEY));
				}
				break;
				case eQuestCompleteCondType.STAGE_COMPLETE:
				case eQuestCompleteCondType.STAGE_CLEAR:
				{
					if (!IsAvailableGotoBattle())
					{
						ToastManager.On(100000623);
						return;
					}

					if (subType.CompareTo(QuestManager.QuestSubTypeParam) == 0)//아무곳이나 스테이지 클리어
					{
						if (PopupManager.IsPopupOpening() && PopupManager.OpenPopupCount == 1)
							PopupManager.GetFirstPopup().ClosePopup();

						PopupManager.OpenPopup<DungeonSelectPopup>();//배틀 선택 팝업 띄우기
					}
					else
					{
						StageBaseData stageData = StageBaseData.Get(typeKey);

						StageManager.Instance.Quest_World = stageData.WORLD;
						StageManager.Instance.Quest_Stage = stageData.STAGE;
						StageManager.Instance.Quest_Diff = (int)stageData.DIFFICULT;

						CheckQuestMove = true;

						string adventureSceneName = SBFunc.ADVENTURE_SCENE_NAME;
						bool isAdventureScene = SBFunc.IsTargetScene(adventureSceneName);
						if (!isAdventureScene)
						{
							SBFunc.MoveScene(null, SBFunc.ADVENTURE_SCENE_NAME);
						}
					}
				}
				break;
				case eQuestCompleteCondType.GAIN:
				case eQuestCompleteCondType.PRODUCE:
				{
					if (subType.CompareTo(QuestManager.QuestSubTypeParam) == 0)
					{
						var hasNormalBuilding = User.Instance.GetAllProducesList(true).Count > 0;
						if (hasNormalBuilding)
							PopupManager.OpenPopup<ProductManagePopup>();//생산 팝업 열기
						else
							ToastManager.On(StringData.GetStringByStrKey("생산관리토스트메시지"));
						return;
					}
					int materialKey = int.Parse(typeKey);//획득 해야되는 재료 키값
					string buildingIndex = ProductData.GetBuildingGroupByProductItem(materialKey);
					if (buildingIndex == "" && materialKey > 40000000 && materialKey < 50000000)
					{
						buildingIndex = SBFunc.BATTERY_TYPE_ITEM_KEY;
					}
					SBFunc.MoveScene(() => {
						SBFunc.RequestBuildingPopup(buildingIndex, materialKey); 
					});
				}
				break;
				case eQuestCompleteCondType.GAIN_BATTERY://건전지 생산
                {
					SBFunc.MoveScene(() => {});//팝업 전부 닫기
				}
				break;
				case eQuestCompleteCondType.ARENA://arena 씬 내부에서도 퀘스트 노티가 맞는지 확인해봐야함 + 버튼 연속 입력 막기
				case eQuestCompleteCondType.ARENA_WIN_AD:
				{
					if (!IsAvailableGotoBattle())
					{
						ToastManager.On(100000623);
						return;
					}

					ArenaManager.Instance.ReqArenaData(() =>
					{
						SBFunc.MoveScene(() => { }, SBFunc.ARENA_LOBBY_SCENE_NAME);
					}, () =>
					{
						ToastManager.On(100002516);
					});
				}
				break;
				case eQuestCompleteCondType.TRAVEL:
				{
					SBFunc.MoveScene(() => SBFunc.RequestBuildingPopup(SBFunc.LANDMARK_TRAVEL_SUBTYPE_KEY));
				}
				break;
				case eQuestCompleteCondType.DELIVERY:
				{
					SBFunc.MoveScene(() => SBFunc.RequestBuildingPopup(SBFunc.LANDMARK_SUBWAY_SUBTYPE_KEY));
				}
				break;
				case eQuestCompleteCondType.REQUEST://소원나무 의뢰완료하기 - 의뢰소 팝업 오픈
				{
					SBFunc.MoveScene(() => { SBFunc.RequestBuildingPopup(SBFunc.LANDMARK_REQUEST_SUBTYPE_KEY); });
				}
				break;
				case eQuestCompleteCondType.DAY_DUNGEON://요일 던전 진입 시키기
				case eQuestCompleteCondType.DAY_STAGE://요일 던전 진입 시키기
				{
					if (!IsAvailableGotoBattle())
					{
						ToastManager.On(100000623);
						return;
					}

					SBFunc.MoveScene(null, SBFunc.DAILY_DUNGEON_SCENE_NAME);
				}
				break;
                case eQuestCompleteCondType.DAILY_ALL_CLEAR_AD: // 일일 미션 모두 완료하기
				{
                    PopupManager.OpenPopup<MissionPopup>(new TabTypePopupData(0, 0)); // 미션팝업 열기
                }
                break;
                case eQuestCompleteCondType.CLEAR_TYPE:
				{
					if (subType == "DAILY")
					{
                        PopupManager.OpenPopup<MissionPopup>(new TabTypePopupData(0, 0)); // 미션팝업 열기
                    }
				}
				break;
				case eQuestCompleteCondType.BOSS_RAID:
				{
					if (WorldBossManager.Instance.IsAvailEnterCondition())
						SBFunc.MoveWorldBossScene();
					else
						ToastManager.On(StringData.GetStringByStrKey("boss_raid_open"));
				}
				break;
				case eQuestCompleteCondType.GAIN_GEMDUNGEON:
				{
					var gemDungeon = LandmarkGemDungeon.Get();

                    if (gemDungeon ==null)
					{
						ToastManager.On(StringData.GetStringByStrKey("guild_desc:121"));
                    }
					else
					{
						switch (gemDungeon.State)
						{
							case eBuildingState.NORMAL:
                                PopupManager.OpenPopup<GemDungeonPopup>();
                                break;
							default:
                                ToastManager.On(StringData.GetStringByStrKey("guild_desc:121"));
                                break;
                        }	
                    }
				}
                break;
                default:
					//WJ -테이블 데이터 세팅 작업 완료 되면 지울 것(바로가기 타입 확인 용도)
					//ToastManager.On("비정상적인 컴플릿 타입 : " + completeState + " 키 : "+ _triggerData.KEY+ "트리거 타입 : " + triggerType);
					break;
			}
		}

		bool IsAvailableGotoBattle()//탐험,아레나,요던 입장 체크 (드래곤 보유 갯수)
		{
			return User.Instance.DragonData.GetAllUserDragons().Count > 0;
		}

		public bool IsAvailableDirect(QuestTriggerData _triggerData)//바로 가기 기능이 가능한지에 대한 정의
		{
			string triggerType = _triggerData.TYPE;
			eQuestCompleteCondType completeState = (eQuestCompleteCondType)QuestCondition.strCondition.IndexOf(triggerType);
			switch (completeState)
			{
				case eQuestCompleteCondType.CLEAR_TYPE:
					if(_triggerData.SUB_TYPE== "DAILY") 
						return true;
					return false;;
                case eQuestCompleteCondType.CLEAR_QUEST:
                case eQuestCompleteCondType.CONSUME_ENERGY:
				case eQuestCompleteCondType.CONSUME_GOLD:
				case eQuestCompleteCondType.NONE:
					return false;
				case eQuestCompleteCondType.GAIN_GEMDUNGEON:
                    var gemDungeon = LandmarkGemDungeon.Get();
					if(gemDungeon == null || gemDungeon.State != eBuildingState.NORMAL)
						return false;
					return true;

                default:
					return true;
			}
		}

		int GetCurrentTagBySubKey(string _subType)
		{
			var installTagTableDataList = BuildingOpenData.GetTagList(_subType);//데이터 테이블에서 키값으로 리스트 가져오기
			if (installTagTableDataList == null || installTagTableDataList.Count <= 0)
				return 0;

			return installTagTableDataList[0];
		}

		#endregion

		void RequestQuestSyncPopup(VoidDelegate _clickYesCallback)
        {
			SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002675),
							() => {
								RequestSyncronizing(_clickYesCallback);
							},
							() => {
								//나가기
							},
							() => {
								//나가기
							}
						);
		}
		/// <summary>
		/// 퀘스트의 각 타입별로 시간 체크를 통해 과거에 발급된 퀘스트인지 체크하고 맞으면 sync 요청하고 콜백 / 과거 아니면 패스 콜백
		/// </summary>
		/// <param name="_quest"></param>
		/// <param name="_passCallback"></param>
		/// <param name="_clickYesCallback"></param>
		public void RequestAcceptableRewardQuest(Quest _quest, VoidDelegate _passCallback, VoidDelegate _clickYesCallback)
		{
			var isPast = IsPastQuestCheckByType(_quest);
			if (isPast)
				RequestQuestSyncPopup(_clickYesCallback);
			else
			{
				if (_passCallback != null)
					_passCallback();
			}
		}

		bool IsPastQuestCheckByType(Quest _quest)
        {
			if (_quest == null)
				return true;//없으면 다시 싱크 요청

			var questType = _quest.Type;
			switch(questType)
            {
				case eQuestType.DAILY:
					return IsPastDailyQuest(_quest);
				case eQuestType.WEEKLY:
				case eQuestType.CHAIN:
					return IsPastWeeklyQuest(_quest);
				case eQuestType.EVENT:
					if (_quest.ID >= 5300000 && _quest.ID < 5400000)
						return IsPastDailyEventQuest(_quest);
					else
						return IsPastEventQuest(_quest);
			}
			return true;
        }

		public void RequestDailyEventAcceptableRewardQuest(List<int> _questIndexList, VoidDelegate _passCallback, VoidDelegate _clickYesCallback)
		{
			if (_questIndexList == null || _questIndexList.Count <= 0)
				return;

			List<Quest> requestQuestList = new List<Quest>();
			foreach(var questID in _questIndexList)
            {
				if (questID <= 0)
					continue;

				requestQuestList.Add(QuestManager.instance.GetQuest(questID));
            }

			int pastCount = 0;

			foreach (var quest in requestQuestList)
				if (IsPastDailyEventQuest(quest))
					pastCount++;

			var isPast = pastCount > 0;
			if (isPast)
			{
				SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002675),
							() => {
								RequestSyncronizing(_clickYesCallback);
							},
							() => {
								//나가기
							},
							() => {
								//나가기
							}
						);
			}
			else
			{
				if (_passCallback != null)
					_passCallback();
			}
		}

		bool IsPastDailyQuest(Quest _quest)//과거에 받은 일일퀘스트 인가
		{
			if (_quest == null)
				return true;

			var currentGoalDate = TimeManager.GetContentClearTime(true);//한국시간(utc+9) 미처리 값
			var timeStampKor = (int)(TimeManager.GetCustomDateTime(0) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;//한국 시간 timeStamp
			currentGoalDate -= timeStampKor;
			
			var questAcceptTime = _quest.TimeStamp;//퀘스트 수령시각 - 서버에서 주는 퀘스트 발생시각
			return questAcceptTime < currentGoalDate;
		}
		bool IsPastDailyEventQuest(Quest _quest)//과거에 받은 일일'이벤트'퀘스트 인가
		{
			if (_quest == null)
				return true;

			if (_quest.ID >= 5300000 && _quest.ID < 5400000)
			{
				var currentGoalDate = TimeManager.GetEventContentClearTime(true);//한국시간(utc+9) 미처리 값
				var timeStampKor = (int)(TimeManager.GetCustomDateTime(0) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;//한국 시간 timeStamp
				currentGoalDate -= timeStampKor;

				var questAcceptTime = _quest.TimeStamp;//퀘스트 수령시각 - 서버에서 주는 퀘스트 발생시각
				return questAcceptTime < currentGoalDate;
			}

			return false;
		}
		bool IsPastEventQuest(Quest _quest)
        {
			if (_quest == null)
				return true;

			//이벤트 기간인지 체크하는 코드가 필요할듯한데..

			return false;
		}
		/// <summary>
		/// 이번주와 다음주 월요일의 사이 타임스탬프 값이냐를 비교함. 사이값이 아니면 과거라 판단함.
		/// </summary>
		/// <param name="_checkDay"></param>
		/// <returns></returns>
		public bool IsPastWeeklyQuest(Quest _quest)
        {
			var obtainTime = _quest.TimeStamp;//수령시각

			DateTime nextMonday = TimeManager.GetSpecificNextDay();
			DateTime todayMonday = nextMonday.AddDays(-7);//이번주 월

			int nextMondayTimeStamp = TimeManager.GetTimeStamp(nextMonday);
			int todayMondayTimeStamp = TimeManager.GetTimeStamp(todayMonday);

			var isContainCondition = todayMondayTimeStamp <= obtainTime && nextMondayTimeStamp > obtainTime;//true 면 정상 발급 퀘
			return !isContainCondition;
		}

		public void RequestSyncronizing(VoidDelegate _successResponse = null)
		{
			NetworkManager.Send("quest/sync", null, (jsonData)=> {
				if (_successResponse != null)
					_successResponse();
			});
		}

		public void Update(float dt) { } //not use
	}
}
