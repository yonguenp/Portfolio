// ── Tile grid constants ────────────────────────────────────────────────────────
export const GRID_W = 20;   // 1280 / 64
export const GRID_H = 11;   // 704  / 64  (bottom 16px unused)
export const TILE   = 64;

// Enemy enters at left-edge, exits at right-edge
export const SPAWN_CELL = { col: 0,  row: 8 };  // canvas-local ≈ (-608, 168)
export const EXIT_CELL  = { col: 19, row: 2 };  // canvas-local ≈ (608, -200)

// ── Shared mutable grid state (read by Enemy, written by TowerManager) ─────────
export const grid = {
    blocked: new Set<string>(), // "col,row" keys of tower-occupied cells
    version: 0,                 // increments every time towers change
};

// ── Helpers ────────────────────────────────────────────────────────────────────
export function cellKey(col: number, row: number): string {
    return `${col},${row}`;
}

export function gridToCanvas(col: number, row: number): { x: number; y: number } {
    return {
        x: -640 + col * TILE + TILE * 0.5,
        y: -360 + row * TILE + TILE * 0.5,
    };
}

export function canvasToGrid(x: number, y: number): { col: number; row: number } {
    return {
        col: Math.max(0, Math.min(GRID_W - 1, Math.floor((x + 640) / TILE))),
        row: Math.max(0, Math.min(GRID_H - 1, Math.floor((y + 360) / TILE))),
    };
}

// ── A* from (startCol, startRow) to EXIT_CELL ──────────────────────────────────
const DIRS: [number, number][] = [[1, 0], [-1, 0], [0, 1], [0, -1]];

export function findPath(
    startCol: number, startRow: number,
    blocked: ReadonlySet<string>,
): Array<{ x: number; y: number }> | null {

    const ec = EXIT_CELL.col;
    const er = EXIT_CELL.row;

    if (startCol === ec && startRow === er) {
        return [gridToCanvas(ec, er)];
    }

    const h = (c: number, r: number) => Math.abs(c - ec) + Math.abs(r - er);

    const openSet  = new Set<string>();
    const cameFrom = new Map<string, string>();
    const gScore   = new Map<string, number>();
    const fScore   = new Map<string, number>();

    const sk = cellKey(startCol, startRow);
    openSet.add(sk);
    gScore.set(sk, 0);
    fScore.set(sk, h(startCol, startRow));

    while (openSet.size > 0) {
        // Pick node with lowest fScore
        let curKey = '';
        let curF   = Infinity;
        for (const k of openSet) {
            const f = fScore.get(k) ?? Infinity;
            if (f < curF) { curF = f; curKey = k; }
        }

        const parts = curKey.split(',');
        const cc = +parts[0];
        const cr = +parts[1];

        if (cc === ec && cr === er) {
            // Reconstruct path
            const path: Array<{ x: number; y: number }> = [];
            let k = curKey;
            while (cameFrom.has(k)) {
                const [c, r] = k.split(',').map(Number);
                path.unshift(gridToCanvas(c, r));
                k = cameFrom.get(k)!;
            }
            path.unshift(gridToCanvas(startCol, startRow));
            return path;
        }

        openSet.delete(curKey);

        for (const [dc, dr] of DIRS) {
            const nc = cc + dc;
            const nr = cr + dr;
            if (nc < 0 || nc >= GRID_W || nr < 0 || nr >= GRID_H) continue;
            const nk = cellKey(nc, nr);
            if (blocked.has(nk)) continue;

            const tg = (gScore.get(curKey) ?? 0) + 1;
            if (tg < (gScore.get(nk) ?? Infinity)) {
                cameFrom.set(nk, curKey);
                gScore.set(nk, tg);
                fScore.set(nk, tg + h(nc, nr));
                openSet.add(nk);
            }
        }
    }

    return null; // No path exists
}

// Quickly check if SPAWN → EXIT is reachable with the given blocked set
export function isPathPossible(blocked: ReadonlySet<string>): boolean {
    return findPath(SPAWN_CELL.col, SPAWN_CELL.row, blocked) !== null;
}
