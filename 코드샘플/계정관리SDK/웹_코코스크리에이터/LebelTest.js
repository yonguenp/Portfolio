// Learn cc.Class:
//  - https://docs.cocos.com/creator/manual/en/scripting/class.html
// Learn Attribute:
//  - https://docs.cocos.com/creator/manual/en/scripting/reference/attributes.html
// Learn life-cycle callbacks:
//  - https://docs.cocos.com/creator/manual/en/scripting/life-cycle-callbacks.html

cc.Class({
    extends: cc.Component,

    properties: {
        canvas : cc.Canvas,
        camera : cc.Camera,
        label : cc.Label,
        samandaMaterial : cc.Material,
        builtinMaterial : cc.Material,
    },

    onLoad () {
        var canvas = this.canvas;
        
        var orientationType = 0;
        
        switch(orientationType)
        {
            case 1:     
                
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                    canvas.designResolution = cc.size(1280, 720);
                    
                    this.camera.node.x = 640;
                    this.camera.node.y = 360;
                }       
                break;
            case 0:
                
                if(canvas)
                {
                    canvas.fitHeight = true;
                    canvas.fitWidth = true;
                    canvas.designResolution = cc.size(720, 1280);
                    
                    this.camera.node.x = 360;
                    this.camera.node.y = 640;
                }  
                break;
            default:
                break;
        }
    },

    onFontUp(){
        this.label.fontSize = this.label.fontSize + 1;
    },

    onFontDown(){
        this.label.fontSize = this.label.fontSize - 1;
    },

    onSamandaMaterial(){
        this.label.setMaterial(0, this.samandaMaterial);
    },

    onBuiltinMaterial(){
        this.label.setMaterial(0, this.builtinMaterial);
    },
});
