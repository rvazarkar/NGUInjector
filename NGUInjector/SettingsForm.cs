﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NGUInjector
{
    public partial class SettingsForm : Form
    {
        internal static readonly Dictionary<int, string> TitanList = new Dictionary<int, string>();
        internal static readonly Dictionary<int, string> ZoneList = new Dictionary<int, string>();
        internal static readonly Dictionary<int, string> CombatModeList = new Dictionary<int, string>();
        private bool _initializing = true;
        public SettingsForm()
        {
            InitializeComponent();

            // Populate our data sources
            TitanList.Add(0, "None");
            TitanList.Add(1, "GRB");
            TitanList.Add(2, "GCT");
            TitanList.Add(3, "Jake");
            TitanList.Add(4, "UUG");
            TitanList.Add(5, "Walderp");
            TitanList.Add(6, "Beast");
            TitanList.Add(7, "Greasy Nerd");
            TitanList.Add(8, "Godmother");
            TitanList.Add(9, "Exile");
            TitanList.Add(10, "IT HUNGERS");
            TitanList.Add(11, "Rock Lobster");
            TitanList.Add(12, "Amalgamate");

            ZoneList.Add(0, "Safe Zone: Awakening Site");
            ZoneList.Add(1, "Tutorial Zone");
            ZoneList.Add(2, "Sewers");
            ZoneList.Add(3, "Forest");
            ZoneList.Add(4, "Cave of Many Things");
            ZoneList.Add(5, "The Sky");
            ZoneList.Add(6, "High Security Base");
            ZoneList.Add(7, "Gordon Ramsay Bolton");
            ZoneList.Add(8, "Clock Dimension");
            ZoneList.Add(9, "Grand Corrupted Tree");
            ZoneList.Add(10, "The 2D Universe");
            ZoneList.Add(11, "Ancient Battlefield");
            ZoneList.Add(12, "Jake From Accounting");
            ZoneList.Add(13, "A Very Strange Place");
            ZoneList.Add(14, "Mega Lands");
            ZoneList.Add(15, "UUG THE UNMENTIONABLE");
            ZoneList.Add(16, "The Beardverse");
            ZoneList.Add(17, "WALDERP");
            ZoneList.Add(18, "Badly Drawn World");
            ZoneList.Add(19, "Boring-Ass Earth");
            ZoneList.Add(20, "THE BEAST");
            ZoneList.Add(21, "Chocolate World");
            ZoneList.Add(22, "The Evilverse");
            ZoneList.Add(23, "Pretty Pink Princess Land");
            ZoneList.Add(24, "GREASY NERD");
            ZoneList.Add(25, "Meta Land");
            ZoneList.Add(26, "Interdimensional Party");
            ZoneList.Add(27, "THE GODMOTHER");
            ZoneList.Add(28, "Typo Zonw");
            ZoneList.Add(29, "The Fad-Lands");
            ZoneList.Add(30, "JRPGVille");
            ZoneList.Add(31, "THE EXILE");
            ZoneList.Add(32, "The Rad-lands");
            ZoneList.Add(33, "Back To School");
            ZoneList.Add(34, "The West World");
            ZoneList.Add(35, "IT HUNGERS");
            ZoneList.Add(36, "The Breadverse");
            ZoneList.Add(37, "That 70's Zone");
            ZoneList.Add(38, "The Halloweenies");
            ZoneList.Add(39, "ROCK LOBSTER");
            ZoneList.Add(40, "Construction Zone");
            ZoneList.Add(41, "DUCK DUCK ZONE");
            ZoneList.Add(42, "The Nether Regions");
            ZoneList.Add(43, "AMALGAMATE");
            ZoneList.Add(1000, "ITOPOD");

            CombatModeList.Add(0, "Manual");
            CombatModeList.Add(1, "Idle");

            HighestTitanDropdown.DataSource = new BindingSource(TitanList, null);
            HighestTitanDropdown.ValueMember = "Key";
            HighestTitanDropdown.DisplayMember = "Value";

            CombatMode.DataSource = new BindingSource(CombatModeList, null);
            CombatMode.ValueMember = "Key";
            CombatMode.DisplayMember = "Value";

            CombatTargetZone.DataSource = new BindingSource(ZoneList, null);
            CombatTargetZone.ValueMember = "Key";
            CombatTargetZone.DisplayMember = "Value";

            GoldLoadoutZone.DataSource = new BindingSource(ZoneList, null);
            GoldLoadoutZone.ValueMember = "Key";
            GoldLoadoutZone.DisplayMember = "Value";

            InitialGoldTarget.DataSource = new BindingSource(ZoneList, null);
            InitialGoldTarget.ValueMember = "Key";
            InitialGoldTarget.DisplayMember = "Value";

            yggdrasilLoadoutBox.Items.Clear();
            yggdrasilLoadoutBox.DataSource = new BindingSource(Main.Settings.YggdrasilLoadout, null);

            priorityBoostBox.Items.Clear();
            priorityBoostBox.DataSource = new BindingSource(Main.Settings.PriorityBoosts, null);

            blacklistBox.Items.Clear();
            blacklistBox.DataSource = new BindingSource(Main.Settings.BoostBlacklist, null);

            titanLoadout.Items.Clear();
            titanLoadout.DataSource = new BindingSource(Main.Settings.TitanLoadout, null);

            GoldLoadout.Items.Clear();
            GoldLoadout.DataSource = new BindingSource(Main.Settings.GoldDropLoadout, null);
            MasterEnable.Checked = Main.Active;

            blacklistLabel.Text = "";
            yggItemLabel.Text = "";
            priorityBoostLabel.Text = "";
            titanLabel.Text = "";

            prioUpButton.Text = char.ConvertFromUtf32(8593);
            prioDownButton.Text = char.ConvertFromUtf32(8595);

            //TestButton.Visible = false;
        }

        internal void UpdateFromSettings(SavedSettings newSettings)
        {
            _initializing = true;
            AutoDailySpin.Checked = newSettings.AutoSpin;
            AutoFightBosses.Checked = newSettings.AutoFight;
            AutoITOPOD.Checked = newSettings.AutoQuestITOPOD;
            AutoMoneyPit.Checked = newSettings.AutoMoneyPit;
            MoneyPitThreshold.Text = newSettings.MoneyPitThreshold.ToString(CultureInfo.InvariantCulture);
            ManageEnergy.Checked = newSettings.ManageEnergy;
            ManageMagic.Checked = newSettings.ManageMagic;
            ManageGear.Checked = newSettings.ManageGear;
            ManageDiggers.Checked = newSettings.ManageDiggers;
            ManageWandoos.Checked = newSettings.ManageWandoos;
            AutoRebirth.Checked = newSettings.AutoRebirth;
            ManageYggdrasil.Checked = newSettings.ManageYggdrasil;
            YggdrasilSwap.Checked = newSettings.SwapYggdrasilLoadouts;
            ManageInventory.Checked = newSettings.ManageInventory;
            ManageBoostConvert.Checked = newSettings.AutoConvertBoosts;
            SwapTitanLoadout.Checked = newSettings.SwapTitanLoadouts;
            HighestTitanDropdown.SelectedIndex = newSettings.HighestAKZone;
            BossesOnly.Checked = newSettings.SnipeBossOnly;
            PrecastBuffs.Checked = newSettings.PrecastBuffs;
            RecoverHealth.Checked = newSettings.RecoverHealth;
            FastCombat.Checked = newSettings.FastCombat;
            CombatMode.SelectedIndex = newSettings.CombatMode;
            CombatTargetZone.SelectedIndex = newSettings.SnipeZone;
            AllowFallthrough.Checked = newSettings.AllowZoneFallback;
            GoldLoadoutZone.SelectedIndex = newSettings.GoldZone;
            InitialGoldTarget.SelectedIndex = newSettings.InitialGoldZone;

            _initializing = false;
        }

        internal void UpdateProgressBar(int progress)
        {
            progressBar1.Value = progress;
        }

        internal void UpdateActive(bool active)
        {
            MasterEnable.Checked = active;
        }

        internal void UpdateITOPOD(bool active)
        {
            AutoITOPOD.Checked = active;
        }

        private void MasterEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Active = MasterEnable.Checked;
        }

        private void AutoDailySpin_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoSpin = AutoDailySpin.Checked;
        }

        private void AutoMoneyPit_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoMoneyPit = AutoMoneyPit.Checked;
        }

        private void AutoITOPOD_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoQuestITOPOD = AutoITOPOD.Checked;
        }

        private void AutoFightBosses_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoFight = AutoFightBosses.Checked;
        }

        private void MoneyPitThresholdSave_Click(object sender, EventArgs e)
        {
            var newVal = MoneyPitThreshold.Text;
            if (double.TryParse(newVal, out var saved))
            {
                if (saved < 0)
                {
                    moneyPitError.SetError(MoneyPitThreshold, "Not a valid value");
                    return;
                }
                Main.Settings.MoneyPitThreshold = saved;
            }
            else
            {
                moneyPitError.SetError(MoneyPitThreshold, "Not a valid value");
            }
        }

        private void MoneyPitThreshold_TextChanged_1(object sender, EventArgs e)
        {
            moneyPitError.SetError(MoneyPitThreshold, "");
        }

        private void ManageEnergy_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageEnergy = ManageEnergy.Checked;
        }

        private void ManageMagic_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageMagic = ManageMagic.Checked;
        }

        private void ManageGear_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageGear = ManageGear.Checked;
        }

        private void ManageDiggers_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageDiggers = ManageDiggers.Checked;
        }

        private void ManageWandoos_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageWandoos = ManageWandoos.Checked;
        }

        private void AutoRebirth_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoRebirth = AutoRebirth.Checked;
        }

        private void ManageYggdrasil_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageYggdrasil = ManageYggdrasil.Checked;
        }

        private void YggdrasilSwap_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.SwapYggdrasilLoadouts = YggdrasilSwap.Checked;
        }

        private void yggLoadoutItem_ValueChanged(object sender, EventArgs e)
        {
            yggErrorProvider.SetError(yggLoadoutItem, "");
            var val = decimal.ToInt32(yggLoadoutItem.Value);
            if (val < 40 || val > 505)
                return;
            var itemName = Main.Character.itemInfo.itemName[val];
            yggItemLabel.Text = itemName;
        }

        private void yggAddButton_Click(object sender, EventArgs e)
        {
            yggErrorProvider.SetError(yggLoadoutItem, "");
            var val = decimal.ToInt32(yggLoadoutItem.Value);
            if (val < 40 || val > 505)
            {
                yggErrorProvider.SetError(yggLoadoutItem, "Not a valid item id");
                return;
            }

            var temp = Main.Settings.YggdrasilLoadout.ToList();
            temp.Add(val);
            Main.Settings.YggdrasilLoadout = temp.ToArray();
            yggdrasilLoadoutBox.DataSource = null;
            yggdrasilLoadoutBox.DataSource = new BindingSource(Main.Settings.YggdrasilLoadout, null);
        }

        private void yggRemoveButton_Click(object sender, EventArgs e)
        {
            yggErrorProvider.SetError(yggLoadoutItem, "");
            var item= yggdrasilLoadoutBox.SelectedItem;
            if (item == null)
                return;

            var id = (int) item;

            var temp = Main.Settings.YggdrasilLoadout.ToList();
            temp.RemoveAll(x => x == id);
            Main.Settings.YggdrasilLoadout = temp.ToArray();
            yggdrasilLoadoutBox.DataSource = null;
            yggdrasilLoadoutBox.DataSource = new BindingSource(Main.Settings.YggdrasilLoadout, null);
        }

        private void ManageInventory_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.ManageInventory = ManageInventory.Checked;
        }

        private void ManageBoostConvert_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AutoConvertBoosts = ManageBoostConvert.Checked;
        }

        private void priorityBoostItemAdd_ValueChanged(object sender, EventArgs e)
        {
            invPrioErrorProvider.SetError(priorityBoostItemAdd, "");
            var val = decimal.ToInt32(priorityBoostItemAdd.Value);
            if (val < 40 || val > 505)
                return;
            var itemName = Main.Character.itemInfo.itemName[val];
            priorityBoostLabel.Text = itemName;
        }

        private void priorityBoostAdd_Click(object sender, EventArgs e)
        {
            invPrioErrorProvider.SetError(priorityBoostItemAdd, "");
            var val = decimal.ToInt32(priorityBoostItemAdd.Value);
            if (val < 40 || val > 505)
            {
                invPrioErrorProvider.SetError(priorityBoostItemAdd, "Not a valid item id");
                return;
            }

            var temp = Main.Settings.YggdrasilLoadout.ToList();
            temp.Add(val);
            Main.Settings.PriorityBoosts = temp.ToArray();
            priorityBoostBox.DataSource = null;
            priorityBoostBox.DataSource = new BindingSource(Main.Settings.PriorityBoosts, null);
        }

        private void priorityBoostRemove_Click(object sender, EventArgs e)
        {
            invPrioErrorProvider.SetError(priorityBoostItemAdd, "");
            var item = priorityBoostBox.SelectedItem;
            if (item == null)
                return;

            var id = (int)item;

            var temp = Main.Settings.PriorityBoosts.ToList();
            temp.RemoveAll(x => x == id);
            Main.Settings.PriorityBoosts = temp.ToArray();
            priorityBoostBox.DataSource = null;
            priorityBoostBox.DataSource = new BindingSource(Main.Settings.PriorityBoosts, null);
        }

        private void prioUpButton_Click(object sender, EventArgs e)
        {
            invPrioErrorProvider.SetError(priorityBoostItemAdd, "");
            var index = priorityBoostBox.SelectedIndex;
            if (index == -1 || index == 0)
                return;

            var temp = Main.Settings.PriorityBoosts.ToList();
            var item = temp[index];
            temp.RemoveAt(index);
            temp.Insert(index - 1, item);
            Main.Settings.PriorityBoosts = temp.ToArray();
            priorityBoostBox.DataSource = null;
            priorityBoostBox.DataSource = new BindingSource(Main.Settings.PriorityBoosts, null);
        }

        private void prioDownButton_Click(object sender, EventArgs e)
        {
            invPrioErrorProvider.SetError(priorityBoostItemAdd, "");
            var index = priorityBoostBox.SelectedIndex;
            if (index == -1)
                return;

            var temp = Main.Settings.PriorityBoosts.ToList();
            if (index == temp.Count - 1)
                return;

            var item = temp[index];
            temp.RemoveAt(index);
            temp.Insert(index + 1, item);
            Main.Settings.PriorityBoosts = temp.ToArray();
            priorityBoostBox.DataSource = null;
            priorityBoostBox.DataSource = new BindingSource(Main.Settings.PriorityBoosts, null);
        }

        private void blacklistAddItem_ValueChanged(object sender, EventArgs e)
        {
            invBlacklistErrProvider.SetError(blacklistAddItem, "");
            var val = decimal.ToInt32(blacklistAddItem.Value);
            if (val < 40 || val > 505)
                return;
            var itemName = Main.Character.itemInfo.itemName[val];
            blacklistLabel.Text = itemName;
        }

        private void blacklistAdd_Click(object sender, EventArgs e)
        {
            invBlacklistErrProvider.SetError(blacklistAddItem, "");
            var val = decimal.ToInt32(blacklistAddItem.Value);
            if (val < 40 || val > 505)
            {
                invBlacklistErrProvider.SetError(blacklistAddItem, "Not a valid item id");
                return;
            }

            var temp = Main.Settings.BoostBlacklist.ToList();
            temp.Add(val);
            Main.Settings.BoostBlacklist = temp.ToArray();
            blacklistBox.DataSource = null;
            blacklistBox.DataSource = new BindingSource(Main.Settings.BoostBlacklist, null);
        }

        private void blacklistRemove_Click(object sender, EventArgs e)
        {
            invBlacklistErrProvider.SetError(blacklistAddItem, "");
            var item = blacklistBox.SelectedItem;
            if (item == null)
                return;

            var id = (int)item;

            var temp = Main.Settings.BoostBlacklist.ToList();
            temp.RemoveAll(x => x == id);
            Main.Settings.BoostBlacklist = temp.ToArray();
            blacklistBox.DataSource = null;
            blacklistBox.DataSource = new BindingSource(Main.Settings.BoostBlacklist, null);
        }

        private void SwapTitanLoadout_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.SwapTitanLoadouts = SwapTitanLoadout.Checked;
        }

        private void HighestTitanDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            var selected = HighestTitanDropdown.SelectedIndex;
            Main.Settings.HighestAKZone = selected;
        }

        private void titanAddItem_ValueChanged(object sender, EventArgs e)
        {
            titanErrProvider.SetError(titanAddItem, "");
            var val = decimal.ToInt32(titanAddItem.Value);
            if (val < 40 || val > 505)
                return;
            var itemName = Main.Character.itemInfo.itemName[val];
            titanLabel.Text = itemName;
        }

        private void titanAdd_Click(object sender, EventArgs e)
        {
            titanErrProvider.SetError(titanAddItem, "");
            var val = decimal.ToInt32(titanAddItem.Value);
            if (val < 40 || val > 505)
            {
                invBlacklistErrProvider.SetError(titanAddItem, "Not a valid item id");
                return;
            }

            var temp = Main.Settings.TitanLoadout.ToList();
            temp.Add(val);
            Main.Settings.TitanLoadout = temp.ToArray();
            titanLoadout.DataSource = null;
            titanLoadout.DataSource = new BindingSource(Main.Settings.TitanLoadout, null);
        }

        private void titanRemove_Click(object sender, EventArgs e)
        {
            titanErrProvider.SetError(titanAddItem, "");
            var selectedItem = titanLoadout.SelectedItem;
            if (selectedItem == null)
                return;

            var temp = Main.Settings.TitanLoadout.ToList();
            temp.RemoveAll(x => x == (int) selectedItem);
            Main.Settings.TitanLoadout = temp.ToArray();
            titanLoadout.DataSource = null;
            titanLoadout.DataSource = new BindingSource(Main.Settings.TitanLoadout, null);
        }

        private void CombatActive_CheckedChanged(object sender, EventArgs e)
        {
            Main.SnipeActive = CombatActive.Checked;
        }

        private void BossesOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.SnipeBossOnly = BossesOnly.Checked;
        }

        private void PrecastBuffs_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.PrecastBuffs = PrecastBuffs.Checked;
        }

        private void RecoverHealth_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.RecoverHealth = RecoverHealth.Checked;
        }

        private void FastCombat_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.FastCombat = FastCombat.Checked;
        }

        private void CombatMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            var selected = CombatTargetZone.SelectedIndex;
            Main.Settings.CombatMode = selected;
        }

        private void CombatTargetZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            var selected = CombatTargetZone.SelectedIndex;
            Main.Settings.SnipeZone = selected;
        }

        private void AllowFallthrough_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.AllowZoneFallback = AllowFallthrough.Checked;
        }

        private void GoldLoadoutZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.GoldZone = GoldLoadoutZone.SelectedIndex;
        }

        private void UseGoldLoadout_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.NextGoldSwap = UseGoldLoadout.Checked;
        }

        private void InitialGoldTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_initializing) return;
            Main.Settings.InitialGoldZone = InitialGoldTarget.SelectedIndex;
        }

        private void GoldItemBox_ValueChanged(object sender, EventArgs e)
        {
            goldErrorProvider.SetError(GoldItemBox, "");
            var val = decimal.ToInt32(GoldItemBox.Value);
            if (val < 40 || val > 505)
                return;
            var itemName = Main.Character.itemInfo.itemName[val];
            GoldItemLabel.Text = itemName;
        }

        private void GoldLoadoutAdd_Click(object sender, EventArgs e)
        {
            goldErrorProvider.SetError(GoldItemBox, "");
            var val = decimal.ToInt32(GoldItemBox.Value);
            if (val < 40 || val > 505)
            {
                goldErrorProvider.SetError(GoldItemBox, "Invalid item id");
                return;
            }

            var temp = Main.Settings.GoldDropLoadout.ToList();
            temp.Add(val);
            Main.Settings.GoldDropLoadout = temp.ToArray();
            GoldLoadout.DataSource = null;
            GoldLoadout.DataSource = new BindingSource(Main.Settings.GoldDropLoadout, null);
        }

        private void GoldLoadoutRemove_Click(object sender, EventArgs e)
        {
            goldErrorProvider.SetError(GoldItemBox, "");
            var selected = GoldLoadout.SelectedItem;
            if (selected == null)
                return;

            var id = (int) selected;

            var temp = Main.Settings.GoldDropLoadout.ToList();
            temp.RemoveAll(x => x == id);
            Main.Settings.GoldDropLoadout = temp.ToArray();
            GoldLoadout.DataSource = null;
            GoldLoadout.DataSource = new BindingSource(Main.Settings.GoldDropLoadout, null);
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            var c = Main.Character;
            Main.Log($"Interactable: {c.buttons.brokenTimeMachine.interactable}\nBaseGold: {c.machine.realBaseGold}\n Zone:{Main.Settings.InitialGoldZone}\n Total: {c.adventureController.zoneDropdown.options.Count}");
        }
    }
}