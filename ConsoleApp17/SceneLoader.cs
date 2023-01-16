using Sprache;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;

namespace ConsoleApp17;
internal static class SceneLoader
{
    private static Parser<string> parseIdentifier = Parse.Letter.Then(c => Parse.LetterOrDigit.Many().Select(chars => string.Concat(chars.Prepend(c)))).Token();
    private static Dictionary<string, IEnumerable<SceneElement>> loadedArchetypeFiles = new();
    private static Parser<IEnumerable<SceneElement>> sceneParser = SceneElement.parser.Many().End();

    public static Scene LoadScene(string path)
    {
        var content = File.ReadAllText(path);

        var commentParser = new CommentParser();
        var elements = sceneParser.Commented(commentParser).Parse(content) ?? throw new Exception();

        return CreateScene(elements.Value);
    }

    private static Scene CreateScene(IEnumerable<SceneElement> elements)
    {
        Scene result = new Scene();
        var context = new SceneLoadContext();

        foreach (var element in elements)
        {
            CreateSceneElement(context, result, element);
        }

        return result;
    }

    private static void CreateSceneElement(SceneLoadContext context, Entity parent, SceneElement element)
    {
        if (element is EntityDesc entityDesc)
        {
            CreateEntity(context, parent, entityDesc);
        }
        else if (element is ComponentDesc componentDesc)
        {
            CreateComponent(context, parent, componentDesc);
        }
        else if (element is UsingElement usingElement)
        {
            context.imports.Add(usingElement.Alias, usingElement.Path.Value);
        }
        else
        {
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

    private static IEnumerable<SceneElement> GetArchetype(string path)
    {
        if (loadedArchetypeFiles.TryGetValue(path, out var archetype))
            return archetype;

        var content = File.ReadAllText(path);

        var parsed = sceneParser.Parse(content);

        loadedArchetypeFiles.Add(path, parsed);

        return parsed;
    }

    private static void CreateComponent(SceneLoadContext context, Entity parent, ComponentDesc componentDesc)
    {
        if (context.imports.TryGetValue(componentDesc.TypeName, out var archetype))
        {
            Entity.Create(archetype, parent);
            return;
        }

        var componentType = Assembly.GetExecutingAssembly().GetTypes().Single(t => t.Name == componentDesc.TypeName);

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

        if (componentType is null)
            throw new Exception();

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
                    throw new Exception();
                }
            }
        }
    }

    private static void AttachArchetype(Entity parent, IEnumerable<SceneElement> archetype)
    {
        var context = new SceneLoadContext();

        foreach (var element in archetype)
        {
            CreateSceneElement(context, parent, element);
        }
    }

    public static void AttachArchetype(Entity parent, string archetypePath)
    {
        AttachArchetype(parent, GetArchetype(archetypePath));
    }


    class SceneLoadContext
    {
        public readonly Dictionary<string, string> imports = new();
    }

    record SceneElement(string TypeName)
    {
        public static readonly Parser<SceneElement> parser = 
            Parse.Or<SceneElement>(UsingElement.parser, ComponentDesc.parser)
            .Or(EntityDesc.parser);
    }

    record UsingElement(string Alias, StringPropertyValue Path) : SceneElement(Alias)
    {
        public static new readonly Parser<UsingElement> parser =
            from usingKeyword in Parse.String("using").Token()
            from alias in parseIdentifier
            from equal in Parse.Char('=').Token()
            from path in StringPropertyValue.parser.Token()
            select new UsingElement(alias, path);
    }

    record EntityDesc(IEnumerable<SceneElement> children) : SceneElement(nameof(Entity))
    {
        public static new readonly Parser<EntityDesc> parser =
            from children in Parse.Ref(() => SceneElement.parser).Many().Contained(Parse.Char('{').Token(), Parse.Char('}').Token())
            select new EntityDesc(children);
    }

    record ComponentDesc(string TypeName, IEnumerable<PropertyDesc> PropertyDescs) : SceneElement(TypeName)
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
