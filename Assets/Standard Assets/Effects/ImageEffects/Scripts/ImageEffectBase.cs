using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("")]
  public class ImageEffectBase : MonoBehaviour
  {
    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader shader;

    private Material _material;

    protected Material material
    {
      get
      {
        if (_material == null)
        {
          _material = new Material(shader);
          _material.hideFlags = HideFlags.HideAndDontSave;
        }
        return _material;
      }
    }


    protected virtual void OnDisable()
    {
      if (_material)
      {
        DestroyImmediate(_material);
      }
    }
  }
}
