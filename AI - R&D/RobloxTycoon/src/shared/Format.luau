-- 큰 숫자를 1.2K, 3.4M 형태로 줄여 표시한다.

local Format = {}

local SUFFIXES = { "", "K", "M", "B", "T", "Qa", "Qi" }

function Format.abbreviate(n: number): string
	local sign = if n < 0 then "-" else ""
	n = math.abs(n)
	local tier = 1
	while n >= 1000 and tier < #SUFFIXES do
		n /= 1000
		tier += 1
	end
	if tier == 1 then
		return sign .. tostring(math.floor(n))
	end
	-- 반올림으로 999.95K가 "1000.0K"가 되지 않도록 소수 첫째 자리에서 내림한다.
	local text = string.format("%.1f", math.floor(n * 10) / 10)
	text = string.gsub(text, "%.0$", "")
	return sign .. text .. SUFFIXES[tier]
end

function Format.cash(n: number): string
	return "$" .. Format.abbreviate(n)
end

return Format
