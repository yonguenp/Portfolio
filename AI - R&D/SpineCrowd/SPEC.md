# SpineCrowd — Spine 애니메이션 GPU 베이킹(VAT) 구현 명세

> 이 문서는 구현 에이전트(Codex)가 그대로 따라 작업할 수 있도록 쓴 명세다.
> 제품명 `SpineCrowd`는 임시 이름이며 네임스페이스·패키지명은 바꾸기 쉽게 한 곳에서 관리한다.

## 1. 목표

spine-unity의 `SkeletonAnimation`은 매 프레임 CPU에서 본 계산과 메시 재생성을 한다. 유닛이 수십~수백 개가 되면 메인 스레드가 병목이 된다.
이 패키지는 Spine 애니메이션을 **에디터에서 미리 구워(bake)** 두고, 런타임에는 **Spine 계산 없이** 재생한다.

- **Lite(1단계):** 프레임별 메시를 구워 두고 메시만 교체해 재생. Spine CPU 비용 0.
- **Pro(2단계):** 정점 위치를 텍스처(VAT)에 굽고 버텍스 셰이더에서 변형. 같은 캐릭터를 **GPU 인스턴싱으로 수백~수천 개** 그린다.
- 프레임 단위 재생이 기본이라 **스프라이트 애니메이션처럼 끊기는 연출(12/15fps 등)** 도 옵션으로 제공한다.

### 성공 기준 (목표 수치 — 실측으로 확정)
- 중급 안드로이드 기기(예: Snapdragon 7xx급)에서 같은 캐릭터 **500마리 60fps**, 메인 스레드 애니메이션 비용이 `SkeletonAnimation` 대비 **90% 이상 감소**.
- 베이크 결과가 원본 `SkeletonAnimation`과 샘플 프레임에서 **시각적으로 동일**(픽셀 비교 임계값 이하, 7장 참고).

## 2. 시장과 포지셔닝 (요약)

- 수요: 유닛이 많은 2D 장르(타워 디펜스, 오토배틀러, 서바이버류, 방치형 RPG). 한·중·일 모바일에서 Spine 비중이 큼. 구매자는 Spine 라이선스를 가진 프로 팀 → $30~60 가능.
- 일반 VAT 도구(무료 다수)는 3D 스킨드 메시용이라 **Spine의 부착물 교체·그리기 순서 변화·슬롯 색상**을 처리하지 못한다. 이것이 핵심 차별점.
- 경쟁: GitHub `MlsMoon/SpineGpuSkinning`(무료, spine-unity 4.2/URP, 스킨 조합·그리기 순서 레이아웃을 굽는 GPU 스키닝 방식). **구현 전에 이 프로젝트의 범위와 한계를 확인**하고 차별점을 문서화할 것.
- 가격안: Lite $19.99 / Pro $49.99 (또는 단일 상품 + Lite 기능 포함).

## 3. 범위

### 지원 대상
- Unity **2021.3 LTS 이상**, Built-in RP와 URP (HDRP는 범위 밖, 추후 검토)
- spine-unity **4.2** (1차 목표). 4.1 지원은 API 차이를 확인한 뒤 결정
- 플랫폼: 모바일(GLES3 / Vulkan / Metal), PC. **버텍스 텍스처 페치(VTF)가 필요**하므로 GLES2는 지원하지 않는다.

### 범위 밖 (v1에서 하지 않음 — 문서에 명시)
- 애니메이션 간 블렌딩/믹스(전환은 즉시 컷. 필요하면 짧은 알파 페이드로 대체)
- 런타임 스킨 조합, 런타임 본 조작, IK 타깃 변경
- 다중 트랙 합성(트랙 0 단일 애니메이션만 굽는다. 합성된 결과를 굽고 싶으면 "베이크용 조합"을 별도 정의 — 9장 확장 항목)

## 4. 아키텍처 개요

