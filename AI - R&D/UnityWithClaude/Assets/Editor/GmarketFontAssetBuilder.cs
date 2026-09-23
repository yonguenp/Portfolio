using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// GmarketSansTTF(Light/Medium/Bold, Assets/Resources/TextMesh Pro/Fonts/에 이미 추가됨)로
/// TMP SDF 폰트 에셋 3개를 만든다. 기존 "ONE Mobile POP SDF"와 같은 생성 파라미터
/// (Dynamic 아틀라스, 2048x2048, SDFAA, pointSize 90, padding 9)를 그대로 맞춰서
/// 나중에 실사용 시 두 폰트가 같은 크기감으로 섞여도 어색하지 않게 했다.
/// Dynamic 모드라 한글 전체를 미리 구울 필요 없이, 실제로 화면에 쓰인 글자만
/// 처음 렌더링될 때 아틀라스에 채워진다.
/// </summary>
public static class GmarketFontAssetBuilder
{
    const string FontDir = "Assets/Resources/TextMesh Pro/Fonts";

    [MenuItem("Tools/Fonts/Build GmarketSans TMP Font Assets")]
    public static void Build()
    {
        BuildOne("GmarketSansTTFLight", "GmarketSans SDF Light");
        BuildOne("GmarketSansTTFMedium", "GmarketSans SDF Medium");
        BuildOne("GmarketSansTTFBold", "GmarketSans SDF Bold");
        AssetDatabase.SaveAssets();
        Logger.Log("[GmarketFontAssetBuilder] 완료 — " + FontDir + " 에 3개 생성");
    }

    static void BuildOne(string ttfName, string outputName)
    {
        string ttfPath = $"{FontDir}/{ttfName}.ttf";
        var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (font == null)
        {
            Debug.LogError("[GmarketFontAssetBuilder] TTF를 못 찾음: " + ttfPath);
            return;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
        fontAsset.name = outputName;

        string outPath = $"{FontDir}/{outputName}.asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath) != null) AssetDatabase.DeleteAsset(outPath);
        AssetDatabase.CreateAsset(fontAsset, outPath);

        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = outputName + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }
        if (fontAsset.material != null)
        {
            fontAsset.material.name = outputName + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }
    }
}
