# -*- coding: utf-8 -*-
import struct, zlib, math, os

OUT_DIR = u"C:\\Users\\SANDBOX\\Desktop\\\uc774\uc9c1\\Portfolio\\AI - R&D\\\uc644\uc804\uc790\ub3d9\ud654\\\ud504\ub85c\uc81d\ud2b8\\assets\\resources"

# ─────────────────────────────────────────────
# Core PNG writer
# ─────────────────────────────────────────────
def write_png(filename, width, height, pixels):
    """pixels: list of (r,g,b,a) tuples, row by row, top to bottom"""
    def make_chunk(name, data):
        c = zlib.crc32(name + data) & 0xffffffff
        return struct.pack('>I', len(data)) + name + data + struct.pack('>I', c)

    signature = b'\x89PNG\r\n\x1a\n'
    ihdr = struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0)

    raw = b''
    for y in range(height):
        raw += b'\x00'
        for x in range(width):
            r, g, b, a = pixels[y * width + x]
            raw += bytes([
                max(0, min(255, int(r))),
                max(0, min(255, int(g))),
                max(0, min(255, int(b))),
                max(0, min(255, int(a)))
            ])

    compressed = zlib.compress(raw, 6)
    png = (signature +
           make_chunk(b'IHDR', ihdr) +
           make_chunk(b'IDAT', compressed) +
           make_chunk(b'IEND', b''))

    with open(filename, 'wb') as f:
        f.write(png)

# ─────────────────────────────────────────────
# Helpers
# ─────────────────────────────────────────────
def new_pixels(w, h, r=0, g=0, b=0, a=0):
    return [(r, g, b, a)] * (w * h)

def set_pixel(pixels, w, x, y, r, g, b, a):
    if 0 <= x < w and 0 <= y < len(pixels) // w:
        pixels[y * w + x] = (r, g, b, a)