```
[에디터]                                   [런타임]
SkeletonDataAsset ──► Sampler ──► FrameData ──► Lite: BakedMeshClip ─► SpineBakedMeshPlayer (MeshFilter 교체)
  (스킨, 애니메이션,     (fps로 샘플링,                  
   fps 선택)            물리 포함 순차 시뮬)    └─► Pro: TopologyBuilder ─► SpineVatAsset ─► SpineVatRenderer (개별 GameObject)
                                                      (통합 메시, 레이아웃,        (메시, VAT 텍스처,  └► SpineVatCrowd   (대량 인스턴싱)
                                                       VAT/색상 텍스처)            클립 표, 이벤트)
```

### 어셈블리 구성
| 어셈블리 | 내용 | 의존성 |
| --- | --- | --- |
| `SpineCrowd.Core` | **Unity/Spine에 의존하지 않는 순수 로직**: 프레임 인덱스 계산, 레이아웃 중복 제거, 토폴로지 할당, 텍스처 패킹 계산 | 없음 (NUnit 테스트 가능) |
| `SpineCrowd.Runtime` | 재생 컴포넌트, 셰이더, 에셋 정의 | Core, Unity |
| `SpineCrowd.Editor` | 베이커, 검증기, 미리보기 창 | Core, Runtime, spine-unity, spine-csharp |
| `SpineCrowd.Tests` | EditMode 테스트 | Core (+ Editor 일부) |

> **런타임 어셈블리는 spine-unity에 의존하지 않는다.** 구운 결과만 있으면 Spine 런타임 없이도 재생할 수 있어야 한다(빌드 크기·초기화 비용 감소, 판매 포인트).

## 5. 상세 설계

### 5.1 샘플링 (공통)
1. 선택한 스킨으로 `Skeleton` 생성 → `SetToSetupPose()`.
2. 애니메이션별로 `t = i / fps` (i = 0..N-1, 마지막 프레임 포함 여부는 loop 옵션에 따름)마다:
   - `animation.Apply(skeleton, lastTime, t, loop, events, 1, MixBlend.Setup, MixDirection.In)`로 포즈 적용, 발생한 이벤트 수집
   - **4.2 물리 제약조건**이 있으면 프레임 0부터 순차적으로 `skeleton.Update(dt)` + `UpdateWorldTransform(Physics.Update)` 진행. 루프 이음새가 맞도록 **워밍업 1회전 후 샘플링** 옵션 제공
   - 물리가 없으면 `UpdateWorldTransform(Physics.Pose)`(4.2) / `UpdateWorldTransform()`(4.1)
3. `skeleton.DrawOrder`를 순회하며 보이는 슬롯의 부착물별 월드 정점을 계산:
   - `RegionAttachment.ComputeWorldVertices(...)` — 정점 4개
   - `MeshAttachment.ComputeWorldVertices(...)` — `WorldVerticesLength / 2`개 (가중치·FFD 디폼 반영됨)
   - 색상 = `skeleton.Color × slot.Color × attachment.Color` (+ Tint Black 사용 시 `slot.DarkColor`)
4. 결과를 `FrameData { entries: [(slotIndex, attachmentKey, positions[], color, darkColor)], drawOrder: [...], events: [...] }`로 저장.

> **API 이름은 반드시 실제 소스로 확인할 것.** `EsotericSoftware/spine-runtimes`의 `4.2` 브랜치에서 `spine-csharp/src`와 `spine-unity`를 확인하고, 문서에 적힌 시그니처와 다르면 소스를 따른다. 추측으로 API를 만들지 않는다.

### 5.2 Lite: 프레임별 메시 (1단계)
- spine-unity의 `MeshGenerator`(또는 `SkeletonAnimation`과 같은 경로)로 프레임마다 `Mesh`를 생성해 그대로 저장한다. 클리핑·블렌드 모드별 서브메시가 **원본과 동일하게** 처리된다.
- 저장: `BakedMeshClip { string name; float fps; bool loop; Mesh[] frames; Material[][] materialsPerFrame(대부분 공유); EventKey[] events; }`
- 재생: `SpineBakedMeshPlayer` — `MeshFilter.sharedMesh`를 프레임에 맞게 교체. 시간 진행은 C#에서, 여러 인스턴스는 **한 매니저가 일괄 업데이트**(MonoBehaviour.Update 수백 개 호출 방지).
- 메모리 최적화: 프레임 간 동일한 메시는 참조 공유(해시 비교), 인덱스 버퍼가 같은 프레임은 정점만 다른 메시로 저장.
- Lite는 Pro의 **폴백**이기도 하다. Pro 검증기가 지원하지 않는 기능(클리핑 등)을 만나면 Lite 베이크를 권장한다.

