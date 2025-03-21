﻿#if UNITY_EDITOR
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
        internal static StructList<LogEntry> _logEntries = new StructList<LogEntry>(256);
        internal static object _lock = new object();

        internal static void EditorGUI_hyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            OnProcessClickData(args.hyperLinkData);
        }
        internal static void OnProcessClickData(Dictionary<string, string> infos)
        {
            if (infos == null) return;
            if (!infos.TryGetValue("href", out var path)) return;
            infos.TryGetValue("line", out var line);

            for (int i = 0; i < _logEntries.Count; i++)
            {
                ref var e = ref _logEntries._items[i];
                if (CheckLogWithIndexedLink(e.LogString))
                {
                    int indexof = e.LogString.LastIndexOf(INDEXED_LINK_PREV) - 1 + INDEXED_LINK_PREV.Length;// откатываю символ ∆
                    int stringIndexLength = e.LogString.Length - (indexof + INDEXED_LINK_POST.Length);

                    if(stringIndexLength == path.Length)
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
            if (_intervalChecksTicks >= IntervalChecksTicksThreshold || 
                _logEntries.Count >= _logEntries.Capacity - 1)
            {
                CheckConsoleClean();
            }
            _logEntries.Add(new LogEntry(logString, stackTrace));

            _consoleLogCounter++;
            _intervalChecksTicks++;
        }


        private static bool CheckConsoleClean()
        {
            lock (_lock)
            {
                if (_intervalChecksTicks < IntervalChecksTicksThreshold) { return false; }
                int currentCount = GetConsoleLogCount();
                if (_consoleLogCounter > currentCount)
                {
                    var l = _consoleLogCounter - currentCount;
                    if(l < _logEntries.Count)
                    {
                        _logEntries.FastRemoveSpan(0, l);
                    }

                    _consoleLogCounter = currentCount;
                    _intervalChecksTicks = 0;
                    return true;
                }
                return false;
            }
        }

        private const string INDEXED_LINK_PREV = "\r\n\r\n<a href=\"∆";
        private const string INDEXED_LINK_POST = "\">Open line</a>";
        private static string CreateIndexedLink(int index)
        {
            return $"{INDEXED_LINK_PREV}{index}{INDEXED_LINK_POST}";
        }



        //multi thread access.
        public static string NewIndexedLink()
        {
            return instance.GetHyperLink_Internal();
        }
        private static int _hyperLinkIndex = 0;
        public string GetHyperLink_Internal()
        {
            var index = Interlocked.Increment(ref _hyperLinkIndex);
            string hyperLink = CreateIndexedLink(index);
            return hyperLink;
        }

        private static bool CheckLogWithIndexedLink(string log)
        {
            if (log.Length < INDEXED_LINK_POST.Length) { return false; }
            for (int i = 0; i < INDEXED_LINK_POST.Length; i++)
            {
                char constChar = INDEXED_LINK_POST[i];
                char logChar = log[log.Length - INDEXED_LINK_POST.Length + i];
                if (constChar != logChar) { return false; }
            }
            return true;
        }

        internal readonly struct LogEntry
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