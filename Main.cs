using System;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Collections;
using Torch.Managers;
using Torch.Mod;
using Torch.Mod.Messages;
using Torch.Session;
using Torch.Commands;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Definitions;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.ModAPI;
using VRageMath;
using VRageRender.Utils;
using NLog;
using VRage.Groups;
using Sandbox.Game.Entities.Cube;
using VRage.Network;
using VRage.Game.Entity;
using NPC_PCU_Fixer2.UI;
using NPC_PCU_Fixer2.Commands;
using System.Windows.Controls;


//Check PCU of ship and player
//If pcu is under player limit, transfer grid
//If over limit, Give warning and say grid will be deleted on log out/Server restart



namespace NPC_PCU_Fixer2
{
    //Needs a list/Textbox for npc players (Incase they have more than 1 npc players)
    //Needs Tick Interval timer box (How often the plugin will check the grid/scan) Most of the looping is done in parrell threads but still
    //Needs BlockType blox (Something for the program to decide what blocktypes to check ownership of on the grid) Ex. IMyControlBlocks will be default


    public class Main : TorchPluginBase, IWpfPlugin
    {
        //public Persistent<Settings> Settings { get; private set; }
        private static readonly Logger Log = LogManager.GetLogger("NPC-Takeover");
        public bool RunOnce = false;
        public long PirateEntityID = 0;

        public Settings Config => _config?.Data;
        private Persistent<Settings> _config;

        public List<Tracker> TrackedGrids = new List<Tracker>();
        public List<Tracker> DeleteList = new List<Tracker>();

        private UserControl _control;
        public UserControl GetControl() => _control ?? (_control = new UserControlInterface() {DataContext=Config});

        public int MaxTickCounter;
        public int PluginTickCounter;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            //Grab Settings
            string path = Path.Combine(StoragePath, "NPCTakeover.cfg");
            _config = Persistent<Settings>.Load(path);


            //Get Configs
            int TickCounter = 0;

            try
            {
                MaxTickCounter = Int32.Parse(Config.TickCounter);

            }
            catch (Exception e)
            {
                //Sometwad put in a string
                //Default to 500
                MaxTickCounter = 500;
                Config.TickCounter = "500";
            }

            
            //Load files

        }


