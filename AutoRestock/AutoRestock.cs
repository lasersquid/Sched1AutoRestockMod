using HarmonyLib;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using Newtonsoft.Json;

#if MONO_BUILD
using FishNet.Object;
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
using ScheduleOne.Property;
using ScheduleOne.StationFramework;
using ScheduleOne.Storage;
using ScheduleOne.UI;
#else
using Il2CppFishNet.Object;
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
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.UI;
#endif

namespace AutoRestock
{
    public class Sched1PatchesBase
    {
        protected static AutoRestockMod Mod;

        public static void SetMod(AutoRestockMod mod)
        {
            Mod = mod;
        }
    }

    public static class Utils
    {
        private static AutoRestockMod Mod;

        public static void SetMod(AutoRestockMod mod)
        {
            Mod = mod;
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

        public static object GetField(Type type, string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(type, fieldName).GetValue(target);
#else
            return AccessTools.Property(type, fieldName).GetValue(target);
#endif
        }

        public static void SetField(Type type, string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(type, fieldName).SetValue(target, value);
#else
            AccessTools.Property(type, fieldName).SetValue(target, value);
#endif
        }

        public static object GetProperty(Type type, string fieldName, object target)
        {
            return AccessTools.Property(type, fieldName).GetValue(target);
        }

        public static void SetProperty(Type type, string fieldName, object target, object value)
        {
            AccessTools.Property(type, fieldName).SetValue(target, value);
        }

        public static object CallMethod(Type type, string methodName, object target, object[] args)
        {
            return AccessTools.Method(type, methodName).Invoke(target, args);
        }

        public static T CastTo<T>(object o) where T : class
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return null;
            }
        }

