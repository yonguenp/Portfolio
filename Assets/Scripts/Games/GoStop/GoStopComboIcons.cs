using UnityEngine;

/// <summary>
/// 뻑/뻑먹기/쪽/따닥/쓸/스톱 센터스크린 아이콘.
///
/// 2026-09-05에 처음 만들 때는 절차적(SDF) 코드로 직접 그렸는데, 사용자가
/// "웹에서 SVG를 찾아 쓰라고 했지 만들라고 하지 않았다"고 명확히 지적해서
/// 2026-09-06에 실제 SVG 참조 방식으로 교체했다 — Twitter의 오픈소스 이모지
/// 세트 Twemoji(CC BY 4.0, twitter/twemoji 저장소)에서 각 콘셉트에 맞는
/// 이모지를 그대로 가져왔다. 원본은 <c>Assets/Art/Twemoji/</c>에 코드포인트
/// 파일명 그대로(예: <c>1f4a9.svg</c>) 보관하고, 실제 게임이 쓰는 6장만
/// <c>Assets/Resources/ComboIcons_SVG/</c>에 역할 이름으로 복사했다(Kenney·
/// 화투 SVG 때와 같은 "원본은 Art, 실제 쓰는 것만 Resources" 원칙).
///
/// | 역할 | 이모지 | 코드포인트 |
/// |---|---|---|
/// | 뻑 | 💩 | 1f4a9 |
/// | 뻑먹기/자뻑 | 🧻 | 1f9fb |
/// | 쪽 | 💋 | 1f48b |
/// | 따닥 | 🫰 | 1faf0 |
/// | 쓸 | 🧹 | 1f9f9 |
/// | 스톱 | ✋ | 270b |
///
/// Twemoji 그래픽은 CC BY 4.0 — 출처 표시가 필요하다(화투 카드 CC BY-SA와
/// 같은 종류의 의무, 타이틀 설정→라이선스 정보 화면에 이미 같이 적어뒀다).
/// 앱 전체를 오픈소스로 풀 필요는 없고 이 이미지 자산에만 적용된다.
///
/// 각 SVG는 Unity 내장 Vector Graphics 임포터(svgType=VectorSprite)로
/// 임포트해서 `Sprite`를 직접 만든다 — 벡터라서 크기를 키워도(뻑 이펙트가
/// 300×300까지 커진다) 깨지지 않는다. `Resources.Load`는 최초 1회만 하고
/// 캐싱해서 재사용한다.
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
