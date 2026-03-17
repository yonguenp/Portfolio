

cc.Label.prototype.ui_width = function(){
    return this.node.width * this.node.scaleX;
}

cc.Label.prototype.ui_height = function(){
    return this.node.height * this.node.scaleY;
}