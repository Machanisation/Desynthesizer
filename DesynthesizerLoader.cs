// By - Machanisation

using System;
using System.IO;
using System.Reflection;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Helpers;
using TreeSharp;
using Action = TreeSharp.Action;

namespace Desynthesizer
{
    public class Desynthesizer : BotBase
    {
        private const string ProjectName = "Desynthesizer";
        private const string ProjectMainType = "Desynthesizer.DesynthesizerBot";
        private const string ProjectAssemblyName = "Desynthesizer.dll";

        private static readonly string ProjectAssembly =
            Path.Combine(Environment.CurrentDirectory, $@"BotBases\{ProjectName}\{ProjectAssemblyName}");

        private static object Product { get; set; }

        private static MethodInfo StartFunc { get; set; }
        private static MethodInfo StopFunc { get; set; }
        private static MethodInfo ButtonFunc { get; set; }
        private static MethodInfo RootFunc { get; set; }
        private static MethodInfo PulseFunc { get; set; }
        private static MethodInfo InitFunc { get; set; }
        private static MethodInfo ShutdownFunc { get; set; }

        private static volatile bool _loaded;

        public override string Name => ProjectName;
        public override PulseFlags PulseFlags => PulseFlags.All;
        public override bool IsAutonomous => false;
        public override bool WantButton => true;
        public override bool RequiresProfile => false;

        public override Composite Root
        {
            get
            {
                if (!_loaded && Product == null)
                    LoadProduct();

                return Product != null
                    ? (Composite)RootFunc.Invoke(Product, null)
                    : new Action();
            }
        }

        public Desynthesizer()
        {
            Log("Loader constructed.");
        }

        public override void Start()
        {
            if (!_loaded && Product == null)
                LoadProduct();

            if (Product != null)
                StartFunc?.Invoke(Product, null);
        }

        public override void Stop()
        {
            if (!_loaded && Product == null)
                LoadProduct();

            if (Product != null)
                StopFunc?.Invoke(Product, null);
        }

        public override void Pulse()
        {
            if (!_loaded && Product == null)
                LoadProduct();

            if (Product != null)
                PulseFunc?.Invoke(Product, null);
        }

        public override void OnButtonPress()
        {
            if (!_loaded && Product == null)
                LoadProduct();

            if (Product != null)
                ButtonFunc?.Invoke(Product, null);
        }

        private static void LoadProduct()
        {
            try
            {
                if (!File.Exists(ProjectAssembly))
                {
                    Log("Missing dll: " + ProjectAssembly);
                    return;
                }

                var assembly = Assembly.LoadFrom(ProjectAssembly);
                if (assembly == null)
                {
                    Log("Assembly load failed.");
                    return;
                }

                var type = assembly.GetType(ProjectMainType);
                if (type == null)
                {
                    Log("Type not found: " + ProjectMainType);
                    return;
                }

                Product = Activator.CreateInstance(type);
                if (Product == null)
                {
                    Log("Could not instantiate bot.");
                    return;
                }

                StartFunc = type.GetMethod("Start");
                StopFunc = type.GetMethod("Stop");
                ButtonFunc = type.GetMethod("OnButtonPress");
                RootFunc = type.GetMethod("GetRoot");
                PulseFunc = type.GetMethod("Pulse");
                InitFunc = type.GetMethod("Initialize");
                ShutdownFunc = type.GetMethod("OnShutdown");

                InitFunc?.Invoke(Product, null);

                _loaded = true;
                Log("Loaded successfully.");
            }
            catch (Exception ex)
            {
                Log("Load error: " + ex);
            }
        }

        private static void Log(string message)
        {
            Logging.Write("[DesynthesizerLoader] " + message);
        }
    }
}