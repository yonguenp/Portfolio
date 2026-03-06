cc.Class({
    extends: cc.Component,
    properties: {
        // item:[cc.Prefab],
        // itemData:[],//item data
        // scrollView:cc.ScrollView,
                 // itemInterval: 0, // spacing
        // firstItemData:{},
        // lastItemData:{},
        // itemArr:[],
        // itemNode:[],
    },
    
    // Get the total control script under the node of this script, used to pass to all its members item;
    setTarget(target) {
        this.target = target;
    },
    
    //itemData is the item data, and itemPosMap is the item position data to be used when the scroll bar is scrolled.
    init(itemData, scrollView, item, itemInterval, scrollViewCanFix, isSkipInit) {
        //The first three items cannot be empty
        if (!itemData||!scrollView||!item) {　　
            return false;　　
        }　　
        
        this.itemData = itemData; //item data
        this.scrollView = scrollView;
        if(item.constructor!==Array){
            let arr=[];
            arr.push(item);
            this.item=arr;
        }else{
            this.item = item;
        }
        this.itemInterval = itemInterval || 0; //pitch
        this.scrollViewCanFix = scrollViewCanFix || true; //Automatically correct
        //Initialize various properties
        this.content = this.scrollView.content;
        this.layerHeight = this.scrollView.node.height;
        this.updateTimer = 0;
        this.updateInterval = 5;
        this.firstItemIndex = 0;
        this.lastItemIndex = 0;
        this.firstItemData = {};
        this.lastItemData = {};
        this.itemArr = [];
        this.itemNode = [];
        this.itemPosMap = new Map();
        this.initItemData = true;
        this.count = 0;
        this.itemNodeHeight=[];
        this.scrollView.node.on('scrolling', this.callback,this);
        
        if (isSkipInit||itemData.length<1) {            
            return;
        }

        this.initItem();
    },
    
    //Can get the location information of the specified index, used to set the position of the prefab
    getItemPos(index) {
        if(this.itemData.length<1){
            return 0;
        }
        for (let i = index; i < this.itemData.length; i++) {
            let obj = {}
            let y;
            if (i === 0) {
                obj.startPos = 0;
            } else {
                obj.startPos = this.itemPosMap.get(i - 1).endPos;
            }
            let j = this.itemData[i].pfbType||0;
            //If there is no height of the prefabricated body, get it.
            if(this.itemNodeHeight[j]){
                obj.endPos = obj.startPos + this.itemNodeHeight[j] + this.itemInterval;
            }else{
                let y;
                if(i-1>=0){
                     y=this.itemPosMap.get(i- 1).endPos;
                }else{
                    y=0;
                }
                this.addItemNode(i,-y);
                obj.endPos = obj.startPos + this.itemNodeHeight[j] + this.itemInterval;
            }
            this.itemPosMap.set(i, obj);
        }
        this.updateContentHeigh(this.itemPosMap.get(this.itemData.length - 1).endPos);
    },
    
    // Instantiate the pre-form when first loaded
    initItem() {
        //Instantiate all used items; j control the number of instantiated items, tentatively exceeds two
        for (let i = 0; i < this.itemData.length; i++) {
            if (this.content.height > this.layerHeight) {
                if (i != 0) {                    
                    break;
                }
            }
            let y;
            if (i === 0) {
                y = 0;
            } else {
                y = this.itemArr[i - 1].y - this.itemArr[i - 1].height - this.itemInterval;
            }
            this.addItemNode(i, y);
            this.updateContentHeigh(this.itemArr[i].height - y);
        } 
    },
    
    // When scrolling, when the cache array this.itemArr has no related prefabs, generate a
    addItemNode(i, y) {
        let pfbType = this.itemData[i].pfbType||0;
        let item = this.getItemNode(pfbType);
        item.parent = this.content;
        item.pfbType = pfbType;
        item.index = i;
        if (i === 0) {
            item.y = 0;
        } else {
            item.y = y;
        }
        item.x = 0;
        // Assign value to item
        item.getComponent(cc.Component).init(this.itemData[i], this);
        this.itemArr.push(item);
    },

    getItemNode(pfbType){
        let item=cc.instantiate(this.item[pfbType]);
        this.itemNodeHeight[pfbType]=item.height;
        return item;
    },

    updateContentHeigh(num) {
        this.content.height = num > this.layerHeight ? num : this.layerHeight;
        //cc.log('scrollbar height:', this.content.height);
    },

    // Touch the scroll bar function callback
    callback(event, eventType) {
        this.updateTimer += 1;
        if (this.updateTimer < this.updateInterval) return; // we don't need to do the math every frame
        this.updateTimer = 0;
        //cc.log(event && event.type || eventType)
        if (this.content&&this.content.height > this.layerHeight) {
 
            let firstItemPos = this.scrollView.getScrollOffset().y;
            let lastItemPos = firstItemPos + this.layerHeight;
            if (firstItemPos < 0) return;
                         //Execute only once for additional initialization.
            if (this.initItemData) {
                this.getItemPos(0);
                this.initItemData = false;
                this.updateFirstItemIndex(firstItemPos);
                this.itemCanMoveDown = true;
                this.updateLastItemIndex(lastItemPos);
                this.itemCanMoveDown = false;
            }
 
            // Return directly beyond the border.
            //The scroll bar will slide up to trigger the function.
            if (firstItemPos > this.firstItemData.endPos) {
                if (this.lastItemIndex + 1 < this.itemData.length) {
                    this.updateFirstItemIndex(firstItemPos);
                }
                this.count++;
            }
            if (lastItemPos > this.lastItemData.endPos) {
                if (this.lastItemIndex + 1 < this.itemData.length) {
                    this.itemCanMoveDown = true;
                    this.updateLastItemIndex(lastItemPos);
                    this.itemCanMoveDown = false;
                }
            }
            //The scrollbar slides down to trigger a function that may be triggered.
            if (lastItemPos < this.lastItemData.startPos) {
                this.updateLastItemIndex(lastItemPos);
                this.count--;
            }
            if (firstItemPos < this.firstItemData.startPos) {
                this.itemCanMoveUp = true;
                this.updateFirstItemIndex(firstItemPos);
                this.itemCanMoveUp = false;
            }
 
        }
 
    },
    
    //Preform the move operation when the top prefab index changes.
    updateFirstItemIndex() {
        let num = this.firstItemIndex;
        if (this.itemCanMoveUp && num > this.getItemIndex()[0] && num > 0) {
            this.itemMoveUp(this.firstItemIndex - 1);
        }
    },
    
    //Preform the move operation when the bottom prefabricated index changes.
    updateLastItemIndex() {
        let num = this.lastItemIndex;
        if (this.itemCanMoveDown && num < this.getItemIndex()[1] && num + 1 < this.itemData.length) {
            this.itemMoveDown(this.lastItemIndex + 1);
        }
    },
 
    
    //Get the itemNode element that should be there in the state of the scroll bar, including one above the scroll bar and one below the scroll bar.
    getItemIndex() {
        let firstItemPos = this.scrollView.getScrollOffset().y;
        let lastItemPos = firstItemPos + this.layerHeight;
        let arr = [];
        for (let [key, value] of this.itemPosMap.entries()) {
            // Determine the positional relationship of the state is [);
            let status1 = value.startPos <= firstItemPos && value.endPos > firstItemPos;
            // let status2 = value.startPos >= firstItemPos && value.endPos < lastItemPos;
            let status3 = value.startPos <= lastItemPos && value.endPos > lastItemPos;
            if (status1) {
                this.firstItemData.startPos = value.startPos;
                this.firstItemData.endPos = value.endPos;
                this.firstItemIndex = key;
                arr.push(key);
            }
            if (status3) {
                this.lastItemData.startPos = value.startPos;
                this.lastItemData.endPos = value.endPos;
                this.lastItemIndex = key;
                arr.push(key);
            }
        }
        return arr;
    },
    
    //firstIndex The scroll bar order is traversed from top to bottom.
    itemMoveUp(num) {
        if (num < 0 || this.lastItemIndex + 1 < num || num + 1 > this.itemData.length) {
            return;
        }
        if (!this.hasItem(num)) {
            this.itemMove(num, -this.itemPosMap.get(num).startPos);
        }
        num++;
        return this.itemMoveUp(num);
 
    },
    
    //firstIndex The scroll bar order is traversed from bottom to top.
    itemMoveDown(num) {
        if (num < 0 || this.firstItemIndex - 1 > num || num + 1 > this.itemData.length) {
            return;
        }
        if (!this.hasItem(num)) {
            this.itemMove(num, -this.itemPosMap.get(num).startPos);
        }
        num--;
        return this.itemMoveDown(num);
 
    },
    
    // Determine whether there is an itemNode in the specified index position.
    hasItem(index) {
        for (let i = 0; i < this.itemArr.length; i++) {
            if (this.itemArr[i].index === index) {
                return true;
            }
        }
        return false;
    },
    
    // Get the index index of the item to be moved,
    // Logical judgment, the first case, modify an object in the basemArr array, the second case instantiates a new itemNode
    itemMove(index, y) {
        for (let i = 0; i < this.itemArr.length; i++) {
 
                         //index has -3, similar to the item in the cache pool.
            let status1 = this.itemArr[i].index < this.firstItemIndex - 1 ? true : false;
            let status2 = this.itemArr[i].index > this.lastItemIndex + 1 ? true : false;
            let status3 = this.itemArr[i].pfbType === this.itemData[index].pfbType;
            //cc.log('item's index', this.firstItemIndex, this.lastItemIndex)
            // If the item is in the expanded state, do not participate in the sort
            if (this.itemArr[i].isOpen) {
                status1 = false;
                status2 = false;
            }
            if (status1 && status3 || status2 && status3) {
                cc.log(i, index, this.itemArr, this.content.height);
                // Assign value to item and set position
                this.itemArr[i].index = index;
                this.itemArr[i].y = y;
                this.itemArr[i].getComponent(cc.Component).init(this.itemData[index], this);
                return;
            }
        }
        this.addItemNode(index, y);
    },
    
    // Get the sort index of the relevant location
    getPosIndex(pos) {
        for (let [key, value] of this.itemPosMap.entries()) {
            if (value.endPos > pos && value.startPos <= pos) {
                return key;
            }
        }
    },
    
    // Add an item, similar to the chat when sending messages.
    addItem(obj) {
        this.itemData.push(obj);
        this.getItemPos(0);
        let endPos = this.itemPosMap.get(this.itemData.length - 1).endPos;
        if (endPos - this.layerHeight > 0) {
            let startPos = endPos - this.layerHeight;
            // Get the current firstItemIndex;
            for (let i = this.itemData.length - 1; i >= 0; i--) {
                if (this.itemPosMap.get(i).endPos > startPos && this.itemPosMap.get(i).startPos <= startPos) {
                    this.firstItemIndex = i;
                }
            }
            this.scrollView.scrollToBottom();
            this.content=this.scrollView.content;
            this.lastItemIndex = this.itemData.length - 1;
            let num = this.firstItemIndex - 1 > 0 ? (this.firstItemIndex - 1) : 0;
            this.itemMoveUp(num);
            return true;
        } else {
            this.firstItemIndex = 0;
            this.lastItemIndex = this.itemData.length - 1;
            this.itemMoveUp(this.firstItemIndex);
            return false;
        }
 
    },
    
    // Clear all items on the interface, such as clear chat information.
    clearItem() {
        this.itemData = [];
        this.itemPosMap.clear();
        this.scrollView.scrollToTop();
        this.content.height = 0;
        for (let i in this.itemArr) {
            this.itemArr[i].index = -3;
            this.itemArr[i].y = 5000;
        }
    },
    
    // Delete a specified item,
    deleteItem(i) {
        this.itemData.splice(i, 1);
        this.getItemPos(0);
        // Change the content of this.itemArr
        for (let j = 0; j < this.itemArr.length; j++) {
            if (this.itemArr[j].index === i) {
                this.itemArr[j].index = -3;
                this.itemArr[j].y = 3000;
            }
            if (this.itemArr[j].index > i) {
                let num = this.itemArr[j].index;
                this.itemArr[j].y = -this.itemPosMap.get(num - 1).startPos;
                this.itemArr[j].index = num - 1;
            }
        }
        this.itemMoveUp(this.firstItemIndex);
    },
    
    // Clear all items, the itemData data is also cleared.
    resetItemData(index) {
        for (let i = 0; i < this.itemArr.length; i++) {
            if (this.itemArr[i].index === index) {
                let js = this.itemArr[i].getComponent(cc.Component);
                js.init(this.itemData[index], this);
                break;
            }
        }
    },
    
    // Applicable to the situation of the development of the record
    resetItemSize(index, infoHeight) {
        cc.log(infoHeight)
        let func = (function (index, infoHeight) {
            for (let i = 0; i < this.itemArr.length; i++) {
                //The itemNode that is behind the clicked node moves down.
                if (this.itemArr[i].index > index) {
                    this.itemArr[i].y -= infoHeight;
                }
                //If they are equal, it indicates that itemNode is already open.
                if (this.itemArr[i].index === index) {
                    if (this.itemArr[i].isOpen) {
                        this.itemArr[i].isOpen = false;
                    } else {
                        this.itemArr[i].isOpen = true;
                    }
                } else {
                    if (this.itemArr[i].isOpen) {
                        this.itemArr[i].isOpen = false;
                    }
                }
 
            }
            // Modify the location map related data
            for (let [key, value] of this.itemPosMap.entries()) {
                if (key === index) {
                    value.endPos += infoHeight;
                }
                if (key > index) {
                    value.endPos += infoHeight;
                    value.startPos += infoHeight;
                }
            }
            this.lastResetItemInfoHeight = infoHeight > 0 ? infoHeight : 0;
            this.updateContentHeigh(this.itemPosMap.get(this.itemData.length - 1).endPos);
        }).bind(this);
        if (this.lastResetItemIndex !== null) {
            // If there is an expanded index, determine whether the index of the newly clicked node is the same.
            if (this.lastResetItemIndex === index) {
                func(this.lastResetItemIndex, -this.lastResetItemInfoHeight);
 
                this.lastResetItemIndex = null;
                this.lastResetItemInfoHeight = 0;
            } else {
                cc.log(this.itemArr, this.itemPosMap)
                if (this.lastResetItemIndex < index) {
                    let offset = this.scrollView.getScrollOffset();
                    offset.y -= this.lastResetItemInfoHeight;
                    this.scrollView.scrollToOffset(offset)
                    cc.log(offset)
                }
 
                func(this.lastResetItemIndex, -this.lastResetItemInfoHeight);
                func(index, infoHeight);
                this.lastResetItemIndex = index;
            }
        } else {
            func(index, infoHeight);
            this.lastResetItemIndex = index;
        }
        //Re-acquire index, and sort
        this.getItemIndex();
        this.itemMoveUp(this.firstItemIndex);
    },
    
    destroyItemArr(){
        this.itemArr.forEach((e)=>{
            e.destroy();
        })

        this.itemArr=[];
        this.scrollView.content.height=0;
    }
 
});