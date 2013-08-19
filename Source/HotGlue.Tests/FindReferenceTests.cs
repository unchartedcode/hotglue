﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;
using NUnit.Framework;
using Shouldly;

namespace HotGlue.Tests
{
    [TestFixture]
    public class FindReferenceTests
    {
        #region Javascript
        [Test]
        public void Can_Parse_Comment_Reference()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();

            // Act
            var references = referencer.Parse(@"
//= require module1.js
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Dependency);
        }
        
        [Test]
        public void Can_Parse_Comment__Library_Reference()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();

            // Act
            var references = referencer.Parse(@"
//= library module1.js
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Library);
        }

        [Test]
        public void Can_Parse_Module_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod = require('module1.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Can_Parse_Module_Reference_Without_Variable()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
require('module1.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Can_Parse_Two_Module_References_Without_Variable()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
require('module1.js');
require('module2.js');

");
            // Assert
            references.Count().ShouldBe(2);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
            references.Last().Name.ShouldBe("module2.js");
            references.Last().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Can_Parse_Module_Reference_Without_Extension()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
require('module1');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Can_Parse_Generate_Reference()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var route1 = generate('all.routes');
var route2 = generate(""all.routes"");

");
            // Assert
            references.Count().ShouldBe(2);
            references.ToList().ForEach(x => x.Name.ShouldBe("all.routes"));
            references.ToList().ForEach(x => x.Type.ShouldBe(Reference.TypeEnum.Generated));
        }

        [Test]
        public void Does_Not_Parse_Generate_Comment_Reference()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
//= generate('all.routes')
//= generate 'all.routes'
//= generate(""all.routes"")
//= generate ""all.routes""
");
            // Assert
            references.Count().ShouldBe(0);
        }

        [Test]
        public void Can_Parse_Multiple_Module_References()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(2);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Not_Parse_Commented_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
//var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Not_Parse_Commented_With_Spaces_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
  //   var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Not_Parse_Commented_Generate_With_Spaces_Reference()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = generate('all.routes');
  //   var mod2 = generate('all.routes');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Not_Parse_Commented_With_Space_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = require('module1.js');
  //var mod2 = require('module2.js');

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_Reference_That_Pulls_SubObject()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = generate('all.routes').increment;
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Parse_Generated_Reference_That_Pulls_SubObject()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var mod1 = generate('all.routes').increment;
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Parse_Object_Initialize_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var someObject = {
        mod1: require('module1.js')
};
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_Generated_Object_Initialize_Reference()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var someObject = {
        mod1: generate('all.routes')
};
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Not_Parse_Object_Initialize_Comment_Reference()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
var someObject = {
        mod1: require('module1.js')
        //mod2: require('module2.js')
};
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Not_Parse_Generated_Object_Initialize_Comment_Reference()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
var someObject = {
        mod1: generate('all.routes')
        //mod2: generate('all.routes')
};
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Parse_TripleSlashReference()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
/// <reference path=""test.js""/>
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Dependency);
        }

        [Test]
        public void Should_Parse_TripleSlashReference_For_Libaries()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
/// <reference path=""test.js"" library/>
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Library);
        }

        [Test]
        public void Should_Parse_TripleSlashWithSpaces()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
///      <reference      path=""test.js""      />       
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Dependency);
        }

        [Test]
        public void Should_Parse_TripleSlashReference_without_the_space()
        {
            // Arrange
            var referencer = new TripleSlashReference();

            // Act
            var references = referencer.Parse(@"
///<reference path=""test.js""/>
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("test.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Dependency);
        }
        #endregion

        #region Coffee Script
        [Test]
        public void Should_Find_CoffeeScript_Module()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse("mod4 = require('mod4.coffee')\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Find_CoffeeScript_Module_Without_Parentheses()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse("mod4 = require 'mod4.coffee'\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Find_Generate_CoffeeScript_Module()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse("mod4 = generate('all.routes')\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("all.routes");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Should_Find_Generate_CoffeeScript_Module_Without_Parentheses()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse("mod4 = generate 'mod4.coffee'\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }

        [Test]
        public void Can_Parse_Multiple_CoffeeScript_Module_References_With_Mixed_Styles()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
mod1 = require('mod1.coffee')
mod4 = require 'mod4.coffee'

");
            // Assert
            references.Count().ShouldBe(2);
            references.First().Name.ShouldBe("mod1.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
            references.Last().Name.ShouldBe("mod4.coffee");
            references.Last().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Find_CoffeeScript_Dependency()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();

            // Act
            var references = referencer.Parse("#= require 'mod4.coffee'\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Dependency);
        }

        [Test]
        public void Should_Find_CoffeeScript_Library()
        {
            // Arrange
            var referencer = new SlashSlashEqualReference();

            // Act
            var references = referencer.Parse("#= library 'mod4.coffee'\r\n\r\nt = 4").ToList();

            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("mod4.coffee");
            references.First().Type.ShouldBe(Reference.TypeEnum.Library);
        }

        [Test]
        public void Should_Not_Parse_Commented_Reference_Coffee_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
mod1 = require('module1.js')
#mod2 = require('module2.js')

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }
        [Test]
        public void Should_Not_Parse_Commented_Reference_With_Spaces_Coffee_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
mod1 = require('module1.js')
  #   mod2 = require('module2.js')

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Not_Parse_Commented_Reference_With_Space_Coffee_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
mod1 = require('module1.js')
   #mod2 = require('module2.js')

");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_Object_Initialize_Reference_Coffee_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
someObject = 
    mod1: require('module1.js')
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_Object_Initialize_Class_Reference_Coffee_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
someObject:
    mod1: require('module1.js')
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }

        [Test]
        public void Should_Parse_Generated_Object_Initialize_Class_Reference_Coffee_Script()
        {
            // Arrange
            var referencer = new GenerateReference();

            // Act
            var references = referencer.Parse(@"
someObject:
    mod1: generate 'module1.js'
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Generated);
        }


        [Test]
        public void Should_Not_Parse_Object_Initialize_Comment_Reference_Coffe_Script()
        {
            // Arrange
            var referencer = new RequireReference();

            // Act
            var references = referencer.Parse(@"
someObject:
    mod1: require('module1.js')
    #mod2: require('module2.js')
");
            // Assert
            references.Count().ShouldBe(1);
            references.First().Name.ShouldBe("module1.js");
            references.First().Type.ShouldBe(Reference.TypeEnum.Module);
        }
        #endregion
    }
}
