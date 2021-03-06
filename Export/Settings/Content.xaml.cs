﻿using Biller.Core.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace OrderTypes_Biller.Export.Settings
{
    /// <summary>
    /// Interaktionslogik für Content.xaml
    /// </summary>
    public partial class Content : UserControl
    {
        public Content()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            var control = new Controls.TextElement();
            control.MouseDown += control_MouseUp;
            viewModel.Elements.Add(control);
        }

        void control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedElement = sender as Settings.Controls.IExportControl;
        }

        #region ArticleColumns
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            ObservableCollection<Models.ArticleListColumnModel> list = ArticleColumnPresenter.ItemsSource as ObservableCollection<Models.ArticleListColumnModel>;
            list.Add(new Models.ArticleListColumnModel());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            ObservableCollection<Models.ArticleListColumnModel> list = ArticleColumnPresenter.ItemsSource as ObservableCollection<Models.ArticleListColumnModel>;
            list.Remove(viewModel.SelectedArticleListColumn);
        }

        /// <summary>
        /// Article columns stackpanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedArticleListColumn = (sender as StackPanel).DataContext as Models.ArticleListColumnModel;

            var sp = (StackPanel)sender;
            sp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDD");
        }

        /// <summary>
        /// Article columns stackpanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            viewModel.SaveSettings();
            
            var sp = (StackPanel)sender;
            sp.Background = System.Windows.Media.Brushes.Transparent;
        }

        /// <summary>
        /// Move article column down / Raise the index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            ObservableCollection<Models.ArticleListColumnModel> list = ArticleColumnPresenter.ItemsSource as ObservableCollection<Models.ArticleListColumnModel>;
            var index = list.IndexOf(viewModel.SelectedArticleListColumn);
            if (index >= 0)
            {
                list.RemoveAt(index);
                list.Insert(Math.Min(index + 1, list.Count), viewModel.SelectedArticleListColumn);
            }
        }

        /// <summary>
        /// Move article column up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            ObservableCollection<Models.ArticleListColumnModel> list = ArticleColumnPresenter.ItemsSource as ObservableCollection<Models.ArticleListColumnModel>;
            var index = list.IndexOf(viewModel.SelectedArticleListColumn);
            if (index >= 0)
            {
                list.RemoveAt(index);
                list.Insert(Math.Max(index - 1, 0), viewModel.SelectedArticleListColumn);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabcontrol = sender as TabControl;
            var viewModel = (DataContext as Export.Settings.ViewModel);
            switch (tabcontrol.SelectedIndex)
            {
                case 0:
                    //Invoice
                    ArticleColumnPresenter.SetBinding(ItemsControl.ItemsSourceProperty, "SettingsController.ArticleListColumns");
                    //ArticleColumnPresenter.ItemsSource = viewModel.SettingsController.ArticleListColumns;
                    break;
                case 1:
                    //Offer
                    ArticleColumnPresenter.SetBinding(ItemsControl.ItemsSourceProperty, "SettingsController.ArticleListColumnsOffer");
                    //ArticleColumnPresenter.ItemsSource = viewModel.SettingsController.ArticleListColumnsOffer;
                    break;
                case 2:
                    //DeliveryNote
                    ArticleColumnPresenter.SetBinding(ItemsControl.ItemsSourceProperty, "SettingsController.ArticleListColumnsDeliveryNote");
                    //ArticleColumnPresenter.ItemsSource = viewModel.SettingsController.ArticleListColumnsDeliveryNote;
                    break;
                default:
                    tabcontrol.SelectedIndex = 0;
                    break;
            }
        }
        #endregion

        #region FooterColumns

        private void SP_FooterColumns_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            viewModel.SaveSettings();

            var sp = (StackPanel)sender;
            sp.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void SP_FooterColumns_GotFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SelectedFooterColumn = (sender as StackPanel).DataContext as Models.FooterColumnModel;

            var sp = (StackPanel)sender;
            sp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#DDD");
        }

        private void ButtonMoveFooterColumnUp(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.FooterColumns.IndexOf(viewModel.SelectedFooterColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.FooterColumns.RemoveAt(index);
                viewModel.SettingsController.FooterColumns.Insert(Math.Max(index - 1, 0), viewModel.SelectedFooterColumn);
            }
        }

        private void ButtonMoveFooterColumnDown(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            this.Focus();
            var index = viewModel.SettingsController.FooterColumns.IndexOf(viewModel.SelectedFooterColumn);
            if (index >= 0)
            {
                viewModel.SettingsController.FooterColumns.RemoveAt(index);
                viewModel.SettingsController.FooterColumns.Insert(Math.Min(index + 1, viewModel.SettingsController.FooterColumns.Count), viewModel.SelectedFooterColumn);
            }
        }

        private void ButtonAddFooterColumn(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.FooterColumns.Add(new Models.FooterColumnModel());
        }

        private void ButtonRemoveFooterColumn(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.SettingsController.FooterColumns.Remove(viewModel.SelectedFooterColumn);
        }

        #endregion

        private void WatermarkTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Bilder|*.jpg; *.jpeg; *.jpe; *.jfif; *.png|PDF Dokument|*.pdf|Alle Dateien|*.*";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
                (sender as TextBox).Text = openFileDialog.FileName;
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = (DataContext as Export.Settings.ViewModel);
            var cb = sender as ComboBox;
            if (cb.SelectedIndex == -1)
            {
                var val = cmConverter.cmUnit.StringToValue(cb.Text);
                cb.Text = cmConverter.cmUnit.ValueToString(val);
                viewModel.SettingsController.ImagePositionLeft = val;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null)
                return;
            var viewModel = (DataContext as Export.Settings.ViewModel);
            viewModel.PropertyChanged += viewModel_PropertyChanged;
            viewModel.SettingsController.PropertyChanged += SettingsController_PropertyChanged;
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SettingsController")
                (DataContext as Export.Settings.ViewModel).SettingsController.PropertyChanged += SettingsController_PropertyChanged;
        }

        void SettingsController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ImagePositionLeftIndex")
                if ((DataContext as Export.Settings.ViewModel).SettingsController.ImagePositionLeftIndex == -1)
                    ComboBoxImagePositionLeft.Text = cmConverter.cmUnit.ValueToString((DataContext as Export.Settings.ViewModel).SettingsController.ImagePositionLeft);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((DataContext as Export.Settings.ViewModel).SettingsController.ImagePositionLeftIndex == -1)
                ComboBoxImagePositionLeft.Text = cmConverter.cmUnit.ValueToString((DataContext as Export.Settings.ViewModel).SettingsController.ImagePositionLeft);
        }
    }
}