### 5.3 Pro: 통합 토폴로지 (핵심)
Spine은 프레임마다 정점 수·부착물·그리기 순서가 바뀐다. VAT는 **고정된 정점 집합**이 필요하므로 다음과 같이 통합한다.

1. **엔트리 수집:** 선택한 모든 애니메이션의 모든 프레임에서 한 번이라도 보이는 `(slotIndex, attachment)` 쌍을 수집한다. 각 쌍이 하나의 **엔트리**.
2. **정점 범위 할당:** 엔트리마다 정점 범위를 할당한다(region 4개, mesh N개). UV와 삼각형은 엔트리마다 고정이다(부착물이 정한다).
3. **보이지 않는 엔트리:** 해당 프레임에 안 보이는 엔트리는 정점을 한 점으로 모아(퇴화 삼각형) **필레이트 비용 0**으로 만든다.
4. **그리기 순서 레이아웃:** 투명 블렌딩이라 삼각형 순서 = 그리는 순서다. 프레임별 드로우 오더(보이는 엔트리의 순서)를 문자열/해시로 만들어 **중복 제거**한다. 고유 레이아웃마다 같은 정점을 쓰되 **인덱스 버퍼 순서만 다른 Mesh**를 하나씩 만든다. 프레임마다 `layoutId`를 기록한다. 대부분의 애니메이션은 레이아웃이 1~3개다.
5. **블렌드 모드:** Additive/Multiply/Screen 슬롯은 별도 서브메시(별도 머티리얼)로 분리. 그리기 순서가 블렌드 모드 사이를 오가면 정확한 재현이 불가능할 수 있으므로 검증기가 경고한다.
6. **아틀라스 페이지:** v1은 **단일 아틀라스 페이지만** 지원(검증기 오류). 여러 페이지면 서브메시 분리를 2차로 검토.

### 5.4 Pro: 텍스처 포맷
- **위치 VAT:** 가로 = 정점 수(4096 초과 시 여러 행으로 랩), 세로 = 전체 프레임 수(모든 클립을 이어 붙임). 포맷 `RGBAHalf`(x, y 위치. z/w는 예약). 필요하면 `RGBA32` + 바운드 정규화(16비트를 2채널에 패킹) 옵션.
  - 위치는 스켈레톤 로컬 공간 × 스켈레톤 스케일. 바운드를 에셋에 저장.
  - 필터 모드 `Point`, 밉맵 없음, 압축 없음, sRGB 끔.
- **색상 텍스처:** 가로 = 엔트리 수, 세로 = 프레임 수, `RGBA32`. 엔트리 단위 색상과 가시성(알파). Tint Black 사용 시 두 번째 텍스처.
- **정점 → 텍스셀 인덱스:** 정점 UV2.x에 정점 인덱스, UV2.y에 엔트리 인덱스를 넣는다(`SV_VertexID`는 GLES3 호환성 이슈가 있어 사용하지 않음).
- 에셋에 메모리 사용량을 계산해 표시한다. 예: 정점 600 × 프레임 300 × 8바이트 ≈ 1.4MB.

### 5.5 Pro: 셰이더
- Built-in과 URP 모두 동작하는 단일 셰이더(HLSL, `UnityCG`/URP 공용 매크로 분기 또는 두 개의 SubShader).
- 버텍스 셰이더에서 `tex2Dlod`/`SAMPLE_TEXTURE2D_LOD`로 위치·색상 샘플링.
- **프레임 계산은 GPU에서:** 인스턴스별 `_ClipStartRow`, `_ClipFrameCount`, `_ClipFps`, `_StartTime`, `_Speed`, `_Loop`를 인스턴싱 버퍼로 넘기고 `_Time`으로 현재 프레임을 계산한다. CPU는 애니메이션을 바꿀 때만 값을 갱신한다.
- 옵션: **Stepped(기본)** = 정수 프레임만, **Interpolate** = 두 프레임 사이를 선형 보간. 보간은 두 프레임의 `layoutId`와 가시성이 같을 때만 정확하므로, 클립별로 "보간 가능 여부"를 베이크 시 계산해 저장한다.
- 블렌딩: Spine 기본(PMA) `Blend One OneMinusSrcAlpha`, Straight Alpha 옵션. Tint Black 키워드.
- GPU 인스턴싱(`#pragma multi_compile_instancing`) 필수. 개별 GameObject 모드는 MaterialPropertyBlock 대신 인스턴싱 버퍼를 쓰므로 SRP Batcher는 비활성.

