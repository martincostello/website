// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Website.Pages;

namespace MartinCostello.Website.EndToEnd;

public sealed class ToolPageTests(WebsiteFixture fixture, ITestOutputHelper outputHelper) : EndToEndTest(fixture, outputHelper)
{
    [SkippableTheory]
    [InlineData("Default", false)]
    [InlineData("Default", true)]
    [InlineData("Numeric", false)]
    public async Task Can_Generate_Guid(string format, bool useUpperCase)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = await page
                    .GuidGenerator()
                    .WithFormatAsync(format);

                await generator.SetUpperCaseAsync(useUpperCase);

                // Act
                string actual = await generator.GenerateAsync();

                // Assert
                Guid.TryParse(actual, out Guid guid).ShouldBeTrue();
                guid.ShouldNotBe(Guid.Empty);

                if (useUpperCase)
                {
                    actual.ShouldBe(actual.ToUpperInvariant());
                }
                else
                {
                    actual.ShouldBe(actual.ToLowerInvariant());
                }
            });
    }

    [SkippableTheory]
    [InlineData("MD5", "Hexadecimal", "foo", "acbd18db4cc2f85cedef654fccc4a4d8")]
    [InlineData("SHA-1", "Base 64", "bar", "Ys23Ag/5IOWqZCw9QGaVDdHwH00=")]
    [InlineData("SHA-256", "Hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
    public async Task Can_Generate_Hash(string algorithm, string format, string plaintext, string expected)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = await page.HashGenerator()
                    .WithAlgorithmAsync(algorithm)
                    .ThenAsync((p) => p.WithFormatAsync(format))
                    .ThenAsync((p) => p.WithPlaintextAsync(plaintext));

                // Act
                string actual = await generator.GenerateAsync();

                // Assert
                actual.ShouldBe(expected);
            });
    }

    [SkippableTheory]
    [InlineData("AES (256 bits)", "SHA-1", "AES", "SHA1")]
    [InlineData("3DES", "HMAC SHA-512", "3DES", "HMACSHA512")]
    public async Task Can_Generate_Machine_Key(
        string decryptionAlgorithm,
        string validationAlgorithm,
        string expectedDecryption,
        string expectedValidation)
    {
        // Arrange
        await AtPageAsync<ToolsPage>(
            async (page) =>
            {
                var generator = await page.MachineKeyGenerator()
                    .WithDecryptionAlgorithmAsync(decryptionAlgorithm)
                    .ThenAsync((p) => p.WithValidationAlgorithmAsync(validationAlgorithm));

                // Act
                string actual = await generator.GenerateAsync();

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
