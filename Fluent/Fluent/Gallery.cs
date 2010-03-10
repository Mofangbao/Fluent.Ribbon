﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Fluent
{
    /// <summary>
    /// Represents gallery control.
    /// </summary>
    [ContentProperty("Items")]
    public class Gallery:RibbonItemsControl
    {
        #region Fields

        private RibbonListBox listBox;

        private ObservableCollection<GalleryGroupFilter> filters;

        private ObservableCollection<GalleryGroupIcon> groupIcons;

        private DropDownButton groupsMenuButton;

        private double cachedWidthDelta;

        #endregion

        #region Properties

        #region ItemsInRow

        /// <summary>
        /// Width of the Gallery 
        /// </summary>
        public int ItemsInRow
        {
            get { return (int)GetValue(ItemsInRowProperty); }
            set { SetValue(ItemsInRowProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ItemsInRow.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ItemsInRowProperty =
            DependencyProperty.Register("ItemsInRow", typeof(int), typeof(Gallery), new UIPropertyMetadata(0, OnItemsInRowChanged));

        private static void OnItemsInRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Gallery).SetMinWidth();            
        }

        private void SetMinWidth()
        {
            if (listBox==null) return;
            //if (cachedWidthDelta == 0)
            {
                Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
                cachedWidthDelta = DesiredSize.Width - listBox.InnerPanelWidth;
                InvalidateMeasure();
            }
            if (ItemsInRow == 0) MinWidth = 0;
            else
            {
                double w = GetItemWidth();
                if (!double.IsNaN(w)) MinWidth = ItemsInRow * w + cachedWidthDelta;
            }
        }

        #endregion

        #region View

        /// <summary>
        /// Gets view of items or itemssource
        /// </summary>
        public CollectionViewSource View
        {
            get { return (CollectionViewSource)GetValue(ViewProperty); }
            private set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for View.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(CollectionViewSource), typeof(Gallery), new UIPropertyMetadata(null));

        #endregion

        #region ScrollBarsVisibility

        /// <summary> 
        /// HorizonalScollbarVisibility is a Windows.Controls.ScrollBarVisibility that
        /// determines if a horizontal scrollbar is shown. 
        /// </summary> 
        [Bindable(true), Category("Appearance")]
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for HorizontalScrollBarVisibility.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Gallery), new UIPropertyMetadata(ScrollBarVisibility.Disabled));

        /// <summary> 
        /// VerticalScrollBarVisibility is a System.Windows.Controls.ScrollBarVisibility that 
        /// determines if a vertical scrollbar is shown.
        /// </summary> 
        [Bindable(true), Category("Appearance")]
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for VerticalScrollBarVisibility.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(Gallery), new UIPropertyMetadata(ScrollBarVisibility.Visible));

        #endregion

        #region GroupBy

        public string GroupBy
        {
            get { return (string)GetValue(GroupByProperty); }
            set { SetValue(GroupByProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupBy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupByProperty =
            DependencyProperty.Register("GroupBy", typeof(string), typeof(Gallery), new UIPropertyMetadata(null, OnGroupByChanged));

        private static void OnGroupByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Gallery).UpdateGroupBy(e.NewValue as string);
        }

        private void UpdateGroupBy(string groupBy)
        {
            if (View!=null)
            {
                View.GroupDescriptions.Clear();
                if (groupBy != null)
                {
                    View.GroupDescriptions.Add(new PropertyGroupDescription(groupBy));
                }
            }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Gets or sets orientation of gallery
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Gallery), new UIPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           
                //(d as InRibbonGallery).gallery.Orientation = (Orientation) e.NewValue;
            if ((d as Gallery)!= null)
            {
                if ((Orientation) e.NewValue == Orientation.Horizontal)
                {
                    ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof (WrapPanel)));
                    template.Seal();
                    (d as Gallery).ItemsPanel = template;
                }
                else
                {
                    ItemsPanelTemplate template =
                        new ItemsPanelTemplate(new FrameworkElementFactory(typeof (StackPanel)));
                    template.Seal();
                    (d as Gallery).ItemsPanel = template;
                }
            }
        }

        #endregion

        #region Filters

        /// <summary>
        /// Gets collection of filters
        /// </summary>
        /// <summary>
        /// Gets collection of filters
        /// </summary>
        public ObservableCollection<GalleryGroupFilter> Filters
        {
            get
            {
                if (this.filters == null)
                {
                    this.filters = new ObservableCollection<GalleryGroupFilter>();
                    this.filters.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnFilterCollectionChanged);
                }
                return this.filters;
            }
        }


        // Handle toolbar iitems changes
        private void OnFilterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateGroupBy(GroupBy);
            if (Filters.Count > 0) HasFilter = true;
            else HasFilter = false;
            InvalidateProperty(SelectedFilterProperty);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object obj2 in e.NewItems)
                    {
                        if(groupsMenuButton!=null)
                        {
                            GalleryGroupFilter filter = obj2 as GalleryGroupFilter;
                            MenuItem item = new MenuItem();
                            item.Text = filter.Title;
                            item.Tag = filter;
                            if (filter == SelectedFilter) item.IsChecked = true;
                            item.Click += OnFilterMenuItemClick;
                            groupsMenuButton.Items.Add(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object obj3 in e.OldItems)
                    {
                        
                        if (groupsMenuButton != null)
                        {
                            groupsMenuButton.Items.Remove(GetFilterMenuItem(obj3 as GalleryGroupFilter));
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object obj4 in e.OldItems)
                    {
                        if (groupsMenuButton != null)
                        {
                            groupsMenuButton.Items.Remove(GetFilterMenuItem(obj4 as GalleryGroupFilter));
                        }
                    }
                    foreach (object obj5 in e.NewItems)
                    {
                        if (groupsMenuButton != null)
                        {
                            GalleryGroupFilter filter = obj5 as GalleryGroupFilter;
                            MenuItem item = new MenuItem();
                            item.Text = filter.Title;
                            item.Tag = filter;
                            if (filter == SelectedFilter) item.IsChecked = true;
                            item.Click += OnFilterMenuItemClick;
                            groupsMenuButton.Items.Add(item);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets or sets selected filter
        /// </summary>               
        public GalleryGroupFilter SelectedFilter
        {
            get { return (GalleryGroupFilter)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedFilter.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedFilterProperty =
            DependencyProperty.Register("SelectedFilter", typeof(GalleryGroupFilter), typeof(Gallery), new UIPropertyMetadata(null, OnFilterChanged, CoerceSelectedFilter));

        // Coerce selected filter
        private static object CoerceSelectedFilter(DependencyObject d, object basevalue)
        {
            Gallery gal = d as Gallery;
            if ((basevalue == null) && (gal.Filters.Count > 0)) return gal.Filters[0];
            return basevalue;
        }

        // Handles filter property changed
        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //(d as Gallery).UpdateFilter();
            if (e.NewValue != null) (d as Gallery).SelectedFilterTitle = (e.NewValue as GalleryGroupFilter).Title;
            else (d as Gallery).SelectedFilterTitle = "";
            if ((d as Gallery).View.View!=null) (d as Gallery).View.View.Refresh();
        }

        /// <summary>
        /// Gets selected filter title
        /// </summary>
        public string SelectedFilterTitle
        {
            get { return (string)GetValue(SelectedFilterTitleProperty); }
            private set { SetValue(SelectedFilterTitleProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedFilterTitle.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedFilterTitleProperty =
            DependencyProperty.Register("SelectedFilterTitle", typeof(string), typeof(Gallery), new UIPropertyMetadata(null));



        /// <summary>
        /// Gets whether gallery has selected filter
        /// </summary>       
        public bool HasFilter
        {
            get { return (bool)GetValue(HasFilterProperty); }
            private set { SetValue(HasFilterProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for HasFilter.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HasFilterProperty =
            DependencyProperty.Register("HasFilter", typeof(bool), typeof(Gallery), new UIPropertyMetadata(false));


        // Set filter
        /*private void UpdateFilter()
        {
            if ((listBox != null) && (listBox.ItemsSource != null))
            {
                if (view != null) view.Filter -= OnFiltering;
                view = CollectionViewSource.GetDefaultView(listBox.ItemsSource);
                view.Filter += OnFiltering;
            }
        }*/

        private void  OnFiltering(object obj, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(GroupBy)) e.Accepted=true;
            else if (SelectedFilter == null) e.Accepted = true;
            else
            {
                string[] filterItems = SelectedFilter.Groups.Split(",".ToCharArray());
                e.Accepted = filterItems.Contains(GetItemGroupName(e.Item));
            }            
        }        

        #endregion

        #region Selectable

        public bool Selectable
        {
            get { return (bool)GetValue(SelectableProperty); }
            set { SetValue(SelectableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectableProperty =
            DependencyProperty.Register("Selectable", typeof(bool), typeof(Gallery), new UIPropertyMetadata(true));

        #endregion

        #region SelectedIndex

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Gallery), new UIPropertyMetadata(-1, OnSelectedIndexChanged, CoerceSelectedIndex));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Gallery gal = d as Gallery;
            object item = gal.GetItem(gal.SelectedIndex);
            if (gal.SelectedItem != item) gal.SelectedItem = item;            
        }

        private static object CoerceSelectedIndex(DependencyObject d, object basevalue)
        {
            if (!(d as Gallery).Selectable)
            {
                (d as Gallery).listBox.SelectedIndex = -1;
                return -1;
            }
            else return basevalue;
        }

        #endregion

        #region SelectedItem

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(Gallery), new UIPropertyMetadata(null, OnSelectedItemChanged, CoerceSelectedItem));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Gallery gal = d as Gallery;
            int index = gal.GetItemIndex(gal.SelectedItem);
            if (gal.SelectedIndex != index) gal.SelectedIndex = index;
        }

        private static object CoerceSelectedItem(DependencyObject d, object basevalue)
        {
            if (!(d as Gallery).Selectable)
            {
                if (basevalue != null)
                {
                    ((d as Gallery).listBox.ContainerFromElement(basevalue as DependencyObject) as ListBoxItem).IsSelected = false;
                    (d as Gallery).listBox.SelectedItem = null;
                }
                return null;
            }
            else return basevalue;
        }

        #endregion

        #region GroupIcons

        /// <summary>
        /// Gets collection of group icons
        /// </summary>
        public ObservableCollection<GalleryGroupIcon> GroupIcons
        {
            get
            {
                if (this.groupIcons == null)
                {
                    this.groupIcons = new ObservableCollection<GalleryGroupIcon>();
                    this.groupIcons.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnGroupIconCollectionChanged);
                }
                return this.groupIcons;
            }
        }


        // Handle toolbar iitems changes
        private void OnGroupIconCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateGroupBy(GroupBy);
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static Gallery()
        {            
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Gallery), new FrameworkPropertyMetadata(typeof(Gallery)));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Gallery()
        {            
            View = new CollectionViewSource();
            View.Filter += OnFiltering;
            
            ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof (WrapPanel)));
            template.Seal();
            ItemsPanel = template;

            //Loaded += delegate { SetMinWidth(); };
        }

        #endregion

        #region Overrides

        protected override void OnItemsCollectionChanged(NotifyCollectionChangedEventArgs e)        
        {
            View.Source = Items;
            UpdateGroupBy(GroupBy);            
        }

        protected override void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            View.Source = ItemsSource;
            UpdateGroupBy(GroupBy);
        }

        public override void OnApplyTemplate()
        {            
            groupsMenuButton = GetTemplateChild("PART_DropDownButton") as DropDownButton;
            if (groupsMenuButton != null)
            {
                for(int i=0;i<Filters.Count;i++)
                {
                    MenuItem item = new MenuItem();
                    item.Text = Filters[i].Title;
                    item.Tag = Filters[i];
                    if (Filters[i] == SelectedFilter) item.IsChecked = true;
                    item.Click += OnFilterMenuItemClick;
                    groupsMenuButton.Items.Add(item);
                }
            }

            if(listBox!=null)
            {
                listBox.ItemContainerGenerator.StatusChanged -= OnItemsContainerGeneratorStatusChanged;
            }

            listBox = GetTemplateChild("PART_ListBox") as RibbonListBox;
            if (listBox != null)
            {
                //listBox.Items.Filter += OnFiltering;

                

                listBox.SelectedIndex = SelectedIndex;
                listBox.SelectedItem = SelectedItem;

                Binding binding = new Binding("SelectedIndex");
                binding.Source = listBox;
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SetBinding(SelectedIndexProperty, binding);

                binding = new Binding("SelectedItem");
                binding.Source = listBox;
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SetBinding(SelectedItemProperty, binding);

                binding = new Binding("HasItems");
                binding.Source = listBox;
                binding.Mode = BindingMode.OneWay;
                SetBinding(HasItemsProperty, binding);

                binding = new Binding("IsGrouping");
                binding.Source = listBox;
                binding.Mode = BindingMode.OneWay;
                SetBinding(IsGroupingProperty, binding);

                binding = new Binding("ItemContainerGenerator");
                binding.Source = listBox;
                binding.Mode = BindingMode.OneWay;
                SetBinding(ItemContainerGeneratorProperty, binding);

                listBox.ItemContainerGenerator.StatusChanged += OnItemsContainerGeneratorStatusChanged;
                
            }
            UpdateGroupBy(GroupBy);
            SetMinWidth();                       
        }

        private void OnItemsContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            //SetMinWidth();
        }

        private void OnFilterMenuItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = GetFilterMenuItem(SelectedFilter);
            item.IsChecked = false;
            (sender as MenuItem).IsChecked = true;
            SelectedFilter = (sender as MenuItem).Tag as GalleryGroupFilter;
            e.Handled = true;
            RibbonPopup.CollapseCurrent();
        }

        private MenuItem GetFilterMenuItem(GalleryGroupFilter filter)
        {
            if(filter==null) return null;
            return groupsMenuButton.Items.First(x => (x as MenuItem).Text == filter.Title) as MenuItem;
        }

        #endregion

        #region Quick Access Item Creating

        /// <summary>
        /// Gets control which represents shortcut item.
        /// This item MUST be syncronized with the original 
        /// and send command to original one control.
        /// </summary>
        /// <returns>Control which represents shortcut item</returns>
        public override FrameworkElement CreateQuickAccessItem()
        {
            DropDownButton button = new DropDownButton();
            CreateMenuIfNeeded();
            parentContextMenu = FindContextMenu();
            if (parentContextMenu != null)
            {
                IQuickAccessItemProvider provider = parentContextMenu.Owner as IQuickAccessItemProvider;
                if (provider != null) return provider.CreateQuickAccessItem();
                button.PreviewMouseLeftButtonDown += OnQuickAccessClick;
            }
            
            BindQuickAccessItem(button);

            return button;
        }

        private void CreateMenuIfNeeded()
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(this);
            while (parent != null)
            {
                DropDownButton btn = (parent as DropDownButton);
                if (btn!=null)
                {
                    btn.DoCreateMenu();
                    return;
                }
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return;            
        }

        private void OnQuickAccessClick(object sender, MouseButtonEventArgs e)
        {
            DropDownButton button = sender as DropDownButton;
            button.MenuResizeMode = parentContextMenu.ResizeMode;
            for (int i = 0; i < parentContextMenu.Items.Count; i++)
            {
                UIElement item = parentContextMenu.Items[0];
                parentContextMenu.Items.Remove(item);
                button.Items.Add(item);
                i--;
            }
            button.MenuClosed += OnQuickAccessMenuClosed;
            quickAccessButton = button;
        }

        private ContextMenu parentContextMenu;
        private DropDownButton quickAccessButton;

        private void OnQuickAccessMenuClosed(object sender, EventArgs e)
        {
            quickAccessButton.MenuClosed -= OnQuickAccessMenuClosed;
            for (int i = 0; i < quickAccessButton.Items.Count; i++)
            {
                UIElement item = quickAccessButton.Items[0];
                quickAccessButton.Items.Remove(item);
                parentContextMenu.Items.Add(item);
                i--;
            }
        }

        private ContextMenu FindContextMenu()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while(parent!=null)
            {
                if (parent is ContextMenuBar) return (parent as ContextMenuBar).ParentContextMenu;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// This method must be overriden to bind properties to use in quick access creating
        /// </summary>
        /// <param name="element">Toolbar item</param>
        protected override void BindQuickAccessItem(FrameworkElement element)
        {
            DropDownButton button = element as DropDownButton;
            //if (parentContextMenu != null) Bind(button, parentContextMenu, "ResizeMode", DropDownButton.MenuResizeModeProperty, BindingMode.Default);
            base.BindQuickAccessItem(element);
        }

        #endregion

        #region Private Methods

        IEnumerable GetItems()
        {
            IEnumerable items = ItemsSource;
            if (items == null) items = Items;
            return items;
        }

        object GetItem(int index)
        {
            IEnumerable items = GetItems();
            if (items == null) return null;
            int i = 0;
            foreach (var item in items)
            {
                if (i == index) return item;
                i++;
            }
            return null;
        }

        int GetItemIndex(object obj)
        {
            IEnumerable items = GetItems();
            if (items == null) return -1;
            int i = 0;
            foreach (var item in items)
            {
                if (item == obj) return i;
                i++;
            }
            return -1;
        }

        internal string GetItemGroupName(object obj)
        {
            if(obj==null) return null;
            object result = obj.GetType().GetProperty(GroupBy, BindingFlags.Public | BindingFlags.Instance).GetValue(obj, null);
            if(result==null) return null;
            return result.ToString();
        }

        private double GetItemWidth()
        {
            if (double.IsNaN(ItemWidth) && (listBox != null) && (listBox.Items.Count > 0))
            {
                GalleryItem item = (listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[0]) as GalleryItem);
                bool useHack = false;
                if (item == null)
                {
                    useHack = true;
                    RemoveLogicalChild(listBox.Items[0]);
                    item = new GalleryItem();
                    item.Width = ItemWidth;
                    item.Height = ItemHeight;
                    if (ItemContainerStyle != null) item.Style = ItemContainerStyle;
                    if (ItemTemplate != null)
                    {
                        item.Content = ItemTemplate;
                        item.DataContext = listBox.Items[0];
                    }
                    else item.Content = listBox.Items[0];
                }
                item.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (useHack)
                {
                    item.Content = null;
                    AddLogicalChild(listBox.Items[0]);
                }
                return item.DesiredSize.Width;
            }
            return ItemWidth;
        }

        #endregion
    }
}
