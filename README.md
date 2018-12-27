# Dictionary CSV Extensions
The extension for the Dictionary type allows you to work directly with CSV files as a database.

## Описание

Расширение основанное на рефлексии и атрибутах классов для типа Dictionary позволяющее работать напрямую с CSV  файлами как с базой данных. Ознакомиться с работой расширения можно собрав [тестовый пример](CsvDictionaryTest.cs).

- Компактный, достаточно добавить один файл в проект.
- Не имеет зависимостей, должен собираться с любой версией `.NET`, `Mono`.
- Удобен для тестовой загрузки/выгрузки данных из типа Dictionary.
- Основывается на рефлексии, не требует дополнительного описания типов.
- В один экземпляр Dictionary можно в разное время загружать различные классы данных, при совпадении типа ключа создание нового экземпляра не требуется.
- Понимает атрибуты указанные в классе данных:
  - Атрибут `Key` указывает на поле являющееся ключем, ключь может быть только один.
  - Атрибут `Index` задает порядок заполнения данных __из__ `csv` файла.
  - Атрибут `Ignore` указывает на игнорируемое поле при сохранении и загрузки файла.
  - Атрибуты не являются обязательными, по умолчанию ключем является первое поле `csv` файла, и первый элемент в описании класса данных. Тип элемента назначенного ключем долежн совпадать с типом ключа указанным при инициализации Dictionary.
  - При указании атрибута `Index`, необходимо описать индексы во всех полях класса.
- Поддерживает формат поля `byte[] array`, данные сохраняет в HEX формате, имеет собственный конвертор.
- Поддерживает вложенные типы данных с типами `Type List<>`, `IEnumerable<>`, сохраняется в отдельных файлах, имя формируется из названия файла `root file. + field name + .csv` - дополняется именем параметра.
 
  
### Пример класса данных

```c#
        public class TestData : CSVPropertyMapMethod
        {
            // Формат атрибутов:
            // имя параметра, ключь, индекс - (nameof(element), true/false, int >= 0)
            // имя параметра, индекс, ключь - (nameof(element), int >= 0, true/false)
            // имя параметра, ключь - (nameof(element), true/false)
            // имя параметра, индекс - (nameof(element), int >= 0)
            // имя параметра, ключь, игнорировать - (nameof(element), false, true)
            // имя параметра - (nameof(element)) значения атрибутов по умолчанию
            
            // поле помеченное как ключь с индексом 0
            [CSVClassMap(nameof(Id), true, 0)]
            public String Id { get; set; }
            [CSVClassMap(nameof(Name), false, 1)]
            public String Name { get; set; }
            [CSVClassMap(nameof(Age), false, 2)]
            public int Age { get; set; }
            [CSVClassMap(nameof(HappyDay), false, 3)]
            public DateTime HappyDay { get; set; }
            // поле имеющее тип byte[] array
            [CSVClassMap(nameof(BytesArray), false, 4)]
            public byte[] BytesArray { get; set; }
            // игнорировать поле
            [CSVClassMap(nameof(IgnoreData), false, true)]
            public ulong IgnoreData { get; set; }
            // сохраняется в отдельных файле, с суфиксом .StringArray.csv
            [CSVClassMap(nameof(StringArray), false, 5)]
            public List<String> StringArray { get; set; }
        }
```

### Пример инициализации CsvDictionary

```c#
        // При инициализации указывается тип ключа
        CsvDictionary<String> dic = new CsvDictionary<String>();
        // При использовании метода `.Load<Type>()` - Dictionary всегда очищается от предыдущих данных
        // по умолчанию, без параметров, будет попытка загрузить файл с именем класса и расширением `.csv`
```
 
 Метод __Load(..)__
 
 ```c#
        dic.Load<TestData>();
        // или с указанием файла
        dic.Load<TestData>("TestData.csv");
        // или с прямым указанием типа
        dic.Load(Type);
        // или с указанием типа и именем файла
        dic.Load(Type, "TestData.csv");
  ```
  
  Метод __Add(..)__
  
  ```c#
        // Добавление класса данных без явного указания ключа в диктонарий
        // перегрузка метода `.Add()` с одним параметром, классом данных,
        // ключь выбирается автоматически
        dic.Add(new TestData() {
           Id = "abc" ,
           Name = "Ivan",
           Age = 22,
           HappytDay = DateTime.Now
        });        
```

Метод __AddOrUpdate(..)__

```c#
        // Добавление или обновление класса данных по ключу в диктонарий
        AddOrUpdate("def", new TestData() {
           Id = "def" ,
           Name = "Ivona",
           Age = 33,
           HappytDay = DateTime.Now
        });
```

Метод __TryGetValue(..)__

```c#
        // Поиск данных по ключу в диктонарий
        // перегрузка метода `.TryGetValue()` с типом класса данных
        TestData xdata;
        if (TryGetValue<TestData>("abc", out xdata))
```

Метод __Save(..)__

```c#
        // по умолчанию, без параметров, файл будет сохранен с именем класса и расширением `.csv`
        dic.Save();
        // или с указанием файла
        dic.Save("TestData.csv");
```

Метод __Flush()__

```c#
        // перезагрузка данных с сохранением, в соответствии с установленными параметрами:
        // `IsHeader`, `IsStrict`, `IsLoadChildren`, `EncodingFile`
        // имя `.csv` файла данных берется из предварительно сформированного при создании
        // или загрузки `root file .csv` или из названия класса данных
        dc.Flush();
```

### Параметры

```c#

        bool IsHeader  // учитывать наличие заголовока в csv файле / записывать заголовок
        bool IsStrict  // применить strict режим при загрузке csv файла
                       // сопоставлять количество элементов прочитанных из csv файла
                       // с количеством полей заполняемого класса
        bool IsTrim    // обрезать лишние пробелы при чтении csv файла
        bool IsAddDate // добавлять дату внесенных изменений в IEnumerable файлы базы,
                       // Logging mode - можно использовать вместо лога
        bool IsLoadChildren // загружать дочерние IEnumerable файлы базы
        uint AutoFlush // переодически сохранять и очищать IEnumerable элементы после сохранения,
                       // параметр задается в секундах
        uint LineSkip  // пропустить указанное количество строк при загрузке из csv файла
        char Separator // задать разделитель данных в csv файле
        Encoding EncodingFile // кодировка csv файлов, по умолчанию UTF8

```

## License

_MIT_
