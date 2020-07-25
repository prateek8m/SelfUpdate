using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Onova.Services;
using Onova.Tests.Internal;
using Xunit;

namespace Onova.Tests.Resolving
{
    public class GithubSourceSpecs : IDisposable
    {
        private string TempDirPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), $"{nameof(GithubSourceSpecs)}_{Guid.NewGuid()}");

        public GithubSourceSpecs() => DirectoryEx.Reset(TempDirPath);

        public void Dispose() => DirectoryEx.DeleteIfExists(TempDirPath);

        // https://github.com/Tyrrrz/OnovaTestRepo
        //private GithubPackageResolver CreateGithubPackageResolver() =>
        //    new GithubPackageResolver("Tyrrrz", "OnovaTestRepo", "*.onv");

        string apiBaseAddress = "https://api.github.factset.com";
        string repoOwner = "mdts";
        string repoName = "environments-cmdrunner-netcore";
        string namePattern = "*.dll";
        HttpClient httpClient = new HttpClient();
        private GithubPackageResolver CreateGithubPackageResolver() =>
           new GithubPackageResolver(httpClient, apiBaseAddress, repoOwner, repoName, namePattern);

        [Fact]
        public async Task I_can_use_a_GitHub_repository_as_a_package_source()
        {
            // Arrange
            var resolver = CreateGithubPackageResolver();

            var version = Version.Parse("1.2");
            var destFilePath = Path.Combine(TempDirPath, "Output.dll");

            // Act
            await resolver.DownloadPackageAsync(version, destFilePath);

            // Assert
            File.ReadAllText(destFilePath).Should().Be("Hello world");
        }

        [Fact]
        public async Task When_using_a_GitHub_repository_as_a_package_source_packages_are_mapped_from_releases()
        {
            // Arrange
            var resolver = CreateGithubPackageResolver();

            // Act
            var versions = await resolver.GetPackageVersionsAsync();

            // Assert
            versions.Should().BeEquivalentTo(
                Version.Parse("1.0"),
                Version.Parse("2.0"),
                Version.Parse("3.0"));
        }
    }
}