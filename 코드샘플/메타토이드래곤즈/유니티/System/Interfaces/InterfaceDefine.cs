using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public interface IManagerBase
    {
        void Initialize();
        void Update(float dt);
    }
    public interface ITimeObject
    {
        void Init(); //해당 오브젝트를 TimeManager에 등록하는 과정.
        void Clear(); //해당 오브젝트를 TimeManager에서 제거하는 과정.
    }
    public interface IPopup
    {
        int GetOrder();
        void Init(PopupData data);
        void ForceUpdate(PopupData data = null);
        void ClosePopup();
        void Close();
        void SetActive(bool v);
        void BackButton();
        void OnClickDimd();

    }
    public interface IUIBase
    {
        void Init();//초기화 용도
        void InitUI(eUIType targetType);//현재 씬의 타입에 따라 UI변동.
        void RefreshUI();//정보 변경에 따른 갱신 
        void ShowEvent();//Visible 관련
        void HideEvent();//Visible 관련
        void ReuseAnim();//사용 연출
        void UnuseAnim();//미사용 연출
    }
    public interface IBattleEventObject
    {
        bool IsEnd { get; }
        void Update(float dt);
        void Translate(Vector3 pos);
    }
    public interface ISingleton
    {
        void Initialize();
    }
}
