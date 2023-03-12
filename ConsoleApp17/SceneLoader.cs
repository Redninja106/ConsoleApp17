using Sprache;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;

namespace ConsoleApp17;
public static class SceneLoader
{
    private static Parser<string> parseIdentifier = Parse.Letter.Then(c => Parse.LetterOrDigit.Many().Select(chars => string.Concat(chars.Prepend(c)))).Token();
    private static Dictionary<string, IEnumerable<SceneElement>> loadedFiles = new();
    private static Parser<IEnumerable<SceneElement>> sceneParser = SceneElement.parser.Many().End();

    public static Scene LoadScene(string path, Assembly? mainAssembly = null)
    {
        mainAssembly ??= Assembly.GetCallingAssembly();

        var elements = ParseSceneFile(path);
        return CreateScene(elements, mainAssembly);
    }

    private static Scene CreateScene(IEnumerable<SceneElement> elements, Assembly mainAssembly)
    {
        Scene result = new Scene();
        var context = new SceneLoadContext(mainAssembly);

        foreach (var element in elements)
        {
            CreateSceneElement(context, result, element);
        }

        return result;
    }

    private static void CreateSceneElement(SceneLoadContext context, Entity parent, SceneElement element)
    {
        switch (element)
        {
            case ExtendElement openElement:
                var baseContext = new SceneLoadContext(context.mainAssembly);
                var baseElements = ParseSceneFile(openElement.Path.Value);
                foreach (var baseElement in baseElements)
                {
                    CreateSceneElement(baseContext, parent, baseElement);
                }
                break;
            case EntityDesc entityDesc:
                CreateEntity(context, parent, entityDesc);
                break;
            case ComponentDesc componentDesc:
                CreateComponent(context, parent, componentDesc);
                break;
            case UsingAliasElement usingElement:
                context.imports.Add(usingElement.Alias, usingElement.Path.Value);
                break;
            case UsingImportElement importElement:
                context.AddSearchAssembly(Assembly.Load(importElement.AssemblyName));
                break;
            default:
                throw new Exception();
        }
    }

    private static void CreateEntity(SceneLoadContext context, Entity parent, EntityDesc desc)
    {
        var entity = Entity.Create(parent);

        foreach (var element in desc.children)
        {
            CreateSceneElement(context, entity, element);
        }
    }

    private static IEnumerable<SceneElement> ParseSceneFile(string path)
    {
        if (loadedFiles.TryGetValue(path, out var archetype))
            return archetype;

        var content = File.ReadAllText(path);

        var parsed = sceneParser.Parse(content);

        loadedFiles.Add(path, parsed);

        return parsed;
    }

    private static void CreateComponent(SceneLoadContext context, Entity parent, ComponentDesc componentDesc)
    {
        if (context.imports.TryGetValue(componentDesc.TypeName, out var archetype))
        {
            Entity.Create(archetype, context.mainAssembly, parent);
            return;
        }

        var componentType = context.FindType(componentDesc.TypeName);

        if (componentType is null)
            throw new Exception();

        if (componentType == typeof(Transform))
        {
            ref var transform = ref parent.Transform;

            foreach (var property in componentDesc.PropertyDescs)
            {
                switch (property.PropertyName)
                {
                    case nameof(Transform.Position):
                        transform.Position = (Vector2)property.Value.GetValue();
                        break;
                    case nameof(Transform.Rotation):
                        transform.Rotation = Convert.ToSingle(property.Value.GetValue());
                        break;
                    case nameof(Transform.Scale):
                        transform.Scale = (Vector2)property.Value.GetValue();
                        break;
                    default:
                        throw new Exception();
                }
            }

            return;
        }

        var component = parent.AddComponent(componentType);

        foreach (var property in componentDesc.PropertyDescs)
        {
            var propInfo = componentType.GetProperty(property.PropertyName);

            if (propInfo is not null)
            {
                propInfo.SetValue(component, Convert.ChangeType(property.Value.GetValue(), propInfo.PropertyType));
            }
            else
            {
                var fieldInfo = componentType.GetField(property.PropertyName);
                if (fieldInfo is not null)
                {
                    fieldInfo.SetValue(component, Convert.ChangeType(property.Value.GetValue(), fieldInfo.FieldType));
                }
                else
                {
                    throw new Exception($"Member \"{property.PropertyName}\" not found.");
                }
            }
        }
    }

    private static void AttachArchetype(Entity parent, IEnumerable<SceneElement> archetype, Assembly mainAssembly)
    {
        var context = new SceneLoadContext(mainAssembly);

        foreach (var element in archetype)
        {
            CreateSceneElement(context, parent, element);
        }
    }

    public static void AttachArchetype(Entity parent, string archetypePath, Assembly? mainAssembly = null)
    {
        mainAssembly ??= Assembly.GetCallingAssembly();

        AttachArchetype(parent, ParseSceneFile(archetypePath), mainAssembly);
    }

    class SceneLoadContext
    {
        public readonly Assembly mainAssembly;
        public readonly List<Assembly> searchAssemblies = new();
        public readonly Dictionary<string, string> imports = new();

        public SceneLoadContext(Assembly mainAssembly)
        {
            this.mainAssembly = mainAssembly;

            AddSearchAssembly(mainAssembly);
            AddSearchAssembly(Assembly.GetExecutingAssembly());
        }

        public Type? FindType(string name)
        {
            Type[] types = searchAssemblies.SelectMany(a => a.GetTypes()).ToArray();
            return types.SingleOrDefault(t => t.Name == name);
        }

