using System;
using Lilac.Interpreter;

namespace Lilac
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (var options = Options.Parse(args))
            {
                try
                {
                    var container = Bootstrapper.SetupContainer(options);
                    var entryPoint = container.GetInstance<IEntryPoint>();
                    entryPoint.Run();
                }
                catch (Exception e)
                {
                    options.Error.WriteLine(e);
                }
            }
        }
    }
}
