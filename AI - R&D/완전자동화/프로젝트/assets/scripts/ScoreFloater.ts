import { _decorator, Component, Node, Label, UIOpacity, tween, Vec3, Color, find } from 'cc';
import { GameManager } from './GameManager';
const { ccclass } = _decorator;

/**
 * ScoreFloater - 별 수집 시 "+score" 팝업 텍스트 연출 (V6-04)
 *
 * 사용법:
 *   ScoreFloater.show(catchWorldPos, scoreValue);
 *
 * - 수집 위치 근처에 "+N" 텍스트 노드를 런타임 동적 생성
 * - 1초간 Y방향으로 60px 부드럽게 부상 후 페이드아웃 → 자동 삭제
 * - 콤보 활성 시 황금색, 일반 시 흰색
 * - 최대 동시 표시 5개 (초과 시 가장 오래된 것 삭제)
 */
@ccclass('ScoreFloater')
export class ScoreFloater extends Component {

    private static _activeFloaters: Node[] = [];
    private static readonly MAX_FLOATERS = 5;

    /**
     * 수집 위치에 "+score" 팝업을 생성합니다.
     * @param worldPos  별이 수집된 월드 좌표 (Vec3)
     * @param score     표시할 점수 값
     */
    static show(worldPos: Vec3, score: number) {
        if (score <= 0) return;

        // 최대 개수 초과 시 가장 오래된 floater 즉시 제거
        if (ScoreFloater._activeFloaters.length >= ScoreFloater.MAX_FLOATERS) {
            const oldest = ScoreFloater._activeFloaters.shift();
            if (oldest && oldest.isValid) oldest.destroy();
        }

        // Canvas 루트 탐색 (런타임 동적 생성 노드를 Canvas 하위에 붙임)
        const canvas = find('Canvas');
        if (!canvas) return;

        // 노드 생성
        const node = new Node('ScoreFloater');
        canvas.addChild(node);

        // Label 추가
        const label = node.addComponent(Label);
        label.string = `+${score}`;
        label.fontSize = 28;
        label.isBold = true;

        // 콤보 여부에 따라 색상 결정
        const isCombo = (GameManager.instance?.comboCount ?? 0) >= 3;
        if (isCombo) {
            label.color = new Color(255, 215, 0, 255);   // 황금색
        } else {
            label.color = new Color(255, 255, 255, 255); // 흰색
        }

        // UIOpacity 추가 (페이드아웃용)
        const opacity = node.addComponent(UIOpacity);
        opacity.opacity = 255;

        // 위치 설정 (월드 좌표 → Canvas 로컬 좌표로 변환은 Canvas가 (0,0)이면 동일)
        node.setWorldPosition(worldPos.x, worldPos.y + 20, worldPos.z);

        // 트래킹 배열에 추가
        ScoreFloater._activeFloaters.push(node);

        // 애니메이션: 1초간 Y+60 부상 + 페이드아웃
        const startPos = node.getPosition().clone();
        tween(node)
            .to(1.0, { position: new Vec3(startPos.x, startPos.y + 60, startPos.z) })
            .start();

        tween(opacity)
            .delay(0.3)
            .to(0.7, { opacity: 0 })
            .call(() => {
                // 트래킹 배열에서 제거 후 노드 삭제
                const idx = ScoreFloater._activeFloaters.indexOf(node);
                if (idx !== -1) ScoreFloater._activeFloaters.splice(idx, 1);
                if (node.isValid) node.destroy();
            })
            .start();
    }
}
