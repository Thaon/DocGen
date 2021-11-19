using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
public class Docs : System.Attribute
{
    string desc;

    public Docs(string desc)
    {
        this.desc = desc;
    }

    public string GetDocs()
    {
        return desc;
    }
}

public class Generator : MonoBehaviour
{
    public class ClassInfo
    {
        public ClassInfo()
        {
            nameSpace = "";
            name = "";
            variables = new List<KeyValuePair<string, string>>();
            methods = new List<KeyValuePair<string, string>>();
        }

        public string nameSpace = "";
        public string name = "";
        public List<KeyValuePair<string, string>> variables;
        public List<KeyValuePair<string, string>> methods;

        public void PrettyPrint()
        {
            string toPrint = "";
            //NameSpace and Class name
            if (nameSpace != null && nameSpace.Length > 0)
                toPrint += ("# NameSpace: " + nameSpace) + "\n";
            toPrint += ("## " + name) + "\n";
            toPrint += ("---") + "\n";

            //Member variables
            toPrint += ("### Fields") + "\n";
            variables.ForEach(v =>
            {
                if (v.Value.Length > 0)
                    toPrint += ("> " + v.Value) + "\n";
                toPrint += ("```cs\n" + v.Key + "\n```") + "\n";
            });

            //Methods
            toPrint += ("### Methods") + "\n";
            methods.ForEach(m =>
            {
                if (m.Value.Length > 0)
                    toPrint += ("> " + m.Value) + "\n";
                toPrint += ("```cs\n" + m.Key + "()\n```") + "\n";
            });

            print(toPrint);
        }
    }

    void Start()
    {
        var info = GetClassInfo(typeof(ExampleClass));
        info.PrettyPrint();

        //typeof(ExampleClass).GetMembers().ToList().ForEach(f => print(f.Name));
    }

    public ClassInfo GetClassInfo<T>(T target) where T : System.Type
    {
        ClassInfo info = new ClassInfo();

        info.nameSpace = target.Namespace;
        info.name = target.Name;
        target.GetFields().ToList().ForEach(p =>
        {
            if (p.DeclaringType == target)
                info.variables.Add(GetNameAndDescription(p));
        });
        target.GetMethods().ToList().ForEach(m =>
        {
            if (m.DeclaringType == target)
                info.methods.Add(GetNameAndDescription(m));
        });

        return info;
    }

    private KeyValuePair<string, string> GetNameAndDescription(System.Reflection.FieldInfo o)
    {
        KeyValuePair<string, string> kvp;
        //gather description
        object[] displayAttributes = o.GetCustomAttributes(typeof(Docs), true);
        string attrDesc = "";
        if (displayAttributes != null && displayAttributes.Length == 1)
            attrDesc = ((Docs)displayAttributes[0]).GetDocs();
        //get name
        kvp = new KeyValuePair<string, string>(o.FieldType.Name + " " + o.Name, attrDesc);
        return kvp;
    }

    private KeyValuePair<string, string> GetNameAndDescription(System.Reflection.MethodInfo o)
    {
        KeyValuePair<string, string> kvp;
        //gather description
        object[] displayAttributes = o.GetCustomAttributes(typeof(Docs), true);
        string attrDesc = "";
        if (displayAttributes != null && displayAttributes.Length == 1)
            attrDesc = ((Docs)displayAttributes[0]).GetDocs();
        //get name
        kvp = new KeyValuePair<string, string>(o.ReturnType.Name + " " + o.Name, attrDesc);
        return kvp;
    }
}
