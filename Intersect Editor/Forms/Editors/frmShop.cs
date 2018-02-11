﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using Intersect.Editor.Classes;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.Localization;

namespace Intersect.Editor.Forms.Editors
{
    public partial class FrmShop : EditorForm
    {
        private List<ShopBase> mChanged = new List<ShopBase>();
        private byte[] mCopiedItem;
        private ShopBase mEditorItem;

        public FrmShop()
        {
            ApplyHooks();
            InitializeComponent();
            lstShops.LostFocus += itemList_FocusChanged;
            lstShops.GotFocus += itemList_FocusChanged;
        }

        protected override void GameObjectUpdatedDelegate(GameObjectType type)
        {
            if (type == GameObjectType.Shop)
            {
                InitEditor();
                if (mEditorItem != null && !ShopBase.Lookup.Values.Contains(mEditorItem))
                {
                    mEditorItem = null;
                    UpdateEditor();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            foreach (var item in mChanged)
            {
                item.RestoreBackup();
                item.DeleteBackup();
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Send Changed items
            foreach (var item in mChanged)
            {
                PacketSender.SendSaveObject(item);
                item.DeleteBackup();
            }

            Hide();
            Globals.CurrentEditor = -1;
            Dispose();
        }

        private void lstShops_Click(object sender, EventArgs e)
        {
            if (mChangingName) return;
            mEditorItem =
                ShopBase.Lookup.Get<ShopBase>(
                    Database.GameObjectIdFromList(GameObjectType.Shop, lstShops.SelectedIndex));
            UpdateEditor();
        }

        public void InitEditor()
        {
            lstShops.Items.Clear();
            lstShops.Items.AddRange(Database.GetGameObjectList(GameObjectType.Shop));
        }

        private void frmShop_Load(object sender, EventArgs e)
        {
            cmbAddBoughtItem.Items.Clear();
            cmbAddSoldItem.Items.Clear();
            cmbBuyFor.Items.Clear();
            cmbSellFor.Items.Clear();
            cmbDefaultCurrency.Items.Clear();
            foreach (var item in ItemBase.Lookup)
            {
                cmbAddBoughtItem.Items.Add(item.Value.Name);
                cmbAddSoldItem.Items.Add(item.Value.Name);
                cmbBuyFor.Items.Add(item.Value.Name);
                cmbSellFor.Items.Add(item.Value.Name);
                cmbDefaultCurrency.Items.Add(item.Value.Name);
            }
            if (cmbAddBoughtItem.Items.Count > 0) cmbAddBoughtItem.SelectedIndex = 0;
            if (cmbAddSoldItem.Items.Count > 0) cmbAddSoldItem.SelectedIndex = 0;
            if (cmbBuyFor.Items.Count > 0) cmbBuyFor.SelectedIndex = 0;
            if (cmbSellFor.Items.Count > 0) cmbSellFor.SelectedIndex = 0;
            InitLocalization();
            UpdateEditor();
        }

        private void InitLocalization()
        {
            Text = Strings.Get("shopeditor", "title");
            toolStripItemNew.Text = Strings.Get("shopeditor", "new");
            toolStripItemDelete.Text = Strings.Get("shopeditor", "delete");
            toolStripItemCopy.Text = Strings.Get("shopeditor", "copy");
            toolStripItemPaste.Text = Strings.Get("shopeditor", "paste");
            toolStripItemUndo.Text = Strings.Get("shopeditor", "undo");

            grpGeneral.Text = Strings.Get("shopeditor", "general");
            lblName.Text = Strings.Get("shopeditor", "name");
            lblDefaultCurrency.Text = Strings.Get("shopeditor", "defaultcurrency");

            grpItemsSold.Text = Strings.Get("shopeditor", "itemssold");
            lblAddSoldItem.Text = Strings.Get("shopeditor", "addlabel");
            lblSellFor.Text = Strings.Get("shopeditor", "sellfor");
            lblSellCost.Text = Strings.Get("shopeditor", "sellcost");
            btnAddSoldItem.Text = Strings.Get("shopeditor", "addsolditem");
            btnDelSoldItem.Text = Strings.Get("shopeditor", "removesolditem");

            grpItemsBought.Text = Strings.Get("shopeditor", "itemsboughtwhitelist");
            rdoBuyWhitelist.Text = Strings.Get("shopeditor", "whitelist");
            rdoBuyBlacklist.Text = Strings.Get("shopeditor", "blacklist");
            lblItemBought.Text = Strings.Get("shopeditor", "addboughtitem");
            lblBuyFor.Text = Strings.Get("shopeditor", "buyfor");
            lblBuyAmount.Text = Strings.Get("shopeditor", "buycost");
            btnAddBoughtItem.Text = Strings.Get("shopeditor", "addboughtitem");
            btnDelBoughtItem.Text = Strings.Get("shopeditor", "removeboughtitem");

            btnSave.Text = Strings.Get("shopeditor", "save");
            btnCancel.Text = Strings.Get("shopeditor", "cancel");
        }

        private void UpdateEditor()
        {
            if (mEditorItem != null)
            {
                pnlContainer.Show();

                txtName.Text = mEditorItem.Name;
                cmbDefaultCurrency.SelectedIndex = Database.GameObjectListIndex(GameObjectType.Item,
                    mEditorItem.DefaultCurrency);
                if (mEditorItem.BuyingWhitelist)
                {
                    rdoBuyWhitelist.Checked = true;
                }
                else
                {
                    rdoBuyBlacklist.Checked = true;
                }
                UpdateWhitelist();
                UpdateLists();
                if (mChanged.IndexOf(mEditorItem) == -1)
                {
                    mChanged.Add(mEditorItem);
                    mEditorItem.MakeBackup();
                }
            }
            else
            {
                pnlContainer.Hide();
            }
            UpdateToolStripItems();
        }

        private void UpdateWhitelist()
        {
            if (rdoBuyWhitelist.Checked)
            {
                cmbBuyFor.Enabled = true;
                nudBuyAmount.Enabled = true;
                grpItemsBought.Text = Strings.Get("shopeditor", "itemsboughtwhitelist");
            }
            else
            {
                cmbBuyFor.Enabled = false;
                nudBuyAmount.Enabled = false;
                grpItemsBought.Text = Strings.Get("shopeditor", "itemsboughtblacklist");
            }
        }

        private void rdoBuyWhitelist_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.BuyingWhitelist = rdoBuyWhitelist.Checked;
            UpdateLists();
            UpdateWhitelist();
        }

        private void rdoBuyBlacklist_CheckedChanged(object sender, EventArgs e)
        {
            mEditorItem.BuyingWhitelist = rdoBuyWhitelist.Checked;
            UpdateLists();
            UpdateWhitelist();
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            mChangingName = true;
            mEditorItem.Name = txtName.Text;
            lstShops.Items[ShopBase.Lookup.IndexKeys.ToList().IndexOf(mEditorItem.Index)] = txtName.Text;
            mChangingName = false;
        }

        private void UpdateLists()
        {
            lstSoldItems.Items.Clear();
            for (int i = 0; i < mEditorItem.SellingItems.Count; i++)
            {
                lstSoldItems.Items.Add("Sell Item #" + (mEditorItem.SellingItems[i].ItemNum + 1) + " " +
                                       ItemBase.GetName(mEditorItem.SellingItems[i].ItemNum) + " For (" +
                                       mEditorItem.SellingItems[i].CostItemVal + ") Item #" +
                                       (mEditorItem.SellingItems[i].CostItemNum + 1) + ". " +
                                       ItemBase.GetName(mEditorItem.SellingItems[i].CostItemNum));
            }
            lstBoughtItems.Items.Clear();
            if (mEditorItem.BuyingWhitelist)
            {
                for (int i = 0; i < mEditorItem.BuyingItems.Count; i++)
                {
                    lstBoughtItems.Items.Add("Buy Item #" + (mEditorItem.BuyingItems[i].ItemNum + 1) +
                                             " " +
                                             ItemBase.GetName(mEditorItem.BuyingItems[i].ItemNum) + " For (" +
                                             mEditorItem.BuyingItems[i].CostItemVal + ") Item #" +
                                             (mEditorItem.BuyingItems[i].CostItemNum + 1) + ". " +
                                             ItemBase.GetName(mEditorItem.BuyingItems[i].CostItemNum));
                }
            }
            else
            {
                for (int i = 0; i < mEditorItem.BuyingItems.Count; i++)
                {
                    lstBoughtItems.Items.Add("Don't Buy Item #" +
                                             (mEditorItem.BuyingItems[i].ItemNum + 1) + " " +
                                             ItemBase.GetName(mEditorItem.BuyingItems[i].ItemNum));
                }
            }
        }

        private void btnAddSoldItem_Click(object sender, EventArgs e)
        {
            bool addedItem = false;
            int cost = (int) nudSellCost.Value;
            ShopItem newItem = new ShopItem(ItemBase.Lookup.IndexKeys.ToList()[cmbAddSoldItem.SelectedIndex]
                , ItemBase.Lookup.IndexKeys.ToList()[cmbSellFor.SelectedIndex], cost);
            for (int i = 0; i < mEditorItem.SellingItems.Count; i++)
            {
                if (mEditorItem.SellingItems[i].ItemNum == newItem.ItemNum)
                {
                    mEditorItem.SellingItems[i] = newItem;
                    addedItem = true;
                    break;
                }
            }
            if (!addedItem) mEditorItem.SellingItems.Add(newItem);
            UpdateLists();
        }

        private void btnDelSoldItem_Click(object sender, EventArgs e)
        {
            if (lstSoldItems.SelectedIndex > -1)
            {
                mEditorItem.SellingItems.RemoveAt(lstSoldItems.SelectedIndex);
            }
            UpdateLists();
        }

        private void btnAddBoughtItem_Click(object sender, EventArgs e)
        {
            bool addedItem = false;
            int cost = (int) nudBuyAmount.Value;
            ShopItem newItem = new ShopItem(ItemBase.Lookup.IndexKeys.ToList()[cmbAddBoughtItem.SelectedIndex],
                ItemBase.Lookup.IndexKeys.ToList()[cmbBuyFor.SelectedIndex], cost);
            for (int i = 0; i < mEditorItem.BuyingItems.Count; i++)
            {
                if (mEditorItem.BuyingItems[i].ItemNum == newItem.ItemNum)
                {
                    mEditorItem.BuyingItems[i] = newItem;
                    addedItem = true;
                    break;
                }
            }
            if (!addedItem) mEditorItem.BuyingItems.Add(newItem);
            UpdateLists();
        }

        private void btnDelBoughtItem_Click(object sender, EventArgs e)
        {
            if (lstBoughtItems.SelectedIndex > -1)
            {
                mEditorItem.BuyingItems.RemoveAt(lstBoughtItems.SelectedIndex);
            }
            UpdateLists();
        }

        private void cmbDefaultCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            mEditorItem.DefaultCurrency = Database.GameObjectIdFromList(GameObjectType.Item,
                cmbDefaultCurrency.SelectedIndex);
        }

        private void toolStripItemNew_Click(object sender, EventArgs e)
        {
            PacketSender.SendCreateObject(GameObjectType.Shop);
        }

        private void toolStripItemDelete_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstShops.Focused)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("shopeditor", "deleteprompt"),
                        Strings.Get("shopeditor", "deletetitle"), DarkDialogButton.YesNo, Properties.Resources.Icon) ==
                    DialogResult.Yes)
                {
                    PacketSender.SendDeleteObject(mEditorItem);
                }
            }
        }

        private void toolStripItemCopy_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && lstShops.Focused)
            {
                mCopiedItem = mEditorItem.BinaryData;
                toolStripItemPaste.Enabled = true;
            }
        }

        private void toolStripItemPaste_Click(object sender, EventArgs e)
        {
            if (mEditorItem != null && mCopiedItem != null && lstShops.Focused)
            {
                mEditorItem.Load(mCopiedItem);
                UpdateEditor();
            }
        }

        private void toolStripItemUndo_Click(object sender, EventArgs e)
        {
            if (mChanged.Contains(mEditorItem) && mEditorItem != null)
            {
                if (DarkMessageBox.ShowWarning(Strings.Get("shopeditor", "undoprompt"),
                        Strings.Get("shopeditor", "undotitle"), DarkDialogButton.YesNo, Properties.Resources.Icon) ==
                    DialogResult.Yes)
                {
                    mEditorItem.RestoreBackup();
                    UpdateEditor();
                }
            }
        }

        private void itemList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Z)
                {
                    toolStripItemUndo_Click(null, null);
                }
                else if (e.KeyCode == Keys.V)
                {
                    toolStripItemPaste_Click(null, null);
                }
                else if (e.KeyCode == Keys.C)
                {
                    toolStripItemCopy_Click(null, null);
                }
            }
            else
            {
                if (e.KeyCode == Keys.Delete)
                {
                    toolStripItemDelete_Click(null, null);
                }
            }
        }

        private void UpdateToolStripItems()
        {
            toolStripItemCopy.Enabled = mEditorItem != null && lstShops.Focused;
            toolStripItemPaste.Enabled = mEditorItem != null && mCopiedItem != null && lstShops.Focused;
            toolStripItemDelete.Enabled = mEditorItem != null && lstShops.Focused;
            toolStripItemUndo.Enabled = mEditorItem != null && lstShops.Focused;
        }

        private void itemList_FocusChanged(object sender, EventArgs e)
        {
            UpdateToolStripItems();
        }

        private void form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.N)
                {
                    toolStripItemNew_Click(null, null);
                }
            }
        }
    }
}