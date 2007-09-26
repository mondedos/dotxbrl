using System;
using System.Collections.Generic;
using System.Text;

#region Reflexion
using Microsoft.CSharp;
using System.Reflection;
using System.CodeDom.Compiler;
#endregion

namespace xbrlCodeGenerator
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static Assembly GenerarObjeto(string codigo)
        {
            // creamos un compilador
            string[] assemblyReferences = new string[] { 
                "System.Xml.dll",
                "dotXbrl.dll" };

            CSharpCodeProvider c = new CSharpCodeProvider();


            ICodeCompiler cc = c.CreateCompiler();

            CompilerParameters cp = new CompilerParameters(assemblyReferences);
            // preparamos el codigo que queremos
            cp.TreatWarningsAsErrors = true;
            // lo compilamos con alegria
            CompilerResults results =
            cc.CompileAssemblyFromSource(cp, codigo);
            if (results.Errors.Count == 0)
            {
                // si la compilacion funciono, podemos usar reflection como con cualquier otra clase
                return results.CompiledAssembly;
                //  Type helloType =
                //a.GetType("dotXbrl.Prueba");
                //  object hello = Activator.CreateInstance(helloType);
                //  MethodInfo mi = helloType.GetMethod("HolaMundo");
                //  mi.Invoke(hello, new object[] { });

                //Assembly asm = results.CompiledAssembly;
                //object o = asm.CreateInstance("dotXbrl.Prueba");
            }
#if DEBUG
            else
            { // ouch! insertar manejo de errores aqui 
                Console.WriteLine("Error al compilar el codigo ");
                foreach (CompilerError errores in results.Errors)
                {
                    Console.WriteLine(errores);
                }
                return null;
            }
#endif
        }
    }
}
