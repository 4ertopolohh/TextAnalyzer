using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TextAnalyzer.Models;
using TextAnalyzer.Services;

namespace TextAnalyzer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        //хранит введеный текст
        private string _inputText;
        private readonly TextAnalysisService _textAnalysisService;

        //конструктор
        public MainWindowViewModel()
        {
            _textAnalysisService = new TextAnalysisService();

            //коллекции для результатов частотности и орфографии
            FrequencyItems = new ObservableCollection<FrequencyItem>();
            SpellingErrors = new ObservableCollection<SpellingError>();

            //комманды для кнопок
            FrequencyAnalysisCommand = new RelayCommand(ExecuteFrequencyAnalysis, _ => CanRunAnalysis);
            SpellingCheckCommand = new RelayCommand(ExecuteSpellingCheck, _ => CanRunAnalysis);
        }

        public string InputText
        {
            //принимает введеный текст
            get => _inputText;
            set
            {
                //срабатывает при введении символов
                //сохранние значения
                _inputText = value;
                //уведомление об изменеиии
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRunAnalysis));

                //вызывает проверку активности у кнопок
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        //возвращает тру если в текстбоксе есть текст
        public bool CanRunAnalysis => !string.IsNullOrWhiteSpace(InputText);

        //коллекции для ошибок
        public ObservableCollection<FrequencyItem> FrequencyItems { get; set; }
        public ObservableCollection<SpellingError> SpellingErrors { get; set; }

        //впф ожидает эти команды для проверки текста
        public ICommand FrequencyAnalysisCommand { get; }
        public ICommand SpellingCheckCommand { get; }

        //метод для анализа частотности
        private void ExecuteFrequencyAnalysis(object parameter)
        {
            //очищает таблицу
            FrequencyItems.Clear();

            //вызывает анализ
            var result = _textAnalysisService.PerformFrequencyAnalysis(InputText);
            foreach (var item in result)
            {
                //добавляет данные в таблицу Слово - Количество
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

        //событие для обновления интерфейса
        public event PropertyChangedEventHandler PropertyChanged;

        //сам подставляет имя места откуда был вызван метод, что бы не писать вручную
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //передает какое свойство изменилось
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
