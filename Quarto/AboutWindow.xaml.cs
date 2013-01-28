using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Quarto
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            TextBlock textBlock = (TextBlock)FindName("AboutTextBlock");
            textBlock.Text = "Quarto game. Version 0.0.0.1.\r\nCreated by Vanvector (vanvector@gmail.com).";
        }
    }
}
