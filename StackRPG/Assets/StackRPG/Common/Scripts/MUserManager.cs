using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml.Serialization;
using System.IO;
using UnityEngine.Events;
using stackRPG;

public class MUserManager : Singleton<MUserManager>
{
    public UserData m_userData;
    public User m_user { get { return m_userData != null ? m_userData.m_user : null; } }
    public bool m_isChangeData { get; private set; }

    public UnityAction m_changeCashEvent;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance != this)
        {
            Destroy(transform.gameObject);
        }
    }

    public void SetNickname(string name)
    {
        string newName = name.Trim();
        if (m_userData.m_user.m_nickName != newName)
        {
            m_userData.m_user.m_nickName = newName;
            m_isChangeData = true;
        }
    }

    public string GetNickname()
    {
        return m_userData.m_user.m_nickName;
    }

    public void AddUnit(int id)
    {
        if (m_user.m_haveUnit.Contains(id) == true) return;
        else
        {
            m_user.m_haveUnit.Add(id);
            m_isChangeData = true;
        }
    }

    public void RemoveUnit(int id)
    {
        if (m_user.m_haveUnit.Contains(id) == false) return;
        else
        {   
            m_user.m_haveUnit.Remove(id);
            m_isChangeData = true;
        }
    }

    public void AddCash(int cash)
    {
        m_user.m_cash += cash;
        m_isChangeData = true;
        if (m_changeCashEvent != null) m_changeCashEvent();
    }

    public bool UseCash(int cash)
    {
        if (m_user.m_cash < cash) return false;

        m_user.m_cash -= cash;
        m_isChangeData = true;
        if (m_changeCashEvent != null) m_changeCashEvent();
        return true;
    }


    public void ShowSaveDialog(UnityAction unityAction)
    {
#if UNITY_EDITOR
        string filePath = EditorUtility.SaveFilePanel("Save UserData", "", "user" + ".xml", "xml");
        if (filePath.Length > 0)
        {
            SaveUser();
            m_userData.SaveToFile(filePath);
        }
#else
			SaveUser();
			string xml = m_userData.GetXmlString( );
			PlayerPrefs.SetString("XmlUserData", xml);
#endif

        m_isChangeData = false;
        if (unityAction != null) unityAction();
    }

    private void SaveUser()
    {
        
    }

    public bool ShowLoadDialog()
    {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Load UserData", "", "xml");
        if (filePath.Length > 0)
        {
            UserData userData = LoadFromFile(filePath);
            LoadUser(userData);
            return true;
        }
#else
			string xml = PlayerPrefs.GetString("XmlUserData", "");
			if( !string.IsNullOrEmpty(xml) )
			{
				UserData userData = LoadFromXmlString( xml );
				LoadUser(userData);				
				return true;
			}
#endif
        {
            MakeUserData();
            return false;
        }
    }

    public void MakeUserData()
    {
        UserData userData = new UserData();
        userData.m_user = new User((ResourcesManager.Load("BaseOwner") as UserData).m_user);
        userData.m_user.m_id = SystemInfo.deviceUniqueIdentifier;
        LoadUser(userData);
    }

    public static UserData LoadFromFile(string _filePath)
    {
        var serializer = new XmlSerializer(typeof(UserData));
        var stream = new FileStream(_filePath, FileMode.Open);
        var obj = serializer.Deserialize(stream) as UserData;
        stream.Close();
        return obj;
    }

    public static UserData LoadFromXmlString(string _xml)
    {
        return MUtilsSerialize.Deserialize<UserData>(_xml);
    }

    void LoadUser(UserData userData)
    {   
        m_userData = userData;
    }
}
