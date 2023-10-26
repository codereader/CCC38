using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CommonWPF
{
    /// <summary>
    /// Interaction logic for FieldGrid.xaml
    /// </summary>
    public partial class FieldGrid : UserControl
    {
        public double ItemSize
        {
            get
            {
                return ((PositionConverter)FindResource("PositionConverter")).Size;
            }
            set
            {
                ((PositionConverter)FindResource("PositionConverter")).Size = value;
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            name: nameof(ItemsSource),
            propertyType: typeof(IEnumerable<IGridItem>),
            ownerType: typeof(FieldGrid),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnItemsSourceChanged));

        public IEnumerable<IGridItem> ItemsSource
        {
            get => (IEnumerable<IGridItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public FieldGrid()
        {
            InitializeComponent();
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (FieldGrid)d;
            self.FieldGridCanvas.ItemsSource = self.ItemsSource;
        }

    }
}
