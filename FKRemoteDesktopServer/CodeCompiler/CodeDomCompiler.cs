using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.CodeCompiler
{
    public static class CodeDomCompiler
    {
        public static bool CompileToExe(string code, string outputExePath, out string[] errors)
        {
            try
            {
                // 创建 C# 编译器
                using (var provider = new CSharpCodeProvider())
                {
                    // 配置编译参数
                    var parameters = new CompilerParameters
                    {
                        GenerateExecutable = true, // 生成 EXE（而非 DLL）
                        OutputAssembly = outputExePath, // 输出路径
                        CompilerOptions = "/optimize /platform:anycpu", // 优化并支持任意 CPU
                        IncludeDebugInformation = false
                    };

                    // 添加 .NET Framework 4.6.2 的核心引用
                    parameters.ReferencedAssemblies.Add("mscorlib.dll");
                    parameters.ReferencedAssemblies.Add("System.dll");
                    parameters.ReferencedAssemblies.Add("System.Core.dll");
                    parameters.ReferencedAssemblies.Add("System.IO.dll");

                    // 编译代码
                    CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                    // 检查编译是否成功
                    if (results.Errors.HasErrors)
                    {
                        var errorList = new List<string>();
                        foreach (CompilerError error in results.Errors)
                        {
                            errorList.Add($"Error ({error.ErrorNumber}): {error.ErrorText} (Line {error.Line})");
                        }
                        errors = errorList.ToArray();
                        return false;
                    }

                    errors = Array.Empty<string>();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errors = new[] { $"Payload代码编译失败: {ex.Message}" };
                return false;
            }
        }
    }
}
