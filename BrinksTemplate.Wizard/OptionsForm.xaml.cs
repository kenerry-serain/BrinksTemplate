using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BrinksTemplate.Wizard
{
    /// <summary>
    /// Interaction logic for OptionsForm.xaml
    /// </summary>
    public partial class OptionsForm : Window
    {
        private readonly IEnumerable<string> _projectCollection;
        public OptionsForm(IEnumerable<string> projetoCollection)
        {
            InitializeComponent();
            _projectCollection = projetoCollection;

            this.Loaded += OptionsForm_Loaded;
        }

        public string DomainProject
        {
            get { return domainBox.SelectedValue?.ToString(); }
        }

        public string WebApiProject
        {
            get { return webApiBox.SelectedValue?.ToString(); }
        }

        private static bool _isCanceled;
        public static bool IsCanceled
        {
            get { return _isCanceled; }
            set { _isCanceled = value; }
        }

        private void buttonGenerate_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = false;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            Close();
        }

        private void OptionsForm_Loaded(object sender, RoutedEventArgs e)
        {
            domainBox.ItemsSource = _projectCollection.ToList();
            domainBox.SelectedItem = _projectCollection?.FirstOrDefault(p => p.Contains("Domain"));

            webApiBox.ItemsSource = _projectCollection.ToList();
            webApiBox.SelectedItem = _projectCollection?.FirstOrDefault(p => p.Contains("API"));
        }
    }
}
