﻿using System;
using System.ComponentModel;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace AiForms.Renderers.iOS
{
    [Foundation.Preserve(AllMembers = true)]
    public class CustomCellContent:UIView
    {
        WeakReference<IVisualElementRenderer> _rendererRef;
        bool _disposed;
        NSLayoutConstraint _heightConstraint;
        View _formsCell;
        public View FormsCell {
            get { return _formsCell; }
            set {
                if (_formsCell == value)
                    return;
                UpdateCell(value);
            }
        }
        public CustomCell CustomCell { get; set; }
        double _lastWidth = -9999d;
        double _lastHeight = -9999d;

        public CustomCellContent() { }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_formsCell != null)
                {
                    _formsCell.PropertyChanged -= CellPropertyChanged;
                }

                CustomCell = null;

                _heightConstraint?.Dispose();
                _heightConstraint = null;

                IVisualElementRenderer renderer = null;
                if (_rendererRef != null && _rendererRef.TryGetTarget(out renderer) && renderer.Element != null)
                {
                    FormsInternals.DisposeModelAndChildrenRenderers(renderer.Element);
                    _rendererRef = null;
                }

                renderer?.Dispose();

                _formsCell = null;
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        public virtual void UpdateNativeCell()
        {
            UpdateIsEnabled();
        }

        public virtual void CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
            {
                UpdateIsEnabled();
            }
        }

        protected virtual void UpdateIsEnabled()
        {
            UserInteractionEnabled = FormsCell.IsEnabled;
        }

        public override void LayoutSubviews()
        {
            if (FormsCell == null)
            {
                return;
            }

            //This sets the content views frame.
            base.LayoutSubviews();

            if(CustomCell.IsMeasureOnce && Frame.Width <= _lastWidth && Frame.Height <= _lastHeight)
            {
                Layout.LayoutChildIntoBoundingRegion(FormsCell,
                new Rectangle(0, 0, _lastWidth, _lastHeight));
                return;
            }

            SizeToFit();

            Layout.LayoutChildIntoBoundingRegion(FormsCell, 
                new Rectangle(0,0,Frame.Width,Frame.Height));

            if (_rendererRef == null)
                return;

            IVisualElementRenderer renderer;
            if (_rendererRef.TryGetTarget(out renderer))
                renderer.NativeView.Frame = FormsCell.Bounds.ToRectangleF();

            if (_heightConstraint != null)
            {
                _heightConstraint.Active = false;
                _heightConstraint?.Dispose();
            }

            _heightConstraint = HeightAnchor.ConstraintEqualTo(Frame.Height);
            _heightConstraint.Priority = 999f;
            _heightConstraint.Active = true;

            _lastWidth = Frame.Width;
            _lastHeight = Frame.Height;
        }


        public override CGSize SizeThatFits(CGSize size)
        {
            IVisualElementRenderer renderer;
            if (!_rendererRef.TryGetTarget(out renderer))
                return base.SizeThatFits(size);

            if (renderer.Element == null)
                return CGSize.Empty;

            double width = size.Width;
            var height = size.Height > 0 ? size.Height : double.PositiveInfinity;
            var result = renderer.Element.Measure(width, height, MeasureFlags.IncludeMargins);

            return new CGSize(size.Width, (float)result.Request.Height);
        }

        protected virtual void UpdateCell(View cell)
        {
            if (_formsCell != null)
            {
                _formsCell.PropertyChanged -= CellPropertyChanged;
            }
            _formsCell = cell;
            _formsCell.PropertyChanged += CellPropertyChanged;

            IVisualElementRenderer renderer;
            if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
            {
                renderer = GetNewRenderer();
            }
            else
            {
                if (renderer.Element != null && renderer == Platform.GetRenderer(renderer.Element))
                    renderer.Element.ClearValue(FormsInternals.RendererProperty);

                var type = Xamarin.Forms.Internals.Registrar.Registered.GetHandlerTypeForObject(this._formsCell);
                var reflectableType = renderer as System.Reflection.IReflectableType;
                var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : renderer.GetType();
                if (rendererType == type || (renderer.GetType() == FormsInternals.DefaultRenderer) && type == null)
                    renderer.SetElement(this._formsCell);
                else
                {
                    //when cells are getting reused the element could be already set to another cell
                    //so we should dispose based on the renderer and not the renderer.Element
                    FormsInternals.DisposeRendererAndChildren(renderer);
                    renderer = GetNewRenderer();
                }
            }

            Platform.SetRenderer(this._formsCell, renderer);
            UpdateNativeCell();
            SetNeedsLayout();
        }

        protected virtual IVisualElementRenderer GetNewRenderer()
        {
            var newRenderer = Platform.CreateRenderer(_formsCell);
            _rendererRef = new WeakReference<IVisualElementRenderer>(newRenderer);
            AddSubview(newRenderer.NativeView);

            return newRenderer;
        }
    }
}
