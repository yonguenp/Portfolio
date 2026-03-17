
import { _decorator } from 'cc';
import { ProductAutoData, ProductData } from './ProductData';
import { MultiTableBase } from './TableBase';

/**
 * Predefined variables
 * Name = ProductTable
 * DateTime = Wed Jan 12 2022 16:08:50 GMT+0900 (대한민국 표준시)
 * Author = seungmin
 * FileBasename = ProductTable.ts
 * FileBasenameNoExtension = ProductTable
 * URL = db://assets/Scripts/Data/ProductTable.ts
 * ManualUrl = https://docs.cocos.com/creator/3.3/manual/en/
 *
 */
 
export class ProductTable extends MultiTableBase<ProductData> {
    public static Name: string = "ProductTable";

    public SetTable(jsonData : Array<Array<string>>)
    {
        if(jsonData == null || jsonData.length < 1) {
            return;
        }

        this.DataClear();
        const columName = jsonData[0]
        const rowLength = jsonData.length
        const columLength = columName.length
        let data = this;

        for(let i = 1; i < rowLength; i++)
        {
            let rowData = jsonData[i]
            let product = new ProductData()
            let needItemIDArray : number[] = []
            let needItemAmountArray : number[] = []

            for(let j = 0; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "KEY":
                        product.ProductKey = Number(rowData[j])
                        break;

                    case "BUILDING_GROUP":
                        product.Index = rowData[j]
                        break

                    case "BUILDING_LEVEL":
                        product.BuildingLevel = Number(rowData[j])
                        break

                    case "ICON":
                        product.ProductIcon = rowData[j]
                        break

                    case "PRODUCT_ITEM":
                        product.ProductItemID = Number(rowData[j])
                        break

                    case "PRODUCT_NUM":
                        product.ProductAmount = Number(rowData[j])
                        break

                    case "PRODUCT_TIME":
                        product.ProductReqTime = Number(rowData[j])
                        break

                    case "NEED_GOLD":
                        product.NeedGold = Number(rowData[j])
                        break

                    case "NEED_ITEM_1": case "NEED_ITEM_2": case "NEED_ITEM_3":
                        if(Number(rowData[j]) > 0)    
                            needItemIDArray.push(Number(rowData[j]))
                        break

                    case "NEED_ITEM_1_NUM": case "NEED_ITEM_2_NUM": case "NEED_ITEM_3_NUM":
                        if(Number(rowData[j]) > 0)    
                            needItemAmountArray.push(Number(rowData[j]))
                        break
                }
            }
            product.NeedItemIDArray = needItemIDArray
            product.NeedItemAmountArray = needItemAmountArray

            data.Add(product)
        }
    }

    GetBuildingGroupByProductItem(productItem : number)
    {
        let tempGroup = "";
        let keys = Object.keys(this.datas);
        if(keys == null || keys.length <= 0){
            return tempGroup;
        }

        keys.forEach(element =>{
            let productList = this.datas[element];
            if(productList == null || productList.length <= 0){
                return;
            }

            let rowlength = productList.length;
            for(let i = 0 ; i < rowlength ; i++){
                let curProductItem = productList[i] as ProductData;
                let curproID = curProductItem.ProductItemID;
                let curGroup = curProductItem.Index;
                if(productItem == curproID)
                {
                    tempGroup = curGroup.toString();
                    break;
                }
            }
        })
        return tempGroup;
    }
}

export class ProductAutoTable extends MultiTableBase<ProductAutoData> {
    public static Name: string = "ProductAutoTable";

    public SetTable(jsonData : Array<Array<string>>)
    {
        if(jsonData == null || jsonData.length < 1) {
            return;
        }

        this.DataClear();
        const columName = jsonData[0]
        const rowLength = jsonData.length
        const columLength = columName.length
        const data = this;

        for(let i = 1; i < rowLength; i++)
        {
            let rowData = jsonData[i];
            let productAuto = new ProductAutoData();

            for(let j = 1; j < columLength; j++)
            {
                switch(columName[j])
                {
                    case "BUILDING_GROUP":
                        productAuto.Index = rowData[j];
                        break;

                    case "LEVEL":
                        productAuto.LEVEL = Number(rowData[j]);
                        break;

                    case "TYPE":
                        productAuto.TYPE = rowData[j];
                        break

                    case "VALUE":
                        productAuto.VALUE = Number(rowData[j])
                        break

                    case "VALUE":
                        productAuto.VALUE = Number(rowData[j])
                        break

                    case "TERM":
                        productAuto.TERM = Number(rowData[j])
                        break

                    case "NUM":
                        productAuto.NUM = Number(rowData[j])
                        break

                    case "MAX_TIME":
                        productAuto.MAX_TIME = Number(rowData[j])
                        break
                }
            }

            data.Add(productAuto)
        }
    }
}