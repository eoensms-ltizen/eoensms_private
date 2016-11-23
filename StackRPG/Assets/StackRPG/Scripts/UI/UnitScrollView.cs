using UnityEngine;
using System.Collections.Generic;
using stackRPG;
using UnityEngine.UI;

public class UnitScrollView : MonoBehaviour
{
    public GameObject m_prefab;
    public ScrollRect m_scrollRect;
    private List<MainUnitBar> m_unitBar = new List<MainUnitBar>();
    

    public void InitScrollView(UserData userData)
    {
        ClearUnitBar();

        if (userData == null) return;

        User user = userData.m_user;

        for(int i = 0;i<MUnitManager.Instance.m_unitDatas.Count;++i)
        {
            MainUnitBar makeUnitBar = GetUnitBar(i);

            int unitID = MUnitManager.Instance.m_unitDatas[i].m_unitData.m_id;
            Unit unit = MUnitManager.Instance.GetUnit(unitID);
            bool isHave = userData.m_user.m_haveUnit.Contains(unitID);
           
            makeUnitBar.SetUnit(unitID, isHave);
            makeUnitBar.gameObject.SetActive(true);
        }
    }

    void ClearUnitBar()
    {
        for(int i = 0;i<m_unitBar.Count;++i)
        {   
            m_unitBar[i].gameObject.SetActive(false);
        }
    }

    MainUnitBar GetUnitBar(int index)
    {
        while(index >= m_unitBar.Count)
        {
            RectTransform rectTransform = Instantiate(m_prefab).GetComponent<RectTransform>();
            rectTransform.SetParent(m_scrollRect.content);
            rectTransform.localScale = Vector3.one;
            m_unitBar.Add(rectTransform.GetComponent<MainUnitBar>());
        }
        
        return m_unitBar[index];
    }
}
