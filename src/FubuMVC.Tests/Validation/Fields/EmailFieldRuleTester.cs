using System.Linq;
using FubuMVC.Core.Validation;
using FubuMVC.Core.Validation.Fields;
using NUnit.Framework;
using Shouldly;

namespace FubuMVC.Tests.Validation.Fields
{
    [TestFixture]
    public class EmailFieldRuleTester
    {
        private EmailTarget theTarget;

        [SetUp]
        public void SetUp()
        {
            theTarget = new EmailTarget();
        }

        private Notification theNotification
        {
            get
            {
                var scenario = ValidationScenario<EmailTarget>.For(x =>
                {
                    x.Model = theTarget;
                    x.FieldRule(new EmailFieldRule());
                });

                return scenario.Notification;
            }
        }

        [Test]
        public void default_token_is_email_key()
        {
            new EmailFieldRule().Token.ShouldBe(ValidationKeys.Email);
        }

        [Test]
        public void no_message_if_email_is_valid()
        {
            AssertEmailValidationReturnsNoMessage("joel+paulus@arnold.com").ShouldBeTrue();
            AssertEmailValidationReturnsNoMessage("user@domain.com").ShouldBeTrue();
            AssertEmailValidationReturnsNoMessage("user@sub.domain.com").ShouldBeTrue();
            AssertEmailValidationReturnsNoMessage("first.last@sub.domain.com").ShouldBeTrue();
            AssertEmailValidationReturnsNoMessage("gmail+style@sub.domain.com").ShouldBeTrue();
        }

        [Test]
        public void registers_message_if_email_is_invalid()
        {
            theTarget.Email = "something";
            var messages = theNotification.MessagesFor<EmailTarget>(x => x.Email);
            messages.Single().StringToken.ShouldBe(ValidationKeys.Email);
        }

        [Test]
        public void uppercase_letters_are_valid()
        {
            theTarget.Email = "Something@there.com";
            theNotification.MessagesFor<EmailTarget>(x => x.Email).Any().ShouldBeFalse();
        }
        
        [Test]
        public void no_message_if_email_is_empty()
        {
            AssertEmailValidationReturnsNoMessage(string.Empty).ShouldBeTrue();
        }

        private bool AssertEmailValidationReturnsNoMessage(string email)
        {
            theTarget.Email = email;
            return theNotification.MessagesFor<EmailTarget>(x => x.Email).Any() == false;
        }

        public class EmailTarget
        {
            public string Email { get; set; }
        }
    }
}