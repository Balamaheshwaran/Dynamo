﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Controls;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Media;
using Dynamo.Utilities;
using Dynamo.Search;

//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SearchView_Loaded);
        }

        void SearchView_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = dynSettings.Controller.SearchViewModel;

            PreviewKeyDown += dynSettings.Controller.SearchViewModel.KeyHandler;

            SearchTextBox.IsVisibleChanged += delegate
            {
                DynamoCommands.Search.Execute(null);
                Keyboard.Focus(this.SearchTextBox);
                var view = WPF.FindUpVisualTree<DynamoView>(this);
                //SearchTextBox.InputBindings.AddRange(dynSettings.Bench.InputBindings);
                SearchTextBox.InputBindings.AddRange(view.InputBindings);
            };

            SearchTextBox.GotKeyboardFocus += delegate
            {
                if (SearchTextBox.Text == "Search...")
                {
                    SearchTextBox.Text = "";
                }

                SearchTextBox.Foreground = Brushes.White;
            };

            SearchTextBox.LostKeyboardFocus += delegate
            {
                SearchTextBox.Foreground = Brushes.Gray;
            };

            dynSettings.Controller.SearchViewModel.RequestFocusSearch += new EventHandler(SearchViewModel_RequestFocusSearch);
            dynSettings.Controller.SearchViewModel.RequestReturnFocusToSearch += new EventHandler(SearchViewModel_RequestReturnFocusToSearch);
        }

        void SearchViewModel_RequestReturnFocusToSearch(object sender, EventArgs e)
        {
            Keyboard.Focus(SearchTextBox);
        }

        void SearchViewModel_RequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
        }

        public void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox) sender).Select(((TextBox) sender).Text.Length, 0);
            BindingExpression binding = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();
        }

        public void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((SearchViewModel) DataContext).ExecuteSelected();
        }

        public void ListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem) sender).IsSelected = true;
            Keyboard.Focus(this.SearchTextBox);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel) DataContext).RemoveLastPartOfSearchText();
            Keyboard.Focus(this.SearchTextBox);
        }

        public void ibtnServiceController_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            RegionMenu.PlacementTarget = (UIElement) sender;
            RegionMenu.IsOpen = true;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(sender);
        }

    }
} ;