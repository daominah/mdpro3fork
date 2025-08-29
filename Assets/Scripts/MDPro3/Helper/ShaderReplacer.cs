using System.Collections;
using UnityEngine;

namespace MDPro3
{
    [RequireComponent(typeof(Renderer))]
    public class ShaderReplacer : MonoBehaviour
    {
        [SerializeField] private string shaderName;

        private bool replaced;

        private Renderer _renderer;

        private Coroutine coroutine;

        private void OnEnable()
        {
            ReplaceShader();
        }

        private void OnDisable()
        {
            if(coroutine != null)
                StopCoroutine(coroutine);
            coroutine = null;
        }

        private void ReplaceShader()
        {
            if(_renderer == null)
                _renderer = GetComponent<Renderer>();
            if(_renderer == null || string.IsNullOrEmpty(shaderName) || replaced)
                return;

            coroutine = StartCoroutine(ReplaceShaderAsync());
        }

        private IEnumerator ReplaceShaderAsync()
        {
            _renderer.enabled = false;

            var load = MaterialLoader.LoadShaderByNameAsync(shaderName);
            while (load.MoveNext())
                yield return null;

            _renderer.material.shader = load.Current;
            _renderer.enabled = true;
            replaced = true;
        }
    }
}