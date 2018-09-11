using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tools
{
    public static class Helpers
    {
        public static string ToJsonString(this object @object)
        {
            return JsonConvert.SerializeObject(@object, Formatting.Indented);
        }

        /// <summary>
        /// Возвращает иерархию типов в виде коллекции, от которой унаследован исходный тип
        /// </summary>
        /// <param name="type">Исходный тип</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            while (type != null)
            {
                var t = type;
                type = type.BaseType;

                foreach (var @interface in t.GetInterfaces())
                {
                    yield return @interface;
                }
                yield return t;
            }
        }

        /// <summary>
        /// Проверяет наследствие типов
        /// </summary>
        /// <param name="type">Исходный тип</param>
        /// <param name="parentType">Преполагаемый родительский тип</param>
        /// <returns></returns>
        public static bool Is(this Type type, Type parentType)
        {
            return GetParentTypes(type).Any(t => t == parentType);
        }

        /// <summary>
        /// Проверяет наследствие типов для исходного объекта
        /// </summary>
        /// <param name="object">Исходный объект</param>
        /// <param name="parentType">Преполагаемый родительский тип</param>
        /// <returns></returns>
        public static bool Is(this object @object, Type parentType)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            return @object.GetType().Is(parentType);
        }

        /// <summary>
        /// Проверяет наследствие типов
        /// </summary>
        /// <param name="type">Исходный тип</param>
        /// <typeparam name="T">Преполагаемый родительский тип</typeparam>
        /// <returns></returns>
        public static bool Is<T>(this Type type)
        {
            return Is(type, typeof(T));
        }

        /// <summary>
        /// Проверяет наследствие типов для исходного объекта
        /// </summary>
        /// <param name="object">Исходный объект</param>
        /// <typeparam name="T">Преполагаемый родительский тип</typeparam>
        /// <returns></returns>
        public static bool Is<T>(this object @object)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            return @object.Is(typeof(T));
        }

        /// <summary>
        /// Приводит исходный объект к заданному типу
        /// </summary>
        /// <typeparam name="T">Тип, к которому предлагается привести объект</typeparam>
        /// <param name="object">Исходный объект</param>
        /// <returns>Если тип заданного объекта не принадлежит к заданному возвращается null</returns>
        public static T As<T>(this object @object)
            where T : class
        {
            if (@object.Is<T>())
                return (T)@object;

            return null;
        }
    }
}
