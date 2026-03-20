using UnityEngine;

/// <summary>
/// 아이템/적/파워업 통합 클래스.
/// Kenney 스프라이트 + 실제 코인 시트 애니메이션 + EffectManager 이펙트.
/// </summary>
public class RunnerItem : MonoBehaviour
{
    public bool    isEnemy     = false;
    public int     scoreValue  = 100;
    public Vector2 size        = new Vector2(0.8f, 0.8f);
    public string  powerUpType = "";   // "" | "shield" | "magnet"

    public bool isPowerUp => powerUpType.Length > 0;

    // ── 코인 애니메이션 ──────────────────────────────────────
    private static Sprite[] _coinFrames;   // 씬별 캐시 (무효화 체크 필수)
    private SpriteRenderer  _sr;
    private float  _animTimer;
    private int    _animFrame;
    private const float COIN_FPS = 14f;
    private const int   COIN_FRAME_COUNT = 11;  // 242px / 22px = 11프레임

    // ── 파워업 스프라이트 캐시 (절차적 유지) ─────────────────
    private static Sprite _shieldSprite;
    private static Sprite _magnetSprite;

    // 파워업 바운스
    private float _spawnY;
    private float _bobTimer;

    // 적 애니메이터
    private EnemySpriteAnimator _enemyAnim;

    void Start()
    {
        _spawnY = transform.position.y;
        SetupVisual();
        RunnerGameManager.Instance?.RegisterItem(this);
    }

    void Update()
    {
        var gm = RunnerGameManager.Instance;
        if (gm == null || !gm.isPlaying) return;

        // 스크롤
        transform.position += Vector3.left * gm.currentScrollSpeed * Time.deltaTime;

        // 코인 스프라이트 애니메이션
        if (!isEnemy && !isPowerUp && _coinFrames != null && _coinFrames.Length > 0)
        {
            _animTimer += Time.deltaTime * COIN_FPS;
            if (_animTimer >= 1f)
            {
                _animTimer -= 1f;
                _animFrame = (_animFrame + 1) % _coinFrames.Length;
                if (_sr) _sr.sprite = _coinFrames[_animFrame];
            }
        }

        // 파워업 바운스
        if (isPowerUp)
        {
            _bobTimer += Time.deltaTime * 2.8f;
            Vector3 p = transform.position;
            p.y = _spawnY + Mathf.Sin(_bobTimer) * 0.22f;
            transform.position = p;
        }

        if (transform.position.x < -22f) DestroyItem();
    }

