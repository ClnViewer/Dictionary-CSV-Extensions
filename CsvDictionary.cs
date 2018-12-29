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
using System.Threading.Tasks;

/// <summary>
/// Public methods:
/// 
///    CsvDictionary<String> = new CsvDictionary<String>();
/// 
///    void   Add(object obj);
///    void   AddOrUpdate(Object obj);
///    void   AddOrUpdate(Tkey id, Object obj);
///    bool   Replace(Object obj);
///    bool   Replace(Tkey id, Object obj);
///    bool   TryGetValue<T>(Tkey id, out T xobj);
///    uint   Load<ClassType>(string fname);
///    uint   Load(Type ClassType, string fname);
///    bool   Save(string fname);
///    bool   FlushAndReload();

namespace Extension
{

    /// <summary>
    /// класс атрибутов для классов данных
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
    /// </summary>
    #region CSV Map Attribute
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CSVClassMapAttribute : Attribute
    {
        public string CsvName { get; private set; }
        public bool CsvKey { get; private set; }
        public bool CsvIgnore { get; private set; }
        public int CsvIndex { get; private set; }

        /// <summary>
        /// метод поля - (имя параметра)
        /// добавляет атрибут с параметрами значений по умолчанию
        /// </summary>
        public CSVClassMapAttribute(string name)
        {
            __CSVClassMapAttribute(name, false, false, -1);
        }
        /// <summary>
        /// метод поля - (имя параметра, ключь)
        /// </summary>
        public CSVClassMapAttribute(string name, bool key)
        {
            __CSVClassMapAttribute(name, key, false, -1);
        }
        /// <summary>
        /// метод поля - (имя параметра, индекс)
        /// </summary>
        public CSVClassMapAttribute(string name, int index)
        {
            __CSVClassMapAttribute(name, false, false, index);
        }
        /// <summary>
        /// метод поля - (имя параметра, ключь, индекс)
        /// </summary>
        public CSVClassMapAttribute(string name, bool key, int index)
        {
            __CSVClassMapAttribute(name, key, false, index);
        }
        /// <summary>
        /// метод поля - (имя параметра, индекс, ключь)
        /// </summary>
        public CSVClassMapAttribute(string name, int index, bool key)
        {
            __CSVClassMapAttribute(name, key, false, index);
        }
        /// <summary>
        /// метод для игнорирования поля - (nameof(element), false, true)
        /// </summary>
        public CSVClassMapAttribute(string name, bool key, bool ignore)
        {
            __CSVClassMapAttribute(name, key, ignore, -1);
        }
        /// <summary>
        /// метод для вызова из CSVPropertyMapMethod.FindAttr
        /// </summary>
        public CSVClassMapAttribute(string name, bool key, bool ignore, int index)
        {
            __CSVClassMapAttribute(name, key, ignore, index);
        }
        /// <summary>
        /// общий приватный метод
        /// </summary>
        private void __CSVClassMapAttribute(string name, bool key, bool ignore, int index)
        {
            CsvName = ((name != null) ? name : CsvName);
            CsvKey = key;
            CsvIndex = index;
            CsvIgnore = ignore;
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
    #endregion

    /// <summary>
    /// класс управления атрибутами для классов данных
    /// наследуемый тип
    /// </summary>
    #region CSV Property Map Attribyte
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
                    return new CSVClassMapAttribute(name, attr.CsvKey, attr.CsvIgnore, attr.CsvIndex);
            }
            return ((isNull) ?
                null :
                new CSVClassMapAttribute(name, false, false, -1)
            );
        }
    }
    #endregion

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
    #region Csv Dictionary main class
    public class CsvDictionary<Tkey> : Dictionary<Tkey, Object>
    {
        private Task __t = null;
        private string __fname = null;
        private char[] __escapeChars = new[] { '|', '\'', '\n', '\r' };
        private static readonly object __lock = new object();

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
        /// добавлять дату внесенных изменений в IEnumerable файлы базы
        /// Logging mode - можно использовать вместо лога
        /// </summary>
        public bool IsAddDate { get; set; }
        /// <summary>
        /// загружать дочерние IEnumerable файлы базы
        /// </summary>
        public bool IsLoadChildren { get; set; }
        /// <summary>
        /// автоматически сохранять даные при добавлении/измении/удалении в файл
        /// </summary>
        public bool IsAutoSave { get; set; }
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
        /// <summary>
        /// текущее имя файла
        /// </summary>
        public string FileName
        {
            get
            {
                return __fname;
            }
            set
            {
                if (value != __fname)
                    __fname = value;
            }
        }
        /// <summary>
        /// кодировка csv файлов, по умолчанию UTF8
        /// </summary>
        public Encoding EncodingFile { get; set; }

        public CsvDictionary() : base()
        {
            IsAddDate = IsHeader = IsStrict = IsTrim = true;
            IsAutoSave = IsLoadChildren = false;
            LineSkip = 0;
            EncodingFile = Encoding.UTF8;
        }
        ~CsvDictionary()
        {
            if (__t != null)
                __t.Wait();
        }

        #region overwrite base methods

        #region global Auto Save prototype

        private void pCsvSaveTask()
        {
            if (__t != null)
                return;

            try
            {
                __t = Task.Factory.StartNew(() =>
                {
                    Save();
                }).ContinueWith((x) =>
                {
                    if (x != null)
                        x.Dispose();
                    __t = null;
                });
            }
            catch (Exception) { }
        }

        private void pCsvOverMethods(Action act)
        {
            if (IsAutoSave)
            {
                lock (__lock)
                {
                    act();
                }
                pCsvSaveTask();
            }
            else
                act();
        }
        #endregion

        public new void Add(Tkey id, object obj)
        {
            Action action = () => base.Add(id, obj);
            pCsvOverMethods(action);
        }

        public new bool Remove(Tkey key)
        {
            Action action = () => base.Remove(key);
            pCsvOverMethods(action);
            return true;
        }

        #endregion

        /// <summary>
        /// Замена обьекта по ключу
        /// </summary>
        /// <param name="id">ключ</param>
        /// <param name="obj">класс данных</param>
        /// <returns>bool</returns>
        public bool Replace(Tkey id, Object obj)
        {
            bool ret = ContainsKey(id);
            if (ret)
            {
                this[id] = obj;

                if (IsAutoSave)
                    pCsvSaveTask();
            }
            return ret;
        }

        /// <summary>
        /// Замена обьекта с автоматическим выбором ключа
        /// </summary>
        /// <param name="obj">класс данных</param>
        /// <returns>bool</returns>
        public bool Replace(object obj)
        {
            Object o = pCvsSelectKey(obj);
            if (o != null)
                return Replace((Tkey)o, obj);
            return false;
        }

        /// <summary>
        /// Добавление или обновление класса данных с автоматическим выбором ключа
        /// </summary>
        /// <param name="obj">класс данных</param>
        public void AddOrUpdate(Object obj)
        {
            if ((Count == 0) || (!Replace(obj)))
                Add(obj);
        }

        /// <summary>
        /// Добавление или обновление класса данных по ключу в диктонарий
        /// </summary>
        /// <param name="id">ключ</param>
        /// <param name="obj">класс данных</param>
        public void AddOrUpdate(Tkey id, Object obj)
        {
            bool ret = ContainsKey(id);
            if (ret)
            {
                this[id] = obj;

                if (IsAutoSave)
                    pCsvSaveTask();
            }
            else
                Add(id, obj);
        }

        /// <summary>
        /// Добавление данных в диктонарий
        /// автоматический выбор ключа на основании атрибутов
        /// </summary>
        /// <param name="obj">тип класса данных</param>
        public void Add(object obj)
        {
            Object o = pCvsSelectKey(obj);
            if (o != null)
                Add((Tkey)o, obj);
        }

        /// <summary>
        /// Поиск данных по ключу в диктонарий
        /// </summary>
        /// <param name="id">тип ключа</param>
        /// <param name="obj">тип класса данных</param>
        public bool TryGetValue<T>(Tkey id, out T xobj) where T : class, new()
        {
            Object obj = null;
            try
            {
                if (Count == 0)
                    return false;

                bool ret = ContainsKey(id);
                if (ret)
                    obj = this[id];
                return ret;
            }
            finally
            {
                xobj = (T)obj;
            }
        }

        /// <summary>
        /// Загруза фйла csv в диктонарий
        /// Если имя файла не указано, то имя формируется из имени класса <T>
        /// </summary>
        /// <typeparam name="T">имя класса данных</typeparam>
        /// <typeparam name="t">тип класса данных</typeparam>
        /// <param name="fname">имя файла csv</param>
        /// <returns>количество загруженных строк</returns>
        public uint Load<T>(string fname = null) where T : class, new()
        {
            return Load(typeof(T), fname);
        }
        public uint Load(Type t, string fname = null)
        {
            lock (__lock)
            {
                if (pCsvIsEmptyFileName(fname))
                {
                    if (pCsvIsEmptyFileName(null, t))
                        return 0;
                }
                return pCsvLoad(t, __fname, null, null);
            }
        }

        /// <summary>
        /// Сохранение диктонария в фйле csv
        /// Если имя файла не указано, то имя формируется из данных класса в диктонарии
        /// </summary>
        /// <param name="fname">имя файла csv</param>
        /// <returns>bool</returns>
        public bool Save(string fname = null)
        {
            if (Count == 0)
                return false;

            lock (__lock)
            {
                bool __IsFileName = pCsvIsEmptyFileName(fname);

                /* not File name */
                if (__IsFileName)
                    if (pCsvIsEmptyFileName(null, this.ElementAt(0).Value.GetType()))
                        return false;

                if (Count == 0)
                    return false;

                return pCsvSaveDictionary<Tkey>(this, __fname, null, null);
            }
        }

        /// <summary>
        /// Flush Dictionary, save and reload
        /// </summary>
        /// <returns>bool</returns>
        public bool FlushAndReload()
        {
            Type t = this.ElementAt(0).Value.GetType();
            if (
                (!Save()) ||
                (Load(t) == 0)
               )
                return false;
            return true;
        }

        #region private Load methods

        private uint pCsvLoad(Type t, string fname, string elename, string id)
        {
            int nele;
            uint cnt = 0;
            PropertyInfo[] p;
            string _fname = pCsvGetChildFileName(fname, elename);

            if (!File.Exists(_fname))
                return cnt;

            if (Count != 0)
                Clear();

            if ((p = t.GetProperties()) == null)
                return 0;

            nele = p.Length;

            using (StreamReader rd = new StreamReader(_fname, EncodingFile))
            {
                string line;
                Object Id = null,
                       obj = Activator.CreateInstance(t);
                CSVClassMapAttribute res = null;
                CSVPropertyMapMethod mmt = obj as CSVPropertyMapMethod;

                while ((line = rd.ReadLine()) != null)
                {
                    cnt++;
                    Id = null;

                    if (
                        (LineSkip > cnt) ||
                        ((cnt == 1) && (IsHeader)) ||
                        (String.IsNullOrWhiteSpace(line))
                       )
                        continue;

                    int cells = 0;
                    string[] part = line.Split(new char[] { __escapeChars[0] }, nele, StringSplitOptions.None);

                    if ((IsStrict) && (part.Length != nele))
                    {
                        for (int i = 0; i < nele; i++)
                        {
                            if (pCvsCheckProperty(p[i], mmt, ref res))
                                continue;
                            cells++;
                        }
                        if (cells == 0)
                            continue;

                        nele = cells;
                        cells = 0;
                    }

                    for (int i = 0, n = 0; ((i < part.Length) && (cells < nele)); i++, n++)
                    {
                        try
                        {
                            string str;
                            res = null;

                            if (n >= p.Length)
                                break;

                            if (pCvsCheckProperty(p[n], mmt, ref res))
                            {
                                --i;
                                continue;
                            }

                            cells++;

                            try
                            {
                                /// Type List<>, IEnumerable<>
                                if (p[n].PropertyType.IsGenericType)
                                {
                                    bool IsIenumerable = false;
                                    Object ie = pCvsGetIEnumerableEle(p[n], ref IsIenumerable);

                                    if (IsIenumerable)
                                    {
                                        if (ie == null)
                                            continue;

                                        p[n].SetValue(obj, ie, null);

                                        if ((res != null) && (res.CsvKey))
                                            Id = p[n].GetValue(obj, null);

                                        continue; // TODO: load data
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            if (
                                (res != null) &&
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

                            if (p[n].PropertyType == typeof(byte[]))
                                p[n].SetValue(obj, pCsvStringToBytes(str), null);
                            else
                                p[n].SetValue(obj, Convert.ChangeType(str, p[n].PropertyType), null);


                            if (
                                (n == 0) ||
                                ((res != null) && (res.CsvKey))
                               )
                                Id = p[n].GetValue(obj, null);
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
        #endregion

        #region private Save methods

        private List<String> pCsvSaveGetHeader(object e)
        {
            List<String> cvsHeader = new List<string>();
            Type t = e.GetType();
            CSVClassMapAttribute res = null;
            PropertyInfo[] p = t.GetProperties();

            for (int i = 0; i < p.Length; i++)
            {
                if (pCvsCheckProperty(p[i], (e as CSVPropertyMapMethod), ref res))
                    continue;

                if (p[i].GetIndexParameters().Length > 0)
                {
                    cvsHeader.Add(t.Name);
                    break;
                }
                else
                    cvsHeader.Add(p[i].Name);
            }
            return cvsHeader;
        }

        private void pCsvSaveGetEle(object e, StringBuilder sb, string fname, string id)
        {
            CSVClassMapAttribute res = null;
            PropertyInfo[] p = e.GetType().GetProperties();

            for (int i = 0; i < p.Length; i++)
            {
                if (pCvsCheckProperty(p[i], (e as CSVPropertyMapMethod), ref res))
                    continue;

                if (i > 0)
                    sb.Append(__escapeChars[0]);

                if (p[i].GetIndexParameters().Length > 0)
                {
                    string raw = e as string;
                    if (!String.IsNullOrWhiteSpace(raw))
                        sb.Append(raw);
                    break;
                }

                object obj;

                try
                {
                    if ((obj = p[i].GetValue(e, null)) == null)
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                if (obj != null)
                {
                    if (obj is IEnumerable<Object>)
                    {
                        try
                        {
                            IEnumerable<Object> ie = (obj as IEnumerable<Object>);
                            if (ie != null)
                            {
                                int cnt;
                                try { cnt = (obj as dynamic).Count; }
                                catch (Exception) { cnt = 1; }

                                if (cnt > 0)
                                {
                                    pCsvSaveIEnumerable(ie, fname, p[i].Name, id);

                                    if (!IsLoadChildren)
                                    {
                                        bool IsIenumerable = false;
                                        Object x = pCvsGetIEnumerableEle(p[i], ref IsIenumerable);

                                        if ((IsIenumerable) && (x != null))
                                            p[i].SetValue(e, x, null);
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    else if (obj.GetType() == typeof(byte[]))
                    {
                        if (((byte[])(obj)).Length > 0)
                            sb.Append(pCsvBytesToString((byte[])obj));
                    }
                    else
                        sb.Append(obj);
                }
            }

        }

        private bool pCsvSaveDictionary<IEkey>(
            IEnumerable<KeyValuePair<IEkey, object>> ie, string fname, string elename, string id
            )
        {
            if (ie.Count() == 0)
                return false;

            List<String> cvsHeader = ((IsHeader) ? pCsvSaveGetHeader(ie.ElementAt(0).Value) : null);
            bool IsWriteId = !String.IsNullOrWhiteSpace(id);
            bool IsWriteDate = (IsAddDate && IsWriteId);
            bool IsAppend = (IsWriteId && !IsLoadChildren);

            using (StreamWriter sw = new StreamWriter(pCsvGetChildFileName(fname, elename), IsAppend, EncodingFile))
            {
                StringBuilder sb = new StringBuilder();
                sw.AutoFlush = true;

                if ((cvsHeader != null) && (cvsHeader.Count > 0) && (sw.BaseStream.Length <= 3))
                {
                    if (IsWriteDate)
                        cvsHeader.Add(typeof(DateTime).ToString());
                    pCvsSaveWriteHeader(cvsHeader, sw, id);
                }

                foreach (KeyValuePair<IEkey, object> e in ie)
                {
                    if (IsWriteId)
                        sb.Append(String.Format("{0}{1}", id.ToString(), __escapeChars[0]));
                    pCsvSaveGetEle(e.Value, sb, fname, e.Key.ToString());
                    if (IsWriteDate)
                        sb.Append(String.Format("{0}{1}", __escapeChars[0], DateTime.Now.ToString()));
                    sb.Append(Environment.NewLine);
                    sw.Write(sb.ToString());
                    sb.Clear();
                }
            }
            return true;
        }

        private bool pCsvSaveIEnumerable(
            IEnumerable<Object> ie, string fname, string elename, string id
            )
        {
            if (ie.Count() == 0)
                return false;

            List<String> cvsHeader = ((IsHeader) ? pCsvSaveGetHeader(ie.ElementAt(0)) : null);
            bool IsWriteId = !String.IsNullOrWhiteSpace(id);
            bool IsWriteDate = (IsAddDate && IsWriteId);
            bool IsAppend = (IsWriteId && !IsLoadChildren);

            using (StreamWriter sw = new StreamWriter(pCsvGetChildFileName(fname, elename), IsAppend, EncodingFile))
            {
                StringBuilder sb = new StringBuilder();
                sw.AutoFlush = true;

                if ((cvsHeader != null) && (cvsHeader.Count > 0) && (sw.BaseStream.Length <= 3))
                {
                    if (IsWriteDate)
                        cvsHeader.Add(typeof(DateTime).ToString());
                    pCvsSaveWriteHeader(cvsHeader, sw, id);
                }

                foreach (var e in ie)
                {
                    if (IsWriteId)
                        sb.Append(String.Format("{0}{1}", id.ToString(), __escapeChars[0]));
                    pCsvSaveGetEle(e, sb, fname, id);
                    if (IsWriteDate)
                        sb.Append(String.Format("{0}{1}", __escapeChars[0], DateTime.Now.ToString()));
                    sb.Append(Environment.NewLine);
                    sw.Write(sb.ToString());
                    sb.Clear();
                }
            }
            return true;
        }

        private void pCvsSaveWriteHeader(List<String> cvsHeader, StreamWriter sw, string id)
        {
            if ((cvsHeader == null) || (cvsHeader.Count == 0))
                return;

            if (!String.IsNullOrWhiteSpace(id))
                sw.Write("Id{0}", __escapeChars[0]);
            sw.WriteLine(string.Join(__escapeChars[0].ToString(), cvsHeader.ToArray()));
        }
        #endregion

        #region private utils

        private string pCsvBytesToString(byte[] data)
        {
            if (data.Length == 0)
                return String.Empty;
            return BitConverter.ToString(data).Replace("-", "");
        }

        private byte[] pCsvStringToBytes(string sdata)
        {
            if (String.IsNullOrWhiteSpace(sdata))
                return new byte[0];

            byte[] bytes = new byte[sdata.Length / 2];
            for (int i = 0; i < sdata.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(sdata.Substring(i, 2), 16);
            return bytes;
        }

        private string pCsvGetChildFileName(string fname, string elename)
        {
            return ((String.IsNullOrWhiteSpace(elename)) ? fname :
                String.Format("{0}.{1}.csv", fname.Substring(0, (fname.Length - 4)), elename)
            );
        }

        private string pCsvGetTypeFileName(Type t)
        {
            return String.Format("{0}.csv", t.Name);
        }

        private bool pCsvIsEmptyFileName(string fname, Type t = null)
        {
            __fname = ((fname != null) ? fname :
                ((__fname != null) ? __fname :
                   ((t != null) ? pCsvGetTypeFileName(t) : null)
                )
            );
            return String.IsNullOrWhiteSpace(__fname);
        }

        private bool pCvsCheckProperty(PropertyInfo p, CSVPropertyMapMethod mmt, ref CSVClassMapAttribute res)
        {
            return (
                    (p == null) ||
                    (!p.CanRead) ||
                    (
                      (mmt != null) &&
                      ((res = mmt.FindAttr(p.Name, true)) != null) &&
                      (res.CsvIgnore)
                    )
                   );
        }

        private Object pCvsGetIEnumerableEle(PropertyInfo p, ref bool IsIenumerable)
        {
            Type[] t0;
            Type t1 = ((p == null) ? null : p.PropertyType),
                 t2, t3;
            Object ie = null;
            IsIenumerable = false;

            if (
                (t1 != null) &&
                (t1.IsGenericType)
               )
            {
                t2 = t1.GetGenericTypeDefinition();

                if ((t2 != null) && (t2 == typeof(IEnumerable<>)))
                    IsIenumerable = true;
                else
                    foreach (Type it in t1.GetInterfaces())
                        if (it.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            IsIenumerable = true;
                            break;
                        }

                if (!IsIenumerable)
                    return ie;

                if (
                    ((t0 = t1.GetGenericArguments()) == null) ||
                    (t0[0] == null) &&
                    (t2 != null)
                   )
                    return ie;

                if ((t2 != null) && (t2 == typeof(IEnumerable<>)))
                    t3 = typeof(List<>).MakeGenericType(t0[0]);
                else if (t2 != null)
                    t3 = t2.MakeGenericType(t0[0]);
                else
                    t3 = t1;

                if (t3 == null)
                    return ie;

                return Activator.CreateInstance(t3);
            }
            return ie;
        }

        private Object pCvsSelectKey(object obj)
        {
            if (obj == null)
                return null;

            int index = -1;
            PropertyInfo[] p = obj.GetType().GetProperties();
            CSVClassMapAttribute res = null;

            for (int i = 0; i < p.Length; i++)
            {
                if (
                    (pCvsCheckProperty(p[i], (obj as CSVPropertyMapMethod), ref res)) ||
                    (res == null) ||
                    (!res.CsvKey)
                   )
                    continue;

                index = ((res.CsvIndex == -1) ? i : res.CsvIndex);
                break;
            }

            index = ((index == -1) ? 0 : index);
            return p[index].GetValue(obj, null);
        }
        #endregion
    }
    #endregion
}

