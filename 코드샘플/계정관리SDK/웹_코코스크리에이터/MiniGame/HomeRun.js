cc.Class({
    extends: cc.Component,

    properties: {
        baseball_bat : cc.Node,
        baseball_bat_aim : cc.Node,
    },

    onLoad () {
        cc.systemEvent.on(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);        
        
        var touchReceiver = cc.Canvas.instance.node;
        touchReceiver.on('touchstart', this.onTouchStart, this);        
        touchReceiver.on('touchmove', this.onTouchMove, this);
        touchReceiver.on('touchend', this.onTouchEnd, this);
        touchReceiver.on('touchcancel', this.onTouchEnd, this);

        this.max_y = 75;
        this.min_y = -75;
    },

    onDestroy () {
        cc.systemEvent.off(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);

        var touchReceiver = cc.Canvas.instance.node;
        touchReceiver.off('touchstart', this.onTouchStart, this);
        touchReceiver.off('touchmove', this.onTouchMove, this);
        touchReceiver.off('touchend', this.onTouchEnd, this);
        touchReceiver.off('touchcancel', this.onTouchEnd, this);
    },

    onKeyDown (event)
    {
        console.log(event.keyCode);
        switch(event.keyCode) 
        {
            case cc.macro.KEY.space:
            {
                this.onHit();
            }
        }
    },

    onTouchStart (event) {
        
    },

    onTouchMove (event) {
        //this.baseball_bat_aim.x += event.touch.getDelta().x;
        this.baseball_bat_aim.y += event.touch.getDelta().y;

        this.baseball_bat_aim.y = Math.max(this.min_y, this.baseball_bat_aim.y);        
        this.baseball_bat_aim.y = Math.min(this.max_y, this.baseball_bat_aim.y);

        this.baseball_bat.y = ((this.baseball_bat_aim.y / 75) * 1.25) - 0.75;
    },

    onTouchEnd (event) {
        this.onHit();
    },

    onHit : function()
    {
        this.baseball_bat.runAction(cc.sequence(
            cc.rotate3DTo(0.1, cc.v3(35, 0, 0)),
            cc.rotate3DTo(0.1, cc.v3(100, 100, 0)),
            cc.rotate3DTo(0.1, cc.v3(50, 100, 0)),
            //cc.rotate3DTo(0.2, cc.v3(-35, 0, 0)),
            cc.rotate3DTo(0.1, cc.v3(0, 0, 0)),
        ));
    }
});
