-- luau CLI로 실행하는 순수 로직 테스트와 밸런스 시뮬레이션.
-- 실행: luau tests/run.luau (프로젝트 루트에서)

local Config = require("../src/shared/Config")
local Economy = require("../src/shared/Economy")
local Format = require("../src/shared/Format")

local failures = 0
local function check(name: string, cond: boolean)
	if not cond then
		failures += 1
		print("FAIL  " .. name)
	end
end

-- Format
check("format small", Format.abbreviate(999) == "999")
check("format K", Format.abbreviate(1500) == "1.5K")
check("format exact K", Format.abbreviate(2000) == "2K")
check("format edge", Format.abbreviate(999999) == "999.9K")
check("format M", Format.abbreviate(3400000) == "3.4M")
check("format cash", Format.cash(120000) == "$120K")

-- Economy
check("multiplier base", Economy.multiplier(0, false, 0.5) == 1)
check("multiplier rebirth+pass", Economy.multiplier(2, true, 0.5) == 4)
check("payout min 1", Economy.eggPayout(0.2, 1) == 1)
check("payout floor", Economy.eggPayout(7.5, 1.5) == 11)

local owned = {}
check("first item available", Economy.isAvailable(Config.Items[1], owned))
check("second item locked", not Economy.isAvailable(Config.Items[2], owned))
owned[Config.Items[1].id] = true
check("owned not available", not Economy.isAvailable(Config.Items[1], owned))
check("second unlocked", Economy.isAvailable(Config.Items[2], owned))
check("not all owned", not Economy.ownsAll(Config.Items, owned))

-- Config 무결성: id 중복 없음, requires가 앞선 아이템을 가리킴, 모든 아이템에 도달 가능.
local seen = {}
for i, item in Config.Items do
	check("unique id " .. item.id, not seen[item.id])
	if item.requires then
		check("requires earlier " .. item.id, seen[item.requires] == true)
	end
	seen[item.id] = true
	if item.kind == "Dropper" then
		check("dropper fields " .. item.id, item.value > 0 and item.interval > 0 and item.x ~= nil)
	elseif item.kind == "Upgrader" then
		check("upgrader fields " .. item.id, item.multiplier > 1 and item.x ~= nil)
	end
end
-- 업그레이더가 모든 드로퍼보다 하류(컨베이어 끝 쪽)에 있어야 incomePerSecond 계산이 맞다.
local maxDropperX, minUpgraderX = -math.huge, math.huge
for _, item in Config.Items do
	if item.kind == "Dropper" then
		maxDropperX = math.max(maxDropperX, item.x)
	elseif item.kind == "Upgrader" then
		minUpgraderX = math.min(minUpgraderX, item.x)
	end
end
check("upgraders downstream of droppers", minUpgraderX > maxDropperX + 2)

-- 밸런스 시뮬레이션: 살 수 있게 되면 바로 사는 플레이어 기준 소요 시간.
local function simulate(rebirths: number): number
	local cash = Config.StartingCash
	local have = {}
	local t = 0
	local mult = Economy.multiplier(rebirths, false, Config.RebirthBonus)
	while not Economy.ownsAll(Config.Items, have) do
		local nextItem = Economy.nextItem(Config.Items, have)
		local income = Economy.incomePerSecond(Config.Items, have, mult)
		if cash < nextItem.price then
			if income <= 0 then
				return math.huge
			end
			local wait = (nextItem.price - cash) / income
			t += wait
			cash += wait * income
		end
		cash -= nextItem.price
		have[nextItem.id] = true
		if rebirths == 0 then
			print(string.format("  %6.1f min  %-22s %s", t / 60, nextItem.name, Format.cash(nextItem.price)))
		end
	end
	return t
end

print("Progression (first run):")
local first = simulate(0)
check("first rebirth reachable", first < math.huge)
check("first rebirth between 10 and 60 minutes", first > 600 and first < 3600)
for r = 1, 3 do
	print(string.format("Run after %d rebirth(s): %.1f min", r, simulate(r) / 60))
end

if failures > 0 then
	print(string.format("%d check(s) failed", failures))
	os.exit(1)
end
print("All checks passed")
