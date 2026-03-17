var Note = require('Note');
var Samanda = require("Samanda");

cc.Class({
    extends: cc.Component,

    properties: {
        keyboard : [cc.Node],
        note : [Note],
        noteSharp : [Note],
    },

    onLoad () {
        if(!Samanda.isAndroid())
        {
            cc.systemEvent.on(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
            cc.systemEvent.on(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);

            this.note.forEach(n => {
                n.node.getChildByName('hint').active = true;
            });

            this.noteSharp.forEach(n => {
                n.node.getChildByName('hint').active = true;
            });
        }
    },

    onDestroy () {        
        if(!Samanda.isAndroid())
        {
            cc.systemEvent.off(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
            cc.systemEvent.off(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);
        }
    },
    
    onKeyDown (event) {
        switch(event.keyCode) {
            case cc.macro.KEY.z:
                this.note[0].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.x:
                this.note[1].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.c:
                this.note[2].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.v:
                this.note[3].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.b:
                this.note[4].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.n:
                this.note[5].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.m:
                this.note[6].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;

            case cc.macro.KEY.s:
                this.noteSharp[0].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.d:
                this.noteSharp[1].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.g:
                this.noteSharp[2].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.h:
                this.noteSharp[3].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
            case cc.macro.KEY.j:
                this.noteSharp[4].getComponent(cc.Button)._onTouchBegan(new cc.Event());            
                break;
        }
    },

    onKeyUp (event) {
        switch(event.keyCode) {
            case cc.macro.KEY.z:
                this.note[0].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.x:
                this.note[1].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.c:
                this.note[2].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.v:
                this.note[3].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.b:
                this.note[4].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.n:
                this.note[3].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.m:
                this.note[3].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;

            case cc.macro.KEY.s:
                this.noteSharp[0].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.d:
                this.noteSharp[1].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.g:
                this.noteSharp[2].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.h:
                this.noteSharp[3].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
            case cc.macro.KEY.j:
                this.noteSharp[4].getComponent(cc.Button)._onTouchEnded(new cc.Event());            
                break;
        }
    },

    start () {    
        // this.onNoteDown(2);
        // return;

        this.schedule(function() {
            //var t = 1.0 + (Math.random() * 1.0);
            var t = 2;
            this.onNoteDown(t);
        }, 2);

        this.schedule(function() {
            //var t = 1.5 + (Math.random() * 1.5);
            var t = 2;
            this.onSharpNoteDown(t);
        }, 2);
    },

    onNoteDown(time)
    {
        var length = 10 + (Math.random() * 100);

        this.note[(Math.random() * this.note.length) | 0].onNote(time, length);
    },

    onSharpNoteDown(time)
    {
        var length = 10 + (Math.random() * 100);

        this.noteSharp[(Math.random() * this.noteSharp.length) | 0].onNote(time, length);
    },
    
    hit : function(event, param)
    {
        event.target.getComponent(Note).onHit();
    },
});
