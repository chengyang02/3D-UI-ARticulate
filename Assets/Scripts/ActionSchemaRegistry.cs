using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionSchemaRegistry : MonoBehaviour
{
    public static ActionSchemaRegistry Instance { get; private set; }

    public class ActionArgument
    {
        public string Name;
        public bool IsRequired;

        public ActionArgument(string name, bool isRequired)
        {
            Name = name;
            IsRequired = isRequired;
        }
    }

    public class ActionSchema
    {
        public string ActionType;
        public List<ActionArgument> Arguments;

        public ActionSchema(string actionType, List<ActionArgument> arguments)
        {
            ActionType = actionType;
            Arguments = arguments;
        }

        public List<string> GetRequiredArguments()
        {
            return Arguments.FindAll(arg => arg.IsRequired).ConvertAll(arg => arg.Name);
        }

        public List<string> GetOptionalArguments()
        {
            return Arguments.FindAll(arg => !arg.IsRequired).ConvertAll(arg => arg.Name);
        }
    }

    public List<string> ObjectTypes = new List<string>
    {
        "cube",
        "sphere",
        "hat",
        "plant",
        "book",
        "notebook",
        "bench",
        "car",
        "building",
        "tree",
    };

    private Dictionary<string, ActionSchema> schemas;

    public static readonly List<ActionArgument> TargetDescriptionArgs = new()
    {
        new ActionArgument("object_type", true),
        new ActionArgument("color", false),
        new ActionArgument("quantity", false),
        new ActionArgument("location", false),
        new ActionArgument("size", false),
        new ActionArgument("name", false),
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSchemas();
    }

    private void InitializeSchemas()
    {
        schemas = new Dictionary<string, ActionSchema>();

        schemas["selection"] = new ActionSchema("selection", TargetDescriptionArgs);

        schemas["translation"] = new ActionSchema("translation", TargetDescriptionArgs.Concat(new List<ActionArgument> {
            new ActionArgument("direction", true),
            new ActionArgument("distance", false),
        }).ToList());

        schemas["rotation"] = new ActionSchema("rotation", TargetDescriptionArgs.Concat(new List<ActionArgument> {
            new ActionArgument("axis", true),
            new ActionArgument("angle", false),
        }).ToList());

        schemas["scale"] = new ActionSchema("scale", TargetDescriptionArgs.Concat(new List<ActionArgument> {
            new ActionArgument("scale_factor", false),
            new ActionArgument("axis", false)
        }).ToList());
    }

    public ActionSchema GetSchema(string actionType)
    {
        schemas.TryGetValue(actionType, out ActionSchema schema);
        return schema;
    }

    public List<string> GetArguments(string actionType)
    {
        List<string> arguments = new List<string>();
        schemas.TryGetValue(actionType, out ActionSchema schema);
        foreach (ActionArgument arg in schema.Arguments) {
            arguments.Add(arg.Name);
        }
        return arguments;
    }

    public List<string> ListActions()
    {
        return new List<string>(schemas.Keys);
    }

    public bool ValidateArgs(string actionType, Dictionary<string, string> args, out string error)
    {
        var schema = GetSchema(actionType);
        if (schema == null)
        {
            error = $"Unknown action type: {actionType}";
            return false;
        }

        foreach (var req in schema.GetRequiredArguments())
        {
            if (!args.ContainsKey(req))
            {
                error = $"Missing required argument: {req}";
                return false;
            }
        }

        error = null;
        return true;
    }

    public string GetObjectTypesPromptLine()
    {
        return $"Valid object types: {string.Join(", ", ObjectTypes)}.";
    }
}
