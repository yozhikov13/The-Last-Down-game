using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class WeaponChangeUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public float angle = 30;
    public float speed = 10;
    public Vector2 dragStart;
    private IEnumerator currentAnimation;
    [SerializeField]
    private Image[] images;
    [SerializeField]
    private UnityEvent leftPress;
    [SerializeField]
    private UnityEvent rightPress;

    private void Start()
    {
        /*
         * 
         * Нужно получать изображение оружия.
        var refer = InputManager.Instance.pClass.pWeapon.weaponClassReferences;
        for (int i = 0; i < refer.Length; i++)
        {
            refer[i].
        }
        images[0].image
        */
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStart = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.rotation *= Quaternion.Euler(0, 0, -eventData.delta.x * Time.deltaTime * speed);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (dragStart != Vector2.zero)
        {
            if (Mathf.Sign(dragStart.x - eventData.position.x) >= 0)
            {
                PlayLeftRotation();
            }
            else
            {
                PlayRightRotation();
            }
            dragStart = Vector2.zero;
        }
    }

    // Start is called before the first frame update
    public void PlayLeftRotation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = Rotation(false);
        StartCoroutine(currentAnimation);
        leftPress.Invoke();
    }
    public void PlayRightRotation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = Rotation(true);
        StartCoroutine(currentAnimation);
        rightPress.Invoke();
    }

    IEnumerator Rotation(bool right)
    {
        Quaternion rotateTo;
        if(right)
        {
            rotateTo = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            rotateTo = Quaternion.Euler(0, 0, -angle);
        }
        while (gameObject.transform.rotation != rotateTo)
        {
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, rotateTo, Time.deltaTime * speed);
            yield return null;
        }
        while (gameObject.transform.rotation != Quaternion.identity)
        {
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.identity, Time.deltaTime * speed);
            yield return null;
        }
    }
}
