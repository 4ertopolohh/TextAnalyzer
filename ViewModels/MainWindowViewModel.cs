using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TextAnalyzer.Models;
using TextAnalyzer.Services;

namespace TextAnalyzer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _inputText;
        private readonly TextAnalysisService _textAnalysisService;

        public MainWindowViewModel()
        {
            _textAnalysisService = new TextAnalysisService();
            FrequencyAnalysisCommand = new RelayCommand(ExecuteFrequencyAnalysis);
            SpellingCheckCommand = new RelayCommand(ExecuteSpellingCheck);
            FrequencyItems = new ObservableCollection<FrequencyItem>();
            SpellingErrors = new ObservableCollection<SpellingError>();
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FrequencyItem> FrequencyItems { get; set; }
        public ObservableCollection<SpellingError> SpellingErrors { get; set; }

        public ICommand FrequencyAnalysisCommand { get; }
        public ICommand SpellingCheckCommand { get; }

        private void ExecuteFrequencyAnalysis(object parameter)
        {
            FrequencyItems.Clear();
            var result = _textAnalysisService.PerformFrequencyAnalysis(InputText);

            foreach (var item in result)
            {
                FrequencyItems.Add(new FrequencyItem { Word = item.Key, Count = item.Value });
            }
        }

        private void ExecuteSpellingCheck(object parameter)
        {
            SpellingErrors.Clear();
            var errors = _textAnalysisService.CheckSpelling(InputText);

            foreach (var error in errors)
            {
                SpellingErrors.Add(error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}