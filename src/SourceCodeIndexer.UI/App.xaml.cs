using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SourceCodeIndexer.UI
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            App.Main();
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = e.Exception?.Message ?? "None";
            MessageBox.Show("An exception was encountered:" + Environment.NewLine + Environment.NewLine + message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
