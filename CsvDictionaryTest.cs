/*
    MIT License

    Copyright (c) 2018 PS
    GitHub: https://github.com/ClnViewer/Dictionary-CSV-Extensions

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sub license, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

using System;
using System.Collections.Generic;
/// CsvDictionary class
using Extension;

namespace CsvDictionaryTest
{
    public class TestData : CSVPropertyMapMethod
    {
        /// Формат атрибутов для классов данных:
        /// 
        /// Отметить как индексное поле.
        /// [CSVClassMapAttribute(nameof(element), true, 0)]
        /// 
        /// Игнорировать поле - не включать его в сохраняемый список
        /// и пропускать при загрузке.
        /// [CSVClassMapAttribute(nameof(element), false, true)]
        /// 
        /// Формат атрибутов:
        /// имя параметра, ключь, индекс - (nameof(element), true/false, int >= 0)
        /// имя параметра, индекс, ключь - (nameof(element), int >= 0, true/false)
        /// имя параметра, ключь - (nameof(element), true/false)
        /// имя параметра, ключь, игнорировать - (nameof(element), false, true)
        /// имя параметра, индекс - (nameof(element), int >= 0)
        /// имя параметра - (nameof(element)) значения атрибутов по умолчанию

        [CSVClassMap(nameof(Id), true, 0)]
        public String Id { get; set; }
        [CSVClassMap(nameof(Name), false, 1)]
        public String Name { get; set; }
        [CSVClassMap(nameof(Age), false, 2)]
        public int Age { get; set; }
        [CSVClassMap(nameof(HappyDay), false, 3)]
        public DateTime HappyDay { get; set; }
        [CSVClassMap(nameof(BytesArray), false, 4)]
        public byte[] BytesArray { get; set; }
        [CSVClassMap(nameof(IgnoreData), false, true)]
        public ulong IgnoreData { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestData td = new TestData()
            {
                Id = "1234567890",
                Name = "DESKTOP-123",
                Age = 22,
                HappyDay = DateTime.Now,
                BytesArray = new byte[] { 0x11, 0x12, 0x13, 0x14 },
                IgnoreData = 12345
            };

            CsvDictionary<String> csvd = new CsvDictionary<String>();
            csvd.Add(td);
            csvd.Save();

            csvd.Load<TestData>();
            TestData tdItem = null;
            bool ret = csvd.TryGetValue<TestData>("1234567890", out tdItem);

            Console.WriteLine("return {0}: {1}", ret, tdItem.ToString());
        }
    }
}
