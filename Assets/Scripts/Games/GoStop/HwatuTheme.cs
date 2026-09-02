using UnityEngine;
using TMPro;

/// <summary>
/// "Modern Traditional Go-Stop" 색상 시스템(ui.md 기준, Assets/Editor/Mockup/
/// GoStopOrientalMockupBuilder.cs 목업에서 먼저 검증됨)의 런타임 버전.
/// 목업 전용 클래스는 UnityEditor API를 쓰는 Editor 스크립트라 게임 런타임
/// 코드(HwatuUI 등)에서 직접 참조할 수 없어서, 색상/폰트 상수만 이 파일로
/// 옮겨 담았다 — 값은 목업과 반드시 동일하게 유지할 것(둘이 갈라지면 "목업엔
/// 있는데 실제 게임엔 없는 색"이 생긴다).
/// </summary>
public static class HwatuTheme
{
    public static readonly Color DeepGreen     = Hex("#24452F"); // 테이블 배경
    public static readonly Color DarkGreen     = Hex("#193523"); // 중앙 필드/획득패 존
    public static readonly Color WarmCream     = Hex("#F3EBDD"); // 플레이어 패널/모달 표면
    public static readonly Color CreamWhite    = Hex("#FFFDF8"); // 밝은 텍스트/카드 프레임
    public static readonly Color HwatuRed      = Hex("#C93A32"); // Primary 액션/승리
    public static readonly Color Gold          = Hex("#D5A43A"); // 현재 턴/선택/보상
    public static readonly Color TextPrimary   = Hex("#20251F");
    public static readonly Color TextSecondary = Hex("#687066");
    public static readonly Color CardBackMaroon= Hex("#5C1A1A"); // 카드 뒷면(전통 화투 뒷면 톤)

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    const string FontPath = "TextMesh Pro/Fonts/GmarketSans SDF Medium";
    const string FontBoldPath = "TextMesh Pro/Fonts/GmarketSans SDF Bold";

    static TMP_FontAsset _font;
    public static TMP_FontAsset Font => _font ??= Resources.Load<TMP_FontAsset>(FontPath);
    static TMP_FontAsset _fontBold;
    public static TMP_FontAsset FontBold => _fontBold ??= Resources.Load<TMP_FontAsset>(FontBoldPath);
}
