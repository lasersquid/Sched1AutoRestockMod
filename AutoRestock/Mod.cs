using MelonLoader;
using System.Reflection;

[assembly: MelonInfo(typeof(AutoRestock.AutoRestockMod), "AutoRestock", "1.0.8", "lasersquid", null)]
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
            Utils.Initialize(this);
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
            melonPrefs.CreateEntry<bool>("enableSpawnStations", true, "Enable auto-restock on mushroom spawn stations", "Enable auto-restock on mushroom spawn stations");
            melonPrefs.CreateEntry<bool>("enableStorage", true, "Enable auto-restock on storage (shelves and safes)", "Enable auto-restock on storage (shelves and safes)");
            melonPrefs.CreateEntry<bool>("playerRestockStations", true, "Enable auto-restock on stations after player-initiated actions (start cauldron, etc)", "Enable auto-restock on stations after player-initiated actions (start cauldron, etc)");
            melonPrefs.CreateEntry<bool>("verboseLogs", false, "Print to the log for each auto-restock transaction", "Print to the log for each auto-restock transaction");
            melonPrefs.CreateEntry<bool>("debugLogs", false, "Print debug logs", "Print debug logs");

            melonPrefs.SaveToFile(false);
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
//  - fix for 0.4.1f12 - done
//  - fix overcharging for online transactions - done (v1.0.2)
//  - calculate *buy* unit price instead of sell unit price - done (v1.0.3)
//  - fix gridcoordinate property access - done (v1.0.4)
//  - genericize utils methods - done
//  - add spawn station patches - done (v1.0.5)
//  - use getfield instead of getproperty for griditem._origincoordinate - done (v1.0.6)
//  - fix settings not being changeable at runtime - done
//  - clean up utils type conversion/checking functions - done
//  - fix bug where storageracks would sometimes not restock if a player was looking into them - done
//  - fix bug where dryingrack speed is disregarded - done
//  - investigate whether the ItemSlot.ChangeQuantity patch can replace station-specific patches - maybe
//  - fix bug where descendants of StorableItemInstance did not have their class-specific fields properly set - done
//  - properly lock itemslot while restocking - done
//  - check for insufficient balance at slot quantity change time and abort if necessary - done (v1.0.7)
//  - so the v1.0.7 upload doesn't seem to be working for people. rebuilding and reuploading after some minor cleanup - done (v1.0.8)
