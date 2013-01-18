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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quarto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[][] Figures;
        const byte NF = 16;

        public MainWindow()
        {
            InitializeComponent();
            InitializeFigures();
        }

        private void InitializeFigures() {
            byte i, j;
            // Shuffle figures randomly
            Figures = new byte[4][];
            for (i = 0; i < 4; i++)
                Figures[i] = new byte[4];

            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            byte[] f = new byte[NF];
            for (i = 0; i < NF; i++)
                f[i] = (byte)r.Next(16 - i);

            bool[] u = new bool[NF];
            for (i = 0; i < NF; i++)
                u[i] = false;

            byte[] res = new byte[NF];
            for (i = 0; i < NF; i++)
            {
                byte t = f[i];
                for (j = NF - 1; j >= 0; j--)
                    if (!u[j])
                    {
                        if (t == 0)
                            break;
                        t--;
                    }
                res[i] = j;
                u[j] = true;
            }

            Grid FiguresToTakeGrid = (Grid)FindName("FiguresToTakeGrid");
            int k = 0;
            for (i = 0; i < 4; i++)
                for (j = 0; j < 4; j++) 
                {
                    FigureWrapper tempFigureWrapper = new FigureWrapper(res[k]);
                    FiguresToTakeGrid.Children.Add(tempFigureWrapper);

                    Grid.SetRow(tempFigureWrapper, i);
                    Grid.SetColumn(tempFigureWrapper, j);

                    k++;
                }
        }
    }
}
