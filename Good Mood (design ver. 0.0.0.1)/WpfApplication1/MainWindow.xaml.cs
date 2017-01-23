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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            text_Title.FontSize = e.NewSize.Height / 17.5;
            //text_Nickname.FontSize = e.NewSize.Height / 30;
            button_Enter.FontSize= e.NewSize.Height / 30;
            tb_Password.FontSize= e.NewSize.Height / 30;
            tb_PhoneNumber.FontSize= e.NewSize.Height / 30;
            text_Number.FontSize= e.NewSize.Height / 29;
            text_Password.FontSize= e.NewSize.Height / 29;
            pic_Avatar.Width = pic_Avatar.Height = e.NewSize.Height / 12.5;
            text_Register.FontSize= e.NewSize.Height / 35;
            text_Link.FontSize= e.NewSize.Height / 35;
        }
    }
}
