
var KeyboardListener = cc.Class({
    extends: cc.Component,

    properties: {
        /**
         * Editbox를 갖는 node. 현재 노드라면 비워도 무방
         * @property {cc.EditBox} editBox
         */
        // editBox : {
        //     tooltip: CC_DEV && "Editbox를 포함한 Node.\n현재 노드라면 비워도 무방",
        //     default: null,
        //     type: cc.EditBox
        // },
        
        /**
         * 키보드 상단과 collider 하단이 겹치는 만큼 mover를 위로 올림
         * @property {cc.Node} collider 
         */
        collider : {
            tooltip: CC_DEV && "키보드와 충돌해 밀려날(키보드에 가려지지 않을) node.",
            default: null,
            type: cc.Node
        },

        /**
         * 위치 조정의 실제 대상이 되는 노드
         * @property {cc.Node} mover 
         */
        mover : {
            tooltip: CC_DEV && "키보드가 표시되면 이동할 최상위 노드.\n함께 움직일 요소를 모두 포함한 node를 선택.",
            default: null,
            type: cc.Node
        },

        /**
         * 여유 공간
         * @property {cc.Float} margin
         */
        margin : {
            tooltip: CC_DEV && "Collider와 키보드 상단의 간격.",
            default: 6,
            type: cc.Float
        }
    },
    
    onLoad : function() {
        var sam = window.Samanda;
        if (sam && false === (sam.isAndroid() || sam.isIPhone())){
            return;
        }

        // 누가 혹시 설정 제대로 안했을까봐 -> 그냥 설정 하지 않도록
        //this.editBox = this.editBox || this.node.getComponent(cc.EditBox);
        this.editBox = this.node.getComponent(cc.EditBox);
        if (null == this.editBox) {
            return;
        }
        this.mover = this.mover || this.editBox.node;
        this.collider = this.collider || this.editBox.node;
        
        // 현재 임시로 mover를 이동시킨 높이
        this.offset = 0;
        // 키보드 포커스 상태 변화가 있어서 updater에서 키보드 위치 보정이 필요한지
        this.needUpdate = false;

        // editbox에 listner 등록
        this.registerHandler("editingDidBegan", "onEditBegan");
        this.registerHandler("editingDidEnded", "onEditEnded");
    },
    
    start () {
        
    },

    registerHandler(type, method)
    {
        var handler = new cc.Component.EventHandler();
        handler.target = this.node;
        handler.component = "KeyboardListener";
        handler.handler = method;

        this.editBox[type].push(handler);
    },

    // editbox callbacks
    onEditBegan : function(eb) {
        this.needUpdate = true;
    },

    onEditEnded : function(eb) {
        this.cancelOffset();
    },

    update (dt) {
        if (!this.needUpdate) {
            return;
        }

        if (0 == this.offset && 0 != window.Samanda.getKeyboardHeight()) {
            this.applyOffset();
             this.needUpdate = false;
        }
    },

    // set offset
    applyOffset : function() {
        if (this.mover && 0 == this.offset) {
            // calculate minimal offset to avoid overwrap
            this.offset = this.getSafeOffset(window.Samanda.getKeyboardHeight());

            if (0 !== this.offset) {
                this.mover.y = this.mover.y + this.offset;
            }

            this.needUpdate = false;
        }
    },

    // reset offset
    cancelOffset : function() {
        if (this.mover && 0 !== this.offset) {
            this.mover.y = this.mover.y - this.offset;
            this.offset = 0;
            window.Samanda.setKeyboardHeight(0);

            this.needUpdate = false;
        }
    },

    // calculate minimal editbox clearance
    getSafeOffset : function(height) {
        if (!this.collider) {
            return 0;
        }

        var worldBL = this.collider.convertToWorldSpace(cc.v2(0, 0));
        var offset = height + this.margin - worldBL.y;
        
        return (0 < offset) ? offset : 0;
    }
});
