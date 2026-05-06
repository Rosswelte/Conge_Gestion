using GestionDeConges.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GestionDeConges.WPF.Views.Pages
{
    public partial class DemandesPage : UserControl
    {
        public DemandesPage()
        {
            InitializeComponent();
            Loaded += DemandesPage_Loaded;
        }

        private async void DemandesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DemandesViewModel vm)
            {
                await vm.ChargerCommand.ExecuteAsync(null);
            }
        }

        // Ouvre une boîte de dialogue pour saisir le motif de refus
        private void BtnRefuser_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not DemandesViewModel vm)
                return;
            if (vm.DemandeSelectionnee is null)
                return;

            var dialog = new RefusMotifDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Motif))
            {
                vm.RefuserCommand.Execute(dialog.Motif);
            }
        }
    }
}