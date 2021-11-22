using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MADD
{
    [CreateAssetMenu(fileName = "Settings", menuName = "MADD/Documentation Settings", order = 1)]
    public class DocumentationSettings : ScriptableObject
    {
        public bool useTables;
        public List<string> Classes;
    }
}
