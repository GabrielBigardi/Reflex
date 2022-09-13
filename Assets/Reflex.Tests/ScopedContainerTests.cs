using System;
using FluentAssertions;
using NUnit.Framework;
using Reflex.Scripts;

namespace Reflex.Tests
{
    public class ScopedContainerTests
    {
        private class Foo : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public Foo()
            {
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class DependsOnFoo : IDisposable
        {
            public Foo Foo { get; }

            public DependsOnFoo(Foo foo)
            {
                Foo = foo;
            }

            public void Dispose()
            {
            }
        }

        [Test]
        public void InnerContainerCanResolveBindingFromOuterScope()
        {
            using (var outer = new Container())
            {
                outer.BindSingleton(42);

                using (var inner = outer.Scope())
                {
                    inner.Resolve<int>().Should().Be(42);
                }
            }
        }

        [Test]
        public void InnerContainerCanOverrideBindingFromOuterScope()
        {
            using (var outer = new Container())
            {
                outer.BindSingleton("outer");

                using (var inner = outer.Scope())
                {
                    inner.BindSingleton("inner");
                    inner.Resolve<string>().Should().Be("inner");
                }
            }
        }

        [Test]
        public void OuterContainerShouldNotBeAffectedByInnerContainerOverride()
        {
            using (var outer = new Container())
            {
                outer.BindSingleton("outer");

                using (var inner = outer.Scope())
                {
                    inner.BindSingleton("inner");
                }

                outer.Resolve<string>().Should().Be("outer");
            }
        }

        [Test]
        public void InnerScopeBindingCanResolveOuterDependency()
        {
            using (var outer = new Container())
            {
                var foo = new Foo();
                outer.BindSingleton(foo);

                using (var inner = outer.Scope())
                {
                    inner.BindSingleton<DependsOnFoo, DependsOnFoo>();
                    inner.Resolve<DependsOnFoo>().Foo.Should().Be(foo);
                }
            }
        }

        [Test]
        public void DisposingInnerScopeShouldNotDisposeInstancesFromOuterScope()
        {
            using (var outer = new Container())
            {
                outer.BindSingleton<Foo, Foo>();

                using (var inner = outer.Scope())
                {
                    inner.BindSingleton<DependsOnFoo, DependsOnFoo>();
                    inner.Resolve<Foo>();
                    inner.Resolve<DependsOnFoo>().Foo.IsDisposed.Should().BeFalse(); 
                }

                outer.Resolve<Foo>().IsDisposed.Should().BeTrue();
            }
        }

        [Test]
        public void ResolvingContainerFromInnerScopeShouldResolveInner()
        {
            using (var outer = new Container())
            {
                using (var inner = outer.Scope())
                {
                    inner.Resolve<Container>().Should().Be(inner);
                    inner.Resolve<IContainer>().Should().Be(inner);
                }
            }
        }
        
        [Test]
        public void ResolvingContainerFromOuterScopeShouldResolveOuter()
        {
            using (var outer = new Container())
            {
                using (var inner = outer.Scope())
                {
                    inner.Resolve<Container>();
                    inner.Resolve<IContainer>();
                }
                
                outer.Resolve<Container>().Should().Be(outer);
                outer.Resolve<IContainer>().Should().Be(outer);
            }
        }
    }
}