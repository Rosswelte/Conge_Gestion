using GestionDeConges.Core.Interfaces;
using GestionDeConges.WPF.ViewModels;
using GestionDeConges.WPF.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace GestionDeConges.WPF.Views.Pages
{
    public partial class EmployesPage : UserControl
    {
        public EmployesPage()
        {
            InitializeComponent();
        }

        private async void BtnAjouterEmploye_Click(object sender, RoutedEventArgs e)
        {
            var posteService = App.Services.GetRequiredService<IPosteService>();
            var dialog = new AjouterEmployeDialog(posteService)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.EmployeCree is not null)
            {
                if (DataContext is EmployesViewModel vm)
                {
                    await vm.AjouterEmployeAsync(dialog.EmployeCree);
                }
            }
        }
    }
}