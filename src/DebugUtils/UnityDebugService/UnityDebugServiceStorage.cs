#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class UnityDebugServiceStorage : ScriptableSingleton<UnityDebugServiceStorage>
    {
        private static readonly MethodInfo _getLogsCountMethod;
        public static readonly bool IsSupportAutoLingks;
        static UnityDebugServiceStorage()
        {
            var logEntriesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
            if (logEntriesType != null)
            {
                _getLogsCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
            IsSupportAutoLingks = _getLogsCountMethod != null;

            EditorGUI.hyperLinkClicked -= EditorGUI_hyperLinkClicked;
            Application.logMessageReceived -= Application_logMessageReceived;
            _consoleLogCounter = -1;
            if (IsSupportAutoLingks)
            {
                EditorGUI.hyperLinkClicked += EditorGUI_hyperLinkClicked;
                Application.logMessageReceived += Application_logMessageReceived;
                _consoleLogCounter = GetConsoleLogCount();
            }
        }
        public UnityDebugServiceStorage() { }

        private const int IntervalChecksTicksThreshold = 100;
        private static int _consoleLogCounter;
        private static int _intervalChecksTicks = 0;
        private static StructList<string> _recycledIndexes;
        private static StructList<LogEntry> _logEntries;
        private static object _lock = new object();

        private static void EditorGUI_hyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void Application_logMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (_intervalChecksTicks >= IntervalChecksTicksThreshold || 
                _logEntries.Count >= _logEntries.Capacity - 1)
            {
                CheckConsoleClean();
                _intervalChecksTicks = 0;
            }
            _logEntries.Add(new LogEntry(logString, stackTrace));

            Interlocked.Increment(ref _consoleLogCounter);
            Interlocked.Increment(ref _intervalChecksTicks);
        }


        private static bool CheckConsoleClean()
        {
            int currentCount = GetConsoleLogCount();
            if (_consoleLogCounter > currentCount)
            {

                error

                _consoleLogCounter = currentCount;
                return true;
            }
            return false;
        }
        private static int GetConsoleLogCount()
        {
            return (int)_getLogsCountMethod.Invoke(null, null);
        }
        private static string CreateIndexedLink(int index)
        {
            return $"<a href=\"{index}\">∆</a> ";
        }



        //multi thread access.
        public static string GetHyperLink()
        {
            return instance.GetHyperLink_Internal();
        }
        public string GetHyperLink_Internal()
        {
            string hyperLink;
            if (_recycledIndexes.Count > 0)
            {
                hyperLink = _recycledIndexes.Dequeue();
            }
            else
            {
                hyperLink = CreateIndexedLink(_logEntries.Count);
            }
            return hyperLink;
        }



        private readonly struct LogEntry
        {
            public readonly string LogString;
            public readonly string StackTrace;
            public LogEntry(string logString, string stackTrace)
            {
                LogString = logString;
                StackTrace = stackTrace;
            }
        }
    }
}
#endif