        public void AddSearchAssembly(Assembly assembly)
        {
            if (searchAssemblies.Contains(assembly))
                return;

            searchAssemblies.Add(assembly);
        }
    }

    record SceneElement()
    {
        public static readonly Parser<SceneElement> parser = 
            Parse.Or<SceneElement>(
                UsingAliasElement.parser,
                UsingImportElement.parser)
            .Or(ExtendElement.parser)
            .Or(ComponentDesc.parser)
            .Or(EntityDesc.parser);
    }

    record ExtendElement(StringPropertyValue Path) : SceneElement()
    {
        public static new readonly Parser<ExtendElement> parser =
            from extendKeyword in Parse.String("extend").Token()
            from path in StringPropertyValue.parser.Token()
            select new ExtendElement(path);
    }

    record UsingAliasElement(string Alias, StringPropertyValue Path) : SceneElement()
    {
        public static new readonly Parser<UsingAliasElement> parser =
            from usingKeyword in Parse.String("using").Token()
            from alias in parseIdentifier
            from equal in Parse.Char('=').Token()
            from path in StringPropertyValue.parser.Token()
            select new UsingAliasElement(alias, path);
    }

    record UsingImportElement(string AssemblyName) : SceneElement()
    {

        public static new readonly Parser<UsingImportElement> parser =
            from usingKeyword in Parse.String("using").Token()
            from assemblyName in parseIdentifier.DelimitedBy(Parse.Char('.'))
            select new UsingImportElement(string.Join('.', assemblyName.ToArray()));

    }

    record EntityDesc(IEnumerable<SceneElement> children) : SceneElement()
    {
        public static new readonly Parser<EntityDesc> parser =
            from children in Parse.Ref(() => SceneElement.parser).Many().Contained(Parse.Char('{').Token(), Parse.Char('}').Token())
            select new EntityDesc(children);
    }

    record ComponentDesc(string TypeName, IEnumerable<PropertyDesc> PropertyDescs) : SceneElement()
    {
        public static new readonly Parser<ComponentDesc> parser =
            from typeName in parseIdentifier
            from descs in (
                from open in Parse.Char('(').Token()
                from propertyDescs in PropertyDesc.parser.Many()
                from close in Parse.Char(')').Token()
                select propertyDescs
            ).Optional()
            select new ComponentDesc(typeName, descs.GetOrElse(Enumerable.Empty<PropertyDesc>()));
    }

    record PropertyDesc(string PropertyName, PropertyValue Value)
    {
        public static readonly Parser<PropertyDesc> parser =
            from propertyName in parseIdentifier
            from equalsSign in Parse.Char('=').Token()
            from value in PropertyValue.parser
            select new PropertyDesc(propertyName, value);
    }

    abstract record PropertyValue
    {
        public static readonly Parser<PropertyValue> parser =
            NumberPropertyValue.parser.Or(
                StringPropertyValue.parser.Or<PropertyValue>(
                    Vector2PropertyValue.parser.Or<PropertyValue>(
                            BoolPropertyValue.parser
                        )
                    )
                );

        public abstract object GetValue();
    }

    record NumberPropertyValue(decimal Value) : PropertyValue
    {
        public static new readonly Parser<NumberPropertyValue> parser =
            from sign in Parse.Char('-').Select(c => c.ToString()).Optional()
            from whole in Parse.Digit.AtLeastOnce().Token()
            from rest in (
                from dot in Parse.Char('.')
                from fractional in Parse.Digit.Many().Token()
                select string.Concat(fractional.Prepend(dot))
            ).Optional()
            select new NumberPropertyValue(decimal.Parse($"{sign.GetOrElse(string.Empty)}{string.Concat(whole)}{rest.GetOrElse(string.Empty)}"));

        public override object GetValue() => this.Value;
    }

    record StringPropertyValue(string Value) : PropertyValue
    {
        public static new readonly Parser<StringPropertyValue> parser =
            from openQuote in Parse.Char('"')
            from content in Parse.Until(Parse.AnyChar, Parse.Char('"').Except(Parse.String("\\\"")))
            select new StringPropertyValue(Unescape(string.Concat(content)));

        private static readonly Dictionary<string, string> escapes = new Dictionary<string, string>()
        {
            { "\\\"", "\"" },
        };

        private static string Unescape(string content)
        {
            foreach (var (escapeKey, escapeValue) in escapes)
            {
                content = content.Replace(escapeKey, escapeValue);
            }

            return content;
        }

        public override object GetValue() => this.Value;
    }

    record Vector2PropertyValue(NumberPropertyValue X, NumberPropertyValue Y) : PropertyValue
    {
        public static new readonly Parser<Vector2PropertyValue> parser =
            from open in Parse.Char('(').Token()
            from x in NumberPropertyValue.parser
            from comma in Parse.Char(',').Token()
            from y in NumberPropertyValue.parser
            from close in Parse.Char(')').Token()
            select new Vector2PropertyValue(x, y);

        public override object GetValue() => new Vector2((float)(decimal)X.GetValue(), (float)(decimal)Y.GetValue());
    }

    record BoolPropertyValue(bool Value) : PropertyValue
    {
        public static new readonly Parser<BoolPropertyValue> parser =
            from literal in Parse.String("true").Or(Parse.String("false")).Token()
            select new BoolPropertyValue(bool.Parse(string.Concat(literal)));

        public override object GetValue() => this.Value;
    }
}
