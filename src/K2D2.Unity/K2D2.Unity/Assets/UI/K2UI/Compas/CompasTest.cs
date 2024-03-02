using System.Diagnostics.Tracing;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using K2UI.Compas;
using K2UI;
using System.Linq;
using System.Collections.Generic;

// TODO : créer le composant et revoir la classe
// ajouter la prise en charge de nouveaux styles pour la taille des traits, couleur et hauteur
// ajouter les boutons




[RequireComponent(typeof(UIDocument))]
public class CompasTest : MonoBehaviour
{

    K2Compass el_compas;
    Label el_Label;

    public void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        // root.AddToClassList("compas");
        el_compas = root.Q<K2Compass>();

        el_Label = root.Q<Label>("Label-Value");
        el_compas.RegisterCallback<ChangeEvent<float>>(onValue);
    }

    private void onValue(ChangeEvent<float> evt)
    {
        el_Label.text = $"Value : {evt.newValue}°";
    }
}