using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.DeleteSObject.Helpers;
using NUnit.Framework;

namespace Frends.Salesforce.DeleteSObject.Tests.HandlersTests;

public class ValidationHandlerTests
{
    private class TestClass
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }

    private class OtherTestClass
    {
        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; }
    }

    [Test]
    public void BasicValidationShouldPass()
    {
        TestClass foobar = new()
        {
            Name = "foobar",
        };
        Assert.DoesNotThrow(() => ValidationHandler.Run(foobar));
    }

    [Test]
    public void ValidationShouldFailOnNullObject()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(new object[] { null }));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("can't be null"));
    }

    [Test]
    public void ValidationShouldFailWhenNoObjectsProvided()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run());
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void ValidationShouldFailWhenObjectArrayIsNull()
    {
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(null));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void MultipleValidationMessagesAreReturned()
    {
        TestClass foobar = new();
        OtherTestClass other = new();
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(foobar, other));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Name is required"));
        Assert.That(ex.Message, Contains.Substring("Value is required"));
    }
}
