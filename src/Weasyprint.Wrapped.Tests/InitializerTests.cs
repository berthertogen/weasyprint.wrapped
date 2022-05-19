using Xunit;

namespace Weasyprint.Wrapped.Tests;

public class InitializerTests
{
    [Fact]
    public void Do_UnzipsAssetToFolder()
    {
        new Initializer().Do();

        
    }
}