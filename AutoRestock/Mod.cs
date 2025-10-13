using MelonLoader;
using System.Reflection;

[assembly: MelonInfo(typeof(AutoRestock.AutoRestockMod), "AutoRestock", "1.0.1", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace AutoRestock
{
    public class AutoRestockMod : MelonMod
    {
        public MelonPreferences_Category melonPrefs;
        public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.autorestock");

        public override void OnInitializeMelon()
        {
            CreateMelonPreferences();
            SetMod();
            LoggerInstance.Msg("Mod initialized.");
        }

        private void CreateMelonPreferences()
        {
            melonPrefs = MelonPreferences.CreateCategory("AutoRestock");
            melonPrefs.SetFilePath("UserData/AutoRestock.cfg", true, false);

            melonPrefs.CreateEntry<float>("itemDiscount", 0f, "Restock discount", "Discount applied to restock price (0.2 = 20% off)");
            melonPrefs.CreateEntry<bool>("payWithCash", true, "Pay for restock with cash", "True to pay with cash, false to pay with bank account");
            melonPrefs.CreateEntry<bool>("useDebt", false, "Enable debt", "Enable restocking even when zero or negative cash/bank balance");
            melonPrefs.CreateEntry<bool>("enableCauldrons", true, "Enable auto-restock on cauldrons", "Enable auto-restock on cauldrons");
            melonPrefs.CreateEntry<bool>("enableMixingStations", true, "Enable auto-restock on mixing stations", "Enable auto-restock on mixing stations");
            melonPrefs.CreateEntry<bool>("enableChemistryStations", true, "Enable auto-restock on chemistry stations", "Enable auto-restock on chemistry stations");
            melonPrefs.CreateEntry<bool>("enablePackagingStations", true, "Enable auto-restock on packaging stations", "Enable auto-restock on packaging stations");
            melonPrefs.CreateEntry<bool>("enableStorage", true, "Enable auto-restock on storage (shelves and safes)", "Enable auto-restock on storage (shelves and safes)");
            melonPrefs.CreateEntry<bool>("playerRestockStations", true, "Enable auto-restock on stations after player-initiated actions (start cauldron, etc)", "Enable auto-restock on stations after player-initiated actions (start cauldron, etc)");
            melonPrefs.CreateEntry<bool>("verboseLogs", false, "Print to the log for each auto-restock transaction", "Print to the log for each auto-restock transaction");
            melonPrefs.CreateEntry<bool>("debugLogs", false, "Print debug logs", "Print debug logs");

            melonPrefs.SaveToFile(false);
        }

        private List<Type> GetPatchTypes()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name.EndsWith("Patches"))
                .ToList<Type>();
        }

        private void SetMod()
        {
            foreach (var t in GetPatchTypes())
            {
                MethodInfo method = t.GetMethod("SetMod", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                method.Invoke(null, [this]);
            }
            Utils.SetMod(this);
        }
    }

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
}

// todo:
//  - register OnDayPass during Manager initialization - done
//  - investigate whether we can store the current day's ledger as hidden MelonPreference on save - done
//  - ensure quality items are restocked with items of the same quality - done
//  - add configurable check for negative player balance - done
//  - add "pay for cash items with cash" configuration entry - done
//  - text message with receipt isn't sending - done
//  - property detection isn't working - done
//  - receipt is mostly empty - done
//  - move utils to utils class - done
//  - rename project - done
//  - write readme - done
//  - any transactions mid-completion during save should be stored in MelonPreferences - done
//  - deserialize transactions and complete on load - done
//  - initialize manager after all objects in the scene are initialized - done
//  - convert for mono release - done
//  - don't initialize/silence warnings for non-host multiplayer - needs testing
//  - deserialize properly under mono - done
//  - restock quality items properly in mono - done
//  - make icon - done (v1.0.0)
//  - make mixing station refill when below start threshold - done (v1.0.1)