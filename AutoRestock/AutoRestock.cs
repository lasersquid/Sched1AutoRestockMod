using HarmonyLib;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Reflection;
using Newtonsoft.Json;
using SteamNetworkLib;
using Il2CppSteamworks;
using SteamNetworkLib.Models;
using System.Text;

#if MONO_BUILD
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Messaging;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Property;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI.Items;
using ScheduleOne.UI;
using ScheduleOne;
using ItemDefList = System.Collections.Generic.List<ScheduleOne.ItemFramework.ItemDefinition>;
using Registry = ScheduleOne.Registry;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.PlayerTasks;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne;
using ItemDefList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ItemFramework.ItemDefinition>;
using Registry = Il2CppScheduleOne.Registry;
#endif

namespace AutoRestock
{
    public static class Utils
    {
        private static AutoRestockMod Mod;
        private static Assembly S1Assembly;

        public static void Initialize(AutoRestockMod mod)
        {
            Mod = mod;
#if !MONO_BUILD
            S1Assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly a) => a.GetName().Name == "Assembly-CSharp");
#endif
        }

        public static void LateInitialize()
        {
            if (Mod.client != null)
            {
                Mod.client.RegisterMessageHandler<TransactionMessage>(Utils.ReceiveTransaction);

                // Debug
                Mod.client.RegisterMessageHandler<TextMessage>(Utils.ReceiveTextTransaction);
            }
            else 
            {
                Warn($"Client was null; couldn't register multiplayer transaction message handler.");
            }
        }

        public class TransactionMessage : P2PMessage
        {
            public override string MessageType => "TRANSACTION";
            public Manager.Transaction Payload { get; set; } = null;

            public override byte[] Serialize()
            {
                Utils.Log($"serializing transactionmessage: {Payload.ToString()}");
                var json = JsonConvert.SerializeObject(Payload);
                return Encoding.UTF8.GetBytes(json);
            }

            public override void Deserialize(byte[] data)
            {
                Utils.Log($"deserializing transactionmessage");
                try 
                {
                    var json = Encoding.UTF8.GetString(data);
                    Payload = JsonConvert.DeserializeObject<Manager.Transaction>(json);
                    Utils.Log($"Payload: : {Payload.ToString()}");
                }
                catch (Exception e)
                {
                    Utils.Log($"deserializing failed:");
                    Utils.PrintException(e);
                }
            }
        }


        public static void PrintException(Exception e)
        {
            Utils.Warn($"Exception: {e.GetType().Name} - {e.Message}");
            Utils.Warn($"Source: {e.Source}");
            Utils.Warn($"{e.StackTrace}");
            if (e.InnerException != null)
            {
                Utils.Warn($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                Utils.Warn($"Source: {e.InnerException.Source}");
                Utils.Warn($"{e.InnerException.StackTrace}");
                if (e.InnerException.InnerException != null)
                {
                    Utils.Warn($"Inner inner exception: {e.InnerException.InnerException.GetType().Name} - {e.InnerException.InnerException.Message}");
                    Utils.Warn($"Source: {e.InnerException.InnerException.Source}");
                    Utils.Warn($"{e.InnerException.InnerException.StackTrace}");
                }
            }
        }

        public static void Log(string message)
        {
            Mod.LoggerInstance.Msg(message);
        }

        public static void Warn(string message)
        {
            Mod.LoggerInstance.Warning(message);
        }

        public static void Debug(string message)
        {
            if (Manager.isInitialized && Manager.melonPrefs.GetEntry<bool>("debugLogs").Value)
            {
                Mod.LoggerInstance.Msg($"DEBUG: {message}");
            }
        }

        public static void VerboseLog(string message)
        {
            if (Manager.isInitialized && Manager.melonPrefs.GetEntry<bool>("verboseLogs").Value)
            {
                Mod.LoggerInstance.Msg(message);
            }
        }

        public static Treturn GetField<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return (Treturn)GetField<Ttarget>(fieldName, target);
        }

        public static object GetField<Ttarget>(string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(typeof(Ttarget), fieldName).GetValue(target);
#else
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
#endif
        }

        public static void SetField<Ttarget>(string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(typeof(Ttarget), fieldName).SetValue(target, value);
#else
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
#endif
        }

        public static Treturn GetProperty<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return (Treturn)GetProperty<Ttarget>(fieldName, target);
        }

        public static object GetProperty<Ttarget>(string fieldName, object target)
        {
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
        }

        public static void SetProperty<Ttarget>(string fieldName, object target, object value)
        {
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target) where Treturn : class
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, []);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target, object[] args) where Treturn : class
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, args);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, Type[] argTypes, object target, object[] args) where Treturn : class
        {
            return (Treturn)CallMethod<Ttarget>(methodName, argTypes, target, args);
        }

        public static object CallMethod<Ttarget>(string methodName, object target)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, []);
        }

        public static object CallMethod<Ttarget>(string methodName, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, args);
        }

        public static object CallMethod<Ttarget>(string methodName, Type[] argTypes, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName, argTypes).Invoke(target, args);
        }

        // In Mono, just do a regular cast that returns default (usually null) on failure.
#if MONO_BUILD
        public static T CastTo<T>(object o)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }
#else
        // In Il2Cpp, type check against object identity before performing a blind cast.
        public static T CastTo<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            if (typeof(T).IsAssignableFrom(GetType(o)))
            {
                return (T)System.Activator.CreateInstance(typeof(T), [((Il2CppObjectBase)o).Pointer]);
            }
            return default(T);
        }
#endif

        // Under Il2Cpp, "is" operator only looks at local scope for
        // type info, instead of checking object identity. 
        // Check against actual object type obtained via GetType.
#if MONO_BUILD
        public static bool Is<T>(object o)
        {
            return o is T;
        }
#else
        public static bool Is<T>(Il2CppObjectBase o)
        {
            return typeof(T).IsAssignableFrom(GetType(o));
        }
#endif

        // In Mono, perform a regular cast.
#if MONO_BUILD
        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }
#else
        // You can't do a standard cast to or from an interface type in IL2CPP, since interface info is stripped.
        // Use this method to perform a blind cast without type checking.
        // Use carefully.
        public static T ToInterface<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            return (T)System.Activator.CreateInstance(typeof(T), [((Il2CppObjectBase)o).Pointer]);
        }
