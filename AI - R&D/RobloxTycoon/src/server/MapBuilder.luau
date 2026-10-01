-- 맵과 타이쿤 구조물을 코드로 생성한다. Studio에서 직접 배치할 것이 없도록 하기 위해서다.
-- 플롯 로컬 좌표: X축 = 컨베이어 진행 방향, +Z = 스폰/광장 방향, 바닥 윗면 y = 0.5.

local ReplicatedStorage = game:GetService("ReplicatedStorage")

local Config = require(ReplicatedStorage.Shared.Config)
local Format = require(ReplicatedStorage.Shared.Format)

local MapBuilder = {}

local PLOT_SIZE = 64
local BUTTON_ROW_Z = -16
local BUTTON_SPACING = 6

local function rgb(c)
	return Color3.fromRGB(c[1], c[2], c[3])
end

local function part(props)
	local p = Instance.new("Part")
	p.Anchored = true
	p.TopSurface = Enum.SurfaceType.Smooth
	p.BottomSurface = Enum.SurfaceType.Smooth
	for key, value in props do
		if key ~= "Parent" then
			p[key] = value
		end
	end
	p.Parent = props.Parent
	return p
end

local function billboard(parent: Instance, size: UDim2, offsetY: number)
	local gui = Instance.new("BillboardGui")
	gui.Size = size
	gui.StudsOffset = Vector3.new(0, offsetY, 0)
	gui.AlwaysOnTop = false
	gui.MaxDistance = 80
	gui.Parent = parent
	return gui
end

local function label(parent: Instance, text: string, size: UDim2, position: UDim2, color: Color3)
	local l = Instance.new("TextLabel")
	l.BackgroundTransparency = 1
	l.Size = size
	l.Position = position
	l.Font = Enum.Font.FredokaOne
	l.TextScaled = true
	l.TextColor3 = color
	l.TextStrokeTransparency = 0.3
	l.Text = text
	l.Parent = parent
	return l
end

function MapBuilder.buildWorld()
	local world = Instance.new("Folder")
	world.Name = "World"

	part({
		Name = "Ground",
		Size = Vector3.new(260, 1, 240),
		CFrame = CFrame.new(0, -0.5, 0),
		Material = Enum.Material.Grass,
		Color = Color3.fromRGB(96, 160, 80),
		Parent = world,
	})

	local spawn = Instance.new("SpawnLocation")
	spawn.Anchored = true
	spawn.Size = Vector3.new(10, 1, 10)
	spawn.CFrame = CFrame.new(0, 0.5, 0)
	spawn.Material = Enum.Material.Slate
	spawn.Color = Color3.fromRGB(200, 170, 120)
	spawn.Neutral = true
	spawn.Duration = 0
	spawn.Parent = world

	world.Parent = workspace
	return world
end

-- 플롯 6개를 광장을 바라보도록 두 줄로 배치한다.
function MapBuilder.plotCFrames(count: number)
	local cframes = {}
	local perRow = math.ceil(count / 2)
	for i = 1, count do
		local row = if i <= perRow then 0 else 1
		local col = (i - 1) % perRow
		local x = (col - (perRow - 1) / 2) * (PLOT_SIZE + 8)
		if row == 0 then
			table.insert(cframes, CFrame.new(x, 0, -50))
		else
			table.insert(cframes, CFrame.new(x, 0, 50) * CFrame.Angles(0, math.pi, 0))
		end
	end
	return cframes
end

function MapBuilder.buildPlot(index: number, cf: CFrame)
	local model = Instance.new("Model")
	model.Name = "Plot" .. index

	local function at(x, y, z)
		return cf * CFrame.new(x, y, z)
	end

	part({
		Name = "Floor",
		Size = Vector3.new(PLOT_SIZE, 1, PLOT_SIZE),
		CFrame = at(0, 0, 0),
		Material = Enum.Material.WoodPlanks,
		Color = Color3.fromRGB(160, 120, 80),
		Parent = model,
	})

	local belt = part({
		Name = "Belt",
		Size = Vector3.new(44, 1, 6),
		CFrame = at(-2, 1, 0),
		Material = Enum.Material.DiamondPlate,
		Color = Color3.fromRGB(60, 60, 70),
		Parent = model,
	})
	-- 고정된 파트에 속도를 주면 위에 놓인 물체가 그 방향으로 실려 간다.
	belt.AssemblyLinearVelocity = belt.CFrame.RightVector * Config.BeltSpeed

	for _, side in { -1, 1 } do
		part({
			Name = "Rail",
			Size = Vector3.new(44, 2, 0.5),
			CFrame = at(-2, 2, side * 3.25),
			Material = Enum.Material.Metal,
			Color = Color3.fromRGB(120, 120, 130),
			Parent = model,
		})
	end
	part({
		Name = "BeltStop",
		Size = Vector3.new(0.5, 2, 6),
		CFrame = at(-24.25, 2, 0),
		Material = Enum.Material.Metal,
		Color = Color3.fromRGB(120, 120, 130),
		Parent = model,
	})

	local collector = part({
		Name = "Collector",
		Size = Vector3.new(6, 4, 8),
		CFrame = at(23, 2, 0),
		Material = Enum.Material.Neon,
		Color = Color3.fromRGB(80, 220, 120),
		Parent = model,
	})
	label(billboard(collector, UDim2.fromOffset(160, 40), 4), "SELL", UDim2.fromScale(1, 1), UDim2.new(), Color3.new(1, 1, 1))

	local post = part({
		Name = "SignPost",
		Size = Vector3.new(1, 8, 1),
		CFrame = at(-28, 4.5, 28),
		Material = Enum.Material.Wood,
		Color = Color3.fromRGB(110, 80, 50),
		Parent = model,
	})
	local ownerLabel = label(
		billboard(post, UDim2.fromOffset(240, 60), 5),
		"Empty Plot",
		UDim2.fromScale(1, 1),
		UDim2.new(),
		Color3.fromRGB(255, 240, 200)
	)

	local items = Instance.new("Folder")
	items.Name = "Items"
	items.Parent = model

	local buttons = Instance.new("Folder")
	buttons.Name = "Buttons"
	buttons.Parent = model

	local eggs = Instance.new("Folder")
	eggs.Name = "Eggs"
	eggs.Parent = model

	model.Parent = workspace

	return {
		model = model,
		cframe = cf,
		belt = belt,
		collector = collector,
		ownerLabel = ownerLabel,
		items = items,
		buttons = buttons,
		eggs = eggs,
		spawnCFrame = at(0, 4, 20),
	}
