# Resource Sources — SamgukDefense (삼국지 로그라이크 디펜스)

이 문서는 `Assets/Scripts/Games/SamgukDefense/` 게임에 실제로 다운로드·적용한
외부 리소스의 출처와 라이선스를 기록한다. 다른 게임(GoStop 등)이 쓰는 리소스는
포함하지 않는다 — 이 저장소는 여러 게임을 함께 담고 있고, 이 문서는 SamgukDefense
전용이다.

원칙: 역사적 인물(유비/관우/장비 등) 자체는 자유롭게 쓸 수 있지만, 그 인물을
그린 기존 상용 게임의 일러스트/모델/UI는 절대 가져오지 않는다. 여기 실린 스프라이트는
전부 CC0(퍼블릭 도메인) 제네릭 판타지 소재이며, "이게 실제 역사적 인물의 초상"이라고
주장하지 않는다 — 로그라이크 유닛을 구분하기 위한 대체 그림(예전의 "단색 사각형 +
이름 첫 글자" placeholder보다 나은 대체물)일 뿐이다.

---

## Character (캐릭터 유닛 스프라이트, 전투 보드 + 로비 카드)

### Tiny Dungeon (Kenney)
- Source: Kenney (www.kenney.nl)
- URL: https://kenney.nl/assets/tiny-dungeon
- License: CC0 (Creative Commons Zero / Public Domain)
- Commercial Use: 허용(제한 없음)
- Attribution: 불필요(권장 사항일 뿐)
- File Type: PNG, 16×16px 개별 타일(132장)
- Usage: 6명의 캐릭터 각각에 타일 1장씩 배정 — 실제 역사적 외형이 아니라 장르에 맞는
  제네릭 판타지 전사/기사 실루엣을 골라 배정했다(캐릭터별 배정 근거는 아래 표).
  `Assets/Resources/Sprites/Samguk/Characters/char_*.png`로 복사, 원본은
  `Assets/Art/Kenney/tiny-dungeon/`에 라이선스 파일과 함께 보관.
  `SamgukCharacterData.icon` 필드에 연결(`Assets/Resources/Data/Samguk/Characters/*.asset`).
- Notes: 원본 타일 인덱스 매핑 — 조조=tile_0100, 관우=tile_0096, 유비=tile_0098,
  여포=tile_0086, 장비=tile_0087, 조운=tile_0099. 색은 원본 그대로 사용(틴트 없음) —
  이미 색이 구워진 픽셀아트라 추가로 틴트하면 셰이딩이 탁해진다.
- 2026-09-16 추가: 이 팩은 사람형(인간형) 스프라이트가 tile_0084~0100 구간에 10여 장
  뿐이라, 신규 캐릭터 18명 전부에 서로 다른 아이콘을 줄 수 없었다 — "캐릭터이자 보스
  몬스터로도 등장하는" 6명(화웅=tile_0110, 장합=tile_0097, 장료=tile_0111, 서황=tile_0089,
  주유=tile_0085, 사마의=tile_0084)에만 실제 스프라이트를 배정해 그 하나로 캐릭터판·보스
  몬스터판 둘 다 커버했다(투자 대비 효율 우선). 나머지 12명은 icon=null 폴백(tintColor
  사각형)으로 남겨뒀다 — 다음에 리소스를 더 구해 채울 수 있는 지점으로 문서에 남겨둔다.

---

## Monster (몬스터 스프라이트)

### Tiny Dungeon (Kenney) — 위와 동일 팩
- Source / URL / License / Commercial Use / Attribution: 위 Character 섹션과 동일
- File Type: PNG, 16×16px
- Usage: 황건적(§30-31, 1종만 반복 스폰) 전용 스프라이트로 tile_0112(로브를 두른
  인물형 실루엣) 배정. `Assets/Resources/Sprites/Samguk/Monsters/monster_yellow_turban.png`,
  `SamgukMonsterData.icon`(huangjin.asset)에 연결.