#endif

#if MONO_BUILD
        public static Type GetType(object o)
        {
            if (o == null)
            {
                return null;
            }
            return o.GetType();
        }
#else
        public static Type GetType(Il2CppObjectBase o)
        {
            string typeName = Il2CppType.TypeFromPointer(o.ObjectClass).FullName;
            return S1Assembly.GetType($"Il2Cpp{typeName}");
        }
#endif
        public static UnityAction ToUnityAction(Action action)
        {
#if MONO_BUILD
            return new UnityAction(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction>(action);
#endif
        }

        public static UnityAction<T> ToUnityAction<T>(Action<T> action)
        {
#if MONO_BUILD
            return new UnityAction<T>(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction<T>>(action);
#endif
        }

        // Convert a delegate to a predicate that IL2CPP ienumerable functions can actually use.
#if MONO_BUILD
        public static Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return new Predicate<T>(func);
        }
#else
        public static Il2CppSystem.Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<T>>(func);
        }
#endif

        // Compare unity objects by their instance ID
        public class UnityObjectComparer : IEqualityComparer<UnityEngine.Object>
        {
            public bool Equals(UnityEngine.Object a, UnityEngine.Object b)
            {
                return a.GetInstanceID() == b.GetInstanceID();
            }

            public int GetHashCode(UnityEngine.Object item)
            {
                return item.GetInstanceID();
            }
        }

        public static ItemDefList GetItemDefsContaining(List<string> terms)
        {
            ItemDefList itemDefs = Registry.Instance.GetAllItems();
            ItemDefList matches = itemDefs.FindAll(Utils.ToPredicate<ItemDefinition>((def) =>
            {
                return terms.Any((term) => def.ID.Contains(term));
            }));
            return matches;
        }

        public static bool IsQualityIngredient(string itemID)
        {
            List<string> qualityIngredients = ["pseudo"];
            return qualityIngredients.Any((id) => itemID.Contains(id));
        }

        public static StorableItemInstance GetItemInstance(string itemID, EQuality quality = EQuality.Standard)
        {
            if (IsQualityIngredient(itemID))
            {
                return GetQualityItemInstance(itemID, quality);
            }
            return Utils.CastTo<StorableItemInstance>(Registry.GetItem(itemID).GetDefaultInstance());
        }

        private static Dictionary<EQuality, string> qualityStrings = new Dictionary<EQuality, string>
        {
            { EQuality.Heavenly, "heavenly" },
            { EQuality.Premium, "highquality" },
            { EQuality.Standard, "" },
            { EQuality.Poor, "lowquality" },
            { EQuality.Trash, "trash" }
        };

        public static string GetQualityItemID(QualityItemInstance item)
        {
            return $"{qualityStrings[item.Quality]}{item.ID}";
        }

        public static QualityItemInstance GetQualityItemInstance(string itemID, EQuality quality)
        {
            string qualityItemID;
            if (IsQualityIngredient(itemID))
            {
                qualityItemID = $"{qualityStrings[quality]}{itemID}";
            }
            else
            {
                Utils.Warn($"itemid {itemID} is not a quality ingredient?");
                return null;
            }
            QualityItemInstance instance = Utils.CastTo<QualityItemInstance>(Registry.GetItem(qualityItemID).GetDefaultInstance());
            instance.Quality = quality;
            return instance;
        }

        public static bool IsStorageRack(ITransitEntity transitEntity)
        {
            if (transitEntity != null && Utils.Is<PlaceableStorageEntity>(transitEntity))
            {
                PlaceableStorageEntity placeable = Utils.CastTo<PlaceableStorageEntity>(transitEntity);
                return IsStorageRack(placeable);
            }
            return false;
        }

        public static bool IsStorageRack(IItemSlotOwner slotOwner)
        {
            if (slotOwner != null && Utils.Is<StorageEntity>(slotOwner))
            {
                StorageEntity storageEntity = Utils.CastTo<StorageEntity>(slotOwner);
                return IsStorageRack(storageEntity);
            }
            return false;
        }

        public static bool IsStorageRack(StorageEntity entity)
        {
            if (entity != null)
            {
                PlaceableStorageEntity placeable = entity.GetComponent<PlaceableStorageEntity>();
                if (placeable != null)
                {
                    return IsStorageRack(placeable);
                }
                else
                {
                    Utils.Warn($"StorageEntity {entity.name} did not have PlaceableStorageEntity component");
                }
                return false;
            }
            return false;
        }

        public static bool IsStorageRack(PlaceableStorageEntity placeable)
        {
            List<string> itemIDs = ["safe", "wallmountedshelf",
                "smallstoragerack", "mediumstoragerack", "largestoragerack",
                "smallstoragecloset", "mediumstoragecloset", "largestoragecloset", "hugestoragecloset"
            ];
            if (placeable != null)
            {
                string placeableID = placeable.ItemInstance.ID;
                return itemIDs.Any((id) => placeableID.Contains(id));
            }
            return false;
        }

        public static bool IsStation(ITransitEntity transitEntity)
        {
            if (transitEntity != null && Utils.Is<GridItem>(transitEntity))
            {
                GridItem gridItem = Utils.CastTo<GridItem>(transitEntity);
                return IsStation(gridItem);
            }
            return false;
        }

        public static bool IsStation(IItemSlotOwner slotOwner)
        {
            if (slotOwner != null && Utils.Is<GridItem>(slotOwner))
            {
                GridItem gridItem = Utils.CastTo<GridItem>(slotOwner);
                return IsStation(gridItem);
            }
            return false;
        }

        public static bool IsStation(GridItem gridItem)
        {
            List<Type> stationTypes = [typeof(PackagingStation), typeof(Cauldron), typeof(ChemistryStation), typeof(MixingStation), typeof(MushroomSpawnStation), typeof(LabOven), typeof(DryingRack)];
            if (gridItem != null)
            {
                Type ownerType = Utils.GetType(gridItem);
                return stationTypes.Any((t) => t.IsAssignableFrom(ownerType));
            }
            return false;
        }

        public static void ReceiveTransaction(TransactionMessage transactionMessage, CSteamID sender)
        {
            if (!Manager.isInitialized)
            {
                Log($"Couldn't process transaction, Manager is not initialized!");
                return;
            }

            try
            {
                Log($"Received transaction from {Utils.SteamIDToDisplayName(sender)}.");
                Manager.Transaction transaction = transactionMessage.Payload;
                StorableItemInstance itemInstance = GetItemInstance(transaction.itemID);
                ItemSlot slot = Manager.DeserializeSlot(transaction.slotID);
                if (slot == null)
                {
                    Warn($"Couldn't deserialize slot ({transaction.slotID.ToString()})");
                    return;
                }
                Manager.TryRestocking(slot, itemInstance, transaction.quantity);
            }
            catch (Exception e)
            {
                Utils.Warn($"Error receiving transaction:");
                Utils.PrintException(e);
            }
        }

        public static string SteamIDToDisplayName(CSteamID steamID)
        {
            if (Mod.steamIDToDisplayName.ContainsKey(steamID))
            {
                return Mod.steamIDToDisplayName[steamID];
            }
            else
            {
                return $"{steamID}";
            }
        }

        public static void ReceiveTextTransaction(TextMessage message, CSteamID sender)
        {
            
            try 
            {
                Log($"ReceiveTextTransaction: received transaction from {Mod.steamIDToDisplayName[sender]}.");
                Manager.Transaction transaction = JsonConvert.DeserializeObject<Manager.Transaction>(message.Content);
                if (transaction == null)
                {
                    Log($"Couldn't deserialize json: {message.Content}");
                }
                StorableItemInstance itemInstance = GetItemInstance(transaction.itemID);
                ItemSlot slot = Manager.DeserializeSlot(transaction.slotID);
                if (slot == null)
                {
                    Log($"Couldn't deserialize slot ({transaction.slotID.ToString()})");
                    return;
                }
                Manager.TryRestocking(slot, itemInstance, transaction.quantity);
            }
            catch (Exception e)
            {
                Utils.Warn($"Error receiving text transaction:");
                Utils.PrintException(e);
            }
        }

        public static void SendTransaction(Manager.Transaction transaction)
        {
            try 
            {
                TransactionMessage transactionMessage = new TransactionMessage { Payload = transaction };
                Log($"Sending transaction for {transaction.itemID} x{transaction.quantity} at {transaction.slotID.ToString()}.");
                Mod.client?.SendMessageToPlayerAsync(Mod.host, transactionMessage);
                Mod.client?.SendTextMessageAsync(Mod.host, JsonConvert.SerializeObject(transaction));
            }
            catch (Exception e)
            {
                Utils.Warn($"Couldn't send transaction:");
                Utils.PrintException(e);
            }
        }

