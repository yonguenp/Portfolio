// Learn cc.Class:
//  - https://docs.cocos.com/creator/manual/en/scripting/class.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/manual/en/scripting/life-cycle-callbacks.html

cc.Class({
    extends: cc.Component,

    properties: {
        mask : cc.Mask,
        label : cc.Label,
    },

    statics: {
        show(msg) {
            console.log(msg);
            cc.resources.load(
                "Prefabs/TopToast",
                this.__onLoadPrefab.bind(this, msg)
            );
        },

        __onLoadPrefab(msg, err, prefab) {
            if (err) {
                console.log(err);
                return;
            }

            var toast = cc.instantiate(prefab);
            var canvas = this.findCanvas();
            if (!toast || !canvas) {
                console.log('toast: ' + toast + ' / canvas: ' + canvas);
                return;
            }
            var script = toast.getComponent('ToastScript');

            script.setString(msg);
            script.attach(canvas);
            script.animate();
        },

        findCanvas() {
            var scn = cc.director.getScene();
            if (null === scn) {
                console.log('null === scn');
                return scn;
            }
    
            var canvas = scn.getChildByName('Canvas');
            if (null === canvas) {
                console.log('find canvas failed #1');
                canvas = cc.find('Canvas');
            }
            
            if (null !== canvas) {
                return canvas;
            } else {
                console.log('find canvas failed #2');
                console.log(scn.children);
                return scn;
            }
        }
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {
    },

    // update (dt) {},

    setString(msg) {
        this.label.string = msg;
    },

    attach(parent) {
        // 사전 작업
        var scn = cc.director.getScene();
        var node = this.node;
        var w = cc.winSize.width;
        var h = cc.winSize.height;
        
        // position
        var worldPos = cc.v2(w / 2, h);
        var nodePos = parent.convertToNodeSpaceAR(worldPos);
        node.setPosition(nodePos);

        // width
        node.width =  w;
        
        // attach
        parent.addChild(node);
    },

    animate() {
        var self = this;
        this.scheduleOnce(this.__animate.bind(self), 0);
    },

    __animate() {
        // node animation
        var toast = this.node;
        var h = toast.height;
        var self = this;
        
        // is label too long?
        var viewW = this.mask.node.width;
        var labelW = this.label.node.width;
        if (labelW > viewW) {
            // 중앙정렬 포기. 스크롤 시작 위치로 설정
            this.scrollW = labelW - viewW;
            this.label.node.x = this.scrollW / 2;
        } else {
            this.scrollW = 0;
        }

        // do act
        cc.tween(toast)
            .by(0.6, { position: cc.v2(0, -h) }, { easing: 'bounceOut' })
            .delay(0.2)
            .call(this.scrollLabel.bind(self, 2))
            .delay(2.2)
            .by(0.4, { position: cc.v2(0, h) }, { easing: 'expoIn' })
            .removeSelf()
        .start();
    },

    scrollLabel(len) {
        if (!this.scrollW) {
            return;
        }

        cc.tween(this.label.node)
            .by(len, {position: cc.v2(-this.scrollW, 0) })
        .start();
    },
});
