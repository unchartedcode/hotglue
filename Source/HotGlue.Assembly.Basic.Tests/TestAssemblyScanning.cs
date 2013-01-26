﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Assembly.Basic.Tests
{
    [TestFixture]
    public class TestAssemblyScanning
    {
        [Test]
        public void Does_It_Find_Only_The_Basic_Classes()
        {
            // Arrange/Act
            var configuration = HotGlueConfiguration.Load();

            // Assert
            configuration.Debug.ShouldBe(false);
            configuration.ScriptPath.ShouldBe(@"Scripts/");
            configuration.Compilers.Length.ShouldBe(0);
            configuration.Referencers.Length.ShouldBe(3);
            configuration.Referencers.Any(x => x.Type == typeof(SlashSlashEqualReference).FullName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(TripleSlashReference).FullName).ShouldBe(true);
            configuration.Referencers.Any(x => x.Type == typeof(RequireReference).FullName).ShouldBe(true);
            configuration.GenerateScript.ShouldBe(null);
        }
    }
}
