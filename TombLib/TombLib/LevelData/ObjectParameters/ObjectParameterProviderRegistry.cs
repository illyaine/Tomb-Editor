using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData.ObjectParameters
{
    public static class ObjectParameterProviderRegistry
    {
        private static readonly List<IObjectParameterProvider> _providers = new List<IObjectParameterProvider>();

        public static IReadOnlyList<IObjectParameterProvider> Providers => _providers;

        public static void Register(IObjectParameterProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(provider.Id))
                throw new ArgumentException("Object parameter provider ID must not be empty.", nameof(provider));

            if (_providers.Any(existing => string.Equals(existing.Id, provider.Id, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Object parameter provider is already registered: " + provider.Id);

            _providers.Add(provider);
        }

        public static void Unregister(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return;

            _providers.RemoveAll(provider => string.Equals(provider.Id, providerId, StringComparison.OrdinalIgnoreCase));
        }

        public static void Clear()
        {
            _providers.Clear();
        }

        public static IObjectParameterProvider FindProvider(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
                return null;

            return _providers.FirstOrDefault(provider => string.Equals(provider.Id, providerId, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<ObjectParameterDefinitionSet> GetDefinitionSets(ObjectParameterContext context)
        {
            foreach (IObjectParameterProvider provider in _providers)
                foreach (ObjectParameterDefinitionSet definitionSet in provider.GetDefinitionSets(context) ?? Enumerable.Empty<ObjectParameterDefinitionSet>())
                    yield return definitionSet;
        }

        public static IEnumerable<ObjectParameterValidationMessage> Validate(ObjectParameterContext context, ObjectParameterValueSet valueSet)
        {
            foreach (IObjectParameterProvider provider in _providers)
                foreach (ObjectParameterValidationMessage message in provider.Validate(context, valueSet) ?? Enumerable.Empty<ObjectParameterValidationMessage>())
                    yield return message;
        }

        public static IEnumerable<ObjectParameterExportEntry> Export(ObjectParameterExportContext context)
        {
            if (context == null || context.Values == null)
                yield break;

            IObjectParameterProvider provider = FindProvider(context.Values.ProviderId);
            if (provider is IObjectParameterExportProvider exportProvider)
                foreach (ObjectParameterExportEntry entry in exportProvider.Export(context) ?? Enumerable.Empty<ObjectParameterExportEntry>())
                    yield return entry;
        }
    }
}
