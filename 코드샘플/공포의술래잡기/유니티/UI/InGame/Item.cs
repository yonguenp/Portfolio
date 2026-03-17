//using SandboxNetwork.Network.Data;

//namespace SandboxNetwork.Network
//{
//  public class ItemInfo
//  {
//      public int ItemDbId;
//      public int TemplateId;
//      public int Count;
//      public int Slot;
//      public bool Equipped;
//  }

//  public class Item
//  {
//      public ItemInfo Info { get; set; } = new ItemInfo();

//      public int ItemDbId
//      {
//          get { return Info.ItemDbId; }
//          set { Info.ItemDbId = value; }
//      }

//      public int TemplateId
//      {
//          get { return Info.TemplateId; }
//          set { Info.TemplateId = value; }
//      }

//      public int Count
//      {
//          get { return Info.Count; }
//          set { Info.Count = value; }
//      }

//      public int Slot
//      {
//          get { return Info.Slot; }
//          set { Info.Slot = value; }
//      }

//      public bool Equipped
//      {
//          get { return Info.Equipped; }
//          set { Info.Equipped = value; }
//      }

//      public ItemType ItemType { get; private set; }
//      public bool Stackable { get; protected set; }

//      public Item(ItemType itemType)
//      {
//          ItemType = itemType;
//      }

//      public static Item MakeItem(ItemInfo itemInfo)
//      {
//          Item item = null;

//          ItemData itemData = null;
//          Managers.Data.ItemDict.TryGetValue(itemInfo.TemplateId, out itemData);

//          if (itemData == null)
//              return null;

//          switch (itemData.itemType)
//          {
//              case ItemType.WEAPON:
//                  item = new Weapon(itemInfo.TemplateId);
//                  break;
//              case ItemType.ARMOR:
//                  item = new Armor(itemInfo.TemplateId);
//                  break;
//              case ItemType.CONSUMABLE:
//                  item = new Consumable(itemInfo.TemplateId);
//                  break;
//          }

//          if (item != null)
//          {
//              item.ItemDbId = itemInfo.ItemDbId;
//              item.Count = itemInfo.Count;
//              item.Slot = itemInfo.Slot;
//              item.Equipped = itemInfo.Equipped;
//          }

//          return item;
//      }
//  }

//  public class Weapon : Item
//  {
//      public WeaponType WeaponType { get; private set; }
//      public int Damage { get; private set; }

//      public Weapon(int templateId) : base(ItemType.WEAPON)
//      {
//          Init(templateId);
//      }

//      void Init(int templateId)
//      {
//          ItemData itemData = null;
//          Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
//          if (itemData.itemType != ItemType.WEAPON)
//              return;

//          WeaponData data = (WeaponData)itemData;
//          {
//              TemplateId = data.id;
//              Count = 1;
//              WeaponType = data.weaponType;
//              Damage = data.damage;
//              Stackable = false;
//          }
//      }
//  }

//  public class Armor : Item
//  {
//      public ArmorType ArmorType { get; private set; }
//      public int Defence { get; private set; }

//      public Armor(int templateId) : base(ItemType.ARMOR)
//      {
//          Init(templateId);
//      }

//      void Init(int templateId)
//      {
//          ItemData itemData = null;
//          Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
//          if (itemData.itemType != ItemType.ARMOR)
//              return;

//          ArmorData data = (ArmorData)itemData;
//          {
//              TemplateId = data.id;
//              Count = 1;
//              ArmorType = data.armorType;
//              Defence = data.defence;
//              Stackable = false;
//          }
//      }
//  }

//  public class Consumable : Item
//  {
//      public ConsumableType ConsumableType { get; private set; }
//      public int MaxCount { get; set; }

//      public Consumable(int templateId) : base(ItemType.CONSUMABLE)
//      {
//          Init(templateId);
//      }

//      void Init(int templateId)
//      {
//          ItemData itemData = null;
//          Managers.Data.ItemDict.TryGetValue(templateId, out itemData);
//          if (itemData.itemType != ItemType.CONSUMABLE)
//              return;

//          ConsumableData data = (ConsumableData)itemData;
//          {
//              TemplateId = data.id;
//              Count = 1;
//              MaxCount = data.maxCount;
//              ConsumableType = data.consumableType;
//              Stackable = (data.maxCount > 1);
//          }
//      }
//  }
//}