#if MONO
        public static List<T> ListConvert<T>(List<T> list)
        {
            return list;
        }
#else
        public static List<T> ListConvert<T>(Il2CppSystem.Collections.Generic.List<T> list)
        {
            List<T> newList = new List<T>();
            foreach (T item in list)
            {
                newList.Add(item);
            }
            return newList;
        }
#endif
    }

    public static class Manager
    {
        public class Transaction
        {
            public string itemID;
            public int quality;
            public int quantity;
            public float discount;
            public float unitPrice;
            public float totalCost;
            public bool useCash;
            public SlotIdentifier slotID;

            public Transaction(string itemID, EQuality quality, int quantity, float discount, float unitPrice, float totalCost, bool useCash, SlotIdentifier slotID)
            {
                this.itemID = itemID;
                this.quality = (int)quality;
                this.quantity = quantity;
                this.discount = discount;
                this.unitPrice = unitPrice;
                this.totalCost = totalCost;
                this.useCash = useCash;
                this.slotID = slotID;
            }

            public override string ToString()
            {
                return $"{itemID} x{quantity} for {slotID.ToString()}";
            }
        }

        public class SlotIdentifier
        {
            public List<float> gridLocation;
            public string type;
            public int slotIndex;
            public string property;
            public string grid;

            public SlotIdentifier(string property, Vector2 gridLocation, int slotIndex, string type, string grid)
            {
                this.property = property;
                this.gridLocation = new List<float>([gridLocation.x, gridLocation.y,]);
                this.slotIndex = slotIndex;
                this.type = type;
                this.grid = grid;
            }

            [JsonConstructor]
            public SlotIdentifier(string property, List<float> gridLocation, int slotIndex, string type, string grid)
            {
                this.property = property;
                this.gridLocation = gridLocation;
                this.slotIndex = slotIndex;
                this.type = type;
                this.grid = grid;
            }

            public override string ToString()
            {
                return $"{type} slot {slotIndex} at {property} ({grid}: {gridLocation[0]}, {gridLocation[1]})";
            }
        }

        public static bool isClient;
        public static ItemSlot playerClickedSlot;
        public static bool doRestockPlayerClickedSlot;
        public static List<ItemSlot> playerStationOperationSlots;
        public static MelonPreferences_Category melonPrefs;
        private static TimeManager timeManager;
        private static MoneyManager moneyManager;
        private static SaveManager saveManager;
        private static NPC oscar;
        private static string ledgerString;
        private static string transactionString;
        private static List<Transaction> ledger;
        private static Dictionary<Transaction, object> coroutines;
        private static EDay ledgerDay;
        private static Mutex exclusiveLock;
        public static bool isInitialized = false;

        public static SlotIdentifier SerializeSlot(ItemSlot slot)
        {
            GridItem gridItem;

            if (Utils.IsStation(slot.SlotOwner))
            {
                gridItem = Utils.CastTo<GridItem>(slot.SlotOwner);
            }
            else if (Utils.IsStorageRack(slot.SlotOwner))
            {
                StorageEntity storageEntity = Utils.CastTo<StorageEntity>(slot.SlotOwner);
                gridItem = storageEntity.gameObject.GetComponent<GridItem>();
            }
            else
            {
                Utils.Warn($"Couldn't serialize itemslot--not station or storage rack? ({Utils.GetType(slot.SlotOwner)})");
                return null;
            }

            string type = gridItem.ItemInstance.Definition.ID;
            string property = gridItem.ParentProperty.name;
            string grid = gridItem.OwnerGrid.name;
            Vector2 coordinate = (Vector2)Utils.GetField<GridItem>("_originCoordinate", gridItem);
            int slotIndex = (int)Utils.GetProperty<ItemSlot>("SlotIndex", slot);
            SlotIdentifier slotID = new SlotIdentifier(property, coordinate, slotIndex, type, grid);
            return slotID;
        }

        public static ItemSlot DeserializeSlot(SlotIdentifier identifier)
        {
            try
            {
                List<Property> properties = UnityEngine.Object.FindObjectsOfType<Property>().ToList();
                Property property = properties.FirstOrDefault<Property>((Property p) => p.name == identifier.property && p.Grids.Count > 0);
                List<BuildableItem> gridItemsOnProperty = Utils.ListConvert<BuildableItem>(property.BuildableItems);

                Vector2 targetCoord = new Vector2(identifier.gridLocation[0], identifier.gridLocation[1]);
                BuildableItem buildableItem = gridItemsOnProperty.FirstOrDefault<BuildableItem>((BuildableItem b) =>
                {
                    if (Utils.Is<GridItem>(b))
                    {
                        GridItem g = Utils.CastTo<GridItem>(b);
                        return g._originCoordinate == targetCoord && g.OwnerGrid.name == identifier.grid;
                    }
                    return false;
                });
                if (buildableItem == null)
                {
                    Utils.Warn($"Couldn't deserialize slot--coordinates did not map to a griditem");
                    return null;
                }
                GridItem gridItem = Utils.CastTo<GridItem>(buildableItem);

                IItemSlotOwner slotOwner;
                if (Utils.IsStation(gridItem))
                {
                    slotOwner = Utils.ToInterface<IItemSlotOwner>(gridItem);
                }
                else if (Utils.Is<PlaceableStorageEntity>(gridItem))
                {
                    StorageEntity storageEntity = Utils.CastTo<PlaceableStorageEntity>(gridItem).StorageEntity;
                    slotOwner = Utils.ToInterface<IItemSlotOwner>(storageEntity);
                }
                else
                {
                    Utils.Warn($"couldn't deserialize slot--obj was not a station or placeablestorageentity ({Utils.GetType(gridItem)})");
                    return null;
                }

                if (slotOwner.ItemSlots.Count <= identifier.slotIndex)
                {
                    Utils.Warn($"couldn't deserialize slot--slot index was greater than itemslot count ({identifier.slotIndex} > {slotOwner.ItemSlots.Count})");
                    return null;
                }
                return slotOwner.ItemSlots[identifier.slotIndex];
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }

            return null;
        }

        public static Transaction CreateTransaction(ItemSlot slot, ItemInstance item, int quantity)
        {
            string itemID = item.ID;
            EQuality quality;
            if (Utils.Is<QualityItemInstance>(item))
            {
                quality = Utils.CastTo<QualityItemInstance>(item).Quality;
            }
            else 
            {
                quality = EQuality.Standard;
            }
            
            int restockAmountSetting = Mathf.Max(melonPrefs.GetEntry<int>("restockAmount").Value, 0);
            int restockQuantity = Mathf.Min(restockAmountSetting == 0 ? quantity : restockAmountSetting, item.StackLimit);
            float discount = Mathf.Clamp01(melonPrefs.GetEntry<float>("itemDiscount").Value);
            float unitPrice = item.GetMonetaryValue() * 2f / (float)item.Quantity;
            float totalCost = unitPrice * (float)restockQuantity * (1f - discount);
            bool useCash = melonPrefs.GetEntry<bool>("payWithCash").Value;
            SlotIdentifier slotID = Manager.SerializeSlot(slot);

            return new Transaction(itemID, quality, restockQuantity, discount, unitPrice, totalCost, useCash, slotID);
        }

        public static void CompleteTransactions(List<Transaction> transactions)
        {
            try
            {
                foreach (Transaction transaction in transactions)
                {
                    ItemSlot slot = DeserializeSlot(transaction.slotID);
                    if (slot == null)
                    {
                        Utils.Warn($"Couldn't deserialize slot!");
                        continue;
                    }
                    StorableItemInstance item = Utils.GetItemInstance(transaction.itemID);
                    TryRestocking(slot, item, transaction.quantity);
                }
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        public static void Initialize()
        {
            try
            {
                melonPrefs = MelonPreferences.GetCategory("AutoRestock");
                timeManager = NetworkSingleton<TimeManager>.Instance;
                moneyManager = NetworkSingleton<MoneyManager>.Instance;
                saveManager = SaveManager.Instance;

                ledgerDay = timeManager.CurrentDay;
                oscar = UnityEngine.Object.FindObjectsOfType<NPC>(true).FirstOrDefault((npc) => npc.ID == "oscar_holland");

                playerClickedSlot = null;
                doRestockPlayerClickedSlot = false;

                playerStationOperationSlots = new List<ItemSlot>();
                coroutines = new Dictionary<Transaction, object>();

                exclusiveLock = new Mutex();

                timeManager.onDayPass += new Action(Manager.OnDayPass);
                saveManager.onSaveStart.AddListener(Utils.ToUnityAction(Manager.OnSaveStart));

                if (InstanceFinder.IsServer)
                {
                    ledgerString = $"{GetSaveString()}_ledger";
                    transactionString = $"{GetSaveString()}_inprogress";

                    if (!melonPrefs.HasEntry(ledgerString))
                    {
                        melonPrefs.CreateEntry<string>(ledgerString, "[]", "", true);
                    }
                    if (!melonPrefs.HasEntry(transactionString))
                    {
                        melonPrefs.CreateEntry<string>(transactionString, "[]", "", true);
                    }
                    melonPrefs.LoadFromFile(false);
                
                    ledger = JsonConvert.DeserializeObject<List<Transaction>>(melonPrefs.GetEntry<string>(ledgerString).Value);
                }

                isInitialized = true;
                Utils.Log($"AutoRestock manager initialized {(InstanceFinder.IsServer ? "for host" : "for client")}.");
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }

            try
            {
                if (InstanceFinder.IsServer)
                {
                    List<Transaction> pendingTransactions = JsonConvert.DeserializeObject<List<Transaction>>(melonPrefs.GetEntry<string>(transactionString).Value);
                    if (pendingTransactions.Count > 0)
                    {
                        Utils.Log($"Completing {pendingTransactions.Count} pending transaction{(pendingTransactions.Count == 1 ? "" : "s")}.");
                        CompleteTransactions(pendingTransactions);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        public static void Stop()
        {
            if (isInitialized)
            {
                isInitialized = false;
                ledger.Clear();
                StopCoroutines();
                exclusiveLock.Dispose();
            }
        }

        public static void StopCoroutines()
        {
            if (isInitialized)
            {
                foreach (var entry in coroutines)
                {
                    MelonCoroutines.Stop(entry.Value);
                }
                coroutines.Clear();
            }
        }

        public static void AcquireMutex()
        {
            if (isInitialized)
            {
                exclusiveLock.WaitOne();
            }
        }

        public static void ReleaseMutex()
        {
            if (isInitialized)
            {
                exclusiveLock.ReleaseMutex();
            }
        }

        public static bool ItemIsRestockable(string itemID)
        {
            if (isInitialized)
            {
                List<string> whitelistCategories = ["Agriculture", "Consumable", "Ingredient", "Packaging"];
                List<string> whitelistItems = ["speedgrow"];
                List<string> blacklistItems = ["cocaleaf", "cocainebase", "liquidmeth", "shroomspawn"];
                List<string> cashOnlyItems = ["cocaseed", "granddaddypurpleseed", "greencrackseed", "ogkushseed", "sourdieselseed", "sporesyringe"];

                ItemDefinition itemDef = Registry.GetItem(itemID);
                return ((whitelistCategories.Contains(itemDef.Category.ToString()) || whitelistItems.Contains(itemDef.ID)) &&
                    !blacklistItems.Contains(itemDef.ID));
            }
            return false;
        }

        public static void TryRestocking(ItemSlot slot, StorableItemInstance item, int quantity)
        {
            if (isInitialized && InstanceFinder.IsServer)
            {
                Transaction transaction = CreateTransaction(slot, item, quantity);

                try
                {
                    if (item.StackLimit == 0)
                    {
                        Utils.Debug($"Stacklimit ({item.StackLimit}) == 0. Not restocking.");
                        item.RequestClearSlot();
                        return;
                    }

                    if (Manager.ItemIsRestockable(item.ID))
                    {
                        float balance = transaction.useCash ? moneyManager.cashBalance : moneyManager.onlineBalance;
                        if (balance < transaction.totalCost)
                        {
                            Utils.Log($"Can't afford to restock {transaction.quantity}x {transaction.itemID} (${transaction.totalCost}).");
                        }
                        else if (balance >= transaction.totalCost)
                        {
                            AcquireMutex();
                            ledger.Add(transaction);
                            coroutines[transaction] = MelonCoroutines.Start(RestockCoroutine(transaction));
                            ReleaseMutex();
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.PrintException(e);
                    ReleaseMutex();
                }
            }
            else
            {
                Utils.Log($"Tried to restock item, but Manager was not initialized!");
            }
        }

        private static IEnumerator RestockCoroutine(Transaction transaction)
        {
            // Give S1 methods a chance to complete before altering slot.
            yield return new WaitForEndOfFrame();

            ItemSlot slot = DeserializeSlot(transaction.slotID);
            slot.ApplyLock(oscar.NetworkObject, "Restocking item", false);
            slot.SetIsAddLocked(true);
            yield return new WaitForSeconds(1f);

            StorableItemInstance item = Utils.GetItemInstance(transaction.itemID);
            if (Utils.Is<QualityItemInstance>(item))
            {
                Utils.CastTo<QualityItemInstance>(item).Quality = (EQuality)transaction.quality;
            }

            // Don't charge the player for items remaining in the slot.
            // There will usually be zero items remaining, but if the mixing station mixer slot falls below 
            // the station threshold, a restock can be triggered with items remaining.
            int quantity;
            float totalCost;
            if (transaction.quantity + slot.Quantity > item.StackLimit)
            {
                quantity = item.StackLimit - slot.Quantity;
                totalCost = (int)((float)quantity * transaction.unitPrice * transaction.discount);

                // Update transaction object with new quantity for accurate receipt reporting
                Manager.AcquireMutex();
                transaction.quantity = quantity;
                transaction.totalCost = totalCost;
                Manager.ReleaseMutex();
            }
            else
            {
                quantity = transaction.quantity;
                totalCost = transaction.totalCost;
            }
            Utils.VerboseLog($"Restocking {item.Name} (${transaction.unitPrice}) x{quantity} with a discount of {transaction.discount} at {transaction.slotID.property}. Total: ${totalCost}.");

            bool didPay = false;
            if (totalCost <= 0f)
            {
                if (quantity > 0)
                {
                    Utils.VerboseLog($"Total cost of transaction is $0. Get a freebie!");
                }
                didPay = true;
            }
            else
            {
                bool useCash = melonPrefs.GetEntry<bool>("payWithCash").Value;
                float balance = useCash ? moneyManager.cashBalance : moneyManager.onlineBalance;
                if (balance < totalCost)
                {
                    Utils.Log($"Insufficient balance to restock {item.Name} (${transaction.unitPrice}) x{quantity} with a discount of {transaction.discount}, at {transaction.slotID.property}, total ${totalCost}; aborting.");
                    AcquireMutex();
                    ledger.Remove(transaction);
                    ReleaseMutex();
                }
                else
                {
                    if (useCash)
                    {
                        moneyManager.ChangeCashBalance(-totalCost);
                    }
                    else
                    {
                        moneyManager.CreateOnlineTransaction("Restock", -totalCost, 1f, $"{item.Definition.Name}");
                    }
                    didPay = true;
                }
            }

            // Tyler. This is not how you implement a lock.
            // You let the person acquire the lock, modify the protected value, then release the lock.
            // You don't release the lock immediately before modifying the protected value.
            slot.SetIsAddLocked(false);
            slot.RemoveLock();
            if (didPay && quantity > 0)
            {
                item.SetQuantity(quantity);
                slot.AddItem(item);
            }

            Manager.AcquireMutex();
            Manager.coroutines.Remove(transaction);
            Manager.ReleaseMutex();
            yield break;
        }

        private static void OnDayPass()
        {
            if (isInitialized && InstanceFinder.IsServer)
            {
                NetworkSingleton<MessagingManager>.Instance.SendMessage(
                    new Message(GetReceipt(), Message.ESenderType.Other, true, -1),
                    true,
                    oscar.ID);

                ledger.Clear();
                ledgerDay = timeManager.CurrentDay;
            }
        }

        private static string GetSaveString()
        {
            string[] savePathStrings = LoadManager.Instance.LoadedGameFolderPath.Split('\\');
            return $"{savePathStrings[^2]}_{savePathStrings[^1]}";
        }

        public static void OnSaveStart()
        {
            if (Manager.isInitialized && InstanceFinder.IsServer)
            {
                string todaysLedger = $"{GetSaveString()}_ledger";
                if (melonPrefs.HasEntry(todaysLedger))
                {
                    melonPrefs.GetEntry<string>(todaysLedger).EditedValue = LedgerToJson();
                }
                else
                {
                    MelonPreferences_Entry entry = melonPrefs.CreateEntry<string>(todaysLedger, "", "", true);
                    entry.BoxedEditedValue = LedgerToJson();
                }
                string transactionsInProgress = $"{GetSaveString()}_inprogress";
                if (melonPrefs.HasEntry(transactionsInProgress))
                {
                    melonPrefs.GetEntry<string>(transactionsInProgress).EditedValue = PendingTransactionsToJson();
                }
                else
                {
                    MelonPreferences_Entry entry = melonPrefs.CreateEntry<string>(transactionsInProgress, "", "", true);
                    entry.BoxedEditedValue = PendingTransactionsToJson();
                }

                melonPrefs.SaveToFile(false);
            }
        }

        public static List<Transaction> GetPendingTransactions()
        {
            return Manager.coroutines.Keys.ToList();
        }

        public static string PendingTransactionsToJson()
        {
            return JsonConvert.SerializeObject(GetPendingTransactions());
        }

        public static float LedgerTotal()
        {
            return ledger.Aggregate<Transaction, float>(0f, (float accum, Transaction transaction) =>
                accum + transaction.totalCost
            );
        }

        public static string LedgerToJson()
        {
            return JsonConvert.SerializeObject(ledger);
        }

        private static string GetReceipt()
        {
            if (isInitialized)
            {
                string receipt = $"Restock receipt for {ledgerDay}:\n\n";
                float grandTotal = 0f;
                if (ledger.Count > 0)
                {
                    Dictionary<string, Dictionary<string, float>> itemTotals = new Dictionary<string, Dictionary<string, float>>();
                    Dictionary<string, Dictionary<string, int>> itemQuantities = new Dictionary<string, Dictionary<string, int>>();
                    Dictionary<string, float> itemPrices = new Dictionary<string, float>();
                    Dictionary<string, float> totalCosts = new Dictionary<string, float>();

                    foreach (var transaction in ledger)
                    {
                        if (!itemTotals.ContainsKey(transaction.slotID.property))
                        {
                            itemTotals[transaction.slotID.property] = new Dictionary<string, float>();
                        }
                        if (!itemTotals[transaction.slotID.property].ContainsKey(transaction.itemID))
                        {
                            itemTotals[transaction.slotID.property][transaction.itemID] = 0f;
                        }
                        if (!itemQuantities.ContainsKey(transaction.slotID.property))
                        {
                            itemQuantities[transaction.slotID.property] = new Dictionary<string, int>();
                        }
                        if (!itemQuantities[transaction.slotID.property].ContainsKey(transaction.itemID))
                        {
                            itemQuantities[transaction.slotID.property][transaction.itemID] = 0;
                        }
                        itemQuantities[transaction.slotID.property][transaction.itemID] += transaction.quantity;
                        itemTotals[transaction.slotID.property][transaction.itemID] += transaction.totalCost;
                        itemPrices[transaction.itemID] = transaction.unitPrice;
                    }

                    foreach (string property in itemTotals.Keys)
                    {
                        float propertyTotal = 0f;
                        receipt += $"{property}: \n";
                        foreach (var entry in itemQuantities[property])
                        {
                            string name = entry.Key;
                            float quantity = entry.Value;
                            float total = itemTotals[property][name];

                            string prettyName = Registry.GetItem(name).Name;
                            receipt += $"  {prettyName} x{quantity} = ${total}\n";
                            propertyTotal += total;
                        }
                        receipt += "=====================\n";
                        receipt += $"  Property total: ${propertyTotal}\n\n";
                        grandTotal += propertyTotal;
                    }
                }
                receipt += "=====================\n";
                receipt += $"  Grand total: ${grandTotal}\n\n";
                receipt += $"Oscar says thank you for your business! :)";

                return receipt;
            }
            else
            {
                return "AutoRestock not initialized!";
            }
        }
    }

    [HarmonyPatch]
    public class PersistencePatches
    {
        // We want to initialize Manager after scene is loaded, but before the player gains control.
        // LoadManager.Start is too early.
        // Player.Activate just doesn't fire at all
        // maybe playercamera.setcanlook? nope, not called either
        // playercamera.lockmouse? nope
        // gameinput.start? nope
        // loadingscreen.close? ding ding ding
        [HarmonyPatch(typeof(LoadingScreen), "Close")]
        [HarmonyPostfix]
        public static void ClosePostfix(LoadingScreen __instance)
        {
            if (!Manager.isInitialized)
            {
                Manager.Initialize();
            }
        }

        [HarmonyPatch(typeof(LoadManager), "ExitToMenu")]
        [HarmonyPrefix]
        public static void ExitToMenuPrefix(LoadManager __instance)
        {
            if (Manager.isInitialized)
            {
                Manager.Stop();
            }
        }
    }

    [HarmonyPatch]
    public class CauldronPatches
    {
        [HarmonyPatch(typeof(Cauldron), "RemoveIngredients")]
        [HarmonyPrefix]
        public static void RemoveIngredientsPrefix(Cauldron __instance)
        {
            if (!Manager.isInitialized)
            {
                return;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableCauldrons").Value)
                {
                    return;
                }
                if (__instance.LiquidSlot.ItemInstance.Quantity > 1)
                {
                    return;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value ||
                    __instance.PlayerUserObject == null)
                {
                    StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(__instance.LiquidSlot.ItemInstance.GetCopy());
                    Manager.TryRestocking(__instance.LiquidSlot, newItem, newItem.StackLimit);
                }
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
            return;
        }
    }

    [HarmonyPatch]
    public class MixingStationPatches
    {
        [HarmonyPatch(typeof(MixingStation), "SendMixingOperation")]
        [HarmonyPrefix]
        public static void SendMixingOperationPrefix(MixingStation __instance, MixOperation operation)
        {
            if (!Manager.isInitialized)
            {
                return;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableMixingStations").Value)
                {
                    return;
                }
                float threshold = Utils.GetProperty<MixingStation, MixingStationConfiguration>("stationConfiguration", __instance).StartThrehold.GetData().Value;
                if ((__instance.MixerSlot.Quantity - operation.Quantity) >= threshold)
                {
                    return;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value || __instance.PlayerUserObject == null)
                {
                    StorableItemInstance newItem = Utils.GetItemInstance(operation.IngredientID);
                    Manager.TryRestocking(__instance.MixerSlot, newItem, newItem.StackLimit);
                }
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
            return;
        }
    }

    [HarmonyPatch]
    public class PackagingStationPatches
    {
        [HarmonyPatch(typeof(PackagingStation), "PackSingleInstance")]
        [HarmonyPrefix]
        public static void PackSingleInstancePrefix(PackagingStation __instance)
        {
            if (!Manager.isInitialized)
            {
                return;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enablePackagingStations").Value)
                {
                    return;
                }
                if (__instance.PackagingSlot.ItemInstance == null || __instance.PackagingSlot.ItemInstance.Quantity > 1)
                {
                    return;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value || __instance.PlayerUserObject == null)
                {
                    StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(__instance.PackagingSlot.ItemInstance.GetCopy(1));
                    Manager.TryRestocking(__instance.PackagingSlot, newItem, newItem.StackLimit);
                }
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
            return;
        }
    }

    [HarmonyPatch]
    public class ChemistryStationPatches
    {
        private static List<List<T>> Permute<T>(List<T> nums)
        {
            var list = new List<List<T>>();
            return DoPermute<T>(nums, 0, nums.Count - 1, list);
        }

        private static List<List<T>> DoPermute<T>(List<T> nums, int start, int end, List<List<T>> list)
        {
            if (start == end)
            {
                list.Add(new List<T>(nums));
            }
            else
            {
                for (var i = start; i <= end; i++)
                {
                    Swap(nums, start, i);
                    DoPermute(nums, start + 1, end, list);
                    Swap(nums, start, i);
                }
            }

            return list;
        }

        private static void Swap<T>(List<T> list, int index1, int index2)
        {
            T item = list[index1];
            list[index1] = list[index2];
            list[index2] = item;
        }

        [HarmonyPatch(typeof(ChemistryStation), "SendCookOperation")]
        [HarmonyPrefix]
        public static bool SendCookOperationPrefix(ChemistryStation __instance, ChemistryCookOperation op)
        {
            if (!Manager.isInitialized)
            {
                return true;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableChemistryStations").Value)
                {
                    return true;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value ||
                    __instance.PlayerUserObject == null)
                {
                    // TODO: see if we can just grab the ingredients during ItemSlot.ChangeQuantity patch
                    // See what ingredients we're missing
                    List<ItemDefinition> missingIngredients = new List<ItemDefinition>();
                    foreach (StationRecipe.IngredientQuantity i in op.Recipe.Ingredients)
                    {
                        if (!__instance.IngredientSlots.Any<ItemSlot>((ItemSlot slot) =>
                            slot.ItemInstance != null && slot.ItemInstance.Definition.Name.Contains(i.Item.Name)
                        ))
                        {
                            missingIngredients.Add(i.Item);
                        }
                    }

                    // If any ingredients are missing, we need to determine a valid missingingredient -> empty slot mapping
                    List<List<ItemDefinition>> possibleMappings = new List<List<ItemDefinition>>();
                    List<List<ItemDefinition>> validMappings = new List<List<ItemDefinition>>();
                    if (missingIngredients.Count > 0)
                    {
                        // First, find the empty slots
                        List<ItemSlot> emptySlots = new List<ItemSlot>();
                        foreach (ItemSlot slot in __instance.IngredientSlots)
                        {
                            if (slot.Quantity == 0)
                            {
                                emptySlots.Add(slot);
                            }
                        }

                        // now generate the mappings
                        // With three slots, we only have 6 possibilities max, so just permute
                        possibleMappings = Permute<ItemDefinition>(missingIngredients);

                        // now find valid mappings
                        validMappings = possibleMappings.Select<List<ItemDefinition>, List<ItemDefinition>>((List<ItemDefinition> mapping) =>
                        {
                            // is this mapping valid
                            bool isValid = true;
                            for (int i = 0; i < mapping.Count; i++)
                            {
                                ItemInstance item = Utils.GetItemInstance(mapping[i].ID, op.ProductQuality);

                                if (!emptySlots[i].DoesItemMatchPlayerFilters(item))
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                            return isValid ? mapping : null;
                        }).ToList<List<ItemDefinition>>();
                        validMappings.RemoveAll((List<ItemDefinition> list) => list == null);

                        if (validMappings.Count == 0)
                        {
                            string property = __instance.ParentProperty.name;
                            Vector2 coordinate = (Vector2)Utils.GetField<GridItem>("_originCoordinate", __instance);
                            Utils.Log($"Couldn't restock {__instance.GetManagementName()} at {property}({coordinate.x}, {coordinate.y}) because items do not agree with filters.");
                            return true;
                        }
                    }

                    int emptySlotsFilled = 0;
                    foreach (ItemSlot slot in __instance.IngredientSlots)
                    {
                        if (slot.ItemInstance == null && missingIngredients.Count > 0)
                        {
                            ItemDefinition newDef = validMappings[0][emptySlotsFilled];
                            StorableItemInstance newItem = Utils.GetItemInstance(newDef.ID, op.ProductQuality);
                            Manager.TryRestocking(slot, newItem, newItem.StackLimit);
                            emptySlotsFilled++;
                        }
                        else
                        {
                            int quantity = 0;
                            foreach (var entry in op.Recipe.Ingredients)
                            {
                                if (slot.ItemInstance.Definition.Name.Contains(entry.Item.Name))
                                {
                                    quantity = entry.Quantity;
                                    break;
                                }
                            }

                            if (quantity > 0 && slot.ItemInstance.Quantity < quantity)
                            {
                                StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(slot.ItemInstance.GetCopy(1));
                                Manager.TryRestocking(slot, newItem, newItem.StackLimit);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
            return true;
        }
    }

    [HarmonyPatch]
    public class StorageEntityPatches
    {
        // Keep track of player-initiated itemslot changes.
        // We only care about the case where a player picks up or
        // quick-drops from an itemslot, since those are the only
        // actions that could deplete the itemslot.
        [HarmonyPatch(typeof(ItemUIManager), "Update")]
        [HarmonyPrefix]
        public static void UpdatePrefix(ItemUIManager __instance)
        {
            if (__instance.DraggingEnabled)
            {
                // Did we just start a drag?
                ItemSlotUI hoveredSlot = Utils.CallMethod<ItemUIManager, ItemSlotUI>("GetHoveredItemSlot", __instance);
                ItemSlotUI draggedSlot = Utils.GetField<ItemUIManager, ItemSlotUI>("draggedSlot", __instance);
                if (draggedSlot == null && hoveredSlot != null &&
                    (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) ||
                     GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick) ||
                     GameInput.GetButtonDown(GameInput.ButtonCode.TertiaryClick)) &&
                     Manager.playerClickedSlot == null)
                {
                    Manager.playerClickedSlot = hoveredSlot.assignedSlot;

                    // Are we holding left ctrl?
                    if (Keyboard.current.leftCtrlKey.isPressed)
                    {
                        Manager.doRestockPlayerClickedSlot = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ItemUIManager), "Update")]
        [HarmonyPostfix]
        public static void UpdatePostfix(ItemUIManager __instance)
        {
            if (__instance.DraggingEnabled)
            {
                // Did we just end a drag?
                ItemSlotUI draggedSlot = Utils.GetField<ItemUIManager, ItemSlotUI>("draggedSlot", __instance);
                if (draggedSlot == null && Manager.playerClickedSlot != null)
                {
                    Manager.playerClickedSlot = null;
                    Manager.doRestockPlayerClickedSlot = false;
                }
            }
        }

        // Only restock when an NPC has depleted storage rack item slot,
        // or the player is holding left ctrl.
        [HarmonyPatch(typeof(ItemSlot), "ChangeQuantity")]
        [HarmonyPrefix]
        public static void ChangeQuantityPrefix(ItemSlot __instance, ref int change)
        {
            if (!Manager.isInitialized)
            {
                return;
            }

            try
            {
                if (__instance.ItemInstance == null || (__instance.Quantity + change > 0))
                {
                    return;
                }
                if (!Manager.melonPrefs.GetEntry<bool>("enableStorage").Value)
                {
                    return;
                }

                // TODO: investigate if we can just use this patch and scrap all station-specific ones
                // we'd still need to mark slots as used on player interactions.
                // not hard to determine through UI methods.
                // might have to do some shenanigans for multiplayer though
                if (!Utils.IsStorageRack(__instance.SlotOwner))
                {
                    return;
                }

                // Return if this slot was clicked but not ctrl+clicked
                if (!((Manager.playerClickedSlot == __instance && Manager.doRestockPlayerClickedSlot) || Manager.playerClickedSlot != __instance))
                {
                    return;
                }

                StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(__instance.ItemInstance.GetCopy(1));
                if (!InstanceFinder.IsServer)
                {
                    Manager.Transaction transaction = Manager.CreateTransaction(__instance, __instance.ItemInstance, __instance.ItemInstance.StackLimit);
                    Utils.SendTransaction(transaction);
                }
                else
                {
                    // DEBUG
                    // Manager.Transaction transaction = Manager.CreateTransaction(__instance, __instance.ItemInstance, __instance.ItemInstance.StackLimit);
                    // Utils.SendTransaction(transaction);

                    Manager.TryRestocking(__instance, Utils.CastTo<StorableItemInstance>(__instance.ItemInstance), __instance.ItemInstance.StackLimit);
                }
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
        }
    }

    [HarmonyPatch]
    public class SpawnStationPatches
    {
        [HarmonyPatch(typeof(InocculateGrainBagTask), "Success")]
        [HarmonyPostfix]
        public static void SuccessPostfix(InocculateGrainBagTask __instance)
        {
            MushroomSpawnStation station = Utils.GetField<InocculateGrainBagTask, MushroomSpawnStation>("_station", __instance);
            if (station.GrainBagSlot.Quantity == 0 && Manager.melonPrefs.GetEntry<bool>("enableSpawnStations").Value && Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value)
            {
                StorableItemInstance grainBagInstance = Utils.GetItemInstance("grainbag");
                Manager.TryRestocking(station.GrainBagSlot, grainBagInstance, grainBagInstance.StackLimit);
            }
            if (station.SyringeSlot.Quantity == 0 && Manager.melonPrefs.GetEntry<bool>("enableSpawnStations").Value && Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value)
            {
                StorableItemInstance syringeInstance = Utils.GetItemInstance("sporesyringe");
                Manager.TryRestocking(station.SyringeSlot, syringeInstance, syringeInstance.StackLimit);
            }
        }

        [HarmonyPatch(typeof(UseSpawnStationBehaviour), "StopWork")]
        [HarmonyPrefix]
        public static void StopWorkPrefix(UseSpawnStationBehaviour __instance)
        {
            if (__instance.Station.GrainBagSlot.Quantity == 0 && Manager.melonPrefs.GetEntry<bool>("enableSpawnStations").Value)
            {
                StorableItemInstance grainBag = Utils.GetItemInstance("grainbag");
                Manager.TryRestocking(__instance.Station.GrainBagSlot, grainBag, grainBag.StackLimit);
            }
            if (__instance.Station.SyringeSlot.Quantity == 0 && Manager.melonPrefs.GetEntry<bool>("enableSpawnStations").Value)
            {
                StorableItemInstance syringe = Utils.GetItemInstance("sporesyringe");
                Manager.TryRestocking(__instance.Station.SyringeSlot, syringe, syringe.StackLimit);
            }
        }
    }
}