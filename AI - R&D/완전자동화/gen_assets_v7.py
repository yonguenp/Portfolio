#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Star Sweeper v7 — Full Asset Generator
Requires: pip install Pillow numpy
Run: py -3.13 gen_assets_v7.py
"""
from PIL import Image, ImageDraw, ImageFilter, ImageFont
import numpy as np
import math, os, random

RES = r"C:\Users\SANDBOX\Desktop\이직\Portfolio\AI - R&D\완전자동화\프로젝트\assets\resources"

def S(img, name):
    img.save(os.path.join(RES, name))
    print(f"  OK  {name}  ({img.size[0]}x{img.size[1]})")

def new(w, h):   return Image.new('RGBA', (w, h), (0, 0, 0, 0))
def comp(a, b):  return Image.alpha_composite(a, b)

def glow(img, cx, cy, r, rgb, blur, alpha=255):
    lay = new(*img.size)
    ImageDraw.Draw(lay).ellipse([cx-r, cy-r, cx+r, cy+r], fill=(*rgb, alpha))
    lay = lay.filter(ImageFilter.GaussianBlur(blur))
    return comp(img, lay)

def circ_mask(size, cx, cy, r):
    m = Image.new('L', size, 0)
    ImageDraw.Draw(m).ellipse([cx-r, cy-r, cx+r, cy+r], fill=255)
    return m

def radgrad(w, h, cx, cy, r0, r1, c0, c1):
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    d = np.sqrt((xx-cx)**2 + (yy-cy)**2)
    t = np.clip((d-r0) / max(r1-r0, 0.01), 0, 1)
    out = np.zeros((h, w, 4), np.float32)
    for i in range(4):
        out[:,:,i] = c0[i]*(1-t) + c1[i]*t
    return Image.fromarray(out.clip(0,255).astype(np.uint8), 'RGBA')

def rays(w, h, cx, cy, n, length, rgb, amax=190):
    yy, xx = np.mgrid[0:h, 0:w].astype(np.float32)
    acc = np.zeros((h, w), np.float32)
    rw2 = (length * 0.055) ** 2
    for i in range(n):
        a = i * 2 * math.pi / n
        dx, dy = math.cos(a), math.sin(a)
        along = (xx-cx)*dx + (yy-cy)*dy
        perp  = (xx-cx)*dy - (yy-cy)*dx
        t = np.clip(along / length, 0, 1)
        af = t * np.exp(-t * 2.6)
        pf = np.exp(-perp**2 / max(rw2, 1.0))
        acc += af * pf
    acc = np.clip(acc / n * 10, 0, 1)
    arr = np.zeros((h, w, 4), np.uint8)
    arr[:,:,0], arr[:,:,1], arr[:,:,2] = rgb
    arr[:,:,3] = (acc * amax).astype(np.uint8)
    return Image.fromarray(arr, 'RGBA')

def try_font(size):
    candidates = [
        r"C:\Windows\Fonts\impact.ttf",
        r"C:\Windows\Fonts\arialbd.ttf",
        r"C:\Windows\Fonts\arial.ttf",
        r"C:\Windows\Fonts\verdanab.ttf",
    ]
    for path in candidates:
        try:
            return ImageFont.truetype(path, size)
        except:
            pass
    return ImageFont.load_default()

# ═══════════════════════════════════════════════════════
#  STARS (normal)
# ═══════════════════════════════════════════════════════

def make_star(glow_rgb, core_rgb=(255,255,255), nrays=8, size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)

    img = glow(img, cx, cy, int(cx*.95), glow_rgb, cx*.50,  60)
    img = glow(img, cx, cy, int(cx*.68), glow_rgb, cx*.28, 145)
    img = glow(img, cx, cy, int(cx*.42), glow_rgb, cx*.14, 215)

    br = int(cx*.38)
    body = radgrad(W, H, cx, cy, 0, br, (*core_rgb,255), (*glow_rgb,245))
    body.putalpha(circ_mask((W,H), cx, cy, br))
    img = comp(img, body)

    img = comp(img, rays(W, H, cx, cy, nrays, int(cx*.94), glow_rgb, 200))

    img = glow(img, cx, cy, int(cx*.14), (255,255,255), cx*.10, 255)
    img = glow(img, cx, cy, int(cx*.06), (255,255,255), cx*.05, 255)

    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  STARS (special)
# ═══════════════════════════════════════════════════════

def make_star_rainbow(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    colors7 = [(255,0,0),(255,140,0),(255,255,0),(0,210,60),(0,150,255),(100,0,255),(200,0,200)]
    yy, xx = np.mgrid[0:H,0:W].astype(np.float32)
    ang = np.arctan2(yy-cy, xx-cx)
    dist = np.sqrt((xx-cx)**2+(yy-cy)**2)
    ring = (dist >= cx*.50) & (dist <= cx*.93)
    for i, c in enumerate(colors7):
        a0 = -math.pi + i*(2*math.pi/7)
        a1 = a0 + 2*math.pi/7
        sec = ring & (ang >= a0) & (ang < a1)
        arr = np.zeros((H,W,4), np.float32)
        arr[:,:,:3] = c
        arr[:,:,3] = np.where(sec, 200, 0)
        seg = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
        seg = seg.filter(ImageFilter.GaussianBlur(cx*.12))
        img = comp(img, seg)
    img = glow(img, cx, cy, int(cx*.45), (255,255,255), cx*.22, 210)
    img = comp(img, rays(W, H, cx, cy, 6, int(cx*.92), (255,255,255), 160))
    img = glow(img, cx, cy, int(cx*.14), (255,255,255), cx*.09, 255)
    return img.resize(size, Image.LANCZOS)

def make_star_bomb(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    img = glow(img, cx, cy, int(cx*.88), (160,0,0),   cx*.48,  75)
    img = glow(img, cx, cy, int(cx*.62), (200,20,0),  cx*.26, 135)
    br = int(cx*.40)
    body = radgrad(W,H,cx,cy,0,br,(55,0,8,255),(110,10,18,245))
    body.putalpha(circ_mask((W,H),cx,cy,br))
    img = comp(img, body)
    # Cracks
    crack = new(W,H)
    cd = ImageDraw.Draw(crack)
    lw = max(3, int(cx*.045))
    crack_segs = [
        [(cx,cy),(cx-int(cx*.28),cy-int(cy*.32)),(cx-int(cx*.42),cy-int(cy*.20))],
        [(cx,cy),(cx+int(cx*.25),cy-int(cy*.33))],
        [(cx,cy),(cx+int(cx*.32),cy+int(cy*.22)),(cx+int(cx*.22),cy+int(cy*.42))],
        [(cx,cy),(cx-int(cx*.30),cy+int(cy*.28))],
    ]
    for seg in crack_segs:
        cd.line(seg, fill=(255,90,20,210), width=lw)
    crack = crack.filter(ImageFilter.GaussianBlur(1.8))
    img = comp(img, crack)
    # X mark
    xm = new(W,H); xd = ImageDraw.Draw(xm)
    s = int(cx*.20)
    xw = max(3, int(cx*.07))
    xd.line([(cx-s,cy-s),(cx+s,cy+s)], fill=(255,70,0,210), width=xw)
    xd.line([(cx+s,cy-s),(cx-s,cy+s)], fill=(255,70,0,210), width=xw)
    img = comp(img, xm)
    img = glow(img, cx, cy, int(cx*.12), (255,130,0), cx*.09, 235)
    return img.resize(size, Image.LANCZOS)

def make_star_speed(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    img = glow(img, cx, cy, int(cx*.90), (0,140,255),  cx*.46,  70)
    img = glow(img, cx, cy, int(cx*.68), (0,210,255),  cx*.30,  95)
    # Motion trail
    yy, xx = np.mgrid[0:H,0:W].astype(np.float32)
    trail_x = np.clip((cx - xx) / (cx*1.1), 0, 1)
    trail_d = np.abs(yy-cy) / (cy*0.32)
    trail_a = trail_x * np.exp(-trail_d**2) * 110
    tarr = np.zeros((H,W,4), np.float32)
    tarr[:,:,0]=0; tarr[:,:,1]=160; tarr[:,:,2]=255; tarr[:,:,3]=trail_a.clip(0,255)
    img = comp(img, Image.fromarray(tarr.clip(0,255).astype(np.uint8),'RGBA'))
    br = int(cx*.38)
    body = radgrad(W,H,cx,cy,0,br,(190,235,255,255),(0,160,255,245))
    body.putalpha(circ_mask((W,H),cx,cy,br))
    img = comp(img, body)
    # Lightning bolt
    bolt = new(W,H); bd = ImageDraw.Draw(bolt)
    bw = int(cx*.10); bh = int(cy*.42)
    pts = [(cx+bw//2,cy-bh),(cx-bw*2,cy-bh//6),(cx+bw*2,cy-bh//6),(cx-bw//2,cy+bh),(cx+bw*2,cy+bh//6),(cx-bw*2,cy+bh//6)]
    bd.polygon(pts, fill=(255,255,255,230))
    img = comp(img, bolt)
    img = glow(img, cx, cy, int(cx*.14), (220,245,255), cx*.10, 255)
    return img.resize(size, Image.LANCZOS)

def make_star_magnet(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    img = glow(img, cx, cy, int(cx*.90), (0,200,180), cx*.46,  70)
    img = glow(img, cx, cy, int(cx*.65), (0,225,205), cx*.30, 145)
    br = int(cx*.40)
    body = radgrad(W,H,cx,cy,0,br,(185,255,245,255),(0,185,165,245))
    body.putalpha(circ_mask((W,H),cx,cy,br))
    img = comp(img, body)
    # Field rings
    field = new(W,H); fd = ImageDraw.Draw(field)
    for rr in [int(cx*.58), int(cx*.74)]:
        for a_start in range(0,360,22):
            a0, a1 = math.radians(a_start), math.radians(a_start+13)
            x0=cx+rr*math.cos(a0); y0=cy+rr*math.sin(a0)
            x1=cx+rr*math.cos(a1); y1=cy+rr*math.sin(a1)
            fd.line([(x0,y0),(x1,y1)], fill=(0,255,225,100), width=max(2,int(cx*.04)))
    img = comp(img, field)
    # N-S U magnet
    mag = new(W,H); md = ImageDraw.Draw(mag)
    mw=int(cx*.38); mh=int(cy*.38); mt=max(3,int(cx*.075))
    md.rectangle([cx-mw,cy-mh,cx-mw+mt*2,cy], fill=(90,90,255,230))
    md.rectangle([cx+mw-mt*2,cy-mh,cx+mw,cy], fill=(255,90,90,230))
    # Arc connector
    for aa in range(0,181,5):
        a=math.radians(aa); r2=int((mw+mh)//2)
        x2=cx+int(r2*math.cos(math.radians(aa)))
        y2=cy+int(r2*math.sin(math.radians(aa)))
    md.arc([cx-mw,cy-mt,cx+mw,cy+mh], start=0, end=180, fill=(0,225,205,230), width=mt)
    img = comp(img, mag)
    img = glow(img, cx, cy, int(cx*.14), (205,255,250), cx*.10, 255)
    return img.resize(size, Image.LANCZOS)

def make_star_ghost(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    img = glow(img, cx, cy, int(cx*.90), (180,80,255), cx*.50,  55)
    img = glow(img, cx, cy, int(cx*.65), (205,105,255), cx*.30, 115)
    br = int(cx*.42)
    body = radgrad(W,H,cx,cy,0,br,(245,205,255,195),(185,85,255,168))
    body.putalpha(circ_mask((W,H),cx,cy,br))
    img = comp(img, body)
    # Drips
    drip = new(W,H)
    for dx, ds in [(-int(cx*.22),int(cx*.13)),(0,int(cx*.15)),(int(cx*.22),int(cx*.13))]:
        dy_t=cy+int(cy*.28); dy_b=cy+int(cy*.68)
        ImageDraw.Draw(drip).ellipse([cx+dx-ds,dy_t,cx+dx+ds,dy_b], fill=(205,125,255,95))
    drip = drip.filter(ImageFilter.GaussianBlur(cx*.08))
    img = comp(img, drip)
    # Eyes
    eyes = new(W,H); ed = ImageDraw.Draw(eyes)
    er = int(cx*.09)
    ed.ellipse([cx-int(cx*.19)-er,cy-er,cx-int(cx*.19)+er,cy+er], fill=(38,0,78,205))
    ed.ellipse([cx+int(cx*.19)-er,cy-er,cx+int(cx*.19)+er,cy+er], fill=(38,0,78,205))
    img = comp(img, eyes)
    img = glow(img, cx, cy, int(cx*.13), (235,205,255), cx*.10, 200)
    return img.resize(size, Image.LANCZOS)

def make_star_dark(size=(64,64)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    img = glow(img, cx, cy, int(cx*.88), (60,0,80),  cx*.46,  60)
    img = glow(img, cx, cy, int(cx*.60), (90,0,120), cx*.25, 120)
    br = int(cx*.40)
    body = radgrad(W,H,cx,cy,0,br,(30,0,50,255),(80,0,100,245))
    body.putalpha(circ_mask((W,H),cx,cy,br))
    img = comp(img, body)
    img = comp(img, rays(W,H,cx,cy,4,int(cx*.88),(90,0,120),80))
    img = glow(img, cx, cy, int(cx*.13), (180,50,220), cx*.09, 210)
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  SLOTS
# ═══════════════════════════════════════════════════════

def make_slot(rgb, size=(40,40)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    # Dark interior
    ir = int(cx*.75)
    interior = radgrad(W,H,cx,cy,0,ir,(12,8,30,210),(5,4,18,185))
    interior.putalpha(circ_mask((W,H),cx,cy,ir))
    img = comp(img, interior)
    # Glow ring
    img = glow(img, cx, cy, int(cx*.84), rgb, cx*.20,  90)
    img = glow(img, cx, cy, int(cx*.84), rgb, cx*.09, 165)
    # Crisp border ring using numpy
    yy, xx = np.mgrid[0:H,0:W].astype(np.float32)
    dist = np.sqrt((xx-cx)**2 + (yy-cy)**2)
    ro, ri = cx*.90, cx*.74
    ring_mask = (dist >= ri) & (dist <= ro)
    rarr = np.zeros((H,W,4), np.uint8)
    rarr[:,:,0], rarr[:,:,1], rarr[:,:,2] = rgb
    rarr[:,:,3] = np.where(ring_mask, 235, 0).astype(np.uint8)
    img = comp(img, Image.fromarray(rarr,'RGBA'))
    # Highlight arc (upper-left)
    ang = np.arctan2(yy-cy, xx-cx) * 180 / math.pi
    hl_r_out, hl_r_in = cx*.82, cx*.68
    hl_mask = (dist>=hl_r_in) & (dist<=hl_r_out) & ((ang>200)|(ang<-100))
    harr = np.zeros((H,W,4), np.uint8)
    harr[:,:,:3] = (255,255,255)
    harr[:,:,3]  = np.where(hl_mask, 100, 0).astype(np.uint8)
    img = comp(img, Image.fromarray(harr,'RGBA'))
    # Center dot
    dot = new(W,H)
    ImageDraw.Draw(dot).ellipse([cx-7,cy-7,cx+7,cy+7], fill=(*rgb,55))
    img = comp(img, dot)
    return img.resize(size, Image.LANCZOS)

def make_slot_empty(size=(40,40)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W, H)
    rgb = (100,100,130)
    ir = int(cx*.75)
    interior = radgrad(W,H,cx,cy,0,ir,(10,8,22,180),(5,4,14,150))
    interior.putalpha(circ_mask((W,H),cx,cy,ir))
    img = comp(img, interior)
    img = glow(img, cx, cy, int(cx*.84), rgb, cx*.18, 60)
    yy, xx = np.mgrid[0:H,0:W].astype(np.float32)
    dist = np.sqrt((xx-cx)**2+(yy-cy)**2)
    # Dashed ring
    ang = np.arctan2(yy-cy,xx-cx)*180/math.pi % 360
    ring_mask = (dist>=cx*.74) & (dist<=cx*.90) & ((ang%30)<15)
    rarr = np.zeros((H,W,4),np.uint8)
    rarr[:,:,:3] = rgb; rarr[:,:,3] = np.where(ring_mask,150,0).astype(np.uint8)
    img = comp(img, Image.fromarray(rarr,'RGBA'))
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  BUCKET
# ═══════════════════════════════════════════════════════

def make_bucket(size=(120,80)):
    K = 3
    W, H = size[0]*K, size[1]*K
    img = new(W, H)
    pad = 14
    top_y, bot_y = int(H*.08), int(H*.92)
    top_x1, top_x2 = pad, W-pad
    bot_w = int((W-pad*2)*.65)
    bot_x1, bot_x2 = (W-bot_w)//2, (W+bot_w)//2
    body_poly = [(top_x1,top_y),(top_x2,top_y),(bot_x2,bot_y),(bot_x1,bot_y)]
    # Body gradient
    yy, xx = np.mgrid[0:H,0:W].astype(np.float32)
    tv = np.clip((yy-top_y)/(bot_y-top_y),0,1)
    garr = np.zeros((H,W,4),np.float32)
    garr[:,:,0] = 18+tv*35; garr[:,:,1] = 8+tv*18; garr[:,:,2] = 52+tv*62; garr[:,:,3] = 238
    g_img = Image.fromarray(garr.clip(0,255).astype(np.uint8),'RGBA')
    body_mask = Image.new('L',(W,H),0)
    ImageDraw.Draw(body_mask).polygon(body_poly, fill=255)
    g_img.putalpha(body_mask)
    img = comp(img, g_img)
    # Inner glow
    img = glow(img, W//2, (top_y+bot_y)//2, int(W*.26), (155,85,255), W*.18, 125)
    # Rim glow
    rim_glow = new(W,H)
    ImageDraw.Draw(rim_glow).line([(top_x1,top_y),(top_x2,top_y)], fill=(255,215,0,200), width=max(3,int(H*.05)))
    rim_glow = rim_glow.filter(ImageFilter.GaussianBlur(3))
    img = comp(img, rim_glow)
    # Gold borders
    brd = new(W,H); bd = ImageDraw.Draw(brd)
    lw = max(2,int(W*.022))
    gc = (210,175,0,225)
    bd.line([(top_x1,top_y),(bot_x1,bot_y)], fill=gc, width=lw)
    bd.line([(top_x2,top_y),(bot_x2,bot_y)], fill=gc, width=lw)
    bd.line([(bot_x1,bot_y),(bot_x2,bot_y)], fill=gc, width=lw)
    bd.line([(top_x1,top_y),(top_x2,top_y)], fill=(255,220,0,240), width=lw+1)
    img = comp(img, brd)
    # Highlight shine
    hl = new(W,H)
    hl_pts = [(top_x1,top_y),(top_x1+int((top_x2-top_x1)*.32),top_y),
              (bot_x1+int(bot_w*.20),bot_y-int(H*.14)),(bot_x1,bot_y-int(H*.18)),(top_x1,top_y+int(H*.32))]
    ImageDraw.Draw(hl).polygon(hl_pts, fill=(255,255,255,20))
    img = comp(img, hl)
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  BACKGROUND
# ═══════════════════════════════════════════════════════

def make_bg_space(w=1280, h=720):
    rng = np.random.default_rng(42)
    yy = np.linspace(0,1,h).reshape(-1,1)*np.ones((1,w))
    arr = np.zeros((h,w,4),np.uint8)
    arr[:,:,0] = (4  + yy*4).astype(np.uint8)
    arr[:,:,1] = (2  + yy*3).astype(np.uint8)
    arr[:,:,2] = (16 + yy*14).astype(np.uint8)
    arr[:,:,3] = 255
    img = Image.fromarray(arr,'RGBA')
    # Nebulae
    for nx,ny,nrx,nry,nc in [
        (int(w*.24),int(h*.38),int(w*.42),int(h*.52),(28,6,80,58)),
        (int(w*.76),int(h*.28),int(w*.38),int(h*.46),(6,18,82,48)),
        (int(w*.50),int(h*.72),int(w*.32),int(h*.36),(38,8,72,42)),
        (int(w*.14),int(h*.68),int(w*.24),int(h*.30),(8,28,82,36)),
        (int(w*.86),int(h*.62),int(w*.26),int(h*.28),(48,4,62,32)),
        (int(w*.50),int(h*.15),int(w*.50),int(h*.20),(12,5,55,28)),
    ]:
        neb = new(w,h)
        ImageDraw.Draw(neb).ellipse([nx-nrx//2,ny-nry//2,nx+nrx//2,ny+nry//2], fill=nc)
        neb = neb.filter(ImageFilter.GaussianBlur(min(nrx,nry)//3))
        img = comp(img, neb)
    # Far stars
    far = new(w,h); fd = ImageDraw.Draw(far)
    xs=rng.integers(0,w,400); ys=rng.integers(0,h,400); als=rng.integers(50,130,400)
    for x,y,a in zip(xs,ys,als):
        fd.ellipse([x-1,y-1,x+1,y+1], fill=(200,210,255,int(a)))
    img = comp(img, far)
    # Bright stars with glow
    near = new(w,h); nd = ImageDraw.Draw(near)
    bx=rng.integers(0,w,90); by=rng.integers(0,h,90); br=rng.integers(1,3,90); bas=rng.integers(160,255,90)
    cols=[(255,255,255),(200,220,255),(255,242,205),(220,202,255)]
    for i,(x,y,r,a) in enumerate(zip(bx,by,br,bas)):
        c=cols[i%4]
        nd.ellipse([x-r,y-r,x+r,y+r], fill=(*c,int(a)))
    img = comp(img, near)
    # Glowing bright stars
    glx=rng.integers(0,w,14); gly=rng.integers(0,h,14)
    gcols=[(180,205,255),(255,242,185),(205,255,225)]
    for i,(x,y) in enumerate(zip(glx,gly)):
        img = glow(img,int(x),int(y), 3, gcols[i%3], 4, 200)
    return img

def make_bg_book(w=1280, h=720):
    rng = np.random.default_rng(77)
    yy = np.linspace(0,1,h).reshape(-1,1)*np.ones((1,w))
    arr = np.zeros((h,w,4),np.uint8)
    arr[:,:,0] = (8  + yy*10).astype(np.uint8)
    arr[:,:,1] = (4  + yy*6).astype(np.uint8)
    arr[:,:,2] = (22 + yy*20).astype(np.uint8)
    arr[:,:,3] = 255
    img = Image.fromarray(arr,'RGBA')
    for nx,ny,nrx,nry,nc in [
        (int(w*.50),int(h*.50),int(w*.80),int(h*.80),(35,10,90,50)),
        (int(w*.20),int(h*.30),int(w*.40),int(h*.40),(15,5,70,35)),
        (int(w*.80),int(h*.70),int(w*.35),int(h*.40),(50,12,80,30)),
    ]:
        neb = new(w,h)
        ImageDraw.Draw(neb).ellipse([nx-nrx//2,ny-nry//2,nx+nrx//2,ny+nry//2], fill=nc)
        neb = neb.filter(ImageFilter.GaussianBlur(min(nrx,nry)//3))
        img = comp(img, neb)
    st = new(w,h); sd = ImageDraw.Draw(st)
    xs=rng.integers(0,w,250); ys=rng.integers(0,h,250); als=rng.integers(40,110,250)
    for x,y,a in zip(xs,ys,als):
        sd.ellipse([x-1,y-1,x+1,y+1],fill=(210,200,255,int(a)))
    img = comp(img,st)
    return img

# ═══════════════════════════════════════════════════════
#  UI ELEMENTS
# ═══════════════════════════════════════════════════════

def make_button(size=(240,70)):
    K = 3
    W, H = size[0]*K, size[1]*K
    img = new(W, H)
    rx = int(H*.36)
    # Outer glow
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(60,100,200,80))
    og = og.filter(ImageFilter.GaussianBlur(H*.18))
    img = comp(img, og)
    # Body
    body = new(W,H)
    ImageDraw.Draw(body).rounded_rectangle([0,0,W,H],radius=rx,fill=(20,20,68,225))
    img = comp(img, body)
    # Top highlight
    hl_h = int(H*.40)
    harr = np.zeros((H,W,4),np.float32)
    harr[:hl_h,:,3] = np.linspace(32,0,hl_h).reshape(-1,1)
    harr[:,:,:3] = 255
    hl = Image.fromarray(harr.clip(0,255).astype(np.uint8),'RGBA')
    mask = Image.new('L',(W,H),0)
    ImageDraw.Draw(mask).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    hl.putalpha(Image.composite(hl.split()[3],Image.new('L',(W,H),0),mask))
    img = comp(img, hl)
    # Outer border
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(80,130,210,190),width=max(2,int(H*.042)))
    brd = brd.filter(ImageFilter.GaussianBlur(1.5))
    img = comp(img, brd)
    # Inner border
    ibrd = new(W,H)
    m = int(H*.042)
    ImageDraw.Draw(ibrd).rounded_rectangle([m,m,W-m,H-m],radius=max(1,rx-m),outline=(130,170,230,95),width=max(1,int(H*.028)))
    img = comp(img, ibrd)
    return img.resize(size, Image.LANCZOS)

def make_logo(size=(640,120)):
    W, H = size
    img = new(W, H)
    font = try_font(int(H*.68))
    text = "STAR SWEEPER"
    # Measure
    tmp = ImageDraw.Draw(new(W*2,H))
    bb = tmp.textbbox((0,0), text, font=font)
    tw, th = bb[2]-bb[0], bb[3]-bb[1]
    tx = (W-tw)//2 - bb[0]
    ty = (H-th)//2 - bb[1]
    # Glow passes
    for blur_r, a in [(20,55),(12,90),(6,120)]:
        gl = new(W,H)
        ImageDraw.Draw(gl).text((tx,ty), text, fill=(255,185,0,255), font=font)
        gl = gl.filter(ImageFilter.GaussianBlur(blur_r))
        # Apply alpha
        gl_arr = np.array(gl, dtype=np.float32)
        gl_arr[:,:,3] *= a/255
        gl = Image.fromarray(gl_arr.clip(0,255).astype(np.uint8),'RGBA')
        img = comp(img, gl)
    # Shadow
    sh = new(W,H)
    ImageDraw.Draw(sh).text((tx+4,ty+4), text, fill=(50,25,0,170), font=font)
    img = comp(img, sh)
    # Gold text
    gold = new(W,H)
    ImageDraw.Draw(gold).text((tx,ty), text, fill=(255,215,0,255), font=font)
    img = comp(img, gold)
    # Top highlight
    hl = new(W,H)
    ImageDraw.Draw(hl).text((tx,ty), text, fill=(255,245,150,160), font=font)
    hlarr = np.array(hl, dtype=np.float32)
    fade = np.linspace(1,0,H).reshape(-1,1)
    row_fade = np.where(np.arange(H) < H//3, np.linspace(1,0,H)[:H], 0).reshape(-1,1)
    hlarr[:,:,3] *= row_fade
    img = comp(img, Image.fromarray(hlarr.clip(0,255).astype(np.uint8),'RGBA'))
    # Decorative stars
    for px in [18, W-18]:
        s = make_star((255,200,50),nrays=8,size=(22,22))
        img.paste(s,(px-11,H//2-11),s)
    # Bottom line
    ld = ImageDraw.Draw(img)
    ld.line([(70,H-9),(W//2-35,H-9)], fill=(255,190,0,120), width=1)
    ld.line([(W//2+35,H-9),(W-70,H-9)], fill=(255,190,0,120), width=1)
    s2 = make_star((255,200,50),nrays=6,size=(14,14))
    img.paste(s2,(W//2-7,H-16),s2)
    return img

def make_boss_warning(size=(480,120)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(H*.26)
    # Outer glow
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(210,0,0,90))
    og = og.filter(ImageFilter.GaussianBlur(H*.22))
    img = comp(img, og)
    # Body gradient
    yy = np.linspace(0,1,H).reshape(-1,1)*np.ones((1,W))
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0]=100+yy*62; arr[:,:,1]=4; arr[:,:,2]=4; arr[:,:,3]=215
    body_img = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0)
    ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    body_img.putalpha(bm)
    img = comp(img, body_img)
    # Scanlines
    sc = new(W,H)
    sd = ImageDraw.Draw(sc)
    for sy in range(0,H,max(3,H//22)):
        sd.line([(0,sy),(W,sy)], fill=(0,0,0,18), width=1)
    img = comp(img, sc)
    # Warning stripe edges
    st = new(W,H); std = ImageDraw.Draw(st)
    sw2 = int(W*.055)
    for sx in range(0, sw2*2, int(sw2*.8)):
        std.rectangle([sx,0,sx+sw2//2,H], fill=(255,185,0,55))
        std.rectangle([W-sx-sw2//2,0,W-sx,H], fill=(255,185,0,55))
    img = comp(img, st)
    # Edge glow lines
    for ly, la in [(int(H*.055),185),(int(H*.945),185)]:
        ln = new(W,H)
        ImageDraw.Draw(ln).line([(int(W*.05),ly),(int(W*.95),ly)], fill=(255,65,65,la), width=max(2,int(H*.045)))
        ln = ln.filter(ImageFilter.GaussianBlur(1.8))
        img = comp(img, ln)
    return img.resize(size, Image.LANCZOS)

def make_combo_popup(size=(280,80)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(H*.35)
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(160,80,255,80))
    og = og.filter(ImageFilter.GaussianBlur(H*.20))
    img = comp(img,og)
    yy = np.linspace(0,1,H).reshape(-1,1)*np.ones((1,W))
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0]=28+yy*10; arr[:,:,1]=8+yy*5; arr[:,:,2]=55+yy*25; arr[:,:,3]=215
    bimg = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0)
    ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    bimg.putalpha(bm); img = comp(img,bimg)
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(200,140,255,200),width=max(2,int(H*.04)))
    brd = brd.filter(ImageFilter.GaussianBlur(1.2))
    img = comp(img,brd)
    ibrd = new(W,H); m=int(H*.04)
    ImageDraw.Draw(ibrd).rounded_rectangle([m,m,W-m,H-m],radius=max(1,rx-m),outline=(160,100,220,90),width=max(1,int(H*.025)))
    img = comp(img,ibrd)
    return img.resize(size, Image.LANCZOS)

def make_progress_bg(size=(120,12)):
    K = 4
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = H//2
    bg = new(W,H)
    ImageDraw.Draw(bg).rounded_rectangle([0,0,W,H],radius=rx,fill=(18,18,58,220))
    img = comp(img,bg)
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(60,62,110,180),width=max(1,int(H*.1)))
    img = comp(img,brd)
    hl = new(W,H)
    hlarr = np.zeros((H,W,4),np.float32)
    hlarr[:H//3,:,3] = 25; hlarr[:H//3,:,:3] = 255
    hl_img = Image.fromarray(hlarr.clip(0,255).astype(np.uint8),'RGBA')
    hl_mask = Image.new('L',(W,H),0)
    ImageDraw.Draw(hl_mask).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    hl_img.putalpha(Image.composite(hl_img.split()[3],Image.new('L',(W,H),0),hl_mask))
    img = comp(img,hl_img)
    return img.resize(size, Image.LANCZOS)

def make_progress_fill(size=(120,12)):
    K = 4
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = H//2
    yy,xx = np.mgrid[0:H,0:W].astype(np.float32)
    tx = xx/(W-1)
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0] = 255
    arr[:,:,1] = 230-tx*80
    arr[:,:,2] = 80-tx*50
    arr[:,:,3] = 245
    grad = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0)
    ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    grad.putalpha(bm); img = comp(img,grad)
    # Shine
    sh = new(W,H)
    sharr = np.zeros((H,W,4),np.float32)
    sharr[:H//2,:,3] = np.linspace(55,0,H//2).reshape(-1,1)
    sharr[:,:,:3] = 255
    sh_img = Image.fromarray(sharr.clip(0,255).astype(np.uint8),'RGBA')
    sh_img.putalpha(Image.composite(sh_img.split()[3],Image.new('L',(W,H),0),bm))
    img = comp(img,sh_img)
    # Tip sparkle
    img = glow(img, W-H, H//2, int(H*.55), (255,240,120), H*.4, 200)
    return img.resize(size, Image.LANCZOS)

def make_icon_life(size=(36,36)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, int(H*.45)
    img = new(W,H)
    # Glow
    img = glow(img,cx,cy,int(cx*.88),(220,30,30),cx*.46,70)
    # Heart shape
    r = int(cx*.52)
    heart = new(W,H); hd = ImageDraw.Draw(heart)
    # Left circle
    lx, ly = cx - int(r*.55), cy - int(r*.20)
    hd.ellipse([lx-r//2,ly-r//2,lx+r//2,ly+r//2], fill=(220,30,40,240))
    # Right circle
    rx2, ry2 = cx + int(r*.55), cy - int(r*.20)
    hd.ellipse([rx2-r//2,ry2-r//2,rx2+r//2,ry2+r//2], fill=(220,30,40,240))
    # Bottom triangle
    hd.polygon([(cx-r,cy),(cx+r,cy),(cx,cy+int(r*1.35))], fill=(220,30,40,240))
    heart = heart.filter(ImageFilter.GaussianBlur(1.5))
    img = comp(img,heart)
    # Highlight
    hl = new(W,H); hld = ImageDraw.Draw(hl)
    hld.ellipse([lx-r//3,ly-r//3,lx+r//4,ly+r//4], fill=(255,160,170,130))
    img = comp(img,hl)
    return img.resize(size, Image.LANCZOS)

def make_icon_pause(size=(48,48)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W,H)
    img = glow(img,cx,cy,int(cx*.88),(60,160,220),cx*.44,70)
    circ = new(W,H)
    ImageDraw.Draw(circ).ellipse([cx-int(cx*.84),cy-int(cy*.84),cx+int(cx*.84),cy+int(cy*.84)],fill=(15,40,80,220))
    img = comp(img,circ)
    bars = new(W,H); bd = ImageDraw.Draw(bars)
    bw = int(W*.14); bh = int(H*.46); gap = int(W*.09)
    bd.rounded_rectangle([cx-gap-bw,cy-bh,cx-gap,cy+bh],radius=int(bw*.35),fill=(100,190,255,235))
    bd.rounded_rectangle([cx+gap,cy-bh,cx+gap+bw,cy+bh],radius=int(bw*.35),fill=(100,190,255,235))
    img = comp(img,bars)
    brd = new(W,H)
    ImageDraw.Draw(brd).ellipse([cx-int(cx*.86),cy-int(cy*.86),cx+int(cx*.86),cy+int(cy*.86)],outline=(80,160,220,200),width=max(2,int(cx*.06)))
    brd = brd.filter(ImageFilter.GaussianBlur(1.5))
    img = comp(img,brd)
    return img.resize(size, Image.LANCZOS)

def make_icon_book(size=(48,48)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W,H)
    img = glow(img,cx,cy,int(cx*.88),(200,160,50),cx*.44,70)
    bk = new(W,H); bd = ImageDraw.Draw(bk)
    bpad = int(W*.12)
    bw, bh = W-bpad*2, H-bpad*2
    bx1,by1 = bpad, bpad; bx2,by2 = W-bpad, H-bpad
    # Back cover
    bd.rounded_rectangle([bx1+int(bw*.08),by1,bx2,by2],radius=int(bw*.08),fill=(80,55,20,220))
    # Front cover
    bd.rounded_rectangle([bx1,by1,bx2-int(bw*.08),by2],radius=int(bw*.08),fill=(150,100,30,235))
    # Spine
    bd.rectangle([bx1,by1,bx1+int(bw*.14),by2],fill=(100,65,15,240))
    # Pages
    bd.rectangle([bx1+int(bw*.18),by1+int(bh*.08),bx2-int(bw*.06),by2-int(bh*.08)],fill=(240,230,210,220))
    # Lines on pages
    for liy in range(int(by1+bh*.20), int(by2-bh*.12), int(bh*.12)):
        bd.line([(bx1+int(bw*.22),liy),(bx2-int(bw*.10),liy)],fill=(180,165,140,160),width=max(1,int(bh*.03)))
    # Gold stars on cover
    bd.text((bx1+int(bw*.03),by1+int(bh*.12)),"★",fill=(255,215,0,200))
    img = comp(img,bk)
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  POWERUP ICONS
# ═══════════════════════════════════════════════════════

def make_icon_shield(size=(48,48)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W,H)
    r = int(min(cx,cy)*.86)
    pts = [(int(cx+r*math.cos(math.radians(i*60-30))),int(cy+r*math.sin(math.radians(i*60-30)))) for i in range(6)]
    gl = new(W,H); ImageDraw.Draw(gl).polygon(pts,fill=(55,115,255,100))
    gl = gl.filter(ImageFilter.GaussianBlur(r*.18)); img = comp(img,gl)
    body = new(W,H); bd = ImageDraw.Draw(body)
    bd.polygon(pts,fill=(18,55,180,235))
    in_pts = [(int(p[0]*.84+cx*.16),int(p[1]*.84+cy*.16)) for p in pts]
    bd.polygon(in_pts,fill=(38,95,220,185)); img = comp(img,body)
    sm = make_star((155,200,255),nrays=6,size=(int(size[0]*.46),int(size[1]*.46)))
    img.paste(sm,(W//2-sm.width//2,H//2-sm.height//2),sm)
    brd = new(W,H); ImageDraw.Draw(brd).polygon(pts,outline=(100,185,255,220),width=max(2,int(r*.08)))
    img = comp(img,brd)
    return img.resize(size, Image.LANCZOS)

def make_icon_slow(size=(48,48)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W,H)
    img = glow(img,cx,cy,int(cx*.86),(0,155,205),cx*.42,80)
    circ = new(W,H)
    ImageDraw.Draw(circ).ellipse([cx-int(cx*.84),cy-int(cy*.84),cx+int(cx*.84),cy+int(cy*.84)],fill=(8,38,78,225))
    img = comp(img,circ)
    hg = new(W,H); hd = ImageDraw.Draw(hg)
    hw,hh = int(cx*.56),int(cy*.62)
    hd.polygon([(cx-hw,cy-hh),(cx+hw,cy-hh),(cx,cy)],fill=(80,185,235,235))
    hd.polygon([(cx-hw,cy+hh),(cx+hw,cy+hh),(cx,cy)],fill=(80,185,235,235))
    lw = max(2,int(cx*.055))
    hd.line([(cx-hw,cy-hh),(cx+hw,cy-hh)],fill=(165,225,255,205),width=lw)
    hd.line([(cx-hw,cy+hh),(cx+hw,cy+hh)],fill=(165,225,255,205),width=lw)
    img = comp(img,hg)
    cr = new(W,H); cd = ImageDraw.Draw(cr)
    for ang in [0,60,120]:
        a = math.radians(ang)
        for mul in [-1,1]:
            lx2 = cx + mul*int(cx*.64)*math.cos(a)
            ly2 = cy + mul*int(cy*.64)*math.sin(a)
            cr2 = int(cx*.09)
            cd.ellipse([lx2-cr2,ly2-cr2,lx2+cr2,ly2+cr2],fill=(185,235,255,155))
    img = comp(img,cr)
    brd = new(W,H)
    ImageDraw.Draw(brd).ellipse([cx-int(cx*.86),cy-int(cy*.86),cx+int(cx*.86),cy+int(cy*.86)],outline=(0,205,255,185),width=max(2,int(cx*.065)))
    img = comp(img,brd)
    return img.resize(size, Image.LANCZOS)

def make_icon_wildcard(size=(48,48)):
    K = 4
    W, H = size[0]*K, size[1]*K
    cx, cy = W//2, H//2
    img = new(W,H)
    img = glow(img,cx,cy,int(cx*.90),(255,205,0),cx*.46,80)
    r_out,r_in = int(cx*.90),int(cx*.42)
    pts5 = [(int(cx+((r_out if i%2==0 else r_in)*math.cos(math.radians(i*36-90)))),
             int(cy+((r_out if i%2==0 else r_in)*math.sin(math.radians(i*36-90))))) for i in range(10)]
    yy,xx = np.mgrid[0:H,0:W].astype(np.float32)
    tx = xx/(W-1)
    garr = np.zeros((H,W,4),np.float32)
    garr[:,:,0]=255; garr[:,:,1]=235-tx*78; garr[:,:,2]=52-tx*28; garr[:,:,3]=255
    gimg = Image.fromarray(garr.clip(0,255).astype(np.uint8),'RGBA')
    sm5 = Image.new('L',(W,H),0); ImageDraw.Draw(sm5).polygon(pts5,fill=255)
    gimg.putalpha(sm5); img = comp(img,gimg)
    bolt = new(W,H); bld = ImageDraw.Draw(bolt)
    bw2,bh2 = int(cx*.11),int(cy*.48)
    bpts = [(cx+bw2//2,cy-bh2),(cx-bw2*2,cy-bh2//7),(cx+bw2*2,cy-bh2//7),
            (cx-bw2//2,cy+bh2),(cx+bw2*2,cy+bh2//7),(cx-bw2*2,cy+bh2//7)]
    bld.polygon(bpts,fill=(255,255,255,240)); img = comp(img,bolt)
    sp = new(W,H); spd = ImageDraw.Draw(sp)
    for pa,pr in [(45,.70),(135,.74),(225,.72),(315,.68)]:
        a=math.radians(pa); sx=cx+int(pr*cx*math.cos(a)); sy=cy+int(pr*cy*math.sin(a))
        spd.ellipse([sx-int(cx*.08),sy-int(cy*.08),sx+int(cx*.08),sy+int(cy*.08)],fill=(255,245,105,205))
    img = comp(img,sp)
    brd = new(W,H); ImageDraw.Draw(brd).polygon(pts5,outline=(255,245,105,185),width=max(2,int(cx*.055)))
    img = comp(img,brd)
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  EFFECTS
# ═══════════════════════════════════════════════════════

def make_shockwave(size=(96,96)):
    W, H = size
    cx, cy = W//2, H//2
    img = new(W,H)
    yy,xx = np.mgrid[0:H,0:W].astype(np.float32)
    dist = np.sqrt((xx-cx)**2+(yy-cy)**2)
    for rr,rw,ra in [(cx*.90,cx*.065,200),(cx*.64,cx*.055,140),(cx*.40,cx*.045,80)]:
        ring = np.exp(-((dist-rr)/rw)**2)
        arr = np.zeros((H,W,4),np.float32)
        arr[:,:,0]=155; arr[:,:,1]=225; arr[:,:,2]=255; arr[:,:,3]=ring*ra
        img = comp(img, Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA'))
    return img

def make_rainbow_burst(size=(128,128)):
    W, H = size
    cx, cy = W//2, H//2
    img = new(W,H)
    colors7=[(255,0,0),(255,140,0),(255,255,0),(0,210,60),(0,150,255),(100,0,255),(200,0,200)]
    yy,xx = np.mgrid[0:H,0:W].astype(np.float32)
    ang = np.arctan2(yy-cy,xx-cx)
    dist = np.sqrt((xx-cx)**2+(yy-cy)**2)
    for i,c in enumerate(colors7):
        a0 = -math.pi+i*(2*math.pi/7); a1 = a0+2*math.pi/7
        sec = (ang>=a0)&(ang<a1)
        falloff = np.exp(-dist/(cx*.62))*np.clip(dist/(cx*.14),0,1)
        arr = np.zeros((H,W,4),np.float32)
        arr[:,:,:3] = c; arr[:,:,3] = np.where(sec,falloff*225,0)
        img = comp(img, Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA'))
    img = glow(img,cx,cy,int(cx*.26),(255,255,255),cx*.15,255)
    for i in range(12):
        a=i*2*math.pi/12; r=cx*.82
        img = glow(img,int(cx+r*math.cos(a)),int(cy+r*math.sin(a)),int(cx*.045),(255,255,255),cx*.044,195)
    return img

def make_bomb_explode(size=(128,128)):
    W, H = size
    cx, cy = W//2, H//2
    rng = np.random.default_rng(123)
    img = new(W,H)
    img = glow(img,cx,cy,int(cx*.86),(255,85,0),cx*.46,62)
    img = glow(img,cx,cy,int(cx*.56),(255,145,0),cx*.26,105)
    burst = new(W,H); bd = ImageDraw.Draw(burst)
    for _ in range(18):
        a = rng.uniform(0,2*math.pi)
        rl = rng.uniform(cx*.38,cx*.86); rw3 = rng.uniform(cx*.10,cx*.26)
        ex,ey = int(cx+rl*math.cos(a)), int(cy+rl*math.sin(a))
        c2 = rng.choice([(255,225,0),(255,145,0),(255,65,0),(205,0,0)])
        av = int(rng.uniform(155,245))
        bd.ellipse([ex-int(rw3),ey-int(rw3*.58),ex+int(rw3),ey+int(rw3*.58)],fill=(*c2,av))
    img = comp(img,burst)
    smoke = new(W,H)
    for i in range(8):
        a = i*2*math.pi/8+math.pi/8
        r2 = rng.uniform(cx*.62,cx*.92)
        sx,sy = int(cx+r2*math.cos(a)), int(cy+r2*math.sin(a))
        sr2 = int(rng.uniform(cx*.08,cx*.18))
        ss = new(W,H); ImageDraw.Draw(ss).ellipse([sx-sr2,sy-sr2,sx+sr2,sy+sr2],fill=(78,68,68,95))
        ss = ss.filter(ImageFilter.GaussianBlur(sr2*.58)); smoke = comp(smoke,ss)
    img = comp(img,smoke)
    img = glow(img,cx,cy,int(cx*.22),(255,255,205),cx*.14,220)
    img = glow(img,cx,cy,int(cx*.10),(255,255,255),cx*.07,255)
    return img

def make_meteor_shower(size=(256,64)):
    W, H = size
    rng = np.random.default_rng(77)
    img = new(W,H)
    base = new(W,H); ImageDraw.Draw(base).rectangle([0,0,W,H],fill=(4,4,28,145))
    img = comp(img,base)
    for i in range(7):
        streak = new(W,H); sd = ImageDraw.Draw(streak)
        sx = int(rng.uniform(W*.04,W*.80)); sy = int(rng.uniform(0,H*.42))
        ln = int(rng.uniform(W*.18,W*.46)); a2 = rng.uniform(18,48); a = math.radians(a2)
        ex = sx+int(ln*math.cos(a)); ey = sy+int(ln*math.sin(a))
        c3 = rng.choice([(255,255,255),(205,225,255),(255,245,205)])
        sd.line([(sx,sy),(ex,ey)],fill=(*c3,195),width=max(1,int(H*.052)))
        streak = streak.filter(ImageFilter.GaussianBlur(1.4)); img = comp(img,streak)
        img = glow(img,sx,sy,int(H*.07),c3,H*.055,175)
    sf = new(W,H); sfd = ImageDraw.Draw(sf)
    for _ in range(32):
        px,py = rng.integers(0,W),rng.integers(0,H); pa = rng.integers(95,215)
        sfd.ellipse([px-1,py-1,px+1,py+1],fill=(205,225,255,int(pa)))
    img = comp(img,sf)
    return img

# ═══════════════════════════════════════════════════════
#  BOOK / CARD assets
# ═══════════════════════════════════════════════════════

def make_book_cover(size=(320,200)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(min(W,H)*.06)
    # Glow
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(100,60,200,70))
    og = og.filter(ImageFilter.GaussianBlur(W*.06)); img = comp(img,og)
    # Body gradient
    yy = np.linspace(0,1,H).reshape(-1,1)*np.ones((1,W))
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0]=14+yy*22; arr[:,:,1]=8+yy*12; arr[:,:,2]=45+yy*35; arr[:,:,3]=240
    bimg = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0); ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    bimg.putalpha(bm); img = comp(img,bimg)
    # Gold border
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(210,170,0,200),width=max(3,int(min(W,H)*.025)))
    brd = brd.filter(ImageFilter.GaussianBlur(1.2)); img = comp(img,brd)
    # Corner stars
    for px2,py2 in [(int(W*.10),int(H*.12)),(int(W*.90),int(H*.12)),(int(W*.10),int(H*.88)),(int(W*.90),int(H*.88))]:
        s = make_star((255,200,50),nrays=6,size=(28,28))
        img.paste(s,(px2-14,py2-14),s)
    # Center constellation symbol (decorative)
    for ang in range(0,360,36):
        a = math.radians(ang)
        r3 = int(min(W,H)*.28)
        x3 = W//2+int(r3*math.cos(a)); y3 = H//2+int(r3*math.sin(a))
        img = glow(img,x3,y3,int(min(W,H)*.028),(200,160,255),min(W,H)*.025,140)
        if ang%72==0:
            img = glow(img,x3,y3,int(min(W,H)*.042),(255,200,100),min(W,H)*.038,180)
    # Connect dots (lines)
    ln = new(W,H); ld = ImageDraw.Draw(ln)
    for ang in range(0,360,72):
        a0 = math.radians(ang); a1 = math.radians(ang+72)
        r3 = int(min(W,H)*.28)
        x0=W//2+int(r3*math.cos(a0)); y0=H//2+int(r3*math.sin(a0))
        x1=W//2+int(r3*math.cos(a1)); y1=H//2+int(r3*math.sin(a1))
        ld.line([(x0,y0),(x1,y1)],fill=(180,140,255,80),width=max(1,int(min(W,H)*.012)))
    img = comp(img,ln)
    return img.resize(size, Image.LANCZOS)

def make_card_constellation(size=(280,160)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(min(W,H)*.06)
    # Glow
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(200,155,0,65))
    og = og.filter(ImageFilter.GaussianBlur(W*.05)); img = comp(img,og)
    # Body
    yy = np.linspace(0,1,H).reshape(-1,1)*np.ones((1,W))
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0]=18+yy*18; arr[:,:,1]=10+yy*14; arr[:,:,2]=48+yy*28; arr[:,:,3]=238
    bimg = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0); ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    bimg.putalpha(bm); img = comp(img,bimg)
    # Gold border
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(220,178,0,205),width=max(2,int(min(W,H)*.022)))
    brd = brd.filter(ImageFilter.GaussianBlur(1.0)); img = comp(img,brd)
    # Left star icon area
    lcx,lcy = int(W*.20), H//2
    for r3 in [int(W*.14),int(W*.10)]:
        img = glow(img,lcx,lcy,r3,(255,210,50),r3*.4,70)
    s3 = make_star((255,200,50),nrays=8,size=(int(W*.22),int(W*.22)))
    img.paste(s3,(lcx-s3.width//2,lcy-s3.height//2),s3)
    # Label area placeholder lines
    ld = ImageDraw.Draw(img)
    tx1 = int(W*.38); lw2 = max(2,int(H*.03))
    for ly2,la2,llen in [(int(H*.28),180,int(W*.50)),(int(H*.48),120,int(W*.38)),(int(H*.65),100,int(W*.32))]:
        ld.rounded_rectangle([tx1,ly2-lw2,tx1+llen,ly2+lw2],radius=lw2,fill=(255,215,80,la2))
    # Checkmark
    ld.line([(int(W*.82),int(H*.68)),(int(W*.87),int(H*.78)),(int(W*.96),int(H*.56))],fill=(80,220,120,200),width=max(2,int(H*.04)))
    return img.resize(size, Image.LANCZOS)

def make_card_locked(size=(280,160)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(min(W,H)*.06)
    og = new(W,H)
    ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(80,80,120,55))
    og = og.filter(ImageFilter.GaussianBlur(W*.05)); img = comp(img,og)
    arr = np.zeros((H,W,4),np.float32)
    arr[:,:,0]=10; arr[:,:,1]=10; arr[:,:,2]=28; arr[:,:,3]=228
    bimg = Image.fromarray(arr.clip(0,255).astype(np.uint8),'RGBA')
    bm = Image.new('L',(W,H),0); ImageDraw.Draw(bm).rounded_rectangle([0,0,W,H],radius=rx,fill=255)
    bimg.putalpha(bm); img = comp(img,bimg)
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=(70,70,115,160),width=max(2,int(min(W,H)*.022)))
    img = comp(img,brd)
    # Lock icon
    lcx,lcy = int(W*.20), H//2
    lk = new(W,H); ld = ImageDraw.Draw(lk)
    lw3,lh3 = int(W*.08),int(H*.22)
    ld.rounded_rectangle([lcx-lw3,lcy,lcx+lw3,lcy+lh3],radius=int(lw3*.35),fill=(80,80,120,220))
    ld.arc([lcx-lw3,lcy-lh3*2,lcx+lw3,lcy],start=180,end=0,fill=(80,80,120,220),width=max(2,int(lw3*.45)))
    ld.ellipse([lcx-int(lw3*.28),lcy+int(lh3*.36),lcx+int(lw3*.28),lcy+int(lh3*.70)],fill=(50,50,90,235))
    img = comp(img,lk)
    # Label placeholders
    ld2 = ImageDraw.Draw(img)
    tx1 = int(W*.38); lw2 = max(2,int(H*.03))
    for ly2,la2,llen in [(int(H*.28),90,int(W*.45)),(int(H*.48),55,int(W*.32)),(int(H*.65),45,int(W*.28))]:
        ld2.rounded_rectangle([tx1,ly2-lw2,tx1+llen,ly2+lw2],radius=lw2,fill=(100,100,150,la2))
    return img.resize(size, Image.LANCZOS)

def make_book_entry(unlocked, size=(180,60)):
    K = 2
    W, H = size[0]*K, size[1]*K
    img = new(W,H)
    rx = int(H*.22)
    if unlocked:
        fill_c=(18,14,48,220); brd_c=(200,165,0,185); dot_c=(255,200,50)
    else:
        fill_c=(10,10,24,200); brd_c=(60,60,100,140); dot_c=(70,70,110)
    body = new(W,H)
    ImageDraw.Draw(body).rounded_rectangle([0,0,W,H],radius=rx,fill=fill_c)
    img = comp(img,body)
    if unlocked:
        og = new(W,H)
        ImageDraw.Draw(og).rounded_rectangle([0,0,W,H],radius=rx,fill=(180,140,0,50))
        og = og.filter(ImageFilter.GaussianBlur(H*.15)); img = comp(img,og)
    brd = new(W,H)
    ImageDraw.Draw(brd).rounded_rectangle([0,0,W,H],radius=rx,outline=brd_c,width=max(1,int(H*.042)))
    img = comp(img,brd)
    dot = new(W,H)
    ImageDraw.Draw(dot).ellipse([int(W*.06),H//2-int(H*.22),int(W*.06)+int(H*.44),H//2+int(H*.22)],fill=(*dot_c,200 if unlocked else 100))
    img = comp(img,dot)
    lns = ImageDraw.Draw(img)
    lw2 = max(1,int(H*.05)); tx2 = int(W*.20)
    a1 = 160 if unlocked else 80
    lns.rounded_rectangle([tx2,int(H*.22),tx2+int(W*.65),int(H*.22)+lw2*2],radius=lw2,fill=(220,200,80,a1) if unlocked else (80,80,110,a1))
    lns.rounded_rectangle([tx2,int(H*.56),tx2+int(W*.45),int(H*.56)+lw2],radius=lw2,fill=(180,160,60,a1//2) if unlocked else (60,60,90,a1//2))
    return img.resize(size, Image.LANCZOS)

# ═══════════════════════════════════════════════════════
#  MAIN
# ═══════════════════════════════════════════════════════

def main():
    os.makedirs(RES, exist_ok=True)
    print("\n=== Star Sweeper v7 Asset Generator ===\n")

    # Normal Stars
    print("[1/8] Stars — Normal")
    for fname, glow_c, core_c in [
        ('star_red.png',    (222,58,58),   (255,165,165)),
        ('star_blue.png',   (58,115,255),  (165,195,255)),
        ('star_yellow.png', (255,218,52),  (255,248,185)),
        ('star_green.png',  (52,208,92),   (165,255,195)),
        ('star_purple.png', (172,72,255),  (225,175,255)),
        ('star_dark.png',   (62,0,85),     (110,30,130)),
    ]:
        S(make_star(glow_c, core_c), fname)

    # Special Stars
    print("\n[2/8] Stars — Special")
    S(make_star_rainbow(), 'star_rainbow.png')
    S(make_star_bomb(),    'star_bomb.png')
    S(make_star_speed(),   'star_speed.png')
    S(make_star_magnet(),  'star_magnet.png')
    S(make_star_ghost(),   'star_ghost.png')

    # Slots
    print("\n[3/8] Slots")
    for fname, rgb in [
        ('slot_red.png',    (222,62,62)),
        ('slot_blue.png',   (62,122,255)),
        ('slot_yellow.png', (255,212,52)),
        ('slot_green.png',  (52,202,92)),
        ('slot_purple.png', (162,72,255)),
    ]:
        S(make_slot(rgb), fname)
    S(make_slot_empty(), 'slot_empty.png')

    # Game objects
    print("\n[4/8] Game Objects & Backgrounds")
    S(make_bucket(),     'bucket.png')
    S(make_bg_space(),   'bg_space.png')
    S(make_bg_book(),    'bg_book.png')

    # UI
    print("\n[5/8] UI Elements")
    S(make_button(),          'ui_button.png')
    S(make_logo(),            'logo_title.png')
    S(make_boss_warning(),    'ui_boss_warning.png')
    S(make_combo_popup(),     'ui_combo_popup.png')
    S(make_progress_bg(),     'ui_progress_bg.png')
    S(make_progress_fill(),   'ui_progress_fill.png')

    # Icons
    print("\n[6/8] Icons")
    S(make_icon_life(),  'icon_life.png')
    S(make_icon_pause(), 'icon_pause.png')
    S(make_icon_book(),  'icon_book.png')

    # Powerup Icons
    print("\n[7/8] Powerup Icons")
    S(make_icon_shield(),   'icon_powerup_shield.png')
    S(make_icon_slow(),     'icon_powerup_slow.png')
    S(make_icon_wildcard(), 'icon_powerup_wildcard.png')

    # Book & Card Assets
    print("\n[8/8] Book & Card Assets + Effects")
    S(make_book_cover(),            'book_cover.png')
    S(make_card_constellation(),    'card_constellation.png')
    S(make_card_locked(),           'card_locked.png')
    S(make_book_entry(True),        'book_entry_unlocked.png')
    S(make_book_entry(False),       'book_entry_locked.png')
    S(make_shockwave(),             'effect_shockwave.png')
    S(make_rainbow_burst(),         'effect_rainbow_burst.png')
    S(make_bomb_explode(),          'effect_bomb_explode.png')
    S(make_meteor_shower(),         'effect_meteor_shower.png')

    print(f"\n✓ 완료! 총 {37}종 PNG → {RES}\n")

if __name__ == '__main__':
    main()
