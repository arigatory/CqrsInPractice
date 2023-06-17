using Logic.Decorators;
using Logic.Students;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Api.Utils
{
    public static class HandlerRegistration
    {
        public static void AddHandlers(this IServiceCollection services)
        {
            List<Type> handlerTypes = typeof(ICommand).Assembly.GetTypes()
                .Where(x => x.GetInterfaces().Any(y => IsHandlerInterface(y)))
                .Where(x => x.Name.EndsWith("Handler"))
                .ToList();

            foreach (var type in handlerTypes)
            {
                AddHandler(services, type);
            }
        }

        private static void AddHandler(IServiceCollection services, Type type)
        {
            object[] attributes = type.GetCustomAttributes(false);

            List<Type> pipeline = attributes
                .Select(x => ToDecorator(x))
                .Concat(new[] { type })
                .Reverse()
                .ToList();

            Type interfaceType = type.GetInterfaces().Single(y => IsHandlerInterface(y));
            Func<IServiceProvider, object> factory = BuildPipeline(pipeline, interfaceType);

            services.AddTransient(interfaceType, factory);
        }

        private static Func<IServiceProvider, object> BuildPipeline(List<Type> pipeline, Type interfaceType)
        {
            List<ConstructorInfo> ctors = pipeline
                .Select(x =>
                {
                    Type type = x.IsGenericType ? x.MakeGenericType(interfaceType.GenericTypeArguments) : x;
                    return type.GetConstructors().Single();
                })
                .ToList();

            Func<IServiceProvider, object> func = provider =>
            {
                object current = null;

                foreach (var ctor in ctors)
                {
                    List<ParameterInfo> parameterInfos = ctor.GetParameters().ToList();

                    object[] parameters = GetParameters(parameterInfos, current, provider);

                    current = ctor.Invoke(parameters);
                }

                return current;
            };

            return func;
        }

        private static object[] GetParameters(List<ParameterInfo> parameterInfos, object current, IServiceProvider provider)
        {
            var result = new object[parameterInfos.Count];

            for (int i = 0; i < parameterInfos.Count; i++)
            {
                result[i] = GetParameter(parameterInfos[i], current, provider);
            }

            return result; ;

        }

        private static object GetParameter(ParameterInfo parameterInfo, object current, IServiceProvider provider)
        {
            Type parameterType = parameterInfo.ParameterType;

            if (IsHandlerInterface(parameterType))
                return current;

            object service = provider.GetService(parameterType);
            if (service != null)
                return service;

            throw new ArgumentException($"Type {parameterType} not  found");
        }

        private static Type ToDecorator(object attribute)
        {
            Type type = attribute.GetType();

            if (type == typeof(DatabaseRetryAttribute))
                return typeof(DatabaseRetryDecorator<>);

            if (type == typeof(AuditLogAttribute))
                return typeof(AuditLoggingDecorator<>);

            throw new ArgumentException(attribute.ToString());
        }

        private static bool IsHandlerInterface(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type typeDefenition = type.GetGenericTypeDefinition();

            return typeDefenition == typeof(ICommandHandler<>) || typeDefenition == typeof(IQueryHandler<,>);
        }
    }
}
//services.AddTransient<ICommandHandler<EditPersonalInfoCommand>>(provider =>
//                new AuditLoggingDecorator<EditPersonalInfoCommand>(
//                    new DatabaseRetryDecorator<EditPersonalInfoCommand>(
//                        new EditPersonalInfoCommandHandler(provider.GetService<SessionFactory>()),
//                        provider.GetService<Config>())));
//services.AddTransient<ICommandHandler<RegisterCommand>, RegisterCommandHandler>();
//services.AddTransient<ICommandHandler<UnregisterCommand>, UnregisterCommandHandler>();
//services.AddTransient<ICommandHandler<EnrollCommand>, EnrollCommandHandler>();
//services.AddTransient<ICommandHandler<TransferCommand>, TransferCommandHandler>();
//services.AddTransient<ICommandHandler<DisenrollCommand>, DisenrollCommandHandler>();
//services.AddTransient<IQueryHandler<GetListQuery, List<StudentDto>>, GetListQueryHandler>();