        public override void Update()
        {

            //NEED TO PULL THIS INFO FROM WPF LIST
            string NPCName = "Space Pirates";

            //Run first checks!
            // Grap space pirate entities id (im assuming its all the same for each world?)
            if (!RunOnce)
            {
                PirateEntityID = GetActiveNPCSIds(NPCName);
                RunOnce = true;
            }


            if (PluginTickCounter >= MaxTickCounter && Config.PluginEnabled)
            {
                //Get active NPC Grids and store to wacthlist
                Parallel.ForEach(MyCubeGridGroups.Static.Mechanical.Groups, group =>
                {
                    foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in group.Nodes)
                    {
                        MyCubeGrid cubeGrid = groupNodes.NodeData;

                        Tracker TGrid = new Tracker();
                        TGrid.GridEntityID = cubeGrid.EntityId;
                        TGrid.GridDisplayName = cubeGrid.DisplayName;
                        TGrid.Grid = cubeGrid;
                        

                        if (cubeGrid == null || cubeGrid.Physics == null)
                            continue;

                        if (TrackedGrids.Any(item => item.GridEntityID == TGrid.GridEntityID)) //Checkwatchlist
                            continue;
                           



                       

                        //Grab all blocks built by space pirates.
                        HashSet<MySlimBlock> Blocks = cubeGrid.FindBlocksBuiltByID(PirateEntityID);
                        HashSet<MySlimBlock> NobodyBlocks = cubeGrid.FindBlocksBuiltByID(0); //Entity ID is ownedby Nobody

                        
                        //Combine these hash sets
                        Blocks.UnionWith(NobodyBlocks);



                        if (Blocks.Count != 0 && cubeGrid.BlocksCount >= Config.BlockThreshold) //Checks to see if the grid is greater than 3 blocks. Debris Limit
                        {
                            //Blocks build by space pirates means the grid still has pcu of space pirate therefore must still be an npc grid.
                            if (cubeGrid.EntityId != null) //Check to make sure its not null and not in the list
                            {
                                //Alert logs and add it to the collection of tracked grids
                                Log.Info("Grid: " + cubeGrid.DisplayName + " Is being tracked with EntityID of (" + cubeGrid.EntityId + ")");

                                TrackedGrids.Add(TGrid);
                            }

                        }

                        /*
                        Log.Info(cubeGrid.DisplayName + " Has large owners:");
                        foreach (long BigOwnerID in cubeGrid.BigOwners)
                        {
                            Log.Info(BigOwnerID);
                            //Log.Info(BigOwnerID);
                            if(BigOwnerID == PirateEntityID)
                            {

                                //Add this cubegrid to tracked entities
                                //Log.Info(cubeGrid.EntityId + "  :  " + TrackedGrids.Count);

                                if (cubeGrid.EntityId != null && !TrackedGrids.Contains(cubeGrid.EntityId))
                                {
                                    Log.Info("Grid: " + cubeGrid.DisplayName + " Is being tracked with EntityID of ("+cubeGrid.EntityId+")");
                                    TrackedGrids.Add(cubeGrid.EntityId);
                                }
                            }
                        }
                        

                        Log.Info(cubeGrid.DisplayName + " Has small owners:");
                        foreach (long SmallOwnerID in cubeGrid.SmallOwners)
                        {
                            Log.Info(SmallOwnerID);
                            //cubeGrid.owner
                        }
                        */
                    }
                });
                PluginTickCounter = 0;

                //Check owner/grid changes to see if player captured npc grids
                Parallel.ForEach(TrackedGrids, grid =>
                {

                    IMyCubeGrid cubeGrid = null;

                    try
                    {

                        foreach (IMyCubeGrid gridentity in MyEntities.GetEntities().OfType<IMyCubeGrid>().Where(g => g.EntityId == grid.GridEntityID))
                        {
                            //Get Cubegrid so we can transfer all blocks built by!
                            cubeGrid = gridentity;
                        }

                        if (cubeGrid == null)
                        {
                            Log.Info("Entity was removed from the game. Removing from watchlist!");
                            TrackedGrids.Remove(grid);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Entity cannot be converted to IMyCubeGrid! Removing entity from tracked list!");
                        TrackedGrids.Remove(grid);
                        Log.Warn(e);
                        return;
                    }



                    var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid);
                    List<Sandbox.ModAPI.Ingame.IMyShipController> blockList = new List<Sandbox.ModAPI.Ingame.IMyShipController>();
                    gts.GetBlocksOfType<Sandbox.ModAPI.Ingame.IMyShipController>(blockList);
                    //Message to alert players when they start taking the grid?
                    //Example: This grid requires your to take all control blocks


                    //Undoo this comment to debug tracked grids!
                    //Log.Info("Checking grid changes on: " + cubeGrid.DisplayName);

                    if (blockList.Count == 0)
                    {
                        //Log.Info("Grid doesnt have any control blocks!");
                        return;
                    }


                    bool AllControlersCaptured = true;
                    long PlayerWhoCapturedNPC = 0;
                    foreach (IMyShipController controler in blockList)
                    {
                        if (controler.OwnerId == PirateEntityID)
                        {
                            AllControlersCaptured = false;
                            continue;
                        }

                        //Basically grab last owner and transfer it all to him. (I dont really care about other faction mates right now. They can transfer between them)
                        PlayerWhoCapturedNPC = controler.OwnerId;
                    }


                    //Transfer all blocks to last block owner

                    if (AllControlersCaptured && PlayerWhoCapturedNPC != 0)
                    {
                        Log.Info("Player " + PlayerWhoCapturedNPC + " captured all the control blocks!");
                        MyCubeGrid Cubegrid = grid.Grid;




                        if (Cubegrid != null)
                        {
                            Log.Info("Transfering " + Cubegrid.DisplayName + " to new player!");


                            MyIdentity NewPlayer = MySession.Static.Players.TryGetIdentity(PlayerWhoCapturedNPC);
                            MyBlockLimits blockLimits = NewPlayer.BlockLimits;

                            ulong UserSteamID = MyModAPIHelper.MyMultiplayer.Static.Players.TryGetSteamId(PlayerWhoCapturedNPC);


                            
                            //Transfer Blocks!
                            List<long> authors = new List<long>();
                            foreach(MySlimBlock block in Cubegrid.GetBlocks())
                            {
                                if(block.BuiltBy == 0 || block.BuiltBy == PirateEntityID)
                                {
                                    block.TransferAuthorshipClient(PlayerWhoCapturedNPC);
                                    block.AddAuthorship();

                                    if (!authors.Contains(block.BuiltBy))
                                    {
                                        authors.Add(block.BuiltBy);
                                    }
                                    
                                }
                            }

                            foreach(long author in authors)
                                MyMultiplayer.RaiseEvent(Cubegrid, x => new Action<long, long>(x.TransferBlocksBuiltByID), author, PlayerWhoCapturedNPC, new EndpointId());
                                
                            

                            
                            int CurrentPcu = blockLimits.PCUBuilt;
                            int MaxPcu = blockLimits.PCU + CurrentPcu;



                            var sb = new StringBuilder();
                            sb.AppendLine("");
                            sb.AppendLine("Current PCU: " + CurrentPcu + "/" + MaxPcu);

                            if (CurrentPcu / MaxPcu >= 1.0)
                            {
                                /*
                                 * If they dont have room, Add list to secondary watch list on removing grid
                                 * This watch list will be saved to file so if server crashes we can still track grid and remove it when it re-starts
                                 * 
                                 * 
                                 * 
                                 * 
                                 * 
                                 */

                                sb.AppendLine("Your are over your PCU Limits! This grid will be deleted on player log-off or server restart!");
                                sb.AppendLine("If you manage to get under your PCU limits before these times, the grid wont be deleted.");


                                Tracker TGrid = new Tracker();
                                TGrid.GridEntityID = grid.GridEntityID;
                                TGrid.PlayerEntityID = NewPlayer;
                                TGrid.GridDisplayName = Cubegrid.DisplayName;
                                TGrid.PlayerName = NewPlayer.DisplayName;
                                TGrid.Grid = Cubegrid;



                                DeleteList.Add(TGrid);

                                Debug("Ship was added to delete watcher");

                            }

                            //Log.Info(NewPlayer.DisplayName);

                            //Will this work with entity id?
                            ModCommunication.SendMessageTo(new DialogMessage("NPC-Takeover", $"All control blocks captured!", "Transfering authorship on " + Cubegrid.DisplayName + " to you! " + sb), UserSteamID);

                            //ModCommunication.SendMessageTo(new TorchChatMessage("NPC-Takeover", $"All control blocks captured!", "Transfering authorship on " + Cubegrid.DisplayName + " to you!"), UserSteamID);

                            //MyAPIGateway.Utilities.SendMessage(76561198045096439, "PrivateTxt", "Target is Space Pirate");

                            //Cubegrid.TransferBlocksBuiltByID(PirateEntityID, PlayerWhoCapturedNPC);
                            //MyCubeGridGroups.Static.UpdateDynamicState(Cubegrid);
                            Debug("Removed grid from watchlist");
                            TrackedGrids.Remove(grid);
                            return;

                        }
                    }




                });
                

                //Check deleted grids list if player is offline or under pcu limit
                Parallel.ForEach(DeleteList, grid =>
                {
                    IMyCubeGrid cubeGrid = null;

                    try
                    {

                        foreach (IMyCubeGrid gridentity in MyEntities.GetEntities().OfType<IMyCubeGrid>().Where(g => g.EntityId == grid.GridEntityID))
                        {
                            //Get Cubegrid so we can transfer all blocks built by!
                            cubeGrid = gridentity;
                        }

                        if (cubeGrid == null)
                        {
                            Log.Info("Entity was removed from the game. Removing from watchlist!");
                            DeleteList.Remove(grid);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Entity cannot be converted to IMyCubeGrid! Removing entity from tracked list!");
                        DeleteList.Remove(grid);
                        Log.Warn(e);
                        return;
                    }


                    //Get player offline
                    bool PlayerIsOffline = true;
                    foreach (MyPlayer Player in MySession.Static.Players.GetOnlinePlayers())
                    {
                        //Log.Info("Player " + Player.DisplayName + " is online!");
                        if (Player.Identity.IdentityId == grid.PlayerEntityID.IdentityId)
                        {
                            //Player is online
                            PlayerIsOffline = false;
                        }
                    }

                    if (PlayerIsOffline)
                    {
                        //Debuggin if players report their grid got deleted
                        Log.Info("Grid owner: " +grid.PlayerName+" is offline. Deleting grid "+grid.GridDisplayName+" ("+grid.GridEntityID+")");
                        //Remove entitiy from game and delete from list
                        MyEntities.Close(cubeGrid as MyEntity);
                        DeleteList.Remove(grid);
                    }


                    bool GridFitsWithinPlayerlimits = false;
                    try
                    {
                        MyIdentity NewPlayer = MySession.Static.Players.TryGetIdentity(grid.PlayerEntityID.IdentityId);
                        MyBlockLimits blockLimits = NewPlayer.BlockLimits;

                        int CurrentPcu = blockLimits.PCUBuilt;
                        int MaxPcu = blockLimits.PCU + CurrentPcu;

                        //Get Current PCU of grid?
                        int GridPCU = grid.Grid.BlocksPCU;

                        if(MaxPcu - CurrentPcu >= GridPCU)
                        {
                            //Grid fits withing player PCU! Remove from watchlist!
                            Log.Info("Grid " + grid.GridDisplayName + " has been removed from watchlist. Player cleared PCU");
                            //Maybe alert player?
                            DeleteList.Remove(grid);
                        }
                    }
                    catch(Exception e)
                    {
                        //Failure to get player entitiy or Cubegrid class
                        Log.Warn(e);
                    }
                });

                //SaveList to file
            }

            PluginTickCounter++;

        }


        public long GetActiveNPCSIds(string name)
        {
            long PirateIdentity = 0;
            Parallel.ForEach(MySession.Static.Players.GetAllIdentities(), identity =>
            {
                if (identity.DisplayName == name)
                    PirateIdentity = identity.IdentityId;

            });
            Log.Info("NPC " + name + " has entity ID of: " + PirateIdentity);
            return PirateIdentity;
        }

        public void Debug(string message)
        {
            if(Config.DebugEnabled == true)
            {
                Log.Info(message);
            }
        }
    }

}
