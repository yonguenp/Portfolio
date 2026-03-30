import { _decorator, Component, Node, Label, Sprite, SpriteFrame, Color,
         resources, director, tween, UIOpacity, ScrollView, Prefab, instantiate,
         UITransform, Layout } from 'cc';
import { ConstellationBookManager, ConstellationRecord } from './ConstellationBookManager';
const { ccclass, property } = _decorator;

/** 도감에 표시할 별자리 마스터 데이터 */
const CONSTELLATION_MASTER = [
    { name: '오리온자리',   wave: 1 },
    { name: '큰곰자리',     wave: 2 },
    { name: '카시오페이아', wave: 3 },
    { name: '사자자리',     wave: 4 },
    { name: '전갈자리',     wave: 5 },
    { name: '황소자리',     wave: 6 },
    { name: '은하의 심연',  wave: 7 },
];

/**
 * ConstellationBookScene - 별자리 도감 씬 컨트롤러 (신규 — spec_v3 NEW-02)
 *
 * 씬 구조 (에디터 연결 필요):
 *   - titleLabel:        "별자리 도감" 타이틀 Label
 *   - cardContainer:     카드 그리드가 배치될 부모 Node (ScrollView 내부 Content)
 *   - cardUnlockedPrefab: 완성된 카드 Prefab (card_constellation.svg 배경 포함)
 *   - cardLockedPrefab:  미완성 카드 Prefab (card_locked.svg 배경 포함)
 *   - backButton:        뒤로가기 버튼 Node
 *   - fadeOverlay:       씬 전환 페이드 오버레이 Node (UIOpacity 컴포넌트 필요)
 *
 * 카드 내 Label 구조 (Prefab 내부):
 *   unlocked card: nameLabel(별자리이름), waveLabel(Wave N 완성), dateLabel(날짜)
 *   locked card:   nameLabel("???"), waveLabel("미완성")
 */
@ccclass('ConstellationBookScene')
export class ConstellationBookScene extends Component {

    @property({ type: Label })
    titleLabel: Label | null = null;

    /** ScrollView의 content Node — 카드들이 여기에 추가됨 */
    @property({ type: Node })
    cardContainer: Node | null = null;

    /** 완성된 별자리 카드 Prefab (card_constellation.svg 배경) */
    @property({ type: Prefab })
    cardUnlockedPrefab: Prefab | null = null;

    /** 미완성 별자리 카드 Prefab (card_locked.svg 배경) */
    @property({ type: Prefab })
    cardLockedPrefab: Prefab | null = null;

    @property({ type: Node })
    backButton: Node | null = null;

    @property({ type: Node })
    fadeOverlay: Node | null = null;

    start() {
        if (this.titleLabel) this.titleLabel.string = '별자리 도감';

        this._applyGridLayout();
        this._fadeIn();
        this._buildCardGrid();
    }

    /** cardContainer에 GridLayout 적용 (4열 그리드) — V6-05 */
    private _applyGridLayout() {
        if (!this.cardContainer) return;
        let layout = this.cardContainer.getComponent(Layout);
        if (!layout) layout = this.cardContainer.addComponent(Layout);
        layout.type = Layout.Type.GRID;
        layout.startAxis = Layout.AxisDirection.HORIZONTAL;
        layout.cellSize.width = 210;
        layout.cellSize.height = 130;
        layout.spacingX = 10;
        layout.spacingY = 10;
        layout.paddingLeft = 10;
        layout.paddingRight = 10;
        layout.paddingTop = 10;
        layout.paddingBottom = 10;
        layout.constraint = Layout.Constraint.FIXED_COL;
        layout.constraintNum = 4;
    }

