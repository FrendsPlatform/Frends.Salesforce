using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.PubSubConsume.Helpers;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubConsume.Tests.HandlersTests;

public class ValidationHandlerTests
{
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
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run([null]));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Validated object can't be null!"));
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
        var ex = Assert.Throws<ValidationException>(() => ValidationHandler.Run(foobar, null));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Validated object can't be null!"));
        Assert.That(ex.Message, Contains.Substring("Name is required"));
    }

    private class TestClass
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
    }
}
