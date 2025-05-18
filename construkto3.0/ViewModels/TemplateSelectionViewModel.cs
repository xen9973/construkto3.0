using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace construkto3._0.ViewModels
{
    public class TemplateSelectionViewModel : ViewModelBase
    {
        public ObservableCollection<string> Templates { get; }
        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }

        public ICommand SelectTemplateCommand { get; }
        private readonly Action<string> _onTemplateSelected;

        public TemplateSelectionViewModel(Action<string> onTemplateSelected)
        {
            Templates = new ObservableCollection<string>();
            SelectTemplateCommand = new RelayCommand(_ => SelectTemplate());
            _onTemplateSelected = onTemplateSelected;
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            string templatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            if (Directory.Exists(templatesFolder))
            {
                string[] templateFiles = Directory.GetFiles(templatesFolder, "*.json");
                foreach (string file in templateFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    Templates.Add(name);
                }
            }
        }

        private void SelectTemplate()
        {
            if (!string.IsNullOrEmpty(SelectedTemplate))
            {
                _onTemplateSelected?.Invoke(SelectedTemplate);
            }
        }
    }
}