using AutoFixture;
using AutoFixture.Xunit2;

namespace Common.Tests.Infrastructure.AutoMoq
{
    /// <summary>
    /// Custom AutoData attribute to use AutoFixture with Moq customizations.
    /// </summary>
    public class AutoMoqAttribute : AutoDataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMoqAttribute"/> class.
        /// </summary>
        public AutoMoqAttribute() : base(() => getCustomFixture())
        {
        }

        /// <summary>
        /// Creates and customizes a Fixture for the tests.
        /// </summary>
        /// <returns>A customized Fixture instance.</returns>
        private static Fixture getCustomFixture()
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoFixtureCustomization());

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            return fixture;
        }
    }
}
