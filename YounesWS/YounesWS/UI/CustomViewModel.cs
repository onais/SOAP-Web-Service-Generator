using Microsoft.AspNet.Scaffolding;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace YounesWS.UI
{
    /// <summary>
    /// View model for code types so that it can be displayed on the UI.
    /// </summary>
    public class CustomViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The code generation context</param>
        public CustomViewModel(CodeGenerationContext context)
        {
            Context = context;
        }

        /// <summary>
        /// This gets all the Model types from the active project.
        /// </summary>
        public IEnumerable<ModelType> ModelTypes
        {
            get
            {
                ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));

                return codeTypeService
                    .GetAllCodeTypes(Context.ActiveProject)
                    .Select(codeType => new ModelType(codeType))
                      .Where(codeType => codeType.CodeType.FullName.Contains("Models"))
                      .OrderBy(codeType => codeType.CodeType.Name);
                    
            }
        }


        public IEnumerable<ModelType> ModelTypes2
        {
            get
            {
                ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));

                return codeTypeService
                    .GetAllCodeTypes(Context.ActiveProject)
                       .Select(codeType => new ModelType(codeType))
                      .Where(codeType => codeType.CodeType.FullName.Contains("Entities"));
            }
        }


        public IEnumerable<ModelType> ModelTypes3
        {
            get
            {
                ICodeTypeService codeTypeService = (ICodeTypeService)Context
                    .ServiceProvider.GetService(typeof(ICodeTypeService));

                return codeTypeService
                    .GetAllCodeTypes(Context.ActiveProject)
                      .Where(codeType => codeType.FullName.Contains("_Result"))
                    .Select(codeType => new ModelType(codeType))
                    .OrderBy(codeType => codeType.CodeType.Name);
            }
        }


        public ModelType SelectedModelType { get; set; }
        public ModelType SelectedModelType2 { get; set; }

        public ModelType SelectedModelType3 { get; set; }


        public string regions { get; set; }

        public CodeGenerationContext Context { get; private set; }
    }
}
