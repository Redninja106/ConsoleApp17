using Sprache;
using System.Numerics;
using System.Reflection;

namespace ConsoleApp17;
internal static class SceneLoader
{
    private static Parser<string> parseIdentifier = Parse.Letter.Then(c => Parse.LetterOrDigit.Many().Select(chars => string.Concat(chars.Prepend(c)))).Token();

    public static Scene LoadScene(string path)
    {
        var content = File.ReadAllText(path);

        var parser = SceneElement.parser.Many().End();

        var commentParser = new CommentParser();
        var elements = parser.Commented(commentParser).Parse(content) ?? throw new Exception();

        return CreateScene(elements.Value);
    }

    private static Scene CreateScene(IEnumerable<SceneElement> elements)
    {
        Scene result = new Scene();

        foreach (var element in elements)
        {
            CreateSceneElement(result, element);
        }

        return result;
    }

    private static void CreateSceneElement(Entity parent, SceneElement element)
    {
        if (element is EntityDesc entityDesc)
        {
            CreateEntity(parent, entityDesc);
        }
        else if (element is ComponentDesc componentDesc)
        {
            CreateComponent(parent, componentDesc);
        }
        else
        {
            throw new Exception();
        }
    }

    private static void CreateEntity(Entity parent, EntityDesc desc)
    {
        var entity = Entity.Create(parent, null);

        foreach (var element in desc.children)
        {
            CreateSceneElement(entity, element);
        }
    }

    private static void CreateComponent(Entity parent, ComponentDesc componentDesc)
    {
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

    record SceneElement(string TypeName)
    {
        public static readonly Parser<SceneElement> parser = Parse.Or<SceneElement>(EntityDesc.parser, ComponentDesc.parser);
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
