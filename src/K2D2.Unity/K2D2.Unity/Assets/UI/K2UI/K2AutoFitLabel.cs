using UnityEngine;
using UnityEngine.UIElements;


namespace K2UI
{
   using System.Collections.Generic;
    using System.IO.Compression;
    using UnityEngine;
using UnityEngine.UIElements;

public class K2AutoFitLabel : Label
{
  [UnityEngine.Scripting.Preserve]
  public new class UxmlFactory : UxmlFactory<K2AutoFitLabel, UxmlTraits> { }

  [UnityEngine.Scripting.Preserve]
  public new class UxmlTraits : Label.UxmlTraits
  {
    public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription { get { yield break; } }

    public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
    {
      base.Init(visualElement, attributes, creationContext);
    }
  }

  public K2AutoFitLabel()
  {
    RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
  }

  private void OnAttachToPanel(AttachToPanelEvent e)
  {
    UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
  }

  private void OnGeometryChanged(GeometryChangedEvent e)
  {
    UpdateFontSize();
    e.StopPropagation();
  }

  private void UpdateFontSize()
  {
    UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    var previousWidthStyle = style.width;

    try
    {
      var width = resolvedStyle.width;
      var height = resolvedStyle.height;

      // Set width to auto temporarily to get the actual width of the label
      style.width = StyleKeyword.Auto;
      var currentFontSize = MeasureTextSize(text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);

      var multiplier = width / Mathf.Max(currentFontSize.x, 1);
      var newFontSize = Mathf.RoundToInt(Mathf.Clamp(multiplier * currentFontSize.y, 1, height));
      Debug.Log("newFontSize"+newFontSize);

      if (Mathf.RoundToInt(currentFontSize.y) != newFontSize)
        style.fontSize = new StyleLength(new Length(newFontSize));
    }
    finally
    {
      style.width = previousWidthStyle;
      RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }
  }
}
}
