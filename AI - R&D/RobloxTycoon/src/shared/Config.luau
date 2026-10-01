-- 게임 전체 밸런스와 설정값. 서버와 클라이언트가 함께 읽는다.
-- Roblox 전용 타입(Color3 등)을 쓰지 않아 luau CLI 테스트에서도 그대로 불러올 수 있다.

local Config = {}

Config.GameName = "Dragon Egg Tycoon"

-- 저장소 이름을 바꾸면 기존 저장 데이터와 분리된다.
Config.DataStoreName = "DragonEggTycoon_v1"
Config.AutosaveInterval = 60

Config.StartingCash = 20
Config.RebirthBonus = 0.5 -- 환생 1회당 수익 +50%

-- Roblox 크리에이터 대시보드에서 만든 ID를 넣는다. 0이면 해당 상품은 숨겨진다.
Config.GamePassIds = {
	DoubleCash = 0,
}
Config.ProductIds = {
	CashPack = 0,
}
Config.CashPackBaseAmount = 2500 -- 환생 횟수에 비례해 늘어난다.

Config.PlotCount = 6 -- 서버 최대 인원도 이 값에 맞춘다.
Config.BeltSpeed = 8
Config.EggLifetime = 20
Config.MaxEggsPerPlot = 60

-- kind: Dropper(알 생성) / Upgrader(지나가는 알 가치 배수) / Decor(최종 장식)
-- requires: 이 아이템을 산 뒤에야 구매 버튼이 나타난다.
-- x: 컨베이어 방향(plot 로컬 X축)으로의 설치 위치.
Config.Items = {
	{ id = "dropper1", name = "Egg Nest", kind = "Dropper", price = 20, value = 2, interval = 2, x = -21, color = { 245, 235, 210 } },
	{ id = "dropper2", name = "Twin Nest", kind = "Dropper", price = 60, value = 5, interval = 2, x = -15, color = { 140, 210, 120 }, requires = "dropper1" },
	{ id = "dropper3", name = "Ember Nest", kind = "Dropper", price = 300, value = 12, interval = 2, x = -9, color = { 240, 120, 60 }, requires = "dropper2" },
	{ id = "upgrader1", name = "Warm Lamp", kind = "Upgrader", price = 900, multiplier = 1.5, x = 13, color = { 255, 200, 80 }, requires = "dropper3" },
	{ id = "dropper4", name = "Frost Nest", kind = "Dropper", price = 1800, value = 30, interval = 2, x = -3, color = { 120, 200, 255 }, requires = "upgrader1" },
	{ id = "dropper5", name = "Storm Nest", kind = "Dropper", price = 6000, value = 70, interval = 2, x = 3, color = { 170, 120, 255 }, requires = "dropper4" },
	{ id = "upgrader2", name = "Dragon Breath", kind = "Upgrader", price = 15000, multiplier = 2, x = 17, color = { 255, 90, 90 }, requires = "dropper5" },
	{ id = "dropper6", name = "Royal Nest", kind = "Dropper", price = 40000, value = 180, interval = 2, x = 8, color = { 255, 215, 0 }, requires = "upgrader2" },
	{ id = "goldenNest", name = "Golden Dragon Statue", kind = "Decor", price = 120000, color = { 255, 200, 40 }, requires = "dropper6" },
}

return Config
