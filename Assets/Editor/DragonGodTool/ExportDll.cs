using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class ExportDll
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static string m_slnSourcePath = m_rootPath + "HotUpdate/HotUpdate.sln";

    [MenuItem("GodDragonTool/导出热更新脚本Dll")]
    public static void BuildDll()
    {
        File.Copy(m_rootPath + "Library/ScriptAssemblies/Invariable.dll", m_rootPath + "HotUpdate/libs/Invariable.dll", true);

        // 使用 dotnet CLI 编译项目
        Process process = new Process();

        process.StartInfo.FileName = "dotnet"; // 或者 "msbuild" 视情况而定
        process.StartInfo.Arguments = $"build \"{m_slnSourcePath}\" -c Release"; // 编译命令
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = false; // 不需要重定向标准输出
        process.StartInfo.RedirectStandardError = false; // 不需要重定向标准错误

        process.Start();
        process.WaitForExit(); // 等待进程完成

        // 确保 DLL 生成成功后，复制到指定路径
        string dllName = Path.GetFileNameWithoutExtension(m_slnSourcePath) + ".dll";
        string sourceDLLPath = Path.Combine(Path.GetDirectoryName(m_slnSourcePath), "bin/Release", dllName);

        if (File.Exists(sourceDLLPath))
        {
            string destinationDLLPath1 = Path.Combine(m_rootPath + "Dll/Android", dllName);
            string destinationDLLPath2 = Path.Combine(m_rootPath + "Dll/Windows", dllName);
            File.Copy(sourceDLLPath, destinationDLLPath1, true);
            File.Copy(sourceDLLPath, destinationDLLPath2, true);
        }
    }
}