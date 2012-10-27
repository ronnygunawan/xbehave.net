// <copyright file="ParameterizedCommand.cs" company="Adam Ralph">
//  Copyright (c) Adam Ralph. All rights reserved.
// </copyright>

namespace Xbehave.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Xunit.Sdk;

    public class ParameterizedCommand : TestCommand, IParameterizedCommand
    {
        private readonly Argument[] arguments;
        private readonly Type[] typeArguments;

        public ParameterizedCommand(IMethodInfo scenarioMethod)
            : this(scenarioMethod, null, null)
        {
        }

        public ParameterizedCommand(IMethodInfo scenarioMethod, Argument[] arguments, Type[] typeArguments)
            : base(scenarioMethod, null, MethodUtility.GetTimeoutParameter(scenarioMethod))
        {
            this.arguments = arguments != null ? arguments.ToArray() : new Argument[0];
            this.typeArguments = typeArguments != null ? typeArguments.ToArray() : new Type[0];
            this.DisplayName = GetString(scenarioMethod, this.arguments, this.typeArguments);
        }

        public IEnumerable<Argument> Arguments
        {
            get { return this.arguments.Select(argument => argument); }
        }

        public IEnumerable<Type> TypeArguments
        {
            get { return this.typeArguments.Select(typeArgument => typeArgument); }
        }

        public override MethodResult Execute(object testClass)
        {
            var parameters = testMethod.MethodInfo.GetParameters();
            if (parameters.Length != this.arguments.Length)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, "Expected {0} arguments, got {1} arguments", parameters.Length, this.arguments.Length));
            }

            try
            {
                testMethod.Invoke(testClass, this.arguments.Select(argument => argument.Value).ToArray());
            }
            catch (TargetInvocationException ex)
            {
                ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
            }

            return new PassedResult(testMethod, this.DisplayName);
        }

        private static string GetString(IMethodInfo method, Argument[] arguments, Type[] typeArguments)
        {
            var csharp = string.Concat(method.TypeName, ".", method.Name);
            if (typeArguments.Length > 0)
            {
                csharp = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}<{1}>",
                    csharp,
                    string.Join(", ", typeArguments.Select(typeArgument => GetString(typeArgument)).ToArray()));
            }

            var parameters = method.MethodInfo.GetParameters();
            var parameterTokens = new List<string>();
            int parameterIndex;
            for (parameterIndex = 0; parameterIndex < arguments.Length; parameterIndex++)
            {
                if (arguments[parameterIndex].IsGeneratedDefault)
                {
                    continue;
                }

                parameterTokens.Add(string.Concat(
                    parameterIndex >= parameters.Length ? "???" : parameters[parameterIndex].Name,
                    ": ",
                    GetString(arguments[parameterIndex])));
            }

            for (; parameterIndex < parameters.Length; parameterIndex++)
            {
                parameterTokens.Add(parameters[parameterIndex].Name + ": ???");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}({1})", csharp, string.Join(", ", parameterTokens.ToArray()));
        }

        private static string GetString(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var genericArgumentCSharpNames = type.GetGenericArguments().Select(typeArgument => GetString(typeArgument)).ToArray();
            return string.Concat(type.Name.Substring(0, type.Name.IndexOf('`')), "<", string.Join(", ", genericArgumentCSharpNames), ">");
        }

        private static string GetString(Argument argument)
        {
            if (argument.Value == null)
            {
                return "null";
            }

            if (argument.Value is char)
            {
                return "'" + argument.Value + "'";
            }

            var stringArgument = argument.Value as string;
            if (stringArgument != null)
            {
                if (stringArgument.Length > 50)
                {
                    return string.Concat("\"", stringArgument.Substring(0, 50), "\"...");
                }

                return string.Concat("\"", stringArgument, "\"");
            }

            return Convert.ToString(argument.Value, CultureInfo.InvariantCulture);
        }
    }
}