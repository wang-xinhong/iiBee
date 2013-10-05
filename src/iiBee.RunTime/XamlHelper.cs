using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace GetArgumentsFromWorklow
{
    public static class XamlHelper
    {
        private static DynamicActivity GetActivityFromString(string xaml)
        {
            Activity root = ActivityXamlServices.Load(new StringReader(xaml));
            WorkflowInspectionServices.CacheMetadata(root);

            return root as DynamicActivity;
        }

        public static List<WorkflowArgument> GetArgumentsNames(string xaml)
        {
            DynamicActivity act = GetActivityFromString(xaml);

            List<WorkflowArgument> args = new List<WorkflowArgument>();
            if (act != null)
            {
                foreach (DynamicActivityProperty p in act.Properties.Where(x => typeof(Argument).IsAssignableFrom(x.Type)))
                {
                    ArgumentType innerType;
                    if(p.Type.BaseType.Name == "InArgument")
                        innerType = ArgumentType.In;
                    else if (p.Type.BaseType.Name == "OutArgument")
                        innerType = ArgumentType.Out;
                    else if (p.Type.BaseType.Name == "InOutArgument")
                        innerType = ArgumentType.InOut;
                    else 
                        throw new Exception("Argument of Type["+ p.Type.BaseType.Name + "] is not supported");

                    args.Add(new WorkflowArgument(p.Name, p.Type.GetGenericArguments()[0], innerType));
                }
            }
            return args;
        }
    }

    public class WorkflowArgument
    {
        public WorkflowArgument(string name, Type innerType, ArgumentType type)
        {
            this.Name = name;
            this.InnerType = innerType;
            this.Type = type;
        }

        /// <summary>
        /// Name of Argument
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// Inner Typer of Argument (int, string, ...)
        /// </summary>
        public Type InnerType { private set; get; }

        /// <summary>
        /// InArgument, OutArgument or InOutArgument
        /// </summary>
        public ArgumentType Type { private set; get; }
    }

    public enum ArgumentType
    {
        In,
        Out,
        InOut
    }
}
