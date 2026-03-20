import { _decorator, Component, Node, Prefab, instantiate } from 'cc';
import { Enemy } from './Enemy';
const { ccclass, property } = _decorator;

@ccclass('EnemySpawner')
export class EnemySpawner extends Component {
    @property(Prefab) enemyPrefab:     Prefab | null = null;
    @property(Node)   enemyContainer:  Node   | null = null;

    spawn(hpMult = 1, speedMult = 1): Enemy | null {
        if (!this.enemyPrefab || !this.enemyContainer) return null;
        const node = instantiate(this.enemyPrefab);
        this.enemyContainer.addChild(node);
        const enemy = node.getComponent(Enemy);
        enemy?.init(hpMult, speedMult);
        return enemy;
    }
}