### 5.6 런타임 API
```csharp
// 개별 오브젝트 (일반 게임 로직용)
public sealed class SpineVatRenderer : MonoBehaviour {
    public SpineVatAsset asset;
    public void Play(string clip, bool loop = true, float speed = 1f, float normalizedStart = 0f);
    public float NormalizedTime { get; }
    public event Action<string> OnEvent;      // 베이크한 Spine 이벤트 (CPU 측 시간으로 발생)
    public event Action<string> OnComplete;
    public Color tint { get; set; }
    public bool flipX { get; set; }
}

// 대량 렌더링 (GameObject 없이)
public sealed class SpineVatCrowd : IDisposable {
    public SpineVatCrowd(SpineVatAsset asset, int capacity);
    public int Add(Vector3 position, string clip, float speed = 1f);
    public void SetPosition(int id, Vector3 position);
    public void Play(int id, string clip, bool loop = true);
    public void Remove(int id);
    public void Render(Camera camera); // Graphics.RenderMeshInstanced / DrawMeshInstancedIndirect
}
```
- **레이아웃별 배치:** 인스턴스를 현재 프레임의 `layoutId`(+서브메시)별로 묶어 그린다. 배치 수 = 사용 중인 레이아웃 수.
  - GPU에서 프레임을 계산하므로 CPU는 각 인스턴스의 현재 `layoutId`를 같은 공식으로 계산해 그룹핑한다(클립별 레이아웃 표를 런타임에 보유).
- **2D 정렬:** 같은 배치 안의 인스턴스는 버퍼 순서대로 그려진다. Y 정렬이 필요하면 매 프레임 인스턴스를 Y로 정렬(옵션, N log N). 정렬 안 하면 겹칠 때 순서가 틀릴 수 있음을 문서화.
- 컬링: 카메라 바운드 밖 인스턴스는 CPU에서 제외(1차), 필요 시 컴퓨트 셰이더 컬링(2차).

### 5.7 에디터 도구
- **Bake 창** (`Window > SpineCrowd > Baker`):
  - 입력: `SkeletonDataAsset`, 스킨, 애니메이션 목록(체크박스), fps, loop 여부, 물리 워밍업, 모드(Lite/Pro), 출력 폴더
  - 출력: Lite는 `BakedMeshClip` 에셋들, Pro는 `SpineVatAsset` + 레이아웃 메시 + VAT/색상 텍스처 + 머티리얼
  - 표시: 정점 수, 엔트리 수, 레이아웃 수, 텍스처 크기, **예상 메모리**
- **검증기:** 베이크 전에 지원하지 않는 기능을 리포트 — 클리핑 부착물, 다중 아틀라스 페이지, 블렌드 모드 혼재, 정점 수 한도 초과, 물리 제약조건(루프 이음새 경고).
- **미리보기:** 씬에 원본 `SkeletonAnimation`과 베이크 결과를 나란히 띄우는 비교 버튼.
- 재베이크 시 기존 에셋을 덮어쓰되 GUID를 유지해 참조가 끊기지 않게 한다.

## 6. 패키지 구조
```
SpineCrowd/
  package.json                 (com.<publisher>.spinecrowd — spine-unity는 사용자 프로젝트에 설치된 것을 사용)
  README.md, CHANGELOG.md, LICENSE.md
  Core/        SpineCrowd.Core.asmdef
  Runtime/     SpineCrowd.Runtime.asmdef, Shaders/
  Editor/      SpineCrowd.Editor.asmdef   (spine-unity 어셈블리 참조, versionDefines로 4.1/4.2 분기)
  Tests/Editor SpineCrowd.Tests.asmdef
  Samples~/Benchmark  (벤치마크 씬 생성 스크립트 — Spine 예제 에셋은 포함하지 않음)
  Documentation~/
```

