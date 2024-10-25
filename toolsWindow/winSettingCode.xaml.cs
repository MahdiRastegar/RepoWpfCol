using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for winSettingCode.xaml
    /// </summary>
    public partial class winSettingCode : Window
    {
        public winSettingCode()
        {
            InitializeComponent();
            MainWindow.Current.Effect = new BlurEffect() { Radius = 4 };
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var db = new ColDbEntities1();
            foreach (SfTextInputLayout item in stack.Children)
            {
                var textBox = item.InputView as System.Windows.Controls.TextBox;
                var y = db.CodeSetting.FirstOrDefault(t => t.Name == item.HelperText);
                if (y != null)
                    y.Value = (item.InputView as System.Windows.Controls.TextBox).Text;
                else
                    db.CodeSetting.Add(new CodeSetting()
                    {
                        Id= Guid.NewGuid(),
                        Name = item.HelperText,
                        Value = (item.InputView as System.Windows.Controls.TextBox).Text
                    });
            }
            db.SaveChanges();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(0);
                Height=stack.ActualHeight+175;
            }), DispatcherPriority.Render);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.Current.Effect = null;
        }
    }
}
