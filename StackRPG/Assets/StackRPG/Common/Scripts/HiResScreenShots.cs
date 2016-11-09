using UnityEngine;
using System.Collections;

public class HiResScreenShots : Singleton<HiResScreenShots>
{
    public bool isUse = false;

    public int resWidth = 427;
    public int resHeight = 240;

    private bool takeHiResShot = false;

    private bool isTakeScreenShot = false;

    private bool isTakeScreenShopHP = false;

    public Texture2D m_texture2D;

    static string m_fileName = "";

    private bool isWriteNow = false;

    private Camera m_camera;
    

    void Start()
    {
        m_camera = FindObjectOfType<Camera>();
        m_camera.enabled = false;
    }

    public static string ScreenShotName()
    {
        return string.Format("{0}/{1}",
                             Application.persistentDataPath,
                             m_fileName);
    }

    public static string ScreenShotName(string fileName)
    {
        return string.Format("{0}/{1}",
                             Application.persistentDataPath,
                             fileName);
    }

    public void TakeRandomShot(string fileName)
    {
        if (isUse == false) return;

        StartCoroutine(RandomScreenShot(fileName));
    }

    IEnumerator RandomScreenShot(string fileName)
    {

        yield break;
        //while (GameManager.Instance.m_player.m_playerCharacter.IsDead() == false)
        //{
        //    yield return new WaitForSeconds(Random.Range(3, 10));
        //    
        //    if(Application.loadedLevelName != "GamePlay" || GameManager.Instance.m_player.m_playerCharacter.IsDead() == true)
        //        break;
        //
        //    TakeHiResShot(fileName);
        //}
    }

    public void TakeHiResShotHp(float hpPersent, string fileName)
    {   
        if (hpPersent > 0.5f) return;
        if (isTakeScreenShopHP == true) return;
        if (gameObject.activeInHierarchy == false) return;

        isTakeScreenShopHP = true;

        StartCoroutine(TakeHiResShotTimer(5, fileName));
    }

    IEnumerator TakeHiResShotTimer(float delaytime,string fileName)
    {
        Debug.Log("사진찍엉!!");
        yield return new WaitForSeconds(delaytime);
        TakeHiResShot(fileName);
    }

    public void TakeHiResShot(string fileName)
    {
        takeHiResShot = true;
        m_fileName = fileName;
    }

    public void TakeHiResShotNow(string fileName)
    {
        takeHiResShot = true;
        m_fileName = fileName;
        isWriteNow = true;
    }

    void OnDisable()
    {
        OnWriteFile();
    }

    public void OnWriteFile()
    {
        if (isUse == false) return;

        if (m_texture2D == null)
            return;
        byte[] bytes;
        //!png
        //bytes = screenShot.EncodeToPNG();

        //!JPG
        JPGEncoder mJPGEncoder = new JPGEncoder(m_texture2D, 50);
        while (!mJPGEncoder.isDone) ;
        bytes = mJPGEncoder.GetBytes();

        string filename = ScreenShotName();

        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
        

        m_texture2D = null;
    }

    void LateUpdate()
    {
        if (isUse == false) return;

        if (takeHiResShot )
        {            
            Debug.Log("take shot!!");
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            m_camera.targetTexture = rt;
            m_texture2D = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            m_camera.Render();
            RenderTexture.active = rt;
            m_texture2D.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            m_camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);

            if (isWriteNow == true)
            {
                OnWriteFile();
            }
        }
        takeHiResShot = false;
        isWriteNow = false;
    }
}