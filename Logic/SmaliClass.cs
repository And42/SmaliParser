using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LongPaths.Logic;

// ReSharper disable PossibleNullReferenceException

namespace SmaliParser.Logic
{
    /// <summary>
    /// Класс одного smali класса
    /// </summary>
    public class SmaliClass : IEquatable<SmaliClass>
    {
        /// <summary>
        /// Название класса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Модификаторы класса
        /// </summary>
        public List<string> Modifers { get; } = new List<string>(); 

        /// <summary>
        /// Класс - родитель
        /// </summary>
        public string SuperClass { get; set; }

        /// <summary>
        /// Файл исходного java кода
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Наследуемые интерфейсы
        /// </summary>
        public List<string> Interfaces { get; } = new List<string>(); 

        /// <summary>
        /// Поля
        /// </summary>
        public List<SmaliField> Fields { get; } = new List<SmaliField>(); 

        /// <summary>
        /// Методы
        /// </summary>
        public List<SmaliMethod> Methods { get; } = new List<SmaliMethod>(); 

        /// <summary>
        /// Аннотации
        /// </summary>
        public List<SmaliAnnotation> Annotations { get; } = new List<SmaliAnnotation>();

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="SmaliClass"/> и заполняет его поля из файла
        /// </summary>
        /// <param name="fileName">Файл</param>
        /// <exception cref="FileNotFoundException">Файл не найден</exception>
        public static SmaliClass ParseFile(string fileName)
        {
            if (!LFile.Exists(fileName))
                throw new FileNotFoundException(nameof(fileName));

            using (StreamReader reader = new StreamReader(fileName, new UTF8Encoding(false)))
                return ParseStream(reader);
        }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="SmaliClass"/> и заполняет его поля из потока
        /// </summary>
        /// <param name="reader">Поток</param>
        public static SmaliClass ParseStream(TextReader reader)
        {
            SmaliClass cls = new SmaliClass();

            var line = reader.ReadLine();  // .class public Lcom/packageName/example;

            string[] split = line.Split(' ');

            cls.Name = split.Last().TrimEnd(';');

            for (int i = 1; i < split.Length - 1; i++)
                cls.Modifers.Add(split[i]);

            line = reader.ReadLine();  // .super Ljava/lang/Object;

            cls.SuperClass = line.Split(' ')[1].TrimEnd(';');

            line = reader.ReadLine();  // .source "example.java"

            if (!string.IsNullOrEmpty(line))
                cls.SourceFile = line.Split(' ')[1].Trim('"');

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(".implements", StringComparison.Ordinal))
                    cls.Interfaces.Add(line.Split(' ')[1].TrimEnd(';'));
                else if (line.StartsWith(".annotation", StringComparison.Ordinal))
                    cls.Annotations.Add(SmaliAnnotation.ParseStream(reader, line));
                else if (line.StartsWith(".method", StringComparison.Ordinal))
                    cls.Methods.Add(SmaliMethod.ParseStream(reader, line));
                else if (line.StartsWith(".field", StringComparison.Ordinal))
                    cls.Fields.Add(SmaliField.ParseString(line));
            }

            return cls;
        }

        /// <summary>
        /// Указывает, равен ли текущий объект другому объекту того же типа.
        /// </summary>
        /// <returns>
        /// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
        /// </returns>
        /// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
        public bool Equals(SmaliClass other)
        {
            if (Name != other.Name)
                return false;

            if (!Modifers.SequenceEqual(other.Modifers))
                return false;

            if (SuperClass != other.SuperClass)
                return false;

            if (SourceFile != other.SourceFile)
                return false;

            if (!Interfaces.SequenceEqual(other.Interfaces))
                return false;

            if (!Fields.SequenceEqual(other.Fields))
                return false;

            if (!Methods.SequenceEqual(other.Methods))
                return false;

            if (!Annotations.SequenceEqual(other.Annotations))
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
            StringBuilder result = new StringBuilder();

            if (!string.IsNullOrEmpty(Name))
            {
                result.Append(".class ");

                foreach (string modifer in Modifers)
                {
                    result.Append(modifer);
                    result.Append(' ');
                }

                result.Append(Name);
                result.AppendLine(";");
            }

            if (!string.IsNullOrEmpty(SuperClass))
            {
                result.Append(".super ");
                result.Append(SuperClass);
                result.AppendLine(Utils.IsPrimitive(SuperClass) ? "" : ";");
            }

            if (!string.IsNullOrEmpty(SourceFile))
            {
                result.Append(".source \"");
                result.Append(SourceFile);
                result.AppendLine("\"\n");
            }

            foreach (var inter in Interfaces)
            {
                result.Append(".implements ");
                result.Append(inter);
                result.AppendLine(";");
            }

            result.AppendLine();

            foreach (var annot in Annotations)
                result.AppendLine(annot.ToString());

            foreach (var field in Fields)
                result.AppendLine(field.ToString());

            foreach (var method in Methods)
                result.AppendLine(method.ToString());

            return result.ToString();
        }

        /// <summary>
        /// Сохраняет текущий экземпляр класса <see cref="SmaliClass"/> в файл
        /// </summary>
        /// <param name="pathToFile">Файл</param>
        public void Save(string pathToFile)
        {
            LFile.WriteAllText(pathToFile, ToString(), new UTF8Encoding(false));
        }

        /// <summary>
        /// Сохраняет текущий экземпляр класса <see cref="SmaliClass"/> в поток
        /// </summary>
        /// <param name="stream">Поток</param>
        /// <exception cref="ArgumentException">Поток недоступен для записи</exception>
        public void Save(Stream stream)
        {
            if (!stream.CanWrite)
                throw new ArgumentException(nameof(stream));

            byte[] buffer = Encoding.UTF8.GetBytes(ToString());

            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Сохраняет текущий экземпляр класса <see cref="SmaliClass"/> в текстовый поток
        /// </summary>
        /// <param name="writer">Текстовый поток</param>
        public void Save(TextWriter writer)
        {
            writer.Write(ToString());
        }
    }
}
