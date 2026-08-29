using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 우측 하단 채팅/이벤트 로그 창의 프리팹 뷰 — 2026-08-28,
/// <c>Assets/Resources/Prefabs/GoStop/UI/ChatPanel.prefab</c>로 뺐다.
/// 이 컴포넌트는 정적 틀(배경·헤더·스크롤 로그·입력 행)의 참조만 노출한다 —
/// 실제 로그 누적·네트워크 송수신 로직(무엇을 언제 적을지, 누구에게
/// 릴레이할지)은 전부 <c>GoStop3PGame.Chat.cs</c>에 그대로 남아있다
/// (GoStopStatusBoxView/DealerDrawPopupView와 같은 "정적 틀=프리팹,
/// 로직=게임 스크립트" 경계).
/// </summary>
public class GoStopChatView : MonoBehaviour
{
    public RectTransform panelRT;
    public TextMeshProUGUI logText;
    public RectTransform logContent;
    public ScrollRect logScroll;
    public GameObject inputRow;
    public TMP_InputField inputField;
    public Button sendButton;

    // 2026-08-28 탭(전체/채팅/로그) — 인덱스 0=전체, 1=채팅, 2=로그로 통일해서
    // 게임 스크립트가 배열을 순회하며 켜고 끌 수 있게 했다(필드 9개 대신
    // 배열 3개).
    public Button[] tabButtons;
    public Image[] tabImages;
    public TextMeshProUGUI[] tabLabels;
}
