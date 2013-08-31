// Copyright (C) Microsoft Corporation. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Delay
{
    /// <summary>
    /// Class to help work around a Silverlight bug where DataContext changes to
    /// an element aren't propagated through Bindings on child elements that use
    /// ElementName or Source.
    /// </summary>
    public class DataContextPropagationGrid : Grid
    {
        /// <summary>
        /// Initializes a new instance of the DataContextPropagationGrid class.
        /// </summary>
        public DataContextPropagationGrid()
        {
            // Create a Binding to keep InheritedDataContextProperty correct
            SetBinding(InheritedDataContextProperty, new Binding());
        }

        /// <summary>
        /// Identifies the InheritedDataContext DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty InheritedDataContextProperty =
            DependencyProperty.Register(
                "InheritedDataContext",
                typeof(object),
                typeof(DataContextPropagationGrid),
                new PropertyMetadata(null, OnInheritedDataContextChanged));

        /// <summary>
        /// Handles changes to the InheritedDataContext DependencyProperty.
        /// </summary>
        /// <param name="d">Instance with property change.</param>
        /// <param name="e">Property change details.</param>
        private static void OnInheritedDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataContextPropagationGrid workaround = (DataContextPropagationGrid)d;
            // Update local value of DataContext to prompt Silverlight to update problematic Bindings
            workaround.DataContext = e.NewValue;
            // Unset local value of DataContext so it will continue to inherit from the parent
            workaround.ClearValue(FrameworkElement.DataContextProperty);
        }
    }
}
