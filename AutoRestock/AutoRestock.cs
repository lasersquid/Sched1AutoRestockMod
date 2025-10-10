using HarmonyLib;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using Newtonsoft.Json;

#if MONO_BUILD
using FishNet;
using ScheduleOne.Messaging;
using ScheduleOne.GameTime;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.Storage;
using ScheduleOne.StationFramework;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.Management;
using ScheduleOne.Persistence;
#else
using Il2CppFishNet;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.StationFramework;
using Il2CppFishNet.Object;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Property;
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

        public static void RestoreDefaults()
        {
            throw new NotImplementedException();
        }
    }

    public static class Utils
    {
        public static AutoRestockMod Mod;
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
            return qualityIngredients.Contains(itemID);
        }

        public static StorableItemInstance GetItem(string itemID)
        {
#if MONO_BUILD
            return new ItemInstance(ScheduleOne.Registry.GetItem(itemID), 1);
#else
            return new StorableItemInstance(Il2CppScheduleOne.Registry.GetItem(itemID), 1);
#endif
        }

        public static QualityItemInstance GetItem(string itemID, EQuality quality)
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
                qualityItemID = itemID;
            }

#if MONO_BUILD
            return new QualityItemInstance(ScheduleOne.Registry.GetItem(qualityItemID), 1, quality);
#else
            return new QualityItemInstance(Il2CppScheduleOne.Registry.GetItem(qualityItemID), 1, quality);
