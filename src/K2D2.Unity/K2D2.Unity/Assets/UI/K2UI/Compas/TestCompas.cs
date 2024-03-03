using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using K2UI.Compas;
using K2UI;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class TestCompas : MonoBehaviour
{
    K2Compass el_compas_int;
    K2Compass el_compas_noint;
    Label el_Label;

    public void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        // root.AddToClassList("compas");
        el_compas_int = root.Q<K2Compass>("interactive");
        el_compas_noint = root.Q<K2Compass>("non_interactive");

        el_Label = root.Q<Label>("Label-Value");

        el_compas_int.RegisterCallback<ChangeEvent<float>>(onValue); 
        el_compas_int.forceValue(50);
    }

    private void onValue(ChangeEvent<float> evt)
    {
        el_Label.text = $"Value : {evt.newValue:n2}°";
        el_compas_noint.Value = evt.newValue;
    }
}