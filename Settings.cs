using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace NPC_PCU_Fixer2
{
    public class Settings : ViewModel
    {

        private bool _Enabled;
        public bool PluginEnabled { get => _Enabled; set => SetValue(ref _Enabled, value); }


        private bool _Debug;
        public bool DebugEnabled { get => _Debug; set => SetValue(ref _Debug, value); }


        private bool _TransferOwnership;
        public bool TransferOwnership { get => _TransferOwnership; set => SetValue(ref _TransferOwnership, value); }


        private string _TickCounter;
        public string TickCounter { get => _TickCounter; set => SetValue(ref _TickCounter, value); }

        
        private int _BlockThreshold = 3;
        public int BlockThreshold { get => _BlockThreshold; set => SetValue(ref _BlockThreshold, value); }

        /*
         * Something to keep tracked of impending deleted grids if server restarts?
        private List<Tracker> _DeleteList;
        public List<Tracker> DeleteList { get => _DeleteList; set => SetValue(ref _DeleteList, value); }
        //Add settings and configs
        */
    }
}
