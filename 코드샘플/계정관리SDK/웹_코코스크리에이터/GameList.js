var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");

cc.Class({
    extends: cc.Component,

    properties: {
        jump : cc.Label,
        darts : cc.Label,
        idle : cc.Label,
        screw : cc.Label,
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad () {
        var data = new FormData();
        data.append('jump', '0');
        data.append('darts', '0');
        data.append('idle', '0');
        data.append('screw', '1');  

        var res_callback = this.onRankerData.bind(this);
        NetworkManager.SendRequestPost("minigame/leaders", data, res_callback, null);
    },

    start () {

    },

    onJump : function()
    {
        cc.director.loadScene("game.fire");
    },

    onDarts : function()
    {
        cc.director.loadScene("DartsGame.fire");
    },

    onIdle : function()
    {
        cc.director.loadScene("LongTouchGame.fire");
    },

    onScrew : function()
    {
        cc.director.loadScene("MiniGame.fire");
    },

    onRankerData : function(response)
    {
        var data = JSON.parse(response);        
        var rs = parseInt(data["rs"]);  
        if(rs == 0)
        {
            var list = data['list'];
            this.jump.string = "1등 : " + list[0][1] + " - " + list[0][2];
            this.darts.string = "1등 : " + list[1][1] + " - " + list[1][2];
            this.idle.string = "1등 : " + list[2][1] + " - " + list[2][2];
            this.screw.string = "1등 : " + list[3][1] + " - " + list[3][2];            
        }
        else
        {
            MessagePopup.openMessageBox("오류 발생");  
        }
    },
});
