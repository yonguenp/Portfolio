using UnityEngine;

/// <summary>
/// 뻑/뻑먹기/쪽/따닥/쓸/스톱 센터스크린 아이콘.
///
/// 2026-09-05에 처음 만들 때는 절차적(SDF) 코드로 직접 그렸는데, 사용자가
/// "웹에서 SVG를 찾아 쓰라고 했지 만들라고 하지 않았다"고 명확히 지적해서
/// 2026-09-06에 실제 SVG 참조 방식으로 교체했다 — 처음엔 Twitter의 오픈소스
/// 이모지 세트 Twemoji(CC BY 4.0)를 썼는데, "게임 분위기와 안 어울린다"는
/// 재지적을 받아 같은 날 OpenMoji(CC BY-SA 4.0, hfg-gmuend/openmoji 저장소
/// color 세트)로 다시 교체했다 — Twemoji는 둥글고 그라데이션이 있는
/// "채팅 이모지" 느낌이라, 굵은 검정 외곽선+평면 채색인 이 프로젝트의
/// Kenney UI·절차적 아이콘(GoStopIcons 등) 톤과 더 잘 어울린다고 판단했다.
/// 원본은 <c>Assets/Art/OpenMoji/</c>에 코드포인트 파일명 그대로(예:
/// <c>1F4A9.svg</c>) 보관하고, 실제 게임이 쓰는 6장만
/// <c>Assets/Resources/ComboIcons_SVG/</c>에 역할 이름으로 복사했다(Kenney·
/// 화투 SVG 때와 같은 "원본은 Art, 실제 쓰는 것만 Resources" 원칙).
///
/// | 역할 | 이모지 | 코드포인트 |
/// |---|---|---|
/// | 뻑 | 💩 | 1F4A9 |
/// | 뻑먹기/자뻑 | 🧻 | 1F9FB |
/// | 쪽 | 💋 | 1F48B |
/// | 따닥 | 🫰 | 1FAF0 |
/// | 쓸 | 🧹 | 1F9F9 |
/// | 스톱 | ✋ | 270B |
///
/// OpenMoji 그래픽은 CC BY-SA 4.0 — 출처 표시 + 수정본도 동일 라이선스
/// 유지 의무가 있다(화투 카드 CC BY-SA와 완전히 같은 종류, 타이틀
/// 설정→라이선스 정보 화면에 이미 같이 적어뒀다). 앱 전체를 오픈소스로
/// 풀 필요는 없고 이 이미지 자산에만 적용된다.
///
/// 각 SVG는 Unity 내장 Vector Graphics 임포터로 임포트하되
/// **svgType=TexturedSprite(값 1)이어야 한다** — VectorSprite(값 0)로
/// 임포트하면 `sprite.texture`가 null인 순수 메시 스프라이트가 나와서
/// UGUI `Image`가 그냥 안 그린다(2026-09-06 최초 도입 때 실제로 겪은
/// 버그, 텍스처 기반의 사각 메시로 감싸는 TexturedSprite여야 일반 PNG
/// 스프라이트처럼 렌더된다 — 벡터 확대 이점은 포기하지만 이 아이콘들은
/// 화면에서 최대 300px 정도로만 쓰여 체감 열화가 없다). `Resources.Load`는
/// 최초 1회만 하고 캐싱해서 재사용한다.
/// </summary>
public static class GoStopComboIcons
{
    const string ResPrefix = "ComboIcons_SVG/";

    static Sprite poopSprite, tissueSprite, lipsSprite, snapSprite, broomSprite, stopHandSprite;

    public static Sprite Poop     => poopSprite     != null ? poopSprite     : (poopSprite     = Resources.Load<Sprite>(ResPrefix + "poop"));
    public static Sprite Tissue   => tissueSprite   != null ? tissueSprite   : (tissueSprite   = Resources.Load<Sprite>(ResPrefix + "tissue"));
    public static Sprite Lips     => lipsSprite     != null ? lipsSprite     : (lipsSprite     = Resources.Load<Sprite>(ResPrefix + "lips"));
    public static Sprite Snap     => snapSprite     != null ? snapSprite     : (snapSprite     = Resources.Load<Sprite>(ResPrefix + "snap"));
    public static Sprite Broom    => broomSprite    != null ? broomSprite    : (broomSprite    = Resources.Load<Sprite>(ResPrefix + "broom"));
    public static Sprite StopHand => stopHandSprite != null ? stopHandSprite : (stopHandSprite = Resources.Load<Sprite>(ResPrefix + "stophand"));
}