## 7. 테스트와 검증

### 자동 테스트 (EditMode, Core 위주)
- 프레임 인덱스 계산: 루프/비루프, 속도 음수, 경계(마지막 프레임), 정규화 시간 시작값
- 레이아웃 중복 제거: 같은 드로우 오더는 같은 id, 다른 오더는 다른 id
- 토폴로지 할당: 엔트리별 정점 범위 겹침 없음, 삼각형 인덱스가 범위 안
- 텍스처 패킹: 4096 초과 랩핑 시 인덱스 ↔ 텍스셀 왕복 일치
- 퇴화 처리: 보이지 않는 엔트리의 정점이 한 점에 모임

### 시각 비교 (에디터 테스트 또는 수동)
- 원본 `SkeletonAnimation`과 베이크 결과를 같은 프레임에서 RenderTexture로 찍어 픽셀 차이 비교. 평균 오차 임계값(예: 채널당 2/255) 이하.
- 테스트 데이터: spine-unity에 포함된 예제(`spineboy-pro`, `raptor`, `goblins`(스킨), `stretchyman`(메시 디폼)). **예제 에셋은 패키지에 넣지 않는다**(라이선스). 테스트는 사용자가 spine-unity 예제를 임포트했을 때만 실행되도록 조건부.

### 벤치마크 (판매 근거)
- 씬: 같은 캐릭터 50 / 200 / 500 / 1000마리
- 비교: `SkeletonAnimation` vs Lite vs Pro(개별) vs Pro(Crowd)
- 측정: 메인 스레드 ms, 렌더 스레드 ms, 드로우콜/배치 수, FPS, 메모리
- 결과를 `Documentation~/benchmark.md`에 기기·Unity 버전과 함께 기록

## 8. 마일스톤
1. **M0 조사 (반나절):** spine-runtimes 4.2 API 확인, `SpineGpuSkinning` 분석 → 차별점 메모
2. **M1 Lite:** 샘플러 + 프레임별 메시 베이크 + 재생 매니저 + 이벤트. 원본과 시각 비교 통과
3. **M2 Pro 코어:** 엔트리 수집, 레이아웃, VAT/색상 텍스처, 셰이더(Stepped), `SpineVatRenderer`
4. **M3 Crowd:** `SpineVatCrowd`, 레이아웃별 배치, Y 정렬, 컬링, 벤치마크 씬
5. **M4 완성도:** 검증기, 미리보기, 보간 옵션, Tint Black, 문서, 스토어 자료

각 마일스톤 끝에 **사람이 Unity에서 확인할 체크리스트**를 `Documentation~/checklist-M?.md`로 남긴다.

## 9. 확장 후보 (v1 이후)
- 다중 트랙 조합을 "베이크용 프리셋"으로 정의해 합성 결과를 굽기(예: run + shoot)
- 클리핑 지원(프레임별 클리핑 결과 삼각형을 최대치로 예약)
- 다중 아틀라스 페이지 서브메시
- Entities(ECS) 통합, 컴퓨트 셰이더 컬링/정렬
- HDRP 지원

## 10. 주의 사항
- **라이선스:** Spine Runtimes License상 이 패키지를 쓰는 사람도 Spine 라이선스가 필요하다. 스토어 설명에 명시. spine-unity 소스나 Spine 예제 에셋을 패키지에 포함하지 않는다.
- **정확성 우선:** 원본과 다르게 보이는 경우는 조용히 넘기지 말고 검증기에서 경고하거나 Lite로 폴백하도록 안내한다.
- **에이전트 작업 원칙:** Unity를 실행할 수 없는 환경이라면 컴파일 가능한 상태를 유지하고, Core 로직은 단위 테스트로 검증하며, Unity에서만 확인 가능한 항목은 체크리스트로 남긴다. spine API는 추측하지 말고 소스를 확인한다.
