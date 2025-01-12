using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Invariable;

public class ExportDll
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static string m_slnSourcePath = m_rootPath + "HotUpdate/HotUpdate.sln";

    [MenuItem("GodDragonTool/DLL/复制InvariableDll到热更新文件夹路径")]
    public static void CopyInvariableDll()
    {
        File.Copy(m_rootPath + "Library/ScriptAssemblies/Invariable.dll", m_rootPath + "HotUpdate/libs/Invariable.dll", true);
    }

    [MenuItem("GodDragonTool/DLL/导出HotUpdateDll")]
    public static void ExportUpdateDll()
    {
        if (Directory.Exists(m_rootPath + "Dll"))
        {
            Directory.Delete(m_rootPath + "Dll", true);
        }

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
            DataUtilityManager.InitDirectory(m_rootPath + "Dll");
            string destinationDLLPath = Path.Combine(m_rootPath, "Dll", dllName);
            File.Copy(sourceDLLPath, destinationDLLPath, true);
        }
    }
}