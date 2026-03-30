import { _decorator, Component, Node, Prefab, instantiate, CCInteger } from 'cc';
const { ccclass, property } = _decorator;

/**
 * ObjectPool - 범용 오브젝트 풀 유틸리티
 * - StarFragment, DarkStar 등 재사용 오브젝트 관리
 * - get() / put() 인터페이스
 */
@ccclass('ObjectPool')
export class ObjectPool extends Component {

    @property({ type: Prefab })
    prefab: Prefab | null = null;

    @property({ type: CCInteger })
    initialSize: number = 10;

    private _pool: Node[] = [];

    onLoad() {
        this._prepopulate();
    }

    private _prepopulate() {
        if (!this.prefab) return;
        for (let i = 0; i < this.initialSize; i++) {
            const node = this._createNode();
            node.active = false;
            this._pool.push(node);
        }
    }

    private _createNode(): Node {
        const node = instantiate(this.prefab!);
        this.node.addChild(node);
        return node;
    }

    /**
     * 풀에서 노드를 꺼낸다. 없으면 새로 생성.
     */
    get(): Node {
        let node = this._pool.find(n => !n.active);
        if (!node) {
            node = this._createNode();
        }
        node.active = true;
        return node;
    }

    /**
     * 노드를 풀로 반환한다.
     */
    put(node: Node) {
        node.active = false;
        if (!this._pool.includes(node)) {
            this._pool.push(node);
        }
    }

    get poolSize(): number { return this._pool.length; }
    get activeCount(): number { return this._pool.filter(n => n.active).length; }
}
