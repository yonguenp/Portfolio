// Learn cc.Class:
//  - https://docs.cocos.com/creator/manual/en/scripting/class.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/manual/en/scripting/life-cycle-callbacks.html

cc.Class({
    extends: cc.Component,

    properties: {
        
    },

    onLoad () {
        this.Button = this.node.getComponent(cc.Button);
        this.ButtonNormalColor = this.Button.normalColor;
        this.ButtonPressColor = this.Button.pressdColor;
        this.ButtonHoverColor = this.Button.hoverColor;

        this.CopyTarget = this.node.getChildByName('Background');        
        this.CopyTargetOriginColor = this.CopyTarget.color;
        

        this.HitAreaSize = 10;
        this.NodeOpacity = 200;

        this.maxY = (this.CopyTarget.height / 2);
        this.minY = (this.maxY * -1) + (this.HitAreaSize / 2);
        
        this.HitArea = cc.instantiate(this.CopyTarget);
        this.HitArea.parent = this.node;       
        this.HitArea.removeComponent(cc.Widget);
        this.HitArea.color = cc.Color.YELLOW;
        this.HitArea.height = this.HitAreaSize;
        this.HitArea.y = this.minY;

        this.ReturnTime = 0.0;

        this.NoteNode = new Array();
        this.NoteMap = new Map();
    },

    onNote(speed, length)
    {
        var newNote = null;
        var data = null;
        
        this.NoteNode.forEach(oldNote => {
            if(oldNote.active == false)
                newNote = oldNote;
        });
        
        if(newNote == null)
        {
            newNote = cc.instantiate(this.CopyTarget);
            newNote.parent = this.node;       
            newNote.color = cc.Color.MAGENTA;
            newNote.removeComponent(cc.Widget);            
            newNote.y = this.maxY;
            newNote.active = false;
            
            this.NoteNode.push(newNote);
            data = new Map();
            this.NoteMap.set(newNote, data);
        }
        else
        {
            data = this.NoteMap.get(newNote);
        }

        if(newNote == null)
            return;

        data.height = length;
        data.time = speed;
        data.cur = speed;
        data.hit = false;

        data.maxY = this.maxY + (data.height/2);
        data.minY = (data.maxY * -1) + this.HitAreaSize;

        data.Normal = data.height;
        data.Good = data.height * 0.5;
        data.Excellent = data.height * 0.3;
        data.Perpect = data.height * 0.1;
        
        data.limittime = -0.1;

        newNote.active = true;
        newNote.y = data.maxY;
        newNote.opacity = this.NodeOpacity;
        newNote.height = length;
    },

    start () {

    },

    update (dt) {
        var pressed = this.Button._getButtonState() == 2;
        var pressed_hit = false;

        this.NoteNode.forEach(note => {
            if(note && note.active == false)
                return;
            
            var data = this.NoteMap.get(note);            

            data.cur = data.cur - dt;
            note.y = (data.maxY - data.minY) * (data.cur / data.time) + data.minY; 
            
            if(data.cur < 0)
            {
                note.y = data.minY;
                note.opacity = this.NodeOpacity + (255 - this.NodeOpacity) - ((255 - this.NodeOpacity) * ((data.limittime - data.cur) / data.limittime)); 

                if(data.cur < data.limittime)
                {
                    if(data.hit == false || pressed)
                        this.onFail(note);                            
                    else
                        this.onDone(note);
                }
            }

            if(note.y < (data.minY + data.height))
            {
                if(pressed)
                {
                    data.hit = true;
                    pressed_hit = true;

                    var center = data.minY + (data.height/2);                    
                    var cur = note.y;

                    var abs = Math.abs(cur - center);
                    
                    if(data.Normal > abs)
                    {
                        if(data.Good > abs)
                        {
                            if(data.Excellent > abs)
                            {
                                if(data.Perpect > abs)
                                {
                                    this.onPerfect(note);
                                }
                                else
                                    this.onExcellent(note);
                            }
                            else
                                this.onGood(note);
                        }
                        else
                            this.onNoraml(note);
                    }
                } 
                else
                {
                    if(data.hit)
                    {
                        this.onDone(note);
                    }
                }               
            }
        });        
        
        if(pressed && pressed_hit == false)
        {
            this.onMiss();
        }

        if(this.ReturnTime > 0)
        {
            this.ReturnTime = this.ReturnTime - dt;
            if(this.ReturnTime <= 0)
            {
                this.clearColorEffect();
            }
        }
    },

    setColorEffect(color)
    {
        this.CopyTarget.color = color;
        this.Button.normalColor = color;
        this.Button.pressdColor = color;
        this.Button.hoverColor = color;

        this.ReturnTime = 0.1;
    },

    clearColorEffect()
    {
        this.CopyTarget.color = this.CopyTargetOriginColor;
        this.Button.normalColor = this.ButtonNormalColor;
        this.Button.pressdColor = this.ButtonPressColor;
        this.Button.hoverColor = this.ButtonHoverColor;
    },

    onDone(note)
    {
        note.active = false;
    },

    onFail(note)
    {        
        this.setColorEffect(cc.Color.RED);        
        note.active = false;
    },

    onNoraml()
    {
        this.setColorEffect(cc.Color.YELLOW);        
    },

    onGood()
    {        
        this.setColorEffect(cc.Color.ORANGE);
    },

    onExcellent()
    {
        this.setColorEffect(cc.Color.BLUE);
    },

    onPerfect()
    {
        this.setColorEffect(cc.Color.GREEN);
    },

    onMiss()
    {
        var cur_note = null;
        this.NoteNode.forEach(note => {
            if(note.active == true)
            {
                if(cur_note == null)
                {
                    cur_note = note;
                }
                else
                {
                    if(cur_note.y > note.y)
                    {
                        cur_note = note;
                    }
                }
            }
        });

        if(cur_note == null)
            return;
        
        this.onFail(cur_note);
    },
});
