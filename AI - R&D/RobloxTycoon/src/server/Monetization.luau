-- 게임패스(2배 수익)와 개발자 상품(현금 팩) 처리.
-- 현금 팩은 저장에 성공한 뒤에만 지급 완료를 알린다. 실패하면 Roblox가 나중에 다시 시도한다.

local MarketplaceService = game:GetService("MarketplaceService")
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Config = require(ReplicatedStorage.Shared.Config)
local Economy = require(ReplicatedStorage.Shared.Economy)
local Format = require(ReplicatedStorage.Shared.Format)
local PlayerData = require(script.Parent.PlayerData)

local Monetization = {}

local notify = nil

local function processReceipt(receipt)
	local player = Players:GetPlayerByUserId(receipt.PlayerId)
	local data = player and PlayerData.get(player)
	if not data then
		return Enum.ProductPurchaseDecision.NotProcessedYet
	end
	if PlayerData.hasReceipt(player, receipt.PurchaseId) then
		return Enum.ProductPurchaseDecision.PurchaseGranted
	end

	if receipt.ProductId ~= Config.ProductIds.CashPack then
		warn("[Monetization] Unknown product", receipt.ProductId)
		return Enum.ProductPurchaseDecision.NotProcessedYet
	end

	local amount = Economy.cashPackAmount(Config.CashPackBaseAmount, data.Rebirths)
	PlayerData.addCash(player, amount)
	PlayerData.addReceipt(player, receipt.PurchaseId)
	if not PlayerData.save(player) then
		PlayerData.addCash(player, -amount)
		PlayerData.removeReceipt(player, receipt.PurchaseId)
		return Enum.ProductPurchaseDecision.NotProcessedYet
	end

	notify:FireClient(player, "+" .. Format.cash(amount) .. " Cash Pack!", "gold")
	return Enum.ProductPurchaseDecision.PurchaseGranted
end

function Monetization.init(notifyRemote: RemoteEvent)
	notify = notifyRemote
	MarketplaceService.ProcessReceipt = processReceipt

	MarketplaceService.PromptGamePassPurchaseFinished:Connect(function(player, passId, purchased)
		if purchased and passId == Config.GamePassIds.DoubleCash then
			PlayerData.setDoubleCash(player, true)
			notify:FireClient(player, "2x Cash unlocked!", "gold")
		end
	end)
end

function Monetization.onPlayerLoaded(player: Player)
	local passId = Config.GamePassIds.DoubleCash
	if passId == 0 then
		return
	end
	local ok, owns = pcall(MarketplaceService.UserOwnsGamePassAsync, MarketplaceService, player.UserId, passId)
	if ok and owns then
		PlayerData.setDoubleCash(player, true)
	end
end

return Monetization
