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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// Public methods:
/// 
///    CsvDictionary<String> = new CsvDictionary<String>();
/// 
///    void   Add(object obj);
///    uint   Load<ClassType>(string fname);
///    bool   Save(string fname);
/// </summary>

namespace Extensions
{

    /// <summary>
    /// класс атрибутов для классов данных
    /// 
    /// [CSVClassMapAttribute(nameof(element), true, 0)]
    /// 
    /// Формат атрибутов:
    /// имя параметра, ключь, индекс - (nameof(element), true/false, int >= 0)
    /// имя параметра, индекс, ключь - (nameof(element), int >= 0, true/false)
    /// имя параметра, ключь - (nameof(element), true/false)
    /// имя параметра, индекс - (nameof(element), int >= 0)
    /// имя параметра - (nameof(element)) значения атрибутов по умолчанию
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CSVClassMapAttribute : Attribute
    {
        public string CsvName { get; private set; }
        public bool CsvKey { get; private set; }
        public int CsvIndex { get; private set; }

        public CSVClassMapAttribute(string name)
        {
            __CSVClassMapAttribute(name, false, -1);
        }
        public CSVClassMapAttribute(string name, bool key)
        {
            __CSVClassMapAttribute(name, key, -1);
        }
        public CSVClassMapAttribute(string name, int index)
        {
            __CSVClassMapAttribute(name, false, index);
        }
        public CSVClassMapAttribute(string name, bool key, int index)
        {
            __CSVClassMapAttribute(name, key, index);
        }
        public CSVClassMapAttribute(string name, int index, bool key)
        {
            __CSVClassMapAttribute(name, key, index);
        }
        private void __CSVClassMapAttribute(string name, bool key, int index)
        {
            CsvName = ((name != null) ? name : CsvName);
            CsvKey = key;
            CsvIndex = index;
        }

        /// <summary>
        /// Class CSVClassMapAttribute Helper
        /// </summary>
        /// <param name="isFormat"></param>
        /// <returns></returns>
        public string ToString(bool isFormat = true)
        {
            return string.Format(
                "\tAttr Nane: {}" + ((isFormat) ? Environment.NewLine : ", ") +
                "\tCSV Primary key: {}" + ((isFormat) ? Environment.NewLine : ", ") +
                "\tCSV Index: {}" + ((isFormat) ? Environment.NewLine : ", "),
                CsvName, CsvKey, CsvIndex
            );
        }
    }

