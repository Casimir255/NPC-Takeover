using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Groups;

namespace NPC_PCU_Fixer2
{

    public class Tracker
    {
        //Class for trackin entites!
        public long GridEntityID;
        public MyCubeGrid Grid;
        public long OwnerEntityID;
        public MyIdentity PlayerEntityID;
        public string PlayerName;
        public string GridDisplayName;
        public int PCU;
    }


    public class Utils
    {
        public static List<long> GetActiveNPCs(long PirateEntityID)
        {
            List<long> TrackedGrids = new List<long>();

            Parallel.ForEach(MyCubeGridGroups.Static.Mechanical.Groups, group =>
            {
                foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in group.Nodes)
                {
                    MyCubeGrid cubeGrid = groupNodes.NodeData;

                    if (cubeGrid == null || cubeGrid.Physics == null)
                        continue;


                    //Grab all blocks built by space pirates.
                    HashSet<MySlimBlock> Blocks = cubeGrid.FindBlocksBuiltByID(PirateEntityID);

                    if (Blocks.Count != 0)
                    {
                        //Blocks build by space pirates means the grid still has pcu of space pirate therefore must still be an npc grid.
                        if (cubeGrid.EntityId != null && !TrackedGrids.Contains(cubeGrid.EntityId)) //Check to make sure its not null and not in the list
                        {
                            //Alert logs and add it to the collection of tracked grids
                            //Log.Info("Grid: " + cubeGrid.DisplayName + " Is being tracked with EntityID of (" + cubeGrid.EntityId + ")");
                            TrackedGrids.Add(cubeGrid.EntityId);
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

            return TrackedGrids;
        }

        public static long GetActiveNPCSIds(string name)
        {
            long PirateIdentity = 0;
            Parallel.ForEach(MySession.Static.Players.GetAllIdentities(), identity =>
            {
                if (identity.DisplayName == name)
                    PirateIdentity = identity.IdentityId;

                //Simple test player identity
                //if (identity.DisplayName == "Bob Da Ross")
                //{
                //   Log.Info("Bob Da Ross ID (" + identity.IdentityId + ")");
                //}
                //var id = MySession.Static.Players.TryGetSteamId(identity.IdentityId)
            });
            //Log.Info("NPC " + name + " has entity ID of: " + PirateIdentity);
            return PirateIdentity;
        }
    }
}
