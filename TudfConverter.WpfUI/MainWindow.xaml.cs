using System.Windows;
using TudfConverter.WpfUI.ViewModels;

namespace TudfConverter.WpfUI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}