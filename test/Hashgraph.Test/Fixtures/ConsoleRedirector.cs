namespace Hashgraph.Test.Fixtures;

public class ConsoleRedirector : IDisposable
{
    private readonly TextWriter _consoleOut;
    private readonly TextWriter _consoleError;
    private readonly StringWriter _buffer;
    private readonly ITestOutputHelper _testHelper;
    public ConsoleRedirector(ITestOutputHelper testHelper)
    {
        _consoleOut = Console.Out;
        _consoleError = Console.Error;
        _buffer = new StringWriter();
        _testHelper = testHelper;
        Console.SetOut(_buffer);
        Console.SetError(_buffer);
    }
    public void Dispose()
    {
        _testHelper.WriteLine(_buffer.ToString());
        Console.SetOut(_consoleOut);
        Console.SetError(_consoleError);
    }
}