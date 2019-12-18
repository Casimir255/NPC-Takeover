using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Managers;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Torch.API.Managers;
using NPC_PCU_Fixer2;


namespace NPC_PCU_Fixer2.Commands
{
    public class PluginCommands : CommandModule
    {
        //private Main Plugin { get; }
        
        private Main Plugin => (Main)Context.Plugin;

        [Command("checkcapture", "Checks current capture status of the grid")]
        [Permission(MyPromoteLevel.None)]

        public void CheckRemainderBlocks()
        {

            /*
             * How to get grid the player is looking at?
             * 
             * Compare grid to watchlist and check to make sure its an npc grid
             * 
             * List all control blocks captures out of total
             * 
             * 
             * 
             */

            Context.Respond("This command is under construction!");
        }


        [Command("watchlist", "Gives admin ability to check all grids in watch list")]
        [Permission(MyPromoteLevel.SpaceMaster)]

        public void Watchlist()
        {



            var sb = new StringBuilder();
            sb.AppendLine("");
            
            foreach (Tracker grid in Plugin.TrackedGrids)
            {
                sb.AppendLine(grid.GridDisplayName +"   (" + grid.GridEntityID+")");
            }

            Context.Respond("There are " + Plugin.TrackedGrids.Count + " grids in the watchlist! " + sb);

    }


        [Command("deletelist", "Gives admin ability to check all grids in delete list")]
        [Permission(MyPromoteLevel.SpaceMaster)]

        public void Deletelist()
        {

            var sb = new StringBuilder();
            sb.AppendLine("");

            foreach (Tracker grid in Plugin.DeleteList)
            {
                sb.AppendLine(grid.GridDisplayName + "Owner: "+grid.PlayerName+"   (" + grid.GridEntityID + ")");
            }

            Context.Respond("There are " + Plugin.DeleteList.Count + " grids in the deletelist! " + sb);


            /*
             * Output list to dialog or chat?
             * 
             * 
             * 
             * 
             */
        }

    }
}
