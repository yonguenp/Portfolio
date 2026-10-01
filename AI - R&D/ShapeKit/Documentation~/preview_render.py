# Python port of ShapeImage.OnPopulateMesh + ShapeImage.shader frag, for visual verification.
# Run from Documentation~: python3 preview_render.py  (needs numpy, pillow)
import numpy as np
from PIL import Image
W,H=1080,900
img=np.zeros((H,W,4)); img[...,:3]=np.array([0.95,0.96,0.98]); img[...,3]=1
ys,xs=np.mgrid[0:H,0:W].astype(float)

def sdf(px,py,hx,hy,r):  # r = TL,TR,BR,BL ; y up
    rr=np.where(px>0, np.where(py>0,r[1],r[2]), np.where(py>0,r[0],r[3]))
    qx=np.abs(px)-hx+rr; qy=np.abs(py)-hy+rr
    return np.minimum(np.maximum(qx,qy),0)+np.hypot(np.maximum(qx,0),np.maximum(qy,0))-rr

def blend(col,a):
    a=np.clip(a,0,1)[...,None]
    img[...,:3]=img[...,:3]*(1-a)+col*a

def smoothstep(e0,e1,x):
    t=np.clip((x-e0)/(e1-e0),0,1); return t*t*(3-2*t)

def shape(cx,cy,w,h,color=(1,1,1,1),mode='uniform',radius=16,radii=(16,16,16,16),grad=None,angle=90,
          outline=0,ocolor=(0,0,0,1),soft=0,shadow=None):
    hx,hy=w/2,h/2; m=min(hx,hy)
    r={'uniform':[radius]*4,'pill':[m]*4,'per':list(radii)}[mode]; r=[min(max(v,0),m) for v in r]
    if shadow:
        sc,off,blur,spread=shadow
        spread=max(spread,-m+0.5); shx,shy=hx+spread,hy+spread
        sr=[min(max(v+spread,0),min(shx,shy)) for v in r]
        px=xs-(cx+off[0]); py=-(ys-(cy-off[1]))
        d=sdf(px,py,shx,shy,sr); b=max(blur,1.0)
        blend(np.array(sc[:3]), sc[3]*color[3]*(1-smoothstep(-b,b,d)))
    px=xs-cx; py=-(ys-cy)
    d=sdf(px,py,hx,hy,r); aa=1.0
    fill=np.ones(d.shape+(4,))*np.array(color)
    if grad:
        rad=np.radians(angle); dx,dy=np.cos(rad),np.sin(rad)
        ext=abs(hx*dx)+abs(hy*dy); t=(px*dx+py*dy)/(2*ext)+0.5
        g=np.array(grad[0])+(np.array(grad[1])-np.array(grad[0]))*t[...,None]
        fill=fill*np.clip(g,0,1)
    o=np.array(ocolor)
    sa=np.clip(0.5-d/max(soft,aa),0,1)
    ia=np.clip(0.5-(d+min(outline,m))/aa,0,1) if outline>0 else np.ones_like(d)
    col=o*(1-ia[...,None])+fill*ia[...,None]
    blend(col[...,:3], col[...,3]*sa)

hexc=lambda h,a=1:(int(h[0:2],16)/255,int(h[2:4],16)/255,int(h[4:6],16)/255,a)
shape(540,170,900,260,radius=32,shadow=((0,0,0,0.15),(0,-10),24,0))
shape(240,170,160,160,color=hexc('3B2F4A'),mode='pill',outline=6,ocolor=(1,1,1,1),shadow=((0,0,0,0.3),(0,-4),8,0))
shape(640,170,480,130,color=hexc('4F8BFF'),mode='per',radii=(36,36,36,4))
shape(310,420,400,110,mode='pill',grad=(hexc('4F8BFF'),hexc('7B5CFF')),angle=0,shadow=((0.23,0.25,0.72,0.4),(0,-8),14,0))
shape(770,420,400,110,color=(1,1,1,0),radius=24,outline=4,ocolor=hexc('4F8BFF'))
for i in range(4):
    shape(210+i*220,620,180,180,radius=40,grad=(hexc('FF7A59'),hexc('FFD250')),angle=i*45)
shape(540,810,600,80,color=hexc('7B5CFF',0.6),mode='pill',soft=30)
Image.fromarray((np.clip(img,0,1)*255).astype(np.uint8)).save('preview.png')
print('saved')
