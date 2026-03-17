
var NetworkManager = require("NetworkManager");
var MessagePopup = require("MessagePopup");
var DartsEffect = require("DartsEffect");

var eDartsState;
eDartsState = cc.Enum({
  NONE : 0,
  PICK : 1,
  THROW : 2,  
  END : 3,
});

cc.Class({
    extends: cc.Component,

    properties: {
        line : cc.Graphics,
        darts : [cc.Node],
        dartsParticleEffect : [cc.Node],
        remain : cc.Label,
        point : cc.Label,
        retry : cc.Node,
        cam : cc.Camera,
        rankingWebview : cc.Node,
        aim : cc.Node,
        hiden : cc.Label,
        board : cc.Node,
        miss_effect : DartsEffect,
        retry_effect : DartsEffect,
        high_effect : DartsEffect,
        round_high_effect : DartsEffect,
        round_low_effect : DartsEffect,        
        noticeButton: cc.Node,
        noticePopup : cc.Node,
        background : cc.Node,

        character : cc.Node,
        character_chat : cc.Node,
        character_chat_label : cc.Node,
    },

    start () {
        // this.background.runAction(
        //     cc.repeatForever(cc.sequence(
        //         cc.tintTo(1, 255, 0, 0),
        //         cc.tintTo(1, 255, 255, 0),
        //         cc.tintTo(1, 0, 255, 0),
        //         cc.tintTo(1, 0, 255, 255),
        //         cc.tintTo(1, 0, 0, 255),
        //         cc.tintTo(1, 255, 0, 255),
        //         cc.tintTo(1, 100, 0, 0),
        //         cc.tintTo(1, 50, 100, 0),
        //         cc.tintTo(1, 0, 100, 0),
        //         cc.tintTo(1, 0, 50, 100),
        //         cc.tintTo(1, 0, 0, 100),
        //         cc.tintTo(1, 100, 0, 100),
        //     ))
        // );

        var inWidth = 3;
        var outWidth = 20;
        this.endDist = 125;
        for(var i = 0; i < 21; i++)
        {
            this.line.strokeColor = cc.Color(9,0,36);      
            this.line.lineWidth = inWidth;
            this.line.lineTo(0,0);
            this.line.stroke();
            this.line.strokeColor = cc.Color(9,0,36);      
            this.line.lineWidth = outWidth;
            this.line.arc(0, 0, this.endDist + outWidth/2, ((0.1 * (i + 1)) - 0.05) * Math.PI, ((0.1 * i) - 0.05) * Math.PI)
            this.line.stroke();
        }

        var touchReceiver = cc.Canvas.instance.node;
        touchReceiver.on('touchstart', this.onTouchStart, this);        
        touchReceiver.on('touchmove', this.onTouchMove, this);
        touchReceiver.on('touchend', this.onTouchEnd, this);
        touchReceiver.on('touchcancel', this.onTouchEnd, this);

        this.darts.forEach(r => {
            r.active = false;
        });

        this.ori_dart_rot = 270;
        this.ori_dart_x = 0;
        this.ori_dart_y = -120;

        this.x_acc = this.ori_dart_x;
        this.y_acc = this.ori_dart_y;
        this.bull_x = 0;
        this.bull_y = 140;

        this.x_acc_max = 30;
        this.y_acc_max = 30;
        this.shake_level = 3;
        
        this.game_seq = 0;
        this.retry_count = 0;
        
        this.onGameSet();        

        this.aim.runAction(cc.repeatForever(cc.spawn(
            cc.sequence(
                cc.scaleTo(0.5, 0.8),
                cc.scaleTo(0.3, 1.2)
            ),
            cc.sequence(
                cc.fadeTo(0.2, 100),
                cc.fadeIn(0.6, 180)
            ),
            cc.sequence(
                cc.tintTo(0.4, 211, 118, 0),                
                cc.tintTo(0.4, 199, 48, 0)
            ),
        )));
        
        if(NetworkManager.product_type != 2)
            this.noticeButton.active = true;

        this.kensei();        
    },

    kensei()
    {
        if(this.character !== null)
        {
            var callback = cc.callFunc(this.kensei, this);
            
            if(Math.random() * 3 > 2)
            {
                var scale = Math.max(1, Math.random() * 2);
                var y = (scale - 1) * -400;
                
                this.character.runAction(cc.sequence(
                    cc.spawn(cc.scaleTo(0.3, scale), cc.moveTo(0.3, cc.v2(this.character.x, y))),
                    callback             
                ));
            }
            else
            {
                var inSpeed = 0.5 + Math.random() * 1;            
                var outSpeed = 0.5 + Math.random() * 2;
                var inMovPos = -227 + Math.random() * (227 + 153);
                var outMovPos = -227 + Math.random() * (227 + 153);

                this.character.runAction(cc.sequence(
                    cc.moveTo(inSpeed, cc.v2(inMovPos, this.character.y)),                
                    cc.moveTo(outSpeed, cc.v2(-outMovPos, this.character.y)),
                    callback             
                ));
            }
        }

        if(this.character_chat !== null)
        {
            this.character_chat.runAction(cc.repeatForever(cc.sequence(                
                cc.scaleTo(0.2, 0),
                cc.delayTime(2),
                cc.scaleTo(0.2, 1),
                cc.delayTime(1.2),                
            )));

            this.character_chat = null;
        }
    },

    onDestroy () {
        if(cc.Canvas.instance)
        {
            var touchReceiver = cc.Canvas.instance.node;
            touchReceiver.off('touchstart', this.onTouchStart, this);
            touchReceiver.off('touchmove', this.onTouchMove, this);
            touchReceiver.off('touchend', this.onTouchEnd, this);
            touchReceiver.off('touchcancel', this.onTouchEnd, this);
        }
    },

    onGameSet : function()
    {   
        if(this.retry_count >= 5)
        {
            MessagePopup.openMessageBox("이런데 광고넣으면 참 잘팔리겠죠?");  
            this.retry_count = Math.random() * 2;
        }
        else if(this.retry_count > 1)
        {
            this.retry_effect.onEffect();
            this.retry_effect.node.opacity = 0;
            this.retry_effect.node.stopAllActions();
        
            this.retry_effect.node.runAction(cc.sequence(
                cc.fadeIn(0.5),
                cc.fadeOut(0.5),         
            ));

            this.retry_count += 1;
        }
        else
        {
            this.retry_count += 1;
        }
        
        if(this.score >= 100)
        {
            this.game_seq += 1;
            this.hiden.string = '누적 라운드 : ' + this.game_seq;
        }
        else
        {
            this.game_seq = 0;
            this.hiden.string = '';            
        }

        this.board.x = this.bull_x;
        this.board.y = this.bull_y;
        this.board.angle = 0;

        this.darts.forEach(dart => {
            dart.x = this.ori_dart_x;
            dart.y = this.ori_dart_y;
            dart.angle = 270;
            dart.scale = 1;
            dart.active = false;
        });
        
        this.curDart = 0;
        this.dartState = eDartsState.NONE;
        this.score = 0;
        this.remain.string = "남은 다트 : 3";
        this.point.string = "점수 : 0";

        this.retry.active = false;

        this.noticeButton.active = false;
    },

    onTouchStart (event) {
        this.noticeButton.active = false;

        cc.director.getScheduler().setTimeScale(1);

        if(this.dartState == eDartsState.NONE)
        {
            this.dartsParticleEffect[this.curDart].active = false;     
            this.darts[this.curDart].active = true;
            this.dartState = eDartsState.PICK;
            
            var rx = this.x_acc_max * 0.5;
            var ry = this.y_acc_max * 0.5;
            this.x_acc = this.ori_dart_x + (Math.random() * rx * 2) - rx;
            this.y_acc = this.ori_dart_y + (Math.random() * ry * 2) - ry;

            this.darts[this.curDart].x = this.x_acc;
            this.darts[this.curDart].y = this.y_acc;

            this.remain.string = "남은 다트 : " + (3 - (this.curDart + 1));
            this.point.string = "점수 : " + this.score;
        }
    },

    onTouchMove (event) {
        if(this.dartState == eDartsState.PICK)
        {
            this.x_acc += event.touch.getDelta().x;
            this.y_acc += event.touch.getDelta().y;
        }
    },

    onTouchEnd (event) {        
        if(this.dartState == eDartsState.PICK)
        {          
            this.dartState = eDartsState.THROW;
            var v = cc.v2(this.x_acc - this.ori_dart_x, this.y_acc - this.ori_dart_y); 
            var goal = cc.v2(this.board.x + (v.x * 5), this.board.y + (v.y * 5));
            
            if(this.getDartScore(goal) > 45)
            {
                cc.director.getScheduler().setTimeScale(0.2);        
                this.dartsParticleEffect[this.curDart].active = true;        
            }

            if(this.character !== null)
            {
                this.darts[this.curDart].setSiblingIndex(this.character.getSiblingIndex() - 1);
            }

            var cur_sb = this.darts[this.curDart].getSiblingIndex();
            for(let i = 0; i < this.curDart; i++)
            {
                var sb = this.darts[i].getSiblingIndex();
                if(cur_sb > sb)
                {
                    if(goal.y < this.darts[i].y)
                    {           
                        //console.log("sible change" + this.curDart + "," + i)         
                        this.darts[i].setSiblingIndex(cur_sb);
                        this.darts[this.curDart].setSiblingIndex(sb);
                        cur_sb = sb;
                    }
                }
                else
                {
                    if(goal.y > this.darts[i].y)
                    {                    
                        //console.log("sible change" + this.curDart + "," + i)         
                        this.darts[i].setSiblingIndex(cur_sb);
                        this.darts[this.curDart].setSiblingIndex(sb);
                        cur_sb = sb;
                    }
                }
            }

            var action = cc.sequence(cc.spawn(
                cc.moveTo(0.5, goal),
                cc.scaleBy(0.2, 1.1),
                cc.scaleBy(0.3, 0.5)                
            ),cc.callFunc(this.onThrowEnd, this));
    
            this.darts[this.curDart++].runAction(action);
        }
    },

    getDartScore : function(pos)
    {
        var v = cc.v2(pos.x, pos.y).sub(cc.v2(this.board.x, this.board.y)).normalize();
        
        var rot = this.board.angle * -1;
        var rad = rot * (Math.PI/180);
        var cos = Math.cos(rad);
        var sin = Math.sin(rad);
        
        var board_normalizevec = cc.v2(sin, cos);    
        var angle = board_normalizevec.angle(v) * (180/Math.PI);

        rot += 90;
        rad = rot * (Math.PI/180);
        cos = Math.cos(rad);
        sin = Math.sin(rad);
        var tmp = cc.v2(sin, cos); 
        var langle = tmp.angle(v) * (180/Math.PI);

        console.log(langle);

        var point_target = 0;
        var left_side = langle > 90;

        if(angle < 9)
        {
            point_target = 20;
        }
        else if(angle >= 9 && angle < 27)
        {
            if(left_side)
                point_target = 5;
            else
                point_target = 1;
        }
        else if(angle >= 27 && angle < 45)
        {
            if(left_side)
                point_target = 12;
            else
                point_target = 18;
        }
        else if(angle >= 45 && angle < 63)
        {
            if(left_side)
                point_target = 9;
            else
                point_target = 4;
        }
        else if(angle >= 63 && angle < 81)
        {
            if(left_side)
                point_target = 14;
            else
                point_target = 13;
        }
        else if(angle >= 81 && angle < 99)
        {
            if(left_side)
                point_target = 11;
            else
                point_target = 6;
        }
        else if(angle >= 99 && angle < 117)
        {
            if(left_side)
                point_target = 8;
            else
                point_target = 10;
        }
        else if(angle >= 117 && angle < 135)
        {
            if(left_side)
                point_target = 16;
            else
                point_target = 15;
        }
        else if(angle >= 135 && angle < 153)
        {
            if(left_side)
                point_target = 7;
            else
                point_target = 2;
        }
        else if(angle >= 153 && angle < 171)
        {
            if(left_side)
                point_target = 19;
            else
                point_target = 17;
        }
        else
        {
            point_target = 3;
        }
        
        var point = 0;
        var distance = pos.sub(cc.v2(this.board.x, this.board.y)).mag();        
        if(distance < 125)
        {
            if(distance < 110)
            {
                if(distance < 80)
                {
                    if(distance < 65)
                    {                        
                        if(distance < 15)
                        {
                            point = 50;
                        }
                        else
                        {
                            point = point_target;                            
                        }
                    }
                    else
                    {
                        point = point_target * 3;
                    }
                }
                else
                {
                    point = point_target;
                }
            }
            else
            {
                point = point_target * 2;
            }
        }

        return point;
    },

    onThrowEnd : function()
    {
        cc.director.getScheduler().setTimeScale(1);
        this.dartsParticleEffect[this.curDart - 1].active = false;        

        var point = this.getDartScore(this.darts[this.curDart - 1].position);
        this.score += point;
       
        if(point == 0)
        {
            this.miss_effect.onEffect();
            this.miss_effect.node.opacity = 0;
            this.miss_effect.node.stopAllActions();
        
            this.miss_effect.node.runAction(cc.sequence(
                cc.fadeIn(0.5),
                cc.fadeOut(0.5),                   
            ));

            
            var goal_rot = Math.random() * 270 * ((-this.darts[this.curDart - 1].angle) > 0 ? 1 : -1);
            var goal_pos = cc.Vec2(this.darts[this.curDart - 1].x, -100);

            this.darts[this.curDart - 1].runAction(cc.spawn(
                cc.rotateBy(0.9, goal_rot),
                cc.moveTo(1, goal_pos),
            ));
        }
        else if(point >= 45)
        {
            this.high_effect.onEffect();
            this.high_effect.node.opacity = 0;
            this.high_effect.node.stopAllActions();
        
            this.high_effect.node.runAction(cc.sequence(
                cc.fadeIn(0.5),
                cc.fadeOut(0.5),  
            ));

            this.onCamShake();
        }
        else
        {
            this.onCamShake();
        }

        this.remain.string = "남은 다트 : " + (3 - this.curDart);
        this.point.string = "점수 : " + this.score;

        var action = cc.rotateBy(0.2, -30 + (Math.random() * 60));
        this.darts[this.curDart - 1].runAction(action);

        if(this.curDart >= 3)
            this.onRoundEnd();
        else
            this.dartState = eDartsState.NONE;
    },

    onCamShake : function()
    {
        var action = cc.sequence(
            cc.moveTo(0.1, cc.v2(0, 3)),
            cc.moveTo(0.1, cc.v2(3, 0)),
            cc.moveTo(0.1, cc.v2(0, -3)),
            cc.moveTo(0.1, cc.v2(-3, 0)),
            cc.moveTo(0.1, cc.v2(0, 0)),                
        );

        this.cam.node.runAction(action);
    },

    onRoundEnd : function()
    {
        this.dartState = eDartsState.END;

        this.node.runAction(cc.sequence(
            cc.delayTime(0.5),            
            cc.callFunc(function(){
                var data = new FormData();
                
                if(NetworkManager.product_type == 2)
                {
                    data.append('game', 'endlovedarts');    
                }
                else
                {
                    data.append('game', 'darts');
                }
                
                require('AccountManager').AppendFormData(data);
                data.append('score', this.score);
                data.append('asc', 0);
                
                var res_callback = this.onRankingResult.bind(this);
                NetworkManager.SendRequestPost("minigame/register", data, res_callback);
            }, this)
        ));
    },

    onRankingResult: function(response)
    {
        console.log(response);
        var data = JSON.parse(response);        
        var rs = parseInt(data["rs"]);  
        if(rs == 0)
        {
            var delay = this.high_effect.node.opacity > 0 || this.miss_effect.node.opacity > 0;

            var rank = parseInt(data["rank"]);  
            if(this.score >= 100)
            {
                this.round_high_effect.onEffect();
                this.round_high_effect.node.opacity = 0;
                this.round_high_effect.node.stopAllActions();
            
                if(delay)
                {
                    this.round_high_effect.node.runAction(cc.sequence(
                        cc.delayTime(0.5),
                        cc.fadeIn(0.5),
                        cc.delayTime(0.5),
                        cc.fadeOut(0.5),    
                        cc.callFunc(this.onRankingBoard, this)
                    ));
                }
                else
                {
                    this.round_high_effect.node.runAction(cc.sequence(
                        cc.fadeIn(0.5),
                        cc.delayTime(0.5),
                        cc.fadeOut(0.5),    
                        cc.callFunc(this.onRankingBoard, this)
                    ));
                }                
            }
            else
            {
                this.round_low_effect.onEffect();
                this.round_low_effect.node.opacity = 0;
                this.round_low_effect.node.stopAllActions();
            

                if(delay)
                {
                    this.round_low_effect.node.runAction(cc.sequence(
                        cc.delayTime(0.5),
                        cc.fadeIn(0.5),
                        cc.delayTime(0.5),
                        cc.fadeOut(0.5),            
                        cc.callFunc(this.onRankingBoard, this)
                    ));
                }
                else
                {
                    this.round_low_effect.node.runAction(cc.sequence(
                        cc.fadeIn(0.5),
                        cc.delayTime(0.5),
                        cc.fadeOut(0.5),            
                        cc.callFunc(this.onRankingBoard, this)
                    ));
                }                
            }
        }
        else
        {
            MessagePopup.openMessageBox("오류 발생\n결과 미반영");  
        }
    },

    onRankingBoard : function()
    {        
        this.rankingWebview.active = !this.rankingWebview.active;
        this.retry.active = true;
    },

    update: function (dt) {        
        if(this.dartState == eDartsState.PICK)
        {
            this.x_acc += (Math.random() * this.shake_level * 2) - this.shake_level;
            this.y_acc += (Math.random() * this.shake_level * 2) - this.shake_level;

            this.x_acc = Math.max(this.x_acc, this.ori_dart_x + (this.x_acc_max * -1));
            this.y_acc = Math.max(this.y_acc, this.ori_dart_y + (this.y_acc_max * -1));
            this.x_acc = Math.min(this.x_acc, this.ori_dart_x +this.x_acc_max);
            this.y_acc = Math.min(this.y_acc, this.ori_dart_y +this.y_acc_max);

            this.darts[this.curDart].x = this.x_acc;
            this.darts[this.curDart].y = this.y_acc;

            if(this.game_seq % 2 == 1)
            {                
                var bx = this.board.x + (Math.random() * this.game_seq * 2) - this.game_seq;
                var by = this.board.y + (Math.random() * this.game_seq * 2) - this.game_seq;
                
                bx = Math.max(bx, this.bull_x + (this.x_acc_max * -1));
                by = Math.max(by, this.bull_y + (this.y_acc_max * -1));
                bx = Math.min(bx, this.bull_x + this.x_acc_max);
                by = Math.min(by, this.bull_y + this.y_acc_max);

                this.board.x = bx;
                this.board.y = by;
            }
            if(this.game_seq % 3 == 2)
            {                
                this.board.angle -= 1;
            }
        }
    },

    onNoticeButton : function()
    {
        this.noticePopup.active = !this.noticePopup.active;
    },
});
