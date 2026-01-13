using System.Reflection;

Version version = Assembly.GetExecutingAssembly().GetName().Version!;

Console.WriteLine($"GSS CLI - Version {version.Major}.{version.Minor}.{version.Build}");