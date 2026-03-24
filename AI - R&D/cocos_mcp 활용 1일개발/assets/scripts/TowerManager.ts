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
import { TowerType, TOWER_CONFIGS, upgradeCost, sellValue } from './TowerData';
import { Tower } from './Tower';
const { ccclass, property } = _decorator;

@ccclass('TowerManager')
export class TowerManager extends Component {
    @property(Prefab) towerPrefab:    Prefab | null = null;
    @property(Node)   towerContainer: Node   | null = null;

    private _selectedType: TowerType | null = null;
    private _typeButtons: Node[] = [];
    private _msgNode:    Node | null = null;
    private _infoPanel:  Node | null = null;
    private _towerNodes = new Map<string, Node>();   // cellKey → tower Node
    private _pauseBtn:   Node | null = null;
    private _speedBtn:   Node | null = null;

    // ── Lifecycle ─────────────────────────────────────────────────────────────────

    protected start(): void {
        // Clear leftover grid state from previous session
        grid.blocked.clear();
        grid.version++;

        const gm = GameManager.instance;
        if (gm && gm.state === GameState.IDLE) gm.reset();

        this._drawMapBorder();
        this._drawMarkers();
        this._createTypeButtons();
        this._createPauseButton();
        this._createSpeedButton();
        this.node.on(Node.EventType.TOUCH_END, this._onTouch, this);
    }

    // ── Map border & grid lines ───────────────────────────────────────────────────

    private _drawMapBorder(): void {
        const n   = new Node('MapBorder');
        n.layer   = this.node.layer;
        n.addComponent(UITransform).setContentSize(1280, 720);
        n.setPosition(Vec3.ZERO);
        const g   = n.addComponent(Graphics);

        g.strokeColor = new Color(255, 255, 255, 30);
        g.lineWidth   = 1;
        for (let c = 0; c <= GRID_W; c++) {
            const x = -640 + c * TILE;
            g.moveTo(x, -360); g.lineTo(x, 360);
        }
        for (let r = 0; r <= GRID_H; r++) {
            const y = -360 + r * TILE;
            g.moveTo(-640, y); g.lineTo(640, y);
        }
        g.stroke();

        g.strokeColor = new Color(200, 200, 220, 200);
        g.lineWidth   = 4;
        g.rect(-640, -360, 1280, 720);
        g.stroke();

        n.setParent(this.node);
    }

    // ── Spawn / Exit markers ──────────────────────────────────────────────────────