        public static bool Is<T>(object o)
        {
            return o is T;
        }

#if !MONO_BUILD
        public static T CastTo<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>();
        }

        public static bool Is<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>() != null;
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

        public static bool IsQualityIngredient(string itemID)
        {
            List<string> qualityIngredients = ["pseudo"];
            return qualityIngredients.Aggregate<string, bool>(false, (bool accum, string name) => accum || itemID.Contains(name));
        }

        public static ItemDefinition GetItemDef(string itemID)
        {
#if MONO_BUILD
            return ScheduleOne.Registry.GetItem(itemID);
#else
            return Il2CppScheduleOne.Registry.GetItem(itemID);
#endif
        }

        public static StorableItemInstance GetItemInstance(string itemID)
        {
#if MONO_BUILD
            return new StorableItemInstance(ScheduleOne.Registry.GetItem(itemID), 1);
#else
            return new StorableItemInstance(Il2CppScheduleOne.Registry.GetItem(itemID), 1);
#endif
        }

        public static QualityItemInstance GetItemInstance(string itemID, EQuality quality)
        {
            Dictionary<EQuality, string> qualityStrings = new Dictionary<EQuality, string>
            {
                { EQuality.Heavenly, "heavenly" },
                { EQuality.Premium, "highquality" },
                { EQuality.Standard, "" },
                { EQuality.Poor, "lowquality" },
                { EQuality.Trash, "trash" }
            };

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
            return new QualityItemInstance(GetItemDef(qualityItemID), 1, quality);
        }

        public static bool IsObjectStorageRack(IItemSlotOwner slotOwner)
        {
#if MONO_BUILD
            string objName = ((UnityEngine.Object)slotOwner).name;
#else
            string objName = new UnityEngine.Object(slotOwner.Pointer).name;
#endif
            List<string> shelves = ["Safe", "Small Storage Rack", "Medium Storage Rack", "Large Storage Rack", "StorageRack"];
            return shelves.Aggregate<string, bool>(false, (bool accum, string name) => accum || objName.Contains(name));
        }

        public static bool IsObjectBuildableItem(IItemSlotOwner slotOwner)
        {
#if MONO_BUILD
            string objName = ((UnityEngine.Object)slotOwner).name;
#else
            string objName = new UnityEngine.Object(slotOwner.Pointer).name;
#endif
            List<string> stations = ["PackagingStation", "PackagingStationMk2", "Cauldron", "ChemistryStation", "MixingStation", "MixingStationMk2"];
            return stations.Aggregate<string, bool>(false, (bool accum, string name) => accum || objName.Contains(name));

        }
    }

    public static class Manager
    {
        public class Transaction
        {
            public string itemID;
            public int quantity;
            public float discount;
            public float unitPrice;
            public float totalCost;
            public string property;
            public bool useCash;
            public SlotIdentifier slotID;

            public Transaction(string itemID, int quantity, float discount, float unitPrice, float totalCost, bool useCash, SlotIdentifier slotID)
            {
                this.itemID = itemID;
                this.quantity = quantity;
                this.discount = discount;
                this.unitPrice = unitPrice;
                this.totalCost = totalCost;
                this.useCash = useCash;
                this.slotID = slotID;
            }
        }

        public class SlotIdentifier
        {
            public List<float> gridLocation;
            public string type;
            public int slotIndex;
            public string property;

            public SlotIdentifier(string property, Vector2 gridLocation, int slotIndex, string type)
            {
                this.property = property;
                this.gridLocation = new List<float>([gridLocation.x, gridLocation.y,]);
                this.slotIndex = slotIndex;
                this.type = type;
            }

            [JsonConstructor]
            public SlotIdentifier(string property, List<float> gridLocation, int slotIndex, string type)
            {
                this.property = property;
                this.gridLocation = gridLocation;
                this.slotIndex = slotIndex;
                this.type = type;
            }
        }
        
        public static Dictionary<ItemSlot, NPC> shelfAccessors;
        public static MelonPreferences_Category melonPrefs;
        private static TimeManager timeManager;
        private static MoneyManager moneyManager;
        private static SaveManager saveManager;
        private static string ledgerString;
        private static string transactionString;
        private static List<Transaction> ledger;
        private static Dictionary<Transaction, object> coroutines;
        private static NPC lockOwner;
        private static EDay ledgerDay;
        private static Mutex exclusiveLock;
        public static bool isInitialized = false;

        public static SlotIdentifier SerializeSlot(ItemSlot slot)
        {
            GridItem gridItem;
            string type;

            if (Utils.IsObjectBuildableItem(slot.SlotOwner))
            {
#if MONO_BUILD
                BuildableItem buildableItem = (BuildableItem)slot.SlotOwner;
#else
                BuildableItem buildableItem = new BuildableItem(slot.SlotOwner.Pointer);
#endif
                gridItem = buildableItem.gameObject.GetComponent<GridItem>();
                type = buildableItem.name;

            }
            else if (Utils.IsObjectStorageRack(slot.SlotOwner))
            {
#if MONO_BUILD
                StorageEntity storageEntity = (StorageEntity)slot.SlotOwner;
#else
                StorageEntity storageEntity = new StorageEntity(slot.SlotOwner.Pointer);
#endif
                gridItem = storageEntity.gameObject.GetComponent<PlaceableStorageEntity>();
                type = storageEntity.name;
            }
            else
            {
                Utils.Warn($"Couldn't serialize itemslot!");
                return null;
            }

            string property = gridItem.ParentProperty.name;

            return new SlotIdentifier(property, gridItem.OriginCoordinate, (int)Utils.GetProperty(typeof(ItemSlot), "SlotIndex", slot), type);
        }

        public static string StringToType(string typeString)
        {
            List<string> stations = ["Cauldron", "ChemistryStation", "MixingStationMk2", "MixingStation", "PackagingStationMk2", "PackagingStation"];
            string objectType = stations.Find((string s) => typeString.Contains(s));
            if (objectType != null && objectType.Length > 0)
            {
                return objectType;
            }

            List<string> storage = ["StorageRack", "Safe"];
            if (storage.Aggregate<string, bool>(false, (bool accum, string s) => accum || typeString.Contains(s)))
            {
                return "StorageEntity";
            }

            return null;
        }

        public static ItemSlot DeserializeSlot(SlotIdentifier identifier)
        {
            try
            {
                List<Property> properties = UnityEngine.Object.FindObjectsOfType<Property>().ToList();
                List<GridItem> gridItems = UnityEngine.Object.FindObjectsOfType<GridItem>().ToList();
                Property property = properties.FirstOrDefault<Property>((Property p) => p.name == identifier.property);
                List<GridItem> gridItemsOnProperty = gridItems.FindAll((GridItem g) => g.ParentProperty == property);
                GridItem gridItem = gridItemsOnProperty.Single<GridItem>((GridItem g) => g.OriginCoordinate == new Vector2(identifier.gridLocation[0], identifier.gridLocation[1]));
#if MONO_BUILD
                Component slotOwner = gridItem.gameObject.GetComponent(StringToType(identifier.type));
                return ((IItemSlotOwner)slotOwner).ItemSlots[identifier.slotIndex];
#else
                Component slotOwner = gridItem.gameObject.GetComponentByName(StringToType(identifier.type));
                return new IItemSlotOwner(slotOwner.Pointer).ItemSlots[identifier.slotIndex];
#endif
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }

            return null;
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

                shelfAccessors = new Dictionary<ItemSlot, NPC>();
                coroutines = new Dictionary<Transaction, object>();

                lockOwner = UnityEngine.Object.FindObjectsOfType<NPC>().FirstOrDefault((NPC npc) => npc.ID == "oscar_holland");
                exclusiveLock = new Mutex();

                timeManager.onDayPass += new Action(Manager.OnDayPass);
                saveManager.onSaveStart.AddListener(Utils.ToUnityAction(Manager.OnSaveStart));

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

                isInitialized = true;
                Utils.Log($"AutoRestock manager initialized.");
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }

            try
            {
                List<Transaction> pendingTransactions = JsonConvert.DeserializeObject<List<Transaction>>(melonPrefs.GetEntry<string>(transactionString).Value);
                if (pendingTransactions.Count > 0)
                {
                    Utils.Log($"Completing {pendingTransactions.Count} pending transaction{(pendingTransactions.Count > 1 ? "s" : "")}.");
                    CompleteTransactions(pendingTransactions);
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
                lockOwner = null;
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
                List<string> blacklistItems = ["cocaleaf", "cocainebase", "liquidmeth"];
                List<string> cashOnlyItems = ["cocaseed", "granddaddypurpleseed", "greencrackseed", "ogkushseed", "sourdieselseed"];

                ItemDefinition itemDef = Utils.GetItemDef(itemID);
                return ((whitelistCategories.Contains(itemDef.Category.ToString()) || whitelistItems.Contains(itemDef.ID)) &&
                    !blacklistItems.Contains(itemDef.ID));
            }
            return false;
        }

        public static void TryRestocking(ItemSlot slot, StorableItemInstance item, int quantity)
        {
            if (isInitialized && InstanceFinder.IsServer)
            {
                string itemID = item.ID;
                float discount = Mathf.Clamp(melonPrefs.GetEntry<float>("itemDiscount").Value, 0f, 1f);
                float unitPrice = item.GetMonetaryValue() / (float)item.Quantity;
                float totalCost = unitPrice * quantity * (1f - discount);
                bool useCash = melonPrefs.GetEntry<bool>("payWithCash").Value;
                bool useDebt = melonPrefs.GetEntry<bool>("useDebt").Value;
                SlotIdentifier slotID = SerializeSlot(slot);
                Utils.Debug($"Trying to reshelve {quantity}x {item.ID} at {slotID.property}, grid location ({slotID.gridLocation[0]},{slotID.gridLocation[1]}");
                
                try
                {
                    if ((item.StackLimit == 0))
                    {
                        Utils.Debug($"Stacklimit ({item.StackLimit}) == 0. Not restocking.");
                        item.RequestClearSlot();
                        return;
                    }

                    if (Manager.ItemIsRestockable(item.ID))
                    {
                        float balance = useCash ? moneyManager.cashBalance : moneyManager.onlineBalance;
                        if (balance < totalCost && !useDebt)
                        {
                            Utils.Log($"Can't afford to restock {quantity}x {itemID} (${totalCost}).");
                        }
                        else if (balance >= totalCost)
                        {
                            AcquireMutex();
                            Transaction transaction = new Transaction(itemID, quantity, discount, unitPrice, totalCost, useCash, slotID);
                            ledger.Add(transaction);
                            Utils.Debug($"Starting restock coroutine ({itemID} x{quantity} at {slotID.property}).");
                            coroutines[transaction] = MelonCoroutines.Start(RestockCoroutine(slot, item, lockOwner.NetworkObject, transaction));
                            ReleaseMutex();
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.PrintException(e);
                    ReleaseMutex();
                    item.RequestClearSlot();
                }
            }
            else
            {
                Utils.Log($"Tried to restock item, but Manager was not initialized!");
            }

        }

#if MONO_BUILD
        public static bool OwnedByNPC(ItemSlot slot)
        {
            if (isInitialized)
            {
                string slotOwnerName = ((UnityEngine.Object)slot.SlotOwner).name;
                if (slotOwnerName.Contains("Cauldron"))
                {
                    return ((Cauldron)slot.SlotOwner).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("MixingStation") || slotOwnerName.Contains("MixingStationMk2"))
                {
                    return ((MixingStation)slot.SlotOwner).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("PackagingStation") || slotOwnerName.Contains("PackagingStationMk2"))
                {
                    return ((PackagingStation)slot.SlotOwner).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("ChemistryStation"))
                {
                    return ((ChemistryStation)slot.SlotOwner).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("StorageEntity"))
                {
                    return (slot.IsLocked || slot.IsAddLocked || slot.IsRemovalLocked);
                }
                return false;
            }
            return false;
        }
#else
        public static bool OwnedByNPC(ItemSlot slot)
        {
            if (isInitialized)
            {
                string slotOwnerName = new UnityEngine.Object(slot.SlotOwner.Pointer).name;
                if (slotOwnerName.Contains("Cauldron"))
                {
                    return new Cauldron(slot.SlotOwner.Pointer).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("MixingStation") || slotOwnerName.Contains("MixingStationMk2"))
                {
                    return new MixingStation(slot.SlotOwner.Pointer).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("PackagingStation") || slotOwnerName.Contains("PackagingStationMk2"))
                {
                    return new PackagingStation(slot.SlotOwner.Pointer).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("ChemistryStation"))
                {
                    return new ChemistryStation(slot.SlotOwner.Pointer).NPCUserObject != null;
                }
                else if (slotOwnerName.Contains("StorageEntity"))
                {
                    return (slot.IsLocked || slot.IsAddLocked || slot.IsRemovalLocked);
                }
                return false;
            }
            return false;
        }
#endif

        private static IEnumerator RestockCoroutine(ItemSlot slot, StorableItemInstance item, NetworkObject lockOwner, Transaction transaction)
        {
            slot.SetIsRemovalLocked(true);
            yield return new WaitForSeconds(1f);

            Utils.VerboseLog($"Restocking {item.Name} (${transaction.unitPrice}) x{transaction.quantity} at {transaction.slotID.property}. Total: ${transaction.totalCost}.");
            if (transaction.quantity > 0)
            {
                item.SetQuantity(transaction.quantity - slot.Quantity);
                slot.AddItem(item);
            }
            slot.SetIsRemovalLocked(false);

            if (transaction.totalCost <= 0f)
            {
                Utils.VerboseLog($"Total cost of transaction is $0. Get a freebie!");
            }
            else
            {
                if (melonPrefs.GetEntry<bool>("payWithCash").Value)
                {
                    moneyManager.ChangeCashBalance(-transaction.totalCost);
                }
                else
                {
                    moneyManager.CreateOnlineTransaction("Restock", -transaction.totalCost, transaction.quantity, $"{item.Definition.Name}");
                }
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
                NetworkSingleton<MessagingManager>.Instance.SendMessage(new Message(GetReceipt(), Message.ESenderType.Other, true, -1), true, "oscar_holland");
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
            return ledger.Aggregate<Transaction, float>(0f, (float accum, Transaction transaction) => accum + transaction.totalCost);
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
                float total = 0f;
                if (ledger.Count > 0)
                {
                    Dictionary<string, Dictionary<string, int>> itemTotals = new Dictionary<string, Dictionary<string, int>>();
                    Dictionary<string, float> itemPrices = new Dictionary<string, float>();

                    foreach (var transaction in ledger)
                    {
                        if (!itemTotals.ContainsKey(transaction.slotID.property))
                        {
                            itemTotals[transaction.slotID.property] = new Dictionary<string, int>();
                        }
                        if (!itemTotals[transaction.slotID.property].ContainsKey(transaction.itemID))
                        {
                            itemTotals[transaction.slotID.property][transaction.itemID] = 0;
                        }
                        itemTotals[transaction.slotID.property][transaction.itemID] += transaction.quantity;
                        itemPrices[transaction.itemID] = transaction.unitPrice;
                    }

                    foreach (string property in itemTotals.Keys)
                    {
                        float propertyTotal = 0f;
                        receipt += $"{property}: \n";
                        foreach (var entry in itemTotals[property])
                        {
                            string itemName = Utils.GetItemDef(entry.Key).Name;
                            receipt += $"  {itemName} x{entry.Value} = ${itemPrices[entry.Key] * (float)entry.Value}\n";
                            propertyTotal += itemPrices[entry.Key] * (float)entry.Value;
                        }
                        receipt += "=====================\n";
                        receipt += $"  Property total: ${propertyTotal}\n\n";
                        total += propertyTotal;
                    }
                }
                receipt += "=====================\n";
                receipt += $"  Grand total: ${total}\n\n";
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
    public class PersistencePatches: Sched1PatchesBase
    {
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
            if (InstanceFinder.IsServer && !Manager.isInitialized)
            {
                Manager.Initialize();
            }
        }



        [HarmonyPatch(typeof(LoadManager), "ExitToMenu")]
        [HarmonyPrefix]
        public static void ExitToMenuPrefix(LoadManager __instance)
        {
            if (InstanceFinder.IsServer && Manager.isInitialized)
            {
                Manager.Stop();
            }
        }
    }

    [HarmonyPatch]
    public class CauldronPatches : Sched1PatchesBase
    {
        [HarmonyPatch(typeof(Cauldron), "RemoveIngredients")]
        [HarmonyPrefix]
        public static bool RemoveIngredientsPrefix(Cauldron __instance)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
            {
                
                return true;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableCauldrons").Value)
                {
                    return true;
                }
                if (__instance.LiquidSlot.ItemInstance.Quantity > 1)
                {
                    return true;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value ||
                    __instance.PlayerUserObject == null)
                {
                    StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(__instance.LiquidSlot.ItemInstance.GetCopy(1));
                    Manager.TryRestocking(__instance.LiquidSlot, newItem, newItem.StackLimit);
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
    public class MixingStationPatches : Sched1PatchesBase
    {
        [HarmonyPatch(typeof(MixingStation), "SendMixingOperation")]
        [HarmonyPrefix]
        public static bool SendMixingOperationPrefix(MixingStation __instance, MixOperation operation)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
            {
                return true;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableMixingStations").Value)
                {
                    return true;
                }
                if ((__instance.MixerSlot.Quantity - operation.Quantity) >= ((MixingStationConfiguration)Utils.GetProperty(typeof(MixingStation), "stationConfiguration", __instance)).StartThrehold.GetData().Value)
                {
                    return true;
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
            return true;
        }
    }

    [HarmonyPatch]
    public class PackagingStationPatches : Sched1PatchesBase
    {
        [HarmonyPatch(typeof(PackagingStation), "PackSingleInstance")]
        [HarmonyPrefix]
        public static bool PackSingleInstancePrefix(PackagingStation __instance)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
            {
                return true;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enablePackagingStations").Value)
                {
                    return true;
                }
                if (__instance.PackagingSlot.ItemInstance == null || __instance.PackagingSlot.ItemInstance.Quantity > 1)
                {
                    return true;
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
            return true;
        }

    }

    [HarmonyPatch]
    public class ChemistryStationPatches : Sched1PatchesBase
    {
        static List<List<T>> Permute<T>(List<T> nums)
        {
            var list = new List<List<T>>();
            return DoPermute<T>(nums, 0, nums.Count - 1, list);
        }

        static List<List<T>> DoPermute<T>(List<T> nums, int start, int end, List<List<T>> list)
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

        static void Swap<T>(List<T> list, int index1, int index2)
        {
            T item = list[index1];
            list[index1] = list[index2];
            list[index2] = item;
        }

        [HarmonyPatch(typeof(ChemistryStation), "SendCookOperation")]
        [HarmonyPrefix]
        public static bool SendCookOperationPrefix(ChemistryStation __instance, ChemistryCookOperation op)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
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
                    // this should really be done with a map operation, but il2cpp ienumerable methods only accept intptrs
                    // oh well. time to iterate
                    List<ItemDefinition> missingIngredients = new List<ItemDefinition>();
                    foreach (StationRecipe.IngredientQuantity i in op.Recipe.Ingredients)
                    {
                        bool isPresent = false;
                        foreach (ItemSlot slot in __instance.IngredientSlots)
                        {
                            if (slot.ItemInstance != null && slot.ItemInstance.Definition.Name.Contains(i.Item.Name))
                            {
                                isPresent = true;
                                break;
                            }
                        }
                        if (!isPresent)
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
                        // With three slots, we only have 3->6 possibilities max, so just permute
                        possibleMappings = Permute<ItemDefinition>(missingIngredients);

                        // now find valid mappings
                        validMappings = possibleMappings.Select<List<ItemDefinition>, List<ItemDefinition>>((List<ItemDefinition> mapping) =>
                        {
                            // is this mapping valid
                            bool isValid = true;
                            for (int i = 0; i < mapping.Count; i++)
                            {
                                ItemInstance item;
                                if (Utils.IsQualityIngredient(mapping[i].ID))
                                {
                                    item = Utils.GetItemInstance(mapping[i].ID, op.ProductQuality);
                                }
                                else
                                {
                                    item = Utils.GetItemInstance(mapping[i].ID);
                                }

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
                            Utils.Log($"Couldn't restock chemistry station because items do not agree with filters.");
                        }
                    }

                    int emptySlotsFilled = 0;
                    foreach (ItemSlot slot in __instance.IngredientSlots)
                    {
                        if (slot.ItemInstance == null && missingIngredients.Count > 0)
                        {
                            ItemDefinition newDef = validMappings[0][emptySlotsFilled];
                            if (Utils.IsQualityIngredient(newDef.ID))
                            {
                                QualityItemInstance newItem = Utils.GetItemInstance(newDef.ID, op.ProductQuality);
                                Manager.TryRestocking(slot, newItem, newItem.StackLimit);
                            }
                            else
                            {
                                StorableItemInstance newItem = Utils.GetItemInstance(newDef.ID);
                                Manager.TryRestocking(slot, newItem, newItem.StackLimit);
                            }
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
    public class StorageEntityPatches : Sched1PatchesBase
    {
        [HarmonyPatch(typeof(MoveItemBehaviour), "TakeItem")]
        [HarmonyPrefix]
        public static void TakeItemPrefix(MoveItemBehaviour __instance)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
            {
                return;
            }

            try
            {
                if ((int)Utils.CallMethod(typeof(MoveItemBehaviour), "GetAmountToGrab", __instance, []) > 0)
                {
                    TransitRoute route = Utils.CastTo<TransitRoute>(Utils.GetField(typeof(MoveItemBehaviour), "assignedRoute", __instance));
                    ItemInstance template = Utils.CastTo<ItemInstance>(Utils.GetField(typeof(MoveItemBehaviour), "itemToRetrieveTemplate", __instance));
                    ItemSlot slot = route.Source.GetFirstSlotContainingTemplateItem(template, ITransitEntity.ESlotType.Both);
                    if (slot.SlotOwner != null && Utils.IsObjectStorageRack(slot.SlotOwner))
                    {
                        if (!Manager.shelfAccessors.TryAdd(slot, __instance.Npc))
                        {
                            Utils.Warn($"ItemSlot is already in list of shelfAccessors?");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        [HarmonyPatch(typeof(MoveItemBehaviour), "TakeItem")]
        [HarmonyPostfix]
        public static void TakeItemPostfix(MoveItemBehaviour __instance)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized) 
            {
                return;
            }

            try
            {
                TransitRoute route = Utils.CastTo<TransitRoute>(Utils.GetField(typeof(MoveItemBehaviour), "assignedRoute", __instance));
                ItemInstance template = Utils.CastTo<ItemInstance>(Utils.GetField(typeof(MoveItemBehaviour), "itemToRetrieveTemplate", __instance));
                ItemSlot slot = route.Source.GetFirstSlotContainingTemplateItem(template, ITransitEntity.ESlotType.Both);
                if (slot != null)
                {
                    Manager.shelfAccessors.Remove(slot);
                }
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        [HarmonyPatch(typeof(StorageEntity), "SetStoredInstance")]
        [HarmonyPrefix]
        public static void SetStoredInstancePrefix(StorageEntity __instance, int itemSlotIndex, ItemInstance instance)
        {
            if (!InstanceFinder.IsServer || !Manager.isInitialized)
            {
                return;
            }

            try
            {
                if (!Manager.melonPrefs.GetEntry<bool>("enableStorage").Value)
                {
                    return;
                }
                if (instance != null || !IsStorageRack(__instance))
                {
                    return;
                }
                ItemSlot slot = __instance.ItemSlots.ToArray()[itemSlotIndex];
                if (slot.ItemInstance == null)
                {
                    return;
                }
                // is a player looking into the shelf?
                if (__instance.CurrentAccessor != null)
                {
                    // is the current slot being accessed by an NPC?
                    if (!Manager.shelfAccessors.ContainsKey(slot))
                    {
                        return;
                    }
                }
                StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(slot.ItemInstance.GetCopy(1));
                Manager.TryRestocking(slot, newItem, newItem.StackLimit);
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
        }

        private static bool IsStorageRack(StorageEntity item)
        {
            if (item.StorageEntityName == "Safe" ||
                item.StorageEntityName == "Large Storage Rack" ||
                item.StorageEntityName == "Medium Storage Rack" ||
                item.StorageEntityName == "Small Storage Rack")
            {
                return true;
            }
            return false;
        }
    }
}
