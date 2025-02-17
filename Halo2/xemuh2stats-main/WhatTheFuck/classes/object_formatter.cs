using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace xemuh2stats.classes
{
    public static class object_formatter
    {
        private static Dictionary<string, Type> _registeredTypes = new Dictionary<string, Type>();

        public static void register_type(string name, Type type) => 
            _registeredTypes.Add(name, type);

        // Format the string with the registered types and provided objects
        public static string Format(string format, params object[] args)
        {
            var typeObjects = args.ToDictionary(arg => arg.GetType());
            var pattern = @"\{\{(.*?)\}\}";
            var matches = Regex.Matches(format, pattern);

            foreach (Match match in matches)
            {
                var expression = match.Groups[1].Value.Trim();
                var (resolvedExpression, defaultValueExpression) = ParseExpression(expression);
                var resolvedExpressionWithValues = ResolveExpression(resolvedExpression, typeObjects);
                var resolvedDefaultValue = ResolveExpression(defaultValueExpression, typeObjects);

                string result;

                if (IsMathExpression(resolvedExpressionWithValues))
                {
                    result = EvaluateExpression(resolvedExpressionWithValues, resolvedDefaultValue).ToString();
                }
                else
                {
                    result = resolvedExpressionWithValues;
                }

                format = format.Replace(match.Value, result);
            }

            return format;
        }

        private static (string expression, string defaultValue) ParseExpression(string expression)
        {
            var parts = expression.Split(new[] { "??" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }
            return (expression, "0");
        }

        private static string ResolveExpression(string expression, Dictionary<Type, object> typeObjects)
        {
            foreach (var registeredType in _registeredTypes)
            {
                string placeholder = $"{registeredType.Key}.";
                Type type = registeredType.Value;

                if (expression.Contains(placeholder))
                {
                    if (typeObjects.TryGetValue(type, out var instance))
                    {
                        var t = type.GetFields();
                        var p = type.GetProperties();
                        if (IsStruct(type))
                        {
                            foreach (var prop in type.GetFields())
                            {
                                string propPlaceholder = placeholder + prop.Name;
                                if (expression.Contains(propPlaceholder))
                                {
                                    var propValue = prop.GetValue(instance)?.ToString();
                                    expression = expression.Replace(propPlaceholder, propValue);
                                }
                            }
                        }
                        else
                        {
                            foreach (var prop in type.GetProperties())
                            {
                                string propPlaceholder = placeholder + prop.Name;
                                if (expression.Contains(propPlaceholder))
                                {
                                    var propValue = prop.GetValue(instance)?.ToString();
                                    expression = expression.Replace(propPlaceholder, propValue);
                                }
                            }
                        }
                    }
                }
            }

            return expression;
        }
        private static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        private static bool IsMathExpression(string expression)
        {
            var mathOperators = new[] { '+', '-', '*', '/' };
            return mathOperators.Any(op => expression.Contains(op));
        }

        private static double EvaluateExpression(string expression, string defaultValue)
        {
            try
            {
                var dataTable = new System.Data.DataTable();
                return Convert.ToDouble(dataTable.Compute(expression, string.Empty));
            }
            catch
            {
                // Resolve the default value in case it contains format tokens
                return Convert.ToDouble(new System.Data.DataTable().Compute(defaultValue, string.Empty));
            }
        }
    }
}
