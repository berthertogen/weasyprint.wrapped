using System.IO;
using Xunit;

namespace Weasyprint.Wrapped.Tests;

public class InitializerTests
{
    public InitializerTests()
    {
        if (Directory.Exists("./weasyprinter")) {
            Directory.Delete("./weasyprinter", true);
        }
    }

    [Fact]
    public void Do_UnzipsAssetToFolder()
    {
        new Initializer(new StubConfigurationProviderWindows()).Do();

        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.True(Directory.Exists("./weasyprinter/python"));
    }

    [Fact]
    public void Do_UnzipsAssetToFolder_DoesNothingIfFolderExists()
    {
        Directory.CreateDirectory("./weasyprinter");

        new Initializer(new StubConfigurationProviderWindows()).Do();

        Assert.True(Directory.Exists("./weasyprinter"));
        Assert.False(Directory.Exists("./weasyprinter/python"));
    }
}