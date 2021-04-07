// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.Website.EndToEnd
{
    using System;
    using Pages;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class ToolPageTests : EndToEndTest
    {
        public ToolPageTests(WebsiteFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Theory]
        [InlineData("Default", false)]
        [InlineData("Default", true)]
        [InlineData("Numeric", false)]
        public void Can_Generate_Guid(string format, bool uppercase)
        {
            // Arrange
            AtPage<ToolsPage>(
                (page) =>
                {
                    var generator = page
                        .GuidGenerator()
                        .WithFormat(format);

                    if (uppercase)
                    {
                        generator.ToggleCase();
                    }

                    // Act
                    string actual = generator.Generate();

                    // Assert
                    Guid.TryParse(actual, out Guid guid).ShouldBeTrue();
                    guid.ShouldNotBe(Guid.Empty);

                    if (uppercase)
                    {
                        actual.ShouldBe(actual.ToUpperInvariant());
                    }
                    else
                    {
                        actual.ShouldBe(actual.ToLowerInvariant());
                    }
                });
        }

        [Theory]
        [InlineData("MD5", "Hexadecimal", "foo", "acbd18db4cc2f85cedef654fccc4a4d8")]
        [InlineData("SHA-1", "Base 64", "bar", "Ys23Ag/5IOWqZCw9QGaVDdHwH00=")]
        [InlineData("SHA-256", "Hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
        public void Can_Generate_Hash(string algorithm, string format, string plaintext, string expected)
        {
            // Arrange
            AtPage<ToolsPage>(
                (page) =>
                {
                    var generator = page.HashGenerator()
                        .WithAlgorithm(algorithm)
                        .WithFormat(format)
                        .WithPlaintext(plaintext);

                    // Act
                    string actual = generator.Generate();

                    // Assert
                    actual.ShouldBe(expected);
                });
        }

        [Theory]
        [InlineData("AES (256 bits)", "SHA-1", "AES", "SHA1")]
        [InlineData("3DES", "HMAC SHA-512", "3DES", "HMACSHA512")]
        public void Can_Generate_Machine_Key(
            string decryptionAlgorithm,
            string validationAlgorithm,
            string expectedDecryption,
            string expectedValidation)
        {
            // Arrange
            AtPage<ToolsPage>(
                (page) =>
                {
                    var generator = page.MachineKeyGenerator()
                        .WithDecryptionAlgorithm(decryptionAlgorithm)
                        .WithValidationAlgorithm(validationAlgorithm);

                    // Act
                    string actual = generator.Generate();

                    // Assert
                    actual.ShouldNotBeNullOrWhiteSpace();
                    actual.ShouldStartWith("<machineKey ");
                    actual.ShouldContain(" validationKey=\"");
                    actual.ShouldContain(" decryptionKey=\"");
                    actual.ShouldContain($"validation=\"{expectedValidation}\"");
                    actual.ShouldContain($"decryption=\"{expectedDecryption}\"");
                    actual.ShouldEndWith(" />");
                });
        }
    }
}
