using System;
using UnityEngine;
using UnityEngine.UI;

namespace Environments
{
    public class BackgroundScroll : MonoBehaviour
    {
        [SerializeField] private float parallaxOffset;
        
        private SpriteRenderer _spriteRenderer;
        private Material _backgroundMaterial;
        private float _currentScroll;
        private float _ratio;
        private Transform _mainCamTrm;
        private float _beforePosition;

        private readonly int _offsetHash = Shader.PropertyToID("_Offset"); //이건 static안돼. 오타내지마라.

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _backgroundMaterial = _spriteRenderer.material; 
            //이미지의 경우 SharedMaterial을 사용한다. 따라서 인스턴싱해야한다.
            
            _currentScroll = 0;
            _ratio = 1f / _spriteRenderer.bounds.size.x; //스프라이트 렌더러는 카메라 바운딩 박스가 존재한다.
            _mainCamTrm = Camera.main.transform;
        }

        private void Start()
        {
            _beforePosition = _mainCamTrm.position.x;
        }

        private void LateUpdate()
        {
            float delta = _mainCamTrm.position.x - _beforePosition;
            _beforePosition = _mainCamTrm.position.x;
            _currentScroll += delta * parallaxOffset * _ratio;
            _backgroundMaterial.SetVector(_offsetHash, new Vector2(_currentScroll, 0));
        }
    }
}