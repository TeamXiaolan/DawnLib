using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dawn.Utils;

[AddComponentMenu($"{DawnConstants.UI}/Marquee")]
public class MarqueeUI : MonoBehaviour
{
    RectTransform _transform, _parent;

    [SerializeField] float _stoppedTime = 1f, _movingTime = 2f;

    Coroutine _currentAnim;

    void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _parent = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_parent.rect.width < _transform.rect.width)
        {
            if (_currentAnim != null)
                return;

            _currentAnim = StartCoroutine(DoAnimation());
        }
        else if (_currentAnim != null)
        {
            StopCoroutine(_currentAnim);
            _currentAnim = null;
            _transform.localPosition = Vector3.zero;
        }
    }

    IEnumerator DoAnimation()
    {
        // wait X seconds
        yield return new WaitForSeconds(_stoppedTime);

        float t = 0;
        Vector3 step = Vector3.left * (_transform.rect.width - _parent.rect.width) / _movingTime;

        // move left for Y seconds
        while (t < _movingTime)
        {
            t += Time.deltaTime;

            _transform.localPosition += step * Time.deltaTime;
            yield return null;
        }

        // wait for X seconds
        yield return new WaitForSeconds(_stoppedTime);

        // move right for Y seconds
        while (t > 0)
        {
            t -= Time.deltaTime;

            _transform.localPosition -= step * Time.deltaTime;
            yield return null;
        }
        _currentAnim = null;
    }
}