def blend(pixels, w, x, y, r, g, b, a):
    """Alpha blend onto existing pixel"""
    if not (0 <= x < w and 0 <= y < len(pixels) // w):
        return
    idx = y * w + x
    br, bg, bb, ba = pixels[idx]
    fa = a / 255.0
    nr = int(br * (1 - fa) + r * fa)
    ng = int(bg * (1 - fa) + g * fa)
    nb = int(bb * (1 - fa) + b * fa)
    na = min(255, ba + int(a * (1 - ba / 255.0)))
    pixels[idx] = (nr, ng, nb, na)

def smooth_step(edge0, edge1, x):
    if x <= edge0: return 0.0
    if x >= edge1: return 1.0
    t = (x - edge0) / (edge1 - edge0)
    return t * t * (3 - 2 * t)

def lerp(a, b, t):
    return a + (b - a) * t

def lerp_color(c1, c2, t):
    return tuple(int(lerp(c1[i], c2[i], t)) for i in range(4))

def draw_circle_filled(pixels, w, h, cx, cy, radius, r, g, b, a, soft=1.0):
    r0 = int(cx - radius - 2)
    r1 = int(cx + radius + 2)
    c0 = int(cy - radius - 2)
    c1 = int(cy + radius + 2)
    for py in range(max(0, c0), min(h, c1 + 1)):
        for px in range(max(0, r0), min(w, r1 + 1)):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < radius - soft:
                blend(pixels, w, px, py, r, g, b, a)
            elif dist < radius:
                alpha = int(a * (1 - (dist - (radius - soft)) / soft))
                blend(pixels, w, px, py, r, g, b, alpha)

def draw_ring(pixels, w, h, cx, cy, r_inner, r_outer, r, g, b, a):
    for py in range(max(0, int(cy - r_outer - 2)), min(h, int(cy + r_outer + 2))):
        for px in range(max(0, int(cx - r_outer - 2)), min(w, int(cx + r_outer + 2))):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if r_inner <= dist <= r_outer:
                edge_in = smooth_step(r_inner - 1, r_inner + 1, dist)
                edge_out = 1 - smooth_step(r_outer - 1, r_outer + 1, dist)
                alpha = int(a * edge_in * edge_out)
                blend(pixels, w, px, py, r, g, b, alpha)

def draw_line(pixels, w, h, x0, y0, x1, y1, r, g, b, a, thickness=1):
    dx = x1 - x0
    dy = y1 - y0
    length = math.sqrt(dx * dx + dy * dy)
    if length == 0:
        return
    steps = int(length * 2) + 1
    for i in range(steps + 1):
        t = i / steps
        px = x0 + dx * t
        py = y0 + dy * t
        for ty in range(-thickness, thickness + 1):
            for tx in range(-thickness, thickness + 1):
                if tx * tx + ty * ty <= thickness * thickness:
                    blend(pixels, w, int(px + tx), int(py + ty), r, g, b, a)

def radial_gradient(pixels, w, h, cx, cy, inner_col, outer_col, max_r):
    for py in range(h):
        for px in range(w):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            t = min(1.0, dist / max_r)
            col = lerp_color(inner_col, outer_col, t)
            blend(pixels, w, px, py, col[0], col[1], col[2], col[3])

# ─────────────────────────────────────────────
# STAR SPRITE GENERATOR
# ─────────────────────────────────────────────
def make_star(filename, glow_col, halo_col):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    gr, gg, gb = glow_col
    hr, hg, hb = halo_col

    # Outer halo (radius 30, opacity ~51)
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 30:
                alpha = int(51 * max(0, 1 - dist / 30))
                blend(pixels, W, px, py, hr, hg, hb, alpha)

    # Glow ring radius 22, opacity ~140
    draw_ring(pixels, W, H, cx, cy, 18, 22, gr, gg, gb, 140)

    # Body circle radius 14 - radial gradient from white center to glow color
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 15:
                t = dist / 14.0
                t = min(1, t)
                cr = int(lerp(255, gr, t))
                cg = int(lerp(255, gg, t))
                cb = int(lerp(255, gb, t))
                alpha = int(255 * (1 - smooth_step(13, 15, dist)))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # 4-way light rays (+shape, length 28px, opacity ~153)
    for angle in [0, 90, 180, 270]:
        rad = math.radians(angle)
        ex = cx + math.cos(rad) * 28
        ey = cy + math.sin(rad) * 28
        # Draw tapered ray
        for step in range(56):
            t = step / 56.0
            rx = cx + math.cos(rad) * step
            ry = cy + math.sin(rad) * step
            ray_a = int(153 * (1 - t))
            ray_w = max(0, int(2 * (1 - t)))
            for dw in range(-ray_w, ray_w + 1):
                perp = math.radians(angle + 90)
                bx = int(rx + math.cos(perp) * dw)
                by = int(ry + math.sin(perp) * dw)
                blend(pixels, W, bx, by, 255, 255, 255, ray_a)

    # Core (radius 6, white)
    draw_circle_filled(pixels, W, H, cx, cy, 6, 255, 255, 255, 255)

    write_png(filename, W, H, pixels)


def make_star_rainbow(filename):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    # Rainbow ring
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if 18 <= dist <= 22:
                angle = math.atan2(py - cy, px - cx)
                t = (angle + math.pi) / (2 * math.pi)
                # Rainbow colors
                hue = t * 6
                i = int(hue)
                f = hue - i
                rainbow = [
                    (255, 0, 0), (255, 165, 0), (255, 255, 0),
                    (0, 200, 0), (0, 100, 255), (148, 0, 211)
                ]
                c1 = rainbow[i % 6]
                c2 = rainbow[(i + 1) % 6]
                cr = int(c1[0] + (c2[0] - c1[0]) * f)
                cg = int(c1[1] + (c2[1] - c1[1]) * f)
                cb = int(c1[2] + (c2[2] - c1[2]) * f)
                edge = 1 - smooth_step(20, 22, dist)
                alpha = int(200 * edge)
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # Body - white-ish
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 15:
                alpha = int(255 * (1 - smooth_step(13, 15, dist)))
                t = dist / 14.0
                cr = int(lerp(255, 200, t))
                cg = int(lerp(255, 200, t))
                cb = int(lerp(255, 255, t))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # 6 color rays
    ray_colors = [(255, 0, 0), (255, 165, 0), (255, 255, 0),
                  (0, 200, 0), (0, 100, 255), (148, 0, 211)]
    for i, col in enumerate(ray_colors):
        angle = math.radians(i * 60)
        for step in range(30):
            t = step / 30.0
            rx = cx + math.cos(angle) * step
            ry = cy + math.sin(angle) * step
            ray_a = int(180 * (1 - t))
            blend(pixels, W, int(rx), int(ry), col[0], col[1], col[2], ray_a)
            blend(pixels, W, int(rx) + 1, int(ry), col[0], col[1], col[2], ray_a // 2)
            blend(pixels, W, int(rx), int(ry) + 1, col[0], col[1], col[2], ray_a // 2)

    # Core white
    draw_circle_filled(pixels, W, H, cx, cy, 6, 255, 255, 255, 255)
    write_png(filename, W, H, pixels)


def make_star_bomb(filename):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    # Dark glow
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 30:
                alpha = int(80 * max(0, 1 - dist / 30))
                blend(pixels, W, px, py, 120, 0, 0, alpha)

    # Dark body
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 15:
                t = dist / 14.0
                cr = int(lerp(100, 61, t))
                cg = 0
                cb = 0
                alpha = int(255 * (1 - smooth_step(13, 15, dist)))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # Red cracks radiating out
    for angle_deg in [30, 90, 150, 210, 270, 330]:
        angle = math.radians(angle_deg)
        for step in range(4, 13):
            rx = int(cx + math.cos(angle) * step)
            ry = int(cy + math.sin(angle) * step)
            blend(pixels, W, rx, ry, 220, 30, 30, 200)

    # X mark center (cross)
    for d in range(-6, 7):
        blend(pixels, W, cx + d, cy + d, 255, 60, 60, 255)
        blend(pixels, W, cx + d, cy - d, 255, 60, 60, 255)
        if abs(d) <= 5:
            blend(pixels, W, cx + d + 1, cy + d, 200, 30, 30, 180)
            blend(pixels, W, cx + d, cy - d + 1, 200, 30, 30, 180)

    write_png(filename, W, H, pixels)


def make_star_speed(filename):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    # Cyan glow
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 30:
                alpha = int(60 * max(0, 1 - dist / 30))
                blend(pixels, W, px, py, 0, 200, 255, alpha)

    # Motion blur tail (right side)
    for i in range(20):
        tx = cx + 10 + i
        for dy in range(-3, 4):
            ta = int(100 * (1 - i / 20.0) * (1 - abs(dy) / 4.0))
            blend(pixels, W, tx, cy + dy, 0, 150, 255, ta)

    # Electric blue body
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 15:
                t = dist / 14.0
                cr = int(lerp(100, 0, t))
                cg = int(lerp(200, 136, t))
                cb = 255
                alpha = int(255 * (1 - smooth_step(13, 15, dist)))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # White lightning bolt
    bolt = [(32, 26), (28, 33), (31, 33), (28, 39), (36, 31), (33, 31), (36, 25)]
    for i in range(len(bolt) - 1):
        x0, y0 = bolt[i]
        x1, y1 = bolt[i + 1]
        draw_line(pixels, W, H, x0, y0, x1, y1, 255, 255, 255, 230, 1)

    # Core
    draw_circle_filled(pixels, W, H, cx, cy, 4, 200, 240, 255, 255)
    write_png(filename, W, H, pixels)


def make_star_magnet(filename):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    # Teal glow
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 30:
                alpha = int(60 * max(0, 1 - dist / 30))
                blend(pixels, W, px, py, 0, 180, 150, alpha)

    # Dotted field rings
    for ring_r in [19, 24]:
        for angle_deg in range(0, 360, 12):
            angle = math.radians(angle_deg)
            px = int(cx + math.cos(angle) * ring_r)
            py = int(cy + math.sin(angle) * ring_r)
            blend(pixels, W, px, py, 0, 200, 180, 140)

    # Teal body
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 15:
                t = dist / 14.0
                cr = int(lerp(100, 0, t))
                cg = int(lerp(220, 180, t))
                cb = int(lerp(200, 160, t))
                alpha = int(255 * (1 - smooth_step(13, 15, dist)))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # U-shape magnet: two arms + bridge
    # Left arm (blue N)
    for dy in range(-8, 3):
        blend(pixels, W, cx - 5, cy + dy, 40, 80, 220, 230)
        blend(pixels, W, cx - 4, cy + dy, 40, 80, 220, 230)
    # Right arm (red S)
    for dy in range(-8, 3):
        blend(pixels, W, cx + 4, cy + dy, 220, 40, 40, 230)
        blend(pixels, W, cx + 5, cy + dy, 220, 40, 40, 230)
    # Bridge bottom
    for dx in range(-4, 6):
        blend(pixels, W, cx + dx, cy + 2, 200, 200, 200, 220)
        blend(pixels, W, cx + dx, cy + 3, 200, 200, 200, 220)

    # N / S labels (tiny dots)
    blend(pixels, W, cx - 5, cy - 9, 200, 230, 255, 255)
    blend(pixels, W, cx + 5, cy - 9, 255, 180, 180, 255)

    draw_circle_filled(pixels, W, H, cx, cy, 3, 255, 255, 255, 200)
    write_png(filename, W, H, pixels)


def make_star_ghost(filename):
    W, H = 64, 64
    cx, cy = 32, 32
    pixels = new_pixels(W, H)

    # Lavender halo
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 30:
                alpha = int(40 * max(0, 1 - dist / 30))
                blend(pixels, W, px, py, 180, 130, 255, alpha)

    # Ghost body - lavender at 70% opacity, irregular wavy edge
    for py in range(H):
        for px in range(W):
            dx = px - cx
            dy = py - cy
            dist = math.sqrt(dx * dx + dy * dy)
            angle = math.atan2(dy, dx)
            # Wavy edge
            wave = 1.0 + 0.12 * math.sin(angle * 5)
            effective_r = 14 * wave
            if dist < effective_r:
                t = dist / effective_r
                cr = int(lerp(255, 180, t))
                cg = int(lerp(230, 100, t))
                cb = 255
                alpha = int(178 * (1 - smooth_step(effective_r - 2, effective_r, dist)))
                blend(pixels, W, px, py, cr, cg, cb, alpha)

    # Drip shapes at bottom
    for i in range(3):
        drip_x = cx - 6 + i * 6
        drip_y = cy + 14
        draw_circle_filled(pixels, W, H, drip_x, drip_y, 3, 200, 150, 255, 140)
        for dy in range(4):
            blend(pixels, W, drip_x, drip_y + dy, 200, 150, 255, max(0, 140 - dy * 30))

    # Eyes
    draw_circle_filled(pixels, W, H, cx - 4, cy - 2, 2, 60, 20, 80, 220)
    draw_circle_filled(pixels, W, H, cx + 4, cy - 2, 2, 60, 20, 80, 220)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# BUCKET
# ─────────────────────────────────────────────
def make_bucket(filename):
    W, H = 120, 80
    pixels = new_pixels(W, H)

    # Trapezoidal shape: wider at top
    top_w = 100
    bot_w = 70
    top_y = 8
    bot_y = 72

    def in_trap(px, py):
        if py < top_y or py > bot_y:
            return False
        t = (py - top_y) / (bot_y - top_y)
        half_w = (top_w + (bot_w - top_w) * t) / 2
        left = W / 2 - half_w
        right = W / 2 + half_w
        return left <= px <= right

    # Interior radial gradient
    for py in range(H):
        for px in range(W):
            if in_trap(px, py):
                dx = px - W / 2
                dy = py - H / 2
                dist = math.sqrt(dx * dx + dy * dy)
                max_d = 55
                t = min(1, dist / max_d)
                cr = int(lerp(102, 26, t))
                cg = int(lerp(68, 10, t))
                cb = int(lerp(170, 58, t))
                blend(pixels, W, px, py, cr, cg, cb, 255)

    # Purple inner glow
    draw_circle_filled(pixels, W, H, W // 2, H // 2, 30, 153, 102, 255, 100)

    # Gold border - draw outline of trapezoid
    def draw_trap_border(thickness=2):
        for py in range(top_y, bot_y + 1):
            t = (py - top_y) / (bot_y - top_y)
            half_w = (top_w + (bot_w - top_w) * t) / 2
            left = int(W / 2 - half_w)
            right = int(W / 2 + half_w)
            for th in range(thickness):
                blend(pixels, W, left + th, py, 255, 215, 0, 255)
                blend(pixels, W, right - th, py, 255, 215, 0, 255)
        # Top and bottom edges
        for t_val in range(thickness):
            t = 0
            half_w = (top_w + (bot_w - top_w) * t) / 2
            left = int(W / 2 - half_w)
            right = int(W / 2 + half_w)
            for bx in range(left, right + 1):
                blend(pixels, W, bx, top_y + t_val, 255, 215, 0, 255)
            t = 1
            half_w = (top_w + (bot_w - top_w) * t) / 2
            left = int(W / 2 - half_w)
            right = int(W / 2 + half_w)
            for bx in range(left, right + 1):
                blend(pixels, W, bx, bot_y - t_val, 255, 215, 0, 255)

    draw_trap_border(2)

    # White highlight at top opening
    half_top = top_w // 2
    for bx in range(W // 2 - half_top + 3, W // 2 + half_top - 3):
        blend(pixels, W, bx, top_y + 3, 255, 255, 255, 180)
        blend(pixels, W, bx, top_y + 4, 255, 255, 255, 90)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# BACKGROUND
# ─────────────────────────────────────────────
def make_background(filename):
    import random
    random.seed(42)
    W, H = 1280, 720
    pixels = new_pixels(W, H)

    # Base color
    for i in range(W * H):
        pixels[i] = (0, 0, 16, 255)

    # Large purple-blue nebula ellipses
    def draw_nebula_ellipse(cx, cy, rx, ry, col, alpha_max):
        r, g, b = col
        for py in range(max(0, cy - ry - 5), min(H, cy + ry + 5)):
            for px in range(max(0, cx - rx - 5), min(W, cx + rx + 5)):
                dx = (px - cx) / rx
                dy = (py - cy) / ry
                dist = math.sqrt(dx * dx + dy * dy)
                if dist < 1.0:
                    alpha = int(alpha_max * (1 - dist) * (1 - dist))
                    blend(pixels, W, px, py, r, g, b, alpha)

    draw_nebula_ellipse(350, 280, 280, 180, (30, 15, 80), 153)
    draw_nebula_ellipse(900, 450, 320, 200, (20, 10, 70), 153)
    draw_nebula_ellipse(640, 200, 200, 120, (10, 25, 70), 89)

    # 200+ small star dots
    for _ in range(220):
        sx = random.randint(0, W - 1)
        sy = random.randint(0, H - 1)
        size = random.randint(1, 2)
        brightness = random.randint(120, 220)
        alpha = random.randint(80, 180)
        draw_circle_filled(pixels, W, H, sx, sy, size, brightness, brightness, brightness, alpha)

    # 50 brighter stars with tiny glow
    for _ in range(50):
        sx = random.randint(0, W - 1)
        sy = random.randint(0, H - 1)
        brightness = random.randint(200, 255)
        # Tiny glow
        draw_circle_filled(pixels, W, H, sx, sy, 4, brightness, brightness, 200, 40)
        draw_circle_filled(pixels, W, H, sx, sy, 2, brightness, brightness, brightness, 200)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# LOGO TITLE
# ─────────────────────────────────────────────
def make_logo(filename):
    import random
    random.seed(7)
    W, H = 640, 120
    pixels = new_pixels(W, H)

    # Background glow
    for py in range(H):
        for px in range(W):
            dist_y = abs(py - H / 2) / (H / 2)
            alpha = int(60 * (1 - dist_y))
            blend(pixels, W, px, py, 255, 200, 0, alpha)

    # Banner shape: rounded rectangle with gradient
    margin = 10
    for py in range(margin, H - margin):
        t = (py - margin) / (H - 2 * margin)
        r = int(lerp(255, 232, t))
        g = int(lerp(232, 100, t))
        b = 0
        for px in range(margin, W - margin):
            blend(pixels, W, px, py, r, g, b, 230)

    # Dark brown border
    for py in range(margin, H - margin):
        for th in range(3):
            blend(pixels, W, margin + th, py, 74, 32, 0, 255)
            blend(pixels, W, W - margin - th - 1, py, 74, 32, 0, 255)
    for px in range(margin, W - margin):
        for th in range(3):
            blend(pixels, W, px, margin + th, 74, 32, 0, 255)
            blend(pixels, W, px, H - margin - th - 1, 74, 32, 0, 255)

    # Simulate "STAR SWEEPER" with golden block letters (rectangles)
    # Draw decorative letter blocks
    letter_y = 35
    letter_h = 50
    # Each "letter" is a set of rectangles
    def draw_letter_rect(lx, ly, lw, lh):
        for py in range(ly, ly + lh):
            for px in range(lx, lx + lw):
                t = (py - ly) / lh
                r = int(lerp(255, 200, t))
                g = int(lerp(240, 150, t))
                b = 0
                blend(pixels, W, px, py, r, g, b, 255)
                # Highlight
                if px == lx or py == ly:
                    blend(pixels, W, px, py, 255, 255, 200, 120)
                if px == lx + lw - 1 or py == ly + lh - 1:
                    blend(pixels, W, px, py, 100, 60, 0, 120)

    # Place decorative letter-like shapes across the banner
    positions = [30, 75, 115, 155, 195, 250, 300, 350, 395, 440, 480, 525]
    widths =    [28, 24, 28, 24, 24,  28,  28,  24,  24,  28,  24,  24 ]
    for i, (lx, lw) in enumerate(zip(positions, widths)):
        draw_letter_rect(lx, letter_y, lw, letter_h)
        # Space between STAR and SWEEPER
        if i == 3:
            continue

    # Star decorations
    for _ in range(12):
        sx = random.randint(15, W - 15)
        sy = random.randint(12, H - 12)
        s = random.randint(2, 5)
        draw_circle_filled(pixels, W, H, sx, sy, s, 255, 255, 200, 200)

    # Inner shine line
    for px in range(margin + 5, W - margin - 5):
        blend(pixels, W, px, margin + 7, 255, 255, 220, 100)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# UI BUTTON
# ─────────────────────────────────────────────
def make_ui_button(filename):
    W, H = 240, 70
    pixels = new_pixels(W, H)
    radius = 16

    def in_rounded_rect(px, py, r):
        if px < 0 or px >= W or py < 0 or py >= H:
            return False, 0.0
        corners = [(r, r), (W - r - 1, r), (r, H - r - 1), (W - r - 1, H - r - 1)]
        # Check corners
        if px < r and py < r:
            dist = math.sqrt((px - r) ** 2 + (py - r) ** 2)
            return dist < r, max(0, min(1, r - dist))
        if px >= W - r and py < r:
            dist = math.sqrt((px - (W - r - 1)) ** 2 + (py - r) ** 2)
            return dist < r, max(0, min(1, r - dist))
        if px < r and py >= H - r:
            dist = math.sqrt((px - r) ** 2 + (py - (H - r - 1)) ** 2)
            return dist < r, max(0, min(1, r - dist))
        if px >= W - r and py >= H - r:
            dist = math.sqrt((px - (W - r - 1)) ** 2 + (py - (H - r - 1)) ** 2)
            return dist < r, max(0, min(1, r - dist))
        return True, 1.0

    # Base fill
    for py in range(H):
        for px in range(W):
            inside, coverage = in_rounded_rect(px, py, radius)
            if inside:
                # Dark blue base
                blend(pixels, W, px, py, 26, 26, 74, int(217 * coverage))
                # Top highlight gradient (top 30%)
                if py < H * 0.3:
                    t = 1 - py / (H * 0.3)
                    blend(pixels, W, px, py, 255, 255, 255, int(60 * t * coverage))

    # Outer border
    for py in range(H):
        for px in range(W):
            inside_full, _ = in_rounded_rect(px, py, radius)
            inside_inner, _ = in_rounded_rect(px, py, radius - 2)
            if inside_full and not inside_inner:
                blend(pixels, W, px, py, 68, 102, 170, 200)
            elif inside_inner:
                inside_inner2, _ = in_rounded_rect(px, py, radius - 4)
                if not inside_inner2:
                    blend(pixels, W, px, py, 102, 136, 204, 140)

    # Star accent dots
    for sx in [15, 25, W - 25, W - 15]:
        sy = H // 2
        draw_circle_filled(pixels, W, H, sx, sy, 3, 150, 200, 255, 180)
        draw_circle_filled(pixels, W, H, sx, sy, 1, 255, 255, 255, 240)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# SLOT SPRITES
# ─────────────────────────────────────────────
def make_slot(filename, col):
    W, H = 40, 40
    cx, cy = 20, 20
    pixels = new_pixels(W, H)
    r, g, b = col

    # Outer glow ring
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if 16 <= dist <= 20:
                alpha = int(200 * (1 - abs(dist - 18) / 2))
                blend(pixels, W, px, py, r, g, b, alpha)

    # Middle ring fill (color opacity ~64)
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 16:
                blend(pixels, W, px, py, r, g, b, 64)

    # Dark inner background
    draw_circle_filled(pixels, W, H, cx, cy, 14, 10, 10, 34, 255)

    # Top highlight arc (white opacity ~100)
    for angle_deg in range(200, 340):
        angle = math.radians(angle_deg)
        for dr in range(11, 14):
            hx = int(cx + math.cos(angle) * dr)
            hy = int(cy + math.sin(angle) * dr)
            blend(pixels, W, hx, hy, 255, 255, 255, 100)

    # Dotted center circle (white opacity ~38)
    for angle_deg in range(0, 360, 30):
        angle = math.radians(angle_deg)
        dx = int(cx + math.cos(angle) * 5)
        dy = int(cy + math.sin(angle) * 5)
        blend(pixels, W, dx, dy, 255, 255, 255, 38)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# POWER-UP ICONS
# ─────────────────────────────────────────────
def make_icon_shield(filename):
    W, H = 48, 48
    cx, cy = 24, 24
    pixels = new_pixels(W, H)

    # Hexagon helper
    def in_hexagon(px, py, cx, cy, size):
        dx = px - cx
        dy = py - cy
        # Regular hexagon check
        return (abs(dx) <= size * math.sqrt(3) / 2 and
                abs(dy) <= size and
                abs(dx) * 0.5 + abs(dy) * math.sqrt(3) / 2 <= size * math.sqrt(3) / 2 * math.sqrt(3))

    # Draw hexagon body
    for py in range(H):
        for px in range(W):
            if in_hexagon(px, py, cx, cy, 20):
                t = math.sqrt((px - cx) ** 2 + (py - cy) ** 2) / 20
                r_c = int(lerp(60, 17, t))
                g_c = int(lerp(120, 68, t))
                b_c = int(lerp(255, 170, t))
                blend(pixels, W, px, py, r_c, g_c, b_c, 230)

    # Bright blue border
    for py in range(H):
        for px in range(W):
            if in_hexagon(px, py, cx, cy, 20) and not in_hexagon(px, py, cx, cy, 17):
                blend(pixels, W, px, py, 100, 180, 255, 255)

    # Star center
    draw_circle_filled(pixels, W, H, cx, cy, 7, 255, 255, 255, 230)
    for angle_deg in [0, 72, 144, 216, 288]:
        angle = math.radians(angle_deg)
        for step in range(4, 10):
            sx = int(cx + math.cos(angle) * step)
            sy = int(cy + math.sin(angle) * step)
            blend(pixels, W, sx, sy, 255, 255, 255, 200)

    write_png(filename, W, H, pixels)


def make_icon_slow(filename):
    W, H = 48, 48
    cx, cy = 24, 24
    pixels = new_pixels(W, H)

    # Hourglass shape - navy
    def in_hourglass(px, py):
        dy = py - cy
        t = abs(dy) / 20.0
        half_w = 4 + t * 12
        return abs(px - cx) <= half_w and abs(dy) <= 20

    for py in range(H):
        for px in range(W):
            if in_hourglass(px, py):
                dy = py - cy
                t = math.sqrt((px - cx) ** 2 + dy * dy) / 24
                r_c = int(lerp(50, 34, t))
                g_c = int(lerp(80, 51, t))
                b_c = int(lerp(150, 102, t))
                blend(pixels, W, px, py, r_c, g_c, b_c, 240)

    # Cyan border
    for py in range(H):
        for px in range(W):
            if in_hourglass(px, py):
                dy = py - cy
                t_in = abs(px - cx) / (4 + abs(dy) / 20.0 * 12)
                if t_in > 0.85:
                    blend(pixels, W, px, py, 0, 220, 220, 255)

    # Ice crystal hint - small diamond shapes
    for angle_deg in [45, 135, 225, 315]:
        angle = math.radians(angle_deg)
        ix = int(cx + math.cos(angle) * 8)
        iy = int(cy + math.sin(angle) * 8)
        draw_circle_filled(pixels, W, H, ix, iy, 2, 180, 240, 255, 180)

    # Center dot
    draw_circle_filled(pixels, W, H, cx, cy, 3, 200, 240, 255, 220)

    write_png(filename, W, H, pixels)


def make_icon_wildcard(filename):
    W, H = 48, 48
    cx, cy = 24, 24
    pixels = new_pixels(W, H)

    # Pentagon star (5-pointed) in gold
    outer_r = 20
    inner_r = 9
    n_points = 5

    def in_star5(px, py, cx, cy, r_out, r_in):
        dx = px - cx
        dy = py - cy
        # Use crossing number
        angle = math.atan2(dy, dx) + math.pi / 2
        dist = math.sqrt(dx * dx + dy * dy)
        # Star polygon check
        seg = int((angle / (2 * math.pi / n_points)) % n_points)
        a0 = seg * (2 * math.pi / n_points) - math.pi / 2
        a1 = a0 + math.pi / n_points
        a2 = a1 + math.pi / n_points
        # Interpolate between outer and inner
        t_a = (angle - a0) / (math.pi / n_points) if (angle - a0) >= 0 else 1
        t_a = t_a % 1.0
        r_edge = r_out * r_in / math.sqrt(
            (r_in * math.cos(t_a * math.pi)) ** 2 + (r_out * math.sin(t_a * math.pi)) ** 2
        ) if r_in > 0 else r_out
        return dist <= r_edge

    # Fallback: draw gold filled polygon star
    # Generate star polygon vertices
    verts = []
    for i in range(n_points * 2):
        a = math.radians(-90 + i * 180 / n_points)
        r = outer_r if i % 2 == 0 else inner_r
        verts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))

    def point_in_polygon(x, y, polygon):
        inside = False
        n = len(polygon)
        j = n - 1
        for i in range(n):
            xi, yi = polygon[i]
            xj, yj = polygon[j]
            if ((yi > y) != (yj > y)) and (x < (xj - xi) * (y - yi) / (yj - yi) + xi):
                inside = not inside
            j = i
        return inside

    for py in range(H):
        for px in range(W):
            if point_in_polygon(px, py, verts):
                dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
                t = dist / outer_r
                r_c = int(lerp(255, 200, t))
                g_c = int(lerp(220, 140, t))
                b_c = 0
                blend(pixels, W, px, py, r_c, g_c, b_c, 255)

    # Border - bright gold
    for py in range(H):
        for px in range(W):
            if point_in_polygon(px, py, verts):
                # Check if border pixel
                is_border = False
                for ddx in [-1, 0, 1]:
                    for ddy in [-1, 0, 1]:
                        if not point_in_polygon(px + ddx, py + ddy, verts):
                            is_border = True
                if is_border:
                    blend(pixels, W, px, py, 255, 240, 100, 255)

    # Lightning bolt center
    bolt = [(cx, cy - 7), (cx - 4, cy + 1), (cx, cy + 1), (cx, cy + 7), (cx + 4, cy - 1), (cx, cy - 1)]
    for i in range(len(bolt) - 1):
        draw_line(pixels, W, H, bolt[i][0], bolt[i][1], bolt[i+1][0], bolt[i+1][1], 255, 255, 255, 240, 1)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# EFFECT SPRITES
# ─────────────────────────────────────────────
def make_effect_shockwave(filename):
    W, H = 96, 96
    cx, cy = 48, 48
    pixels = new_pixels(W, H)

    # Transparent center, white ring at 35-38, faint cyan, double ring
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            # Inner ring
            if 33 <= dist <= 40:
                t = 1 - abs(dist - 36.5) / 3.5
                blend(pixels, W, px, py, 255, 255, 255, int(220 * t))
            # Faint cyan fill inside ring
            if dist < 33:
                alpha = int(30 * (1 - dist / 33))
                blend(pixels, W, px, py, 0, 220, 255, alpha)
            # Double ring (outer)
            if 43 <= dist <= 46:
                t = 1 - abs(dist - 44.5) / 1.5
                blend(pixels, W, px, py, 100, 230, 255, int(100 * t))

    write_png(filename, W, H, pixels)


def make_effect_rainbow_burst(filename):
    import random
    random.seed(13)
    W, H = 128, 128
    cx, cy = 64, 64
    pixels = new_pixels(W, H)

    # White center explosion
    draw_circle_filled(pixels, W, H, cx, cy, 18, 255, 255, 255, 255)
    draw_circle_filled(pixels, W, H, cx, cy, 25, 255, 250, 220, 180)

    # 8 color rays outward
    ray_cols = [
        (255, 0, 0), (255, 128, 0), (255, 255, 0), (0, 220, 0),
        (0, 150, 255), (0, 0, 255), (148, 0, 211), (255, 0, 180)
    ]
    for i, col in enumerate(ray_cols):
        angle = math.radians(i * 45)
        for step in range(20, 55):
            t = (step - 20) / 35.0
            rx = cx + math.cos(angle) * step
            ry = cy + math.sin(angle) * step
            alpha = int(220 * (1 - t))
            width = max(1, int(4 * (1 - t)))
            for dw in range(-width, width + 1):
                perp = angle + math.pi / 2
                bx = int(rx + math.cos(perp) * dw)
                by = int(ry + math.sin(perp) * dw)
                blend(pixels, W, bx, by, col[0], col[1], col[2], alpha)

        # Star particle at tip
        tip_x = int(cx + math.cos(angle) * 52)
        tip_y = int(cy + math.sin(angle) * 52)
        draw_circle_filled(pixels, W, H, tip_x, tip_y, 4, col[0], col[1], col[2], 220)
        draw_circle_filled(pixels, W, H, tip_x, tip_y, 2, 255, 255, 255, 200)

    write_png(filename, W, H, pixels)


def make_effect_bomb_explode(filename):
    import random
    random.seed(99)
    W, H = 128, 128
    cx, cy = 64, 64
    pixels = new_pixels(W, H)

    # Irregular spiky explosion blob
    # Draw as many overlapping circles at random angles
    for i in range(24):
        angle = math.radians(i * 15 + random.randint(-8, 8))
        dist_r = 30 + random.randint(-8, 15)
        ex = int(cx + math.cos(angle) * dist_r)
        ey = int(cy + math.sin(angle) * dist_r)
        blob_r = random.randint(12, 22)
        # Orange-red
        draw_circle_filled(pixels, W, H, ex, ey, blob_r, 220, 60 + random.randint(0, 40), 0, 200)

    # Main explosion center - orange to yellow
    for py in range(H):
        for px in range(W):
            dist = math.sqrt((px - cx) ** 2 + (py - cy) ** 2)
            if dist < 38:
                t = dist / 38.0
                r_c = 255
                g_c = int(lerp(255, 120, t))
                b_c = 0
                alpha = int(240 * (1 - smooth_step(30, 38, dist)))
                blend(pixels, W, px, py, r_c, g_c, b_c, alpha)

    # Yellow core
    draw_circle_filled(pixels, W, H, cx, cy, 16, 255, 255, 100, 255)
    draw_circle_filled(pixels, W, H, cx, cy, 8, 255, 255, 255, 255)

    # Smoke wisps
    for i in range(5):
        angle = math.radians(i * 72 + 20)
        for step in range(35, 55):
            t = (step - 35) / 20.0
            wx = int(cx + math.cos(angle) * step)
            wy = int(cy + math.sin(angle) * step)
            alpha = int(60 * (1 - t))
            draw_circle_filled(pixels, W, H, wx, wy, int(4 * (1 - t * 0.5)), 80, 80, 80, alpha)

    write_png(filename, W, H, pixels)


def make_effect_meteor_shower(filename):
    import random
    random.seed(55)
    W, H = 256, 64
    pixels = new_pixels(W, H)

    # Dark blue banner base
    for i in range(W * H):
        pixels[i] = (5, 10, 30, 220)

    # 5 diagonal white-to-blue streaks
    for i in range(5):
        sx = 20 + i * 46
        sy = 10
        ex = sx + 30
        ey = H - 10
        for step in range(60):
            t = step / 60.0
            lx = int(sx + (ex - sx) * t)
            ly = int(sy + (ey - sy) * t)
            alpha = int(200 * (1 - t * 0.5))
            r_c = int(lerp(255, 0, t))
            g_c = int(lerp(255, 100, t))
            b_c = 255
            # Streak width
            for dw in range(-2, 3):
                blend(pixels, W, lx + dw, ly, r_c, g_c, b_c, int(alpha * (1 - abs(dw) / 3.0)))

    # Star particles
    for _ in range(40):
        sx = random.randint(0, W - 1)
        sy = random.randint(0, H - 1)
        brightness = random.randint(150, 255)
        alpha = random.randint(100, 200)
        blend(pixels, W, sx, sy, brightness, brightness, 255, alpha)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# UI BOSS WARNING
# ─────────────────────────────────────────────
def make_boss_warning(filename):
    W, H = 480, 120
    pixels = new_pixels(W, H)

    # Dark red base
    for i in range(W * H):
        pixels[i] = (26, 0, 0, 255)

    # Warning stripe pattern (diagonal red/dark alternating)
    stripe_w = 20
    for py in range(H):
        for px in range(W):
            stripe = (px + py) // stripe_w
            if stripe % 2 == 0:
                blend(pixels, W, px, py, 180, 0, 0, 80)

    # Red border
    border = 4
    for px in range(W):
        for th in range(border):
            blend(pixels, W, px, th, 204, 0, 0, 255)
            blend(pixels, W, px, H - 1 - th, 204, 0, 0, 255)
    for py in range(H):
        for th in range(border):
            blend(pixels, W, th, py, 204, 0, 0, 255)
            blend(pixels, W, W - 1 - th, py, 204, 0, 0, 255)

    # Inner panel semi-transparent
    panel_margin = 12
    for py in range(panel_margin, H - panel_margin):
        for px in range(panel_margin, W - panel_margin):
            blend(pixels, W, px, py, 60, 0, 0, 120)

    # Warning symbol: triangle with ! inside (centered)
    tri_cx = W // 2
    tri_cy = H // 2
    tri_h = 60
    tri_w = 70

    # Draw filled triangle
    for py in range(tri_cy - tri_h // 2, tri_cy + tri_h // 2 + 1):
        row = py - (tri_cy - tri_h // 2)
        half = int(tri_w * row / tri_h / 2)
        for px in range(tri_cx - half, tri_cx + half + 1):
            blend(pixels, W, px, py, 200, 0, 0, 200)

    # Triangle border
    for py in range(tri_cy - tri_h // 2, tri_cy + tri_h // 2 + 1):
        row = py - (tri_cy - tri_h // 2)
        half = int(tri_w * row / tri_h / 2)
        for th in range(3):
            blend(pixels, W, tri_cx - half + th, py, 255, 100, 0, 255)
            blend(pixels, W, tri_cx + half - th, py, 255, 100, 0, 255)
    # Top of triangle
    blend(pixels, W, tri_cx, tri_cy - tri_h // 2, 255, 100, 0, 255)
    # Bottom edge
    row = tri_h
    half = int(tri_w * row / tri_h / 2)
    for px in range(tri_cx - half, tri_cx + half + 1):
        for th in range(3):
            py = tri_cy + tri_h // 2 - th
            blend(pixels, W, px, py, 255, 100, 0, 255)

    # ! mark inside triangle
    # Bar
    for dy in range(-20, -2):
        for dx in range(-3, 4):
            blend(pixels, W, tri_cx + dx, tri_cy + dy, 255, 220, 0, 255)
    # Dot
    draw_circle_filled(pixels, W, H, tri_cx, tri_cy + 8, 4, 255, 220, 0, 255)

    # Bright red edge glow
    glow_size = 8
    for py in range(H):
        for px in range(W):
            edge_dist = min(px, py, W - 1 - px, H - 1 - py)
            if edge_dist < glow_size:
                alpha = int(120 * (1 - edge_dist / glow_size))
                blend(pixels, W, px, py, 255, 0, 0, alpha)

    write_png(filename, W, H, pixels)


# ─────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────
def save(name, fn, *args, **kwargs):
    path = os.path.join(OUT_DIR, name)
    fn(path, *args, **kwargs)
    print(f"  Saved: {name}")

if __name__ == '__main__':
    print("=== Star Sweeper Asset Generator ===\n")

    # 1. Standard stars
    print("[1] Standard stars...")
    save("star_red.png",    make_star, (255, 68, 68),  (255, 0, 0))
    save("star_blue.png",   make_star, (68, 136, 255), (0, 68, 255))
    save("star_yellow.png", make_star, (255, 238, 68), (255, 204, 0))
    save("star_green.png",  make_star, (68, 221, 102), (0, 170, 68))
    save("star_purple.png", make_star, (187, 68, 255), (119, 0, 255))

    # 2. Special stars
    print("[2] Special stars...")
    save("star_rainbow.png", make_star_rainbow)
    save("star_bomb.png",    make_star_bomb)
    save("star_speed.png",   make_star_speed)
    save("star_magnet.png",  make_star_magnet)
    save("star_ghost.png",   make_star_ghost)

    # 3. Bucket
    print("[3] Bucket...")
    save("bucket.png", make_bucket)

    # 4. Background
    print("[4] Background (1280x720 - may take a moment)...")
    save("bg_space.png", make_background)

    # 5. Logo
    print("[5] Logo title...")
    save("logo_title.png", make_logo)

    # 6. UI Button
    print("[6] UI Button...")
    save("ui_button.png", make_ui_button)

    # 7. Slot sprites
    print("[7] Slot sprites...")
    save("slot_red.png",    make_slot, (220, 60, 60))
    save("slot_blue.png",   make_slot, (60, 120, 255))
    save("slot_yellow.png", make_slot, (240, 210, 40))
    save("slot_green.png",  make_slot, (60, 200, 90))
    save("slot_purple.png", make_slot, (180, 60, 240))

    # 8. Power-up icons
    print("[8] Power-up icons...")
    save("icon_powerup_shield.png",   make_icon_shield)
    save("icon_powerup_slow.png",     make_icon_slow)
    save("icon_powerup_wildcard.png", make_icon_wildcard)

    # 9. Effect sprites
    print("[9] Effect sprites...")
    save("effect_shockwave.png",     make_effect_shockwave)
    save("effect_rainbow_burst.png", make_effect_rainbow_burst)
    save("effect_bomb_explode.png",  make_effect_bomb_explode)
    save("effect_meteor_shower.png", make_effect_meteor_shower)

    # 10. UI Boss Warning
    print("[10] UI Boss Warning...")
    save("ui_boss_warning.png", make_boss_warning)

    # Verify
    print("\n=== Verifying files ===")
    expected = [
        "star_red.png", "star_blue.png", "star_yellow.png", "star_green.png", "star_purple.png",
        "star_rainbow.png", "star_bomb.png", "star_speed.png", "star_magnet.png", "star_ghost.png",
        "bucket.png", "bg_space.png", "logo_title.png", "ui_button.png",
        "slot_red.png", "slot_blue.png", "slot_yellow.png", "slot_green.png", "slot_purple.png",
        "icon_powerup_shield.png", "icon_powerup_slow.png", "icon_powerup_wildcard.png",
        "effect_shockwave.png", "effect_rainbow_burst.png", "effect_bomb_explode.png", "effect_meteor_shower.png",
        "ui_boss_warning.png"
    ]
    all_ok = True
    for name in expected:
        path = os.path.join(OUT_DIR, name)
        exists = os.path.exists(path)
        size = os.path.getsize(path) if exists else 0
        status = "OK" if exists else "MISSING"
        print(f"  {status:8s} {name:40s} ({size:,} bytes)" if exists else f"  {status:8s} {name}")
        if not exists:
            all_ok = False

    print(f"\n{'All files generated successfully!' if all_ok else 'Some files are missing!'}")
    print(f"Total files: {len(expected)}")
