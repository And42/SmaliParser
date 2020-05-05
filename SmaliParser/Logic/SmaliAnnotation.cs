using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmaliParser.Logic
{
    /// <summary>
    /// Класс аннотации smali файла
    /// </summary>
    public class SmaliAnnotation : IEquatable<SmaliAnnotation>
    {
        /// <summary>
        /// Текст аннотации
        /// </summary>
        public List<string> Body { get; } = new List<string>();

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="SmaliAnnotation"/> и заполняет его поля из потока
        /// </summary>
        /// <param name="reader">Поток</param>
        /// <param name="firstLine">Первая строка текста</param>
        public static SmaliAnnotation ParseStream(TextReader reader, string firstLine = null)
        {
            var annotation = new SmaliAnnotation();

            if (firstLine != null)
                annotation.Body.Add(firstLine);

            string line;
            do
            {
                line = reader.ReadLine();
                annotation.Body.Add(line);
            } while (line != ".end annotation");

            return annotation;
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SmaliAnnotation other)
        {
            if (other == null)
                return false;

            return Body.SequenceEqual(other.Body);
        }

        /// <summary>
        /// Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return String.Join("\n", Body) +"\n";
        }
    }
}
