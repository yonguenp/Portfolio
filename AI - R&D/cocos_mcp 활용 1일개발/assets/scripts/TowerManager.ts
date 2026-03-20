import {
    _decorator, Component, Node, EventTouch, Prefab, instantiate,
    Label, Color, UITransform, Graphics, Vec3,
} from 'cc';
import { GameManager, GameState } from './GameManager';
import {
    grid, TILE, GRID_W, GRID_H,
    cellKey, canvasToGrid, gridToCanvas,
    SPAWN_CELL, EXIT_CELL,
    isPathPossible,
} from './PathFinder';
const { ccclass, property } = _decorator;

const TOWER_COST   = 50;
const MIN_PATH_GAP = 48; // kept for legacy; grid check now covers placement

@ccclass('TowerManager')
export class TowerManager extends Component {
    @property(Prefab) towerPrefab:    Prefab | null = null;
    @property(Node)   towerContainer: Node   | null = null;

    private _buildMode = false;
    private _btnLabel: Label | null = null;
    private _btnBg:    Graphics | null = null;

    protected start(): void {
        this._drawMarkers();
        this._createBuildButton();
        // Map-click listener on this node (full-screen overlay)
        this.node.on(Node.EventType.TOUCH_END, this._onTouch, this);
    }

    // ── Spawn / Exit visual markers ──────────────────────────────────────────────
    private _drawMarkers(): void {
        this._makeMarker(
            gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row),
            new Color(60, 200, 60, 220), 'START',
        );
        this._makeMarker(
            gridToCanvas(EXIT_CELL.col, EXIT_CELL.row),
            new Color(220, 50, 50, 220), 'EXIT',
        );
    }

    private _makeMarker(pos: { x: number; y: number }, color: Color, label: string): void {
        const n = new Node(label + 'Marker');
        n.layer  = this.node.layer;
        const tr = n.addComponent(UITransform);
        tr.setContentSize(TILE, TILE);
        n.setPosition(new Vec3(pos.x, pos.y, 0));

        const g = n.addComponent(Graphics);
        g.fillColor = color;
        g.circle(0, 0, TILE * 0.4);
        g.fill();
        g.strokeColor = new Color(255, 255, 255, 200);
        g.lineWidth = 3;
        g.circle(0, 0, TILE * 0.4);
        g.stroke();

        const ln = new Node('Lbl');
        ln.layer  = this.node.layer;
        const lt  = ln.addComponent(UITransform);
        lt.setContentSize(TILE * 1.5, 24);
        ln.setPosition(new Vec3(0, -TILE * 0.55, 0));
        const lbl = ln.addComponent(Label);
        lbl.string   = label;
        lbl.fontSize = 16;
        lbl.color    = new Color(255, 255, 255, 255);

        ln.setParent(n);
        n.setParent(this.node);
    }

    // ── Build-Tower button ────────────────────────────────────────────────────────
    private _createBuildButton(): void {
        const panelNode = new Node('BuildPanel');
        panelNode.layer = this.node.layer;
        const panelTr   = panelNode.addComponent(UITransform);
        panelTr.setContentSize(180, 56);
        panelNode.setPosition(new Vec3(0, -300, 0));

        const bg = panelNode.addComponent(Graphics);
        this._btnBg = bg;
        this._drawBtn(false);

        const labelNode = new Node('BuildLabel');
        labelNode.layer = this.node.layer;
        const labelTr   = labelNode.addComponent(UITransform);
        labelTr.setContentSize(180, 56);
        labelNode.setPosition(new Vec3(0, 0, 0));

        const lbl = labelNode.addComponent(Label);
        lbl.string     = `Build Tower\n(${TOWER_COST}G)`;
        lbl.fontSize   = 18;
        lbl.lineHeight = 22;
        lbl.color      = new Color(255, 255, 255, 255);
        this._btnLabel = lbl;

        labelNode.setParent(panelNode);
        panelNode.setParent(this.node); // child → rendered on top → touch hits before TowerManager

        panelNode.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
            this._toggleBuildMode();
        }, this);
    }

    private _drawBtn(active: boolean): void {
        const g = this._btnBg;
        if (!g) return;
        g.clear();
        g.fillColor = active
            ? new Color(80, 180, 80, 220)
            : new Color(40, 80, 160, 200);
        g.roundRect(-90, -28, 180, 56, 10);
        g.fill();
    }

    private _toggleBuildMode(): void {
        this._buildMode = !this._buildMode;
        this._drawBtn(this._buildMode);
        if (this._btnLabel) {
            this._btnLabel.string = this._buildMode
                ? `Placing...\n(${TOWER_COST}G)`
                : `Build Tower\n(${TOWER_COST}G)`;
        }
    }

    // ── Map touch → place tower ───────────────────────────────────────────────────
    private _onTouch(e: EventTouch): void {
        if (!this._buildMode) return;
        const gm = GameManager.instance;
        if (!gm || gm.state !== GameState.PLAYING) return;
        if (!this.towerPrefab || !this.towerContainer) return;

        const ui  = e.getUILocation();
        const wx  = ui.x - 640;          // canvas-local
        const wy  = ui.y - 360;

        const { col, row } = canvasToGrid(wx, wy);

        // Reject spawn/exit cells
        if ((col === SPAWN_CELL.col && row === SPAWN_CELL.row) ||
            (col === EXIT_CELL.col  && row === EXIT_CELL.row)) return;

        const key = cellKey(col, row);
        if (grid.blocked.has(key)) return;

        // Validate: would placing here still leave a path?
        grid.blocked.add(key);
        if (!isPathPossible(grid.blocked)) {
            grid.blocked.delete(key);
            return; // 경로 완전 차단 → 배치 거부
        }

        if (!gm.spendGold(TOWER_COST)) {
            grid.blocked.delete(key);
            return;
        }

        // Commit tower
        grid.version++;   // signals all enemies to recalculate their paths

        const tilePos = gridToCanvas(col, row);
        const node = instantiate(this.towerPrefab);
        this.towerContainer.addChild(node);
        node.setPosition(tilePos.x, tilePos.y, 0);

        // Exit build mode
        this._buildMode = false;
        this._drawBtn(false);
        if (this._btnLabel) {
            this._btnLabel.string = `Build Tower\n(${TOWER_COST}G)`;
        }
    }
}