end

-- 아이템 순서대로 버튼 줄에 위치를 정한다.
function MapBuilder.buildButton(plot, item, order: number)
	local x = -24 + (order - 1) * BUTTON_SPACING
	local button = part({
		Name = "Button_" .. item.id,
		Size = Vector3.new(4, 0.6, 4),
		CFrame = plot.cframe * CFrame.new(x, 0.8, BUTTON_ROW_Z),
		Material = Enum.Material.Neon,
		Color = Color3.fromRGB(90, 200, 90),
		Parent = plot.buttons,
	})
	local gui = billboard(button, UDim2.fromOffset(180, 70), 3)
	label(gui, item.name, UDim2.fromScale(1, 0.55), UDim2.new(), Color3.new(1, 1, 1))
	label(gui, Format.cash(item.price), UDim2.fromScale(1, 0.45), UDim2.fromScale(0, 0.55), Color3.fromRGB(140, 255, 140))
	return button
end

-- 아이템 모델을 만들고, 드로퍼는 알이 떨어질 위치를, 업그레이더는 감지 영역을 함께 반환한다.
function MapBuilder.buildItem(plot, item)
	local model = Instance.new("Model")
	model.Name = item.id
	local color = rgb(item.color)
	local cf = plot.cframe
	local result = { model = model }

	if item.kind == "Dropper" then
		part({
			Name = "Body",
			Size = Vector3.new(4, 4, 4),
			CFrame = cf * CFrame.new(item.x, 8.5, 0),
			Material = Enum.Material.SmoothPlastic,
			Color = color,
			Parent = model,
		})
		part({
			Name = "Nozzle",
			Size = Vector3.new(2, 1.5, 2),
			CFrame = cf * CFrame.new(item.x, 5.75, 0),
			Material = Enum.Material.Metal,
			Color = Color3.fromRGB(80, 80, 90),
			Parent = model,
		})
		for _, side in { -1, 1 } do
			part({
				Name = "Leg",
				Size = Vector3.new(0.6, 10, 0.6),
				CFrame = cf * CFrame.new(item.x, 5.5, side * 4.2),
				Material = Enum.Material.Metal,
				Color = Color3.fromRGB(80, 80, 90),
				Parent = model,
			})
		end
		result.dropPosition = (cf * CFrame.new(item.x, 4, 0)).Position
	elseif item.kind == "Upgrader" then
		for _, side in { -1, 1 } do
			part({
				Name = "Pillar",
				Size = Vector3.new(1, 6, 1),
				CFrame = cf * CFrame.new(item.x, 3.5, side * 4),
				Material = Enum.Material.Metal,
				Color = Color3.fromRGB(70, 70, 80),
				Parent = model,
			})
		end
		part({
			Name = "Top",
			Size = Vector3.new(1.5, 1, 9),
			CFrame = cf * CFrame.new(item.x, 7, 0),
			Material = Enum.Material.Neon,
			Color = color,
			Parent = model,
		})
		result.zone = part({
			Name = "Zone",
			Size = Vector3.new(1, 4, 6),
			CFrame = cf * CFrame.new(item.x, 3.5, 0),
			Material = Enum.Material.ForceField,
			Color = color,
			Transparency = 0.4,
			CanCollide = false,
			Parent = model,
		})
		label(
			billboard(result.zone, UDim2.fromOffset(120, 36), 4),
			"x" .. item.multiplier,
			UDim2.fromScale(1, 1),
			UDim2.new(),
			color
		)
	elseif item.kind == "Decor" then
		part({
			Name = "Pedestal",
			Size = Vector3.new(8, 2, 8),
			CFrame = cf * CFrame.new(22, 1.5, 22),
			Material = Enum.Material.Marble,
			Color = Color3.fromRGB(230, 230, 230),
			Parent = model,
		})
		part({
			Name = "Egg",
			Shape = Enum.PartType.Ball,
			Size = Vector3.new(6, 6, 6),
			CFrame = cf * CFrame.new(22, 5.5, 22),
			Material = Enum.Material.Neon,
			Color = color,
			Parent = model,
		})
		local light = Instance.new("PointLight")
		light.Color = color
		light.Range = 20
		light.Brightness = 2
		light.Parent = model:FindFirstChild("Egg")
	end

	model.Parent = plot.items
	return result
end

function MapBuilder.buildEgg(plot, position: Vector3, color)
	return part({
		Name = "Egg",
		Shape = Enum.PartType.Ball,
		Size = Vector3.new(1.2, 1.2, 1.2),
		Position = position,
		Anchored = false,
		Material = Enum.Material.SmoothPlastic,
		Color = rgb(color),
		Parent = plot.eggs,
	})
end

return MapBuilder
