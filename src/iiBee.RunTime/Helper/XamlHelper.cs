using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using NLog;
using System.Xaml;

namespace iiBee.RunTime.Helper
{
    public static class XamlHelper
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        private static DynamicActivity GetActivityFromString(string workflow)
        {
            XamlXmlReaderSettings settings = new XamlXmlReaderSettings();
            settings.LocalAssembly = Assembly.GetExecutingAssembly();

            using (XamlXmlReader reader = new XamlXmlReader(workflow, settings))
            {
                DynamicActivity activity = ActivityXamlServices.Load(reader) as DynamicActivity;
                return activity;
            }
        }

        public static List<WorkflowArgument> GetArgumentsNames(string workflow)
        {
            DynamicActivity act = GetActivityFromString(workflow);

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

        public static Dictionary<string, object> ConvertDictionary(Dictionary<string, string> dic, List<WorkflowArgument> args)
        {
            Dictionary<string, object> workflowInput = new Dictionary<string, object>();

            foreach (WorkflowArgument arg in args.Where(x => (x.Type == ArgumentType.In) || (x.Type == ArgumentType.InOut)))
            {
                if (!dic.Keys.Contains(arg.Name))
                {
                    log.Warn("Argument[" + arg.Name + "] is missing");
                    continue;
                }

                workflowInput.Add(arg.Name, Convert.ChangeType(dic[arg.Name], arg.InnerType));
            }

            return workflowInput;
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
