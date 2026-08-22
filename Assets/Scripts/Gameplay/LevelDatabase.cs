using UnityEngine;

public static class LevelDatabase
{
    public const int CAPACITY = 4;

    // Colors: 1=Red 2=Blue 3=Green 4=Yellow 5=Purple 6=Orange
    // Each tube = [bottom → top] color indices
    // Difficulty levers: color count ↑, empty tube count ↓
    //
    // L1-2   : Easy   (3색, 빈튜브 2→1)
    // L3-5   : Normal (4색, 빈튜브 2→1)
    // L6-8   : Hard   (5색, 빈튜브 2→1)
    // L9-12  : Expert (6색, 빈튜브 2→1)
    // L13-15 : Master (6색, 빈튜브 1, 최대 섞임)
    public static readonly int[][][] Levels =
    {
        // ── Easy ──────────────────────────────────────────
        // L1  3색 · 빈2  (tutorial)
        new[]{ new[]{1,2,3,1}, new[]{2,3,1,2}, new[]{3,1,2,3},
               new int[0], new int[0] },

        // L2  3색 · 빈1  (버퍼 타이트)
        new[]{ new[]{1,2,3,1}, new[]{2,3,1,2}, new[]{3,1,2,3},
               new int[0] },

        // ── Normal ────────────────────────────────────────
        // L3  4색 · 빈2  cyclic-1
        new[]{ new[]{1,2,3,4}, new[]{2,3,4,1}, new[]{3,4,1,2}, new[]{4,1,2,3},
               new int[0], new int[0] },

        // L4  4색 · 빈2  anti-diagonal (다른 배열)
        new[]{ new[]{1,2,3,4}, new[]{2,1,4,3}, new[]{3,4,1,2}, new[]{4,3,2,1},
               new int[0], new int[0] },

        // L5  4색 · 빈1
        new[]{ new[]{1,2,3,4}, new[]{2,1,4,3}, new[]{3,4,1,2}, new[]{4,3,2,1},
               new int[0] },

        // ── Hard ──────────────────────────────────────────
        // L6  5색 · 빈2  cyclic-1
        new[]{ new[]{1,2,3,4}, new[]{2,3,4,5}, new[]{3,4,5,1},
               new[]{4,5,1,2}, new[]{5,1,2,3},
               new int[0], new int[0] },

        // L7  5색 · 빈2  latin-rectangle (다른 배열)
        new[]{ new[]{1,2,3,4}, new[]{2,4,5,3}, new[]{3,1,4,5},
               new[]{4,5,2,1}, new[]{5,3,1,2},
               new int[0], new int[0] },

        // L8  5색 · 빈1
        new[]{ new[]{1,2,3,4}, new[]{2,3,4,5}, new[]{3,4,5,1},
               new[]{4,5,1,2}, new[]{5,1,2,3},
               new int[0] },

        // ── Expert ────────────────────────────────────────
        // L9  6색 · 빈2  cyclic-1
        new[]{ new[]{1,2,3,4}, new[]{2,3,4,5}, new[]{3,4,5,6},
               new[]{4,5,6,1}, new[]{5,6,1,2}, new[]{6,1,2,3},
               new int[0], new int[0] },

        // L10 6색 · 빈2  latin-rectangle
        // 검증: 각 색 1~6 정확히 4회 등장 ✓
        new[]{ new[]{1,2,3,4}, new[]{2,4,6,1}, new[]{3,6,2,5},
               new[]{4,1,5,6}, new[]{5,3,1,2}, new[]{6,5,4,3},
               new int[0], new int[0] },

        // L11 6색 · 빈1  (L9 배열, 버퍼 절반)
        new[]{ new[]{1,2,3,4}, new[]{2,3,4,5}, new[]{3,4,5,6},
               new[]{4,5,6,1}, new[]{5,6,1,2}, new[]{6,1,2,3},
               new int[0] },

        // L12 6색 · 빈1  (L10 배열, 버퍼 절반)
        new[]{ new[]{1,2,3,4}, new[]{2,4,6,1}, new[]{3,6,2,5},
               new[]{4,1,5,6}, new[]{5,3,1,2}, new[]{6,5,4,3},
               new int[0] },

        // ── Master ────────────────────────────────────────
        // L13 6색 · 빈1  최대 섞임 A
        // 검증: R=T0[0]+T2[3]+T3[2]+T5[1]=4✓ B=T0[3]+T1[1]+T3[0]+T4[2]=4✓
        //       G=T1[0]+T2[2]+T4[1]+T5[3]=4✓ Y=T0[1]+T1[3]+T4[0]+T5[2]=4✓
        //       P=T1[2]+T2[0]+T3[1]+T4[3]=4✓ O=T0[2]+T2[1]+T3[3]+T5[0]=4✓
        new[]{ new[]{1,4,6,2}, new[]{3,2,5,4}, new[]{5,6,3,1},
               new[]{2,5,1,6}, new[]{4,3,2,5}, new[]{6,1,4,3},
               new int[0] },

        // L14 6색 · 빈1  최대 섞임 B (L13 튜브 순서 회전)
        new[]{ new[]{6,1,4,3}, new[]{1,4,6,2}, new[]{3,2,5,4},
               new[]{5,6,3,1}, new[]{2,5,1,6}, new[]{4,3,2,5},
               new int[0] },

        // L15 6색 · 빈1  최대 섞임 C (L13 역순)
        new[]{ new[]{2,5,1,6}, new[]{4,3,2,5}, new[]{6,1,4,3},
               new[]{1,4,6,2}, new[]{3,2,5,4}, new[]{5,6,3,1},
               new int[0] },
    };

    public static readonly Color[] Palette =
    {
        new Color(.08f, .11f, .24f), // 0 = empty slot
        new Color(.93f, .25f, .30f), // 1 = Red
        new Color(.20f, .50f, .95f), // 2 = Blue
        new Color(.18f, .80f, .38f), // 3 = Green
        new Color(.98f, .82f, .10f), // 4 = Yellow
        new Color(.70f, .20f, .92f), // 5 = Purple
        new Color(.97f, .55f, .12f), // 6 = Orange
    };

    public static readonly Color[] PaletteLight =
    {
        new Color(.10f, .14f, .28f),
        new Color(1f,   .45f, .50f),
        new Color(.40f, .70f, 1f  ),
        new Color(.38f, .95f, .55f),
        new Color(1f,   .95f, .35f),
        new Color(.88f, .40f, 1f  ),
        new Color(1f,   .72f, .35f),
    };
}
