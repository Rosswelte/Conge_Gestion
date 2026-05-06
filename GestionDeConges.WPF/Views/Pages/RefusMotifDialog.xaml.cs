using System.Windows;

namespace GestionDeConges.WPF.Views.Pages
{
    public partial class RefusMotifDialog : Window
    {
        public string Motif => TxtMotif.Text;

        public RefusMotifDialog()
        {
            InitializeComponent();
        }

        private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtMotif.Text))
            {
                MessageBox.Show("Le motif est obligatoire.",
                    "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
