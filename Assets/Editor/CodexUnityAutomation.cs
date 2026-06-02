using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

[InitializeOnLoad]
public static class CodexUnityAutomation
{
    private const string RequestPath = "Temp/CodexStayAliveAutomation.request";
    private const string RunningPath = "Temp/CodexStayAliveAutomation.running";
    private const string JsonResultPath = "Logs/codex-unity-automation.json";
    private const string XmlResultPath = "Logs/codex-unity-test-results.xml";

    private static bool isRunning;

    static CodexUnityAutomation()
    {
        EditorApplication.update -= PollForRequest;
        EditorApplication.update += PollForRequest;
    }

    [MenuItem("Stay Alive/Run Codex Build And Tests")]
    public static void RunFromMenu()
    {
        RunAutomation();
    }

    private static void PollForRequest()
    {
        if (isRunning || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        if (!File.Exists(RequestPath))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            return;
        }

        RunAutomation();
    }

    private static void RunAutomation()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            return;
        }

        isRunning = true;
        Directory.CreateDirectory("Logs");
        Directory.CreateDirectory("Temp");

        TryDelete(RequestPath);
        File.WriteAllText(RunningPath, DateTime.UtcNow.ToString("O"));

        try
        {
            StayAlivePrototypeBuilder.BuildProofOfConcept();
            RunEditModeTests();
        }
        catch (Exception exception)
        {
            WriteJsonResult(false, "build", 0, 0, 0, 0, "Build automation failed.", exception.ToString());
            TryDelete(RunningPath);
            isRunning = false;
            Debug.LogException(exception);
        }
    }

    private static void RunEditModeTests()
    {
        TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
        AutomationCallbacks callbacks = new AutomationCallbacks();
        api.RegisterCallbacks(callbacks);

        Filter filter = new Filter
        {
            testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,
            groupNames = new[] { "^StayAlivePrototypeTests" }
        };

        ExecutionSettings settings = new ExecutionSettings(filter)
        {
            runSynchronously = true
        };

        api.Execute(settings);
    }

    private static void FinishTestRun(ITestResultAdaptor result)
    {
        TestRunnerApi.SaveResultToFile(result, XmlResultPath);

        int total = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount;
        bool passed = result.FailCount == 0 && result.InconclusiveCount == 0 && result.PassCount >= 5;
        string summary = passed
            ? "Build completed and StayAlivePrototypeTests passed."
            : "Build completed but one or more StayAlivePrototypeTests failed.";

        WriteJsonResult(
            passed,
            "tests",
            total,
            result.PassCount,
            result.FailCount,
            result.SkipCount + result.InconclusiveCount,
            summary,
            CollectFailures(result));

        TryDelete(RunningPath);
        isRunning = false;
    }

    private static string CollectFailures(ITestResultAdaptor result)
    {
        StringBuilder builder = new StringBuilder();
        CollectFailures(result, builder);
        return builder.ToString();
    }

    private static void CollectFailures(ITestResultAdaptor result, StringBuilder builder)
    {
        if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
        {
            builder.AppendLine(result.FullName);
            if (!string.IsNullOrEmpty(result.Message))
            {
                builder.AppendLine(result.Message);
            }
            if (!string.IsNullOrEmpty(result.StackTrace))
            {
                builder.AppendLine(result.StackTrace);
            }
        }

        if (!result.HasChildren)
        {
            return;
        }

        foreach (ITestResultAdaptor child in result.Children)
        {
            CollectFailures(child, builder);
        }
    }

    private static void WriteJsonResult(bool success, string stage, int total, int passed, int failed, int other, string summary, string details)
    {
        string json = "{\n"
            + "  \"success\": " + (success ? "true" : "false") + ",\n"
            + "  \"stage\": \"" + Escape(stage) + "\",\n"
            + "  \"total\": " + total + ",\n"
            + "  \"passed\": " + passed + ",\n"
            + "  \"failed\": " + failed + ",\n"
            + "  \"other\": " + other + ",\n"
            + "  \"summary\": \"" + Escape(summary) + "\",\n"
            + "  \"details\": \"" + Escape(details) + "\",\n"
            + "  \"timestampUtc\": \"" + Escape(DateTime.UtcNow.ToString("O")) + "\"\n"
            + "}\n";

        File.WriteAllText(JsonResultPath, json);
        Debug.Log("Codex Unity automation result written to " + JsonResultPath);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class AutomationCallbacks : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            TestRunnerApi.UnregisterTestCallback(this);
            FinishTestRun(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }
    }
}
