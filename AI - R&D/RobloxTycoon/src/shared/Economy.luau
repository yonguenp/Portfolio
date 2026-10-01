-- 수익 배수, 구매 조건, 초당 수익 계산. Roblox API에 의존하지 않는 순수 함수만 둔다.

local Economy = {}

function Economy.multiplier(rebirths: number, hasDoubleCash: boolean, rebirthBonus: number): number
	local m = 1 + rebirths * rebirthBonus
	if hasDoubleCash then
		m *= 2
	end
	return m
end

function Economy.isAvailable(item, owned): boolean
	if owned[item.id] then
		return false
	end
	return item.requires == nil or owned[item.requires] == true
end

function Economy.ownsAll(items, owned): boolean
	for _, item in items do
		if not owned[item.id] then
			return false
		end
	end
	return true
end

-- 다음에 살 수 있는 아이템 중 가장 싼 것.
function Economy.nextItem(items, owned)
	local best = nil
	for _, item in items do
		if Economy.isAvailable(item, owned) and (best == nil or item.price < best.price) then
			best = item
		end
	end
	return best
end

-- 모든 업그레이더가 모든 드로퍼의 하류에 있으므로 배수는 전체 알에 곱해진다.
function Economy.incomePerSecond(items, owned, multiplier: number): number
	local base = 0
	local upgrade = 1
	for _, item in items do
		if owned[item.id] then
			if item.kind == "Dropper" then
				base += item.value / item.interval
			elseif item.kind == "Upgrader" then
				upgrade *= item.multiplier
			end
		end
	end
	return base * upgrade * multiplier
end

-- 알 하나를 수거했을 때 받는 금액. 최소 1.
function Economy.eggPayout(eggValue: number, multiplier: number): number
	return math.max(1, math.floor(eggValue * multiplier))
end

function Economy.cashPackAmount(base: number, rebirths: number): number
	return base * (1 + rebirths)
end

return Economy
