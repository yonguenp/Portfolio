# Dragon Egg Tycoon (로블록스 타이쿤)

드래곤 알을 생산하는 둥지를 사서 수익을 늘리고, 전부 지으면 환생해 수익 배수를 올리는 작은 타이쿤 게임입니다.
맵과 구조물은 모두 코드로 생성되므로 Studio에서 직접 배치할 것이 없습니다.

## 게임 흐름

1. 입장하면 빈 플롯(최대 6개)을 자동으로 배정받습니다.
2. 초록 버튼을 밟아 둥지(Dropper)를 삽니다. 둥지에서 떨어진 알이 컨베이어를 타고 **SELL** 상자에 들어가면 돈이 됩니다.
3. 램프·브레스(Upgrader)를 지나간 알은 가치가 1.5배·2배가 됩니다.
4. 9개를 모두 지으면 **Rebirth** 버튼이 열립니다. 환생하면 진행은 초기화되고 수익이 회당 +50% 늘어납니다.
5. 진행 상황은 DataStore에 저장됩니다(60초마다 자동 저장, 퇴장 시, 서버 종료 시).

밸런스 시뮬레이션 기준으로 첫 환생까지 약 20분, 환생할수록 짧아집니다(`tests/run.luau` 출력 참고).

## 바로 열어 보기

1. Roblox Studio에서 **`DragonEggTycoon.rbxlx`** 를 엽니다.
2. **Play** 를 눌러 테스트합니다.
   - Studio에서 저장까지 테스트하려면 먼저 게임을 게시하고 **Game Settings → Security → Enable Studio Access to API Services** 를 켜야 합니다. 꺼져 있으면 저장 없이 플레이됩니다.

## 출시하기

1. **File → Publish to Roblox** 로 새 게임을 게시합니다.
2. **Game Settings**
   - Security: *Enable Studio Access to API Services* 켜기 (DataStore 사용)
   - Places → 서버 최대 인원(Max Players): **6** (플롯 수와 같게)
3. 크리에이터 대시보드(create.roblox.com)에서
   - 아이콘·썸네일·설명 등록
   - **성숙도 설문(Maturity & Compliance Questionnaire)** 작성 — 작성해야 공개할 수 있습니다.
   - 공개 여부를 **Public** 으로 변경
4. (선택) 수익 상품 만들기
   - 게임패스 **2x Cash**, 개발자 상품 **Cash Pack** 을 만들고
   - 받은 ID를 `src/shared/Config.luau` 의 `GamePassIds.DoubleCash`, `ProductIds.CashPack` 에 넣습니다.
   - ID가 0이면 해당 버튼은 화면에 나오지 않습니다.
   - Studio에서 바로 고칠 경우 `ReplicatedStorage → Shared → Config` 를 수정해도 됩니다.

## 코드 구조

| 파일 | 역할 |
| --- | --- |
| `src/shared/Config.luau` | 아이템 가격·수익·위치, 상품 ID 등 모든 설정값 |
| `src/shared/Economy.luau` | 수익 배수, 구매 조건, 초당 수익 계산 (순수 함수) |
| `src/shared/Format.luau` | `$1.2K` 같은 숫자 표시 |
| `src/server/Main.server.luau` | 서버 진입점: 플롯 배정, 저장 주기, 환생 요청 |
| `src/server/Tycoon.luau` | 플롯 하나의 구매·알 생산·수거·환생 |
| `src/server/MapBuilder.luau` | 맵, 플롯, 버튼, 둥지 모델을 코드로 생성 |
| `src/server/PlayerData.luau` | DataStore 로드·저장, leaderstats, HUD용 속성 |
| `src/server/Monetization.luau` | 게임패스와 개발자 상품 영수증 처리 |
| `src/client/Hud.client.luau` | 현금·초당 수익·다음 목표·환생·상점 UI |

설계 원칙
- 구매, 수익, 환생 판정은 모두 서버에서 합니다. 클라이언트는 표시만 합니다.
- 로드에 실패한 데이터는 저장하지 않아 기존 기록을 빈 값으로 덮어쓰지 않습니다.
- 현금 팩은 저장에 성공한 뒤에만 지급 완료를 알립니다(중복 지급 방지용 영수증 기록 포함).

## 수정 후 다시 빌드하기 (Rojo)

```bash
rojo build default.project.json -o DragonEggTycoon.rbxlx   # 파일로 빌드
rojo serve                                                  # Studio Rojo 플러그인과 실시간 동기화
luau tests/run.luau                                         # 로직 테스트와 밸런스 시뮬레이션
```

밸런스를 바꿀 때는 `Config.Items` 의 `price`, `value`, `multiplier` 를 고친 뒤 테스트를 돌려 첫 환생까지 걸리는 시간을 확인하세요.
