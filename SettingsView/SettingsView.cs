﻿using System;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace AiForms.Renderers
{
    /// <summary>
    /// Settings view.
    /// </summary>
    [ContentProperty("Root")]
    public partial class SettingsView : TableView
    {
        internal static Action _clearCache;
        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void ClearCache()
        {
            _clearCache?.Invoke();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public new SettingsModel Model { get; set; }
        /// <summary>
        /// Occurs when model changed.
        /// </summary>
        public new event EventHandler ModelChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollectionChangedEventHandler SectionCollectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:AiForms.Renderers.SettingsView"/> class.
        /// </summary>
        public SettingsView()
        {
            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;
            Root = new SettingsRoot();
            Model = new SettingsModel(Root);
        }


        SettingsRoot _root;
        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        /// <value>The root.</value>
        public new SettingsRoot Root
        {
            get { return _root; }
            set {
                if (_root != null) {
                    _root.SectionPropertyChanged -= RootOnPropertyChanged;
                    _root.CollectionChanged -= OnCollectionChanged;
                    _root.SectionCollectionChanged -= OnSectionCollectionChanged;
                }

                _root = value;

                //transfer binding context to the children (maybe...)
                SetInheritedBindingContext(_root, BindingContext);

                _root.SectionPropertyChanged += RootOnPropertyChanged;
                _root.CollectionChanged += OnCollectionChanged;
                _root.SectionCollectionChanged += OnSectionCollectionChanged;
                OnModelChanged();
            }
        }

        /// <summary>
        /// Ons the binding context changed.
        /// </summary>
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (Root != null)
                SetInheritedBindingContext(Root, BindingContext);
        }

        void RootOnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TableSectionBase.TitleProperty.PropertyName ||
                e.PropertyName == Section.FooterTextProperty.PropertyName) {
                OnModelChanged();
            }
            else if(e.PropertyName == Section.IsVisibleProperty.PropertyName)
            {
                OnModelChanged();
            }
        }

        /// <summary>
        /// Ons the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == HasUnevenRowsProperty.PropertyName ||
                propertyName == HeaderHeightProperty.PropertyName ||
                propertyName == HeaderFontSizeProperty.PropertyName ||
                propertyName == HeaderTextColorProperty.PropertyName ||
                propertyName == HeaderBackgroundColorProperty.PropertyName ||
                propertyName == HeaderTextVerticalAlignProperty.PropertyName ||
                propertyName == HeaderPaddingProperty.PropertyName ||
                propertyName == FooterFontSizeProperty.PropertyName ||
                propertyName == FooterTextColorProperty.PropertyName ||
                propertyName == FooterBackgroundColorProperty.PropertyName ||
                propertyName == FooterPaddingProperty.PropertyName
               ) {

                OnModelChanged();
            }
        }

        /// <summary>
        /// CollectionChanged by the section
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                e.NewItems.Cast<Section>().ForEach(section => {
                    if (section.HeaderView != null)
                    {
                        section.HeaderView.Parent = this;
                    }
                    if (section.FooterView != null)
                    {
                        section.FooterView.Parent = this;
                    }
                });
                e.NewItems.Cast<Section>().SelectMany(x => x).ForEach(cell => cell.Parent = this);
            }
            CollectionChanged?.Invoke(sender, e);
            //OnModelChanged();
        }

        /// <summary>
        /// CollectionChanged by the child in section
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">The ${ParameterType} instance containing the event data.</param>
        public void OnSectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                e.NewItems.Cast<Cell>().ForEach(cell => cell.Parent = this);
            }
            SectionCollectionChanged?.Invoke(sender, e);
            //OnModelChanged();
        }

        new void OnModelChanged()
        {
            foreach(var section in Root)
            {
                if (section.HeaderView != null)
                {
                    section.HeaderView.Parent = this;
                }
                if (section.FooterView != null)
                {
                    section.FooterView.Parent = this;
                }
            }

            var cells = Root?.SelectMany(r => r);
            if (cells == null)
            {
                return;
            }
                      

            foreach (Cell cell in cells)
            {
                //ViewCell size is not decided if parent isn't set.
                cell.Parent = this;
            }

            //notify Native
            if (ModelChanged != null)
                ModelChanged(this, EventArgs.Empty);

        }

        //make the unnecessary property existing at TableView sealed.
        private new int Intent { get; set; }
    }
}
