﻿using Microsoft.EntityFrameworkCore;
using OnePass.Services.DataAccess;
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

namespace OnePass.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Content = new LoginPage();
        }

        //private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void MenuItem_Click_Add(object sender, RoutedEventArgs e)
        //{
        //    ContentFrame.Navigate(new AddPage());
        //}
    }
}