    private _fadeIn() {
        if (!this.fadeOverlay) return;
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) return;
        this.fadeOverlay.active = true;
        opacity.opacity = 255;
        tween(opacity)
            .to(0.4, { opacity: 0 })
            .call(() => { this.fadeOverlay!.active = false; })
            .start();
    }

    /**
     * 별자리 7종 카드 그리드 빌드
     * - 해금된 별자리: 완성 카드 (card_constellation 배경, 이름/Wave/날짜 표시)
     * - 미해금 별자리: 잠김 카드 (card_locked 배경, "???" 표시)
     */
    private _buildCardGrid() {
        if (!this.cardContainer) return;

        const records = ConstellationBookManager.getRecords();
        const recordMap = new Map<string, ConstellationRecord>();
        for (const r of records) recordMap.set(r.name, r);

        for (const master of CONSTELLATION_MASTER) {
            const record = recordMap.get(master.name);
            const isUnlocked = !!record;

            const prefab = isUnlocked ? this.cardUnlockedPrefab : this.cardLockedPrefab;
            if (!prefab) {
                // Prefab 미연결 시 레이블 노드로 대체
                this._createFallbackCard(master.name, isUnlocked, record);
                continue;
            }

            const cardNode = instantiate(prefab);
            this.cardContainer.addChild(cardNode);

            if (isUnlocked && record) {
                this._fillUnlockedCard(cardNode, record);
            } else {
                this._fillLockedCard(cardNode, master.name);
            }
        }
    }

    /** 완성 카드 내부 라벨 채우기 */
    private _fillUnlockedCard(cardNode: Node, record: ConstellationRecord) {
        const nameLabel = cardNode.getChildByName('nameLabel')?.getComponent(Label);
        const waveLabel = cardNode.getChildByName('waveLabel')?.getComponent(Label);
        const dateLabel = cardNode.getChildByName('dateLabel')?.getComponent(Label);

        if (nameLabel) nameLabel.string = record.name;
        if (waveLabel) waveLabel.string = `Wave ${record.wave} 완성`;
        if (dateLabel) {
            // ISO 날짜를 YYYY-MM-DD 형식으로 표시
            dateLabel.string = record.date.substring(0, 10);
        }
    }

    /** 잠김 카드 내부 라벨 채우기 */
    private _fillLockedCard(cardNode: Node, _name: string) {
        const nameLabel = cardNode.getChildByName('nameLabel')?.getComponent(Label);
        const waveLabel = cardNode.getChildByName('waveLabel')?.getComponent(Label);

        if (nameLabel) nameLabel.string = '???';
        if (waveLabel) waveLabel.string = '미완성';
    }

    /**
     * Prefab 미연결 시 동적 카드 생성 — Sprite 배경 + Label 3종 (V6-05 개선)
     * card_constellation.png / card_locked.png 리소스를 런타임 로드하여 배경으로 사용
     */
    private _createFallbackCard(name: string, isUnlocked: boolean, record?: ConstellationRecord) {
        if (!this.cardContainer) return;

        const node = new Node(isUnlocked ? `card_${name}` : `card_locked_${name}`);

        // UITransform 200×120
        const uiTransform = node.addComponent(UITransform);
        uiTransform.setContentSize(200, 120);

        // 배경 Sprite 동적 로드
        const bgPath = isUnlocked ? 'card_constellation/spriteFrame' : 'card_locked/spriteFrame';
        const sprite = node.addComponent(Sprite);
        resources.load(bgPath, SpriteFrame, (err, sf) => {
            if (!err && node.isValid) sprite.spriteFrame = sf;
        });

        // nameLabel
        const nameNode = new Node('nameLabel');
        node.addChild(nameNode);
        const nameUI = nameNode.addComponent(UITransform);
        nameUI.setContentSize(168, 14);
        nameNode.setPosition(4, 36, 0);
        const nameLabel = nameNode.addComponent(Label);
        nameLabel.string = isUnlocked ? name : '???';
        nameLabel.fontSize = 13;
        nameLabel.color = isUnlocked ? new Color(255, 215, 0, 255) : new Color(120, 120, 160, 255);

        // waveLabel
        const waveNode = new Node('waveLabel');
        node.addChild(waveNode);
        const waveUI = waveNode.addComponent(UITransform);
        waveUI.setContentSize(110, 11);
        waveNode.setPosition(4, 18, 0);
        const waveLabel = waveNode.addComponent(Label);
        waveLabel.string = isUnlocked && record ? `Wave ${record.wave} 완성` : '미완성';
        waveLabel.fontSize = 11;
        waveLabel.color = isUnlocked ? new Color(180, 160, 220, 255) : new Color(80, 80, 120, 255);

        // dateLabel (해금 카드만)
        if (isUnlocked && record) {
            const dateNode = new Node('dateLabel');
            node.addChild(dateNode);
            const dateUI = dateNode.addComponent(UITransform);
            dateUI.setContentSize(90, 11);
            dateNode.setPosition(4, 4, 0);
            const dateLabel = dateNode.addComponent(Label);
            dateLabel.string = record.date.substring(0, 10);
            dateLabel.fontSize = 10;
            dateLabel.color = new Color(140, 120, 180, 255);
        }

        this.cardContainer.addChild(node);
    }

    /** 뒤로가기 버튼 클릭 — TitleScene 복귀 */
    onBackButtonClicked() {
        if (!this.fadeOverlay) {
            director.loadScene('TitleScene');
            return;
        }
        const opacity = this.fadeOverlay.getComponent(UIOpacity);
        if (!opacity) { director.loadScene('TitleScene'); return; }
        this.fadeOverlay.active = true;
        opacity.opacity = 0;
        tween(opacity)
            .to(0.4, { opacity: 255 })
            .call(() => { director.loadScene('TitleScene'); })
            .start();
    }
}
