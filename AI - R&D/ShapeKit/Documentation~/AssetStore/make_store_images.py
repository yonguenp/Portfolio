# Generates Asset Store key images with the same shape math as ShapeImage.shader.
# Run from this folder: python3 make_store_images.py  (needs numpy, pillow)
import numpy as np
from PIL import Image, ImageDraw, ImageFont

FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
NAME = "ShapeKit"
TAGLINE = "Sprite-free UI shapes for uGUI"

def hexc(h, a=1.0):
    return (int(h[0:2], 16) / 255, int(h[2:4], 16) / 255, int(h[4:6], 16) / 255, a)

BLUE, VIOLET = hexc("4F8BFF"), hexc("7B5CFF")
ORANGE, YELLOW = hexc("FF7A59"), hexc("FFD250")
INK = hexc("222633")

class Canvas:
    def __init__(self, w, h, top=hexc("F4F6FB"), bottom=hexc("DDE4F5")):
        self.w, self.h = w, h
        self.img = np.zeros((h, w, 3))
        t = np.linspace(0, 1, h)[:, None, None]
        self.img[:] = np.array(top[:3]) * (1 - t) + np.array(bottom[:3]) * t
        self.ys, self.xs = np.mgrid[0:h, 0:w].astype(float)

    @staticmethod
    def sdf(px, py, hx, hy, r):
        rr = np.where(px > 0, np.where(py > 0, r[1], r[2]), np.where(py > 0, r[0], r[3]))
        qx = np.abs(px) - hx + rr
        qy = np.abs(py) - hy + rr
        return np.minimum(np.maximum(qx, qy), 0) + np.hypot(np.maximum(qx, 0), np.maximum(qy, 0)) - rr

    def blend(self, col, a):
        a = np.clip(a, 0, 1)[..., None]
        self.img = self.img * (1 - a) + col * a

    def shape(self, cx, cy, w, h, color=(1, 1, 1, 1), mode="uniform", radius=16, radii=(16, 16, 16, 16),
              grad=None, angle=90, outline=0, ocolor=(0, 0, 0, 1), soft=0, shadow=None, s=1.0):
        cx, cy, w, h = cx * s, cy * s, w * s, h * s
        radius, outline, soft = radius * s, outline * s, soft * s
        radii = [v * s for v in radii]
        hx, hy = w / 2, h / 2
        m = min(hx, hy)
        r = {"uniform": [radius] * 4, "pill": [m] * 4, "per": list(radii)}[mode]
        r = [min(max(v, 0), m) for v in r]
        if shadow:
            sc, off, blur, spread = shadow
            off, blur, spread = (off[0] * s, off[1] * s), blur * s, spread * s
            spread = max(spread, -m + 0.5)
            shx, shy = hx + spread, hy + spread
            sr = [min(max(v + spread, 0), min(shx, shy)) for v in r]
            d = self.sdf(self.xs - (cx + off[0]), -(self.ys - (cy - off[1])), shx, shy, sr)
            b = max(blur, 1.0)
            t = np.clip((d + b) / (2 * b), 0, 1)
            self.blend(np.array(sc[:3]), sc[3] * color[3] * (1 - t * t * (3 - 2 * t)))
        px, py = self.xs - cx, -(self.ys - cy)
        d = self.sdf(px, py, hx, hy, r)
        fill = np.ones(d.shape + (4,)) * np.array(color)
        if grad:
            rad = np.radians(angle)
            dx, dy = np.cos(rad), np.sin(rad)
            ext = abs(hx * dx) + abs(hy * dy)
            t = (px * dx + py * dy) / (2 * ext) + 0.5
            g = np.array(grad[0]) + (np.array(grad[1]) - np.array(grad[0])) * t[..., None]
            fill = fill * np.clip(g, 0, 1)
        o = np.array(ocolor)
        sa = np.clip(0.5 - d / max(soft, 1.0), 0, 1)
        ia = np.clip(0.5 - (d + min(outline, m)), 0, 1) if outline > 0 else np.ones_like(d)
        col = o * (1 - ia[..., None]) + fill * ia[..., None]
        self.blend(col[..., :3], col[..., 3] * sa)

    def image(self):
        return Image.fromarray((np.clip(self.img, 0, 1) * 255).astype(np.uint8))

def showcase(c, ox, oy, s):
    """The demo layout (1080x900 units), placed at (ox, oy) with scale s."""
    def sh(x, y, w, h, **k):
        c.shape((ox / s) + x, (oy / s) + y, w, h, s=s, **k)
    sh(540, 170, 900, 260, radius=32, shadow=((0, 0, 0, 0.15), (0, -10), 24, 0))
    sh(240, 170, 160, 160, color=hexc("3B2F4A"), mode="pill", outline=6, ocolor=(1, 1, 1, 1),
       shadow=((0, 0, 0, 0.3), (0, -4), 8, 0))
    sh(640, 170, 480, 130, color=BLUE, mode="per", radii=(36, 36, 36, 4))
    sh(310, 420, 400, 110, mode="pill", grad=(BLUE, VIOLET), angle=0,
       shadow=((0.23, 0.25, 0.72, 0.4), (0, -8), 14, 0))
    sh(770, 420, 400, 110, color=(1, 1, 1, 0), radius=24, outline=4, ocolor=BLUE)
    for i in range(4):
        sh(210 + i * 220, 620, 180, 180, radius=40, grad=(ORANGE, YELLOW), angle=i * 45)
    sh(540, 810, 600, 80, color=hexc("7B5CFF", 0.6), mode="pill", soft=30)