    /// <summary>
    /// класс управления атрибутами для классов данных
    /// наследуемый тип
    /// </summary>
    public abstract class CSVPropertyMapMethod
    {
        public virtual CSVClassMapAttribute FindAttr(string name, bool isNull = false)
        {
            PropertyInfo[] pi = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in pi)
            {
                CSVClassMapAttribute attr = p.GetCustomAttributes(true)
                    .Where(c => c.GetType() == typeof(CSVClassMapAttribute))
                    .Cast<CSVClassMapAttribute>()
                    .FirstOrDefault();

                if (
                    (attr != null) &&
                    (!string.IsNullOrWhiteSpace(attr.CsvName)) &&
                    (attr.CsvName.Equals(name))
                   )
                    return new CSVClassMapAttribute(name, attr.CsvKey, attr.CsvIndex);
            }
            return ((isNull) ?
                null :
                new CSVClassMapAttribute(name, false, -1)
            );
        }
    }

    /// <summary>
    /// Расширение для класса Dictionary
    /// позволяющее загружать и сохранять данные на диск в формате csv файла
    /// </summary>
    /// <typeparam name="Tkey">тип ключа</typeparam>
    /// <example>
    ///         public class User : CSVPropertyMapMethod
    ///         {
    ///             // Формат атрибутов:
    ///             //         Name = (string)(nameof(Field)),
    ///             //         Primary Key = (bool)(true/false),
    ///             //         Index = (int)(0 .. max)
    ///             [CSVClassMapAttribute("Id", false, 1)]
    ///             public String Id { get; set; }
    ///             [CSVClassMapAttribute("Name", true, 0)]
    ///             public String Name { get; set; }
    ///             [CSVClassMapAttribute("Age", true, 2)]
    ///             public String Age { get; set; }
    ///         }
    /// </example>
    public class CsvDictionary<Tkey> : Dictionary<Tkey, Object>
    {
        private string __fname = null;
        private char[] __escapeChars = new[] { '|', '\'', '\n', '\r' };

        /// <summary>
        /// учитывать заголовок в csv файле
        /// </summary>
        public bool IsHeader { get; set; }
        /// <summary>
        /// применить strict режим при загрузке csv файла
        /// сопоставлять количество элементов прочитанных из csv файла
        /// с количеством полей заполняемого класса
        /// </summary>
        public bool IsStrict { get; set; }
        /// <summary>
        /// обрезать лишние пробелы при чтении csv файла
        /// </summary>
        public bool IsTrim { get; set; }
        /// <summary>
        /// пропустить указанное количество строк при загрузке из csv файла
        /// </summary>
        public uint LineSkip { get; set; }
        /// <summary>
        /// задать разделитель данных в csv файле
        /// </summary>
        public char Separator
        {
            get
            {
                return __escapeChars[0];
            }
            set
            {
                if (value != __escapeChars[0])
                    __escapeChars[0] = value;
            }
        }

        private string __GetTypeName(Type t)
        {
            return String.Format("{0}.csv", t.Name);
        }

        private bool __EmptyFileName(string fname, Type t = null)
        {
            __fname = ((fname != null) ? fname :
                ((__fname != null) ? __fname :
                   ((t != null) ? __GetTypeName(t) : null)
                )
            );
            return String.IsNullOrWhiteSpace(__fname);
        }

        public CsvDictionary() : base()
        {
            IsHeader = true;
            IsStrict = true;
            IsTrim = true;
            LineSkip = 0;
        }

        /// <summary>
        /// Добавление данных в диктонарий
        /// автоматический выбор ключа на основании атрибутов
        /// </summary>
        /// <param name="obj">тип класса данных</param>
        public void Add(object obj)
        {
            if (obj == null)
                return;

            int index = -1;
            Type t = obj.GetType();
            PropertyInfo[] p = t.GetProperties();
            CSVClassMapAttribute res;
            CSVPropertyMapMethod mmt = obj as CSVPropertyMapMethod;

            for (int i = 0; i < p.Length; i++)
            {
                if ((res = mmt.FindAttr(p[i].Name, true)) == null)
                    continue;

                if (!res.CsvKey)
                    continue;

                index = ((res.CsvIndex == -1) ? i : res.CsvIndex);
                break;
            }

            index = ((index == -1) ? 0 : index);
            Object o = p[index].GetValue(obj, null);
            
            if (o != null)
                Add((Tkey)o, obj);
        }

        /// <summary>
        /// Загруза фйла csv в диктонарий
        /// Если имя файла не указано, то имя формируется из имени класса <T>
        /// </summary>
        /// <typeparam name="T">имя класса данных</typeparam>
        /// <param name="fname">имя файла csv</param>
        /// <returns>количество загруженных строк</returns>
        public uint Load<T>(string fname = null) where T : class, new()
        {
            Type t = typeof(T);
            uint cnt = 0;

            if (__EmptyFileName(fname))
            {
                if (__EmptyFileName(null, t))
                    return cnt;
            }

            if (!File.Exists(__fname))
                return cnt;

            if (Count != 0)
                Clear();

            PropertyInfo[] p = t.GetProperties();
            int nele = p.Length;

            using (StreamReader rd = new StreamReader(__fname, Encoding.UTF8))
            {
                string line;

                while ((line = rd.ReadLine()) != null)
                {
                    cnt++;

                    if (
                        (LineSkip > cnt) ||
                        ((cnt == 1) && (IsHeader)) ||
                        (String.IsNullOrWhiteSpace(line))
                       )
                        continue;

                    string[] part = line.Split(__escapeChars[0]);

                    if ((IsStrict) && (part.Length != nele))
                        continue;

                    T obj = new T();
                    Object Id = null;
                    CSVClassMapAttribute res;
                    CSVPropertyMapMethod mmt = obj as CSVPropertyMapMethod;

                    for (int i = 0; ((i < part.Length) && (i < nele)); i++)
                    {
                        try
                        {
                            string str;

                            if (
                                ((res = mmt.FindAttr(p[i].Name, true)) != null) &&
                                (res.CsvIndex > -1) &&
                                (res.CsvIndex < part.Length)
                               )
                                str = part[res.CsvIndex];
                            else
                                str = part[i];

                            if (String.IsNullOrWhiteSpace(str))
                            {
                                if (i == 0)
                                    break;
                                else
                                    continue;
                            }
                            if (IsTrim)
                                str.Trim();
                            if (str.StartsWith("\"") && str.EndsWith("\""))
                                str = str.Substring(1, str.Length - 2).Replace("\"\"", "\"");

                            p[i].SetValue(obj, Convert.ChangeType(str, p[i].PropertyType), null);

                            if (
                                (i == 0) ||
                                ((res != null) && (res.CsvKey))
                               )
                                Id = p[i].GetValue(obj, null);
                        }
                        catch (Exception)
                        {
                            Id = null;
                        }
                    }

                    if (Id != null)
                        Add((Tkey)Id, obj);
                }
            }
            return ((IsHeader) ? ((cnt > 0) ? (cnt - 1) : 0) : cnt);
        }

        /// <summary>
        /// Сохранение диктонария в фйле csv
        /// Если имя файла не указано, то имя формируется из данных класса в диктонарии
        /// </summary>
        /// <param name="fname">имя файла csv</param>
        /// <returns>bool</returns>
        public bool Save(string fname = null)
        {
            bool __IsFileName = __EmptyFileName(fname);

            if (Count == 0)
                return false;

            StringBuilder sb = new StringBuilder();

            /* write Header || not file name */
            if (IsHeader || __IsFileName)
            {
                KeyValuePair<Tkey, object> e = this.ElementAt(0);
                Type t = e.Value.GetType();

                if (__IsFileName)
                {
                    if (__EmptyFileName(null, t))
                        return false;
                }

                if (IsHeader)
                {
                    PropertyInfo[] p = t.GetProperties();

                    for (int i = 0; i < p.Length; i++)
                    {
                        sb.Append(p[i].Name);
                        if (i < p.Length - 1)
                            sb.Append(__escapeChars[0]);
                    }
                    sb.Append(__escapeChars[2]);
                }
            }
            foreach (KeyValuePair<Tkey, object> e in this)
            {
                Type t = e.Value.GetType();
                PropertyInfo[] p = t.GetProperties();

                for (int i = 0; i < p.Length; i++)
                {
                    sb.Append(p[i].GetValue(e.Value, null));
                    if (i < p.Length - 1)
                        sb.Append(__escapeChars[0]);
                }
                sb.Append(__escapeChars[2]);
            }
            File.WriteAllText(__fname, sb.ToString());
            return true;
        }
    }
}

