# JSONSharp
Simple JSON Parser and Unparser Library for C#. The library has support for many differenet object types, and most objects can be easily extended with the `IParser` Interface.

## JSON Parsing
By default the JSONSharp parser supports Serialization of either `IParser` Objects, or `Dictionary<String, Object>` types.

Here is an example of parsing a `Dictionary<String, Object>`:
```CS
Dictionary<String, Object> person = new Dictionary<string, object>();
person["name"] = "Dominic";
person["sex"] = "Male";
person["likes"] = new String[]{"Coding", "Coffee", "Video Games" };

Dictionary<String, Object> skills = new Dictionary<string, object>();
skills["programming"] = new String[]{"C#", "PHP", "Java", "Javascript", "HTML" };
skills["gaming"] = new String[]{"RPG", "Racing", "Shooter" };
person["skills"] = skills;
person["age"] = 22;

Debug.WriteLine(Serializer.serialize(person));
//PRINTS:
//{"name":"Dominic","sex":"Male","likes":["Coding","Coffee","Video Games"],"skills":{"programming":["C#","PHP","Java","Javascript","HTML"],"gaming":["RPG","Racing","Shooter"]},"age":22}
```

As previously mentioned, Classes can inherit the ISerializable interface, which should be familiar for those used to the PHP JsonSerializable interace.

Here's an example:

**Person.cs:**
```CS
public class Person : ISerializable {
    private string name;
    private int age;
    private List<String> likes = new List<string>();
    
    public Person(string name, int age) {
        this.name = name;
        this.age = age;
    }

    public void addLike(string like) {this.likes.Add(like); }

    public void deserialize(Dictionary<string,object> values) {
        throw new NotImplementedException();
    }

    public Dictionary<string,object> serialize() {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["name"] = name;
        data["age"] = age;
        data["likes"] = likes;
        return data;
    }
}
```

**Main.cs:**
```CS
Person me = new Person("Dominic", 22);
me.addLike("Programming");
me.addLike("Coffee");
me.addLike("Games");
Debug.WriteLine(Serializer.serialize(me));
//Prints: {"name":"Dominic","age":22,"likes":["Programming","Coffee","Games"]}

Person you = new Person("John", 25);
you.addLike("Going outside");
you.addLike("Fresh Air");
you.addLike("Something normal people do");
Debug.WriteLine(Serializer.serialize(you));
//Prints: {"name":"John","age":25,"likes":["Going outside","Fresh Air","Something normal people do"]}
```


## JSON Unparsing
The lightweight library can also handle unserializing, however a few issues do occur with this, but I'll get to that later.
Objects unparsed/unserialized will be stored in a Dictionary<string, object>, and nested objects will be in the same format.

Here are a few examples of unserialization:
**Main.cs:**
```CS
string text = System.IO.File.ReadAllText(@"test.json");
Dictionary<string, object> data = Serializer.unserialize(text);
foreach(string key in data.Keys) {
    Debug.WriteLine(key + ": " + data[key]);
}
/*
 * Prints:
 * name: Dominic
 * age: 22
 * likes: System.Object[]
 */
```

**test.json:**
```JSON
{
    "name": "Dominic",
    "age": 22,
    "likes": [
        "programming",
        "gaming", 
        "coffee"
    ]
}
```

Objects can also be deserialized directly into an ISerializable object (messily)

Example:

**Main.cs:**
```CS
string text = System.IO.File.ReadAllText(@"test.json");
Person whatever = new Person("invalid", 99);
whatever.deserialize(Serializer.unserialize(text));
Debug.WriteLine("Hello " + whatever.getName());
Debug.WriteLine("Age: " + whatever.getAge());
Debug.WriteLine("Likes: ");
foreach(string like in whatever.getLikes()) {
    Debug.WriteLine("\t"+like);
}
```

**Person.cs**
```CS
public class Person : ISerializable {
    private string name;
    private int age;
    private List<String> likes = new List<string>();
    
    public Person(string name, int age) {
        this.name = name;
        this.age = age;
    }

    public string getName() {return name; }
    public int getAge() {return age; }
    public List<String> getLikes() {return likes; }

    public void deserialize(Dictionary<string,object> values) {
        this.name = (string)values["name"];
        this.age = Int32.Parse(values["age"].ToString());
        this.likes = new List<string>();
        foreach(object o in (object[])values["likes"]) {
            this.likes.Add((string)o);
        }
    }

    public Dictionary<string,object> serialize() {
        throw new NotImplementedException();
    }
}
```

**test.json**
```json
{
    "name": "Dominic",
    "age": 22,
    "likes": [
        "programming",
        "gaming", 
        "coffee"
    ]
}
```

As easy as that!

## To Note
As I mentioned above there are a few issues when it comes to unserialization, most notably is the fact that the unserializer has to "assume" what the data type it's given is, I tend to use a "most likely" scenario but that may not be perfect for everyone.

In the future I may allow a JSON Markup to be passed to the unserializer that will force certain types of unserialization but until then this is how it works.


Given the following JSON:
`"test": 4`
Any of the following COULD be true:
```CS
test is byte;
test is short;
test is int;
test is long;
test is float;
test is double;
test is sbyte;
test is ushort;
test is uint;
test is ulong;
test is udobule;
```
.. and so on...
In this case I would make the assumption that `test` is a `byte`, since it is the most likely.
If test was changed to be 1000, then the parser would asume `test` is a `short`.

Other times that this can be an issue besides numbers is arrays...
Given that we were using a `List<string>` on our `Person.cs` class, it was stored as a `string[]` when unserialized, this is because the parser assumes arrays by default... so you will need to convert the resulting array back to a list when unserializing.


I will work on this library from time to time, however it's not a major priority for me right now, send me an email (dominic@domsplace.com) if you need something added/changed/fixed urgently and I'll see to it asap.

## Plans
Some future plans I would like to eventually add:
 * Pass a Schema or Data Set to Unserializer to guarantee data types are correct
 * Improve efficiency, the parsing and unserializing are fairly poor at the moment
 * Clean Code, the thing is rather spaghettified at the moment
 * Add custom parser/deparsers, this can be done now but could be "extended" in the future.
 * "JSON Pretty Printing" and other formatting, currently only does a minified JSON print.


 