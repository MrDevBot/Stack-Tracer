# Stack-Tracer
C# stack tracer with "Post" and live exception tracing capability. supports both basic and advanced trace listeners, outfile and direct console output. useful in scenarios where anti-debugging is present.

#Implementation Example
```cs
[STAThread]
private static void Main()
{
        Debugger.Initilise();

        Debugger.Post(0, "[POST] " + "Hooking global error supression");
        //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += GlobalErrorSupression;
}

private static void GlobalErrorSupression(object sender, ThreadExceptionEventArgs e)
{
        Debugger.LiveTracer();
        Debugger.Post(0, "[SUPRESSION] " + "GlobalErrorSupression triggerd");
        MessageBox.Show(e.Exception.Message.ToString(), "MoonShine | Unknown Error");
}
```
note: "GlobalErrorSupression" is not required but strongly recommended when trying to catch exceptions
