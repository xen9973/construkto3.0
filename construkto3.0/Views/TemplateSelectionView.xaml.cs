using construkto3._0.ViewModels;
using System;
using System.Windows;

namespace construkto3._0.Views
{
    public partial class TemplateSelectionView : Window
    {
        private readonly Action<string> _onTemplateSelected;
        public TemplateSelectionView(Action<string> onTemplateSelected)
        {
            InitializeComponent();
            _onTemplateSelected = onTemplateSelected;
            DataContext = new TemplateSelectionViewModel(selectedTemplate =>
            {
                _onTemplateSelected?.Invoke(selectedTemplate);
                Close();
            });
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            var viewModel = (TemplateSelectionViewModel)DataContext;
        }
    }
}