using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YounesWS
{
    class CGU
    {
        public static string getParamsString<T>(string methodName)
        {
            MethodInfo methodInfo = typeof(T).GetMethod(methodName);
            string typeName;
            string result = "";
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }
                result += typeName + " " + pParameter.Name + " ,";
            }
            result = result.Remove(result.Length - 1);
            return result;
        }


        public static string[,] getParamsMatrix<T>(string methodName)
        {
            MethodInfo methodInfo = typeof(T).GetMethod(methodName);
            string typeName;
            string[,] result = new string[30, 2];
            int i = 0;
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }
                result[i, 0] = typeName;
                result[i, 1] = pParameter.Name;
            }

            return result;
        }

    }
}
