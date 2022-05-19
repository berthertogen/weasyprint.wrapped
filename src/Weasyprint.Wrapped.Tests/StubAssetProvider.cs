namespace Weasyprint.Wrapped.Tests;

public class StubAssetProvider: IAssetProvider
{
    byte[] IAssetProvider.Get()
    {
        return new [] { (byte)0 };
    }
}
