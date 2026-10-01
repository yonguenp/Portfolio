-- 플롯 하나의 진행 상태: 주인, 구매한 아이템, 알 생산과 수거, 환생.
-- 모든 판정은 서버에서 하고 클라이언트는 결과만 본다.

local Debris = game:GetService("Debris")
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Config = require(ReplicatedStorage.Shared.Config)
local Economy = require(ReplicatedStorage.Shared.Economy)
local Format = require(ReplicatedStorage.Shared.Format)
local MapBuilder = require(script.Parent.MapBuilder)
local PlayerData = require(script.Parent.PlayerData)

local NOTIFY_COOLDOWN = 1.5

local Tycoon = {}
Tycoon.__index = Tycoon

function Tycoon.new(index: number, cframe: CFrame, notify)
	local self = setmetatable({}, Tycoon)
	self.index = index
	self.plot = MapBuilder.buildPlot(index, cframe)
	self.owner = nil
	self.notify = notify
	self.eggCount = 0
	self.lastNotify = 0

	self.plot.collector.Touched:Connect(function(hit)
		self:_collect(hit)
	end)
	return self
end

function Tycoon:isFree(): boolean
	return self.owner == nil
end

function Tycoon:claim(player: Player)
	self.owner = player
	self.plot.ownerLabel.Text = player.DisplayName .. "'s Tycoon"
	local data = PlayerData.get(player)
	for _, item in Config.Items do
		if data.Owned[item.id] then
			self:_buildItem(item)
		end
	end
	self:_refreshButtons()
end

function Tycoon:release()
	self.owner = nil
	self.plot.ownerLabel.Text = "Empty Plot"
	self:_clear()
end

function Tycoon:spawnCFrame(): CFrame
	return self.plot.spawnCFrame
end

-- 모든 아이템을 산 상태에서만 환생할 수 있다.
function Tycoon:rebirth(): boolean
	local player = self.owner
	local data = player and PlayerData.get(player)
	if not data or not Economy.ownsAll(Config.Items, data.Owned) then
		return false
	end
	data.Rebirths += 1
	data.Cash = Config.StartingCash
	data.Owned = {}
	self:_clear()
	self:_refreshButtons()
	PlayerData.refresh(player)
	self.notify:FireClient(
		player,
		string.format("Rebirth #%d! Income x%s", data.Rebirths, tostring(PlayerData.multiplier(player))),
		"gold"
	)
	return true
end

function Tycoon:_clear()
	self.plot.items:ClearAllChildren()
	self.plot.buttons:ClearAllChildren()
	self.plot.eggs:ClearAllChildren()
end

function Tycoon:_refreshButtons()
	self.plot.buttons:ClearAllChildren()
	local player = self.owner
	local data = player and PlayerData.get(player)
	if not data then
		return
	end
	for order, item in Config.Items do
		if Economy.isAvailable(item, data.Owned) then
			local button = MapBuilder.buildButton(self.plot, item, order)
			button.Touched:Connect(function(hit)
				local toucher = Players:GetPlayerFromCharacter(hit.Parent)
				if toucher and toucher == self.owner then
					self:_purchase(toucher, item)
				end
			end)
		end
	end
end

function Tycoon:_purchase(player: Player, item)
	local data = PlayerData.get(player)
	if not data or not Economy.isAvailable(item, data.Owned) then
		return
	end
	if data.Cash < item.price then
		local now = os.clock()
		if now - self.lastNotify > NOTIFY_COOLDOWN then
			self.lastNotify = now
			self.notify:FireClient(player, "Need " .. Format.cash(item.price - data.Cash) .. " more", "red")
		end
		return
	end

	-- 같은 프레임에 Touched가 여러 번 와도 위의 isAvailable 검사에서 두 번째부터 걸러진다.
	data.Cash -= item.price
	data.Owned[item.id] = true
	self:_buildItem(item)
	self:_refreshButtons()
	PlayerData.refresh(player)
	self.notify:FireClient(player, "Built " .. item.name .. "!", "green")
end

function Tycoon:_buildItem(item)
	local built = MapBuilder.buildItem(self.plot, item)
	if item.kind == "Dropper" then
		local model = built.model
		task.spawn(function()
			while model.Parent do
				task.wait(item.interval)
				if not model.Parent then
					break
				end
				self:_spawnEgg(item, built.dropPosition)
			end
		end)
	elseif item.kind == "Upgrader" then
		local flag = "Up_" .. item.id
		built.zone.Touched:Connect(function(hit)
			if hit.Name == "Egg" and hit:GetAttribute("Plot") == self.index and not hit:GetAttribute(flag) then
				hit:SetAttribute(flag, true)
				hit:SetAttribute("Value", hit:GetAttribute("Value") * item.multiplier)
				hit.Material = Enum.Material.Neon
			end
		end)
	end
end

function Tycoon:_spawnEgg(item, position: Vector3)
	if self.eggCount >= Config.MaxEggsPerPlot then
		return
	end
	local egg = MapBuilder.buildEgg(self.plot, position, item.color)
	egg:SetAttribute("Plot", self.index)
	egg:SetAttribute("Value", item.value)
	-- 물리 계산을 서버가 맡아 클라이언트가 알을 조작하지 못하게 한다.
	egg:SetNetworkOwner(nil)
	self.eggCount += 1
	egg.Destroying:Connect(function()
		self.eggCount -= 1
	end)
	Debris:AddItem(egg, Config.EggLifetime)
end

function Tycoon:_collect(hit: BasePart)
	if hit.Name ~= "Egg" or hit:GetAttribute("Plot") ~= self.index or hit:GetAttribute("Collected") then
		return
	end
	hit:SetAttribute("Collected", true)
	local value = hit:GetAttribute("Value")
	hit:Destroy()

	local player = self.owner
	if player then
		PlayerData.addCash(player, Economy.eggPayout(value, PlayerData.multiplier(player)))
	end
end

return Tycoon
