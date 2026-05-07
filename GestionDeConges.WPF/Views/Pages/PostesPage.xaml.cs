using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace GestionDeConges.WPF.Views.Pages;

public partial class PostesPage : UserControl
{
    public PostesPage()
    {
        InitializeComponent();
    }

    private void BtnAjouterPoste_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AjouterPosteDialog
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.PosteCree is not null)
        {
            if (DataContext is PostesViewModel vm)
                vm.AjouterPosteAsync(dialog.PosteCree);
        }
    }
}