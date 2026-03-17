var MessagePopup = require("MessagePopup");
var NetworkManager = require("NetworkManager");

var einputState;
einputState = cc.Enum({
  NONE : 0,
  UP : 1,
  DOWN : 2,  
  LEFT : 3,
  RIGHT : 4
});

cc.Class({
    extends: cc.Component,

    properties: {
        screw : cc.Node,
        score : cc.Label,
        rank : [cc.Label]
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad: function () {
        this.startTime = 0;
        this.preInput = einputState.NONE;
        this.gameStart = false;
        this.originalScale = this.screw.getScale(cc.v2()).x;
        
        this.action = cc.spawn(
            cc.rotateBy(1, -30),
            cc.scaleBy(1, 1.02)
        );

        //cc.systemEvent.on(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
        cc.systemEvent.on(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);

        this.reqRankingBoard();
    },

    onDestroy () {        
        //cc.systemEvent.off(cc.SystemEvent.EventType.KEY_DOWN, this.onKeyDown, this);
        cc.systemEvent.off(cc.SystemEvent.EventType.KEY_UP, this.onKeyUp, this);        
    },

    screwStart: function() {
        console.log('Game Start');
        this.startTime = 0;
        this.preInput = einputState.NONE;
        this.screw.setScale(this.originalScale);
        this.gameStart = true;     
        this.screw.stopAllActions();
        
        var actionRotate = cc.rotate3DBy(0.5, cc.v3(0, 0, 0));
        this.screw.runAction(actionRotate);
    },

    onScrew: function()
    {
        this.screw.runAction(this.action);
    },

    onKeyUp (event) {
        if(this.gameStart)
        {
            switch(event.keyCode) 
            {
                case cc.macro.KEY.down:
                    if(this.preInput == einputState.NONE || this.preInput == einputState.LEFT)
                    {
                        this.onScrew();                        
                    }
                    this.preInput = einputState.DOWN;
                    break;
                case cc.macro.KEY.right:
                    if(this.preInput == einputState.NONE || this.preInput == einputState.DOWN)
                    {
                        this.onScrew();
                    }
                    this.preInput = einputState.RIGHT;
                    break;
                case cc.macro.KEY.left:
                    if(this.preInput == einputState.NONE || this.preInput == einputState.UP)
                    {
                        this.onScrew();                        
                    }
                    this.preInput = einputState.LEFT;
                    break;
                case cc.macro.KEY.up:
                    if(this.preInput == einputState.NONE || this.preInput == einputState.RIGHT)
                    {
                        this.onScrew();                        
                    }
                    this.preInput = einputState.UP;
                    break;
            }
        }
        else
        {
            switch(event.keyCode) 
            {
                case cc.macro.KEY.space:
                    this.screwStart();
                    break;
                case cc.macro.KEY.r:
                    this.reqRankingBoard();
                    break;
            }
        }
    },
    
    update : function(dt)
    {
        if(this.gameStart)
        {
            this.startTime += dt;
            this.score.string = "Time : " + this.startTime.toFixed(3);   
            if(this.screw.getScale(cc.v2()).x > this.originalScale * 2)
            {
                this.onGameFinish();
            }
        }
        else
        {            
            this.score.string = "Result : " + this.startTime.toFixed(3);              
        }
    },

    onGameFinish: function()
    {
        this.gameStart = false; 
        var actionRotate = cc.repeatForever(cc.rotate3DBy(2, cc.v3(0, 180, 0)));
        this.screw.runAction(actionRotate);

        var data = new FormData();
        data.append('game', 'screw');
        require('AccountManager').AppendFormData(data);
        data.append('score', this.startTime);
        data.append('asc', 1);
          
        var res_callback = this.onRankingResult.bind(this);
        NetworkManager.SendRequestPost("minigame/register", data, res_callback);
    },

    onRankingResult: function(response)
    {
        var data = JSON.parse(response);        
        var rs = parseInt(data["rs"]);  
        if(rs == 0)
        {
            var rank = parseInt(data["rank"]);  
            MessagePopup.openMessageBox("현재 등수 결과는 " + rank + "등입니다.");  
        }
        else
        {
            MessagePopup.openMessageBox("오류 발생\n결과 미반영");  
        }

        this.reqRankingBoard();
    },

    reqRankingBoard: function()
    {
        var data = new FormData();
        data.append('game', 'screw');
        data.append('asc', 1);
        
        var res_callback = this.onRankingBoard.bind(this);
        NetworkManager.SendRequestPost("minigame/get_list", data, res_callback);        
    },

    onRankingBoard: function(response)
    {
        var data = JSON.parse(response);  
        var rs = parseInt(data["rs"]);  
        if(rs == 0)
        {
            var ranking = data["ranking"];
            ranking.forEach(r =>
                {
                    var ra = parseInt(r["rank"]) - 1;
                    if(ra < 10)
                        this.rank[ra].string = (ra + 1) + "등 : " + r["nick"] + "-" + r["score"].toFixed(3)
                }
            );
        }
    }
});
