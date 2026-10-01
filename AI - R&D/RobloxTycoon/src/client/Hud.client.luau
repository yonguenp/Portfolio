-- 화면 UI: 보유 현금, 초당 수익, 다음 목표, 환생 버튼, 상점 버튼, 알림.
-- 표시만 담당하고 값은 서버가 넣어 주는 leaderstats와 플레이어 속성에서 읽는다.

local MarketplaceService = game:GetService("MarketplaceService")
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local TweenService = game:GetService("TweenService")

local Config = require(ReplicatedStorage.Shared.Config)
local Format = require(ReplicatedStorage.Shared.Format)

local player = Players.LocalPlayer
local remotes = ReplicatedStorage:WaitForChild("Remotes")
local notifyRemote = remotes:WaitForChild("Notify")
local rebirthRemote = remotes:WaitForChild("RequestRebirth")

local COLORS = {
	green = Color3.fromRGB(120, 230, 120),
	red = Color3.fromRGB(255, 110, 110),
	gold = Color3.fromRGB(255, 210, 80),
	panel = Color3.fromRGB(30, 30, 40),
	text = Color3.fromRGB(255, 255, 255),
	muted = Color3.fromRGB(180, 180, 190),
	disabled = Color3.fromRGB(90, 90, 100),
}

local gui = Instance.new("ScreenGui")
gui.Name = "Hud"
gui.ResetOnSpawn = false
gui.IgnoreGuiInset = false
gui.Parent = player:WaitForChild("PlayerGui")

local function round(instance: GuiObject, radius: number)
	local corner = Instance.new("UICorner")
	corner.CornerRadius = UDim.new(0, radius)
	corner.Parent = instance
end

local function stroke(instance: GuiObject)
	local s = Instance.new("UIStroke")
	s.Thickness = 2
	s.Color = Color3.fromRGB(0, 0, 0)
	s.Transparency = 0.5
	s.Parent = instance
end

local function text(parent: Instance, props)
	local l = Instance.new("TextLabel")
	l.BackgroundTransparency = 1
	l.Font = Enum.Font.FredokaOne
	l.TextScaled = true
	l.TextColor3 = COLORS.text
	for key, value in props do
		l[key] = value
	end
	l.Parent = parent
	return l
end

local function button(parent: Instance, props)
	local b = Instance.new("TextButton")
	b.Font = Enum.Font.FredokaOne
	b.TextScaled = true
	b.TextColor3 = COLORS.text
	b.AutoButtonColor = true
	for key, value in props do
		b[key] = value
	end
	round(b, 12)
	stroke(b)
	b.Parent = parent
	return b
end

-- 상단 현금 패널
local cashPanel = Instance.new("Frame")
cashPanel.AnchorPoint = Vector2.new(0.5, 0)
cashPanel.Position = UDim2.new(0.5, 0, 0, 8)
cashPanel.Size = UDim2.fromOffset(260, 92)
cashPanel.BackgroundColor3 = COLORS.panel
cashPanel.BackgroundTransparency = 0.25
round(cashPanel, 14)
stroke(cashPanel)
cashPanel.Parent = gui

local cashLabel = text(cashPanel, {
	Size = UDim2.new(1, -16, 0, 40),
	Position = UDim2.fromOffset(8, 4),
	TextColor3 = COLORS.green,
	Text = "$0",
})
local incomeLabel = text(cashPanel, {
	Size = UDim2.new(1, -16, 0, 20),
	Position = UDim2.fromOffset(8, 44),
	TextColor3 = COLORS.muted,
	Text = "",
})
local nextLabel = text(cashPanel, {
	Size = UDim2.new(1, -16, 0, 20),
	Position = UDim2.fromOffset(8, 66),
	TextColor3 = COLORS.gold,
	Text = "",
})

-- 알림
local toast = text(gui, {
	AnchorPoint = Vector2.new(0.5, 0),
	Position = UDim2.new(0.5, 0, 0, 110),
	Size = UDim2.fromOffset(420, 36),
	Text = "",
	TextTransparency = 1,
	TextStrokeTransparency = 1,
})

