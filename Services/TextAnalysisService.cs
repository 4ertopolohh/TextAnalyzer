using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TextAnalyzer.Data;
using TextAnalyzer.Models;

namespace TextAnalyzer.Services
{
    public class TextAnalysisService
    {
        //подключение к бд
        private readonly ApplicationDbContext _context;
        //словарь со всеми словами из бд
        private HashSet<string> _dictionaryWords;

        //конструктор
        public TextAnalysisService()
        {
            _context = new ApplicationDbContext();
            _dictionaryWords = LoadDictionaryWords();
        }

        //анализ частотности
        public Dictionary<string, int> PerformFrequencyAnalysis(string text)
        {
            //разбивает на слова
            var words = ExtractWords(text);
            //словарь Слово - Колчиество
            var frequencyDict = new Dictionary<string, int>();

            //цикл считае повторы
            foreach (var word in words)
            {
                if (frequencyDict.ContainsKey(word))
                    frequencyDict[word]++;
                else
                    frequencyDict[word] = 1;
            }

            //сортировка на убывание
            return frequencyDict.OrderByDescending(x => x.Value)
                              .ToDictionary(x => x.Key, x => x.Value);
        }

        //проверка орфографии
        public List<SpellingError> CheckSpelling(string text)
        {
            var words = ExtractWords(text);
            //быстрая сортировка
            var sortedWords = QuickSort(words.ToArray());
            //список ошибок
            var errors = new List<SpellingError>();

            //проверка слов
            foreach (var word in sortedWords)
            {
                //если слова нет в словаре
                if (!_dictionaryWords.Contains(word))
                {
                    //исправленный вариант
                    var suggestion = FindClosestWord(word, _dictionaryWords);
                    //добавление в список ошибок
                    errors.Add(new SpellingError
                    {
                        ErrorWord = word,
                        SuggestedCorrection = suggestion
                    });
                }
            }

            return errors;
        }

        //подбор исправление
        private string FindClosestWord(string errorWord, HashSet<string> dictionary)
        {
            if (string.IsNullOrEmpty(errorWord))
                return string.Empty;

            //самое похожее слово
            string closestWord = string.Empty;
            //самое маленькое количество отличий
            int minDistance = int.MaxValue;

            foreach (var correctWord in dictionary)
            {
                //количество ралзичий
                int distance = CalculateLevenshteinDistance(errorWord, correctWord);

                //если количество различий меньше самого маленького и отличий меньше или равно 2, то слово подбирается в похожее
                if (distance < minDistance && distance <= 2) 
                {
                    minDistance = distance;
                    closestWord = correctWord;

                    //если в слове всего одно различие то дальше можно не искать
                    if (distance == 1)
                        break;
                }
            }

            return closestWord ?? string.Empty;
        }

        //вычисление раличий между словами 
        private int CalculateLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
            if (string.IsNullOrEmpty(b))
                return a.Length;

            //создание матрицы
            int[,] matrix = new int[a.Length + 1, b.Length + 1];

            //заполнение матрицы
            for (int i = 0; i <= a.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= b.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,     
                            matrix[i, j - 1] + 1),   
                        matrix[i - 1, j - 1] + cost   
                    );
                }
            }

            return matrix[a.Length, b.Length];
        }

        //текст в список слов
        private List<string> ExtractWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            //поиск слов с игнором регистра, "\b[а-яё]+\b" шаблон слов, идет одна буква от а до я/ё и больше
            var matches = Regex.Matches(text, @"\b[а-яё]+\b", RegexOptions.IgnoreCase);
            //возврат коллекции совпадений 
            return matches.Cast<Match>()
                .Where(m => m.Success)
                         //к нижнему регистру
                         .Select(m => m.Value.ToLower())
                         //слова только с длиной больше 2 включительно
                         .Where(w => w.Length > 1)
                         //готовый список слов
                         .ToList();
        }

        private string[] QuickSort(string[] array)
        {
            if (array.Length <= 1) return array;

            //сортировка массива
            QuickSort(array, 0, array.Length - 1);
            return array;
        }

        //быстрая сортировка
        private void QuickSort(string[] array, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(array, left, right);
                //сортировка левой части масива
                QuickSort(array, left, pivotIndex - 1);
                //сортировка правой части массива
                QuickSort(array, pivotIndex + 1, right);
            }
        }

        private int Partition(string[] array, int left, int right)
        {
            string pivot = array[right];
            //граница между тем что слева от опорного слова и тем что справа
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                //если текущее слово стоит по алфавиту раньше опорного или равно еик то оно должно быть слева
                if (string.Compare(array[j], pivot, StringComparison.Ordinal) <= 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }

            //ставим опорное слово на правильное место
            Swap(array, i + 1, right);
            return i + 1;
        }

        private void Swap(string[] array, int i, int j)
        {
            (array[i], array[j]) = (array[j], array[i]);
        }

        private HashSet<string> LoadDictionaryWords()
        {
            //слова из базы данных
            var list = _context.Words
                               //только текст
                               .Select(w => w.Text)
                               //отключение от sql
                               .AsEnumerable()
                               //если нулл то пустая строка, убираем пробелы по краям слов, делаем нижний регистр
                               .Select(s => (s ?? string.Empty).Trim().ToLowerInvariant())
                               //оставляем только не пустые строки
                               .Where(s => s.Length > 0)
                               //удаление повторов
                               .Distinct()
                               //создаем список из слов
                               .ToList();
            //загрузка списка слов, приравнивание к нижнему регистру
            return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        }
    }
}