    // ── 비주얼 세팅 ──────────────────────────────────────────
    void SetupVisual()
    {
        if (powerUpType == "shield")
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;
            if (_shieldSprite == null) _shieldSprite = BuildShieldSprite();
            _sr.sprite = _shieldSprite;
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        }
        else if (powerUpType == "magnet")
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;
            if (_magnetSprite == null) _magnetSprite = BuildMagnetSprite();
            _sr.sprite = _magnetSprite;
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        }
        else if (isEnemy)
        {
            // Kenney Zombie 스프라이트 + 애니메이터
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;
            _enemyAnim = gameObject.AddComponent<EnemySpriteAnimator>();
            _enemyAnim.FaceLeft();
            transform.localScale = new Vector3(size.x * 1.2f, size.y * 1.2f, 1f);
        }
        else
        {
            // 실제 코인 스프라이트 시트 사용
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 3;
            LoadCoinFrames();
            if (_coinFrames != null && _coinFrames.Length > 0)
                _sr.sprite = _coinFrames[0];
            transform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }

    // 코인 시트를 프레임 배열로 슬라이싱
    void LoadCoinFrames()
    {
        // 씬 재로드 시 스프라이트 무효화 체크 (static 캐시 갱신)
        if (_coinFrames != null && _coinFrames.Length > 0 && _coinFrames[0] != null) return;
        _coinFrames = null;

        // ── 방법 1: Sprite Editor로 슬라이싱된 스프라이트 직접 로드 ──
        var preSliced = Resources.LoadAll<Sprite>("Items/coin_sheet");
        if (preSliced != null && preSliced.Length > 0)
        {
            // 이름 끝 숫자 기준 정렬 (coin_sheet_0, coin_sheet_1 ... 또는 coin_0, coin_1 ...)
            System.Array.Sort(preSliced, (a, b) => {
                var ma = System.Text.RegularExpressions.Regex.Match(a.name, @"(\d+)$");
                var mb = System.Text.RegularExpressions.Regex.Match(b.name, @"(\d+)$");
                int ia = ma.Success ? int.Parse(ma.Value) : 0;
                int ib = mb.Success ? int.Parse(mb.Value) : 0;
                return ia.CompareTo(ib);
            });
            _coinFrames = preSliced;
            return;
        }

        // ── 방법 2: Texture2D 로드 후 수동 슬라이싱 (Read/Write 필요) ──
        var tex = Resources.Load<Texture2D>("Items/coin_sheet");
        if (tex == null)
        {
            Debug.LogWarning("[RunnerItem] coin_sheet 없음 - 절차적 코인");
            _coinFrames = new[] { BuildCoinSprite() };
            return;
        }

        try
        {
            const int FW = 22;   // 프레임 폭 고정 (242/11=22)
            int fh = tex.height;
            int count = Mathf.Max(1, tex.width / FW);
            _coinFrames = new Sprite[count];
            for (int i = 0; i < count; i++)
                _coinFrames[i] = Sprite.Create(tex,
                    new Rect(i * FW, 0, FW, fh), new Vector2(0.5f, 0.5f), 42f);
        }
        catch
        {
            // Read/Write 비활성 → 절차적 폴백
            Debug.LogWarning("[RunnerItem] coin_sheet Read/Write 비활성 - 절차적 코인");
            _coinFrames = new[] { BuildCoinSprite() };
        }
    }

    // ── 아이템 수집/처치 ─────────────────────────────────────
    public AABB GetAABB() => new AABB(transform.position, size);

    public void Collect()
    {
        // 이펙트
        EffectManager.Instance?.SpawnCoinCollect(transform.position);

        if (powerUpType == "shield")
        {
            PowerUpSystem.Instance?.ActivateShield();
            EffectManager.Instance?.SpawnPowerUp(transform.position, new Color(0f, 0.8f, 1f));
        }
        else if (powerUpType == "magnet")
        {
            PowerUpSystem.Instance?.ActivateMagnet();
            EffectManager.Instance?.SpawnPowerUp(transform.position, new Color(0.6f, 0.2f, 1f));
        }

        DestroyItem();
    }

    public void Die()
    {
        // 적 처치 이펙트
        if (isEnemy)
            EffectManager.Instance?.SpawnEnemyDefeat(transform.position);

        DestroyItem();
    }

    void DestroyItem()
    {
        RunnerGameManager.Instance?.UnregisterItem(this);
        Destroy(gameObject);
    }

    void OnDestroy() => RunnerGameManager.Instance?.UnregisterItem(this);

    void OnDrawGizmos()
    {
        Gizmos.color = isEnemy ? Color.red : isPowerUp ? Color.cyan : Color.yellow;
        Gizmos.DrawWireCube(transform.position, size);
    }

    // ══════════════════════════════════════════════════════════
    //  절차적 스프라이트 (폴백용 + 파워업 아이콘)
    // ══════════════════════════════════════════════════════════

    static Sprite BuildShieldSprite()
    {
        int w = 64, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        var pixels = new Color[w * h];
        Color clear = new Color(0,0,0,0);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color outer = new Color(0.0f, 0.8f, 0.9f);
        Color inner = new Color(0.3f, 1.0f, 1.0f);
        Color shine = Color.white;
        Color dark  = new Color(0.0f, 0.4f, 0.5f);
        float cx = w * 0.5f;

        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
        {
            float fx = x - cx, fy = y;
            float ny = (fy - h * 0.62f) / (h * 0.30f);
            float nx = fx / (w * 0.42f);
            float rTop = nx*nx + ny*ny;
            float vProg = (h * 0.62f - fy) / (h * 0.62f);
            float vWidth = (1f - vProg) * w * 0.42f;
            bool inV   = fy < h * 0.62f && Mathf.Abs(fx) < vWidth && fy > 0;
            bool inTop = rTop < 1f;
            if (!inTop && !inV) continue;
            bool border = (inTop && rTop > 0.72f) || (inV && (Mathf.Abs(fx) > vWidth - 3f || fy < 3f));
            float grad  = Mathf.Clamp01(fy / (h * 0.9f));
            Color fill  = Color.Lerp(inner, outer, grad);
            float shA   = Mathf.Clamp01(1f - (Mathf.Abs(fx+w*0.12f)+Mathf.Abs(fy-h*0.75f))/(w*0.20f));
            fill = Color.Lerp(fill, shine, shA * 0.45f);
            pixels[y*w+x] = border ? dark : fill;
            pixels[y*w+x].a = 1f;
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), w);
    }

    static Sprite BuildMagnetSprite()
    {
        int w = 64, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        var pixels = new Color[w * h];
        Color clear = new Color(0,0,0,0);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color body = new Color(0.55f,0.10f,0.80f);
        Color dark = new Color(0.30f,0.05f,0.50f);
        Color polN = new Color(0.95f,0.15f,0.10f);
        Color polS = new Color(0.10f,0.40f,0.95f);
        Color sh   = new Color(0.85f,0.65f,1.0f);
        float cx = w*0.5f, outerR = w*0.40f, innerR = w*0.24f;

        for (int y=0;y<h;y++) for (int x=0;x<w;x++)
        {
            float fx=x-cx, fy=y;
            bool inArm=false, isLeft=false;
            if (fx<-innerR&&fx>-outerR&&fy<h*0.55f){inArm=true;isLeft=true;}
            if (fx>innerR&&fx<outerR&&fy<h*0.55f){inArm=true;}
            float r=Mathf.Sqrt(fx*fx+(fy-h*0.5f)*(fy-h*0.5f));
            bool inArc=r>innerR&&r<outerR&&fy>h*0.45f;
            if(!inArm&&!inArc)continue;
            Color col;
            if(inArm){float tb=Mathf.Clamp01(1f-fy/(h*0.35f));col=isLeft?Color.Lerp(body,polN,tb):Color.Lerp(body,polS,tb);}
            else col=body;
            bool border=(inArc&&(r>outerR-3f||r<innerR+2f))||(inArm&&(Mathf.Abs(fx)>outerR-3f||Mathf.Abs(fx)<innerR+2f||fy<3f));
            float sa=Mathf.Clamp01(1f-(Mathf.Abs(fx+w*0.05f)+Mathf.Abs(fy-h*0.75f))/(w*0.25f));
            pixels[y*w+x]=border?dark:Color.Lerp(col,sh,sa*0.30f);
            pixels[y*w+x].a=1f;
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f), w);
    }

    // 폴백 코인 (coin_sheet 없을 때)
    static Sprite BuildCoinSprite()
    {
        int w=128,h=128;
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
        Color clear=new Color(0,0,0,0);
        Color[] pixels=new Color[w*h];
        float cx=w*0.5f,cy=h*0.5f,outerR=w*0.46f,innerR=w*0.38f;
        Color g1=new Color(1f,0.85f,0.10f),g2=new Color(0.9f,0.65f,0.05f);
        Color rim=new Color(0.7f,0.45f,0.02f),shine=new Color(1f,0.97f,0.75f);
        for(int i=0;i<pixels.Length;i++)
        {
            float fx=(i%w)-cx,fy=(i/w)-cy,r=Mathf.Sqrt(fx*fx+fy*fy);
            if(r>outerR){pixels[i]=clear;continue;}
            if(r>innerR){float t=(r-innerR)/(outerR-innerR);pixels[i]=Color.Lerp(g2,rim,t);pixels[i].a=1f;continue;}
            float grad=Mathf.Clamp01((fx+fy)/(innerR*1.4f)*0.5f+0.5f);
            Color b=Color.Lerp(g1,g2,grad);
            float sx=fx+innerR*0.3f,sy=fy+innerR*0.3f;
            float sa=Mathf.SmoothStep(0f,1f,Mathf.Clamp01(1f-Mathf.Sqrt(sx*sx*0.6f+sy*sy*0.6f)/(innerR*0.45f)))*0.7f;
            float ea=Mathf.Clamp01((innerR-r)*0.1f);
            pixels[i]=Color.Lerp(b,shine,sa);pixels[i].a=ea<1f?ea:1f;
        }
        tex.SetPixels(pixels);tex.Apply();
        return Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(0.5f,0.5f),w);
    }
}
