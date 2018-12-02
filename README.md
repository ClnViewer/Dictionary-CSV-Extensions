# Dictionary CSV Extensions
The extension for the Dictionary type allows you to work directly with CSV files as a database.

## Описание

Расширение основанное на рефлексии и атрибутах классов для типа Dictionary позволяющее работать напрямую с CSV  файлами как с базой данных.

- Компактный
- Удобен для тестовой загрузки/выгрузки данных из типа Dictionary
- Основывается на рефлексии, не требует дополнительного описания типов
- Понимает атрибуты указанные в классе данных
  - Атрибут `Key` указывает на поле являющееся ключом
  - Атрибут `Index` задает порядок заполнения данных _из_ `csv` файла
  - Атрибуты не являются обязательными, по умолчанию ключом является первое поле `csv` файла
  
  
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
