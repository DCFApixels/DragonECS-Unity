#if UNITY_EDITOR
#if DRAGONECS_ENABLE_UNITY_CONSOLE_SHORTCUT_LINKS
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    using static UnityDebugServiceStorage;
    using static UnityDebugServiceStorageInitializer;

    [InitializeOnLoad]
    internal static class UnityDebugServiceStorageInitializer
    {
        private static readonly MethodInfo _getLogsCountMethod;
        public static readonly bool IsSupportAutoLingks;
        static UnityDebugServiceStorageInitializer()
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
            //if (IsSupportAutoLingks)
            {
                EditorGUI.hyperLinkClicked += EditorGUI_hyperLinkClicked;
                Application.logMessageReceived += Application_logMessageReceived;
                _consoleLogCounter = GetConsoleLogCount();
            }
        }

        internal static int GetConsoleLogCount()
        {
            return (int)_getLogsCountMethod.Invoke(null, null);
        }
    }
    internal class UnityDebugServiceStorage : ScriptableSingleton<UnityDebugServiceStorage>
    {


        public UnityDebugServiceStorage() { }

        internal const int IntervalChecksTicksThreshold = 100;
        internal static int _consoleLogCounter;
        internal static int _intervalChecksTicks = 0;

        [SerializeField]
        internal StructList<LogEntry> _logEntries = new StructList<LogEntry>(256);
        [SerializeField]
        private int _hyperLinkIndex = 0;

        internal static object _lock = new object();

        internal static void EditorGUI_hyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            OnProcessClickData(args.hyperLinkData);
        }
        internal static void OnProcessClickData(Dictionary<string, string> infos)
        {
            var inst = instance;
            if (infos == null) return;
            if (!infos.TryGetValue("href", out var path)) return;

            for (int i = 0; i < inst._logEntries.Count; i++)
            {
                ref var e = ref inst._logEntries._items[i];
                if (CheckLogWithIndexedLink(e.LogString))
                {
                    int indexof = INDEXED_LINK_PREV.Length - 1;// откатываю символ ∆
                    int stringIndexLength = e.LogString.IndexOf(INDEXED_LINK_POST) - indexof;

                    if (stringIndexLength == path.Length)
                    {
                        bool isSkip = false;
                        for (int j = 1; j < stringIndexLength; j++)
                        {
                            var pathchar = path[j];
                            var logchar = e.LogString[indexof + j];
                            if (pathchar != logchar) { isSkip = true; break; }
                        }

                        if (isSkip) { continue; }

                        OpenIDE(e);

                        break;
                    }
                }
            }
        }

        private static void OpenIDE(LogEntry entry)
        {
            var parsed = ParseLastCall(entry.StackTrace);
            if (string.IsNullOrEmpty(parsed.path)) { return; }
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(parsed.path, parsed.line); //TODO
        }
        public static (string path, int line) ParseLastCall(string stackTrace)
        {
            var debugTypeFullname = typeof(DCFApixels.DragonECS.EcsDebug).FullName;
            stackTrace = stackTrace.Remove(0, stackTrace.IndexOf(debugTypeFullname));
            var lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                Match match = Regex.Match(line, @"\(at (?<path>.+?):(?<line>\d+)\)");
                if (match.Success)
                {
                    string filePath = match.Groups["path"].Value;
                    string lineNumber = match.Groups["line"].Value;
                    return (filePath, int.Parse(lineNumber));
                }
            }
            return default;
        }


        internal static void Application_logMessageReceived(string logString, string stackTrace, LogType type)
        {
            var inst = instance;
            if (_intervalChecksTicks >= IntervalChecksTicksThreshold ||
                inst._logEntries.Count >= inst._logEntries.Capacity - 1)
            {
                CheckConsoleClean();
            }
            inst._logEntries.Add(new LogEntry(logString, stackTrace));

            _consoleLogCounter++;
            _intervalChecksTicks++;
        }


        private static bool CheckConsoleClean()
        {
            lock (_lock)
            {
                var inst = instance;
                if (_intervalChecksTicks < IntervalChecksTicksThreshold) { return false; }
                int currentCount = GetConsoleLogCount();
                if (_consoleLogCounter > currentCount)
                {
                    var l = _consoleLogCounter - currentCount;
                    if (l < inst._logEntries.Count)
                    {
                        inst._logEntries.FastRemoveSpan(0, l);
                    }

                    _consoleLogCounter = currentCount;
                    _intervalChecksTicks = 0;
                    return true;
                }
                return false;
            }
        }

        private const string INDEXED_LINK_PREV = "<a href=\"∆";
        private const string INDEXED_LINK_POST = "\">→ </a>";
        private static string CreateIndexedLink(int index)
        {
            return $"{INDEXED_LINK_PREV}{index}{INDEXED_LINK_POST}";
        }



        //multi thread access.
        public static string NewIndexedLink()
        {
            return instance.GetHyperLink_Internal();
        }
        public string GetHyperLink_Internal()
        {
            var index = Interlocked.Increment(ref _hyperLinkIndex);
            string hyperLink = CreateIndexedLink(index);
            return hyperLink;
        }

        private static bool CheckLogWithIndexedLink(string log)
        {
            if (log == null) { return false; }
            if (log.Length < INDEXED_LINK_PREV.Length) { return false; }
            for (int i = 0; i < INDEXED_LINK_PREV.Length; i++)
            {
                char constChar = INDEXED_LINK_PREV[i];
                char logChar = log[i];
                if (constChar != logChar) { return false; }
            }
            return true;
        }

        [Serializable]
        internal struct LogEntry
        {
            public string LogString;
            public string StackTrace;
            public LogEntry(string logString, string stackTrace)
            {
                LogString = logString;
                StackTrace = stackTrace;
            }
        }
    }
}
#endif
#endif