- Notes: "황건적"이 두건을 쓴 반란군이라는 역사적 특징만 참고해 로브 실루엣을
  골랐을 뿐, 특정 게임의 몬스터 디자인을 베낀 게 아니다.
- 2026-09-16 추가: 몬스터 다양성 확장(동물/도적/정예부대/보스 웨이브, §「고스톱」과
  무관 — SamgukDefense §100 근처 참고)으로 15종을 새로 만들었다. 이 중 산적=tile_0108
  (녹색 인간형)과 보스 6종(위 Character 섹션의 6명과 동일 스프라이트 재사용)만 실제
  아이콘이 있고, 나머지(메뚜기/늑대/곰/호랑이/해적/청주병/단양병/호표기)는 이 팩에
  정확히 맞는 동물·병사 실루엣이 없어 tintColor 사각형 폴백으로 남겨뒀다 — 억지로
  안 맞는 스프라이트(거미/박쥐 등)를 끼워 넣는 대신 정직하게 색상 구분만 하는 쪽을
  택했다. 다음에 동물 테마 CC0 팩을 따로 구하면 채울 수 있다.

---

## Item — Weapon (무기 아이콘)

### Generic Fantasy RPG Items (HomoHikka, OpenGameArt)
- Source: OpenGameArt.org
- URL: https://opengameart.org/content/generic-fantasy-rpg-items
- License: CC0 (Creative Commons Zero / Public Domain)
- Commercial Use: 허용(제한 없음)
- Attribution: 불필요
- File Type: PNG 스프라이트시트(`items_28.png`, 102×68px, 17px 격자)에서 개별
  아이콘을 크롭
