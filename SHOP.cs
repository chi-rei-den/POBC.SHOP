using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using Terraria.Localization;

namespace Shop
{
    [ApiVersion(2, 1)]
    public class Shop : TerrariaPlugin
    {
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author => "欲情";

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description => "POBC 配套商店插件";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "POBC.SHOP";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 1);
        public string ConfigPath { get { return Path.Combine(TShock.SavePath, "POBC.Shop.json"); } }
        public ShopConfing Config = new ShopConfing();

        /// <summary>
        /// Initializes a new instance of the TestPlugin class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public Shop(Main game) : base(game)
        {

        }

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            File();
        }

        private void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("shop.user", Shopcom,"shop")
            {
                HelpText = " 使用POBC 货币购买或卖出物品"
            });

        }

        private void Shopcom(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("shop 命令如下\r\n/shop list -商店物品列表.\r\n /shop buy  -购买商品\r\n /sell -卖出物品");
                return;
            }
            switch (args.Parameters[0])
            {
                case "list":
                    {
                        args.Player.SendErrorMessage("商店出售的物品:");
                        int Length = Config.ShopC.Length;
                        if (Length < 5)
                        {
                            for (int i = 0; i < Length; i++)
                            {
                                args.Player.SendErrorMessage("ID：[" + Config.ShopC[i].id + "] " + "物品名 : [" + Config.ShopC[i].name + "]  购买需要: " + Config.ShopC[i].C + "  货币");
                            }
                            return;
                        }
                        int page;
                        if (Length % 5==0)
                        {
                            page = Length/5;
                        }
                        else
                        {
                            page = Length /5+ 1;
                        }
                        if (args.Parameters.Count>1)
                        {
                            if (!Int32.TryParse(args.Parameters[1], out int temp))
                            {
                                args.Player.SendErrorMessage("语法错误, 正确语法： /shop list 1（最大页数"+ page + ")");
                                return;
                            }
                            if (Int32.Parse(args.Parameters[1]) >= page)
                            {
                                for (int i = page * 5-5; i < Length; i++)
                                {
                                    args.Player.SendErrorMessage("ID：[" + Config.ShopC[i].id + "] " + "物品名 : [" + Config.ShopC[i].name + "]  购买需要: " + Config.ShopC[i].C + "  货币");
                                }
                                args.Player.SendErrorMessage("当前页数:"+ page + "最大页数:" + page);
                                return;
                            }
                            if (Int32.Parse(args.Parameters[1]) < page)
                            {
                                for (int i = Int32.Parse(args.Parameters[1])*5-5; i < Int32.Parse(args.Parameters[1]) * 5; i++)
                                {
                                    args.Player.SendErrorMessage("ID：[" + Config.ShopC[i].id + "] " + "物品名 : [" + Config.ShopC[i].name + "]  购买需要: " + Config.ShopC[i].C + "  货币");
                                }
                                args.Player.SendErrorMessage("当前页数 :" + args.Parameters[1] + "最大页数:" + page);
                                return;
                            }
                        }
                        else
                        {
                                for (int i = 0; i < 5; i++)
                                {
                                    args.Player.SendErrorMessage("ID：[" + Config.ShopC[i].id + "] " + "物品名 : [" + Config.ShopC[i].name + "]  购买需要: " + Config.ShopC[i].C + "  货币");
                                }
                            args.Player.SendErrorMessage("当前页数 : 1"+ "最大页数:" + page);
                        }
                    
                    }
                    break;
                case "buy":
                    {
                        if (args.Parameters.Count != 2)
                        {
                            args.Player.SendErrorMessage("语法错误, 正确语法： /shop buy [id]");
                            return;

                        }
                        if (Itemlist().Count<1)     // itemlist = shopc.select(i => i.id).tolist();  鱼鱼给的方法 后面再研究
                        {
                            args.Player.SendErrorMessage("配置文件无物品信息，请确认");
                            return;
                        }
                        if (!int.TryParse(args.Parameters[1], out int tmp))
                        {
                            args.Player.SendErrorMessage("语法错误, 正确语法： /shop buy [id]");
                            return;

                        }

                        if (Itemlist().Contains(int.Parse(args.Parameters[1])))
                        {
                            var x = Itemlist().IndexOf(int.Parse(args.Parameters[1]));
                            if (Config.ShopC[x].C > POBC2.Db.QueryCurrency(args.Player.Name))
                            {
                                args.Player.SendErrorMessage("您拥有的货币不够购买该物品，当前货币：" + POBC2.Db.QueryCurrency(args.Player.Name) + ", 需要货币：" + Config.ShopC[x].C);
                                return;
                            }
                            if (Stack(args.Player.Index)<0)
                            {
                                args.Player.SendErrorMessage("您背包中没有足够的空间！");
                                return;
                            }
                            PlayItemSet(args.Player.Index, Stack(args.Player.Index), Config.ShopC[x].itemid, 1);
                            POBC2.Db.DownC(args.Player.Name, Config.ShopC[x].C,"购买商城物品");
                            args.Player.SendErrorMessage("您已使用：" + Config.ShopC[x].C + "购买了物品：[ " + Config.ShopC[x].name + " ]");

                        }
                        else
                        {
                            args.Player.SendErrorMessage("商店中 暂无您输入的 ID ，请确认");
                            return;
                        }
                     
                    }
                    break;
                case "sell":
                    {

                        args.Player.SendErrorMessage("功能开发中！");
                    }
                    break;

                default:
                    args.Player.SendErrorMessage("请输入正确的参数！");
                    break;
            }

        }

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }
        public void File()
        {
            try
            {
                Config = ShopConfing.Read(ConfigPath).Write(ConfigPath);
            }
            catch (Exception ex)
            {
                Config = new ShopConfing();
                TShock.Log.ConsoleError("[POBC.Shop] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
            }


        }
        public List<int> Itemlist()  //获取商店物品list
        {
            List<int> itemlist = new List<int>();

            for (int i = 0; i < Config.ShopC.Length; i++)
            {

                itemlist.Add(Config.ShopC[i].id);

               
            }
            return itemlist;

        }

        // 给与玩家物品
        public void PlayItemSet(int ID, int slot, int Item, int stack)//ID 玩家ID，slot 格子ID，Item 物品ID，stack 物品堆叠
        {
            TSPlayer player = new TSPlayer(ID);
            int index;
            Item item = TShock.Utils.GetItemById(Item);
            item.stack = stack;
            //Inventory slots
            if (slot < NetItem.InventorySlots)
            {
                index = slot;
                player.TPlayer.inventory[slot] = item;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.inventory[index].Name), player.Index, slot, player.TPlayer.inventory[index].prefix);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.inventory[index].Name), player.Index, slot, player.TPlayer.inventory[index].prefix);
            }

            //Armor & Accessory slots
            else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots)
            {
                index = slot - NetItem.InventorySlots;
                player.TPlayer.armor[index] = item;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.armor[index].Name), player.Index, slot, player.TPlayer.armor[index].prefix);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.armor[index].Name), player.Index, slot, player.TPlayer.armor[index].prefix);
            }

            //Dye slots
            else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots)
            {
                index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots);
                player.TPlayer.dye[index] = item;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.dye[index].Name), player.Index, slot, player.TPlayer.dye[index].prefix);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.dye[index].Name), player.Index, slot, player.TPlayer.dye[index].prefix);
            }

            //Misc Equipment slots
            else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots)
            {
                index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots);
                player.TPlayer.miscEquips[index] = item;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.miscEquips[index].Name), player.Index, slot, player.TPlayer.miscEquips[index].prefix);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.miscEquips[index].Name), player.Index, slot, player.TPlayer.miscEquips[index].prefix);
            }

            //Misc Dyes slots
            else if (slot < NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots + NetItem.MiscDyeSlots)
            {
                index = slot - (NetItem.InventorySlots + NetItem.ArmorSlots + NetItem.DyeSlots + NetItem.MiscEquipSlots);
                player.TPlayer.miscDyes[index] = item;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(player.TPlayer.miscDyes[index].Name), player.Index, slot, player.TPlayer.miscDyes[index].prefix);
                NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(player.TPlayer.miscDyes[index].Name), player.Index, slot, player.TPlayer.miscDyes[index].prefix);
            }
        }

        public int Stack(int userid)
        {
            int x=-1;
            for (int i = 0; i < 50; i++)
            {
                if (TShock.Players[userid].TPlayer.inventory[i].netID ==0)
                {
                    x= i;
                    break;
                }
                else
                {
                    x = -1;
                }
            }
            return x;
        }


    }
}