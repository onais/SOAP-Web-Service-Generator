using YounesWS.UI;
using Microsoft.AspNet.Scaffolding;
using System.Collections.Generic;
using System;
using Microsoft.AspNet.Scaffolding.Core.Metadata;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using EnvDTE;
using YounesWS.Utils;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.Scaffolding.NuGet;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom;
using System.Linq;

namespace YounesWS
{
    public class CustomCodeGenerator : CodeGenerator
    {
        CustomViewModel _viewModel;
        public delegate void TestDelegate(string message);
        /// <summary>
        /// Constructor for the custom code generator
        /// </summary>
        /// <param name="context">Context of the current code generation operation based on how scaffolder was invoked(such as selected project/folder) </param>
        /// <param name="information">Code generation information that is defined in the factory class.</param>
        public CustomCodeGenerator(
            CodeGenerationContext context,
            CodeGeneratorInformation information)
            : base(context, information)
        {
            _viewModel = new CustomViewModel(Context);
        }



        /// <summary>
        /// Any UI to be displayed after the scaffolder has been selected from the Add Scaffold dialog.
        /// Any validation on the input for values in the UI should be completed before returning from this method.
        /// </summary>
        /// <returns></returns>
        public override bool ShowUIAndValidate()
        {
            // Bring up the selection dialog and allow user to select a model type
            SelectModelWindow window = new SelectModelWindow(_viewModel);
            bool? showDialog = window.ShowDialog();
            return showDialog ?? false;
        }

        /// <summary>
        /// This method is executed after the ShowUIAndValidate method, and this is where the actual code generation should occur.
        /// In this example, we are generating a new file from t4 template based on the ModelType selected in our UI.
        /// </summary>
        public override void GenerateCode()
        {
            // Get the selected code type
            var codeType = _viewModel.SelectedModelType.CodeType;
            var codeType2 = _viewModel.SelectedModelType2.CodeType;
            var filterClass = _viewModel.SelectedModelType3.CodeType;

            string[] regions = _viewModel.regions.Split(';');

            
            // Setup the scaffolding item creation parameters to be passed into the T4 template.
            string str = "";
          string t=  String.Concat(str.Where(s => String.Equals(s+"", (s+"").ToUpper(),StringComparison.Ordinal)));

            string modelTypeName = codeType.FullName;
            string dbContextTypeName = codeType2.FullName;

            string storedProcedureName = filterClass.Name;
            storedProcedureName = storedProcedureName.Substring(0, storedProcedureName.Length - 7);
            // First Scaffold the DB Context
            IEntityFrameworkService efService = (IEntityFrameworkService)Context.ServiceProvider.GetService(typeof(IEntityFrameworkService));

            ModelMetadata modelMetadata = efService.AddRequiredEntity(Context, dbContextTypeName, modelTypeName);

            Type storedClass = Type.GetType(filterClass.FullName);

            string[,] p = getParamsMatrix(dbContextTypeName, storedProcedureName);

            Type contextClass = Type.GetType(dbContextTypeName);

            MethodInfo[] mi = contextClass.GetMethods();
            List<MethodInfo> li = new List<MethodInfo>(mi);

            li = li.Where(i => i.Name.Contains("spWS_")).OrderBy(i=>i.Name).ToList();


            string[,] AllWsParams = new string[100, 4];
            int comp=0;
            bool typeMethod;
            foreach (var item in li)
            {
                 typeMethod= item.ReturnType.Name.Contains("ObjectResult");
                 AllWsParams[comp, 0] = typeMethod.ToString();
                 AllWsParams[comp, 1] = item.Name;
                 AllWsParams[comp, 2] = getParamsStringWithType(dbContextTypeName, item.Name);
                 AllWsParams[comp, 3] = getParamsString(dbContextTypeName, item.Name);
                 comp++;
            }

            var parameters = new Dictionary<string, object>()
            {
                {"ProjectName", Context.ActiveProject.Name},
                {"ContextName", dbContextTypeName},
                 {"AllWsParams",AllWsParams},
                 {"Regions",regions}
            };

            // Add the custom scaffolding item from T4 template.
            this.AddFolder(Context.ActiveProject, @"WebServices");
            this.AddFileFromTemplate(Context.ActiveProject,
                "WebServices\\" + Context.ActiveProject.Name,
                "WebServiceASMX",
                parameters,
                skipIfExists: false);

            this.AddFileFromTemplate(Context.ActiveProject,
            "WebServices\\" + Context.ActiveProject.Name + ".asmx",
            "WebServiceCS",
            parameters,
            skipIfExists: false);

        }


