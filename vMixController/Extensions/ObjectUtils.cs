using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Extensions
{

    public static class ObjectCopier
    {
        /// <summary>
        /// Создает глубокую копию переданного объекта.
        /// Поддерживает типы-значения, массивы, коллекции (List, Dictionary),
        /// объекты, реализующие ICloneable, и объекты, которые можно сериализовать в бинарный формат.
        /// </summary>
        /// <param name="originalObject">Объект для копирования.</param>
        /// <returns>Копия объекта.</returns>
        /// <exception cref="ArgumentException">Если объект не может быть скопирован ни одним из поддерживаемых способов.</exception>
        public static object Copy(this object originalObject)
        {
            // 1. Обработка null
            if (originalObject == null)
            {
                return null;
            }

            var type = originalObject.GetType();

            // 2. Копирование типов-значений (struct, int, double, etc.)
            // При передаче в метод они "упаковываются" (boxing), создавая копию.
            if (type.IsValueType)
            {
                return originalObject;
            }

            // 3. Копирование массивов
            if (type.IsArray)
            {
                var originalArray = (Array)originalObject;
                var newArray = (Array)originalArray.Clone(); // Создаем поверхностную копию для структуры массива

                // Рекурсивно копируем каждый элемент для глубокого копирования
                for (int i = 0; i < originalArray.Length; i++)
                {
                    newArray.SetValue(Copy(originalArray.GetValue(i)), i);
                }
                return newArray;
            }

            // 4. Копирование коллекций (словари, списки)
            // Эта проверка должна идти до ICloneable, т.к. многие коллекции реализуют его,
            // но выполняют поверхностное копирование.

            // Для словарей
            if (originalObject is IDictionary originalDict)
            {
                // Создаем экземпляр того же типа словаря
                var newDict = (IDictionary)Activator.CreateInstance(type);
                foreach (DictionaryEntry item in originalDict)
                {
                    // Рекурсивно копируем и ключ, и значение
                    newDict.Add(Copy(item.Key), Copy(item.Value));
                }
                return newDict;
            }

            // Для списков и других коллекций, реализующих IList
            if (originalObject is IList originalList)
            {
                // Создаем экземпляр того же типа списка
                var newList = (IList)Activator.CreateInstance(type);
                foreach (var item in originalList)
                {
                    // Рекурсивно копируем каждый элемент
                    newList.Add(Copy(item));
                }
                return newList;
            }


            // 5. Копирование через интерфейс ICloneable
            // Примечание: ICloneable не гарантирует глубокое копирование. Реализация зависит от класса.
            if (originalObject is ICloneable cloneable)
            {
                return cloneable.Clone();
            }

            // 6. Глубокое копирование через бинарную сериализацию
            // Требует, чтобы класс и все его вложенные ссылочные типы были помечены атрибутом [Serializable]
            if (type.IsDefined(typeof(SerializableAttribute), inherit: false))
            {
                // BinaryFormatter устарел и небезопасен. Используйте с осторожностью.
                // Директива ниже подавляет предупреждение компилятора SYSLIB0011.
#pragma warning disable SYSLIB0011

                System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, originalObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    return formatter.Deserialize(stream);
                }

#pragma warning restore SYSLIB0011
            }

            // 7. Если ни один способ не подошел
            throw new ArgumentException($"Объект типа '{type.FullName}' не может быть скопирован.", nameof(originalObject));
        }
    }


}