- Usage: `iron_sword`(격자 좌표 열0), `steel_sword`(열1, 금장식 손잡이 — "상위
  등급" 느낌에 자연스럽게 맞아 그대로 배정), `iron_bow`(열4)를 크롭해
  `Assets/Resources/Sprites/Samguk/Items/{iron_sword,steel_sword,iron_bow}.png`로
  저장. `SamgukItemData.icon`에 연결. 원본은
  `Assets/Art/OpenGameArt/generic-fantasy-rpg-items/items_28.png`에 보관.
- Notes: 색 변경 없이 원본 그대로 사용.

---

## Item — Armor (방어구 아이콘)

### Generic Fantasy RPG Items (HomoHikka, OpenGameArt) — 위와 동일 팩
- Source / URL / License / Commercial Use / Attribution: 위와 동일
- File Type: PNG 스프라이트시트(`item2.png`, 68×68px, 17px 격자)에서 크롭
- Usage: 흉갑(격자 (0,0)-(1,2)) 크롭 1장을 원본 그대로 `iron_armor`에,
  같은 크롭을 가죽색(RGB 168,120,70, 80% 세기)으로 톤 다운한 버전을 `leather_armor`에
  사용 — 같은 실루엣에 재질(금속/가죽)만 다르게 표현하는 흔한 게임 관습을 따랐다.
  `Assets/Resources/Sprites/Samguk/Items/{iron_armor,leather_armor}.png`.
- Notes: `leather_armor`는 원본을 파이썬(Pillow)으로 곱연산 틴트한 파생본 —
  CC0 라이선스가 명시적으로 허용하는 수정/변형이다.

---

## Item — Accessory (장식 아이콘)

### Generic Fantasy RPG Items (HomoHikka, OpenGameArt) — 위와 동일 팩
- Source / URL / License / Commercial Use / Attribution: 위와 동일
- File Type: PNG 스프라이트시트(`item2.png`)에서 크롭
- Usage: 금반지(격자 (0,3)) 크롭 1장을 원본 그대로 `bronze_ornament`에, 같은
  크롭을 옥색(RGB 70,200,130, 85% 세기)으로 틴트한 버전을 `jade_ornament`에 사용.
  `Assets/Resources/Sprites/Samguk/Items/{bronze_ornament,jade_ornament}.png`.
- Notes: `jade_ornament`도 위와 같은 방식의 파생본.

---

## UI (버튼/패널/바 — 확보했으나 이번 패스에서는 미적용)

### UI Pack (Kenney)
- Source: Kenney (www.kenney.nl)
- URL: https://kenney.nl/assets/ui-pack
- License: CC0
- Commercial Use: 허용
- Attribution: 불필요
- File Type: PNG, 9-slice 버튼/패널 다수
- Usage: **아직 실제 UI에 적용하지 않았다.** 이 프로젝트(`Assets/Art/Kenney/ui-pack/`)에
  이미 존재하고 라이선스도 확인됐지만, 이번 작업에서는 데이터 구조·UI 레이아웃
  안정성(ScrollRect/SafeArea)과 캐릭터·아이템·몬스터 스프라이트 적용까지만
  마쳤다. SamgukDefense는 현재도 단색 Image + TMP 텍스트로 패널/버튼을 그린다 —
  다음 패스에서 버튼/패널을 이 팩의 9-slice 스프라이트로 교체할 수 있다.

### UI pack: RPG extension (Kenney)
- Source: Kenney (www.kenney.nl)
- URL: https://kenney.nl/assets/ui-pack-rpg-expansion
- License: CC0
- Commercial Use: 허용
- Attribution: 불필요
- File Type: PNG(HP/마나 바, 커서, 버튼 등)
- Usage: **다운로드·라이선스 확인만 완료, 미적용.** `Assets/Art/Kenney/`에는
  아직 옮기지 않고 `/tmp` 임시 폴더에만 있다 — HP바(`barGreen_horizontalMid.png`
  등)를 실제 유닛 HP바에 적용해보려 했으나, 이 게임의 HP바가 매우 얇게
  렌더링돼(약 5px 높이) 18×18 텍스처의 디테일이 거의 안 보여 효과 대비 위험이
  크다고 판단해 이번엔 보류했다. 다음에 HP바를 더 두껍게 키우는 작업과 함께
  다시 검토할 것.

---

## 2026-09-16 추가 — 캐릭터 20명·몬스터 8종·아이템 540개 공백을 실제 에셋으로 채움

**중요한 선행 사고 — 절차적 생성 시도 후 롤백.** 이 공백을 먼저 Python(PIL)으로 직접
그려서(픽셀아트/벡터 아이콘) 채웠다가 사용자에게 전면 리젝당했다 — "파이썬등으로 png,
svg 만들지 말 것, 웹에서 찾아서 교체할 것"이라는 명확한 지시. 생성했던 568개 파일을
전부 삭제하고 아래처럼 실제 CC0 팩만으로 다시 채웠다(자세한 경위는 `ssam.md` §96.9
참고). **이 프로젝트의 확고한 규칙: 게임 아트는 어떤 경우에도 코드로 새로 그리지
않는다. 항상 실제 라이선스 에셋을 찾아 크롭/틴트해서 쓴다.**

### Tiny Creatures (Clint Bellanger, OpenGameArt) — 신규 팩

- Source: OpenGameArt.org (제작자 Clint Bellanger, clintbellanger.net)
- URL: https://opengameart.org/content/tiny-creatures
- License: CC0 (Creative Commons Zero / Public Domain)
- Commercial Use: 허용(제한 없음) / Attribution: 불필요
- File Type: PNG, 16×16px 개별 타일(180장, `tile_0001`~`tile_0180`)
- **이미 이 프로젝트가 쓰고 있는 Kenney Tiny Dungeon의 공식 확장판**이다
  ("Made with Kenney's permission", 두 팩과 호환되도록 제작됨) — 그래서 기존
  캐릭터/몬스터 아이콘과 그림체가 자연스럽게 이어진다.
- `Assets/Art/OpenGameArt/tiny-creatures/`에 원본(License.txt + Tiles/) 보관.
- Usage: 동물·곤충 몬스터 4종에 **실제 그 종을 그대로** 배정(틴트 없음) —
  곰=tile_0164, 호랑이=tile_0158(줄무늬까지 정확히 일치), 늑대=tile_0143,
  메뚜기 대체=tile_0141(벌 — 이 팩에 정확한 메뚜기/방아깨비가 없어 그나마
  가장 가까운 곤충형을 정직하게 대체했다). 그 외 무장 병사 실루엣 4장
  (tile_0017/0018/0019 — 녹/청/적 갑주 전사, tile_0068 — 맨몸 전사)을 캐릭터·
  사람형 몬스터 풀에 합류시켰다(아래 참고).

### Kenney Tiny Dungeon — 기존 팩 재활용 확대

- Source/URL/License: 위 §Character 섹션과 동일(이미 확보된 팩, 추가 다운로드 없음)
- 기존엔 인간형 tile_0084~0100 구간 중 6장만 썼는데, 남아있던 tile_0085/0086/0088/
  0096/0097/0098/0100/0109/0112 총 9장을 이번에 마저 인간형 캐릭터/몬스터 풀에
  합류시켰다 — Tiny Creatures의 전사 4장과 합쳐 총 **13장의 실제 인간형 타일 풀**을
  캐릭터 20명 + 사람형 몬스터 4종(청주병/단양병/해적/호표기) = 24슬롯에 배정했다.
- **13장으로 24슬롯을 다 채울 수 없어서, 풀을 다 쓴 뒤로는 같은 실제 타일을 그
  캐릭터의 `tintColor`로 곱연산 틴트해 재사용**했다 — leather_armor/jade_ornament
  때 이미 쓴 것과 동일한 기법(CC0가 명시적으로 허용하는 파생 작업). 빈 캔버스에
  새로 그린 도형은 하나도 없다 — 전부 위 두 팩의 실제 PNG에서 시작했다.
- `Assets/Resources/Sprites/Samguk/Characters/char_*.png`(20장 신규) /
  `Assets/Resources/Sprites/Samguk/Monsters/monster_*.png`(8장 신규)로 저장,
  각 ScriptableObject의 `icon` 필드에 연결.

### Generic Fantasy RPG Items — 기존 팩에서 무기/방어구/장신구 추가 크롭

- Source/URL/License: 위 §Item 섹션과 동일(이미 확보된 팩, `items_28.png`/`item2.png`)
- 기존엔 검(2종)·활·흉갑·반지만 크롭해 썼는데, 이번에 같은 시트에서 실제로 존재하는
  나머지 아이콘을 더 크롭했다 — 철퇴(`items_28.png` 격자 (0,2)), 투구(`item2.png`
  격자 (2,0)-(3,1)), 방패(`item2.png` 격자 (2,2)-(3,3)), 두루마리(`items_28.png`
  격자 (2,2)), 열쇠(`items_28.png` 격자 (2,3)) — 전부 이미 시트 안에 있던 실제
  그림이고 새로 그린 게 아니다.
- 무기 4종(검-철/검-금장식/철퇴/활)·방어구 3종(흉갑/투구/방패)·장신구 3종(반지/
  두루마리/열쇠), 총 10장의 실제 아이콘을 기반으로 540개 아이템(캐릭터 30명 ×
  슬롯 3 × 등급 6)을 채웠다. 어느 캐릭터의 어느 슬롯이 어떤 실루엣을 쓸지는
  `(ownerCharacterId, slot)` 해시로 결정해서, 같은 무기 조합 체인(등급1→6)은
  항상 같은 실루엣을 쓰고(강화되는 무기라는 느낌), 등급색(`GradeColor`, 인벤토리
  UI가 이미 쓰는 그 6단계 회/초록/파랑/보라/금/적주홍)을 강도 55~80%(등급이
  높을수록 진하게)로 곱연산 틴트했다 — `leather_armor.png`가 이미 쓴 것과
  완전히 같은 파생 기법을 그대로 540배 확장 적용한 것뿐이다.
- `Assets/Resources/Sprites/Samguk/Items/{owner}_{slot}_g{grade}.png`(540장 신규)로
  저장, 각 `SamgukItemData.icon`에 연결.

### 검증

라이브 리플렉션으로 캐릭터 30/30·몬스터 16/16·아이템 540/540 전부 `icon != null`
확인, 실제 Play 모드 로비 화면을 `capture_game_view`로 캡처해 새 아이콘(공도/한현/
간옹 등)이 정상 렌더링되는 것까지 육안으로 재확인했다. 콘솔 에러/예외 0건.

---

## Effects / Background — 미착수

전투 이펙트(공격/피격/사망)와 배경(로비/전투 배경화면)은 이번 패스에서 손대지
않았다. 현재 전투 이펙트는 `SamgukProjectile.cs`가 그리는 단순 원형 발사체뿐이고,
배경은 단색(`ColorBg`)이다. 리소스 서치는 실행했지만(§10) 실제 다운로드·적용까지는
이번 범위에 못 넣었다 — 다음 패스 과제.

---

## 2026-09-16 추가 (2) — 캐릭터 초상 전면 교체: 사용자 제공 Dynasty Kingdoms Pack

**주의: 이 섹션이 위 "Character" 섹션(Kenney Tiny Dungeon 기반)을 대체한다.**
캐릭터 30명 전원의 `icon`이 이제 아래 팩으로 배정돼 있다 — 위쪽 Kenney Tiny Dungeon
Character 섹션에 적힌 타일 인덱스 매핑은 더 이상 유효하지 않다(단, Monster 섹션의
Kenney 사용은 그대로 유지되므로 그 섹션은 안 바뀜).

### Dynasty Kingdoms Pack 1 (QianGuo) / Pack 2 (Lianzhou) — 사용자 직접 제공

- Source: 사용자가 프로젝트에 직접 추가(`Assets/Art/Dynasty_Kingdoms_Pack_1`,
  `Assets/Art/Dynasty_Kingdoms_Pack_2_Lianzhou`, 사용 후 삭제됨 — 아래 참고)
- License: **동봉된 라이선스 파일 없음 — Claude가 독립적으로 검증하지 않았다.**
  사용자가 직접 보유·제공한 에셋이라 그대로 신뢰하고 사용했다. 상업 배포 전에는
  사용자가 직접 라이선스 조건을 재확인할 것.
- File Type: PNG, 64×64px 개별 파일(Pack1 16장 `QianGuo_Warrior_00~15`, Pack2
  15장 `LianZhou_Warrior_00~14`). `Large_*` 폴더는 같은 그림의 4배 업스케일
  사본(픽셀 대조로 완전 동일 확인) — 원본 64×64만 채택.
- Usage: 캐릭터 30명 전원에 1:1로 고유 배정(성급이 높을수록 두 팩의 "화려한"
  뒤쪽 인덱스부터 배정, 두 팩을 성급 구간마다 번갈아 섞음). 남은 1장(QianGuo)은
  미사용. 상세 배정 로직·근거는 `ssam.md` §96.11 참고.
  `Assets/Resources/Sprites/Samguk/Characters/char_*.png`(30장)로 저장,
  `SamgukCharacterData.icon`에 연결. TextureImporter: `spritePixelsPerUnit=64`,
  `filterMode=Point`, `spriteImportMode=Single`(과거 이 경로의 파일들이 Multiple
  모드였던 잔재를 명시적으로 초기화 — `ssam.md` §96.11의 함정 기록 참고).
- **원본 팩 폴더는 사용 후 완전히 삭제했다** — 사용자의 명시적 지시("활용할것만
  Resources 밑으로, 나머지는 삭제")에 따라 이번 건에 한해 이 프로젝트의 통상
  관례(Assets/Art에 원본 보관)를 따르지 않았다. `Assets/Art/Dynasty_Kingdoms_Pack_1`,
  `Assets/Art/Dynasty_Kingdoms_Pack_2_Lianzhou` 둘 다 현재 존재하지 않는다.
