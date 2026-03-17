//const LOGGER_MAX_MESSAGES = 10;

var Samanda = require('Samanda');

cc.Class({
    extends: cc.Component,

    properties: {
        panel : {
            default: null,
            type: cc.Node
        },

        msgs: null,

        labels: null
    },

    statics: {
        is_init: false,

        self: null,

        proxy: function(ctx, method, message) {
            var self = this.self;
            return function() {
                method.apply(ctx, Array.prototype.slice.apply(arguments));
                self.onLog([message], Array.prototype.slice.apply(arguments));
            }
        },

        init: function() {
            if (this.is_init) {
                return;
            }

            this.is_init = true;

            this.self = new this.prototype.constructor();
            var self = this.self;
            self.msgs = new Array();
            
            console.log = this.proxy(console, console.log, '[L]');
            console.info = this.proxy(console, console.info, '[I]');
            console.warn = this.proxy(console, console.warn, '[W]');
            console.error = this.proxy(console, console.error, '[E]');
        }
    },

    // LIFE-CYCLE CALLBACKS:

    // onLoad () {},

    start () {

    },

    // update (dt) {},

    applyMsg() {
        var canvas = Samanda.findCanvas();
        var bg = canvas.getChildByName("SCLOGGER_BG");
        if (!bg) {
            this.labels = new Array();

            bg = new cc.Node();
            bg.width = canvas.width;
            bg.height = 300;

            bg.x = - bg.width / 2;
            bg.y = canvas.height / 2 - bg.height;

            let grp = bg.addComponent(cc.Graphics);
            grp.lineWidth = 0;
            grp.fillColor = new cc.Color(0, 0, 0, 160);
            grp.fillRect(0, 0, bg.width, bg.height);
            canvas.addChild(bg, 3, "SCLOGGER_BG");
        }

        for (let i = 0; this.msgs.length > i; ++i) {
            let label = null;
            if (this.labels.length <= i) {
                let labelNode = new cc.Node();                
                labelNode.x = bg.width / 2;
                labelNode.y = bg.height - 30 - 30 * i;
                bg.addChild(labelNode);

                label = labelNode.addComponent(cc.Label);
                label.fontSize = 22;
                
                this.labels.push(label);
            } else {
                label = this.labels[i];
            }
            if (!label) {
                return;
            }
            label.string = this.msgs[i];
        }
    },

    onLog(header, msg) {
        while (10 <= this.msgs.length) {
            this.msgs.shift();
        }

        this.msgs.push(header + " " + msg);
        this.applyMsg();
    }
});
