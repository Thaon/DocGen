using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MADD
{
    #region utility classes

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Docs : Attribute
    {
        readonly string desc;

        public Docs(string desc)
        {
            this.desc = desc;
        }

        public string GetDocs()
        {
            return desc;
        }
    }

    #endregion

    [ExecuteInEditMode]
    public class DocGen : EditorWindow
    {
        #region nested classes

        public struct MemberInfo
        {
            public string type;
            public string name;
            public string docs;
        }
        public class ClassInfo
        {
            public ClassInfo()
            {
                nameSpace = "";
                name = "";

                superClasses = new List<string>();
                variables = new List<MemberInfo>();
                methods = new List<MemberInfo>();
            }

            public string nameSpace = "";
            public string name = "";
            public string classDocs = "";
            public List<string> superClasses;
            public List<MemberInfo> variables;
            public List<MemberInfo> methods;

            public void GenerateMarkdown(bool useTables)
            {
                string previousInheritance = "";
                string toPrint = "";
                //NameSpace and Class name
                if (nameSpace != null && nameSpace.Length > 0)
                    toPrint += ("# NameSpace: " + nameSpace) + "\n";
                toPrint += ("## " + name) + "\n";
                toPrint += ("> " + classDocs) + "\n";
                //Super classes
                if (superClasses.Count > 0)
                {
                    toPrint += ("##### Inherits from:\n");
                    foreach (var super in superClasses)
                    {
                        if (super != previousInheritance)
                        {
                            toPrint += (" - " + super) + "\n";
                            previousInheritance = super;
                        }
                    }
                    toPrint += ("---") + "\n";
                }

                //Member variables
                toPrint += ("### Fields") + "\n";
                toPrint += PrettyPrintMembers(variables, useTables);

                //Methods
                toPrint += ("### Methods") + "\n";
                toPrint += PrettyPrintMembers(methods, useTables);

                WriteDocFile(name, toPrint);
            }

            private string PrettyPrintMembers(List<MemberInfo> members, bool useTables)
            {
                string toPrint = "";
                if (useTables)
                {
                    toPrint += "|type|name|docs|\n";
                    toPrint += "|---|---|---|\n";
                }
                members.ForEach(m =>
                {
                    string docs = m.docs.Length > 0 ? m.docs : " - ";
                    if (useTables)
                    {
                        toPrint += "|" + m.type + "|" + m.name + "|" + docs + "|\n";
                    }
                    else
                    {
                        toPrint += ("```cs\n" + m.type + " " + m.name + "\n```") + "\n";
                        if (m.docs.Length > 0)
                            toPrint += ("> " + m.docs) + "\n";
                    }
                });
                return toPrint + "\n";
            }

            private void WriteDocFile(string name, string documentation)
            {
                if (!Directory.Exists("Assets/Documentation"))
                {
                    Directory.CreateDirectory("Assets/Documentation");
                }
                string path = "Assets/Documentation/" + name + ".md";
                //Write some text to the test.txt file
                StreamWriter writer = new StreamWriter(path, false);
                writer.Write(documentation);
                writer.Close();
                //Re-import the file to update the reference in the editor
                AssetDatabase.ImportAsset(path);
            }
        }

        #endregion

        #region member variables

        public DocumentationSettings settings;

        private Texture2D _logo = null;

        #endregion

        [MenuItem("MADD/DocGen")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DocGen));
        }

        private void OnEnable()
        {
            _logo = (Texture2D)Resources.Load("DocImg", typeof(Texture2D));
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(_logo, centeredStyle,GUILayout.MaxHeight(200), GUILayout.MinHeight(10), GUILayout.MaxWidth(200), GUILayout.MinWidth(10));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            settings = EditorGUILayout.ObjectField("", settings, typeof(DocumentationSettings), true, GUILayout.MaxWidth(200), GUILayout.MinWidth(10)) as DocumentationSettings;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate!", GUILayout.MaxWidth(200), GUILayout.MinWidth(10)))
            {
                if (settings != null) GenerateDocumentation();
                else
                {
                    Debug.LogWarning("Please select a documentation settngs file.");
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            EditorGUILayout.Space(10);
        }

        public void GenerateDocumentation()
        {
            settings.Classes.ForEach(c =>
            {
                ClassInfo info = GetClassInfo(c);
                info.GenerateMarkdown(settings.useTables);
            });
        }

        public ClassInfo GetClassInfo(string typeName)
        {
            Type target = Type.GetType(typeName);
            if (target == null)
            {
                Debug.LogWarning("Type '" + typeName + "' Could not be found, have you double checked the spelling?");
                return null;
            }

            ClassInfo info = new ClassInfo();

            info.nameSpace = target.Namespace;
            info.name = target.Name;
            //get class docs
            object[] documentation = target.GetCustomAttributes(typeof(Docs), true);
            if (documentation != null && documentation.Length == 1)
                info.classDocs = ((Docs)documentation[0]).GetDocs();

            for (int i = 0; i < 10; i++)
            {
                var type = target.BaseType;

                if (type != null && type != typeof(MonoBehaviour))
                    info.superClasses.Add(type.ToString());

            }
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

        private MemberInfo GetNameAndDescription(FieldInfo o)
        {
            MemberInfo info;
            //gather description
            object[] documentation = o.GetCustomAttributes(typeof(Docs), true);
            string attrDesc = "";
            if (documentation != null && documentation.Length == 1)
                attrDesc = ((Docs)documentation[0]).GetDocs();
            //get info
            info = new MemberInfo();
            info.type = o.FieldType.Name;
            info.name = o.Name;
            info.docs = attrDesc;
            return info;
        }

        private MemberInfo GetNameAndDescription(MethodInfo o)
        {
            MemberInfo info;
            //gather description
            object[] documentation = o.GetCustomAttributes(typeof(Docs), true);
            string attrDesc = "";
            if (documentation != null && documentation.Length == 1)
                attrDesc = ((Docs)documentation[0]).GetDocs();
            //get name
            //get info
            info = new MemberInfo();
            info.type = o.ReturnType.Name;
            info.name = o.Name;
            info.docs = attrDesc;
            return info;
        }
    }
}
