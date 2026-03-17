var MessagePopup = require("MessagePopup");
var NetworkManager = require("NetworkManager");

cc.Class({
    extends: cc.Component,

    properties: {
        label : cc.Label,
        particle : cc.Node,
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad: function () {
        var touchReceiver = cc.Canvas.instance.node;
        touchReceiver.on('touchstart', this.onTouchStart, this);
        touchReceiver.on('touchend', this.onTouchEnd, this);
        touchReceiver.on('touchmove', this.onTouchEnd, this);
        touchReceiver.on('touchcancel', this.onTouchEnd, this);
    },

    start () {
        this.gameStart = false;
        this.gameTime = 0;
    },  

    onTouchStart (event) {
        console.log('touch start');
        this.gameStart = true;    
        this.gameTime = 0;    
        this.particle.active = true;

        this.particle.x = event.touch.getLocation().x - (this.node.width/2);
        this.particle.y = event.touch.getLocation().y - (this.node.height/2);
        
        console.log(event.touch.getLocation().x, event.touch.getLocation().y);
    },

    onTouchEnd (event) {
        if(this.gameStart)
        {
            console.log('touch end');
            this.onGameEnd();
            this.particle.active = false;
        }
    },

    onGameEnd: function()
    {
        this.gameStart = false; 
       
        var data = new FormData();
        data.append('game', 'idle');
        require('AccountManager').AppendFormData(data);
        data.append('score', this.gameTime);
        data.append('asc', 0);
          
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
    },

    update : function(dt)
    {
        if(this.gameStart)
        {
            if(dt > 1)
            {
                console.log('too much idle');
                this.onGameEnd();
            }
            else
            {
                this.gameTime += dt;
                this.label.string = "Time : " + this.gameTime.toFixed(2);
            }
        }
    }
});
