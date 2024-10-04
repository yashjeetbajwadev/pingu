using FluentValidation;
using Humanizer;
using pingu.Helpers;
using System.Reflection;

namespace pingu.Providers.ModelValidator;

public static class ModelValidatorExtensions
{
    public static IServiceCollection AddModelValidator(this IServiceCollection services, Assembly assembly)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        return services.AddModelValidator([assembly]);
    }

    public static IServiceCollection AddModelValidator(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

        services.AddSingleton<IModelValidator, ModelValidator>();

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DisplayNameResolver = (_, memberInfo, expression) =>
        {
            string? RelovePropertyName()
            {
                if (expression != null)
                {
                    var chain = FluentValidation.Internal.PropertyChain.FromExpression(expression);
                    if (chain.Count > 0) return chain.ToString();
                }

                if (memberInfo != null)
                {
                    return memberInfo.Name;
                }

                return null;
            }

            var propertyName = RelovePropertyName()?.Humanize();
            return propertyName;
        };

        var validatorTypes = assemblies.SelectMany(a => a.DefinedTypes).Select(i => i.AsType()).Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType && type.IsCompatibleWith(typeof(IValidator<>))).ToArray();

        foreach (var concreteType in validatorTypes)
        {
            var matchingInterfaceType = concreteType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

            if (matchingInterfaceType != null)
            {
                services.AddScoped(matchingInterfaceType, concreteType);
            }
        }

        return services;
    }
}