    private _drawMarkers(): void {
        this._makeMarker(gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row),
            new Color(60, 200, 60, 220), 'START');
        this._makeMarker(gridToCanvas(EXIT_CELL.col, EXIT_CELL.row),
            new Color(220, 50, 50, 220), 'EXIT');
    }

    private _makeMarker(pos: { x: number; y: number }, color: Color, label: string): void {
        const n  = new Node(label + 'Marker');
        n.layer  = this.node.layer;
        n.addComponent(UITransform).setContentSize(TILE, TILE);
        n.setPosition(new Vec3(pos.x, pos.y, 0));

        const g = n.addComponent(Graphics);
        g.fillColor = color;
        g.circle(0, 0, TILE * 0.4);
        g.fill();
        g.strokeColor = new Color(255, 255, 255, 200);
        g.lineWidth   = 3;
        g.circle(0, 0, TILE * 0.4);
        g.stroke();

        const ln  = new Node('Lbl');
        ln.layer  = this.node.layer;
        ln.addComponent(UITransform).setContentSize(TILE * 1.5, 24);
        ln.setPosition(new Vec3(0, -TILE * 0.55, 0));
        const lbl = ln.addComponent(Label);
        lbl.string   = label;
        lbl.fontSize = 16;
        lbl.color    = new Color(255, 255, 255, 255);

        ln.setParent(n);
        n.setParent(this.node);
    }

    // ── Tower type buttons ────────────────────────────────────────────────────────

    private _createTypeButtons(): void {
        const btnW  = 100;
        const btnH  = 70;
        const gap   = 8;
        const total = 5 * btnW + 4 * gap;
        const leftEdge = -total / 2;

        for (let i = 0; i < 5; i++) {
            const type = i as TowerType;
            const cfg  = TOWER_CONFIGS[type];
            const cx   = leftEdge + i * (btnW + gap) + btnW / 2;

            const btn   = new Node(`TypeBtn_${i}`);
            btn.layer   = this.node.layer;
            btn.addComponent(UITransform).setContentSize(btnW, btnH);
            btn.setPosition(new Vec3(cx, -310, 0));

            const g = btn.addComponent(Graphics);
            this._redrawTypeButton(g, type, false);

            // Label on a CHILD node (Graphics + Label on same node conflict)
            const ln    = new Node('BtnLbl');
            ln.layer    = this.node.layer;
            ln.addComponent(UITransform).setContentSize(btnW - 4, btnH - 4);
            ln.setPosition(Vec3.ZERO);
            const lbl   = ln.addComponent(Label);
            lbl.string  = `${cfg.name}\n${cfg.cost}G\n${cfg.desc}`;
            lbl.fontSize    = 11;
            lbl.lineHeight  = 14;
            lbl.color       = new Color(255, 255, 255, 255);
            ln.setParent(btn);

            btn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
                e.propagationStopped = true;
                this._selectType(type);
            }, this);

            btn.setParent(this.node);
            this._typeButtons[i] = btn;
        }
    }

    private _selectType(type: TowerType): void {
        if (this._selectedType === type) {
            this._selectedType = null;
        } else {
            this._selectedType = type;
            this._hideInfoPanel();
        }
        this._updateTypeButtonVisuals();
    }

    private _updateTypeButtonVisuals(): void {
        for (let i = 0; i < this._typeButtons.length; i++) {
            const btn = this._typeButtons[i];
            if (!btn || !btn.isValid) continue;
            const g = btn.getComponent(Graphics);
            if (!g) continue;
            this._redrawTypeButton(g, i as TowerType, this._selectedType === (i as TowerType));
        }
    }

    private _redrawTypeButton(g: Graphics, type: TowerType, selected: boolean): void {
        const cfg   = TOWER_CONFIGS[type];
        const [r, gb, b] = cfg.color;
        g.clear();

        if (selected) {
            g.fillColor = new Color(
                Math.min(255, r + 60),
                Math.min(255, gb + 60),
                Math.min(255, b + 60),
                220,
            );
            g.roundRect(-50, -35, 100, 70, 8);
            g.fill();
            g.strokeColor = new Color(80, 255, 80, 255);
            g.lineWidth   = 4;
            g.roundRect(-50, -35, 100, 70, 8);
            g.stroke();
        } else {
            g.fillColor = new Color(
                Math.floor(r * 0.3),
                Math.floor(gb * 0.3),
                Math.floor(b * 0.3),
                200,
            );
            g.roundRect(-50, -35, 100, 70, 8);
            g.fill();
            g.strokeColor = new Color(r, gb, b, 120);
            g.lineWidth   = 2;
            g.roundRect(-50, -35, 100, 70, 8);
            g.stroke();
        }
    }

    // ── Notification message ──────────────────────────────────────────────────────

    private _showMsg(text: string, color: Color = new Color(255, 220, 50, 255)): void {
        this._msgNode?.destroy();

        const n  = new Node('Msg');
        n.layer  = this.node.layer;
        n.addComponent(UITransform).setContentSize(480, 40);
        n.setPosition(new Vec3(0, -240, 0));

        const lbl    = n.addComponent(Label);
        lbl.string   = text;
        lbl.fontSize = 20;
        lbl.color    = color;

        n.setParent(this.node);
        this._msgNode = n;

        this.scheduleOnce(() => {
            if (n.isValid) n.destroy();
            if (this._msgNode === n) this._msgNode = null;
        }, 2);
    }

    // ── Info / Sell / Upgrade panel ───────────────────────────────────────────────

    private _showInfoPanel(towerNode: Node, key: string): void {
        this._hideInfoPanel();
        towerNode.getComponent(Tower)?.showRangeCircle();

        const tower = towerNode.getComponent(Tower);
        if (!tower) return;

        const type   = tower.towerType;
        const level  = tower.level;
        const cfg    = TOWER_CONFIGS[type];
        const sell   = sellValue(type, level);
        const canUp  = level < 10;

        // Calculate max affordable upgrades
        const gm = GameManager.instance;
        let maxUpLevels = 0;
        let tempLevel   = level;
        let tempGold    = gm?.gold ?? 0;
        while (tempLevel < 10) {
            const c = upgradeCost(type, tempLevel);
            if (c > tempGold) break;
            tempGold -= c;
            tempLevel++;
            maxUpLevels++;
        }

        // Panel layout
        // Rows: header(20) + [maxUpBtn(36) + upBtn(36)] if canUp + sellBtn(32) + padding
        const btnGap  = 8;
        const upGap   = canUp ? (36 + btnGap + 36 + btnGap) : 0;
        const panelH  = 16 + 20 + btnGap + upGap + 32 + 16; // top pad + header + gap + [up rows] + sell + bot pad

        // Smart position: prefer above tower, fall back to below if near top
        const towerPos = towerNode.getPosition();
        const aboveY   = towerPos.y + 50 + panelH * 0.5;
        const belowY   = towerPos.y - 50 - panelH * 0.5;
        const panelY   = (aboveY + panelH * 0.5 <= 350) ? aboveY : belowY;

        const panel  = new Node('InfoPanel');
        panel.layer  = this.node.layer;
        panel.addComponent(UITransform).setContentSize(160, panelH);
        panel.setPosition(new Vec3(towerPos.x, panelY, 0));

        // Background
        const bg = panel.addComponent(Graphics);
        bg.fillColor = new Color(20, 20, 40, 230);
        bg.roundRect(-80, -panelH / 2, 160, panelH, 8);
        bg.fill();
        bg.strokeColor = new Color(150, 150, 200, 200);
        bg.lineWidth   = 2;
        bg.roundRect(-80, -panelH / 2, 160, panelH, 8);
        bg.stroke();

        // Layout from top
        let curY = panelH / 2 - 16 - 10;  // start below top padding, center of header

        // Header
        this._addLabel(panel, `${cfg.name}  Lv.${level}`, 0, curY, 16, new Color(255, 220, 100, 255));
        curY -= 10 + btnGap;  // bottom of header + gap

        if (canUp) {
            // Max upgrade button
            curY -= 18;  // center of 36px button
            const upCost = upgradeCost(type, level);
            const spentOnMax = (gm?.gold ?? 0) - tempGold;
            const maxBtn = this._addButton(
                panel,
                maxUpLevels > 0
                    ? `최대 업그레이드\n→Lv.${tempLevel}  (${spentOnMax}G)`
                    : '골드 부족',
                0, curY, 144, 36,
                maxUpLevels > 0 ? new Color(30, 100, 200, 230) : new Color(80, 80, 80, 180),
            );
            if (maxUpLevels > 0) {
                maxBtn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
                    e.propagationStopped = true;
                    this._maxUpgradeTower(key);
                }, this);
            }
            curY -= 18 + btnGap;  // bottom of maxBtn + gap

            // Single upgrade button
            curY -= 18;
            const upBtn = this._addButton(
                panel,
                `업그레이드\n${upCost}G`,
                0, curY, 144, 36,
                new Color(40, 80, 180, 230),
            );
            upBtn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
                e.propagationStopped = true;
                this._upgradeTower(key);
            }, this);
            curY -= 18 + btnGap;
        }

        // Sell button
        curY -= 16;  // center of 32px button
        const sellBtn = this._addButton(
            panel, `판매  +${sell}G`, 0, curY, 144, 32,
            new Color(160, 40, 40, 220),
        );
        sellBtn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
            this._sellTower(key);
        }, this);

        panel.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
        }, this);

        panel.setParent(this.node);
        this._infoPanel = panel;
    }

    /** Label on its own node (no Graphics conflict) */
    private _addLabel(
        parent: Node, text: string,
        x: number, y: number,
        fs: number, color: Color,
    ): Node {
        const n  = new Node('Lbl');
        n.layer  = this.node.layer;
        n.addComponent(UITransform).setContentSize(152, fs + 6);
        n.setPosition(new Vec3(x, y, 0));
        const lbl    = n.addComponent(Label);
        lbl.string   = text;
        lbl.fontSize = fs;
        lbl.color    = color;
        n.setParent(parent);
        return n;
    }

    /** Button: Graphics bg node + child Label node */
    private _addButton(
        parent: Node, text: string,
        x: number, y: number,
        w: number, h: number,
        color: Color,
    ): Node {
        const n  = new Node('Btn');
        n.layer  = this.node.layer;
        n.addComponent(UITransform).setContentSize(w, h);
        n.setPosition(new Vec3(x, y, 0));

        const bg = n.addComponent(Graphics);
        bg.fillColor = color;
        bg.roundRect(-w / 2, -h / 2, w, h, 6);
        bg.fill();

        // Label on child node to avoid Graphics/Label render conflict
        const ln = new Node('Lbl');
        ln.layer = this.node.layer;
        ln.addComponent(UITransform).setContentSize(w - 4, h - 4);
        ln.setPosition(Vec3.ZERO);
        const lbl       = ln.addComponent(Label);
        lbl.string      = text;
        lbl.fontSize    = 13;
        lbl.lineHeight  = 16;
        lbl.color       = new Color(255, 255, 255, 255);
        ln.setParent(n);

        n.setParent(parent);
        return n;
    }

    private _hideInfoPanel(): void {
        // Hide range circle on currently shown tower
        if (this._infoPanel) {
            for (const [, tNode] of this._towerNodes) {
                if (tNode?.isValid) tNode.getComponent(Tower)?.hideRangeCircle();
            }
        }
        this._infoPanel?.destroy();
        this._infoPanel = null;
    }

    // ── Upgrade ───────────────────────────────────────────────────────────────────

    private _upgradeTower(key: string): void {
        const node = this._towerNodes.get(key);
        if (!node?.isValid) { this._hideInfoPanel(); return; }
        const tower = node.getComponent(Tower);
        if (!tower) return;
        if (tower.level >= 10) {
            this._showMsg('이미 최대 레벨입니다!');
            this._hideInfoPanel();
            return;
        }
        const cost = upgradeCost(tower.towerType, tower.level);
        const gm   = GameManager.instance;
        if (!gm || !gm.spendGold(cost)) {
            this._showMsg(`골드 부족! (필요: ${cost}G)`, new Color(255, 80, 80, 255));
            this._hideInfoPanel();
            return;
        }
        tower.upgrade();
        this._hideInfoPanel();
        this._showMsg(`업그레이드! Lv.${tower.level}`, new Color(100, 220, 255, 255));
    }

    private _maxUpgradeTower(key: string): void {
        const node = this._towerNodes.get(key);
        if (!node?.isValid) { this._hideInfoPanel(); return; }
        const tower = node.getComponent(Tower);
        if (!tower) return;
        const gm = GameManager.instance;
        if (!gm) return;

        let upgraded = 0;
        while (tower.level < 10) {
            const cost = upgradeCost(tower.towerType, tower.level);
            if (!gm.spendGold(cost)) break;
            tower.upgrade();
            upgraded++;
        }

        this._hideInfoPanel();
        if (upgraded > 0) {
            this._showMsg(`${upgraded}레벨 업! → Lv.${tower.level}`, new Color(100, 220, 255, 255));
        } else {
            this._showMsg('골드 부족!', new Color(255, 80, 80, 255));
        }
    }

    // ── Sell ──────────────────────────────────────────────────────────────────────

    private _sellTower(key: string): void {
        const node = this._towerNodes.get(key);
        if (!node?.isValid) { this._hideInfoPanel(); return; }
        const tower = node.getComponent(Tower);
        const sell  = tower ? sellValue(tower.towerType, tower.level) : 25;
        GameManager.instance?.addGold(sell);
        grid.blocked.delete(key);
        grid.version++;
        node.destroy();
        this._towerNodes.delete(key);
        this._hideInfoPanel();
        this._showMsg(`판매! +${sell}G`, new Color(100, 220, 100, 255));
    }

    // ── Pause button ─────────────────────────────────────────────────────────────

    private _createPauseButton(): void {
        const W = 90, H = 36;
        const btn = new Node('PauseBtn');
        btn.layer = this.node.layer;
        btn.addComponent(UITransform).setContentSize(W, H);
        btn.setPosition(new Vec3(545, 330, 0));

        btn.addComponent(Graphics);   // background only

        const ln = new Node('Lbl');
        ln.layer = this.node.layer;
        ln.addComponent(UITransform).setContentSize(W - 4, H - 4);
        ln.setPosition(Vec3.ZERO);
        ln.addComponent(Label).fontSize = 14;
        (ln.getComponent(Label) as Label).color = new Color(255, 255, 255, 255);
        ln.setParent(btn);

        this._pauseBtn = btn;
        this._refreshPauseBtn();

        btn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
            const gm = GameManager.instance;
            if (!gm) return;
            if (gm.state === GameState.PAUSED) gm.resume();
            else if (gm.state === GameState.PLAYING) gm.pause();
            this._refreshPauseBtn();
        }, this);

        btn.setParent(this.node);
    }

    private _refreshPauseBtn(): void {
        if (!this._pauseBtn?.isValid) return;
        const paused = GameManager.instance?.state === GameState.PAUSED;
        const W = 90, H = 36;
        const g = this._pauseBtn.getComponent(Graphics)!;
        g.clear();
        g.fillColor = paused ? new Color(50, 150, 50, 220) : new Color(160, 80, 20, 220);
        g.roundRect(-W / 2, -H / 2, W, H, 6);
        g.fill();
        const lbl = this._pauseBtn.getChildByName('Lbl')?.getComponent(Label);
        if (lbl) lbl.string = paused ? '재개' : '일시정지';
    }

    // ── Speed button ──────────────────────────────────────────────────────────────

    private _createSpeedButton(): void {
        const W = 70, H = 36;
        const btn = new Node('SpeedBtn');
        btn.layer = this.node.layer;
        btn.addComponent(UITransform).setContentSize(W, H);
        btn.setPosition(new Vec3(460, 330, 0));

        btn.addComponent(Graphics);

        const ln = new Node('Lbl');
        ln.layer = this.node.layer;
        ln.addComponent(UITransform).setContentSize(W - 4, H - 4);
        ln.setPosition(Vec3.ZERO);
        ln.addComponent(Label).fontSize = 14;
        (ln.getComponent(Label) as Label).color = new Color(255, 255, 255, 255);
        ln.setParent(btn);

        this._speedBtn = btn;
        this._refreshSpeedBtn();

        btn.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
            const gm = GameManager.instance;
            if (!gm) return;
            gm.setSpeed(gm.speed === 1 ? 2 : 1);
            this._refreshSpeedBtn();
        }, this);

        btn.setParent(this.node);
    }

    private _refreshSpeedBtn(): void {
        if (!this._speedBtn?.isValid) return;
        const is2x = GameManager.instance?.speed === 2;
        const W = 70, H = 36;
        const g = this._speedBtn.getComponent(Graphics)!;
        g.clear();
        g.fillColor = is2x ? new Color(180, 130, 20, 220) : new Color(40, 80, 150, 220);
        g.roundRect(-W / 2, -H / 2, W, H, 6);
        g.fill();
        const lbl = this._speedBtn.getChildByName('Lbl')?.getComponent(Label);
        if (lbl) lbl.string = is2x ? '>> 2배속' : '> 1배속';
    }

    // ── Map touch ─────────────────────────────────────────────────────────────────

    private _onTouch(e: EventTouch): void {
        const gm  = GameManager.instance;
        const ui  = e.getUILocation();
        const wx  = ui.x - 640;
        const wy  = ui.y - 360;
        const { col, row } = canvasToGrid(wx, wy);
        const key = cellKey(col, row);

        // Close info panel on any map click
        if (this._infoPanel) {
            this._hideInfoPanel();
            return;
        }

        // Not in build mode: tap tower to show info panel
        if (this._selectedType === null) {
            if (grid.blocked.has(key)) {
                const towerNode = this._towerNodes.get(key);
                if (towerNode) this._showInfoPanel(towerNode, key);
            }
            return;
        }

        // ── Build mode ────────────────────────────────────────────────────────────

        if (!gm || !this.towerPrefab || !this.towerContainer) return;

        const type = this._selectedType;
        const cfg  = TOWER_CONFIGS[type];

        if ((col === SPAWN_CELL.col && row === SPAWN_CELL.row) ||
            (col === EXIT_CELL.col  && row === EXIT_CELL.row)) {
            this._showMsg('여기엔 건설할 수 없습니다!');
            return;
        }

        if (grid.blocked.has(key)) {
            this._showMsg('이미 건설된 타워가 있습니다!');
            return;
        }

        grid.blocked.add(key);
        if (!isPathPossible(grid.blocked)) {
            grid.blocked.delete(key);
            this._showMsg('경로가 막힙니다! 다른 곳에 건설하세요.', new Color(255, 80, 80, 255));
            return;
        }

        if (!gm.spendGold(cfg.cost)) {
            grid.blocked.delete(key);
            this._showMsg(
                `골드 부족! (필요: ${cfg.cost}G, 보유: ${gm.gold}G)`,
                new Color(255, 80, 80, 255),
            );
            return;
        }

        grid.version++;
        const tilePos = gridToCanvas(col, row);
        const node    = instantiate(this.towerPrefab);
        this.towerContainer.addChild(node);
        node.setPosition(tilePos.x, tilePos.y, 0);

        const tower = node.getComponent(Tower);
        tower?.init(type);

        this._towerNodes.set(key, node);

        this._selectedType = null;
        this._updateTypeButtonVisuals();
    }
}
