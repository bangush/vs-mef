﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.Composition.Tests.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Composition.Reflection;
    using Xunit;

    public class TypeRefTests
    {
        [Fact]
        public void EqualsDistinguishesArrays()
        {
            Assert.NotEqual(TypeRef.Get(typeof(object), TestUtilities.Resolver), TypeRef.Get(typeof(object[]), TestUtilities.Resolver));
        }

        [Fact]
        public void EqualsChecksAssemblyVersionEquality()
        {
            const string assemblyNameFormat = "MyAssembly, Version={0}, Culture=neutral, PublicKeyToken=abcdef1234567890, processorArchitecture=MSIL";
            string assemblyNameV1 = string.Format(assemblyNameFormat, "1.0.0.0");
            string assemblyNameV2 = string.Format(assemblyNameFormat, "2.0.0.0");
            this.TestAssemblyNameEqualityNotEqual(assemblyNameV1, assemblyNameV2, @"C:\MyAssembly.dll", @"C:\MyAssembly.dll", Guid.Empty, Guid.Empty);
        }

        [Fact]
        public void EqualsChecksAssemblyPKTEquality()
        {
            const string assemblyNameFormat = "MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken={0}, processorArchitecture=MSIL";
            string assemblyNameV1 = string.Format(assemblyNameFormat, "abcdef1234567890");
            string assemblyNameV2 = string.Format(assemblyNameFormat, "1234567890abcdef");
            this.TestAssemblyNameEqualityNotEqual(assemblyNameV1, assemblyNameV2, @"C:\MyAssembly.dll", @"C:\MyAssembly.dll", Guid.Empty, Guid.Empty);
        }

        [Fact]
        public void EqualsChecksMvidEquality()
        {
            const string assemblyName = "MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=abcdef1234567890, processorArchitecture=MSIL";
            Guid guidV1 = new Guid("00000000-0000-0000-0000-000000000001");
            Guid guidV2 = new Guid("00000000-0000-0000-0000-000000000002");
            this.TestAssemblyNameEqualityNotEqual(assemblyName, assemblyName, @"C:\MyAssembly.dll", @"C:\MyAssembly.dll", guidV1, guidV2);
        }

        private void TestAssemblyNameEqualityNotEqual(string assemblyNameV1String, string assemblyNameV2String, string codeBaseV1, string codeBaseV2, Guid mvidV1, Guid mvidV2)
        {
            AssemblyName assemblyNameV1 = new AssemblyName(assemblyNameV1String);
            assemblyNameV1.CodeBase = codeBaseV1;
            AssemblyName assemblyNameV2 = new AssemblyName(assemblyNameV2String);
            assemblyNameV2.CodeBase = codeBaseV2;

            StrongAssemblyIdentity assemblyIdentityV1 = new StrongAssemblyIdentity(assemblyNameV1, mvidV1);
            StrongAssemblyIdentity assemblyIdentityV2 = new StrongAssemblyIdentity(assemblyNameV2, mvidV2);
            TypeRef typeRefV1 = TypeRef.Get(TestUtilities.Resolver, assemblyIdentityV1, 0x02000001, "SomeType", TypeRefFlags.None, 0, ImmutableArray<TypeRef>.Empty, false, ImmutableArray<TypeRef>.Empty, null);
            TypeRef typeRefV2 = TypeRef.Get(TestUtilities.Resolver, assemblyIdentityV2, 0x02000001, "SomeType", TypeRefFlags.None, 0, ImmutableArray<TypeRef>.Empty, false, ImmutableArray<TypeRef>.Empty, null);

            Assert.NotEqual(typeRefV1, typeRefV2);
        }
    }
}
