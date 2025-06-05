using System;
using System.Diagnostics;
using System.IO;
using InteractiveExtension.Services;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.Util;

namespace InteractiveExtension.Actions
{
    [Action("InteractiveExtension.RunCSharpScript", "Run C# Script")]
    public class RunCSharpScriptAction : IExecutableAction
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            // Only enable for .cs files
            var projectFile = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT) as IProjectFile;
            if (projectFile == null)
                return false;

            // Check if it's a .cs file
            if (!projectFile.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                return false;

            // Enable the action
            presentation.Visible = true;
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
            if (solution == null)
                return;

            var projectFile = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT) as IProjectFile;
            if (projectFile == null)
                return;

            var scriptService = solution.GetComponent<CSharpScriptService>();
            scriptService.RunScript(projectFile.Location.FullPath);
        }
    }
}