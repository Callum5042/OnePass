using System.Windows;
using System.Windows.Controls;

namespace OnePass.WPF.Controls
{
    /// <summary>
    /// Interaction logic for CustomTextbox.xaml
    /// </summary>
    public partial class CustomTextbox : UserControl
    {
        public CustomTextbox()
        {
            InitializeComponent();
        }

        public string Label { get; set; }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(CustomTextbox));
    }
}
