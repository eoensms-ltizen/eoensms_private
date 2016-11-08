using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class MLoading : Singleton<MLoading>
{
    public int resWidth = 427;
    public int resHeight = 240;
    public float m_baseTime = 1.0f;

    private SpriteRenderer m_spriteRenderer;
    public Texture2D m_texture2D;

    private Camera m_camera;
    private Camera m_original;

    void Awake()
    {
        m_camera = GetComponent<Camera>();
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadingAnimation(sceneName));
    }

    IEnumerator LoadingAnimation(string sceneName)
    {
        yield return StartCoroutine(StandardAnimation(Camera.main));
        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator StandardAnimation(Camera original)
    {
        //! 매인카메라 종료
        m_original = original;
        //! Texture 만들기
        MakeTexture(m_original);
        
        m_camera.gameObject.SetActive(true);
        m_camera.tag = "MainCamera";

        int cullingMask = m_camera.cullingMask;
        m_camera.CopyFrom(m_original);
        m_camera.cullingMask = cullingMask;

        //! 연출
        _2dxFX_Pixel pixel = m_spriteRenderer.GetComponent<_2dxFX_Pixel>();
        
        float time = m_baseTime;
        while (time > 0)
        {
            time -= Time.deltaTime;
            pixel._Offset = Mathf.LerpUnclamped(128, 4, 1 - time / m_baseTime);
            yield return null;
        }
    }
    public IEnumerator ReverseAnimation(Camera original)
    {
        //! 매인카메라 종료
        m_original = original;

        MakeTexture(m_original);
        
        m_camera.gameObject.SetActive(true);
        m_camera.tag = "MainCamera";

        int cullingMask = m_camera.cullingMask;
        m_camera.CopyFrom(m_original);
        m_camera.cullingMask = cullingMask;

        _2dxFX_Pixel pixel = m_spriteRenderer.GetComponent<_2dxFX_Pixel>();
        
        float time = m_baseTime;
        while (time > 0)
        {
            time -= Time.deltaTime;
            pixel._Offset = Mathf.LerpUnclamped(128, 4, time / m_baseTime);
            yield return null;
        }

        m_camera.gameObject.SetActive(false);
        m_original.gameObject.SetActive(true);
        m_camera.tag = "Untagged";
    }

    private void MakeTexture(Camera camera)
    {
        //! Texture 만들기
        camera.gameObject.SetActive(true);
        RenderTexture rendTexture = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rendTexture;
        m_texture2D = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rendTexture;
        m_texture2D.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        m_texture2D.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rendTexture);

        //! Texture로 Sprite 만들기
        Sprite newSprite = Sprite.Create(m_texture2D, new Rect(0f, 0f, m_texture2D.width, m_texture2D.height), new Vector2(0.5f, 0.5f), 128f);
        m_spriteRenderer.sprite = newSprite;
        camera.gameObject.SetActive(false);
    }
}
