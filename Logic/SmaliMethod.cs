using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable PossibleNullReferenceException

namespace SmaliParser.Logic
{
    /// <summary>
    /// Класс одного smali метода
    /// </summary>
    public class SmaliMethod : IEquatable<SmaliMethod>
    {
        /// <summary>
        /// Вид записи регистров
        /// </summary>
        public enum RegistersTypes
        {
            /// <summary>
            /// .locals
            /// </summary>
            Locals,

            /// <summary>
            /// .registers
            /// </summary>
            Registers
        }

        /// <summary>
        /// Название метода
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Модификаторы метода
        /// </summary>
        public List<string> Modifers { get; } = new List<string>();

        /// <summary>
        /// Параметры
        /// </summary>
        public List<string> Parameters { get; } = new List<string>(); 

        /// <summary>
        /// Возвращаемый тип
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Тело метода
        /// </summary>
        public List<string> Body { get; } = new List<string>(); 

        /// <summary>
        /// Тип записи регистров
        /// </summary>
        public RegistersTypes RegistersType { get; set; } = RegistersTypes.Locals;

        /// <summary>
        /// Количество регистров
        /// </summary>
        public ushort? Registers { get; set; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="SmaliMethod"/> и заполняет его поля из потока
        /// </summary>
        /// <param name="reader">Поток</param>
        /// <param name="firstLine">Первая строка</param>
        /// <returns></returns>
        public static SmaliMethod ParseStream(TextReader reader, string firstLine = null)
        {
            SmaliMethod method = new SmaliMethod();

            if (firstLine == null)
                firstLine = reader.ReadLine();

            var split = firstLine.Split(' ');

            for (int i = 1; i < split.Length - 1; i++)
                method.Modifers.Add(split[i]);

            var methodSplit = split.Last().Split(new[] {'(', ')', ';'}, StringSplitOptions.RemoveEmptyEntries);

            method.Name = methodSplit[0];
            method.ReturnType = methodSplit.Last();

            for (int i = 1; i < methodSplit.Length - 1; i++)
            {
                var temp = methodSplit[i];

                var param = new StringBuilder();

                for (int j = 0; j < temp.Length; j++)
                {
                    char ch = temp[j];

                    if (ch == '[')
                        param.Append(ch);
                    else if (GlobalConstants.Primitives.Contains(ch))
                    {
                        param.Append(ch);
                        method.Parameters.Add(param.ToString());
                        param.Clear();
                    }
                    else
                    {
                        param.Append(temp.Remove(0, j));
                        method.Parameters.Add(param.ToString());
                        break;
                    }
                }
            }

            string line = reader.ReadLine().TrimStart(' ');

            while (line != ".end method")
            {
                if (line.StartsWith(".locals", StringComparison.Ordinal))
                    method.Registers = ushort.Parse(line.Split(' ')[1]);
                else if (line.StartsWith(".registers", StringComparison.Ordinal))
                {
                    method.Registers = ushort.Parse(line.Split(' ')[1]);
                    method.RegistersType = RegistersTypes.Registers;
                }
                else 
                    method.Body.Add(line);

                line = reader.ReadLine();
            }

            return method;
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SmaliMethod other)
        {
            if (Name != other.Name)
                return false;

            if (!Modifers.SequenceEqual(other.Modifers))
                return false;

            if (!Parameters.SequenceEqual(other.Parameters))
                return false;

            if (ReturnType != other.ReturnType)
                return false;

            if (!Body.SequenceEqual(other.Body))
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

            if (ReturnType == null)
                throw new NullReferenceException(nameof(ReturnType));

            StringBuilder result = new StringBuilder();

            result.Append(".method ");

            foreach (string modifer in Modifers)
            {
                result.Append(modifer);
                result.Append(' ');
            }

            result.Append(Name);
            result.Append('(');

            foreach (string parameter in Parameters)
            {
                result.Append(parameter);
                if (!Utils.IsPrimitive(parameter))
                    result.Append(';');
            }

            result.Append(')');
            result.Append(ReturnType);
            result.AppendLine(Utils.IsPrimitive(ReturnType) ? "" : ";");

            if (Registers.HasValue)
                result.AppendLine((RegistersType == RegistersTypes.Locals ? "    .locals " : "    .registers ") + Registers.Value + "\n");

            foreach (string str in Body)
                result.AppendLine(str);

            result.AppendLine(".end method");

            return result.ToString();
        }
    }
}
