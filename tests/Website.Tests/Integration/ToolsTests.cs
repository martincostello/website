// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MartinCostello.Website.Integration
{
    /// <summary>
    /// A class containing tests for the <c>/tools</c> resource. This class cannot be inherited.
    /// </summary>
    public class ToolsTests : IntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolsTests"/> class.
        /// </summary>
        /// <param name="fixture">The fixture to use.</param>
        /// <param name="outputHelper">The test output helper to use.</param>
        public ToolsTests(TestServerFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task Tools_Get_Guid_Returns_Correct_Response_If_Format_Is_Invalid()
        {
            // Arrange
            using HttpClient client = Fixture.CreateClient();

            // Act
            using HttpResponseMessage response = await client.GetAsync("/tools/guid?format=foo");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            string json = await response!.Content!.ReadAsStringAsync();

            using var error = JsonDocument.Parse(json);
            error.RootElement.GetProperty("message").GetString().ShouldBe("The specified format 'foo' is invalid.");
        }

        [Theory]
        [InlineData("", "base64", "foo", "No hash algorithm name specified.")]
        [InlineData("sha1", "", "foo", "No hash output format specified.")]
        [InlineData("sha1", "foo", "bar", "The specified hash format 'foo' is invalid.")]
        [InlineData("foo", "base64", "bar", "The specified hash algorithm 'foo' is not supported.")]
        public async Task Tools_Post_Hash_Returns_Correct_Response_If_Request_Is_Invalid(string algorithm, string format, string plaintext, string expected)
        {
            // Arrange
            var request = new
            {
                algorithm,
                format,
                plaintext,
            };

            string requestJson = JsonSerializer.Serialize(request);
            using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Act
            using HttpClient client = Fixture.CreateClient();
            using HttpResponseMessage response = await client.PostAsync("/tools/hash", content);

            // Assert
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.Content.ShouldNotBeNull();

            string json = await response!.Content!.ReadAsStringAsync();

            using var error = JsonDocument.Parse(json);
            error.RootElement.GetProperty("message").GetString().ShouldBe(expected);
        }

        [Fact]
        public async Task Tools_Post_Hash_Returns_Correct_Response_If_Request_Is_Too_Long()
        {
            // Arrange
            var request = new
            {
                algorithm = "sha1",
                format = "base64",
                plaintext = new string(' ', 4097),
            };

            string requestJson = JsonSerializer.Serialize(request);
            using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Act
            using HttpClient client = Fixture.CreateClient();
            using HttpResponseMessage response = await client.PostAsync("/tools/hash", content);

            // Assert
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.Content.ShouldNotBeNull();

            string json = await response!.Content!.ReadAsStringAsync();

            using var error = JsonDocument.Parse(json);
            error.RootElement.GetProperty("message").GetString().ShouldBe("The plaintext to hash cannot be more than 4096 characters in length.");
        }

        [Theory]
        [InlineData("MD5", "Hexadecimal", "", "d41d8cd98f00b204e9800998ecf8427e")]
        [InlineData("SHA1", "Hexadecimal", "", "da39a3ee5e6b4b0d3255bfef95601890afd80709")]
        [InlineData("SHA256", "Hexadecimal", "", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData("SHA384", "Hexadecimal", "", "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData("SHA512", "HEXADECIMAL", "", "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]
        [InlineData("MD5", "Base64", "", "1B2M2Y8AsgTpgAmY7PhCfg==")]
        [InlineData("SHA1", "Base64", "", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=")]
        [InlineData("SHA256", "Base64", "", "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=")]
        [InlineData("SHA384", "BASE64", "", "OLBgp1GsljhM2TJ+sbHjaiH9txEUvgdDTAzHv2P24donTt6/529l+9Ua0vFImLlb")]
        [InlineData("SHA512", "base64", "", "z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg/SpIdNs6c5H0NE8XYXysP+DGNKHfuwvY7kxvUdBeoGlODJ6+SfaPg==")]
        [InlineData("md5", "hexadecimal", "martincostello.com", "e6c3105bdb8e6466f9db1dab47a85131")]
        [InlineData("sha1", "hexadecimal", "martincostello.com", "7fbd8e8cf806e5282af895396f5268483bf6af1b")]
        [InlineData("sha256", "hexadecimal", "martincostello.com", "3b8143aa8119eaf0910aef5cade45dd0e6bb7b70e8d1c8c057bf3fc125248642")]
        [InlineData("sha384", "hexadecimal", "martincostello.com", "5c0e892a9348c184df255f46ab7282eb5792d552c896eb6893d90f36c7202540a9942c80ce5812616d29c08331c60510")]
        [InlineData("sha512", "hexadecimal", "martincostello.com", "3be0167275455dcf1e34f8818d48b7ae4a61fb8549153f42d0d035464fdccee97022d663549eb249d4796956e4016ad83d5e64ba766fb751c8fb2c03b2b4eb9a")]
        public async Task Tools_Post_Hash_Returns_Correct_Response(string algorithm, string format, string plaintext, string expected)
        {
            // Arrange
            var request = new
            {
                algorithm,
                format,
                plaintext,
            };

            string requestJson = JsonSerializer.Serialize(request);
            using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Act
            using HttpClient client = Fixture.CreateClient();
            using HttpResponseMessage response = await client.PostAsync("/tools/hash", content);

            // Assert
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.ShouldNotBeNull();

            string json = await response!.Content!.ReadAsStringAsync();

            using var hash = JsonDocument.Parse(json);
            hash.RootElement.GetProperty("hash").GetString().ShouldBe(expected);
        }

        [Theory]
        [InlineData("", "3DES", "The specified decryption algorithm '' is invalid.")]
        [InlineData("foo", "3DES", "The specified decryption algorithm 'foo' is invalid.")]
        [InlineData("3DES", "", "The specified validation algorithm '' is invalid.")]
        [InlineData("3DES", "foo", "The specified validation algorithm 'foo' is invalid.")]
        public async Task Tools_Get_Machine_Key_Returns_Correct_Response_If_Request_Is_Invalid(
            string decryptionAlgorithm,
            string validationAlgorithm,
            string expected)
        {
            // Arrange
            using HttpClient client = Fixture.CreateClient();

            // Act
            using HttpResponseMessage response = await client.GetAsync($"/tools/machineKey?decryptionAlgorithm={decryptionAlgorithm}&validationAlgorithm={validationAlgorithm}");

            // Assert
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            response.Content.ShouldNotBeNull();

            string json = await response!.Content!.ReadAsStringAsync();

            using var error = JsonDocument.Parse(json);
            error.RootElement.GetProperty("message").GetString().ShouldBe(expected);
        }
    }
}
