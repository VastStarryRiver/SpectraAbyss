using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Invariable;

public class ExportDll
{
    private static string m_rootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");
    private static string m_slnSourcePath = m_rootPath + "HotUpdate/HotUpdate.sln";

    [MenuItem("GodDragonTool/DLL/����InvariableDll���ȸ����ļ���·��")]
    public static void CopyInvariableDll()
    {
        File.Copy(m_rootPath + "Library/ScriptAssemblies/Invariable.dll", m_rootPath + "HotUpdate/libs/Invariable.dll", true);
    }

    [MenuItem("GodDragonTool/DLL/����HotUpdateDll")]
    public static void ExportUpdateDll()
    {
        if (Directory.Exists(m_rootPath + "Dll"))
        {
            Directory.Delete(m_rootPath + "Dll", true);
        }

        // ʹ�� dotnet CLI ������Ŀ
        Process process = new Process();

        process.StartInfo.FileName = "dotnet"; // ���� "msbuild" ���������
        process.StartInfo.Arguments = $"build \"{m_slnSourcePath}\" -c Release"; // ��������
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = false; // ����Ҫ�ض����׼���
        process.StartInfo.RedirectStandardError = false; // ����Ҫ�ض����׼����

        process.Start();
        process.WaitForExit(); // �ȴ��������

        // ȷ�� DLL ���ɳɹ��󣬸��Ƶ�ָ��·��
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