        public static string getParamsString(string context, string methodName)
        {
            MethodInfo methodInfo = Type.GetType(context).GetMethod(methodName);
            string typeName;
            string result = "";
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }

                if (pParameter.Name.Equals("pageNumber"))
                    result += "numPage ,";

                else if (pParameter.Name.Equals("pageSize"))
                    result += "nbrRows ,";

                else result += pParameter.Name + " ,";
                
            }
            if(result.Length>0)
            return result.Substring(0,result.Length-1);
            return result;
        }


        public static string getParamsStringWithType(string context, string methodName)
        {
            MethodInfo methodInfo = Type.GetType(context).GetMethod(methodName);
            string typeName;
            string result = "";
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }


                if (pParameter.Name.Equals("pageNumber"))
                    result += "int numPage ,";

                else if (pParameter.Name.Equals("pageSize"))
                    result += "int nbrRows ,";
                else
                {
                    if (!typeName.Equals("string") && typeName.Contains("Nullable") && typeName.Length > 9)
                    {
                        result += typeName.Substring(16, typeName.Length - 17) + " " + pParameter.Name + " ,";
                    }
                    else
                    {
                        result +=typeName+" "+ pParameter.Name + " ,";
                    }
                }
            }

            if (result.Length > 0)
                return result.Substring(0, result.Length - 1);
            return result;
        }






        public static string getParamsStringForCreateUpdate(string model, string context, string methodName)
        {
            MethodInfo methodInfo = Type.GetType(context).GetMethod(methodName);
            string typeName;
            string result = "";
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }
                result += Char.ToLowerInvariant(model[0]) + model.Substring(1) + "." + Char.ToUpperInvariant(pParameter.Name[0]) + pParameter.Name.Substring(1) + " ,";
            }
            return result.Substring(0, result.Length - 1);
        }




        public static string[,] getParamsMatrixForCreateUpdate(string context, string methodName)
        {
            MethodInfo methodInfo = Type.GetType(context).GetMethod(methodName);
            string typeName;
            string[,] result = new string[1000, 2];
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
                    i++;
                
            }

            return result;
        }


        public static string[,] getParamsMatrix(string context, string methodName)
        {
            MethodInfo methodInfo = Type.GetType(context).GetMethod(methodName);
            string typeName;
            string[,] result = new string[1000, 2];
            int i = 0;
            foreach (ParameterInfo pParameter in methodInfo.GetParameters())
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(pParameter.ParameterType);
                    typeName = provider.GetTypeOutput(typeRef);
                }

                if (!pParameter.Name.Equals("iSortCol") && !pParameter.Name.Equals("sSortDir") && !pParameter.Name.Equals("pageNumber") && !pParameter.Name.Equals("pageSize"))
                {

                    if (!typeName.Equals("string") && typeName.Contains("Nullable") && typeName.Length > 9)
                    {
                        result[i, 0] = typeName.Substring(16, typeName.Length - 17) + "?";
                        result[i, 1] = pParameter.Name;
                    }
                    else
                    {
                        result[i, 0] = typeName;
                        result[i, 1] = pParameter.Name;
                    }
                    i++;
                }
            }

            return result;
        }

        public static string[,] getPropsMatrix(string fullname, string model)
        {
            string[,] result = new string[1000, 2];
            int i = 0;
            Type storedClass = Type.GetType(fullname);
            PropertyInfo[] mb = storedClass.GetProperties(BindingFlags.DeclaredOnly |
            BindingFlags.Public |
            BindingFlags.Instance);
            IList<PropertyInfo> props = new List<PropertyInfo>(mb);

            string str = "";

            foreach (var prop in props)
            {
                if (!prop.Name.Equals("TotalRows") && !prop.Name.Equals(Char.ToUpperInvariant(model[0]) + model.Substring(1) + "Id"))
                {
                        result[i, 0] = prop.PropertyType.ToString();
                        result[i, 1] = prop.Name;
                        i++;
                }
               
            }
            return result;
        }


    }
}
