using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmaliParser.Logic
{
    /// <summary>
    /// Класс одного поля smali файла
    /// </summary>
    public class SmaliField : IEquatable<SmaliField>
    {
        /// <summary>
        /// Модификаторы
        /// </summary>
        public List<string> Modifers { get; } = new List<string>(); 

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="SmaliField"/> и заполняет его поля из строки
        /// </summary>
        /// <param name="line">Строка</param>
        public static SmaliField ParseString(string line)
        {
            SmaliField field = new SmaliField();

            // .field final synthetic val$act:Landroid/app/Activity; = smth
            var eqSplit = line.Split('=').Select(it => it.Trim()).ToArray();

            var split = eqSplit[0].Split(' ');

            for (int i = 1; i < split.Length - 1; i++)
                field.Modifers.Add(split[i]);

            var lastSplit = split.Last().Split(':');

            field.Name = lastSplit[0];
            field.Type = lastSplit[1].TrimEnd(';');

            if (eqSplit.Length == 2)
                field.Value = eqSplit[1];

            return field;
        }

        /// <summary>
        /// Возвращает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SmaliField other)
        {
            if (other == null)
                return false;

            if (Name != other.Name)
                return false;

            if (!Modifers.SequenceEqual(other.Modifers))
                return false;

            if (Type != other.Type)
                return false;

            if (Value != other.Value)
                return false;

            return true;
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
            if (Name == null)
                throw new NullReferenceException(nameof(Name));

            if (Type == null)
                throw new NullReferenceException(nameof(Type));

            StringBuilder result = new StringBuilder();

            result.Append(".field ");

            foreach (string modifer in Modifers)
            {
                result.Append(modifer);
                result.Append(' ');
            }

            result.Append(Name);
            result.Append(':');
            result.Append(Type);
            result.Append(Utils.IsPrimitive(Type) ? "" : ";");

            if (Value != null)
            {
                result.Append(" = ");
                result.Append(Value);
            }

            result.AppendLine();

            return result.ToString();
        }
    }
}
