using System.Collections.Generic;
using UnityEngine;

namespace SamgukDefense
{
    /// <summary>
    /// 20×20 전투 그리드의 좌표 변환 + 바닥 렌더링을 담당한다. 좌표계는 보드 중심을 원점으로 하는
    /// 연속 좌표(BoardPosition, 단위=셀)를 쓴다 — 셀 (col,row)의 중심은
    /// (col - gridSize/2 + 0.5, row - gridSize/2 + 0.5). 2026-09-16부터 완전 3D로 전환하면서
    /// BoardRoot가 RectTransform(UGUI)에서 일반 Transform(월드 스페이스)으로 바뀌었다 — X,Y 두
    /// 값의 "의미"(그리드 좌표계, 이동/공격 로직)는 전혀 안 바뀌었고, SamgukUnit.SyncTransform()이
    /// 이 Vector2를 3D 월드의 X,Z로 매핑할 뿐이다(Y=바닥 높이 0 고정). 기존 400개 UGUI Image 셀
    /// 대신 체커보드를 구운 텍스처 하나를 입힌 Plane 메시 하나로 바닥을 그린다 — 시각적으로는
    /// 동일하되 훨씬 가볍다.
    /// </summary>
    public class SamgukGridBoard
    {
        public int GridSize { get; }
        public float CellSize { get; }
        public Transform BoardRoot { get; }
        public GameObject Floor { get; private set; }

        readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        public SamgukGridBoard(Transform parent, int gridSize, float cellSize)
        {
            GridSize = gridSize;
            CellSize = cellSize;

            var existingRoot = parent.Find("BoardRoot");
            if (existingRoot != null && existingRoot.Find("Floor") != null)
            {
                BoardRoot = existingRoot;
                Floor = existingRoot.Find("Floor").gameObject;
                return;
            }

            var rootGo = new GameObject("BoardRoot");
            BoardRoot = rootGo.transform;
            BoardRoot.SetParent(parent, false);
            BoardRoot.localPosition = Vector3.zero;

            BuildFloor(gridSize, cellSize);
        }

        void BuildFloor(int gridSize, float cellSize)
        {
            Floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Floor.name = "Floor";
            Floor.transform.SetParent(BoardRoot, false);
            // Unity 기본 Plane은 10유닛 정사각형(스케일 1 기준) — gridSize*cellSize 크기로 맞춘다.
            float worldSize = gridSize * cellSize;
            Floor.transform.localScale = new Vector3(worldSize / 10f, 1f, worldSize / 10f);
            Floor.transform.localPosition = Vector3.zero;
            // 유닛 클릭 레이캐스트(Physics)와 겹치면 헷갈리니 바닥 자체의 콜라이더는 없앤다 —
            // 바닥 클릭 좌표는 SamgukBoardPanZoom이 수학적 평면 교차로 직접 계산한다.
            // 에디터에서 BuildStaticUI()를 직접 불러 씬에 구울 때(플레이 모드 밖)도 여길
            // 지나가므로 Application.isPlaying으로 갈라야 한다 — Destroy()를 에디트 모드에서
            // 부르면 조용히 무시되고 "Destroy may not be called from edit mode!" 에러만 남아서,
            // 콜라이더가 안 지워진 채로 씬에 그대로 저장되는 사고가 실제로 났었다(2026-09-16).
            var col = Floor.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Object.Destroy(col);
                else Object.DestroyImmediate(col);
            }

            var renderer = Floor.GetComponent<MeshRenderer>();
            // 이 프로젝트가 실제로는 URP였다(Standard 셰이더는 Built-in 전용이라 URP 카메라에서
            // 아예 안 그려짐 — 화면이 완전히 까맣게 나온 원인). URP Lit → Simple Lit → Built-in
            // Standard 순으로 찾아 첫 번째로 존재하는 걸 쓴다.
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Legacy Shaders/Diffuse");
            var mat = new Material(shader);
            var tex = BuildCheckerTexture(gridSize);
            mat.mainTexture = tex;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.05f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 실시간 그림자 없음(BrickBreaker3D와 동일 원칙 — 블롭 섀도로 대체)
        }

        static Texture2D BuildCheckerTexture(int gridSize)
        {
            const int pxPerCell = 12;
            int size = gridSize * pxPerCell;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var colA = new Color(0.16f, 0.15f, 0.11f);
            var colB = new Color(0.13f, 0.12f, 0.09f);
            var colBorder = new Color(0.08f, 0.075f, 0.055f);

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                int row = y / pxPerCell;
                int localY = y % pxPerCell;
                for (int x = 0; x < size; x++)
                {
                    int col = x / pxPerCell;
                    int localX = x % pxPerCell;
                    bool border = localX == 0 || localY == 0;
                    bool alt = (col + row) % 2 == 0;
                    pixels[y * size + x] = border ? colBorder : (alt ? colA : colB);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public Vector2 CellToBoardPos(int col, int row) =>
            new Vector2(col - GridSize / 2f + 0.5f, row - GridSize / 2f + 0.5f);

        public Vector2Int BoardPosToCell(Vector2 boardPos) => new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(boardPos.x - 0.5f + GridSize / 2f), 0, GridSize - 1),
            Mathf.Clamp(Mathf.RoundToInt(boardPos.y - 0.5f + GridSize / 2f), 0, GridSize - 1));

        public bool IsOccupied(Vector2Int cell) => occupied.Contains(cell);

        public void MarkOccupied(Vector2Int cell) => occupied.Add(cell);
        public void ClearOccupied(Vector2Int cell) => occupied.Remove(cell);
        public void ClearAllOccupied() => occupied.Clear();

        public bool TryRandomEmptyCell(out Vector2Int cell)
        {
            const int maxAttempts = 400;
            for (int i = 0; i < maxAttempts; i++)
            {
                var c = new Vector2Int(Random.Range(0, GridSize), Random.Range(0, GridSize));
                if (!occupied.Contains(c))
                {
                    cell = c;
                    occupied.Add(c);
                    return true;
                }
            }
            cell = default;
            return false;
        }

        public float BoardHalfExtent => GridSize / 2f;
    }
}