#endif
        }
        public static bool IsObjectBuildableItem(string objName)
        {
            List<string> stations = ["PackagingStation", "PackagingStationMk2", "Cauldron", "ChemistryStation", "MixingStation", "MixingStationMk2"];
            return stations.Aggregate<string, bool>(false, (bool accum, string name) => accum || objName.Contains(name));

        }

        public static bool IsObjectBuildableItem(IntPtr pointer)
        {
            string objName = new UnityEngine.Object(pointer).name;
            return IsObjectBuildableItem(objName);
        }

        public static bool IsObjectStorageRack(IntPtr pointer)
        {
            string objName = new UnityEngine.Object(pointer).name;
            return IsObjectStorageRack(objName);
        }

        public static bool IsObjectStorageRack(string objName)
        {
            List<string> shelves = ["Safe", "Small Storage Rack", "Medium Storage Rack", "Large Storage Rack", "StorageRack"];
            return shelves.Aggregate<string, bool>(false, (bool accum, string name) => accum || objName.Contains(name));
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
            if (Utils.IsObjectBuildableItem(slot.SlotOwner.Pointer))
            {
                BuildableItem buildableItem = new BuildableItem(slot.SlotOwner.Pointer);
                gridItem = buildableItem.gameObject.GetComponent<GridItem>();
                type = buildableItem.name;

            }
            else if (Utils.IsObjectStorageRack(slot.SlotOwner.Pointer))
            {
                StorageEntity storageEntity = new StorageEntity(slot.SlotOwner.Pointer);
                gridItem = storageEntity.gameObject.GetComponent<PlaceableStorageEntity>();
                type = storageEntity.name;
            }
            else
            {
                Utils.Warn($"Couldn't serialize itemslot!");
                return null;
            }

            string property = gridItem.ParentProperty.name;

            return new SlotIdentifier(property, gridItem.OriginCoordinate, slot.SlotIndex, type);
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
                List<Property> properties = UnityEngine.Object.FindObjectsOfType<Property>().ToList<Property>();
                List<GridItem> gridItems = UnityEngine.Object.FindObjectsOfType<GridItem>().ToList<GridItem>();

                Property property = properties.FirstOrDefault<Property>((Property p) => p.name == identifier.property);
                List<GridItem> gridItemsOnProperty = gridItems.FindAll((GridItem g) => g.GetProperty() == property);
                GridItem gridItem = gridItemsOnProperty.Single<GridItem>((GridItem g) => g.OriginCoordinate == new Vector2(identifier.gridLocation[0], identifier.gridLocation[1]));

                Component slotOwner = gridItem.gameObject.GetComponentByName(StringToType(identifier.type));
                return new IItemSlotOwner(slotOwner.Pointer).ItemSlots[identifier.slotIndex];
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
                    StorableItemInstance item = Utils.GetItem(transaction.itemID);
                    TryReshelving(slot, item, transaction.quantity);
                }
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        public static void Initialize()
        {
            if (!InstanceFinder.IsServer)
            {
                return;
            }

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
            saveManager.onSaveStart.AddListener(new Action(Manager.OnSaveStart));

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
            List<Transaction> pendingTransactions = JsonConvert.DeserializeObject<List<Transaction>>(melonPrefs.GetEntry<string>(transactionString).Value);
            
            isInitialized = true;
            Utils.Log($"AutoRestock manager initialized.");

            if (pendingTransactions.Count > 0)
            {
                Utils.Log($"Completing {pendingTransactions.Count} pending transaction{(pendingTransactions.Count > 1 ? "s" : "")}.");
                CompleteTransactions(pendingTransactions);
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
            foreach (var entry in coroutines)
            {
                MelonCoroutines.Stop(entry.Value);
            }
            coroutines.Clear();
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

                ItemDefinition itemDef = Il2CppScheduleOne.Registry.GetItem(itemID);
                return ((whitelistCategories.Contains(itemDef.Category.ToString()) || whitelistItems.Contains(itemDef.ID)) &&
                    !blacklistItems.Contains(itemDef.ID));
            }
            return false;
        }


        public static void TryReshelving(ItemSlot slot, StorableItemInstance item, int quantity)
        {
            if (isInitialized)
            {
                string itemID = item.ID;
                float discount = Mathf.Clamp(melonPrefs.GetEntry<float>("itemDiscount").Value, 0f, 1f);
                float unitPrice = item.GetMonetaryValue() / (float)item.Quantity;
                float totalCost = unitPrice * quantity * (1f - discount);
                bool useCash = melonPrefs.GetEntry<bool>("payWithCash").Value;
                bool useDebt = melonPrefs.GetEntry<bool>("useDebt").Value;
                SlotIdentifier slotID = SerializeSlot(slot);
                
                try
                {
                    if ((!InstanceFinder.IsServer && OwnedByNPC(slot)) || (item.StackLimit == 0))
                    {
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
                            coroutines[transaction] = MelonCoroutines.Start(ReshelveCoroutine(slot, item, lockOwner.NetworkObject, transaction));
                            ReleaseMutex();
                        }
                    }
                }
                catch
                {
                    ReleaseMutex();
                    item.RequestClearSlot();
                }
            }
            else
            {
                Utils.Log($"Tried to restock item, but Manager was not initialized!");
            }
        }

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

        private static IEnumerator ReshelveCoroutine(ItemSlot slot, ItemInstance item, NetworkObject lockOwner, Transaction transaction)
        {
            slot.SetIsRemovalLocked(true);
            yield return new WaitForSeconds(2f);

            bool isVerbose = melonPrefs.GetEntry<bool>("verboseLogs").Value;
            if (isVerbose)
            {
                Utils.Log($"Restocking {item.Name} (${transaction.unitPrice}) x{transaction.quantity} at {transaction.slotID.property}. Total: ${transaction.totalCost}.");
            }
            if (transaction.quantity > 0)
            {
                item.SetQuantity(transaction.quantity - slot.Quantity);
                slot.InsertItem(item);
            }
            slot.SetIsRemovalLocked(false);

            if (transaction.totalCost <= 0f && isVerbose)
            {
                Utils.Log($"Total cost of transaction is $0. Get a freebie!");
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
            if (isInitialized)
            {
                if (InstanceFinder.IsServer)
                {
                    NetworkSingleton<MessagingManager>.Instance.SendMessage(new Message(GetReceipt(), Message.ESenderType.Other, true, -1), true, "oscar_holland");
                    ledger.Clear();
                    ledgerDay = timeManager.CurrentDay;
                }
            }
        }

        private static string GetSaveString()
        {
            string[] savePathStrings = LoadManager.Instance.LoadedGameFolderPath.Split('\\');
            return $"{savePathStrings[^2]}_{savePathStrings[^1]}";
        }

        public static void OnSaveStart()
        {
            if (Manager.isInitialized)
            {
                MelonPreferences_Category prefs = MelonPreferences.GetCategory("AutoRestock");
                string todaysLedger = $"{GetSaveString()}_ledger";
                if (prefs.HasEntry(todaysLedger))
                {
                    prefs.GetEntry<string>(todaysLedger).EditedValue = LedgerToJson();
                }
                else
                {
                    MelonPreferences_Entry entry = prefs.CreateEntry<string>(todaysLedger, "", "", true);
                    entry.BoxedEditedValue = LedgerToJson();
                }
                string transactionsInProgress = $"{GetSaveString()}_inprogress";
                if (prefs.HasEntry(transactionsInProgress))
                {
                    prefs.GetEntry<string>(transactionsInProgress).EditedValue = PendingTransactionsToJson();
                }
                else
                {
                    MelonPreferences_Entry entry = prefs.CreateEntry<string>(transactionsInProgress, "", "", true);
                    entry.BoxedEditedValue = PendingTransactionsToJson();
                }

                prefs.SaveToFile(false);
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
                            string itemName = Il2CppScheduleOne.Registry.GetItem(entry.Key).Name;
                            receipt += $"  {itemName} x{entry.Value} = ${itemPrices[entry.Key] * (float)entry.Value}\n";
                            propertyTotal += itemPrices[entry.Key] * (float)entry.Value;
                        }
                        receipt += "\n======================\n";
                        receipt += $"  Property total: ${propertyTotal}\n\n";
                        total += propertyTotal;
                    }
                }
                receipt += "\n======================\n";
                receipt += $"  Grand total: ${total}\n\n";
                receipt += $" Oscar says thank you for your business! :)";

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
            if (InstanceFinder.IsServer)
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
            if (!InstanceFinder.IsServer)
            {
                return true;
            }

            try
            {
                if (!Manager.isInitialized)
                {
                    return true;
                }
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
                    Manager.TryReshelving(__instance.LiquidSlot, newItem, newItem.StackLimit);
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
            if (!InstanceFinder.IsServer)
            {
                return true;
            }

            try
            {
                if (!Manager.isInitialized)
                {
                    return true;
                }
                if (!Manager.melonPrefs.GetEntry<bool>("enableMixingStations").Value)
                {
                    return true;
                }
                if (__instance.MixerSlot.ItemInstance != null)
                {
                    return true;
                }
                if (Manager.melonPrefs.GetEntry<bool>("playerRestockStations").Value || __instance.PlayerUserObject == null)
                {
                    StorableItemInstance newItem = new StorableItemInstance(Il2CppScheduleOne.Registry.GetItem(operation.IngredientID), 1);
                    Manager.TryReshelving(__instance.MixerSlot, newItem, newItem.StackLimit);
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
            if (!InstanceFinder.IsServer)
            {
                return true;
            }

            try
            {
                if (!Manager.isInitialized)
                {
                    return true;
                }
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
                    Manager.TryReshelving(__instance.PackagingSlot, newItem, newItem.StackLimit);
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
            if (!InstanceFinder.IsServer)
            {
                return true;
            }

            try
            {
                if (!Manager.isInitialized)
                {
                    return true;
                }
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
                                    item = Utils.GetItem(mapping[i].ID, op.ProductQuality);
                                }
                                else
                                {
                                    item = Utils.GetItem(mapping[i].ID);
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
                            StorableItemInstance newItem;
                            if (Utils.IsQualityIngredient(newDef.Name))
                            {
                                newItem = Utils.GetItem(newDef.ID, op.ProductQuality);
                                Manager.TryReshelving(slot, newItem, newItem.StackLimit);
                            }
                            else
                            {
                                newItem = Utils.GetItem(newDef.ID);
                                Manager.TryReshelving(slot, newItem, newItem.StackLimit);
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
                                Manager.TryReshelving(slot, newItem, newItem.StackLimit);
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
            if (!InstanceFinder.IsServer)
            {
                return;
            }

            try
            {
                if (!Manager.isInitialized)
                {
                    return;
                }
                if (__instance.GetAmountToGrab() > 0)
                {
                    ItemSlot slot = __instance.assignedRoute.Source.GetFirstSlotContainingTemplateItem(__instance.itemToRetrieveTemplate, ITransitEntity.ESlotType.Both);
                    if (slot.SlotOwner != null && Utils.IsObjectStorageRack(slot.SlotOwner.Pointer))
                    {
                        if (!Manager.shelfAccessors.TryAdd(slot, __instance.Npc))
                        {
                            Utils.Log($"ItemSlot {slot.SlotIndex} is already in list of shelfAccessors?");
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
            if (!InstanceFinder.IsServer)
            {
                return;
            }

            if (!Manager.isInitialized)
            {
                return;
            }

            ItemSlot slot = __instance.assignedRoute.Source.GetFirstSlotContainingTemplateItem(__instance.itemToRetrieveTemplate, ITransitEntity.ESlotType.Both);
            if (slot != null)
            {
                Manager.shelfAccessors.Remove(slot);
            }
        }

        [HarmonyPatch(typeof(StorageEntity), "SetStoredInstance")]
        [HarmonyPrefix]
        public static bool SetStoredInstancePrefix(StorageEntity __instance, int itemSlotIndex, ItemInstance instance)
        {
            if (!InstanceFinder.IsServer)
            {
                return true;
            }

            ItemSlot slot = null;
            try
            {
                if (!Manager.isInitialized)
                {
                    return true;
                }
                if (!Manager.melonPrefs.GetEntry<bool>("enableStorage").Value)
                {
                    return true;
                }
                if (instance != null || !IsStorageRack(__instance))
                {
                    return true;
                }
                slot = __instance.ItemSlots.ToArray()[itemSlotIndex];
                if (slot.ItemInstance == null)
                {
                    return true;
                }
                // is a player looking into the shelf?
                if (__instance.CurrentAccessor != null)
                {
                    // is the current slot being accessed by an NPC?
                    if (!Manager.shelfAccessors.ContainsKey(slot))
                    {
                        return true;
                    }
                }
                StorableItemInstance newItem = Utils.CastTo<StorableItemInstance>(slot.ItemInstance.GetCopy(1));
                Manager.TryReshelving(slot, newItem, newItem.StackLimit);
            }
            catch (Exception e)
            {
                Utils.Warn($"{MethodBase.GetCurrentMethod().DeclaringType.Name}:");
                Utils.PrintException(e);
            }
            return true;
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
