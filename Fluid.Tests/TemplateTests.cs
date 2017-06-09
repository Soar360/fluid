﻿using System;
using Fluid.Ast.Values;
using Fluid.Tests.Domain;
using Xunit;

namespace Fluid.Tests
{
    public class TemplateTests
    {
        private object _products = new []
        {
            new { name = "product 1", price = 1 },
            new { name = "product 2", price = 2 },
            new { name = "product 3", price = 3 },
        };

        private void Check(string source, string expected, Action<TemplateContext> init = null)
        {
            FluidTemplate.TryParse(source, out var template, out var messages);

            var context = new TemplateContext();
            init?.Invoke(context);

            var result = template.Render(context);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        public void ShouldRenderText(string source, string expected)
        {
            Check(source, expected);

        }

        [Theory]
        [InlineData("{{ 'abc' }}", "abc")]
        public void ShouldEvaluateString(string source, string expected)
        {
            Check(source, expected);
        }

        [Theory]
        [InlineData("{{ 0 }}", "0")]
        [InlineData("{{ 0 }}", "0")]
        [InlineData("{{ 123 }}", "123")]
        [InlineData("{{ 123.456 }}", "123.456")]
        [InlineData("{{ -123.456 }}", "-123.456")]
        [InlineData("{{ +123.456 }}", "123.456")]
        public void ShouldEvaluateNumber(string source, string expected)
        {
            Check(source, expected);
        }

        [Theory]
        [InlineData("{{ true }}", "true")]
        [InlineData("{{ false }}", "false")]
        public void ShouldEvaluateBoolean(string source, string expected)
        {
            Check(source, expected);
        }

        [Theory]
        [InlineData("{{ 1 | inc }}", "2")]
        [InlineData("{{ 1 | inc | inc }}", "3")]
        [InlineData("{{ 1 | inc:2 | inc }}", "4")]
        [InlineData("{{ 'a' | append:'b', 'c' }}", "abc")]
        public void ShouldEvaluateFilters(string source, string expected)
        {
            FluidTemplate.TryParse(source, out var template, out var messages);
            var context = new TemplateContext();

            context.Filters.Add("inc", (i, args) => 
            {
                var increment = 1;
                if (args.Length > 0)
                {
                    increment = (int)args[0].ToNumberValue();
                }

                return new NumberValue(i.ToNumberValue() + increment);
            });

            context.Filters.Add("append", (i, args) =>
            {
                var s = i.ToStringValue();

                foreach(var a in args)
                {
                    s += a.ToStringValue();
                }

                return new StringValue(s);
            });

            var result = template.Render(context);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ShouldEvaluateBooleanValue()
        {
            FluidTemplate.TryParse("{{ x }}", out var template, out var messages);
            var context = new TemplateContext();
            context.SetValue("x", true);

            var result = template.Render(context);
            Assert.Equal("true", result);
        }

        [Fact]
        public void ShouldEvaluateStringValue()
        {
            FluidTemplate.TryParse("{{ x }}", out var template, out var messages);
            var context = new TemplateContext();
            context.SetValue("x", "abc");

            var result = template.Render(context);
            Assert.Equal("abc", result);
        }

        [Fact]
        public void ShouldEvaluateNumberValue()
        {
            FluidTemplate.TryParse("{{ x }}", out var template, out var messages);
            var context = new TemplateContext();
            context.SetValue("x", 1);

            var result = template.Render(context);
            Assert.Equal("1", result);
        }

        [Fact]
        public void ShouldEvaluateObjectProperty()
        {
            FluidTemplate.TryParse("{{ p.Name }}", out var template, out var messages);
            var context = new TemplateContext();
            context.SetValue("p", new Person { Name = "John" });

            var result = template.Render(context);
            Assert.Equal("John", result);
        }

        [Fact]
        public void ShouldEvaluateStringIndex()
        {
            FluidTemplate.TryParse("{{ x[1] }}", out var template, out var messages);
            var context = new TemplateContext();
            context.SetValue("x", "abc");

            var result = template.Render(context);
            Assert.Equal("b", result);
        }

        [Theory]
        [InlineData("{% for i in (1..3) %}{{i}}{% endfor %}", "123")]
        [InlineData("{% for p in products %}{{p.price}}{% endfor %}", "123")]
        public void ShouldEvaluateForStatement(string source, string expected)
        {
            Check(source, expected, ctx => { ctx.SetValue("products", _products); });
        }
    }
}