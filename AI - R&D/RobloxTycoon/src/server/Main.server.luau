-- 서버 진입점: 맵 생성, 플레이어 입장/퇴장, 자동 저장, 환생 요청 처리.

local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Config = require(ReplicatedStorage.Shared.Config)
local MapBuilder = require(script.Parent.MapBuilder)
local Monetization = require(script.Parent.Monetization)
local PlayerData = require(script.Parent.PlayerData)
local Tycoon = require(script.Parent.Tycoon)

local remotes = Instance.new("Folder")
remotes.Name = "Remotes"
local notify = Instance.new("RemoteEvent")
notify.Name = "Notify"
notify.Parent = remotes
local requestRebirth = Instance.new("RemoteEvent")
requestRebirth.Name = "RequestRebirth"
requestRebirth.Parent = remotes
remotes.Parent = ReplicatedStorage

MapBuilder.buildWorld()

local tycoons = {}
for index, cframe in MapBuilder.plotCFrames(Config.PlotCount) do
	tycoons[index] = Tycoon.new(index, cframe, notify)
end

local tycoonOf = {} -- [Player] = Tycoon

Monetization.init(notify)

local function sendHome(player: Player, character: Model)
	local tycoon = tycoonOf[player]
	if not tycoon then
		return
	end
	-- 캐릭터가 기본 스폰 위치에 놓인 뒤에 옮겨야 다시 광장으로 되돌아가지 않는다.
	task.spawn(function()
		local root = character:WaitForChild("HumanoidRootPart", 10)
		task.wait()
		if root and character.Parent then
			character:PivotTo(tycoon:spawnCFrame())
		end
	end)
end

local function onPlayerAdded(player: Player)
	local data = PlayerData.load(player)
	if not data then
		if player.Parent then
			player:Kick("Could not load your save. Please rejoin in a moment.")
		end
		return
	end

	local tycoon = nil
	for _, candidate in tycoons do
		if candidate:isFree() then
			tycoon = candidate
			break
		end
	end
	if not tycoon then
		PlayerData.release(player)
		player:Kick("This server is full. Please join another server.")
		return
	end

	tycoonOf[player] = tycoon
	tycoon:claim(player)
	Monetization.onPlayerLoaded(player)

	player.CharacterAdded:Connect(function(character)
		sendHome(player, character)
	end)
	if player.Character then
		sendHome(player, player.Character)
	end
	notify:FireClient(player, "Welcome to " .. Config.GameName .. "!", "gold")
end

local function onPlayerRemoving(player: Player)
	local tycoon = tycoonOf[player]
	if tycoon then
		tycoon:release()
		tycoonOf[player] = nil
	end
	PlayerData.release(player)
end

Players.PlayerAdded:Connect(onPlayerAdded)
Players.PlayerRemoving:Connect(onPlayerRemoving)
for _, player in Players:GetPlayers() do
	task.spawn(onPlayerAdded, player)
end

requestRebirth.OnServerEvent:Connect(function(player)
	local tycoon = tycoonOf[player]
	if tycoon and tycoon:rebirth() then
		PlayerData.save(player)
	end
end)

task.spawn(function()
	while true do
		task.wait(Config.AutosaveInterval)
		for _, player in PlayerData.players() do
			task.spawn(PlayerData.save, player)
		end
	end
end)

game:BindToClose(function()
	PlayerData.saveAll()
end)
