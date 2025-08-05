using UnityEngine;

[CreateAssetMenu(fileName = "NamespaceSettings", menuName = "Tools/Namespace Settings", order = 1)]
public class NamespaceSettings : ScriptableObject
{
    public string defaultNamespace = "MyCompany.Project";
}
