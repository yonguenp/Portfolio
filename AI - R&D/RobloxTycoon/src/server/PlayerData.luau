-- 플레이어 저장 데이터와 세션 상태(leaderstats, 속성)를 관리한다.
-- 로드에 실패한 플레이어는 저장하지 않는다. 빈 데이터로 기존 기록을 덮어쓰지 않기 위해서다.

local DataStoreService = game:GetService("DataStoreService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local RunService = game:GetService("RunService")

local Config = require(ReplicatedStorage.Shared.Config)
local Economy = require(ReplicatedStorage.Shared.Economy)

local MAX_RECEIPTS = 50

local PlayerData = {}

local store = nil
do
	local ok, result = pcall(function()
		return DataStoreService:GetDataStore(Config.DataStoreName)
	end)
	if ok then
		store = result
	else
		warn("[PlayerData] DataStore unavailable:", result)
	end
end

-- [Player] = { data = table, canSave = boolean, hasDoubleCash = boolean }
local sessions = {}

local function template()
	return {
		Cash = Config.StartingCash,
		Rebirths = 0,
		Owned = {},
		Receipts = {},
	}
end

local function reconcile(saved)
	local data = template()
	if type(saved) == "table" then
		for key, value in saved do
			if data[key] ~= nil and type(value) == type(data[key]) then
				data[key] = value
			end
		end
	end
	return data
end

local function keyFor(player: Player): string
	return "player_" .. player.UserId
end

local function withRetries(label: string, fn)
	for attempt = 1, 3 do
		local ok, result = pcall(fn)
		if ok then
			return true, result
		end
		warn(string.format("[PlayerData] %s failed (attempt %d): %s", label, attempt, tostring(result)))
		-- Studio에서 API 접근이 꺼져 있으면 재시도해도 소용없다.
		if RunService:IsStudio() and string.find(tostring(result), "Studio") then
			break
		end
		if attempt < 3 then
			task.wait(attempt * 2)
		end
	end
	return false, nil
end

local function createLeaderstats(player: Player, data)
	local folder = Instance.new("Folder")
	folder.Name = "leaderstats"

	local cash = Instance.new("IntValue")
	cash.Name = "Cash"
	cash.Value = data.Cash
	cash.Parent = folder

	local rebirths = Instance.new("IntValue")
	rebirths.Name = "Rebirths"
	rebirths.Value = data.Rebirths
	rebirths.Parent = folder

	folder.Parent = player
end

-- 데이터를 불러와 세션을 만든다. 실패하면 nil을 반환하고 호출자가 플레이어를 내보낸다.
function PlayerData.load(player: Player)
	local data
	local canSave = true

	if store then
		local ok, saved = withRetries("load " .. player.Name, function()
			return store:GetAsync(keyFor(player))
		end)
		if ok then
			data = reconcile(saved)
		elseif RunService:IsStudio() then
			warn("[PlayerData] Studio test without saving. Enable 'Studio Access to API Services' to test saves.")
			data = template()
			canSave = false
		else
			return nil
		end
	elseif RunService:IsStudio() then
		data = template()
		canSave = false
	else
		return nil
	end

	if not player.Parent then
		return nil -- 로드 중에 나간 경우
	end

	sessions[player] = { data = data, canSave = canSave, hasDoubleCash = false }
	createLeaderstats(player, data)
	PlayerData.refresh(player)
	return data
end

function PlayerData.get(player: Player)
	local session = sessions[player]
	return session and session.data
end

function PlayerData.multiplier(player: Player): number
	local session = sessions[player]
	if not session then
		return 1
	end
	return Economy.multiplier(session.data.Rebirths, session.hasDoubleCash, Config.RebirthBonus)
end

function PlayerData.setDoubleCash(player: Player, value: boolean)
	local session = sessions[player]
	if session then
		session.hasDoubleCash = value
		PlayerData.refresh(player)
	end
end

-- leaderstats와 클라이언트 HUD가 읽는 속성을 데이터에 맞춰 갱신한다.
function PlayerData.refresh(player: Player)
	local session = sessions[player]
	if not session then
		return
	end
	local data = session.data
	local stats = player:FindFirstChild("leaderstats")
	if stats then
		stats.Cash.Value = data.Cash
		stats.Rebirths.Value = data.Rebirths
	end

	local multiplier = PlayerData.multiplier(player)
	local nextItem = Economy.nextItem(Config.Items, data.Owned)
	player:SetAttribute("Multiplier", multiplier)
	player:SetAttribute("HasDoubleCash", session.hasDoubleCash)
	player:SetAttribute("CanRebirth", Economy.ownsAll(Config.Items, data.Owned))
	player:SetAttribute("Income", Economy.incomePerSecond(Config.Items, data.Owned, multiplier))
	player:SetAttribute("NextItemName", nextItem and nextItem.name or "")
	player:SetAttribute("NextItemPrice", nextItem and nextItem.price or 0)
end

function PlayerData.addCash(player: Player, amount: number)
	local data = PlayerData.get(player)
	if not data then
		return
	end
	data.Cash = math.max(0, data.Cash + amount)
	local stats = player:FindFirstChild("leaderstats")
	if stats then
		stats.Cash.Value = data.Cash
	end
end

function PlayerData.hasReceipt(player: Player, purchaseId: string): boolean
	local data = PlayerData.get(player)
	return data ~= nil and table.find(data.Receipts, purchaseId) ~= nil
end

function PlayerData.addReceipt(player: Player, purchaseId: string)
	local data = PlayerData.get(player)
	if not data then
		return
	end
	table.insert(data.Receipts, purchaseId)
	while #data.Receipts > MAX_RECEIPTS do
		table.remove(data.Receipts, 1)
	end
end

function PlayerData.removeReceipt(player: Player, purchaseId: string)
	local data = PlayerData.get(player)
	local index = data and table.find(data.Receipts, purchaseId)
	if index then
		table.remove(data.Receipts, index)
	end
end

-- 저장 성공 여부를 반환한다. 저장이 꺼진 Studio 세션은 성공으로 취급한다.
function PlayerData.save(player: Player): boolean
	local session = sessions[player]
	if not session then
		return false
	end
	if not session.canSave then
		return true
	end
	local ok = withRetries("save " .. player.Name, function()
		store:UpdateAsync(keyFor(player), function()
			return session.data
		end)
	end)
	return ok
end

function PlayerData.release(player: Player)
	if sessions[player] then
		PlayerData.save(player)
		sessions[player] = nil
	end
end

function PlayerData.saveAll()
	local pending = 0
	for player in sessions do
		pending += 1
		task.spawn(function()
			PlayerData.save(player)
			pending -= 1
		end)
	end
	local deadline = os.clock() + 25
	while pending > 0 and os.clock() < deadline do
		task.wait(0.1)
	end
end

function PlayerData.players()
	local list = {}
	for player in sessions do
		table.insert(list, player)
	end
	return list
end

return PlayerData
