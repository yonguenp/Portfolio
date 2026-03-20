using UnityEngine;

public struct AABB
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public AABB(Vector2 position, Vector2 size)
    {
        float extentsX = size.x * 0.5f;
        float extentsY = size.y * 0.5f;
        
        minX = position.x - extentsX;
        maxX = position.x + extentsX;
        minY = position.y - extentsY;
        maxY = position.y + extentsY;
    }

    public static bool Intersect(AABB a, AABB b)
    {
        return a.minX < b.maxX &&
               a.maxX > b.minX &&
               a.minY < b.maxY &&
               a.maxY > b.minY;
    }

    // Helper to resolve overlap on Y axis specifically for standing on ground
    // Returns the amount to push 'up' to stay exactly on top.
    // Assumes A (player) is falling onto B (ground).
    public static bool TryResolveFloor(AABB player, AABB ground, out float floorY)
    {
        floorY = 0f;
        // Check horizontal overlap first
        if (player.minX < ground.maxX && player.maxX > ground.minX)
        {
            // If the player's bottom edge is reasonably close to the ground's top edge
            // We give a small tolerance so they don't clip through if moving fast
            if (player.minY <= ground.maxY && player.maxY >= ground.maxY)
            {
                floorY = ground.maxY;
                return true;
            }
            
            // Wall collision (hit the side of the chunk) 
            // If horizontal overlaps, but player didn't satisfy floor condition, they hit a wall.
            // But we handle wall death separately by checking just Intersect.
        }
        return false;
    }
}
