# Dictionary CSV Extensions
The extension for the Dictionary type allows you to work directly with CSV files as a database.

## Описание

Расширение основанное на рефлексии и атрибутах классов для типа Dictionary позволяющее работать напрямую с CSV  файлами как с базой данных.

- Компактный
- Удобен для тестовой загрузки/выгрузки данных из типа Dictionary.
- Основывается на рефлексии, не требует дополнительного описания типов.
- Понимает атрибуты указанные в классе данных:
  - Атрибут `Key` указывает на поле являющееся ключем.
  - Атрибут `Index` задает порядок заполнения данных __из__ `csv` файла.
  - Атрибуты не являются обязательными, по умолчанию ключем является первое поле `csv` файла, и первый элемент в описании класса данных. Тип элемента долежн совпадать с ключем указанным при инициализации Dictionary.
  
  
### Пример класса данных

```c#
        public class TestData : CSVPropertyMapMethod
        {
            [CSVClassMapAttribute(nameof(Id), true, 0)]
            public String Id { get; set; }
            [CSVClassMapAttribute(nameof(Name), false, 1)]
            public String Name { get; set; }
            [CSVClassMapAttribute(nameof(Age), false, 2)]
            public int Age { get; set; }
            [CSVClassMapAttribute(nameof(HappytDay), false, 3)]
            public DateTime HappytDay { get; set; }
        }
```

### Пример инициализации Dictionary

```c#

        // при инициализации указывается тип ключа
        CvsDictionary<String> dic = new CvsDictionary<String>();
        // при использовании метода `.Load<Type>()` - Dictionary всегда очищается от предыдущих данных
        // по умолчанию, без параметров, будет попытка загрузить файл с именем класса и расширением `.csv`
        dic.Load<TestData>();
        // или с указанием файла
        dic.Load<TestData>("TestData.csv");
        // перегрузка метода `.Add()` с одним параметром, классом данных,
        // ключь выбирается автоматически
        dic.Add(new TestData() {
           Id = "abc" ,
           Name = "Ivan",
           Age = 22,
           HappytDay = DateTime.Now
        });
        // по умолчанию, без параметров, файл будет сохранен с именем класса и расширением `.csv`
        dic.Save();
        // или с указанием файла
        dic.Save("TestData.csv");
```

## License

_MIT_
