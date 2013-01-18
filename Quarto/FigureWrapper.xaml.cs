﻿using System;
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

using QuartoLib;

namespace Quarto
{
    /// <summary>
    /// Interaction logic for FigureWrapper.xaml
    /// </summary>
    public partial class FigureWrapper : UserControl
    {
        /// <summary>
        /// Figure code
        /// </summary>
        private byte _figure;
        public byte Figure
        {
            set { _figure = value; }
            get { return _figure; }
        }

        /// <summary>
        /// Figure then can not be taken.
        /// Hover pointer is set to default.
        /// </summary>
        private bool _figurePlacedOrChosen;
        public bool FigurePlacedOrChosen {
            set { FigurePlacedOrChosen_Changing(); _figurePlacedOrChosen = value; FigurePlacedOrChosen_Changed(); }
            get { return _figurePlacedOrChosen; }
        }

        private void FigurePlacedOrChosen_Changing() {
        
        }

        private void FigurePlacedOrChosen_Changed() {
            this.Cursor = Cursors.Hand;
        }

        public FigureWrapper()
        {
            InitializeComponent();
        }

        public FigureWrapper(byte figure) {
            InitializeComponent();
            FigurePlacedOrChosen = false;
            Figure = figure;

            Ellipse e1 = (Ellipse)this.FindName("Ellipse1");
            Ellipse e2 = (Ellipse)this.FindName("Ellipse2");
            Ellipse e3 = (Ellipse)this.FindName("Ellipse3");
            Ellipse e4 = (Ellipse)this.FindName("Ellipse4");

            e1.Fill = new SolidColorBrush(((figure & 1) == 0) ? Color.FromArgb(255, 0x44, 0x44, 0x44) : Color.FromArgb(255, 255, 255, 255));
            e2.Fill = new SolidColorBrush(((figure & 2) == 0) ? Color.FromArgb(255, 0x44, 0x44, 0x44) : Color.FromArgb(255, 255, 255, 255));
            e3.Fill = new SolidColorBrush(((figure & 4) == 0) ? Color.FromArgb(255, 0x44, 0x44, 0x44) : Color.FromArgb(255, 255, 255, 255));
            e4.Fill = new SolidColorBrush(((figure & 8) == 0) ? Color.FromArgb(255, 0x44, 0x44, 0x44) : Color.FromArgb(255, 255, 255, 255));
        }

        private void FigureRectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rec = (Rectangle)FindName("FigureRectangle");
            rec.Stroke = new SolidColorBrush(Color.FromRgb(0xCC, 0x55, 0x55));
        }

        private void FigureRectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rec = (Rectangle)FindName("FigureRectangle");
            rec.Stroke = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
        }
    }
}
