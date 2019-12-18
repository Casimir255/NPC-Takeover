using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace NPC_PCU_Fixer2.UI
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class UserControlInterface : UserControl
    {
        private Main Plugin { get; }
        //Do we really need to do anything? here?


        

        public UserControlInterface()
        {
            InitializeComponent();
        }

    }
}
