using System.ComponentModel.DataAnnotations;
using Frends.Salesforce.PubSubPublish.Helpers;
using NUnit.Framework;

namespace Frends.Salesforce.PubSubPublish.Tests.HandlersTests;

public class ValidationHandlerTests
{
    [Test]
    public void BasicValidationShouldPass()
    {
        TestClass foobar = new()
        {
            Name = "foobar",
        };
        TestDelegate action = () => ValidationHandler.Run(foobar);
        Assert.DoesNotThrow(action);
    }

    [Test]
    public void ValidationShouldFailOnNullObject()
    {
        TestDelegate action = () => ValidationHandler.Run([null]);
        var ex = Assert.Throws<ValidationException>(action);
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("Validated object can't be null!"));
    }

    [Test]
    public void ValidationShouldFailWhenNoObjectsProvided()
    {
        TestDelegate action = () => ValidationHandler.Run();
        var ex = Assert.Throws<ValidationException>(action);
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void ValidationShouldFailWhenObjectArrayIsNull()
    {
        TestDelegate action = () => ValidationHandler.Run(null);
        var ex = Assert.Throws<ValidationException>(action);
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring("You must provide objects to validate"));
    }

    [Test]
    public void MultipleValidationMessagesAreReturned()
    {
        TestClass foobar = new();
        TestDelegate action = () => ValidationHandler.Run(foobar, null);
        var ex = Assert.Throws<ValidationException>(action);
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
