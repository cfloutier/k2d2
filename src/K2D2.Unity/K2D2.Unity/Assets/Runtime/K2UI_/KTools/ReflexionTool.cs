using System.Reflection;
using System;

namespace KTools
{
    public class ReflexionTool
    {
        /// <summary>
        /// Uses reflection to get the field value from an object. useful for private fields in KSP2
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            object value = field.GetValue(instance);
            return value;
        }

        public static void callFunction(Type type, object instance, string methodName, object[] args)
        {
            MethodInfo dynMethod = type.GetMethod(methodName,
                        BindingFlags.NonPublic | BindingFlags.Instance);

            dynMethod.Invoke(instance, args);
        }
    }
}