-- 오른쪽 아래 환생 버튼
local rebirthButton = button(gui, {
	AnchorPoint = Vector2.new(1, 1),
	Position = UDim2.new(1, -12, 1, -12),
	Size = UDim2.fromOffset(180, 56),
	Text = "Rebirth",
})

-- 왼쪽 상점 버튼. ID가 설정된 상품만 보인다.
local shop = Instance.new("Frame")
shop.BackgroundTransparency = 1
shop.AnchorPoint = Vector2.new(0, 0.5)
shop.Position = UDim2.new(0, 12, 0.5, 0)
shop.Size = UDim2.fromOffset(150, 124)
shop.Parent = gui
local shopLayout = Instance.new("UIListLayout")
shopLayout.Padding = UDim.new(0, 8)
shopLayout.Parent = shop

local doubleCashButton = nil
if Config.GamePassIds.DoubleCash ~= 0 then
	doubleCashButton = button(shop, {
		Size = UDim2.fromOffset(150, 52),
		BackgroundColor3 = Color3.fromRGB(200, 140, 30),
		Text = "2x Cash",
	})
	doubleCashButton.Activated:Connect(function()
		MarketplaceService:PromptGamePassPurchase(player, Config.GamePassIds.DoubleCash)
	end)
end

if Config.ProductIds.CashPack ~= 0 then
	local cashPackButton = button(shop, {
		Size = UDim2.fromOffset(150, 52),
		BackgroundColor3 = Color3.fromRGB(60, 160, 80),
		Text = "Cash Pack",
	})
	cashPackButton.Activated:Connect(function()
		MarketplaceService:PromptProductPurchase(player, Config.ProductIds.CashPack)
	end)
end

local toastTween = nil
local function showToast(message: string, colorName: string?)
	if toastTween then
		toastTween:Cancel()
	end
	toast.Text = message
	toast.TextColor3 = COLORS[colorName or "green"] or COLORS.green
	toast.TextTransparency = 0
	toast.TextStrokeTransparency = 0.3
	toastTween = TweenService:Create(
		toast,
		TweenInfo.new(0.6, Enum.EasingStyle.Quad, Enum.EasingDirection.In, 0, false, 2),
		{ TextTransparency = 1, TextStrokeTransparency = 1 }
	)
	toastTween:Play()
end

local function refreshStats()
	local multiplier = player:GetAttribute("Multiplier") or 1
	local income = player:GetAttribute("Income") or 0
	incomeLabel.Text = string.format("%s/s  ·  x%s", Format.cash(income), tostring(multiplier))

	local nextName = player:GetAttribute("NextItemName") or ""
	if nextName ~= "" then
		nextLabel.Text = "Next: " .. nextName .. " " .. Format.cash(player:GetAttribute("NextItemPrice") or 0)
	else
		nextLabel.Text = "All built! Rebirth for more"
	end

	local canRebirth = player:GetAttribute("CanRebirth") == true
	rebirthButton.BackgroundColor3 = if canRebirth then COLORS.gold else COLORS.disabled
	rebirthButton.Text = if canRebirth
		then string.format("Rebirth (+%d%%)", Config.RebirthBonus * 100)
		else "Rebirth (locked)"

	if doubleCashButton then
		doubleCashButton.Visible = player:GetAttribute("HasDoubleCash") ~= true
	end
end

rebirthButton.Activated:Connect(function()
	if player:GetAttribute("CanRebirth") == true then
		rebirthRemote:FireServer()
	else
		showToast("Build everything to unlock Rebirth", "red")
	end
end)

notifyRemote.OnClientEvent:Connect(showToast)

for _, attribute in { "Multiplier", "Income", "NextItemName", "NextItemPrice", "CanRebirth", "HasDoubleCash" } do
	player:GetAttributeChangedSignal(attribute):Connect(refreshStats)
end
refreshStats()

local leaderstats = player:WaitForChild("leaderstats")
local cashValue = leaderstats:WaitForChild("Cash")
local function refreshCash()
	cashLabel.Text = Format.cash(cashValue.Value)
end
cashValue.Changed:Connect(refreshCash)
refreshCash()
