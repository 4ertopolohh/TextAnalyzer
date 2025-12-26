using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer.Models
{
    //элемент таблицы ошибок
    public class SpellingError
    {
        public string ErrorWord { get; set; }
        public string SuggestedCorrection { get; set; }
    }
}