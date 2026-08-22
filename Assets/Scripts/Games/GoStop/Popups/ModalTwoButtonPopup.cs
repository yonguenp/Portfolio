using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 딤 + Kenney 헤더바 패널 + 메시지/서브텍스트 + 버튼 2개로 구성된 공용 팝업.
/// 흔들기 확인·9월 열끗 선택·(4인) 참가 선언이 전부 이 모양이라 프리팹
/// 하나의 컴포넌트를 공유한다 — 제목·색·서브텍스트처럼 안 바뀌는 내용은
/// 프리팹 자체에 구워두고(에디터에서 열어 바로 확인·수정 가능), 클릭
/// 동작처럼 매번 달라지는 것만 게임 스크립트가 인스턴스화 직후 새로 연결한다.
///
/// 프리팹은 씬의 특정 스크립트 인스턴스를 직렬화 참조할 수 없다(이 프로젝트
/// 기존 제약 — GameUIManager의 런타임 등록 패턴과 같은 이유)는 원칙을 그대로
/// 따른다: <see cref="SetPrimary"/>/<see cref="SetSecondary"/>가 그 런타임
/// 등록 지점이다.
/// </summary>
public class ModalTwoButtonPopup : MonoBehaviour
{
    public RectTransform dim;
    public TextMeshProUGUI messageText;   // 동적 문구(예: "5월 흔들기 선언하시겠습니까?") — 없으면 null
    public Button primaryButton;
    public Button secondaryButton;

    public void Show() => dim.gameObject.SetActive(true);
    public void Hide() => dim.gameObject.SetActive(false);

    public void SetPrimary(UnityEngine.Events.UnityAction onClick)
    {
        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(onClick);
    }

    public void SetSecondary(UnityEngine.Events.UnityAction onClick)
    {
        secondaryButton.onClick.RemoveAllListeners();
        secondaryButton.onClick.AddListener(onClick);
    }
}
