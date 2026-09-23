using UnityEngine;

namespace SamgukDefense
{
    /// <summary>무기 발사체 — 자기 자신을 스스로 굴린다(별도 코루틴 호스트 불필요, 공격자가
    /// 발사 직후 죽어도 발사체는 끝까지 날아간다). 도착하면 원래 지정된 타겟에게만 데미지를
    /// 주고 사라진다(스플래시는 별도 attackType, 발사체 자체는 단일 타격). 2026-09-16 3D 전환 —
    /// 바닥(Y=0)보다 약간 띄운 높이로 날아가야 "허공을 가른다"는 느낌이 난다.</summary>
    public class SamgukProjectile : MonoBehaviour
    {
        const float FlightHeight = 1.1f;

        Transform t;
        float cellSize;
        Vector2 boardPos;
        SamgukUnit target;
        float speed;
        float damage;
        const float HitDistance = 0.25f;

        // 착탄 시 발동하는 무기 명중 옵션 — SamgukUnit.ResolveWeaponHit과 완전히 같은 규칙을
        // 여기서도 그대로 적용한다(발사 순간이 아니라 "실제로 맞은 순간"에 효과가 나가야 하므로
        // 투사체 자신이 들고 다니다가 착탄할 때 쓴다).
        SamgukUnit shooter;
        float knockback;
        float stunChance, stunDuration;
        float bleedChance, bleedDamagePerTick, bleedDuration, bleedTickInterval;
        float lifestealPercent;

        public static void Spawn(Transform boardRoot, float cellSize, Vector2 fromBoardPos, SamgukUnit target, float speed, float damage, Color color,
            SamgukUnit shooter = null, float knockback = 0f, float stunChance = 0f, float stunDuration = 0f,
            float bleedChance = 0f, float bleedDamagePerTick = 0f, float bleedDuration = 0f, float bleedTickInterval = 0.5f, float lifestealPercent = 0f)
        {
            var go = new GameObject("Projectile");
            go.transform.SetParent(boardRoot, false);
            float size = Mathf.Max(0.08f, cellSize * 0.22f);
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProjectileSprite();
            sr.color = color;
            sr.sortingOrder = 15;

            var p = go.AddComponent<SamgukProjectile>();
            p.t = go.transform;
            p.cellSize = cellSize;
            p.boardPos = fromBoardPos;
            p.target = target;
            p.speed = speed;
            p.damage = damage;
            p.shooter = shooter;
            p.knockback = knockback;
            p.stunChance = stunChance;
            p.stunDuration = stunDuration;
            p.bleedChance = bleedChance;
            p.bleedDamagePerTick = bleedDamagePerTick;
            p.bleedDuration = bleedDuration;
            p.bleedTickInterval = bleedTickInterval;
            p.lifestealPercent = lifestealPercent;
            p.t.localPosition = new Vector3(fromBoardPos.x * cellSize, FlightHeight, fromBoardPos.y * cellSize);
        }

        static Sprite projectileSpriteCache;
        static Sprite ProjectileSprite()
        {
            if (projectileSpriteCache == null)
            {
                // Texture2D.whiteTexture는 "1×1"이 아니라 실제로는 4×4 픽셀이라(엔진 내부 폴백용,
                // 문서화된 보장이 아니다), pixelsPerUnit=1로 그대로 쓰면 스프라이트가 4배 부풀어
                // 렌더링된다(SamgukUnit.cs의 SolidWhiteTex 주석에 자세한 경위 — 캐릭터 크기가
                // 4배로 커 보이던 사고와 같은 원인, 2026-09-16). 진짜 1×1 텍스처를 직접 만든다.
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                projectileSpriteCache = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            }
            return projectileSpriteCache;
        }

        void Update()
        {
            if (target == null || !target.IsAlive) { Destroy(gameObject); return; }

            Vector2 to = target.BoardPosition - boardPos;
            float dist = to.magnitude;
            if (dist <= HitDistance)
            {
                Vector2 impactSourcePos = boardPos; // 투사체가 실제로 명중한 지점 — 넉백 방향 기준
                target.TakeDamage(damage);
                if (shooter != null && lifestealPercent > 0f) shooter.Heal(damage * lifestealPercent);
                if (knockback > 0f) target.ApplyKnockback(impactSourcePos, knockback);
                if (stunChance > 0f && Random.value <= stunChance) target.ApplyStun(stunDuration);
                if (bleedChance > 0f && Random.value <= bleedChance) target.ApplyBleed(bleedDamagePerTick, bleedDuration, bleedTickInterval);
                Destroy(gameObject);
                return;
            }
            boardPos += to.normalized * speed * Time.deltaTime;
            t.localPosition = new Vector3(boardPos.x * cellSize, FlightHeight, boardPos.y * cellSize);
            if (SamgukUnit.BillboardCamera != null) t.rotation = SamgukUnit.BillboardCamera.rotation;
        }
    }
}
