using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀에서 고스톱 카드를 누르면 뜨는 인원수 선택 팝업 — 2인(맞고)/3인/
/// 4인(전부 고스톱) 중 골라도, 네트워크 대전을 골라도 결국 전부
/// <c>GoStop3PScene</c>(2~4인을 전부 처리하는 <see cref="GoStop3PGame"/>)
/// 하나로 들어간다.
///
/// 2026-08-28: 5개 버튼이 전부 정적 구조/문구라(런타임에 값을 바꿔 넣을 게
/// 없다) <c>Assets/Resources/Prefabs/GoStop/UI/GoStopModeChoiceUI.prefab</c>
/// 으로 전환했다 — StatusBoxView/OverlayCard와 같은 "코드 생성 → 프리팹
/// 인스턴스 재사용" 패턴. 버튼 5개(패널 바깥 탭 닫기·닫기 버튼·2/3/4인·
/// 네트워크) 전부 프리팹 저장 시점에 <c>AddVoidPersistentListener</c>로 이
/// 컴포넌트의 메서드에 영구 연결해뒀다 — 런타임은 <see cref="netLobby"/>
/// 참조 하나만 이어주면 된다.
/// </summary>
public class GoStopModeChoiceUI : MonoBehaviour
{
    [SerializeField] RectTransform panelRT;
    GoStopNetLobbyUI netLobby;

    public static GoStopModeChoiceUI Create(RectTransform canvasRT)
    {
        var ui = HwatuUI.InstantiateUIPrefab<GoStopModeChoiceUI>("GoStopModeChoiceUI", canvasRT);
        ui.netLobby = GoStopNetLobbyUI.Create(canvasRT);
        return ui;
    }

    public void Open()  { panelRT.gameObject.SetActive(true); panelRT.SetAsLastSibling(); }
    public void Close() { panelRT.gameObject.SetActive(false); }

    // 아래 4개 + Close()는 프리팹의 각 버튼 onClick에 persistent listener로
    // 이미 연결돼 있다(베이킹 시점) — GoStop3PGame.PendingOfflineSeatCount를
    // 미리 세팅해두면 GoStop3PScene의 Awake()가 읽어서 좌석 수를 맞춘다
    // (네트워크 로비가 없는 오프라인 경로라 static 값으로 전달한다).
    public void OnTwoPlayer()   { GoStop3PGame.PendingOfflineSeatCount = 2; SceneManager.LoadScene("GoStop3PScene"); }
    public void OnThreePlayer() { GoStop3PGame.PendingOfflineSeatCount = 3; SceneManager.LoadScene("GoStop3PScene"); }
    public void OnFourPlayer()  { GoStop3PGame.PendingOfflineSeatCount = 4; SceneManager.LoadScene("GoStop3PScene"); }

    // 2026-08-19: 로컬 네트워크(같은 와이파이) 대전 — IP 입력 없이 자동으로
    // 방을 찾는다. 이 팝업은 닫고 로비 팝업(GoStopNetLobbyUI)을 연다 — 실제
    // 씬 전환은 그 팝업이 호스트가 "시작"을 누른 뒤 GoStopNetLobby.
    // OnGameStarting을 받아서 한다.
    public void OnNetwork() { Close(); netLobby?.Open(); }
}
