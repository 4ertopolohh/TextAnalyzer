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
        private readonly ApplicationDbContext _context;
        private HashSet<string> _dictionaryWords;

        public TextAnalysisService()
        {
            _context = new ApplicationDbContext();
            _dictionaryWords = LoadDictionaryWords();
        }

        public Dictionary<string, int> PerformFrequencyAnalysis(string text)
        {
            var words = ExtractWords(text);
            var frequencyDict = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (frequencyDict.ContainsKey(word))
                    frequencyDict[word]++;
                else
                    frequencyDict[word] = 1;
            }

            return frequencyDict.OrderByDescending(x => x.Value)
                              .ToDictionary(x => x.Key, x => x.Value);
        }

        public List<SpellingError> CheckSpelling(string text)
        {
            var words = ExtractWords(text);
            var sortedWords = QuickSort(words.ToArray());
            var errors = new List<SpellingError>();

            foreach (var word in sortedWords)
            {
                if (!_dictionaryWords.Contains(word))
                {
                    var suggestion = FindClosestWord(word, _dictionaryWords);
                    errors.Add(new SpellingError
                    {
                        ErrorWord = word,
                        SuggestedCorrection = suggestion
                    });
                }
            }

            return errors;
        }

        private string FindClosestWord(string errorWord, HashSet<string> dictionary)
        {
            if (string.IsNullOrEmpty(errorWord))
                return string.Empty;

            string closestWord = string.Empty;
            int minDistance = int.MaxValue;

            foreach (var correctWord in dictionary)
            {
                int distance = CalculateLevenshteinDistance(errorWord, correctWord);

                if (distance < minDistance && distance <= 2) 
                {
                    minDistance = distance;
                    closestWord = correctWord;

                    if (distance == 1)
                        break;
                }
            }

            return closestWord ?? string.Empty;
        }

        private int CalculateLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
            if (string.IsNullOrEmpty(b))
                return a.Length;

            int[,] matrix = new int[a.Length + 1, b.Length + 1];

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

        private List<string> ExtractWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var matches = Regex.Matches(text, @"\b[а-яё]+\b", RegexOptions.IgnoreCase);
            return matches.Cast<Match>()
                         .Select(m => m.Value.ToLower())
                         .Where(w => w.Length > 1)
                         .ToList();
        }

        private string[] QuickSort(string[] array)
        {
            if (array.Length <= 1) return array;

            QuickSort(array, 0, array.Length - 1);
            return array;
        }

        private void QuickSort(string[] array, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(array, left, right);
                QuickSort(array, left, pivotIndex - 1);
                QuickSort(array, pivotIndex + 1, right);
            }
        }

        private int Partition(string[] array, int left, int right)
        {
            string pivot = array[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (string.Compare(array[j], pivot, StringComparison.Ordinal) <= 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }

            Swap(array, i + 1, right);
            return i + 1;
        }

        private void Swap(string[] array, int i, int j)
        {
            (array[i], array[j]) = (array[j], array[i]);
        }

        private HashSet<string> LoadDictionaryWords()
        {
            var list = _context.Words
                               .Select(w => w.Text)
                               .AsEnumerable()
                               .Select(s => (s ?? string.Empty).Trim().ToLowerInvariant())
                               .Where(s => s.Length > 0)
                               .Distinct()
                               .ToList();

            return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        }
    }
}