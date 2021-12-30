using Bindito.Core;
using Timberborn.TemplateSystem;

namespace AutomizeMyDroughts.Components
{
    public static class IContainerDefinitionExtensions
    {
        /// <summary>
        /// Opens up a component (i.e. mono behvaiour) to be extended with another component.
        /// </summary>
        public static ExtenableComponent<T> ExtendComponent<T>(this IContainerDefinition container)
            => new(container);

        public struct ExtenableComponent<T> {
            private readonly IContainerDefinition _container;
            public ExtenableComponent(IContainerDefinition container) {
                _container = container;
            }

            /// <summary>
            /// Extends with the given component.
            /// </summary>
            public void With<E>() {
                _container.MultiBind<TemplateModule>().ToProvider(Decorate<E>).AsSingleton();
            }

            private static TemplateModule Decorate<E>() {
                var builder = new TemplateModule.Builder();
                builder.AddDecorator<T, E>();
                return builder.Build();
            }
        }
    }
}
