using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private Camera _mainCam;
    private Sprite _sprite;
    
    private void Awake()
    {
        _mainCam = Camera.main;

        if (spriteRenderer != null)
        {
            _sprite = spriteRenderer.sprite;
        }
    }
    
    private void Update()
    {
        ResizeSprite();
    }

    private void ResizeSprite()
    {
        if (spriteRenderer == null || _sprite == null)
        {
            return;
        }

        // Получаем размеры спрайта
        float width = _sprite.bounds.size.x;
        float height = _sprite.bounds.size.y;

        // Получаем размеры экрана в мировых координатах
        float worldScreenHeight = _mainCam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * _mainCam.aspect;

        // Масштабируем по ширине экрана
        float widthMultiplier = worldScreenWidth / width;

        // Масштабируем по высоте экрана (если нужно)
        float heightMultiplier = worldScreenHeight / height;

        // Находим наибольший множитель, чтобы объект не "искажался"
        float finalMultiplier = Mathf.Max(widthMultiplier, heightMultiplier);

        // Применяем масштаб
        transform.localScale = Vector3.one * finalMultiplier;

        // Привязка по Y (низ бэкграунда к низу экрана)
        float worldScreenBottom = _mainCam.transform.position.y - _mainCam.orthographicSize;
        Vector3 newPosition = transform.position;
        newPosition.y = worldScreenBottom + (_sprite.bounds.size.y * finalMultiplier / 2f);
        transform.position = newPosition;
    }
}