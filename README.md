# Dictionary CSV Extensions
The extension for the Dictionary type allows you to work directly with CSV files as a database.

## Описание

Расширение основанное на рефлексии и атрибутах классов для типа Dictionary позволяющее работать напрямую с CSV  файлами как с базой данных.

- Компактный, достаточно добавить один файл в проект.
- Не имеет зависимостей, должен собираться с любой версией `.NET`, `Mono`.
- Удобен для тестовой загрузки/выгрузки данных из типа Dictionary.
- Основывается на рефлексии, не требует дополнительного описания типов.
- В один экземпляр Dictionary можно в разное время загружать различные классы данных, при совпадении типа ключа создание нового экземпляра не требуется.
- Понимает атрибуты указанные в классе данных:
  - Атрибут `Key` указывает на поле являющееся ключем, ключь может быть только один.
  - Атрибут `Index` задает порядок заполнения данных __из__ `csv` файла.
  - Атрибуты не являются обязательными, по умолчанию ключем является первое поле `csv` файла, и первый элемент в описании класса данных. Тип элемента назначенного ключем долежн совпадать с типом ключа указанным при инициализации Dictionary.
  - При указании атрибута `Index`, необходимо описать индексы во всех полях класса.
  
  
### Пример класса данных

```c#
        public class TestData : CSVPropertyMapMethod
        {
            // Формат атрибутов:
            // имя параметра, ключь, индекс - (nameof(element), true/false, int >= 0)
            // имя параметра, индекс, ключь - (nameof(element), int >= 0, true/false)
            // имя параметра, ключь - (nameof(element), true/false)
            // имя параметра, индекс - (nameof(element), int >= 0)
            // имя параметра - (nameof(element)) значения атрибутов по умолчанию
            [CSVClassMapAttribute(nameof(Id), true, 0)]
            public String Id { get; set; }
            [CSVClassMapAttribute(nameof(Name), false, 1)]
            public String Name { get; set; }
            [CSVClassMapAttribute(nameof(Age), false, 2)]
            public int Age { get; set; }
            [CSVClassMapAttribute(nameof(HappyDay), false, 3)]
            public DateTime HappyDay { get; set; }
        }
```

### Пример инициализации Dictionary

```c#

        // При инициализации указывается тип ключа
        CsvDictionary<String> dic = new CsvDictionary<String>();
        // При использовании метода `.Load<Type>()` - Dictionary всегда очищается от предыдущих данных
        // по умолчанию, без параметров, будет попытка загрузить файл с именем класса и расширением `.csv`
        dic.Load<TestData>();
        // или с указанием файла
        dic.Load<TestData>("TestData.csv");
        // Добавление класса данных без явного указания ключа в диктонарий
        // перегрузка метода `.Add()` с одним параметром, классом данных,
        // ключь выбирается автоматически
        dic.Add(new TestData() {
           Id = "abc" ,
           Name = "Ivan",
           Age = 22,
           HappytDay = DateTime.Now
        });
        // Добавление или обновление класса данных по ключу в диктонарий
        AddOrUpdate("abc",new TestData() {
           Id = "abc" ,
           Name = "Ivan",
           Age = 22,
           HappytDay = DateTime.Now
        });
        // Поиск данных по ключу в диктонарий
        // перегрузка метода `.TryGetValue()` с типом класса данных,
        // отдельная реализация для `Mono`
        TestData xdata;
        if (TryGetValue<TestData>("abc", out xdata))
           ...
        // по умолчанию, без параметров, файл будет сохранен с именем класса и расширением `.csv`
        dic.Save();
        // или с указанием файла
        dic.Save("TestData.csv");
```

### Параметры

```c#

        bool IsHeader  // учитывать наличие заголовока в csv файле / записывать заголовок
        bool IsStrict  // применить strict режим при загрузке csv файла
                       // сопоставлять количество элементов прочитанных из csv файла
                       // с количеством полей заполняемого класса
        bool IsTrim    // обрезать лишние пробелы при чтении csv файла
        uint LineSkip  // пропустить указанное количество строк при загрузке из csv файла
        char Separator // задать разделитель данных в csv файле

```

## License

_MIT_