def text(img, xy, msg, size, color=INK, bold=True, anchor="la"):
    d = ImageDraw.Draw(img)
    f = ImageFont.truetype(FONT_BOLD if bold else FONT, size)
    d.text(xy, msg, font=f, fill=tuple(int(v * 255) for v in color[:3]), anchor=anchor)

def cover(w, h, path):
    c = Canvas(w, h)
    s = min(w * 0.55 / 1080, h * 0.86 / 900)
    showcase(c, w - 1080 * s - w * 0.03, (h - 900 * s) / 2, s)
    img = c.image()
    x = int(w * 0.05)
    text(img, (x, int(h * 0.50)), NAME, int(min(h * 0.11, w * 0.075)), anchor="ls")
    text(img, (x, int(h * 0.58)), TAGLINE, int(min(h * 0.04, w * 0.024)), hexc("555B6E"), bold=False, anchor="ls")
    text(img, (x, int(h * 0.66)), "1 material · 1 draw call", int(min(h * 0.035, w * 0.026)), BLUE, anchor="ls")
    img.save(path)

def card(w, h, path):
    c = Canvas(w, h)
    s = min(w * 0.5 / 1080, h * 0.8 / 900)
    showcase(c, w - 1080 * s - w * 0.03, (h - 900 * s) / 2, s)
    img = c.image()
    x = int(w * 0.05)
    text(img, (x, int(h * 0.52)), NAME, int(min(h * 0.12, w * 0.085)), anchor="ls")
    text(img, (x, int(h * 0.64)), "Sprite-free UI", int(min(h * 0.065, w * 0.045)), hexc("555B6E"), bold=False, anchor="ls")
    img.save(path)

def icon(size, path):
    c = Canvas(size, size, hexc("FFFFFF"), hexc("FFFFFF"))
    k = size / 160
    c.shape(80, 80, 150, 150, s=k, radius=36, grad=(BLUE, VIOLET), angle=45)
    c.shape(80, 80, 90, 56, s=k, mode="pill", color=(1, 1, 1, 0), outline=8, ocolor=(1, 1, 1, 1))
    c.image().save(path)

def feature(path, title, subtitle, draw):
    w, h = 1950, 1300
    c = Canvas(w, h)
    draw(c)
    img = c.image()
    text(img, (w // 2, 110), title, 84, anchor="mm")
    text(img, (w // 2, 200), subtitle, 44, hexc("555B6E"), bold=False, anchor="mm")
    img.save(path)

def draw_corners(c):
    c.shape(520, 720, 640, 420, radius=60, color=BLUE, shadow=((0, 0, 0, 0.18), (0, -14), 30, 0))
    c.shape(1430, 720, 640, 420, mode="per", radii=(120, 8, 120, 8), color=VIOLET,
            shadow=((0, 0, 0, 0.18), (0, -14), 30, 0))
    c.shape(520, 1080, 520, 120, mode="pill", color=ORANGE)
    c.shape(1430, 1080, 220, 220, mode="pill", color=YELLOW)

def draw_effects(c):
    c.shape(380, 700, 480, 480, radius=80, grad=(ORANGE, YELLOW), angle=45)
    c.shape(975, 700, 480, 480, radius=80, color=(1, 1, 1, 0), outline=18, ocolor=BLUE)
    c.shape(1570, 700, 480, 480, radius=80, color=(1, 1, 1, 1), shadow=((0.1, 0.1, 0.3, 0.35), (0, -30), 60, 0))
    c.shape(975, 1120, 1200, 120, mode="pill", color=hexc("7B5CFF", 0.7), soft=50)

def draw_batching(c):
    rng = np.random.default_rng(7)
    for _ in range(160):
        size = 30 + rng.random() * 90
        x, y = 140 + rng.random() * 1670, 330 + rng.random() * 880
        hue = rng.random()
        import colorsys
        col = colorsys.hsv_to_rgb(hue, 0.6, 0.95) + (1,)
        c.shape(x, y, size * (0.8 + rng.random()), size, radius=rng.random() * size * 0.5, color=col,
                outline=4 if rng.random() < 0.3 else 0, ocolor=(1, 1, 1, 1),
                shadow=((0, 0, 0, 0.2), (0, -4), 8, 0) if rng.random() < 0.5 else None)

if __name__ == "__main__":
    import sys
    only_keyart = "--keyart" in sys.argv
    cover(1950, 1300, "cover_1950x1300.png")
    card(420, 280, "card_420x280.png")
    cover(1200, 630, "social_1200x630.png")
    icon(160, "icon_160x160.png")
    if only_keyart:
        raise SystemExit(0)
    feature("screenshot_1_showcase.png", "Cards, pills, avatars, gradients", "One component, no sprites, no 9-slicing",
            lambda c: showcase(c, (1950 - 1080 * 1.05) / 2, 290, 1.05))
    feature("screenshot_2_corners.png", "Any corner, any radius", "Uniform, per-corner, pill and circle", draw_corners)
    feature("screenshot_3_effects.png", "Gradient · Outline · Shadow · Glow", "Crisp at every resolution", draw_effects)
    feature("screenshot_4_batching.png", "160 shapes, 1 draw call", "Every parameter lives in vertex data", draw_batching)
    print("done")
